using MoonSharp.Interpreter;
using Spectre.Console;

namespace Lunar.Core;

public enum PermissionLevel
{
    User = 1,
    Moderator = 4,
    Developer = 7,
    System = 8,
    Bypass = 9
}

public class LunarObject
{
    public string ClassName { get; set; }
    public string Name { get; set; }
    public LunarObject? Parent { get; set; }
    public List<LunarObject> Children { get; } = new();
    public Dictionary<string, object> Properties { get; } = new();
    public Dictionary<string, object> Attributes { get; } = new();
    public string? Source { get; set; }

    public LunarObject(string className, string name = "")
    {
        ClassName = className;
        Name = string.IsNullOrEmpty(name) ? className : name;
    }

    public void SetParent(LunarObject? newParent)
    {
        Parent?.Children.Remove(this);
        Parent = newParent;
        Parent?.Children.Add(this);
    }
}

public class SecurityManager
{
    public PermissionLevel CurrentLevel { get; set; } = PermissionLevel.User;

    public void Demand(PermissionLevel required)
    {
        if (CurrentLevel < required)
        {
            throw new ScriptRuntimeException($"Security Error: Permission Denied. Required: {required} (Level {(int)required}), Current: {CurrentLevel} (Level {(int)CurrentLevel})");
        }
    }
}
