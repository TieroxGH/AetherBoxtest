using AetherBox.Configurations;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace AetherBox.Helpers.Extensions;

public static class ImguiTooltips
{
    /// <summary>
    /// Creates a tooltip for the last item if it's hovered.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    public static void Tooltip(string text)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(text);
            ImGui.EndTooltip();
        }
    }
}
