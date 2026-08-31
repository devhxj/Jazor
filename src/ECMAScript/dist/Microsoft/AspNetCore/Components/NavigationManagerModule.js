import { GetUriWithFragmentCore, GetUriWithQueryParameterCore, GetUriWithQueryParametersObjectCore } from "Microsoft/AspNetCore/Components/NavigationManagerExtensionsModule.js";
import { createLocationChangedEventArgs } from "Microsoft/AspNetCore/Components/Routing/LocationChangedEventArgsModule.js";
import { CreateLocationChangingContext, IsNavigationPrevented } from "Microsoft/AspNetCore/Components/Routing/LocationChangingContextModule.js";
import { createNotFoundEventArgs } from "Microsoft/AspNetCore/Components/Routing/NotFoundEventArgsModule.js";
import { _0333a0fd5f67d8a0, _3ae4900da2b07b27, _946b7129a48c8114 } from "System/StringModule.js";
let LocationHandlers = new WeakMap;
let NotFoundHandlers = new WeakMap;
let LocationChangingHandlers = new WeakMap;
let LocationChangingCancellations = new WeakMap;
let RefreshHandlers = new WeakMap;
export function CreateNavigationManager(refresh) {
  let instance = Object.create(null);
  LocationHandlers.set(instance, []);
  NotFoundHandlers.set(instance, []);
  LocationChangingHandlers.set(instance, []);
  RefreshHandlers.set(instance, refresh);
  Object.defineProperty(instance, "baseUri", { get: () => {
    return GetBaseUri();
  } });
  Object.defineProperty(instance, "uri", { get: GetUri });
  Object.defineProperty(instance, "historyEntryState", { get: GetHistoryEntryState });
  Object.defineProperty(instance, "version", { get: () => {
    return GetVersion(instance);
  } });
  Object.defineProperty(instance, "addLocationChanged", { value: value => {
    AddLocationChanged(instance, value);
    return;
  } });
  Object.defineProperty(instance, "removeLocationChanged", { value: value => {
    RemoveLocationChanged(instance, value);
    return;
  } });
  Object.defineProperty(instance, "addOnNotFound", { value: value => {
    AddNotFound(instance, value);
    return;
  } });
  Object.defineProperty(instance, "removeOnNotFound", { value: value => {
    RemoveNotFound(instance, value);
    return;
  } });
  Object.defineProperty(instance, "notFound", { value: () => {
    NotFound(instance);
    return;
  } });
  Object.defineProperty(instance, "registerLocationChangingHandler", { value: handler => {
    return RegisterLocationChangingHandler(instance, handler);
  } });
  Object.defineProperty(instance, "notifyLocationChanged", { value: intercepted => {
    NotifyLocationChanged(instance, intercepted);
    return;
  } });
  Object.defineProperty(instance, "navigateTo", { value: (uri, optionsOrForceLoad, replace) => {
    NavigateTo(instance, uri, optionsOrForceLoad, replace);
    return;
  } });
  Object.defineProperty(instance, "toAbsoluteUri", { value: uri => {
    return ToAbsoluteUri(instance, uri);
  } });
  Object.defineProperty(instance, "toBaseRelativePath", { value: uri => {
    return ToBaseRelativePath(instance, uri);
  } });
  Object.defineProperty(instance, "getUriWithQueryParameter", { value: (name, value) => {
    return GetUriWithQueryParameterCore(GetUri(), name, value);
  } });
  Object.defineProperty(instance, "getUriWithQueryParameters", { value: parameters => {
    return GetUriWithQueryParametersObjectCore(GetUri(), parameters);
  } });
  Object.defineProperty(instance, "getUriWithQueryParametersFromUri", { value: (uri, parameters) => {
    return GetUriWithQueryParametersObjectCore(uri, parameters);
  } });
  Object.defineProperty(instance, "getUriWithFragment", { value: fragment => {
    return GetUriWithFragmentCore(GetUri(), fragment);
  } });
  Object.defineProperty(instance, "__jazorNavigationVersion", { value: 0, writable: true });
  return instance;
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.LocationChanged.add*/
export function addLocationChanged(instance, value) {
  AddLocationChanged(instance, value);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.LocationChanged.remove*/
export function removeLocationChanged(instance, value) {
  RemoveLocationChanged(instance, value);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.OnNotFound.add*/
export function addOnNotFound(instance, value) {
  AddNotFound(instance, value);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.OnNotFound.remove*/
export function removeOnNotFound(instance, value) {
  RemoveNotFound(instance, value);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.BaseUri.get*/
export function getBaseUri(instance) {
  return GetBaseUri();
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.Uri.get*/
export function getUri(instance) {
  return GetUri();
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.HistoryEntryState.get*/
export function getHistoryEntryState(instance) {
  return GetHistoryEntryState();
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool)*/
export function navigateToForceLoad(instance, uri, forceLoad) {
  NavigateTo(instance, uri, forceLoad, false);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool, bool)*/
export function navigateToForceLoadReplace(instance, uri, forceLoad, replace) {
  NavigateTo(instance, uri, forceLoad, replace);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, Microsoft.AspNetCore.Components.NavigationOptions)*/
export function navigateToOptions(instance, uri, options) {
  NavigateTo(instance, uri, options, null);
}
/*jazor:clr-member virtual Microsoft.AspNetCore.Components.NavigationManager.Refresh(bool)*/
function _f9e277a2ad23f3f2(instance, forceReload) {
  Refresh(instance, forceReload);
}
export { _f9e277a2ad23f3f2 as refresh };
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.NotFound()*/
export function notFound(instance) {
  NotFound(instance);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.ToAbsoluteUri(string)*/
export function toAbsoluteUri(instance, relativeUri) {
  return ToAbsoluteUri(instance, relativeUri);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.ToBaseRelativePath(string)*/
export function toBaseRelativePath(instance, uri) {
  return ToBaseRelativePath(instance, uri);
}
/*jazor:clr-member Microsoft.AspNetCore.Components.NavigationManager.RegisterLocationChangingHandler(System.Func<Microsoft.AspNetCore.Components.Routing.LocationChangingContext, System.Threading.Tasks.ValueTask>)*/
export function registerLocationChangingHandler(instance, locationChangingHandler) {
  return RegisterLocationChangingHandler(instance, locationChangingHandler);
}
function AddLocationChanged(instance, value) {
  AddHandler(LocationHandlers, instance, value);
}
function RemoveLocationChanged(instance, value) {
  RemoveHandler(LocationHandlers, instance, value);
}
function AddNotFound(instance, value) {
  AddHandler(NotFoundHandlers, instance, value);
}
function RegisterLocationChangingHandler(instance, handler) {
  AddHandler(LocationChangingHandlers, instance, handler);
  return { dispose: () => {
    RemoveHandler(LocationChangingHandlers, instance, handler);
    return;
  } };
}
function RemoveNotFound(instance, value) {
  RemoveHandler(NotFoundHandlers, instance, value);
}
function AddHandler(registry, instance, value) {
  if (instance == null || value == null)
    return;
  let handlers = GetHandlers(registry, instance);
  handlers.splice(handlers.length, 0, value);
}
function RemoveHandler(registry, instance, value) {
  if (instance == null || value == null || !registry.has(instance))
    return;
  let handlers = registry.get(instance);
  for (let index = handlers.length - 1; index >= 0; index--) {
    if (Object.is(handlers[index], value)) {
      handlers.splice(index, 1);
      return;
    }
  }
}
function GetHandlers(registry, instance) {
  if (!registry.has(instance))
    registry.set(instance, []);
  return registry.get(instance);
}
function GetVersion(instance) {
  return Reflect.get(instance, "__jazorNavigationVersion") ?? 0;
}
function GetRefreshHandler(instance) {
  return RefreshHandlers.has(instance) ? RefreshHandlers.get(instance) : null;
}
function NotifyLocationChanged(instance, intercepted) {
  let __cacc$c2a5854889411b2a70cc8606;
  let version = GetVersion(instance) + 1;
  Reflect.set(instance, "__jazorNavigationVersion", version);
  __cacc$c2a5854889411b2a70cc8606 = GetRefreshHandler(instance), __cacc$c2a5854889411b2a70cc8606 == null ? undefined : __cacc$c2a5854889411b2a70cc8606();
  let args = createLocationChangedEventArgs(GetUri(), intercepted);
  let handlers = GetHandlers(LocationHandlers, instance).slice();
  for (let handlerIndex = 0; handlerIndex < handlers.length; handlerIndex++) {
    let handler = handlers[handlerIndex];
    if (handler == null)
      continue;
    Reflect.apply(handler, null, [instance, args]);
  }
}
function NotFound(instance) {
  let handlers = GetHandlers(NotFoundHandlers, instance).slice();
  if (handlers.length === 0)
    return;
  let args = createNotFoundEventArgs();
  for (let handlerIndex = 0; handlerIndex < handlers.length; handlerIndex++) {
    let handler = handlers[handlerIndex];
    if (handler == null)
      continue;
    Reflect.apply(handler, null, [instance, args]);
  }
}
function NavigateTo(instance, uri, optionsOrForceLoad, replaceArgument) {
  let options = null;
  if (optionsOrForceLoad !== null && typeof optionsOrForceLoad === "object") {
    options = optionsOrForceLoad;
  }
  let forceLoad = !(options == null) ? GetBool(options, "forceLoad") : IsBooleanTrue(optionsOrForceLoad);
  let replace = !(options == null) ? GetBool(options, "replaceHistoryEntry") : IsBooleanTrue(replaceArgument);
  let historyState = options == null ? null : Reflect.get(options, "historyEntryState");
  if (historyState === undefined)
    historyState = null;
  let target = URL.parse(uri ?? "", !(options == null) && GetBool(options, "relativeToCurrentUri") ? GetUri() : GetBaseUri());
  if (target == null)
    throw new Error(`Navigation URI '${uri ?? ""}' is invalid.`);
  if (forceLoad) {
    AssignWindowLocation(target.href, replace);
    return;
  }
  let baseUri = URL.parse(GetBaseUri(), GetUri());
  if (baseUri == null || !IsWithinBaseUriSpace(target, baseUri)) {
    AssignWindowLocation(uri ?? target.href, replace);
    return;
  }
  let handlers = GetHandlers(LocationChangingHandlers, instance).slice();
  if (handlers.length === 0) {
    CommitInternalNavigation(instance, target, replace, historyState);
    return;
  }
  let cancellation = BeginLocationChangingCancellation(instance);
  let context = CreateLocationChangingContext(target.href, typeof historyState === "string" ? historyState : null, false, cancellation.signal);
  DispatchLocationChanging(handlers, context, () => {
    EndLocationChangingCancellation(instance, cancellation);
    if (cancellation.signal.aborted)
      return;
    if (!IsNavigationPrevented(context))
      CommitInternalNavigation(instance, target, replace, historyState);
    return;
  });
}
function BeginLocationChangingCancellation(instance) {
  if (LocationChangingCancellations.has(instance))
    LocationChangingCancellations.get(instance).abort();
  let cancellation = new AbortController;
  LocationChangingCancellations.set(instance, cancellation);
  return cancellation;
}
function EndLocationChangingCancellation(instance, cancellation) {
  if (LocationChangingCancellations.get(instance) === cancellation)
    LocationChangingCancellations.delete(instance);
}
function CommitInternalNavigation(instance, target, replace, historyState) {
  let route = target.pathname + target.search + target.hash;
  let history = GetWindowHistory();
  if (replace)
    history.replaceState(historyState, "", route);
  else
    history.pushState(historyState, "", route);
  NotifyLocationChanged(instance, true);
}
function DispatchLocationChanging(handlers, context, commit) {
  let settled = Promise.resolve();
  for (let handlerIndex = 0; handlerIndex < handlers.length; handlerIndex++) {
    let handler = handlers[handlerIndex];
    if (handler == null)
      continue;
    let awaited = Promise.resolve(Reflect.apply(handler, null, [context]));
    settled = settled.then(() => {
      return awaited;
    });
  }
  settled.then(commit);
}
function Refresh(instance, forceReload) {
  if (forceReload) {
    let location = GetWindowLocation();
    let reload = Reflect.get(location, "reload") ?? (() => {
      throw new Error("The browser window location cannot reload.");
    })();
    Reflect.apply(reload, location, []);
    return;
  }
  NavigateTo(instance, GetUri(), true, true);
}
function GetBool(instance, name) {
  return IsBooleanTrue(Reflect.get(instance, name));
}
function IsBooleanTrue(value) {
  return typeof value === "boolean" && value;
}
function GetUri() {
  let __trycast$01b37d95ff9a0a4858a90df1;
  return (__trycast$01b37d95ff9a0a4858a90df1 = Reflect.get(GetWindowLocation(), "href"), typeof __trycast$01b37d95ff9a0a4858a90df1 === "string" ? __trycast$01b37d95ff9a0a4858a90df1 : null) ?? "";
}
function GetHistoryEntryState() {
  let state = Reflect.get(GetWindowHistory(), "state");
  return typeof state === "string" ? state : null;
}
function GetBaseUri() {
  let __cacc$c7b5e688875840c032cb9298;
  let current = GetUri();
  let declared = (__cacc$c7b5e688875840c032cb9298 = document.querySelector("base[href]"), __cacc$c7b5e688875840c032cb9298 == null ? undefined : __cacc$c7b5e688875840c032cb9298.getAttribute("href")) ?? "/";
  let baseUri = URL.parse(declared, current);
  if (baseUri == null)
    return "/";
  let pathname = baseUri.pathname;
  if (!_946b7129a48c8114(pathname, "/", 4))
    baseUri.pathname = pathname + "/";
  baseUri.search = "";
  baseUri.hash = "";
  return baseUri.href;
}
function ToAbsoluteUri(instance, relativeUri) {
  let resolved = URL.parse(relativeUri ?? "", GetBaseUri());
  if (resolved == null)
    throw new Error(`Navigation URI '${relativeUri ?? ""}' is invalid.`);
  return resolved;
}
function ToBaseRelativePath(instance, uri) {
  let __pmut$290fd76fa50794049d49fd09;
  let baseUri = GetBaseUri();
  if (_0333a0fd5f67d8a0(uri, baseUri, 4))
    return __pmut$290fd76fa50794049d49fd09 = baseUri.length, uri.substring(__pmut$290fd76fa50794049d49fd09, __pmut$290fd76fa50794049d49fd09 + (uri.length - __pmut$290fd76fa50794049d49fd09));
  let suffixIndex = IndexOfPathSuffix(uri);
  let pathOnly = suffixIndex < 0 ? uri : uri.substring(0, 0 + suffixIndex);
  if (_946b7129a48c8114(baseUri, "/", 4) && pathOnly === baseUri.substring(0, 0 + (baseUri.length - 1))) {
    let __pmut$62f8ae3df77d37c328037c58;
    return __pmut$62f8ae3df77d37c328037c58 = baseUri.length - 1, uri.substring(__pmut$62f8ae3df77d37c328037c58, __pmut$62f8ae3df77d37c328037c58 + (uri.length - __pmut$62f8ae3df77d37c328037c58));
  }
  throw new Error(`ArgumentException: The URI '${uri ?? ""}' is not contained by the base URI '${baseUri ?? ""}'.`);
}
function IsWithinBaseUriSpace(target, baseUri) {
  if (target.origin !== baseUri.origin)
    return false;
  let basePath = _946b7129a48c8114(baseUri.pathname, "/", 4) ? baseUri.pathname : baseUri.pathname + "/";
  return _0333a0fd5f67d8a0(target.pathname, basePath, 4) || target.pathname === basePath.substring(0, 0 + (basePath.length - 1));
}
function IndexOfPathSuffix(uri) {
  let queryIndex = _3ae4900da2b07b27(uri, "?", 4);
  let hashIndex = _3ae4900da2b07b27(uri, "#", 4);
  if (queryIndex < 0)
    return hashIndex;
  if (hashIndex < 0)
    return queryIndex;
  return queryIndex < hashIndex ? queryIndex : hashIndex;
}
function GetWindowLocation() {
  return window.location ?? (() => {
    throw new Error("The browser window does not expose a location object.");
  })();
}
function GetWindowHistory() {
  return window.history ?? (() => {
    throw new Error("The browser window does not expose a history object.");
  })();
}
function AssignWindowLocation(href, replace) {
  let location = GetWindowLocation();
  let method = Reflect.get(location, replace ? "replace" : "assign") ?? (() => {
    throw new Error("The browser window location cannot navigate.");
  })();
  Reflect.apply(method, location, [href]);
}
