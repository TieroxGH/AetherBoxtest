using AetherBox.UI;
using AetherBox.Configurations;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using Dalamud.Interface.Internal;

namespace AetherBox;

/// <summary>
/// The AetherBox class is the main entry point of the plugin, implementing the functionality required for the plugin.
/// </summary>
/// <remarks> This class implements IDalamudPlugin and IDisposable interfaces for integration with the Dalamud plugin architecture and resource management, respectively.</remarks>
public sealed class AetherBox : IDalamudPlugin, IDisposable
{
    /// <summary>
    /// Property: Manages a list of disposable objects for centralized disposal.
    /// <example>
    /// <code>
    /// // Example usage not found in the file.
    /// </code>
    /// </example>
    /// </summary>
    public static readonly List<IDisposable> _dis = [];

    /// <summary>
    /// Property: Gets the name of the plugin.
    /// <example>
    /// <code>
    /// // Usage typically implicit in the plugin framework.
    /// </code>
    /// </example>
    /// </summary>
    public string Name => "AetherBox";

    /// <summary>
    /// Field: Command name for activating the plugin via a slash command.
    /// <example>
    /// <code>
    /// // Activating the plugin via command
    /// if (command == CommandName) { /* Plugin activation logic */ }
    /// </code>
    /// </example>
    /// </summary>
    private const string CommandName = "/atb";

    /// <summary>
    /// Property: Provides an interface to interact with the Dalamud plugin system.
    /// <example>
    /// <code>
    /// // Example usage not directly found, but typically used for plugin interface operations.
    /// </code>
    /// </example>
    /// </summary>
    private DalamudPluginInterface PluginInterface { get; init; }

    /// <summary>
    /// Property: Manages the commands within the Dalamud framework for this plugin.
    /// <example>
    /// <code>
    /// // Registering a command with the CommandManager
    /// CommandManager.RegisterCommand(CommandName, CommandHandler);
    /// </code>
    /// </example>
    /// </summary>
    private ICommandManager CommandManager { get; init; }

    /// <summary>
    /// Property: Manages logging using pluginLog
    /// </summary>
    public static IPluginLog ? PluginLog { get; set; }

    /// <summary>
    /// Property: Holds configuration settings for the plugin.
    /// <example>
    /// <code>
    /// // Accessing a configuration setting
    /// var someSetting = Configuration.SomeSetting;
    /// </code>
    /// </example>
    /// </summary>
    public PluginConfig Configuration { get; init; }

    /// <summary>
    /// Property: Manages the window system for the plugin's user interface.
    /// <example>
    /// <code>
    /// // Adding a window to the WindowSystem
    /// WindowSystem.AddWindow(new CustomWindow());
    /// </code>
    /// </example>
    /// </summary>
    public WindowSystem WindowSystem = new("AetherBox");

    /// <summary>
    /// Property: Represents the configuration window of the plugin.
    /// <example>
    /// <code>
    /// // Opening the configuration window
    /// ConfigWindow.Show();
    /// </code>
    /// </example>
    /// </summary>
    //private ConfigWindow? ConfigWindow { get; init; }

    /// <summary>
    /// Property: Represents the main window of the plugin.
    /// <example>
    /// <code>
    /// // Toggling the main window's visibility
    /// MainWindow.ToggleVisibility();
    /// </code>
    /// </example>
    /// </summary>
    private MainWindow MainWindow { get; init; }


    /// <summary>
    /// Constructor: Initializes the AetherBox plugin with necessary dependencies.
    /// </summary>
    /// <param name="pluginInterface">The Dalamud plugin interface provided by the framework.</param>
    /// <param name="commandManager">The command manager for handling plugin commands.</param>
    /// <example>
    /// <code>
    /// // Example initialization
    /// var aetherBox = new AetherBox(pluginInterface, commandManager);
    /// </code>
    /// </example>
    public AetherBox(DalamudPluginInterface pluginInterface, ICommandManager commandManager, IPluginLog pluginlog)
    {
        PluginLog = pluginlog;
        PluginLog.Debug($"PluginLog = {PluginLog}");

        ECommonsMain.Init(pluginInterface, this, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);

        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        

        this.Configuration = this.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        this.Configuration.Initialize(this.PluginInterface);

        // The images are in an 'Images' subfolder in the same directory as the assembly
        var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Images", "icon.png");
        var iconImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

        MainWindow = new MainWindow(this, iconImage);

        WindowSystem.AddWindow(MainWindow);

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Main Menu"
        });

        this.PluginInterface.UiBuilder.Draw += DrawUI;
        this.PluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;
    }

    /// <summary>
    /// Method: Disposes of the resources used by the AetherBox plugin.
    /// <remarks>Responsible for releasing unmanaged resources and cleanup operations before garbage collection.</remarks>
    /// </summary>
    public void Dispose()
    {
        PluginLog?.Debug($"  {Name} used {CommandName} Mainwindow is now {MainWindow.IsOpen}");
        this.WindowSystem.RemoveAllWindows();
        
        MainWindow.Dispose();

        this.CommandManager.RemoveHandler(CommandName);
    }

    /// <summary>
    /// Method: Handles the command input, typically used to toggle the plugin's main UI.
    /// <remarks>Invoked in response to a slash command to open or interact with the main interface.</remarks>
    /// </summary>
    /// <param name="command">The command that triggered this method.</param>
    /// <param name="args">Additional arguments passed with the command.</param>
    private void OnCommand(string command, string args)
    {
        MainWindow.IsOpen = !MainWindow.IsOpen;
        PluginLog.Debug($"{CommandName} -> Mainwindow: {MainWindow.IsOpen}");
    }

    /// <summary>
    /// Method: Renders the user interface of the plugin.
    /// <remarks>Responsible for drawing the plugin's UI elements on the screen.</remarks>
    /// </summary>
    private void DrawUI()
    {
        this.WindowSystem.Draw();
    }

    /// <summary>
    /// Method: Toggles the visibility of the main window of the plugin.
    /// <remarks>Changes the open state of the MainWindow, showing or hiding it.</remarks>
    /// </summary>
    private void ToggleMainWindow()
    {
        MainWindow.IsOpen = !MainWindow.IsOpen;
    }

    /// <summary>
    /// Method: Loads an image from the 'Images' folder within the assembly directory.
    /// <remarks>Used to load and handle images for the plugin's UI.</remarks>
    /// </summary>
    /// <param name="imageName">Name of the image to load.</param>
    /// <returns>An IDalamudTextureWrap object representing the loaded image, or null if the image cannot be loaded.</returns>
    /// <example>
    /// <code>
    /// // Loading an image for the UI
    /// var myImage = LoadImage("example.png");
    /// </code>
    /// </example>
    public IDalamudTextureWrap? LoadImage(string imageName)
    {
        // Assuming the 'Images' folder is in the same directory as the assembly
        var imagesDirectory = Path.Combine(this.PluginInterface.AssemblyLocation.Directory?.FullName!, "Images");
        var imagePath = Path.Combine(imagesDirectory, imageName);

        // Check if the file exists before trying to load it
        if (File.Exists(imagePath))
        {
            return this.PluginInterface.UiBuilder.LoadImage(imagePath);
        }
        else
        {
            // Handle the case where the image does not exist
            // You could log an error or throw an exception, depending on your error handling strategy
            Svc.Log.Error($"Image not found: {imagePath}");
            return null; // Or however you wish to handle this case
        }
    }
}
