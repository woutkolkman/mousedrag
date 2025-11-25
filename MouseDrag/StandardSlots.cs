using UnityEngine;
using System;
using System.Collections.Generic;

namespace MouseDrag
{
    public static class StandardSlots
    {
        //function can be called multiple times without causing issues
        public static void RegisterSlots()
        {
            int idx = 0;
            int validIdx(int origIdx, int? preferredIdx) {
                if (preferredIdx.HasValue && preferredIdx.Value >= 0)
                    return (preferredIdx.Value < MenuManager.registeredSlots.Count) ? preferredIdx.Value : MenuManager.registeredSlots.Count;
                return (idx >= 0 && idx < MenuManager.registeredSlots.Count) ? idx : MenuManager.registeredSlots.Count;
            };

            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.pauseOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.pauseOneMenu));
            if (Options.pauseOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.pauseOneIdx?.Value), new RadialMenu.Slot(nameof(Options.pauseOneMenu)) {
                    requiresBodyChunk = true,
                    reload = (game, slot, chunk) => {
                        bool paused = Pause.IsObjectPaused(chunk?.owner);
                        slot.name = paused ? "mousedragPlay" : "mousedragPause";
                        slot.tooltip = paused ? "Unpause" : "Pause";
                    },
                    actionPO = (game, slot, po) => {
                        Pause.TogglePauseObject(po);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.pauseRoomCreaturesMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.pauseRoomCreaturesMenu));
            if (Options.pauseRoomCreaturesMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.pauseRoomCreaturesIdx?.Value), new RadialMenu.Slot(nameof(Options.pauseRoomCreaturesMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragPauseCreatures",
                    tooltip = "Pause creatures in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Pause.PauseObjects(room, true, false);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.unpauseAllMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.unpauseAllMenu));
            if (Options.unpauseAllMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.unpauseAllIdx?.Value), new RadialMenu.Slot(nameof(Options.unpauseAllMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragPlayAll",
                    tooltip = "Unpause all",
                    reload = (game, slot, chunk) => {
                        slot.curIconColor = Pause.pausedObjects.Count > 0 || Pause.pauseAllCreatures || Pause.pauseAllItems ? Color.white : Color.grey;
                    },
                    actionBC = (game, slot, chunk) => {
                        Pause.UnpauseAll();
                    }
                });
            Action<RainWorldGame, RadialMenu.Slot, BodyChunk> pauseAllCallback = (game, slot, chunk) => {
                if (Options.pauseAllCreaturesMenu?.Value != false && Options.pauseAllItemsMenu?.Value != false) {
                    Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                    Pause.pauseAllItems = Pause.pauseAllCreatures;
                } else if (Options.pauseAllCreaturesMenu?.Value != false) {
                    Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                } else if (Options.pauseAllItemsMenu?.Value != false) {
                    Pause.pauseAllItems = !Pause.pauseAllItems;
                }
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllCreatures: " + Pause.pauseAllCreatures + ", pauseAllItems: " + Pause.pauseAllItems);
            };
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.pauseAllCreaturesMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.pauseAllCreaturesMenu));
            if (Options.pauseAllCreaturesMenu?.Value != false) {
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.pauseAllCreaturesIdx?.Value), new RadialMenu.Slot(nameof(Options.pauseAllCreaturesMenu)) {
                    requiresBodyChunk = false,
                    reload = (game, slot, chunk) => {
                        slot.name = Pause.pauseAllCreatures ? "mousedragPlayGlobal" : "mousedragPauseGlobal";
                        slot.tooltip = Options.pauseAllItemsMenu?.Value != false ?
                        (Pause.pauseAllCreatures ? "Unpause all" : "Pause all") :
                        (Pause.pauseAllCreatures ? "Unpause all creatures" : "Pause all creatures");
                    },
                    actionBC = pauseAllCallback
                });
            } else if (Options.pauseAllItemsMenu?.Value != false) {
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.pauseAllCreaturesIdx?.Value), new RadialMenu.Slot(nameof(Options.pauseAllCreaturesMenu)) {
                    requiresBodyChunk = false,
                    reload = (game, slot, chunk) => {
                        slot.name = Pause.pauseAllItems ? "mousedragPlayGlobal" : "mousedragPauseGlobal";
                        slot.tooltip = Pause.pauseAllItems ? "Unpause all items" : "Pause all items";
                    },
                    actionBC = pauseAllCallback
                });
            }
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.killOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.killOneMenu));
            if (Options.killOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.killOneIdx?.Value), new RadialMenu.Slot(nameof(Options.killOneMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragKill",
                    skipTranslateTooltip = true,
                    update = (game, slot, chunk) => {
                        if (!slot.hover)
                            return;
                        string tooltip = game?.rainWorld?.inGameTranslator?.Translate("Trigger") ?? "Trigger";
                        if (chunk?.owner is Creature) {
                            tooltip = game?.rainWorld?.inGameTranslator?.Translate("Kill") ?? "Kill";
                            if ((chunk.owner as Creature).abstractCreature?.state is HealthState)
                                tooltip += " (" + ((chunk.owner as Creature).abstractCreature.state as HealthState).health.ToString("0.###") + "/1)";
                        }
                        slot.tooltip = tooltip;
                    },
                    actionPO = (game, slot, po) => {
                        Health.KillCreature(game, po);
                        Health.TriggerItem(po);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.killRoomMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.killRoomMenu));
            if (Options.killRoomMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.killRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.killRoomMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragKillCreatures",
                    tooltip = "Kill in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Health.KillCreatures(game, room);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.reviveOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.reviveOneMenu));
            if (Options.reviveOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.reviveOneIdx?.Value), new RadialMenu.Slot(nameof(Options.reviveOneMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragRevive",
                    skipTranslateTooltip = true,
                    update = (game, slot, chunk) => {
                        if (!slot.hover)
                            return;
                        string tooltip = game?.rainWorld?.inGameTranslator?.Translate("Reset") ?? "Reset";
                        if (chunk?.owner is Creature) {
                            tooltip = game?.rainWorld?.inGameTranslator?.Translate("Revive/heal") ?? "Revive/heal";
                            if ((chunk.owner as Creature).abstractCreature?.state is HealthState)
                                tooltip += " (" + ((chunk.owner as Creature).abstractCreature.state as HealthState).health.ToString("0.###") + "/1)";
                        }
                        slot.tooltip = tooltip;
                    },
                    actionPO = (game, slot, po) => {
                        Health.ReviveCreature(po);
                        Health.ResetItem(po);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.reviveRoomMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.reviveRoomMenu));
            if (Options.reviveRoomMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.reviveRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.reviveRoomMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragReviveCreatures",
                    tooltip = "Revive/heal in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Health.ReviveCreatures(room);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.duplicateOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.duplicateOneMenu));
            if (Options.duplicateOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.duplicateOneIdx?.Value), new RadialMenu.Slot(nameof(Options.duplicateOneMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragDuplicate",
                    tooltip = "Duplicate",
                    actionPO = (game, slot, po) => {
                        Duplicate.DuplicateObject(po);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.clipboardMenu) && slot.requiresBodyChunk == true);
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.clipboardMenu) && slot.requiresBodyChunk == true);
            if (Options.clipboardMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.clipboardOneIdx?.Value), new RadialMenu.Slot(nameof(Options.clipboardMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragCut",
                    tooltip = "Cut",
                    actionPO = (game, slot, po) => {
                        Clipboard.CutObject(po);
                    },
                    finalize = (game, slot) => {
                        //prevent ghost selections
                        Select.selectedChunks.RemoveAll(bc => Clipboard.cutObjects.Contains(bc.owner?.abstractPhysicalObject));
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.clipboardMenu) && slot.requiresBodyChunk == false);
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.clipboardMenu) && slot.requiresBodyChunk == false);
            if (Options.clipboardMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.clipboardRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.clipboardMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragPaste",
                    skipTranslateTooltip = true,
                    reload = (game, slot, chunk) => {
                        string tooltip = game?.rainWorld?.inGameTranslator?.Translate("Paste") ?? "Paste";
                        if (Clipboard.cutObjects.Count > 0)
                            tooltip += " " + Special.ConsistentName(Clipboard.cutObjects[Clipboard.cutObjects.Count - 1]);
                        slot.tooltip = tooltip;
                        slot.curIconColor = Clipboard.cutObjects.Count > 0 ? Color.white : Color.grey;
                    },
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        if (room != null)
                            Clipboard.PasteObject(game, room, room.ToWorldCoordinate(slot.menu.menuPos));
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.tpWaypointBgMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.tpWaypointBgMenu));
            if (Options.tpWaypointBgMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.tpWaypointBgIdx?.Value), new RadialMenu.Slot(nameof(Options.tpWaypointBgMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragCrosshair",
                    reload = (game, slot, chunk) => {
                        slot.tooltip = Teleport.crosshair == null ? "Set teleport position" : "Cancel teleportation";
                    },
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Teleport.SetWaypoint(room, slot.menu.menuPos);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.tpWaypointCrMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.tpWaypointCrMenu));
            if (Options.tpWaypointCrMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.tpWaypointCrIdx?.Value), new RadialMenu.Slot(nameof(Options.tpWaypointCrMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragCrosshair",
                    reload = (game, slot, chunk) => {
                        slot.tooltip = Teleport.crosshair == null ? "Set teleport position" : "Cancel teleportation";
                    },
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Teleport.SetWaypoint(room, slot.menu.menuPos, chunk);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.controlMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.controlMenu));
            if (Options.controlMenu?.Value != false) {
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.controlIdx?.Value), new RadialMenu.Slot(nameof(Options.controlMenu)) {
                    requiresBodyChunk = true,
                    reload = (game, slot, chunk) => {
                        bool isControlled = (chunk?.owner?.abstractPhysicalObject as AbstractCreature)?.controlled == true;
                        bool isPlayer = chunk?.owner is Player && !(chunk.owner as Player).isNPC;
                        bool hasControl = false;
                        if (isPlayer)
                            hasControl = Control.PlayerHasControl((chunk.owner as Player).playerState?.playerNumber ?? -1);
                        slot.name = isControlled ? "mousedragUncontrol" : "mousedragControl";
                        slot.tooltip = isPlayer ? "Release all for this player" : (isControlled ? "Release control" : "Safari-control");
                        if (!ModManager.MSC)
                            slot.tooltip = "MSC not enabled";
                        slot.curIconColor = ModManager.MSC && 
                            chunk?.owner?.abstractPhysicalObject is AbstractCreature && 
                            (!isPlayer || hasControl) ? Color.white : Color.grey;
                    },
                    actionPO = (game, slot, po) => {
                        bool isControlled = (po?.abstractPhysicalObject as AbstractCreature)?.controlled == true;
                        if (isControlled) {
                            Control.ToggleControl(game, po as Creature);
                            return;
                        }
                        if (po is Player && !(po as Player).isNPC) {
                            //skip selection and uncontrol all
                            Control.ToggleControl(game, po as Creature);
                        } else if (po is Creature) {
                            //creature can be controlled
                            MenuManager.SetSubMenuID("SafariPlayer");
                        }
                    }
                });
                //submenu for selecting player number which will safari control creature
                for (int i = 0; i < RWCustom.Custom.rainWorld?.options?.controls?.Length; i++) {
                    //add all selectable safari control players to submenu
                    MenuManager.registeredSlots.Add(new RadialMenu.Slot(nameof(Options.controlMenu)) {
                        name = (i + 1).ToString(),
                        isLabel = true,
                        actionOnASingleObject = true,
                        subMenuID = "SafariPlayer",
                        reload = (game, slot, chunk) => {
                            MenuManager.lowPrioText = "Select safari player";
                        },
                        actionPO = (game, slot, po) => {
                            MenuManager.GoToRootMenu();
                            if (!Int32.TryParse(slot.name, out int pI)) {
                                Plugin.Logger.LogWarning("Parsing playerIndex failed, value is \"" + slot.name + "\"");
                                return;
                            }
                            Drag.playerNr = pI - 1;
                            List<PhysicalObject> objects = new List<PhysicalObject>();
                            if (po != null) {
                                if (Select.selectedObjects.Contains(po)) {
                                    objects = Select.selectedObjects;
                                } else {
                                    objects.Add(po);
                                }
                            }
                            foreach (PhysicalObject obj in objects)
                                Control.ToggleControl(game, obj as Creature);
                        }
                    });
                }
            }
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.forceFieldMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.forceFieldMenu));
            if (Options.forceFieldMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.forceFieldIdx?.Value), new RadialMenu.Slot(nameof(Options.forceFieldMenu)) {
                    requiresBodyChunk = true,
                    reload = (game, slot, chunk) => {
                        bool forceField = ForceField.HasForceField(chunk);
                        slot.name = forceField ? "mousedragForceFieldOff" : "mousedragForceFieldOn";
                        slot.tooltip = forceField ? "Disable ForceField" : "Enable ForceField";
                        ForceField.radMSelectsAForceField = forceField;
                    },
                    update = (game, slot, chunk) => {
                        ForceField.hoversOverSlot = slot.hover;
                    },
                    actionBC = (game, slot, chunk) => {
                        ForceField.SetForceField(chunk, toggle: true, apply: true);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.tameOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.tameOneMenu));
            if (Options.tameOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.tameOneIdx?.Value), new RadialMenu.Slot(nameof(Options.tameOneMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragHeart",
                    tooltip = "Tame",
                    reload = (game, slot, chunk) => {
                        slot.curIconColor = Tame.IsTamable(game, chunk?.owner) ? Color.white : Color.grey;
                    },
                    actionPO = (game, slot, po) => {
                        Tame.TameCreature(game, po);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.tameRoomMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.tameRoomMenu));
            if (Options.tameRoomMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.tameRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.tameRoomMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragHeartCreatures",
                    tooltip = "Tame in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Tame.TameCreatures(game, room);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.clearRelOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.clearRelOneMenu));
            if (Options.clearRelOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.clearRelOneIdx?.Value), new RadialMenu.Slot(nameof(Options.clearRelOneMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragUnheart",
                    tooltip = "Clear relationships",
                    reload = (game, slot, chunk) => {
                        slot.curIconColor = Tame.IsTamable(game, chunk?.owner) ? Color.white : Color.grey;
                    },
                    actionPO = (game, slot, po) => {
                        Tame.ClearRelationships(po);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.clearRelRoomMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.clearRelRoomMenu));
            if (Options.clearRelRoomMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.clearRelRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.clearRelRoomMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragUnheartCreatures",
                    tooltip = "Clear relationships in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Tame.ClearRelationships(room);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.stunOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.stunOneMenu));
            if (Options.stunOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.stunOneIdx?.Value), new RadialMenu.Slot(nameof(Options.stunOneMenu)) {
                    requiresBodyChunk = true,
                    reload = (game, slot, chunk) => {
                        bool stunned = Stun.IsObjectStunned(chunk?.owner);
                        slot.name = stunned ? "mousedragUnstun" : "mousedragStun";
                        slot.tooltip = stunned ? "Unstun" : "Stun";
                        slot.curIconColor = chunk?.owner is Oracle || chunk?.owner is Creature ? Color.white : Color.grey;
                    },
                    actionPO = (game, slot, po) => {
                        Stun.StunObject(po?.abstractPhysicalObject, toggle: true, apply: true);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.stunRoomMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.stunRoomMenu));
            if (Options.stunRoomMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.stunRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.stunRoomMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragStunAll",
                    tooltip = "Stun in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Stun.StunObjects(room);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.unstunAllMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.unstunAllMenu));
            if (Options.unstunAllMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.unstunAllIdx?.Value), new RadialMenu.Slot(nameof(Options.unstunAllMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragUnstunAll",
                    tooltip = "Unstun all",
                    reload = (game, slot, chunk) => {
                        slot.curIconColor = Stun.stunnedObjects.Count > 0 || Stun.stunAll ? Color.white : Color.grey;
                    },
                    actionBC = (game, slot, chunk) => {
                        Stun.UnstunAll();
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.stunAllMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.stunAllMenu));
            if (Options.stunAllMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.stunAllIdx?.Value), new RadialMenu.Slot(nameof(Options.stunAllMenu)) {
                    requiresBodyChunk = false,
                    reload = (game, slot, chunk) => {
                        slot.name = Stun.stunAll ? "mousedragUnstunGlobal" : "mousedragStunGlobal";
                        slot.tooltip = Stun.stunAll ? "Unstun all" : "Stun all";
                    },
                    actionBC = (game, slot, chunk) => {
                        Stun.stunAll = !Stun.stunAll;
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.destroyOneMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.destroyOneMenu));
            if (Options.destroyOneMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.destroyOneIdx?.Value), new RadialMenu.Slot(nameof(Options.destroyOneMenu)) {
                    requiresBodyChunk = true,
                    name = "mousedragDestroy",
                    tooltip = "Destroy",
                    actionPO = (game, slot, po) => {
                        Destroy.DestroyObject(po?.abstractPhysicalObject);
                    },
                    finalize = (game, slot) => {
                        //prevent ghost selections
                        Select.selectedChunks.RemoveAll(bc => bc.owner?.slatedForDeletetion != false);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.destroyRoomCreaturesMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.destroyRoomCreaturesMenu));
            if (Options.destroyRoomCreaturesMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.destroyRoomCreaturesIdx?.Value), new RadialMenu.Slot(nameof(Options.destroyRoomCreaturesMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragDestroyCreatures",
                    tooltip = "Destroy creatures in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Destroy.DestroyObjects(room, creatures: true, items: false, onlyDead: false);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.destroyRoomItemsMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.destroyRoomItemsMenu));
            if (Options.destroyRoomItemsMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.destroyRoomItemsIdx?.Value), new RadialMenu.Slot(nameof(Options.destroyRoomItemsMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragDestroyItems",
                    tooltip = "Destroy items in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Destroy.DestroyObjects(room, creatures: false, items: true, onlyDead: false);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.destroyRoomObjectsMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.destroyRoomObjectsMenu));
            if (Options.destroyRoomObjectsMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.destroyRoomObjectsIdx?.Value), new RadialMenu.Slot(nameof(Options.destroyRoomObjectsMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragDestroyAll",
                    tooltip = "Destroy all in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Destroy.DestroyObjects(room, creatures: true, items: true, onlyDead: false);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.destroyRegionCreaturesMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.destroyRegionCreaturesMenu));
            if (Options.destroyRegionCreaturesMenu?.Value != false || Options.destroyRegionItemsMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.destroyRegionCreaturesIdx?.Value), new RadialMenu.Slot(nameof(Options.destroyRegionCreaturesMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragDestroyGlobal",
                    reload = (game, slot, chunk) => {
                        string tooltip = "Destroy all in region";
                        if (!(Options.destroyRegionCreaturesMenu?.Value != false))
                            tooltip = "Destroy items in region";
                        if (!(Options.destroyRegionItemsMenu?.Value != false))
                            tooltip = "Destroy creatures in region";
                        slot.tooltip = tooltip;
                    },
                    actionBC = (game, slot, chunk) => {
                        Destroy.DestroyRegionObjects(game, Options.destroyRegionCreaturesMenu?.Value == true, Options.destroyRegionItemsMenu?.Value == true);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.destroyRoomDeadCreaturesMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.destroyRoomDeadCreaturesMenu));
            if (Options.destroyRoomDeadCreaturesMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.destroyRoomDeadCreaturesIdx?.Value), new RadialMenu.Slot(nameof(Options.destroyRoomDeadCreaturesMenu)) {
                    requiresBodyChunk = false,
                    name = "mousedragDestroyDeadCreatures",
                    tooltip = "Destroy dead creatures in room",
                    actionBC = (game, slot, chunk) => {
                        var room = Drag.MouseCamera(game)?.room;
                        Destroy.DestroyObjects(room, creatures: true, items: false, onlyDead: true);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.lockMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.lockMenu));
            if (Options.lockMenu?.Value != false)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.lockIdx?.Value), new RadialMenu.Slot(nameof(Options.lockMenu)) {
                    requiresBodyChunk = true,
                    reload = (game, slot, chunk) => {
                        bool unlocked = Lock.ListContains(chunk) == null;
                        slot.name = unlocked ? "mousedragLocked" : "mousedragUnlocked";
                        slot.tooltip = unlocked ? "Lock position" : "Unlock position";
                        Lock.radMSelectsALock = !unlocked;
                    },
                    update = (game, slot, chunk) => {
                        Lock.hoversOverSlot = slot.hover;
                    },
                    actionBC = (game, slot, chunk) => {
                        Lock.SetLock(chunk, toggle: true, apply: true);
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.copySelectorMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.copySelectorMenu));
            if (Options.copySelectorMenu?.Value != false && Integration.devConsoleEnabled)
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.copySelectorIdx?.Value), new RadialMenu.Slot(nameof(Options.copySelectorMenu)) {
                    requiresBodyChunk = null,
                    name = "mousedragCLI",
                    reload = (game, slot, chunk) => {
                        slot.tooltip = chunk != null ? "Copy selector & open console" : "Open console";
                    },
                    actionPO = (game, slot, po) => {
                        if (!Integration.devConsoleEnabled)
                            return;
                        try {
                            Integration.DevConsoleOpen(po == null ? null : Integration.DevConsoleGetSelector(po.abstractPhysicalObject));
                        } catch {
                            Plugin.Logger.LogError("Exception while writing Dev Console, integration is now disabled");
                            Integration.devConsoleEnabled = false;
                            throw; //throw original exception while preserving stack trace
                        }
                    }
                });
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.gravityRoomMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.gravityRoomMenu));
            if (Options.gravityRoomMenu?.Value != false) {
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.gravityRoomIdx?.Value), new RadialMenu.Slot(nameof(Options.gravityRoomMenu)) {
                    requiresBodyChunk = false,
                    tooltip = "Set gravity",
                    reload = (game, slot, chunk) => {
                        if (Gravity.gravityType == Gravity.GravityTypes.None) {
                            slot.name = "mousedragGravityReset";
                        } else if (Gravity.gravityType == Gravity.GravityTypes.Off ||
                            Gravity.gravityType == Gravity.GravityTypes.Custom) {
                            slot.name = "mousedragGravityOff";
                        } else if (Gravity.gravityType == Gravity.GravityTypes.Low) {
                            slot.name = "mousedragGravityHalf";
                        } else if (Gravity.gravityType == Gravity.GravityTypes.On) {
                            slot.name = "mousedragGravityOn";
                        } else if (Gravity.gravityType == Gravity.GravityTypes.Inverse) {
                            slot.name = "mousedragGravityInverse";
                        }
                    },
                    actionBC = (game, slot, chunk) => {
                        MenuManager.SetSubMenuID("Gravity");
                    }
                });
                //submenu for quick select gravity type
                MenuManager.registeredSlots.Add(new RadialMenu.Slot(nameof(Options.gravityRoomMenu)) {
                    name = "mousedragGravityReset",
                    tooltip = "Reset gravity",
                    subMenuID = "Gravity",
                    actionBC = (game, slot, chunk) => {
                        Gravity.gravityType = Gravity.GravityTypes.None;
                        MenuManager.GoToRootMenu();
                    },
                    reload = (game, slot, chunk) => {
                        MenuManager.lowPrioText = "Select gravity type";
                    }
                });
                MenuManager.registeredSlots.Add(new RadialMenu.Slot(nameof(Options.gravityRoomMenu)) {
                    name = "mousedragGravityOff",
                    tooltip = "Gravity off",
                    subMenuID = "Gravity",
                    actionBC = (game, slot, chunk) => {
                        Gravity.gravityType = Gravity.GravityTypes.Off;
                        MenuManager.GoToRootMenu();
                    }
                });
                MenuManager.registeredSlots.Add(new RadialMenu.Slot(nameof(Options.gravityRoomMenu)) {
                    name = "mousedragGravityHalf",
                    tooltip = "Gravity low",
                    subMenuID = "Gravity",
                    actionBC = (game, slot, chunk) => {
                        Gravity.gravityType = Gravity.GravityTypes.Low;
                        MenuManager.GoToRootMenu();
                    }
                });
                MenuManager.registeredSlots.Add(new RadialMenu.Slot(nameof(Options.gravityRoomMenu)) {
                    name = "mousedragGravityOn",
                    tooltip = "Gravity on",
                    subMenuID = "Gravity",
                    actionBC = (game, slot, chunk) => {
                        Gravity.gravityType = Gravity.GravityTypes.On;
                        MenuManager.GoToRootMenu();
                    }
                });
                MenuManager.registeredSlots.Add(new RadialMenu.Slot(nameof(Options.gravityRoomMenu)) {
                    name = "mousedragGravityInverse",
                    tooltip = "Gravity inversed",
                    subMenuID = "Gravity",
                    actionBC = (game, slot, chunk) => {
                        Gravity.gravityType = Gravity.GravityTypes.Inverse;
                        MenuManager.GoToRootMenu();
                    }
                });
            }
            idx = MenuManager.registeredSlots.FindIndex(slot => slot.slotID == nameof(Options.infoMenu));
            MenuManager.registeredSlots.RemoveAll(slot => slot.slotID == nameof(Options.infoMenu));
            if (Options.infoMenu?.Value != false) {
                string dumpedInfo = string.Empty;
                MenuManager.registeredSlots.Insert(validIdx(idx, Options.infoIdx?.Value), new RadialMenu.Slot(nameof(Options.infoMenu)) {
                    requiresBodyChunk = null,
                    name = "mousedragInfo",
                    reload = (game, slot, chunk) => {
                        slot.tooltip = chunk != null ? "Copy info" : "Copy room info";
                    },
                    actionPO = (game, slot, po) => {
                        if (po != null) {
                            if (dumpedInfo == string.Empty) { //first object?
                                MenuManager.highPrioText = "Object copied to clipboard";
                            } else {
                                MenuManager.highPrioText = "Objects copied to clipboard";
                            }
                            dumpedInfo += Info.GetInfo(po);
                        } else {
                            MenuManager.highPrioText = "Room copied to clipboard";
                            dumpedInfo += Info.GetInfo(Drag.MouseCamera(game)?.room);
                        }
                    },
                    finalize = (game, slot) => {
                        if (!string.IsNullOrEmpty(dumpedInfo))
                            Info.CopyToClipboard(dumpedInfo);
                        dumpedInfo = string.Empty;
                    }
                });
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("StandardSlots.RegisterSlots called");
        }
    }
}
