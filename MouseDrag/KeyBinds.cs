using UnityEngine;

namespace MouseDrag
{
    public static class KeyBinds
    {
        public static void Update(RainWorldGame self)
        {
            Duplicate.Update();

            if (Options.tpCreaturesKey?.Value != null && Input.GetKey(Options.tpCreaturesKey.Value))
                Teleport.TeleportObjects(self, Drag.MouseCamera(self)?.room, true, false);

            if (Options.tpItemsKey?.Value != null && Input.GetKey(Options.tpItemsKey.Value))
                Teleport.TeleportObjects(self, Drag.MouseCamera(self)?.room, false, true);
        }


        public static void RawUpdate(RainWorldGame self)
        {
            if (Options.throwWeapon?.Value != null && Input.GetKeyDown(Options.throwWeapon.Value)) {
                Drag.TryThrow(self, Drag.dragChunk?.owner, overrideThreshold: true);
                Drag.tempStopTicks = 20;
            }

            if (Options.pauseOneKey?.Value != null && Input.GetKeyDown(Options.pauseOneKey.Value))
                Pause.TogglePauseObject(Drag.dragChunk?.owner);

            if (Options.pauseRoomCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseRoomCreaturesKey.Value))
                Pause.PauseObjects(Drag.MouseCamera(self)?.room, true, false);

            if (Options.unpauseAllKey?.Value != null && Input.GetKeyDown(Options.unpauseAllKey.Value))
                Pause.UnpauseAll();

            if (Options.pauseAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseAllCreaturesKey.Value)) {
                Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllCreatures: " + Pause.pauseAllCreatures);
            }

            if (Options.pauseAllItemsKey?.Value != null && Input.GetKeyDown(Options.pauseAllItemsKey.Value)) {
                Pause.pauseAllItems = !Pause.pauseAllItems;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllItems: " + Pause.pauseAllItems);
            }

            if (Options.killOneKey?.Value != null && Input.GetKeyDown(Options.killOneKey.Value)) {
                Health.KillCreature(self, Drag.dragChunk?.owner);
                Health.TriggerItem(Drag.dragChunk?.owner);
            }

            if (Options.killRoomKey?.Value != null && Input.GetKeyDown(Options.killRoomKey.Value))
                Health.KillCreatures(self, Drag.MouseCamera(self)?.room);

            if (Options.reviveOneKey?.Value != null && Input.GetKeyDown(Options.reviveOneKey.Value)) {
                Health.ReviveCreature(Drag.dragChunk?.owner);
                Health.ResetItem(Drag.dragChunk?.owner);
            }

            if (Options.reviveRoomKey?.Value != null && Input.GetKeyDown(Options.reviveRoomKey.Value))
                Health.ReviveCreatures(Drag.MouseCamera(self)?.room);

            if (Options.duplicateOneKey?.Value != null && Input.GetKeyDown(Options.duplicateOneKey.Value))
                Duplicate.DuplicateObject(Drag.dragChunk?.owner);

