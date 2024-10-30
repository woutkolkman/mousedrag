using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace MouseDrag
{
    public static class MenuManager
    {
        public static RadialMenu menu = null;
        public static bool shouldOpen = false; //signal from RawUpdate to open menu
        public static bool prevFollowsObject = false;
        public static bool reloadSlots = false;
        public static string lowPrioText, highPrioText;
        public static List<RadialMenu.Slot> slots = new List<RadialMenu.Slot>(){};
        public static SubMenuTypes subMenuType;

        public enum SubMenuTypes
        {
            None,
            Gravity,
            SafariPlayer
        }


        public static void Update(RainWorldGame game)
        {
            if (shouldOpen && menu == null && State.activated) {
                menu = new RadialMenu(game);
                reloadSlots = true;
                subMenuType = SubMenuTypes.None;
                lowPrioText = null;
                highPrioText = null;
            }
            shouldOpen = false;

            if (menu?.closed == true || !State.activated) {
                menu?.Destroy();
                menu = null;
            }

            //menu closed
            if (menu == null) {
                page = 0;
                subPage = 0;
                return;
            }

            //followchunk changed
            if (menu.prevFollowChunk != menu.followChunk) {
                reloadSlots = true;
                subMenuType = SubMenuTypes.None;
                lowPrioText = null;
                highPrioText = null;

                //menu switched from bodychunk to background or vice versa
                if (menu.prevFollowChunk == null || menu.followChunk == null) {
                    page = 0;
                    subPage = 0;
                }
            }

            RadialMenu.Slot slot = menu.Update(game, out RadialMenu.Slot hoverSlot);

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject ^ prevFollowsObject || reloadSlots) {
                ReloadSlots(game, menu, menu.followChunk);
                if (subMenuType == SubMenuTypes.None) {
                    CreatePage(ref page);
                } else {
                    CreatePage(ref subPage);
                }
                menu.LoadSlots(slots);
            }
            prevFollowsObject = followsObject;
            reloadSlots = false;

            if (slot != null) {
                lowPrioText = null;
                highPrioText = null;

                //run command if a menu slot was pressed
                RunAction(game, slot, menu.followChunk);

                //reload slots if command is executed
                reloadSlots = true;
            }

            //change label text
            if (highPrioText?.Length > 0) {
                menu.labelText = highPrioText;
            } else if (!string.IsNullOrEmpty(hoverSlot?.tooltip) && Options.showTooltips?.Value != false) {
                menu.labelText = hoverSlot.tooltip;
            } else if (lowPrioText?.Length > 0) {
                menu.labelText = lowPrioText;
            } else if (menu.followChunk?.owner != null) {
                menu.labelText = menu.followChunk?.owner.ToString();
            } else if (!string.IsNullOrEmpty(menu.roomName)) {
                menu.labelText = menu.roomName;
            } else {
                menu.labelText = "";
            }
        }


        //add-on mods need to hook the RunAction() function, and do an action when their slot was pressed
        public static void RunAction(RainWorldGame game, RadialMenu.Slot slot, BodyChunk chunk)
        {
            if (slot?.name == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("MenuManager.RunAction, slot null or invalid name");
                return;
            }

            //next page
            if (slot.name == "+") {
                if (subMenuType == SubMenuTypes.None) {
                    page++;
                } else {
                    subPage++;
                }
                return;
            }

            //submenu for quick select gravity type
            if (subMenuType == SubMenuTypes.Gravity) {
                subMenuType = SubMenuTypes.None;
                switch (slot.name)
                {
                    case "mousedragGravityReset":   Gravity.gravityType = Gravity.GravityTypes.None; break;
                    case "mousedragGravityOff":     Gravity.gravityType = Gravity.GravityTypes.Off; break;
                    case "mousedragGravityHalf":    Gravity.gravityType = Gravity.GravityTypes.Low; break;
                    case "mousedragGravityOn":      Gravity.gravityType = Gravity.GravityTypes.On; break;
                    case "mousedragGravityInverse": Gravity.gravityType = Gravity.GravityTypes.Inverse; break;
                }
                return;
            }

            //submenu for selecting player number which will safari control creature
            if (subMenuType == SubMenuTypes.SafariPlayer) {
                subMenuType = SubMenuTypes.None;
                if (!Int32.TryParse(slot.name, out int pI)) {
                    Plugin.Logger.LogWarning("MenuManager.RunAction, parsing playerIndex failed, value is \"" + slot.name + "\"");
                    return;
                }
                Drag.playerNr = pI - 1;
                Control.ToggleControl(game, chunk?.owner as Creature);
                return;
            }

            if (chunk?.owner != null) {
                //menu follows object
                switch (slot.name)
                {
                    case "mousedragPause":
                    case "mousedragPlay":           Pause.TogglePauseObject(chunk.owner); break;
                    case "mousedragKill":
                        Health.KillCreature(game, chunk.owner);
                        Health.TriggerItem(chunk.owner);
                        break;
                    case "mousedragRevive":
                        Health.ReviveCreature(chunk.owner);
                        Health.ResetItem(chunk.owner);
                        break;
                    case "mousedragDuplicate":      Duplicate.DuplicateObject(chunk.owner); break;
                    case "mousedragCut":            Clipboard.CutObject(chunk.owner); break;
                    case "mousedragCrosshair":      Teleport.SetWaypoint(Drag.MouseCamera(game)?.room, menu.menuPos, chunk); break;
                    case "mousedragControl":
                        if (chunk.owner is Player && !(chunk.owner as Player).isNPC) {
                            //skip selection and uncontrol all
                            Control.ToggleControl(game, chunk.owner as Creature);
                        } else if (chunk.owner is Creature) {
                            //creature can be controlled
                            subMenuType = SubMenuTypes.SafariPlayer;
                        }
                        break;
                    case "mousedragUncontrol":      Control.ToggleControl(game, chunk.owner as Creature); break;
                    case "mousedragForceFieldOn":
                    case "mousedragForceFieldOff":  Forcefield.ToggleForcefield(chunk); break;
                    case "mousedragHeart":          Tame.TameCreature(game, chunk.owner); break;
                    case "mousedragUnheart":        Tame.ClearRelationships(chunk.owner); break;
                    case "mousedragStun":
                    case "mousedragUnstun":         Stun.ToggleStunObject(chunk.owner); break;
                    case "mousedragDestroy":        Destroy.DestroyObject(chunk.owner); break;
                    case "mousedragLocked":
                    case "mousedragUnlocked":       Lock.ToggleLock(chunk); break;
                    case "mousedragInfo":
                        Info.DumpInfo(chunk.owner);
                        highPrioText = "Object copied to clipboard";
                        break;
                }

            } else {
                var rcam = Drag.MouseCamera(game);

                //menu on background
                switch (slot.name)
                {
                    case "mousedragPauseCreatures":         Pause.PauseObjects(rcam?.room, true); break;
                    case "mousedragPauseGlobal":
                    case "mousedragPlayGlobal":
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
                        break;
                    case "mousedragPlayAll":                Pause.UnpauseAll(); break;
                    case "mousedragKillCreatures":          Health.KillCreatures(game, rcam?.room); break;
                    case "mousedragReviveCreatures":        Health.ReviveCreatures(rcam?.room); break;
                    case "mousedragPaste":
                        if (rcam?.room != null)
                            Clipboard.PasteObject(game, rcam.room, rcam.room.ToWorldCoordinate(menu.menuPos));
                        break;
                    case "mousedragCrosshair":              Teleport.SetWaypoint(rcam?.room, menu.menuPos); break;
                    case "mousedragHeartCreatures":         Tame.TameCreatures(game, rcam?.room); break;
                    case "mousedragUnheartCreatures":       Tame.ClearRelationships(rcam?.room); break;
                    case "mousedragStunAll":                Stun.StunObjects(rcam?.room); break;
                    case "mousedragUnstunAll":              Stun.UnstunAll(); break;
                    case "mousedragStunGlobal":
                    case "mousedragUnstunGlobal":
                        Stun.stunAll = !Stun.stunAll;
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
                        break;
                    case "mousedragDestroyCreatures":       Destroy.DestroyObjects(rcam?.room, creatures: true, items: false, onlyDead: false); break;
                    case "mousedragDestroyItems":           Destroy.DestroyObjects(rcam?.room, creatures: false, items: true, onlyDead: false); break;
                    case "mousedragDestroyAll":             Destroy.DestroyObjects(rcam?.room, creatures: true, items: true, onlyDead: false); break;
                    case "mousedragDestroyGlobal":          Destroy.DestroyRegionObjects(game, Options.destroyRegionCreaturesMenu?.Value == true, Options.destroyRegionItemsMenu?.Value == true); break;
                    case "mousedragDestroyDeadCreatures":   Destroy.DestroyObjects(rcam?.room, creatures: true, items: false, onlyDead: true); break;
                    case "mousedragGravityReset":
                    case "mousedragGravityOff":
                    case "mousedragGravityHalf":
                    case "mousedragGravityOn":
                    case "mousedragGravityInverse":         subMenuType = SubMenuTypes.Gravity; break;
                    case "mousedragInfo":
                        Info.DumpInfo(rcam?.room);
                        highPrioText = "Room copied to clipboard";
                        break;
                }
            }
        }


        //add-on mods need to hook the ReloadSlots() function, and insert their slots afterwards
        public static List<RadialMenu.Slot> ReloadSlots(RainWorldGame game, RadialMenu menu, BodyChunk chunk)
        {
            slots.Clear();

            //add sprites
            if (subMenuType == SubMenuTypes.Gravity) {
                //add all selectable gravity types to submenu
                slots.Add(new RadialMenu.Slot(menu) {
                    name = "mousedragGravityReset",
                    tooltip = "Reset gravity"
                });
                slots.Add(new RadialMenu.Slot(menu) {
                    name = "mousedragGravityOff",
                    tooltip = "Gravity off"
                });
                slots.Add(new RadialMenu.Slot(menu) {
                    name = "mousedragGravityHalf",
                    tooltip = "Gravity low"
                });
                slots.Add(new RadialMenu.Slot(menu) {
                    name = "mousedragGravityOn",
                    tooltip = "Gravity on"
                });
                slots.Add(new RadialMenu.Slot(menu) {
                    name = "mousedragGravityInverse",
                    tooltip = "Gravity inversed"
                });
                lowPrioText = "Select gravity type";

            } else if (subMenuType == SubMenuTypes.SafariPlayer) {
                //do not add sprites

            } else if (chunk?.owner != null) {
                //menu follows object
                if (Options.pauseOneMenu?.Value != false) {
                    bool paused = Pause.IsObjectPaused(chunk.owner);
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = paused ? "mousedragPlay" : "mousedragPause",
                        tooltip = paused ? "Unpause" : "Pause"
                    });
                }
                if (Options.killOneMenu?.Value != false) {
                    string tooltip = "Trigger";
                    if (chunk.owner is Creature) {
                        tooltip = "Kill";
                        if ((chunk.owner as Creature).abstractCreature?.state is HealthState)
                            tooltip += " (" + ((chunk.owner as Creature).abstractCreature.state as HealthState).health + "/1)";
                    }
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragKill",
                        tooltip = tooltip
                    });
                }
                if (Options.reviveOneMenu?.Value != false) {
                    string tooltip = "Reset";
                    if (chunk.owner is Creature) {
                        tooltip = "Revive/heal";
                        if ((chunk.owner as Creature).abstractCreature?.state is HealthState)
                            tooltip += " (" + ((chunk.owner as Creature).abstractCreature.state as HealthState).health + "/1)";
                    }
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragRevive",
                        tooltip = tooltip
                    });
                }
                if (Options.duplicateOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDuplicate",
                        tooltip = "Duplicate"
                    });
                if (Options.clipboardMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragCut",
                        tooltip = "Cut"
                    });
                if (Options.tpWaypointCrMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragCrosshair",
                        tooltip = Teleport.crosshair == null ? "Set teleport position" : "Cancel teleportation"
                    });
                if (Options.controlMenu?.Value != false) {
                    bool isControlled = (chunk.owner.abstractPhysicalObject as AbstractCreature)?.controlled == true;
                    bool isPlayer = chunk.owner is Player && !(chunk.owner as Player).isNPC;
                    bool hasControl = false;
                    if (isPlayer)
                        hasControl = Control.PlayerHasControl((chunk.owner as Player).playerState?.playerNumber ?? -1);
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = isControlled ? "mousedragUncontrol" : "mousedragControl",
                        tooltip = isPlayer ? "Release all for this player" : (isControlled ? "Release control" : "Safari-control"),
                        curIconColor = chunk.owner.abstractPhysicalObject is AbstractCreature && (!isPlayer || hasControl) ? Color.white : Color.grey
                    });
                }
                if (Options.forcefieldMenu?.Value != false) {
                    bool forcefield = Forcefield.HasForcefield(chunk);
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = forcefield ? "mousedragForceFieldOff" : "mousedragForceFieldOn",
                        tooltip = forcefield ? "Disable forcefield" : "Enable forcefield"
                    });
                }
                if (Options.tameOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragHeart",
                        tooltip = "Tame",
                        curIconColor = Tame.IsTamable(game, chunk.owner) ? Color.white : Color.grey
                    });
                if (Options.clearRelOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragUnheart",
                        tooltip = "Clear relationships",
                        curIconColor = Tame.IsTamable(game, chunk.owner) ? Color.white : Color.grey
                    });
                if (Options.stunOneMenu?.Value != false) {
                    bool stunned = Stun.IsObjectStunned(chunk.owner);
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = stunned ? "mousedragUnstun" : "mousedragStun",
                        tooltip = stunned ? "Unstun" : "Stun",
                        curIconColor = chunk.owner is Oracle || chunk.owner is Creature ? Color.white : Color.grey
                    });
                }
                if (Options.destroyOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDestroy",
                        tooltip = "Destroy"
                    });
                if (Options.lockMenu?.Value != false) {
                    bool unlocked = Lock.ListContains(chunk) == null;
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = unlocked ? "mousedragLocked" : "mousedragUnlocked",
                        tooltip = unlocked ? "Lock position" : "Unlock position"
                    });
                }
                if (Options.infoMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragInfo",
                        tooltip = "Copy info"
                    });

            } else {
                //menu on background
                if (Options.pauseRoomCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragPauseCreatures",
                        tooltip = "Pause creatures in room"
                    });
                if (Options.pauseAllCreaturesMenu?.Value != false) {
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Pause.pauseAllCreatures ? "mousedragPlayGlobal" : "mousedragPauseGlobal",
                        tooltip = Options.pauseAllItemsMenu?.Value != false ? 
                        (Pause.pauseAllCreatures ? "Unpause all" : "Pause all") : 
                        (Pause.pauseAllCreatures ? "Unpause all creatures" : "Pause all creatures")
                    });
                } else if (Options.pauseAllItemsMenu?.Value != false) {
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Pause.pauseAllItems ? "mousedragPlayGlobal" : "mousedragPauseGlobal",
                        tooltip = Pause.pauseAllItems ? "Unpause all items" : "Pause all items"
                    });
                }
                if (Options.unpauseAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragPlayAll",
                        tooltip = "Unpause all",
                        curIconColor = Pause.pausedObjects.Count > 0 || Pause.pauseAllCreatures || Pause.pauseAllItems ? Color.white : Color.grey
                    });
                if (Options.killRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragKillCreatures",
                        tooltip = "Kill in room"
                    });
                if (Options.reviveRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragReviveCreatures",
                        tooltip = "Revive/heal in room"
                    });
                if (Options.clipboardMenu?.Value != false) {
                    string tooltip = "Paste";
                    if (Clipboard.cutObjects.Count > 0)
                        tooltip += " " + Clipboard.cutObjects[Clipboard.cutObjects.Count - 1]?.realizedObject?.ToString();
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragPaste",
                        tooltip = tooltip,
                        curIconColor = Clipboard.cutObjects.Count > 0 ? Color.white : Color.grey
                    });
                }
                if (Options.tpWaypointBgMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragCrosshair",
                        tooltip = Teleport.crosshair == null ? "Set teleport position" : "Cancel teleportation"
                    });
                if (Options.tameRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragHeartCreatures",
                        tooltip = "Tame in room"
                    });
                if (Options.clearRelRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragUnheartCreatures",
                        tooltip = "Clear relationships in room"
                    });
                if (Options.stunRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragStunAll",
                        tooltip = "Stun in room"
                    });
                if (Options.unstunAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragUnstunAll",
                        tooltip = "Unstun all",
                        curIconColor = Stun.stunnedObjects.Count > 0 || Stun.stunAll ? Color.white : Color.grey
                    });
                if (Options.stunAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Stun.stunAll ? "mousedragUnstunGlobal" : "mousedragStunGlobal",
                        tooltip = Stun.stunAll ? "Unstun all" : "Stun all"
                    });
                if (Options.destroyRoomCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDestroyCreatures",
                        tooltip = "Destroy creatures in room"
                    });
                if (Options.destroyRoomItemsMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDestroyItems",
                        tooltip = "Destroy items in room"
                    });
                if (Options.destroyRoomObjectsMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDestroyAll",
                        tooltip = "Destroy all in room"
                    });
                if (Options.destroyRegionCreaturesMenu?.Value != false || Options.destroyRegionItemsMenu?.Value != false) {
                    string tooltip = "Destroy all in region";
                    if (!(Options.destroyRegionCreaturesMenu?.Value != false))
                        tooltip = "Destroy items in region";
                    if (!(Options.destroyRegionItemsMenu?.Value != false))
                        tooltip = "Destroy creatures in region";
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDestroyGlobal",
                        tooltip = tooltip
                    });
                }
                if (Options.destroyRoomDeadCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragDestroyDeadCreatures",
                        tooltip = "Destroy dead creatures in room"
                    });
                if (Options.gravityRoomMenu?.Value != false) {
                    string tooltip = "Set gravity";
                    if (Gravity.gravityType == Gravity.GravityTypes.None) {
                        slots.Add(new RadialMenu.Slot(menu) {
                            name = "mousedragGravityReset",
                            tooltip = tooltip
                        });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Off) {
                        slots.Add(new RadialMenu.Slot(menu) {
                            name = "mousedragGravityOff",
                            tooltip = tooltip
                        });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Low) {
                        slots.Add(new RadialMenu.Slot(menu) {
                            name = "mousedragGravityHalf",
                            tooltip = tooltip
                        });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.On) {
                        slots.Add(new RadialMenu.Slot(menu) {
                            name = "mousedragGravityOn",
                            tooltip = tooltip
                        });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Inverse) {
                        slots.Add(new RadialMenu.Slot(menu) {
                            name = "mousedragGravityInverse",
                            tooltip = tooltip
                        });
                    }
                }
                if (Options.infoMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragInfo",
                        tooltip = "Copy room info"
                    });
            }

            //add labels
            if (subMenuType == SubMenuTypes.SafariPlayer) {
                //add all selectable safari control players to submenu
                for (int i = 0; i < game?.rainWorld?.options?.controls?.Length; i++)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = (i + 1).ToString(),
                        isLabel = true
                    });
                lowPrioText = "Select safari player";
            }

            return slots;
        }


        public static int page, subPage;
        public static void CreatePage(ref int page)
        {
            int maxOnPage = Options.maxOnPage?.Value ?? 7;
            int count = slots.Count;

            //go to last page if negative
            if (page < 0)
                page = (count - 1) / maxOnPage;

            //reset page if out of bounds
            if (count <= maxOnPage * page)
                page = 0;

            //no page slot is required
            if (count <= maxOnPage)
                return;

            for (int i = count - 1; i >= 0; i--) {
                if (i < (maxOnPage * page) + maxOnPage && 
                    i >= maxOnPage * page)
                    continue;
                slots.RemoveAt(i);
            }
            slots.Add(new RadialMenu.Slot(menu) {
                name = "+",
                tooltip = "Next page (" + (page + 1) + "/" + (((count - 1) / maxOnPage) + 1) + ")",
                isLabel = true
            });
        }


        public static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            //if editing in sandbox, disable open menu with right mouse button
            bool inSandboxAndEditing = (game.GetArenaGameSession as SandboxGameSession)?.overlay?.playMode == false;

            //if BeastMaster is enabled and opened, don't use right mouse button
            bool beastMasterOpened = false;
            if (Integration.beastMasterEnabled) {
                try {
                    beastMasterOpened = Integration.BeastMasterUsesRMB(game);
                } catch {
                    Plugin.Logger.LogError("MenuManager.RawUpdate exception while reading BeastMaster state, integration is now disabled");
                    Integration.beastMasterEnabled = false;
                    throw; //throw original exception while preserving stack trace
                }
            }

            //if regionkit is enabled and dev tools menu is opened, don't use right mouse button (because of iggy)
            bool devToolsOpened = Integration.regionKitEnabled && game.devUI != null;

            if (RadialMenu.menuOpenButtonPressed(noRMB: inSandboxAndEditing || beastMasterOpened || devToolsOpened)) {
                shouldOpen = true;
            } else if (beastMasterOpened && RadialMenu.menuOpenButtonPressed()) {
                menu?.Destroy();
                menu = null;
            }

            //also use scroll wheel to navigate pages
            if (UnityEngine.Input.mouseScrollDelta.y < 0) {
                if (subMenuType == SubMenuTypes.None) {
                    page++;
                } else {
                    subPage++;
                }
                reloadSlots = true;
            }
            if (UnityEngine.Input.mouseScrollDelta.y > 0) {
                if (subMenuType == SubMenuTypes.None) {
                    page--;
                } else {
                    subPage--;
                }
                reloadSlots = true;
            }
        }


        public static void DrawSprites(float timeStacker)
        {
            menu?.DrawSprites(timeStacker);
        }


        public static void LoadSprites()
        {
            try {
                Futile.atlasManager.LoadAtlas("sprites" + Path.DirectorySeparatorChar + "mousedrag");
            } catch (Exception ex) {
                Plugin.Logger.LogError("MenuManager.LoadSprites exception: " + ex.ToString());
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("LoadSprites called");
        }


        public static void UnloadSprites()
        {
            try {
                Futile.atlasManager.UnloadAtlas("sprites" + Path.DirectorySeparatorChar + "mousedrag");
            } catch (Exception ex) {
                Plugin.Logger.LogError("MenuManager.UnloadSprites exception: " + ex.ToString());
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("UnloadSprites called");
        }
    }
}
