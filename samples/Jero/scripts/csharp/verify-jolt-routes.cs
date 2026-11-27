#!/usr/bin/env dotnet run
#:property PublishAot=false
#:package BouncyCastle.Cryptography@2.7.0
#:include ../../src/Jero.Test/TestWorkspace.cs
using Jero.Test;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

if (args is not [var publishDirectory, var chromiumPath])
    throw new ArgumentException("Usage: verify-jolt-routes.cs <isolated-publish-directory> <chromium-executable>");

var published = Path.GetFullPath(publishDirectory);
var chromium = Path.GetFullPath(chromiumPath);
if (!File.Exists(Path.Combine(published, "Jero.Web.dll")) || !File.Exists(chromium))
    throw new ArgumentException("Provide an isolated Jero.Web publish directory and a Chromium executable.");
var work = TestWorkspace.Create("routes");
Console.WriteLine($"Browser profile and host log: {work}");
var target = Path.Combine(work, "host");
Directory.CreateDirectory(target);
foreach (var file in Directory.EnumerateFiles(published, "*", SearchOption.AllDirectories))
{
    var relative = Path.GetRelativePath(published, file);
    var destination = Path.Combine(target, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    // Share packaged runtime and web assets. Copy mutable state, including configuration and databases.
    var directory = relative.Split(Path.DirectorySeparatorChar)[0];
    var extension = Path.GetExtension(relative).ToLowerInvariant();
    var linkable = directory is "runtimes" or "wwwroot" || extension is ".dll" or ".exe" or ".pdb";
    if (!linkable || !OperatingSystem.IsWindows() || !CreateHardLink(destination, file, IntPtr.Zero))
        File.Copy(file, destination);
}
using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
var stop = timeout.Token;
var hostPort = FreePort();
var chromePort = FreePort();
var hostOrigin = $"http://127.0.0.1:{hostPort}";
var cdpOrigin = $"http://127.0.0.1:{chromePort}";
var browserProfile = Path.Combine(work, "chromium-profile");
var passed = false;
using var host = new Process
{
    StartInfo = new ProcessStartInfo(Path.Combine(target, OperatingSystem.IsWindows() ? "Jero.Web.exe" : "Jero.Web"))
    {
        WorkingDirectory = target,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    }
};
host.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
host.StartInfo.Environment["DOTNET_ENVIRONMENT"] = "Production";
host.StartInfo.Environment["Kestrel__Endpoints__Http__Url"] = hostOrigin;
host.StartInfo.Environment["JWTSettings__IssuerSigningKey"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
var hostCurve = GMNamedCurves.GetByName("sm2p256v1");
var hostKey = new BigInteger(1, RandomNumberGenerator.GetBytes(32)).Mod(hostCurve.N.Subtract(BigInteger.One)).Add(BigInteger.One);
host.StartInfo.Environment["Logging__EventLog__LogLevel__Default"] = "None";
// Furion loads modular Configuration files after the default providers.
var appConfiguration = Path.Combine(target, "Configuration", "App.json");
var appOptions = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(appConfiguration),
    documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!;
appOptions["Cryptogram"]!["PrivateKey"] = Convert.ToHexString(hostKey.ToByteArrayUnsigned());
appOptions["Cryptogram"]!["PublicKey"] = Convert.ToHexString(hostCurve.G.Multiply(hostKey).GetEncoded(false));
File.WriteAllText(appConfiguration, appOptions.ToJsonString());
var databaseConfiguration = Path.Combine(target, "Configuration", "Database.json");
var databaseOptions = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(databaseConfiguration),
    documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!;
databaseOptions["DbConnection"]!["ConnectionConfigs"]![0]!["ConnectionString"] =
    $"DataSource={Path.Combine(work, "route-test.db")}";
File.WriteAllText(databaseConfiguration, databaseOptions.ToJsonString());
var hostLog = new StringBuilder();
void Log(object sender, DataReceivedEventArgs e)
{
    if (e.Data is not null)
        lock (hostLog)
            hostLog.AppendLine(e.Data);
}
host.OutputDataReceived += Log;
host.ErrorDataReceived += Log;
using var chrome = new Process
{
    StartInfo = new ProcessStartInfo(chromium)
    {
        WorkingDirectory = work,
        UseShellExecute = false,
        CreateNoWindow = true,
    }
};
foreach (var argument in new[] { "--headless=new", "--no-first-run", "--no-default-browser-check",
         "--remote-debugging-address=127.0.0.1", "--remote-debugging-port=" + chromePort, "--user-data-dir=" + browserProfile,
         "--disable-background-networking", "about:blank" })
    chrome.StartInfo.ArgumentList.Add(argument);
try
{
    host.Start();
    host.BeginOutputReadLine();
    host.BeginErrorReadLine();
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    var shell = await WaitFor(async () =>
    {
        try { return await http.GetStringAsync(hostOrigin + "/about", stop); }
        catch (HttpRequestException error) when (error.StatusCode is null && !host.HasExited) { return null; }
        catch (HttpRequestException error) when (host.HasExited)
        {
            throw new InvalidOperationException($"The published Jero host exited with code {host.ExitCode}. See {Path.Combine(work, "host.log")}.", error);
        }
    }, stop);
    var entry = System.Text.RegularExpressions.Regex.Match(shell, "<script\\s+type=\"module\"(?:\\s+crossorigin)?\\s+src=\"([^\"]+)\"></script>");
    var entryPath = entry.Groups[1].Value;
    if (!entry.Success || !System.Text.RegularExpressions.Regex.IsMatch(entryPath, @"^\./assets/[^/]+-[\w-]+\.js$"))
        throw new InvalidOperationException("No hashed Vite production entry in Jero shell.");
    var assetsDirectory = Path.Combine(target, "wwwroot", "assets");
    var entryFile = Path.Combine(assetsDirectory, Path.GetFileName(entryPath));
    if (!File.Exists(entryFile))
        throw new InvalidOperationException($"The Jolt entry is missing from the publish directory: {entryFile}");
    // Vite emits both eager imports and lazy route chunks here. Inspect the entire published JS set.
    string[] legacyMarkers = ["ZeroJazorRoot", "CreateNavigationHost", "blazor-routing", "Microsoft.AspNetCore.Components", "razorvue", "/jazor/dist/bundle.js"];
    foreach (var script in Directory.EnumerateFiles(assetsDirectory, "*.js", SearchOption.AllDirectories))
    {
        var source = await File.ReadAllTextAsync(script, stop);
        var legacyMarker = legacyMarkers.FirstOrDefault(marker => source.Contains(marker, StringComparison.OrdinalIgnoreCase));
        if (legacyMarker is not null)
            throw new InvalidOperationException($"Legacy frontend marker {legacyMarker} in {Path.GetFileName(script)}.");
    }
    var assetUrl = new Uri(new Uri(hostOrigin + "/about"), entryPath);
    using (var asset = await http.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead, stop))
    {
        asset.EnsureSuccessStatusCode();
        if (asset.Content.Headers.ContentType?.MediaType?.Contains("javascript", StringComparison.Ordinal) != true)
            throw new InvalidOperationException($"Unexpected Jolt entry content type: {asset.Content.Headers.ContentType}");
    }
    Console.WriteLine($"Jero production HTTP shell {hostOrigin}/about serves {assetUrl.AbsolutePath}.");
    // The published logo is 128x128. Read the PNG header to distinguish a resized response from a static file.
    var thumbnail = await http.GetByteArrayAsync(hostOrigin + "/images/logo.png?width=32&height=32", stop);
    if (thumbnail.Length < 24 || BinaryPrimitives.ReadInt32BigEndian(thumbnail.AsSpan(16)) != 32 ||
        BinaryPrimitives.ReadInt32BigEndian(thumbnail.AsSpan(20)) != 32)
        throw new InvalidOperationException("The image URL did not return a 32x32 thumbnail.");
    using (var jpeg = await http.GetAsync(hostOrigin + "/images/logo.png?width=32&format=jpg&quality=80", stop))
    {
        jpeg.EnsureSuccessStatusCode();
        var bytes = await jpeg.Content.ReadAsByteArrayAsync(stop);
        if (jpeg.Content.Headers.ContentType?.MediaType != "image/jpeg" || bytes is not [0xff, 0xd8, ..])
            throw new InvalidOperationException("The image URL did not convert the thumbnail to JPEG.");
    }
    Console.WriteLine("Image URLs served a 32x32 thumbnail and converted PNG to JPEG.");
    chrome.Start();
    var targetJson = await WaitFor(async () =>
    {
        if (chrome.HasExited)
            throw new InvalidOperationException($"Chrome exited with {chrome.ExitCode}.");
        try { return await http.GetStringAsync(cdpOrigin + "/json", stop); }
        catch (HttpRequestException) { return null; }
    }, stop);
    using var targets = JsonDocument.Parse(targetJson);
    var websocket = targets.RootElement.EnumerateArray()
        .Single(item => item.GetProperty("type").GetString() == "page")
        .GetProperty("webSocketDebuggerUrl").GetString()!;
    using var browser = new ClientWebSocket();
    await browser.ConnectAsync(new Uri(websocket), stop);
    var id = 0;
    var exceptions = new List<string>();
    async Task<JsonElement> Send(string method, object? arguments = null)
    {
        var sequence = ++id;
        await browser.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { id = sequence, method, @params = arguments ?? new { } }),
            WebSocketMessageType.Text, true, stop);
        while (true)
        {
            using var message = new MemoryStream();
            var buffer = new byte[16384];
            ValueWebSocketReceiveResult part;
            do
            {
                part = await browser.ReceiveAsync(buffer.AsMemory(), stop);
                message.Write(buffer, 0, part.Count);
            } while (!part.EndOfMessage);
            using var response = JsonDocument.Parse(message.ToArray());
            var root = response.RootElement;
            if (root.TryGetProperty("method", out var name) && name.GetString() == "Runtime.exceptionThrown")
                exceptions.Add(root.GetProperty("params").GetProperty("exceptionDetails").GetRawText());
            if (root.TryGetProperty("method", out var consoleMethod) && consoleMethod.GetString() == "Runtime.consoleAPICalled"
                && root.GetProperty("params").GetProperty("type").GetString() == "error")
                exceptions.Add(root.GetProperty("params").GetRawText());
            if (!root.TryGetProperty("id", out var responseId) || responseId.GetInt32() != sequence)
                continue;
            if (root.TryGetProperty("error", out var error))
                throw new InvalidOperationException($"{method} failed: {error}");
            return root.GetProperty("result").Clone();
        }
    }

    await Send("Page.enable");
    await Send("Runtime.enable");
    // A test-only SM2 key. Verify mode 1 with the same ciphertext ordering as the API.
    var sm2 = GMNamedCurves.GetByName("sm2p256v1");
    var domain = new ECDomainParameters(sm2.Curve, sm2.G, sm2.N, sm2.H);
    var scalar = BigInteger.ValueOf(42);
    var privateKey = new ECPrivateKeyParameters(scalar, domain);
    var publicKey = Convert.ToHexString(sm2.G.Multiply(scalar).GetEncoded(false));
    await Send("Page.addScriptToEvaluateOnNewDocument", new { source = $$"""
        if (/\/(?:login|code-login|qrcode-login|forget-password|register)$/.test(location.pathname))
            localStorage.removeItem('jero.jazor.access-token');
        else
            localStorage.setItem('jero.jazor.access-token', 'mock-shell-test');
        window.__jeroRequests = [];
        window.open = (url, target) => { window.__jeroOpened = [url, target]; return null; };
        const originalFetch = globalThis.fetch;
        globalThis.fetch = (input, init) => {
            const url = new URL(input instanceof Request ? input.url : String(input), location.href);
            if (!url.pathname.includes('/api/') && !url.pathname.includes('/hubs/'))
                return originalFetch(input, init);
            const path = url.pathname.toLowerCase();
            window.__jeroRequests.push([path, init?.body, url.search]);
            if (path.includes('/negotiate'))
                return Promise.resolve(new Response('{"negotiateVersion":1,"connectionId":"fixture","connectionToken":"fixture","availableTransports":[{"transport":"WebSockets","transferFormats":["Text"]}]}', { headers: { 'Content-Type': 'application/json' } }));
            let result = [];
            if (path.endsWith('/sysauth/userinfo')) result = {
                id: '1', account: 'smoke', realName: 'Shell smoke', accountType: 777,
                tenantId: '1', currentTenantId: '1', homePath: '/about', buttons: ['sysCache:keyList', 'sysDifflog:page', 'sysUpdate:list', 'sysUpdate:logs', 'sysMenu:update', 'sysMenu:setStatus', 'sysFile:page', 'sysConfig:page', 'sysConfig:batchDelete', 'sysTenantConfig:batchDelete', 'sysLangText:batchDelete', 'sysOnlineUser:page', 'sysPlugin:add', 'sysUser:changePwd', 'sysUser:update', 'sysPrint:update', 'sysWechatPay:listRefund'], roleIds: []
            };
            else if (path.endsWith('/sysmenu/loginmenutree')) result = [
                { id: '1', path: '/about', type: 2, meta: {title:'关于项目'}, children: [] },
                { id: '2', path: '/received-notice', type: 2, meta: {title:'站内信'}, children: [] },
                { id: '3', path: '/cache', type: 2, meta: {title:'缓存管理'}, children: [] },
                { id: '4', path: '/platform/server', type: 2, meta: {title:'系统监控'}, children: [] },
                { id: '5', path: '/log/difflog', type: 2, meta: {title:'差异日志'}, children: [] },
                { id: '6', path: '/system/user', type: 2, meta: {title:'账号管理'}, children: [] },
                { id: '7', path: '/system/pos', type: 2, meta: {title:'职位管理'}, children: [] },
                { id: '8', path: '/platform/update', type: 2, meta: {title:'系统更新'}, children: [] },
                { id: '9', path: '/platform/plugin', type: 2, meta: {title:'插件管理'}, children: [] },
                { id: '10', path: '/system/ldap', type: 2, meta: {title:'LDAP'}, children: [] },
                { id: '11', path: '/user-reg-way', type: 2, meta: {title:'注册方案'}, children: [] },
                { id: '12', path: '/role', type: 2, meta: {title:'角色管理'}, children: [] },
                { id: '13', path: '/system/notice', type: 2, meta: {title:'通知公告'}, children: [] },
                { id: '14', path: '/system/lang', type: 2, meta: {title:'语言管理'}, children: [] },
                { id: '15', path: '/develop/database/visual', type: 2, meta: {title:'库表可视化'}, children: [] },
                { id: '16', path: '/menu', type: 2, meta: {title:'菜单管理'}, children: [] },
                { id: '17', path: '/org', type: 2, meta: {title:'机构管理'}, children: [] },
                { id: '18', path: '/file', type: 2, meta: {title:'文件管理'}, children: [] },
                { id: '19', path: '/tenant', type: 2, meta: {title:'租户管理'}, children: [] },
                { id: '20', path: '/tenant-config', type: 2, meta: {title:'租户配置'}, children: [] },
                { id: '21', path: '/lang-text', type: 2, meta: {title:'翻译管理'}, children: [] },
                { id: '22', path: '/print', type: 2, meta: {title:'打印模板'}, children: [] },
                { id: '101', path: '/analytics', type: 2, meta: {title:'数据分析'}, children: [] },
                { id: '102', path: '/approval-flow', type: 2, meta: {title:'审批流程'}, children: [] },
                { id: '103', path: '/database', type: 2, meta: {title:'数据库管理'}, children: [] },
                { id: '104', path: '/dict-type', type: 2, meta: {title:'字典管理'}, children: [] },
                { id: '105', path: '/ex-log', type: 2, meta: {title:'异常日志'}, children: [] },
                { id: '106', path: '/form-design', type: 2, meta: {title:'表单设计'}, children: [] },
                { id: '107', path: '/info-setting', type: 2, meta: {title:'系统配置'}, children: [] },
                { id: '108', path: '/job', type: 2, meta: {title:'任务调度'}, children: [] },
                { id: '109', path: '/job/dashboard', type: 2, meta: {title:'任务看板'}, children: [] },
                { id: '110', path: '/online-user', type: 2, meta: {title:'在线用户'}, children: [] },
                { id: '111', path: '/open-access', type: 2, meta: {title:'开放接口'}, children: [] },
                { id: '112', path: '/op-log', type: 2, meta: {title:'操作日志'}, children: [] },
                { id: '113', path: '/platform-config', type: 2, meta: {title:'平台参数'}, children: [] },
                { id: '114', path: '/region', type: 2, meta: {title:'行政区域'}, children: [] },
                { id: '115', path: '/stress-test', type: 2, meta: {title:'接口压测'}, children: [] },
                { id: '116', path: '/template', type: 2, meta: {title:'模板管理'}, children: [] },
                { id: '117', path: '/vis-log', type: 2, meta: {title:'访问日志'}, children: [] },
                { id: '118', path: '/wechat-pay', type: 2, meta: {title:'微信支付'}, children: [] },
                { id: '119', path: '/wechat-user', type: 2, meta: {title:'三方账号'}, children: [] },
                { id: '99', path: '/mock-iframe', type: 2, meta: {title:'Shell iframe fixture', isIframe: true, isLink: 'about:blank'}, children: [] }
            ];
            else if (path.endsWith('/sysconfig/sysinfo')) result = { title: 'Shell smoke', viceTitle: 'Fixture subtitle', viceDesc: 'Fixture description', copyright: 'Fixture copyright', icp: 'Fixture ICP', icpUrl: 'https://example.test', enableReg: 2, wayList: [{value:'fixture-way',label:'Fixture registration'}] };
            else if (path.endsWith('/approvalflow/page')) result = { total: 1, items: [{id:'951', name:'Fixture approval flow', code:'fixture-flow'}] };
            else if (path.endsWith('/approvalflow/detail')) result = {id:'951', name:'Fixture approval flow', code:'fixture-flow'};
            else if (path.endsWith('/approvalflow/formroutes')) result = [];
            else if (path.endsWith('/sysconfig/page')) result = { total: 2, items: [
                {id:'901', name:'fixture-platform-custom', code:'fixture-custom', value:'true', sysFlag:2},
                {id:'902', name:'fixture-platform-built-in', code:'fixture-built-in', value:'false', sysFlag:1}] };
            else if (path.endsWith('/sysonlineuser/page')) result = { total: 1, items: [
                {id:'903', userName:'fixture-online-account', realName:'fixture-online-name', ip:'127.0.0.1'}] };
            else if (path.endsWith('/sysnotice/pagereceived')) result = { total: 1, items: [{ id: 1, noticeId: 1,
                readStatus: 0, sysNotice: { title: 'Shell notice fixture', type: 1, content: 'Shell notice body',
                    publicTime: '2026-11-23' } }] };
            else if (path.endsWith('/sysdicttype/datalist')) result = [{ value: 1, label: '通知' }];
            else if (path.endsWith('/sysnotice/setread')) result = true;
            else if (path.endsWith('/syscache/keylist')) result = ['fixture:key'];
            else if (path.includes('/syscache/value/')) result = { marker: 'Shell cache fixture' };
            else if (path.endsWith('/sysauth/loginconfig')) result = { publicKey: {{JsonSerializer.Serialize(publicKey)}}, captcha: 2 };
            else if (path.endsWith('/sysauth/login')) result = { accessToken: 'signed-in-fixture', homepage: '/plugin' };
            else if (path.endsWith('/sysuser/changepwd')) result = true;
            else if (path.endsWith('/sysuser/baseinfo')) result = { id:'1', account:'smoke', realName:'Shell smoke', introduction:'fixture-profile-introduction' };
            else if (path.endsWith('/sysuser/page')) {
                const items = [
                {id: '101', account: 'fixture-user', realName: 'fixture-user-name', phone: '17700000001', roleName: 'fixture-role-cell', orgName: 'fixture-org-cell', posName: 'fixture-pos-cell', status: 1, accountType: 777},
                {id: '102', account: 'fixture-second', realName: 'fixture-second-name', status: 1, accountType: 777}];
                if (window.__jeroRequests.filter(([url]) => url.endsWith('/sysuser/page')).length > 1) items.reverse();
                result = {total: 18, items};
            }
            else if (path.endsWith('/systenant/page')) result = { total: 13, items: [
                {id:'201', name:'fixture-tenant', phone:'12345678', status:1}] };
            else if (path.endsWith('/sysmenu/list')) result = [
                {id:'301', title:'fixture-menu-root', type:1, status:1, children:[
                    {id:'302', pid:'301', title:'fixture-menu-child', type:2, status:1, path:'/fixture', children:[]}]}];
            else if (path.endsWith('/sysmenu/setstatus') || path.endsWith('/sysmenu/update')) result = true;
            else if (path.endsWith('/sysorg/tree')) result = [
                {id:'401', name:'fixture-org-root', children:[{id:'402', pid:'401', name:'fixture-org-child'}]}];
            else if (path.endsWith('/sysorg/list')) result = [{id:'402', name:'fixture-org-result', code:'fixture-org-code'}];
            else if (path.endsWith('/sysfile/folder')) result = [{name:'fixture-folder', children:[{name:'nested'}]}];
            else if (path.endsWith('/sysfile/page')) result = {total:1, items:[
                {id:'501', fileName:'fixture-file.txt', filePath:'fixture-folder/nested', suffix:'.txt'}]};
            else if (path.endsWith('/systenantconfig/page')) result = {total:1, items:[
                {id:'601', name:'fixture-setting', code:'fixture-config-key', value:'true'}]};
            else if (path.endsWith('/sysprint/page')) result = {total:1, items:[
                {id:'801', name:'fixture-print', status:1, printType:1, createUserName:'fixture-print-author'}]};
            else if (path.endsWith('/syswechatpay/page')) result = {total:1, items:[
                {id:'22', outTradeNumber:'fixture-payment', total:12345, tradeState:'REFUND', qrcodeContent:'https://example.test/pay', transactionId:'transaction-fixture'}]};
            else if (path.endsWith('/syswechatpay/listrefund')) result = [{id:'23', outRefundNumber:'fixture-refund', refund:2345}];
            else if (path.endsWith('/syslangtext/page')) result = {total:1, items:[
                {id:'701', entityName:'FixtureEntity', entityId:'702', fieldName:'title', langCode:'en', content:'fixture-translation'}]};
            else if (path.endsWith('/sysjob/pagejobdetail')) result = { total: 7, items: [] };
            else if (path.endsWith('/sysnotice/page')) result = {total: 42, items: [{id:'102', title:'fixture-announcement'}]};
            else if (path.endsWith('/sysrole/page')) result = {total: 1, items: [{id:'103', name:'fixture-role', code:'role-code'}]};
            else if (path.endsWith('/syslang/page')) result = {total: 1, items: [{id:'104', name:'fixture-language', code:'lang-code'}]};
            else if (path.endsWith('/sysdatabase/list')) result = [{configId:'catalog', dbNickName:'fixture-catalog'}];
            else if (path.endsWith('/sysdatabase/entitytables/catalog')) result = [
                {tableName:'order_request', tableComment:'Fixture order requests', entityName:'OrderRequest'}];
            else if (path.endsWith('/sysdatabase/visualdbtable/catalog')) result = {
                visualTableList: [{tableName:'Orders', x:50, y:50}, {tableName:'Items', x:300, y:50}],
                visualColumnList: [{tableName:'Orders', columnName:'id'}, {tableName:'Items', columnName:'orderId'}],
                columnRelationList: [{sourceTableName:'Items', sourceColumnName:'orderId', targetTableName:'Orders', targetColumnName:'id', type:'1'}] };
            else if (path.endsWith('/sysserver/serverbase')) result = { hostName: 'server-fixture' };
            else if (path.endsWith('/sysserver/serverused')) result = { ramRate: '37%', cpuRate: '19%' };
            else if (path.endsWith('/sysserver/serverdisk')) result = [{diskName: 'fixture-disk', availablePercent: '62%'}];
            else if (path.endsWith('/sysserver/assemblylist')) result = [{name: 'Fixture.Assembly', version: '2.0'}];
            else if (path.endsWith('/syslogdiff/page')) result = { total: 2, items: [
                { id: '41', businessData: 'structured-fixture', diffType: 'Update' },
                { id: '42', businessData: 'raw-fixture', diffType: 'Update' }
            ] };
            else if (path.endsWith('/syslogdiff/detail/41')) result = { id: '41', sql: 'fixture SQL',
                diffData: JSON.stringify([{TableName:'FixtureTable', Columns:[{ ColumnName:'fixture-field', IsPrimaryKey:true,
                    BeforeValue:'old-fixture-value', AfterValue:'new-fixture-value' }]}]),
                parameters: JSON.stringify([{ ParameterName:'fixture-param', Value:'fixture-param-value' }]) };
            else if (path.endsWith('/syslogdiff/detail/42')) result = { id: '42',
                diffData: 'raw fixture change', parameters: 'raw fixture parameters' };
            else if (path.endsWith('/syspos/list')) result = [{id: '71', name: 'fixture-position',
                userList: [{id: '72', account: 'fixture-account', realName: 'fixture-colleague'}]}];
            else if (path.endsWith('/sysplugin/page')) result = {total: 1, items: [
                {id: '81', name: 'fixture-plugin', tenantId: '1', createUserName: 'fixture-author'}]};
            else if (path.endsWith('/sysupdate/list')) result = [{fileName:'fixture-backup.zip', createTime:'2026-11-23'}];
            else if (path.endsWith('/sysupdate/logs')) result = ['fixture-update-log'];
            else if (path.endsWith('/sysldap/page')) result = {total: 1, items: [{id:'91', host:'fixture-ldap'}]};
            else if (path.endsWith('/sysuserregway/list')) result = [{id:'92', tenantId:'1', name:'fixture-registration'}];
            return Promise.resolve(new Response(JSON.stringify({ code: 200, result }), { headers: { 'Content-Type': 'application/json' } }));
        };
        class MockSocket extends EventTarget {
            readyState = 0;
            constructor() {
                super();
                queueMicrotask(() => { this.readyState = 1; const event = new Event('open'); this.dispatchEvent(event); this.onopen?.(event); });
            }
            send(message) {
                if (!String(message).includes('"protocol"')) return;
                queueMicrotask(() => { const event = new MessageEvent('message', {data: '{}\u001e'}); this.dispatchEvent(event); this.onmessage?.(event); });
            }
            close() { this.readyState = 3; const event = new Event('close'); this.dispatchEvent(event); this.onclose?.(event); }
        }
        MockSocket.OPEN = 1;
        window.WebSocket = MockSocket;
        """ });
    async Task<JsonElement> Evaluate(string expression)
    {
        var result = await Send("Runtime.evaluate", new { expression, returnByValue = true });
        if (result.TryGetProperty("exceptionDetails", out var details))
            throw new InvalidOperationException($"Browser evaluation failed for {expression}: {details}");
        return result.GetProperty("result").TryGetProperty("value", out var value) ? value : default;
    }
    async Task Expect(string expression)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            await WaitFor(async () => (await Evaluate(expression)).ValueKind == JsonValueKind.True ? "ok" : null, deadline.Token);
        }
        catch (OperationCanceledException) when (!stop.IsCancellationRequested)
        {
            var state = await Evaluate("({ path: location.pathname, text: document.body?.innerText?.slice(-1500), requests: window.__jeroRequests })");
            throw new InvalidOperationException($"Browser check timed out: {expression}. Page: {state}; JS: {string.Join(Environment.NewLine, exceptions)}");
        }
    }
    async Task WaitForPage(string path, string marker)
    {
        var navigation = await Send("Page.navigate", new { url = hostOrigin + path });
        if (navigation.TryGetProperty("errorText", out var error))
            throw new InvalidOperationException($"{path} failed: {error}");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop);
        deadline.CancelAfter(TimeSpan.FromSeconds(18));
        try
        {
            var expectedLocation = JsonSerializer.Serialize(path);
            await WaitFor(async () =>
            {
                var content = await Evaluate($"location.pathname + location.search === {expectedLocation} ? (document.querySelector('.jero-shell__main') ?? document.querySelector('#app'))?.innerText ?? '' : ''");
                return content.ValueKind == JsonValueKind.String && content.GetString()!.Contains(marker, StringComparison.Ordinal) ? marker : null;
            }, deadline.Token);
        }
        catch (OperationCanceledException)
        {
            var debug = await Evaluate("({ path: location.pathname, text: document.querySelector('.jero-shell__main')?.innerText ?? document.body?.innerText?.slice(0,700), req: window.__jeroRequests })");
            throw new InvalidOperationException($"{path} did not render {marker}: {debug}; JS: {string.Join(Environment.NewLine, exceptions)}");
        }
        Console.WriteLine($"{path} mounted {marker}.");
    }
    await WaitForPage("/about", "关于项目");
    await Expect("document.querySelector('.about-card')?.innerText.includes('Element Plus') && document.querySelector('.about-card a')?.href === 'https://github.com/devhxj/Jazor'");
    string[] expectedRoutes =
    [
        "/", "/403", "/404", "/500", "/:pathMatch(.*)*", "/about", "/analytics", "/approval-flow",
        "/auth/code-login", "/auth/forget-password", "/auth/login", "/auth/qrcode-login", "/auth/register",
        "/cache", "/code-login", "/coming-soon", "/database", "/database/visual", "/dict-type", "/diff-log",
        "/ex-log", "/file", "/forget-password", "/form-design", "/home", "/iframe/:MenuId", "/info-setting",
        "/job", "/job/dashboard", "/lang", "/lang-text", "/ldap", "/login", "/menu", "/notice", "/offline",
        "/online-user", "/op-log", "/open-access", "/org", "/platform-config", "/platform/job/dashboard",
        "/plugin", "/pos", "/print", "/profile", "/qrcode-login", "/received-notice", "/region", "/register",
        "/role", "/server", "/stress-test", "/template", "/tenant", "/tenant-config", "/update", "/user",
        "/user-reg-way", "/vis-log", "/wechat-pay", "/wechat-user", "/workspace"
    ];
    var routeInventory = await Evaluate("document.querySelector('#app').__vue_app__.config.globalProperties.$router.getRoutes().map(route => route.path)");
    var actualRoutes = routeInventory.EnumerateArray().Select(route => route.GetString()!).ToHashSet(StringComparer.Ordinal);
    var missingRoutes = expectedRoutes.Except(actualRoutes, StringComparer.Ordinal).ToArray();
    var extraRoutes = actualRoutes.Except(expectedRoutes, StringComparer.Ordinal).ToArray();
    if (missingRoutes.Length > 0 || extraRoutes.Length > 0)
        throw new InvalidOperationException($"Unexpected Vue Router inventory. Missing: {string.Join(", ", missingRoutes)}. Extra: {string.Join(", ", extraRoutes)}.");
    Console.WriteLine($"Vue Router registered {actualRoutes.Count} distinct paths.");
    await WaitForPage("/403", "无权访问");
    await Evaluate("document.querySelector('.jero-fallback button').click()");
    await Expect("location.pathname === '/home'");
    Console.WriteLine("ErrorPage Action navigated to /home.");
    await WaitForPage("/404", "访问地址不存在");
    await WaitForPage("/500", "页面加载过程中发生异常");
    await WaitForPage("/offline", "请检查网络连接");
    await WaitForPage("/coming-soon", "该功能仍在迁移中");
    await WaitForPage("/home", "常用入口");
    await Expect("[...document.querySelectorAll('.home-overview__value')].map(x => x.textContent.trim()).join(',') === '18,13,7,42'");
    await Expect("document.querySelectorAll('.home-quick__item').length === 12 && [...document.querySelectorAll('.home-quick__item:enabled')].some(x => x.textContent.includes('账号管理')) && [...document.querySelectorAll('.home-quick__item:disabled')].some(x => x.textContent.includes('角色管理'))");
    await WaitForPage("/workspace", "待办事项");
    await Expect("document.querySelector('.workspace-todo__check')?.checked === false");
    await Evaluate("document.querySelector('.workspace-todo__check').click()");
    await Expect("document.querySelector('.workspace-todo')?.classList.contains('is-completed') && document.querySelector('.workspace-todo__check').checked");
    await WaitForPage("/server", "server-fixture");
    await Expect("document.querySelector('.server-disks')?.innerText.includes('fixture-disk') && document.querySelector('.server-resource-grid')?.innerText.includes('37%') && document.querySelector('.server-assemblies')?.innerText.includes('Fixture.Assembly')");
    await Evaluate("[...document.querySelectorAll('button')].find(x => x.textContent.trim() === '刷新全部').click()");
    await Expect("['serverbase','serverused','serverdisk','assemblylist'].every(api => window.__jeroRequests.filter(([url]) => url.endsWith('/sysserver/' + api)).length >= 2)");
    foreach (var (row, values) in new[] {
        ("structured-fixture", new[] { "FixtureTable", "fixture-field", "old-fixture-value", "new-fixture-value", "fixture SQL", "fixture-param-value" }),
        ("raw-fixture", new[] { "原始差异数据", "raw fixture change", "原始参数", "raw fixture parameters" }) })
    {
        await WaitForPage("/diff-log", row);
        await Evaluate($"[...document.querySelectorAll('tr')].find(x => x.textContent.includes('{row}')).querySelector('button').click()");
        await Expect($"{JsonSerializer.Serialize(values)}.every(text => document.querySelector('[role=dialog]')?.innerText.includes(text))");
    }
    await WaitForPage("/pos", "fixture-position");
    await Evaluate("[...document.querySelectorAll('tr')].find(x => x.textContent.includes('fixture-position')).querySelector('button').click()");
    await Expect("[...document.querySelectorAll('[role=dialog]')].some(dialog => dialog.innerText.includes('fixture-colleague'))");
    await WaitForPage("/plugin", "fixture-plugin");
    await Evaluate("[...document.querySelectorAll('tr')].find(x => x.textContent.includes('fixture-plugin')).querySelector('button').click()");
    await Expect("[...document.querySelectorAll('[role=dialog]')].some(dialog => dialog.innerText.includes('fixture-author'))");
    await WaitForPage("/update", "fixture-backup.zip");
    await Expect("document.querySelector('.update-terminal')?.textContent.includes('fixture-update-log') && ![...document.querySelectorAll('.update-page button')].some(x => ['更新','还原','清空日志'].includes(x.textContent.trim()))");
    await WaitForPage("/ldap", "fixture-ldap");
    await WaitForPage("/user-reg-way", "fixture-registration");
    await WaitForPage("/role", "fixture-role");
    await WaitForPage("/user", "fixture-user-name");
    await Evaluate("window.__jeroUserRows = [...document.querySelectorAll('.el-table__row')]; [...document.querySelectorAll('button')].find(button => button.textContent.trim() === '刷新').click()");
    await Expect("""
        (() => {
            const rows = [...document.querySelectorAll('.el-table__row')];
            return rows.length === 2 && rows[0].innerText.includes('fixture-second-name') &&
                rows[1].innerText.includes('fixture-user-name') &&
                rows[0] === window.__jeroUserRows[1] && rows[1] === window.__jeroUserRows[0];
        })()
        """);
    foreach (var (account, userId) in new[] { ("fixture-user", "101"), ("fixture-second", "102") })
    {
        var exactAccount = JsonSerializer.Serialize(account);
        await Evaluate($"[...[...document.querySelectorAll('.el-table__row')].find(row => [...row.cells].some(cell => cell.innerText.trim() === {exactAccount})).querySelectorAll('.user-action-buttons button')].find(button => button.textContent.trim() === '编辑').click()");
        await Expect($"window.__jeroRequests.some(([url]) => url.endsWith('/sysuser/ownrolelist/{userId}')) && [...document.querySelectorAll('[role=dialog] input')].some(input => input.value === {exactAccount} && input.getClientRects().length > 0)");
        await Evaluate("[...document.querySelectorAll('[role=dialog] button')].find(button => button.textContent.trim() === '取消' && button.getClientRects().length > 0).click()");
        await Expect("![...document.querySelectorAll('[role=dialog] input')].some(input => input.getClientRects().length > 0)");
    }
    await Expect("['fixture-user','fixture-user-name','17700000001','fixture-role-cell','fixture-org-cell','fixture-pos-cell'].every(value => [...document.querySelectorAll('.el-table__row td')].some(cell => cell.innerText.trim() === value))");
    await Expect("window.__jeroRequests.some(([url, body]) => { if (!url.endsWith('/sysuser/page')) return false; const input = JSON.parse(body); return input.page === 1 && input.pageSize === 20 && ['account','realName','posName','phone','orgId'].every(key => Object.hasOwn(input, key)) && !Object.hasOwn(input, 'PageSize'); })");
    await Expect("document.querySelector('.el-pagination__total')?.textContent.includes('18') && !!document.querySelector('.el-table__row') && !document.querySelector('.el-table__body')?.textContent.includes('else')");
    await WaitForPage("/analytics", "访问来源");
    await Expect("document.querySelector('#jero-analytics-main-chart canvas, #jero-analytics-main-chart svg') !== null");
    Console.WriteLine("Verified user wire keys, Element Plus cells, both row edit IDs, and ECharts rendering.");

    await WaitForPage("/notice", "fixture-announcement");
    await WaitForPage("/lang", "fixture-language");
    await WaitForPage("/database/visual?configId=catalog", "库表可视化");
    await Expect("document.querySelector('.database-visual__counts')?.innerText.includes('1 条关系') && document.querySelectorAll('.database-rg-card').length === 2 && document.getElementById('dbcol-Items-orderId') !== null");
    await Evaluate("[...document.querySelectorAll('.database-graph-tools button')].find(x => x.textContent.trim() === '放大').click()");
    await WaitForPage("/menu", "fixture-menu-child");
    await Evaluate("document.querySelector('.menu-tree-toggle:not(:disabled)').click()");
    await Expect("!document.querySelector('.menu-table').innerText.includes('fixture-menu-child')");
    await Evaluate("document.querySelector('.menu-tree-toggle:not(:disabled)').click()");
    await Expect("document.querySelector('.menu-table').innerText.includes('fixture-menu-child')");
    await Expect("!document.querySelector('.menu-page').innerText.includes('新增菜单') && !document.querySelector('.menu-actions').innerText.includes('删除')");
    await Evaluate("document.querySelector('.menu-status-switch[aria-label=切换fixture-menu-child状态]').click()");
    await Expect("window.__jeroRequests.some(([url, body]) => url.endsWith('/sysmenu/setstatus') && JSON.parse(body).id === '302' && JSON.parse(body).status === 2) && document.querySelector('.menu-status-switch[aria-label=切换fixture-menu-child状态]').checked === false");
    await Evaluate("[...document.querySelectorAll('.menu-table tbody tr')].find(row => row.innerText.includes('fixture-menu-child')).querySelector('.menu-actions button').click()");
    await Expect("document.querySelector('[role=dialog] input[placeholder=\"菜单名称，最多64个字符\"]')?.value === 'fixture-menu-child'");

    await WaitForPage("/org", "fixture-org-result");
    await Evaluate("[...document.querySelectorAll('.org-tree__name')].find(button => button.textContent.trim() === 'fixture-org-root').click()");
    await Expect("window.__jeroRequests.some(([url, , query]) => url.endsWith('/sysorg/list') && new URLSearchParams(query).get('Id') === '401')");

    await WaitForPage("/file", "fixture-file.txt");
    await Expect("document.querySelector('.file-folder-item[title=\"fixture-folder/nested\"]') !== null");
    await Evaluate("document.querySelector('.file-folder-item[title=\"fixture-folder/nested\"]').click()");
    await Expect("window.__jeroRequests.some(([url, body]) => url.endsWith('/sysfile/page') && JSON.parse(body).filePath === 'fixture-folder/nested')");

    foreach (var (route, visible, placeholder, endpoint, key, value) in new[]
    {
        ("/tenant", "fixture-tenant", "租户名称", "/systenant/page", "name", "fixture-tenant"),
        ("/tenant-config", "fixture-setting", "搜索配置编码", "/systenantconfig/page", "code", "fixture-config-key"),
        ("/lang-text", "fixture-translation", "实体名、字段名、语言代码、翻译内容", "/syslangtext/page", "keyword", "FixtureEntity")
    })
    {
        await WaitForPage(route, visible);
        if (route is "/lang-text" or "/tenant-config")
        {
            await Evaluate("document.querySelector('.jero-shell__main tbody input[type=checkbox]').click()");
            await Expect("[...document.querySelectorAll('.jero-shell__main button')].some(button => button.textContent.trim() === '批量删除')");
        }
        await Evaluate("document.querySelector('.jero-query-card__head[aria-expanded=false]')?.click()");
        await Expect("document.querySelector('.jero-query-card__body input') !== null");
        await Evaluate($$"""
            (() => {
                const input = document.querySelector('input[placeholder=' + CSS.escape({{JsonSerializer.Serialize(placeholder)}}) + ']');
                input.value = {{JsonSerializer.Serialize(value)}};
                input.dispatchEvent(new Event('input', { bubbles: true }));
                [...document.querySelectorAll('button')].find(button => button.textContent.trim() === '查询').click();
            })()
            """);
        await Expect($"window.__jeroRequests.some(([url, body]) => url.endsWith('{endpoint}') && JSON.parse(body)['{key}'] === '{value}')");
        if (route is "/lang-text" or "/tenant-config")
            await Expect("document.querySelector('.jero-shell__main tbody input[type=checkbox]')?.checked === false && ![...document.querySelectorAll('.jero-shell__main button')].some(button => button.textContent.trim() === '批量删除')");
    }

    await WaitForPage("/print", "fixture-print");
    await Evaluate("[...[...document.querySelectorAll('.jero-shell__main tbody tr')].find(row => row.innerText.includes('fixture-print')).querySelectorAll('button')].find(button => button.innerText.trim() === '详情').click()");
    await Expect("[...document.querySelectorAll('[role=dialog]')].some(dialog => dialog.getClientRects().length && dialog.innerText.includes('fixture-print-author'))");
    await Evaluate("[...document.querySelectorAll('[role=dialog]')].find(dialog => dialog.getClientRects().length && dialog.innerText.includes('fixture-print-author')).querySelector('.el-dialog__headerbtn').click()");
    await Expect("![...document.querySelectorAll('[role=dialog]')].some(dialog => dialog.getClientRects().length && dialog.innerText.includes('fixture-print-author'))");
    await Evaluate("[...[...document.querySelectorAll('.jero-shell__main tbody tr')].find(row => row.innerText.includes('fixture-print')).querySelectorAll('button')].find(button => button.innerText.trim() === '编辑').click()");
    await Expect("document.querySelector('#jero-print-template .hiprint-printPanel') && document.querySelectorAll('#jero-print-elements [tid]').length >= 5");
    Console.WriteLine("Hiprint rendered the template and its element palette.");

    await WaitForPage("/wechat-pay", "fixture-payment");
    await Expect("[...document.querySelectorAll('.jero-shell__main tbody tr')].some(row => row.innerText.includes('fixture-payment') && row.innerText.includes('123.45') && row.innerText.includes('付款二维码'))");
    await Evaluate("[...[...document.querySelectorAll('.jero-shell__main tbody tr')].find(row => row.innerText.includes('fixture-payment')).querySelectorAll('button')].find(button => button.innerText.trim() === '查看退款').click()");
    await Expect("[...document.querySelectorAll('[role=dialog] tr')].some(row => row.innerText.includes('fixture-refund') && row.innerText.includes('23.45'))");
    Console.WriteLine("WeChat Pay rendered payment and refund amounts and the payment QR action.");

    await WaitForPage("/received-notice", "Shell notice fixture");
    await Expect("document.querySelector('.jero-query-card__head')?.getAttribute('aria-expanded') === 'false'");
    await Evaluate("document.querySelector('.jero-query-card__head').click()");
    await Expect("document.querySelector('.jero-query-card input[placeholder=\"搜索站内信标题\"]') !== null && localStorage.getItem('jero-query-open:received-notice') === '1'");
    await Evaluate("document.querySelector('.jero-query-card__head').click()");
    await Expect("document.querySelector('.jero-query-card__body') === null && localStorage.getItem('jero-query-open:received-notice') === '0'");
    await Evaluate("[...document.querySelectorAll('button')].find(button => button.textContent.trim() === '详情').click()");
    await WaitFor(async () =>
    {
        var state = await Evaluate("({ text: document.body?.innerText, read: window.__jeroRequests.find(([url]) => url.endsWith('/sysnotice/setread'))?.[1] })");
        return state.ValueKind == JsonValueKind.Object && state.TryGetProperty("read", out var request) &&
            request.GetString() == "{\"id\":\"1\"}" && state.GetProperty("text").GetString()!.Contains("Shell notice body", StringComparison.Ordinal)
            ? "read" : null;
    }, stop);
    Console.WriteLine("Vue notice row callback opened details and posted the correct notice id.");
    await WaitForPage("/cache", "缓存列表");
    await WaitFor(async () =>
    {
        var clicked = await Evaluate("[...document.querySelectorAll('button')].find(button => button.textContent.trim() === 'key')?.click() === undefined && [...document.querySelectorAll('button')].some(button => button.textContent.trim() === 'key')");
        return clicked.ValueKind == JsonValueKind.True ? "key" : null;
    }, stop);
    await WaitFor(async () =>
    {
        var value = await Evaluate("document.querySelector('pre')?.textContent");
        return value.ValueKind == JsonValueKind.String && value.GetString()!.Contains("Shell cache fixture", StringComparison.Ordinal) ? "cache value" : null;
    }, stop);
    var destructive = await Evaluate("window.__jeroRequests.some(([url]) => url.includes('/syscache/delete') || url.includes('/syscache/clear'))");
    if (destructive.ValueKind != JsonValueKind.False)
        throw new InvalidOperationException("Cache read unexpectedly sent a destructive request.");
    Console.WriteLine("/cache loaded keys and rendered the mock value without delete or clear requests.");
    await WaitForPage("/iframe/99", "Shell iframe fixture");
    // The menu, breadcrumbs and tab can show the fixture title before the iframe renders.
    await Expect("document.querySelector('iframe.jero-iframe__frame')?.getAttribute('src') === 'about:blank'");
    await Evaluate("[...document.querySelectorAll('button')].find(button => button.textContent.trim() === '新窗口打开').click()");
    var opened = await Evaluate("window.__jeroOpened");
    if (opened.ValueKind != JsonValueKind.Array || opened[0].GetString() != "about:blank" || opened[1].GetString() != "_blank")
        throw new InvalidOperationException($"Iframe action used the wrong target: {opened}");
    Console.WriteLine("/iframe/99 rendered the provided URL and called the existing open-window Action.");

    foreach (var (route, marker) in new[]
    {
        ("/analytics", "访问来源"),
        ("/approval-flow", "业务表单绑定"),
        ("/database", "数据库管理"),
        ("/dict-type", "字典管理"),
        ("/ex-log", "模块、方法、账号"),
        ("/form-design", "VForm 3 可视化表单设计器"),
        ("/info-setting", "品牌配置"),
        ("/job", "任务调度"),
        ("/job/dashboard", "任务看板"),
        ("/online-user", "fixture-online-account"),
        ("/open-access", "服务端密钥生成"),
        ("/op-log", "模块、方法、账号"),
        ("/platform-config", "fixture-platform-custom"),
        ("/profile", "个人简介"),
        ("/region", "行政区域树"),
        ("/stress-test", "该功能仅超级管理员可执行"),
        ("/template", "模板管理"),
        ("/vis-log", "按创建时间倒序分页"),
        ("/wechat-pay", "微信支付"),
        ("/wechat-user", "三方账号"),
    })
    {
        await WaitForPage(route, marker);
        // The shell can contain the same title in the menu or open tabs.
        await Expect($"document.querySelector('.jero-shell__main')?.innerText.includes({JsonSerializer.Serialize(marker)}) === true");
        if (route == "/approval-flow")
        {
            await Expect("[...document.querySelectorAll('.jero-shell__main button')].some(button => button.textContent.trim() === '表单配置')");
            await Evaluate("[...document.querySelectorAll('.jero-shell__main button')].find(button => button.textContent.trim() === '表单配置').click()");
            await Expect("[...document.querySelectorAll('.el-dialog .el-form-item')].find(field => field.querySelector('label')?.textContent.trim() === '库定位器')?.querySelector('.el-select input')?.disabled === false");
            await Evaluate("[...document.querySelectorAll('.el-dialog .el-form-item')].find(field => field.querySelector('label')?.textContent.trim() === '库定位器').querySelector('.el-select').click()");
            await Expect("[...document.querySelectorAll('.el-select-dropdown__item')].some(item => item.textContent.includes('fixture-catalog'))");
            await Evaluate("[...document.querySelectorAll('.el-select-dropdown__item')].find(item => item.textContent.includes('fixture-catalog')).click()");
            await Expect("[...document.querySelectorAll('.el-dialog .el-form-item')].find(field => field.querySelector('label')?.textContent.trim() === '数据库表')?.querySelector('.el-select input')?.disabled === false");
            await Evaluate("[...document.querySelectorAll('.el-dialog .el-form-item')].find(field => field.querySelector('label')?.textContent.trim() === '数据库表').querySelector('.el-select').click()");
            await Expect("[...document.querySelectorAll('.el-select-dropdown__item')].some(item => item.textContent.includes('order_request'))");
            await Evaluate("[...document.querySelectorAll('.el-select-dropdown__item')].find(item => item.textContent.includes('order_request')).click()");
            await Expect("document.querySelector('input[placeholder=\"例如 OrderRequest\"]')?.value === 'OrderRequest'");
        }
        if (route == "/database")
            await Expect("![...document.querySelectorAll('.jero-shell__main button')].some(button => ['生成实体', '生成种子'].includes(button.textContent.trim()))");
        if (route == "/form-design")
            await Expect("document.querySelectorAll('.form-design-host').length === 1");
        if (route == "/online-user")
            await Expect("![...document.querySelectorAll('.jero-shell__main button')].some(button => button.textContent.trim() === '下线')");
        if (route == "/profile")
            await Expect("![...document.querySelectorAll('.jero-shell__main button')].some(button => button.textContent.trim() === '更新基本信息')");
    }

    await WaitForPage("/profile", "个人简介");
    await Evaluate("[...document.querySelectorAll('.profile-tab')].find(button => button.textContent.trim() === '修改密码').click()");
    await Expect("document.querySelector('input[placeholder=请输入旧密码]')?.getClientRects().length > 0");
    const string oldPassword = "Fixture旧Password";
    const string newPassword = "Fixture新Password";
    await Evaluate($$"""
        (() => {
            for (const [placeholder, value] of [
                ['请输入旧密码', {{JsonSerializer.Serialize(oldPassword)}}],
                ['请输入新密码', {{JsonSerializer.Serialize(newPassword)}}],
                ['请再次输入新密码', {{JsonSerializer.Serialize(newPassword)}}]
            ]) {
                const input = document.querySelector(`input[placeholder="${placeholder}"]`);
                input.value = value;
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
        })()
        """);
    await Evaluate("[...document.querySelectorAll('.profile-form button')].find(button => button.textContent.trim() === '更新密码').click()");
    await Expect("[...document.querySelectorAll('.el-message.el-message--success')].some(message => message.textContent.trim() === '密码修改成功')");
    var passwordRequests = await Evaluate("window.__jeroRequests.filter(([url]) => url.endsWith('/sysuser/changepwd')).map(([, body]) => JSON.parse(body))");
    if (passwordRequests.GetArrayLength() != 1)
        throw new InvalidOperationException("Expected a single profile password-change request.");
    var passwordPayload = passwordRequests[0];
    void CheckPassword(JsonElement payload, string name, string expected)
    {
        var hex = payload.GetProperty(name).GetString()!;
        var ciphertext = Convert.FromHexString(hex);
        // sm-crypto-v2 omits the uncompressed EC point prefix on the wire.
        var encoded = new byte[ciphertext.Length + 1];
        encoded[0] = 4;
        ciphertext.CopyTo(encoded, 1);
        var decrypt = new SM2Engine(SM2Engine.Mode.C1C3C2);
        decrypt.Init(false, privateKey);
        if (Encoding.UTF8.GetString(decrypt.ProcessBlock(encoded, 0, encoded.Length)) != expected)
            throw new InvalidOperationException($"Password field {name} did not decrypt to the fixture input in mode 1.");
    }
    CheckPassword(passwordPayload, "passwordOld", oldPassword);
    CheckPassword(passwordPayload, "passwordNew", newPassword);
    Console.WriteLine("Verified mode-1 encryption of both profile password fields and the Element Plus success message.");

    await WaitForPage("/info-setting", "品牌配置");
    await Evaluate("[...document.querySelectorAll('.info-setting__section')].find(section => section.innerText.includes('注册配置')).querySelectorAll('.el-radio')[0].click()");
    await Expect("[...document.querySelectorAll('.info-setting__section')].find(section => section.innerText.includes('注册配置')).querySelector('.el-select')?.classList.contains('is-disabled') === false");
    await Evaluate("[...document.querySelectorAll('.info-setting__section')].find(section => section.innerText.includes('注册配置')).querySelector('.el-select').click()");
    await Expect("[...document.querySelectorAll('.el-select-dropdown__item')].some(item => item.textContent.includes('Fixture registration'))");
    await Evaluate("[...document.querySelectorAll('.el-select-dropdown__item')].find(item => item.textContent.includes('Fixture registration')).click()");
    await Evaluate("[...document.querySelectorAll('button')].find(button => button.textContent.trim() === '保存配置').click()");
    await Expect("window.__jeroRequests.some(([url,body]) => url.endsWith('/sysconfig/savesysinfo') && JSON.parse(body).enableReg === 1 && JSON.parse(body).regWayId === 'fixture-way')");
    Console.WriteLine("Jazor radio enabled registration, option selection updated its model, and save received both values.");
    await WaitForPage("/platform-config", "fixture-platform-custom");
    await Expect("document.querySelector('.jero-shell__breadcrumb-current')?.textContent.includes('平台') === true");
    await Expect("document.querySelector('.jero-shell__tab.is-active[data-route=platform-config]') !== null");
    await Evaluate("""
        [...document.querySelectorAll('.jero-shell__main tr')].find(row => row.innerText.includes('fixture-platform-custom'))
            .querySelector('input[type=checkbox]').click()
        """);
    await Expect("[...document.querySelectorAll('.jero-shell__main button')].some(button => button.textContent.trim() === '批量删除')");
    await Expect("[...document.querySelectorAll('.jero-shell__main tr')].find(row => row.innerText.includes('fixture-platform-built-in')).querySelector('input[type=checkbox]').disabled");
    await Expect("!window.__jeroRequests.some(([url]) => url.endsWith('/sysconfig/batchdelete'))");

    await WaitForPage("/plugin", "fixture-plugin");
    const string editorAssets = "performance.getEntriesByType('resource').some(entry => /\\/assets\\/editor\\.api-[^/]+\\.js$/.test(new URL(entry.name).pathname))";
    const string designerAssets = "performance.getEntriesByType('resource').some(entry => /\\/assets\\/designer(?:\\.umd)?-[^/]+\\.(?:js|css)$/.test(new URL(entry.name).pathname))";
    await Expect("!(" + editorAssets + ") && !(" + designerAssets + ")");
    const string openPlugin = "[...document.querySelectorAll('.jero-shell__main button')].find(button => button.textContent.trim() === '新增并安装').click()";
    const string openCode = "[...document.querySelectorAll('[role=tab]')].find(tab => tab.textContent.trim() === 'C# 代码').click()";
    const string closePlugin = "[...document.querySelectorAll('.el-dialog button')].find(button => button.textContent.trim() === '取消').click()";
    await Evaluate(openPlugin);
    await Expect("document.querySelector('.el-dialog') !== null && !(" + editorAssets + ")");
    await Evaluate(openCode);
    await Expect("document.querySelectorAll('#jero-plugin-monaco .monaco-editor').length === 1 && (" + editorAssets + ")");
    await Evaluate(closePlugin);
    await Expect("document.querySelector('.el-dialog') === null");
    await Evaluate(openPlugin);
    await Evaluate(openCode);
    await Expect("document.querySelectorAll('#jero-plugin-monaco .monaco-editor').length === 1");
    await Evaluate(closePlugin);
    await Expect("document.querySelector('.el-dialog') === null");
    Console.WriteLine("Monaco requested its editor chunk on the code tab and remounted once after reopening.");

    await Evaluate("window.__jeroResizeBeforeDesigner = window.onresize; void document.querySelector('#app').__vue_app__.config.globalProperties.$router.push('/form-design')");
    await Expect("document.querySelectorAll('.form-design-host .main-container').length === 1 && (" + designerAssets + ")");
    await Evaluate("void document.querySelector('#app').__vue_app__.config.globalProperties.$router.push('/plugin')");
    await Expect("document.querySelector('.form-design-host') === null && window.onresize === window.__jeroResizeBeforeDesigner");
    await Evaluate("void document.querySelector('#app').__vue_app__.config.globalProperties.$router.push('/form-design')");
    await Expect("document.querySelectorAll('.form-design-host .main-container').length === 1");
    Console.WriteLine("VForm loaded on route entry and remounted once after leaving and returning.");

    await WaitForPage("/login", "登录 Jero");
    await Expect("window.__jeroRequests.some(([path]) => path.endsWith('/sysauth/loginconfig')) && !document.querySelector('input[placeholder=\"输入右侧字符\"]')");
    await Evaluate("""
        (() => {
            const inputs = document.querySelectorAll('.login-page input');
            for (const [index, value] of ['fixture-account', 'FixtureLoginPassword'].entries()) {
                inputs[index].value = value;
                inputs[index].dispatchEvent(new Event('input', { bubbles: true }));
            }
            document.querySelector('.login-page button.el-button--primary').click();
        })()
        """);
    await Expect("location.pathname === '/plugin' && !!document.querySelector('.jero-shell__main') && localStorage.getItem('jero.jazor.access-token') === 'signed-in-fixture'");
    var signIn = await Evaluate("window.__jeroRequests.filter(([url]) => url.endsWith('/sysauth/login')).map(([, body]) => JSON.parse(body))");
    if (signIn.GetArrayLength() != 1 || signIn[0].GetProperty("account").GetString() != "fixture-account")
        throw new InvalidOperationException("Expected one successful transition from a public login to the authenticated shell.");
    CheckPassword(signIn[0], "password", "FixtureLoginPassword");
    await WaitForPage("/code-login", "验证码登录");
    await WaitForPage("/forget-password", "找回密码");
    await WaitForPage("/register", "创建账号");
    var loginSlot = await Evaluate("document.querySelector('input') !== null");
    if (loginSlot.ValueKind != JsonValueKind.True)
        throw new InvalidOperationException("The shared LoginCard's native form slot was not rendered.");
    await Send("Emulation.setDeviceMetricsOverride", new { width = 390, height = 844, deviceScaleFactor = 1, mobile = false });
    await WaitForPage("/qrcode-login", "使用移动设备扫描下方二维码");
    await Expect("document.querySelector('.qr-login-code img')?.naturalWidth > 0 && getComputedStyle(document.querySelector('.qr-login-code')).width === '220px'");
    if (exceptions.Count > 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, exceptions));
    passed = true;
    return 0;
}
finally
{
    foreach (var process in new[] { chrome, host })
        try
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
        }
        catch (InvalidOperationException) { }
    File.WriteAllText(Path.Combine(work, "host.log"), hostLog.ToString());
    if (passed)
    {
        // Keep diagnostics and the test database, and release the large disposable copies.
        foreach (var directory in new[] { target, browserProfile })
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Could not remove test artifacts at {directory}: {error.Message}");
            }
    }
}

[DllImport("kernel32.dll", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode)]
static extern bool CreateHardLink(string destination, string source, IntPtr attributes);

static int FreePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static async Task<string> WaitFor(Func<Task<string?>> action, CancellationToken stop)
{
    while (true)
    {
        stop.ThrowIfCancellationRequested();
        if (await action() is { } value)
            return value;
        await Task.Delay(150, stop);
    }
}
