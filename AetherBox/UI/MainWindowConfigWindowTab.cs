namespace AetherBox.UI;

using System;

[AttributeUsage(AttributeTargets.Field)]
internal class TabSkipAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Field)]
internal class TabIconAttribute : Attribute
{
    public uint Icon { get; set; }
}

internal enum MainWindowConfigWindowTab : byte
{
    [TabSkip] About,
    //[TabSkip] Rotation,

    [TabIcon(Icon = 4)] Info,
    [TabIcon(Icon = 47)] Settings,
}

public struct IncompatiblePlugin
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Url { get; set; }
    public string Features { get; set; }

    public CompatibleType Type { get; set; }
}

[Flags]
public enum CompatibleType : byte
{
    Skill_Usage = 1 << 0,
    Skill_Selection = 1 << 1,
    Crash = 1 << 2,
}
