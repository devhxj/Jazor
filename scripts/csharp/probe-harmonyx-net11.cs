#!/usr/bin/env dotnet run
#:package HarmonyX@2.16.1
#:property MonoMod_CheckTargetRuntime=false

using HarmonyLib;

var harmony = new Harmony("Jazor.Probes.HarmonyX.Net11");
var target = typeof(Target).GetMethod(nameof(Target.Value))
    ?? throw new InvalidOperationException("Target method was not found.");
var postfix = typeof(Patch).GetMethod(nameof(Patch.Postfix))
    ?? throw new InvalidOperationException("Patch method was not found.");

harmony.Patch(target, postfix: new HarmonyMethod(postfix));

Console.WriteLine("Runtime: " + Environment.Version);
Console.WriteLine("Harmony assembly: " + typeof(Harmony).Assembly.FullName);
Console.WriteLine("Patched value: " + Target.Value());

internal static class Target
{
    public static string Value() => "original";
}

internal static class Patch
{
    public static void Postfix(ref string __result)
        => __result = "patched";
}
