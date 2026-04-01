using ECMAScript.WebIDL.Generator;

var exitCode = await WebIdlGeneratorApplication.RunAsync(args, CancellationToken.None);
return exitCode;
