namespace Jolt.Npm;

public sealed record NpmVueModel(string PropName, string EventName, string Type, NpmDefinition? Definition);

public static class NpmVueModels
{
    public static IEnumerable<NpmVueModel> Get(NpmVueComponent component)
    {
        foreach (var (propName, prop) in component.Props)
        {
            var eventName = "update:" + propName;
            if (prop.Type is ("boolean" or "string" or "number") && prop.LiteralValues.Count == 0 &&
                component.Events.TryGetValue(eventName, out var update) &&
                update.Signatures is [[var payload]] && payload.Type == prop.Type)
            {
                yield return new NpmVueModel(propName, eventName, prop.Type, prop.Definition);
            }
        }
    }
}
