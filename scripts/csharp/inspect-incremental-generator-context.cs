#!/usr/bin/env dotnet run
#:package Microsoft.CodeAnalysis.CSharp@5.7.0-1.26207.106

using Microsoft.CodeAnalysis;

var type = typeof(IncrementalGeneratorInitializationContext);
Console.WriteLine(type.Assembly.FullName);

foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
{
    Console.WriteLine(property.Name + " : " + property.PropertyType.FullName);
}

foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
             .Where(static method => method.DeclaringType == typeof(IncrementalGeneratorInitializationContext))
             .OrderBy(static method => method.Name, StringComparer.Ordinal))
{
    Console.WriteLine("method " + method);
}
