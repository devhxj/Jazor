using System.Reflection;
using ECMAScript;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class ECMAScriptHostNamingAuditTest
{
    [TestMethod]
    public void HostMembers_RequiringExplicitRuntimeName_MustDeclareIt()
    {
        var missing = GetHostContractTypes()
            .SelectMany(GetDeclaredHostMembers)
            .Where(static member => !IsCompilerManaged(member))
            .Where(static member => RequiresExplicitRuntimeName(member))
            .Where(static member => !HasExplicitRuntimeName(member))
            .Select(FormatMember)
            .OrderBy(static item => item, StringComparer.Ordinal)
            .ToArray();

        if (missing.Length == 0)
            return;

        Assert.Fail(
            "The following ECMAScript host members use risky C# spellings and must declare explicit runtime names:\n"
            + string.Join("\n", missing));
    }

    private static IEnumerable<Type> GetHostContractTypes()
        => typeof(Global).Assembly
            .GetTypes()
            .Where(static type => type is
            {
                IsVisible: true,
                IsEnum: false,
                IsGenericParameter: false,
            })
            .Where(static type => !typeof(Attribute).IsAssignableFrom(type))
            .Where(static type => !typeof(Delegate).IsAssignableFrom(type))
            .Where(static type => HasAttribute(type, nameof(ECMAScriptAttribute)));

    private static IEnumerable<MemberInfo> GetDeclaredHostMembers(Type type)
    {
        const BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        foreach (var field in type.GetFields(flags))
            yield return field;

        foreach (var property in type.GetProperties(flags))
        {
            if (property.GetIndexParameters().Length == 0)
                yield return property;
        }

        foreach (var method in type.GetMethods(flags))
        {
            if (method.IsSpecialName ||
                method.IsConstructor ||
                method.Name.StartsWith("get_", StringComparison.Ordinal) ||
                method.Name.StartsWith("set_", StringComparison.Ordinal) ||
                method.Name.StartsWith("add_", StringComparison.Ordinal) ||
                method.Name.StartsWith("remove_", StringComparison.Ordinal) ||
                method.Name.StartsWith("op_", StringComparison.Ordinal))
                continue;

            yield return method;
        }

        foreach (var @event in type.GetEvents(flags))
            yield return @event;
    }

    private static bool RequiresExplicitRuntimeName(MemberInfo member)
    {
        var name = member.Name;
        if (string.IsNullOrEmpty(name))
            return false;

        return name.EndsWith("Fn", StringComparison.Ordinal)
            || name.Contains('_', StringComparison.Ordinal)
            || IsAllUppercaseContractName(name)
            || string.Equals(name, "NaN", StringComparison.Ordinal)
            || member.DeclaringType == typeof(Global) && char.IsUpper(name[0]);
    }

    private static bool IsAllUppercaseContractName(string name)
    {
        var hasLetter = false;
        foreach (var ch in name)
        {
            if (char.IsUpper(ch))
            {
                hasLetter = true;
                continue;
            }

            if (char.IsDigit(ch) || ch == '_')
                continue;

            return false;
        }

        return hasLetter;
    }

    private static bool IsCompilerManaged(MemberInfo member)
        // Jazor-mapped members carry their own runtime contract, and inline-template members
        // never emit a runtime member name at all (call sites lower to computed expressions),
        // so the risky-spelling rule has no collision surface for either family.
        => HasAttribute(member, "JazorAttribute") ||
           HasAttribute(member, nameof(ECMAScriptInlineAttribute));

    private static bool HasExplicitRuntimeName(MemberInfo member)
    {
        foreach (var attribute in member.GetCustomAttributesData())
        {
            if (attribute.AttributeType == typeof(System.ComponentModel.DescriptionAttribute))
            {
                var value = attribute.ConstructorArguments.FirstOrDefault().Value as string;
                if (!string.IsNullOrEmpty(value) && value.StartsWith("@#", StringComparison.Ordinal) && value.Length > 2)
                    return true;
            }

            if (attribute.AttributeType.Name == nameof(ECMAScriptNameAttribute))
            {
                var value = attribute.ConstructorArguments.FirstOrDefault().Value as string;
                if (!string.IsNullOrWhiteSpace(value))
                    return true;
            }
        }

        return false;
    }

    private static bool HasAttribute(MemberInfo member, string attributeTypeName)
        => member.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == attributeTypeName);

    private static string FormatMember(MemberInfo member)
    {
        var fallbackName = ToImplicitJsMemberName(member.Name);
        return member switch
        {
            MethodInfo method => $"{method.DeclaringType!.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(static parameter => parameter.ParameterType.Name))}) -> {fallbackName}",
            PropertyInfo property => $"{property.DeclaringType!.FullName}.{property.Name} -> {fallbackName}",
            FieldInfo field => $"{field.DeclaringType!.FullName}.{field.Name} -> {fallbackName}",
            EventInfo @event => $"{@event.DeclaringType!.FullName}.{@event.Name} -> {fallbackName}",
            _ => $"{member.DeclaringType!.FullName}.{member.Name} -> {fallbackName}",
        };
    }

    private static string ToImplicitJsMemberName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            return name;

        if (name.Length == 1)
            return char.ToLowerInvariant(name[0]).ToString();

        var chars = name.ToCharArray();
        chars[0] = char.ToLowerInvariant(chars[0]);

        for (var index = 1; index < chars.Length; index++)
        {
            if (!char.IsUpper(chars[index]))
                break;

            var hasNext = index + 1 < chars.Length;
            if (hasNext && !char.IsUpper(chars[index + 1]))
                break;

            chars[index] = char.ToLowerInvariant(chars[index]);
        }

        return new string(chars);
    }
}
