using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace MountingParty
{
  public sealed class MountingPartyPlugin : IDalamudPlugin
  {
    public string Name => "MountingParty";

    private const string commandName = "/mountingparty";

    public DalamudPluginInterface PluginInterface { get; init; }
    public CommandManager CommandManager { get; init; }
    public ChatGui ChatGui { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public MountingPartyUI Window { get; init; }
    public MountRouletteHandler MountRouletteHandler { get; init; }

    public MountingPartyPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] ChatGui chatGui)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;
      ChatGui = chatGui;
      WindowSystem = new("MountingPartyPlugin");

      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Configuration.Initialize(PluginInterface);

      Window = new MountingPartyUI(this)
      {
        IsOpen = Configuration.IsVisible
      };

      WindowSystem.AddWindow(Window);

      MountRouletteHandler = new MountRouletteHandler(this);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "opens the configuration window"
      });

      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
      
      MountRouletteHandler.Enable();
    }

    public void Dispose()
    {
      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

      WindowSystem.RemoveAllWindows();

      CommandManager.RemoveHandler(commandName);

      MountRouletteHandler.Disable();
    }

    public void PrintDebug(string message)
    {
      if (Configuration.DebugMessages)
      {
        ChatGui.Print(message);
      }
    }

    private void SetVisible(bool isVisible)
    {
      Configuration.IsVisible = isVisible;
      Configuration.Save();

      Window.IsOpen = Configuration.IsVisible;
    }

    private void OnCommand(string command, string args)
    {
      SetVisible(!Configuration.IsVisible);
    }

    private void DrawUI()
    {
      WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
      SetVisible(!Configuration.IsVisible);
    }
  }
}