            if (Options.clipboardCtrlXCV?.Value == true) {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                    if (Input.GetKeyDown(KeyCode.X))
                        Clipboard.CutObject(Drag.dragChunk?.owner);
                    if (Input.GetKeyDown(KeyCode.C))
                        Clipboard.CopyObject(Drag.dragChunk?.owner);
                    var rcam = Drag.MouseCamera(self);
                    if (Input.GetKeyDown(KeyCode.V) && rcam?.room != null) {
                        Clipboard.PasteObject(
                            self,
                            rcam.room,
                            rcam.room.ToWorldCoordinate(Drag.MousePos(self))
                        );
                    }
                }
            }

            if (Options.controlKey?.Value != null && Input.GetKeyDown(Options.controlKey.Value)) {
                if (Drag.dragChunk?.owner != null) {
                    Control.ToggleControl(self, Drag.dragChunk?.owner as Creature);
                } else {
                    Control.CycleCamera(self);
                }
            }

            if (Options.forcefieldKey?.Value != null && Input.GetKeyDown(Options.forcefieldKey.Value))
                Forcefield.SetForcefield(Drag.dragChunk, toggle: true, apply: true);

            if (Options.tameOneKey?.Value != null && Input.GetKeyDown(Options.tameOneKey.Value))
                Tame.TameCreature(self, Drag.dragChunk?.owner);

            if (Options.tameRoomKey?.Value != null && Input.GetKeyDown(Options.tameRoomKey.Value))
                Tame.TameCreatures(self, Drag.MouseCamera(self)?.room);

            if (Options.clearRelOneKey?.Value != null && Input.GetKeyDown(Options.clearRelOneKey.Value))
                Tame.ClearRelationships(Drag.dragChunk?.owner);

            if (Options.clearRelRoomKey?.Value != null && Input.GetKeyDown(Options.clearRelRoomKey.Value))
                Tame.ClearRelationships(Drag.MouseCamera(self)?.room);

            if (Options.stunOneKey?.Value != null && Input.GetKeyDown(Options.stunOneKey.Value))
                Stun.StunObject(Drag.dragChunk?.owner?.abstractPhysicalObject, toggle: true, apply: true);

            if (Options.stunRoomKey?.Value != null && Input.GetKeyDown(Options.stunRoomKey.Value))
                Stun.StunObjects(Drag.MouseCamera(self)?.room);

            if (Options.unstunAllKey?.Value != null && Input.GetKeyDown(Options.unstunAllKey.Value))
                Stun.UnstunAll();

            if (Options.stunAllKey?.Value != null && Input.GetKeyDown(Options.stunAllKey.Value)) {
                Stun.stunAll = !Stun.stunAll;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
            }

            if (Options.destroyOneKey?.Value != null && Input.GetKeyDown(Options.destroyOneKey.Value))
                Destroy.DestroyObject(Drag.dragChunk?.owner);

            if (Options.destroyRoomCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRoomCreaturesKey.Value))
                Destroy.DestroyObjects(Drag.MouseCamera(self)?.room, creatures: true, items: false, onlyDead: false);

            if (Options.destroyRoomItemsKey?.Value != null && Input.GetKeyDown(Options.destroyRoomItemsKey.Value))
                Destroy.DestroyObjects(Drag.MouseCamera(self)?.room, creatures: false, items: true, onlyDead: false);

            if (Options.destroyRegionCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRegionCreaturesKey.Value))
                Destroy.DestroyRegionObjects(self, creatures: true, items: false);

            if (Options.destroyRegionItemsKey?.Value != null && Input.GetKeyDown(Options.destroyRegionItemsKey.Value))
                Destroy.DestroyRegionObjects(self, creatures: false, items: true);

            if (Options.destroyRoomDeadCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRoomDeadCreaturesKey.Value))
                Destroy.DestroyObjects(Drag.MouseCamera(self)?.room, creatures: true, items: false, onlyDead: true);

            if (Options.lockKey?.Value != null && Input.GetKeyDown(Options.lockKey.Value))
                Lock.SetLock(Drag.dragChunk, toggle: true, apply: true);

            if (Options.copySelectorKey?.Value != null && Input.GetKeyDown(Options.copySelectorKey.Value) && Integration.devConsoleEnabled) {
                try {
                    Integration.DevConsoleOpen(Integration.DevConsoleGetSelector(Drag.dragChunk?.owner?.abstractPhysicalObject));
                } catch {
                    Plugin.Logger.LogError("RainWorldGameRawUpdateHook exception while writing Dev Console, integration is now disabled");
                    Integration.devConsoleEnabled = false;
                    throw; //throw original exception while preserving stack trace
                }
            }

            if (Options.gravityRoomKey?.Value != null && Input.GetKeyDown(Options.gravityRoomKey.Value))
                Gravity.CycleGravity();

            if (Options.infoKey?.Value != null && Input.GetKeyDown(Options.infoKey.Value)) {
                if (Drag.dragChunk?.owner != null) {
                    Info.DumpInfo(Drag.dragChunk.owner);
                } else {
                    Info.DumpInfo(Drag.MouseCamera(self)?.room);
                }
            }

            if (Options.loadRegionRoomsKey?.Value != null && Input.GetKeyDown(Options.loadRegionRoomsKey.Value))
                Special.ActivateRegionRooms(self);
        }
    }
}
