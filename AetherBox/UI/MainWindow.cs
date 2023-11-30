using AetherBox.Configurations;
using AetherBox.Helpers.ImGuiExtensions;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;

namespace AetherBox.UI;

public class MainWindow : Window, IDisposable
{
    private readonly IDalamudTextureWrap LogoImage;
    private readonly IDalamudTextureWrap ? CloseButtonTexture;
    private readonly AetherBox Plugin;
    private static float Scale => ImGuiHelpers.GlobalScale;

    // Add flags for each category's open state
    private bool isCategoryInfoOpen = false;
    private bool isCategorySettingsOpen = false;
    private string? selectedCategory;

    public MainWindow(AetherBox plugin, IDalamudTextureWrap logoImage) : base("AetherBox Menu", ImGuiWindowFlags.NoScrollbar, false)
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        Size = new Vector2(300, 500);
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(250, 300),
            MaximumSize = new Vector2(5000, 5000)
        };
        RespectCloseHotkey = true;

        this.LogoImage = logoImage;
        this.Plugin = plugin;

        // Load the close button image using the new method in AetherBoxPlugin
        // Just pass the image name, the method will handle the path
        CloseButtonTexture = plugin.LoadImage("close.png");
    }

    /// <summary>
    /// Code to be executed when the window is closed.
    /// </summary>
    public override void OnClose()
    {
        Plugin.Configuration.Save();
        base.OnClose();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        this.LogoImage.Dispose();
    }

    /// <summary>
    /// Code to be executed every time the window renders.
    /// </summary>
    /// <remarks>In this method, implement your drawing code. You do NOT need to ImGui.Begin your window.</remarks>
    public override void Draw()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.5f, 0.5f));
        try
        {
            var leftTop = ImGui.GetWindowPos() + ImGui.GetCursorPos();
            var rightDown = leftTop + ImGui.GetWindowSize();
            var screenSize = ImGuiHelpers.MainViewport.Size;
            if ((leftTop.X <= 0 || leftTop.Y <= 0 || rightDown.X >= screenSize.X || rightDown.Y >= screenSize.Y)
                && !ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
            {
                var str = string.Empty;
                for (var i = 0; i < 150; i++)
                {
                    str += "Move Screen!";
                }

                using var font = ImRaii.PushFont(ImGuiHelper.GetFont(24));
                using var color = ImRaii.PushColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudYellow));
                ImGui.TextWrapped(str);
            }
            else
            {
                using var table = ImRaii.Table("AetherBox Config Table", 2, ImGuiTableFlags.Resizable);
                if (table)
                {
                    // Set the width of the navigation panel column
                    var navigationPanelWidth = 150 * Scale;
                    ImGui.TableSetupColumn("AetherBox Config Side Bar", ImGuiTableColumnFlags.WidthFixed, navigationPanelWidth);
                    ImGui.TableNextColumn();

                    // Draw the logo at the top
                    DrawHeader();

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    try
                    {
                        DrawNavigationpanel();
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Warning(ex, "Something wrong with navigation panel");
                    }

                    ImGui.TableNextColumn();

                    try
                    {
                        DrawBody();
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Warning(ex, "Something wrong with body");
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Warning(ex, "Something wrong with config window.");
        }
    }

    /// <summary>
    /// Draw the navigation panel
    /// </summary>
    private void DrawNavigationpanel()
    {
        // Info
        if (ImGui.Selectable("Info", isCategoryInfoOpen))
        {
            isCategoryInfoOpen = !isCategoryInfoOpen; // Toggle the state
            if (isCategoryInfoOpen)
            {
                selectedCategory = "Info";
            }
            else if (selectedCategory == "Info")
            {
                selectedCategory = null;
            }
        }

        //Settings
        if (ImGui.Selectable("Settings", isCategorySettingsOpen))
        {
            isCategorySettingsOpen = !isCategorySettingsOpen; // Toggle the state
            if (isCategorySettingsOpen)
            {
                selectedCategory = "Settings";
            }
            else if (selectedCategory == "Settings")
            {
                selectedCategory = null;
            }
        }

        // ... additional categories with the same pattern



        // Calculate the space available for the button, or set a fixed size
        var spaceForButton = 50.0f * Scale; // Example size, adjust as needed

        // Assuming 'CloseButtonTexture' is the variable holding your loaded texture
        if (CloseButtonTexture != null)
        {
            // Calculate the center position for the button
            var windowWidth = ImGui.GetContentRegionAvail().X;
            var windowHeight = ImGui.GetWindowHeight();
            var buttonPosX = (windowWidth - spaceForButton) * 0.5f; // Center the button horizontally
            var offsetX = 9.5f; // Adjust this value as needed to align the button correctly
            buttonPosX += offsetX; // Apply the offset
            var buttonPosY = windowHeight - spaceForButton - ImGui.GetStyle().ItemSpacing.Y; // Position the button at the bottom

            // Set the cursor position to the calculated X and Y positions
            ImGui.SetCursorPosX(buttonPosX);
            ImGui.SetCursorPosY(buttonPosY);

            // Draw the image button without padding and background color
            if (ImGuiHelper.NoPaddingNoColorImageButton(CloseButtonTexture.ImGuiHandle, new Vector2(spaceForButton, spaceForButton)))
            {
                // Ensure 'Plugin.Configuration' is not null before saving
                if (Plugin.Configuration != null)
                {
                    // Save the settings
                    Plugin.Configuration.Save();

                    // Ensure 'Svc.Log' is not null before logging
                    // Log the information
                    Svc.Log?.Information("Settings have been saved.");

                    // Close the window by toggling the visibility off
                    this.IsOpen = false; // Assuming 'IsOpen' is a property that controls the window's visibility
                }

            }
        }

    }

    /// <summary>
    /// Draws the Top left Icon
    /// </summary>
    private void DrawHeader()
    {
        // Calculate the available width for the header and constrain the image to that width while maintaining aspect ratio
        var availableWidth = ImGui.GetContentRegionAvail().X;
        var aspectRatio = (float)this.LogoImage.Width / this.LogoImage.Height;
        var imageWidth = availableWidth;
        var imageHeight = imageWidth / aspectRatio;

        // Ensure the image is not taller than a certain threshold, e.g., 100 pixels
        var maxHeight = 100.0f * Scale;
        if (imageHeight > maxHeight)
        {
            imageHeight = maxHeight;
            imageWidth = imageHeight * aspectRatio;
        }

        // Center the image in the available space
        var spaceBeforeImage = (availableWidth - imageWidth) * 0.5f;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + spaceBeforeImage);

        // Draw the image
        ImGui.Image(this.LogoImage.ImGuiHandle, new Vector2(imageWidth, imageHeight));
    }

    private PluginInfoUI pluginInfoUI = new();
    private PluginSettingsUI pluginSettingsUI = new();

    /// <summary>
    /// Draws thee left collum
    /// </summary>
    private void DrawBody()
    {
        if (selectedCategory != null)
        {
            switch (selectedCategory)
            {
                case "Info":
                    pluginInfoUI.DrawUI();
                    break;
                case "Settings":
                    pluginSettingsUI.DrawUI();
                    break;
                    // Add more cases as needed for additional categories
            }
        }
    }

    private void DrawPluginSettings()
    {
        // Your code to draw the contents of the Settings category
        ImGui.Text("This is the content for the Settings category.");
        // Add more ImGui calls to build out this category's UI
    }



}
