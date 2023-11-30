namespace AetherBox.UI;

internal class PluginInfoUI
{
    public void DrawUI()
    {
        // ImGui code to draw the plugin info UI
        ImGui.Text("Plugin Name: AetherBox");
        // More ImGui elements...
    }
    private void DrawPluginInfo()
    {
        // Your code to draw the contents of the Info category
        ImGui.Text("This is the content for the Info category.");
        // Add more ImGui calls to build out this category's UI
    }
}
