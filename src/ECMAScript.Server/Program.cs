using ECMAScript.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.Enrich
	.FromLogContext()
	.WriteTo
	.Console()
	.CreateLogger();

try
{
	Log.Information("Starting host");

	await Host.CreateDefaultBuilder(args)
		.UseWindowsService()
		.UseSystemd()
		.ConfigureServices(static (hostContext, services) =>
		{
			services.AddSerilog();
			services.AddHostedService<NamedPipeServerBackgroundService>();
		})
		.Build()
		.RunAsync();

	return 0;
}
catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated unexpectedly");
	return 1;
}
finally
{
	await Log.CloseAndFlushAsync();
}
