using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Lumina.Excel.GeneratedSheets;

namespace MountingParty
{
  public unsafe class MountRouletteHandler
  {
    private readonly MountingPartyPlugin plugin;
    private readonly Hook<UseActionHandler>? UseActionHook;
    private readonly IEnumerable<Mount> PartyMounts;
    private readonly Dalamud.Game.ClientState.Conditions.Condition Condition;
    private readonly Random Rng;
    public unsafe delegate byte UseActionHandler(ActionManager* actionManager, ActionType actionType, uint actionID, long targetID = 3758096384U, uint a4 = 0U, uint a5 = 0U, uint a6 = 0U, void* a7 = default);

    public unsafe MountRouletteHandler(MountingPartyPlugin plugin)
    {
      this.plugin = plugin;

      PartyMounts = Service.DataManager.GetExcelSheet<Mount>()!
        .Where(mount =>
          mount.UIPriority > 0
          && mount.Icon != 0
          && mount.ExtraSeats > 0);

      Rng = new Random();
      Condition = Service.Condition;

      var renderAddress = (nint)ActionManager.Addresses.UseAction.Value;

      if (renderAddress != 0)
      {
        UseActionHook = Hook<UseActionHandler>.FromAddress(renderAddress, OnUseAction);
        plugin.PrintDebug("Mounting Party: Hooked");
      }
      else
      {
        plugin.ChatGui.PrintError("Mounting Party failed to hook");
      }
    }

    private ActionType previousActionType;
    private uint previousActionID;
    private unsafe byte OnUseAction(ActionManager* actionManager, ActionType actionType, uint actionID, long targetID, uint a4, uint a5, uint a6, void* a7)
    {
      if (Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.Mounted2] || !plugin.Configuration.Enabled)
      {
        plugin.PrintDebug("Mounted or Disabled, doing nothing");
        return UseActionHook!.Original(actionManager, actionType, actionID, targetID, a4, a5, a6, a7);
      }

      if (previousActionType == ActionType.General && previousActionID is 9 or 24)
      {
        plugin.PrintDebug($"Previous Action was {previousActionType} {previousActionID}");
        var groupSize = (int)GroupManager.Instance()->MemberCount;

        if (plugin.Configuration.DebugForcePartySize)
        {
          groupSize = plugin.Configuration.DebugPartySize;
          plugin.PrintDebug($"Using forced Party Size {groupSize}");
        }

        if (groupSize > 1)
        {
          plugin.PrintDebug($"Party Size {groupSize} > 1");
          if (actionType is ActionType.Mount)
          {
            plugin.PrintDebug($"Action is {actionType} {actionID}");
            var available = PartyMounts
              .Where(mount => actionManager->GetActionStatus(ActionType.Mount, mount.RowId) == 0)
              .ToList();

            if (available.Count > 0)
            {
              var mount = available.ElementAt(Rng.Next(1, available.Count));
              plugin.PrintDebug($"Override Mount {actionID} -> {mount.RowId}");
              actionID = mount.RowId;
            }
            else
            {
              plugin.PrintDebug($"No Multi-Seater mounts found, doing nothing");
            }
          }
        }
      }

      previousActionID = actionID;
      previousActionType = actionType;

      plugin.PrintDebug($"Action Executed: {actionType} {actionID}");

      var result = UseActionHook!.Original(actionManager, actionType, actionID, targetID, a4, a5, a6, a7);

      return result;
    }

    public void Enable()
    {
      UseActionHook?.Enable();
      plugin.PrintDebug("Hook Enabled");
    }

    public void Disable()
    {
      UseActionHook?.Disable();
      plugin.PrintDebug("Hook Disabled");
    }
  }
}
