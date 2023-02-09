using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;

namespace MountingParty
{
  [Serializable]
  public unsafe class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;

    public bool IsVisible { get; set; } = false;

    public bool Enabled { get; set; } = true;

    public bool DebugMessages = false;
    public bool DebugForcePartySize = false;
    public int DebugPartySize = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => this.pluginInterface = pluginInterface;
    public void Save()
    {
      pluginInterface!.SavePluginConfig(this);
    }
  }
}
