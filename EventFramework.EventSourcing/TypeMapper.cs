namespace EventFramework.EventSourcing;

public static class TypeMapper
{
    private static readonly Dictionary<Type, string> EventNames = new();
    private static readonly Dictionary<string, Type> EventTypes = new();

    public static void Map(Type type, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = type.FullName ?? throw new Exception("No type name??");
        if (EventTypes.ContainsKey(name))
            throw new InvalidOperationException($"'{type}' is already mapped to the event name: {EventTypes[name]}");
        // oops
        EventNames[type] = name;
        EventTypes[name] = type;
    }
    
    public static bool TryGetEventType(string name, out Type? type)
    {
        return EventTypes.TryGetValue(name, out type);
    }
    
    public static bool TryGetEventName(Type type, out string? name)
    {
        return EventNames.TryGetValue(type, out name);
    }
    
    public static void Map<T>(string name)
    {
        Map(typeof(T), name);
    }
}