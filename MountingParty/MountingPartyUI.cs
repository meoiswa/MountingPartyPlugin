using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using System;
using System.Numerics;

namespace MountingParty
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  public unsafe class MountingPartyUI : Window, IDisposable
  {
    private readonly MountingPartyPlugin plugin;

    public MountingPartyUI(MountingPartyPlugin plugin)
      : base(
        "MountingParty##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.plugin = plugin;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
      plugin.Configuration.IsVisible = false;
      plugin.Configuration.Save();
    }

    private void DrawSectionMasterEnable()
    {
      // can't ref a property, so use a local copy
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("Debug Options"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Use these to test your settings.");

        var debugMessages = plugin.Configuration.DebugMessages;
        if (ImGui.Checkbox("Debug Messages", ref debugMessages))
        {
          plugin.Configuration.DebugMessages = debugMessages;
          plugin.Configuration.Save();
        }

        var psize = plugin.Configuration.DebugPartySize;
        var forcePSize = plugin.Configuration.DebugForcePartySize;
        if (ImGui.Checkbox("Force Party Size", ref forcePSize))
        {
          plugin.Configuration.DebugForcePartySize = forcePSize;
          plugin.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("##psize", ref psize))
        {
          plugin.Configuration.DebugPartySize = Math.Max(psize, 0);
          plugin.Configuration.Save();
        }
        
        ImGui.Unindent();
      }
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
