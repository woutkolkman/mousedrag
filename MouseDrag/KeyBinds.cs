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
                foreach (var obj in Drag.dragObjects)
                    Drag.TryThrow(self, obj, overrideThreshold: true);
//                Drag.tempStopGrabTicks = 20; //only stops grabbing new objects, keep hold of non-weapons
                Drag.tempStopDragTicks = 20; //stops dragging all objects
            }

            if (Options.pauseOneKey?.Value != null && Input.GetKeyDown(Options.pauseOneKey.Value))
                foreach (var obj in Select.selectedObjects)
                    Pause.TogglePauseObject(obj);

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
                foreach (var obj in Select.selectedObjects) {
                    Health.KillCreature(self, obj);
                    Health.TriggerItem(obj);
                }
            }

            if (Options.killRoomKey?.Value != null && Input.GetKeyDown(Options.killRoomKey.Value))
                Health.KillCreatures(self, Drag.MouseCamera(self)?.room);

            if (Options.reviveOneKey?.Value != null && Input.GetKeyDown(Options.reviveOneKey.Value)) {
                foreach (var obj in Select.selectedObjects) {
                    Health.ReviveCreature(obj);
                    Health.ResetItem(obj);
                }
            }

            if (Options.reviveRoomKey?.Value != null && Input.GetKeyDown(Options.reviveRoomKey.Value))
                Health.ReviveCreatures(Drag.MouseCamera(self)?.room);

            if (Options.duplicateOneKey?.Value != null && Input.GetKeyDown(Options.duplicateOneKey.Value))
                foreach (var obj in Select.selectedObjects)
                    Duplicate.DuplicateObject(obj);

            if (Options.clipboardCtrlXCV?.Value == true) {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                    if (Input.GetKeyDown(KeyCode.X)) {
                        foreach (var obj in Select.selectedObjects)
                            Clipboard.CutObject(obj);
                        Select.selectedChunks.Clear();
                    }
                    if (Input.GetKeyDown(KeyCode.C))
                        foreach (var obj in Select.selectedObjects)
                            Clipboard.CopyObject(obj);
                    if (Input.GetKeyDown(KeyCode.V)) {
                        var rcam = Drag.MouseCamera(self);
                        if (rcam?.room != null) {
                            Clipboard.PasteObject(
                                self,
                                rcam.room,
                                rcam.room.ToWorldCoordinate(Drag.MousePos(self))
                            );
                        }
                    }
                }
            }

            if (Options.controlKey?.Value != null && Input.GetKeyDown(Options.controlKey.Value)) {
                if (Select.selectedChunks.Count > 0) {
                    foreach (var obj in Select.selectedObjects)
                        Control.ToggleControl(self, obj as Creature);
                } else {
                    Control.CycleCamera(self);
                }
            }

            if (Options.forceFieldKey?.Value != null && Input.GetKeyDown(Options.forceFieldKey.Value))
                foreach (var bc in Select.selectedChunks)
                    ForceField.SetForceField(bc, toggle: true, apply: true);

            if (Options.tameOneKey?.Value != null && Input.GetKeyDown(Options.tameOneKey.Value))
                foreach (var obj in Select.selectedObjects)
                    Tame.TameCreature(self, obj);

            if (Options.tameRoomKey?.Value != null && Input.GetKeyDown(Options.tameRoomKey.Value))
                Tame.TameCreatures(self, Drag.MouseCamera(self)?.room);

            if (Options.clearRelOneKey?.Value != null && Input.GetKeyDown(Options.clearRelOneKey.Value))
                foreach (var obj in Select.selectedObjects)
                    Tame.ClearRelationships(obj);

            if (Options.clearRelRoomKey?.Value != null && Input.GetKeyDown(Options.clearRelRoomKey.Value))
                Tame.ClearRelationships(Drag.MouseCamera(self)?.room);

            if (Options.stunOneKey?.Value != null && Input.GetKeyDown(Options.stunOneKey.Value))
                foreach (var obj in Select.selectedObjects)
                    Stun.StunObject(obj?.abstractPhysicalObject, toggle: true, apply: true);

            if (Options.stunRoomKey?.Value != null && Input.GetKeyDown(Options.stunRoomKey.Value))
                Stun.StunObjects(Drag.MouseCamera(self)?.room);

            if (Options.unstunAllKey?.Value != null && Input.GetKeyDown(Options.unstunAllKey.Value))
                Stun.UnstunAll();

            if (Options.stunAllKey?.Value != null && Input.GetKeyDown(Options.stunAllKey.Value)) {
                Stun.stunAll = !Stun.stunAll;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
            }

            if (Options.destroyOneKey?.Value != null && Input.GetKeyDown(Options.destroyOneKey.Value)) {
                foreach (var obj in Select.selectedObjects)
                    Destroy.DestroyObject(obj?.abstractPhysicalObject);
                Select.selectedChunks.Clear();
            }

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
                foreach (var bc in Select.selectedChunks)
                    Lock.SetLock(bc, toggle: true, apply: true);

            if (Options.copySelectorKey?.Value != null && Input.GetKeyDown(Options.copySelectorKey.Value) && Integration.devConsoleEnabled) {
                //NOTE: using multiple selectors in one Dev Console command doesn't make sense, only use first selected object
                PhysicalObject obj = null;
                if (Select.selectedChunks.Count > 0)
                    obj = Select.selectedChunks[0].owner;
                try {
                    Integration.DevConsoleOpen(Integration.DevConsoleGetSelector(obj?.abstractPhysicalObject));
                } catch {
                    Plugin.Logger.LogError("RainWorldGameRawUpdateHook exception while writing Dev Console, integration is now disabled");
                    Integration.devConsoleEnabled = false;
                    throw; //throw original exception while preserving stack trace
                }
            }

            if (Options.gravityRoomKey?.Value != null && Input.GetKeyDown(Options.gravityRoomKey.Value))
                Gravity.CycleGravity();

            if (Options.infoKey?.Value != null && Input.GetKeyDown(Options.infoKey.Value)) {
                if (Select.selectedChunks.Count > 0) {
                    string allInfo = "";
                    foreach (var bc in Select.selectedChunks)
                        allInfo += Info.GetInfo(bc?.owner);
                    Info.CopyToClipboard(allInfo);
                } else {
                    Info.CopyToClipboard(Info.GetInfo(Drag.MouseCamera(self)?.room));
                }
            }

            if (Options.loadRegionRoomsKey?.Value != null && Input.GetKeyDown(Options.loadRegionRoomsKey.Value))
                Special.ActivateRegionRooms(self);
        }
    }
}
