using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AetherBox.Configurations;

[Serializable]
public class PluginConfig : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    [NonSerialized]
    private DalamudPluginInterface ? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.PluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.PluginInterface!.SavePluginConfig(this);
    }
}
