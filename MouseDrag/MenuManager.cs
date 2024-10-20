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
        public static string tempText = "";
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
                tempText = "";
            }
            shouldOpen = false;

            if (menu?.closed == true || !State.activated) {
                menu?.Destroy();
                menu = null;
            }

            if (menu == null)
                return;

            //followchunk changed
            if (menu.prevFollowChunk != menu.followChunk) {
                reloadSlots = true;
                subMenuType = SubMenuTypes.None;
                tempText = "";
                page = 0;
            }

            RadialMenu.Slot slot = menu.Update(game);

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject ^ prevFollowsObject || reloadSlots) {
                ReloadSlots(game, menu, menu.followChunk);
                CreatePage();
                menu.LoadSlots(slots);
            }
            prevFollowsObject = followsObject;
            reloadSlots = false;

            if (slot != null) {
                tempText = "";

                //run command if a menu slot was pressed
                RunAction(game, slot, menu.followChunk);

                //reload slots if command is executed
                reloadSlots = true;
            }

            //change label text
            if (tempText?.Length > 0) {
                menu.labelText = tempText;
            } else if (menu.followChunk?.owner != null) {
                menu.labelText = menu.followChunk?.owner.ToString();
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
                page++;
                return;
            }

            //submenu for quick select gravity type
            if (subMenuType == SubMenuTypes.Gravity) {
                subMenuType = SubMenuTypes.None;
                switch (slot.name)
                {
                    case "mousedragGravityReset":   Gravity.gravityType = Gravity.GravityTypes.None; break;
                    case "mousedragGravityOff":     Gravity.gravityType = Gravity.GravityTypes.Off; break;
                    case "mousedragGravityHalf":    Gravity.gravityType = Gravity.GravityTypes.Half; break;
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
                        Health.TriggerObject(chunk.owner);
                        break;
                    case "mousedragRevive":
                        Health.ReviveCreature(chunk.owner);
                        Health.ResetObject(chunk.owner);
                        break;
                    case "mousedragDuplicate":      Duplicate.DuplicateObject(chunk.owner); break;
                    case "mousedragCut":            Clipboard.CutObject(chunk.owner); break;
                    case "mousedragCrosshair":      Teleport.SetWaypoint(Drag.MouseCamera(game)?.room, menu.menuPos, chunk); break;
                    case "mousedragMove":
                        if (chunk.owner is Player && !(chunk.owner as Player).isNPC) {
                            //skip selection and uncontrol all
                            Control.ToggleControl(game, chunk.owner as Creature);
                        } else if (chunk.owner is Creature) {
                            //creature can be controlled
                            subMenuType = SubMenuTypes.SafariPlayer;
                        }
                        break;
                    case "mousedragUnmove":         Control.ToggleControl(game, chunk.owner as Creature); break;
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
                        tempText = "Object Copied To Clipboard";
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
                        if (Options.pauseAllCreaturesMenu?.Value != false && Options.pauseAllObjectsMenu?.Value != false) {
                            Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                            Pause.pauseAllObjects = Pause.pauseAllCreatures;
                        } else if (Options.pauseAllCreaturesMenu?.Value != false) {
                            Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                        } else if (Options.pauseAllObjectsMenu?.Value != false) {
                            Pause.pauseAllObjects = !Pause.pauseAllObjects;
                        }
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("pauseAllCreatures: " + Pause.pauseAllCreatures + ", pauseAllObjects: " + Pause.pauseAllObjects);
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
                    case "mousedragDestroyCreatures":       Destroy.DestroyObjects(rcam?.room, creatures: true, objects: false, onlyDead: false); break;
                    case "mousedragDestroyItems":           Destroy.DestroyObjects(rcam?.room, creatures: false, objects: true, onlyDead: false); break;
                    case "mousedragDestroyAll":             Destroy.DestroyObjects(rcam?.room, creatures: true, objects: true, onlyDead: false); break;
                    case "mousedragDestroyGlobal":          Destroy.DestroyRegionObjects(game, Options.destroyRegionCreaturesMenu?.Value == true, Options.destroyRegionObjectsMenu?.Value == true); break;
                    case "mousedragDestroyDeadCreatures":   Destroy.DestroyObjects(rcam?.room, creatures: true, objects: false, onlyDead: true); break;
                    case "mousedragGravityReset":
                    case "mousedragGravityOff":
                    case "mousedragGravityHalf":
                    case "mousedragGravityOn":
                    case "mousedragGravityInverse":         subMenuType = SubMenuTypes.Gravity; break;
                    case "mousedragInfo":
                        Info.DumpInfo(rcam?.room);
                        tempText = "Room Copied To Clipboard";
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
                slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityReset" });
                slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityOff" });
                slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityHalf" });
                slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityOn" });
                slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityInverse" });
                tempText = "Select Gravity Type";

            } else if (subMenuType == SubMenuTypes.SafariPlayer) {
                //do not add sprites

            } else if (chunk?.owner != null) {
                //menu follows object
                if (Options.pauseOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Pause.IsObjectPaused(chunk.owner) ? "mousedragPlay" : "mousedragPause"
                    });
                if (Options.killOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragKill" });
                if (Options.reviveOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragRevive" });
                if (Options.duplicateOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDuplicate" });
                if (Options.clipboardMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragCut" });
                if (Options.tpWaypointCrMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragCrosshair" });
                if (Options.controlMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = (chunk.owner.abstractPhysicalObject as AbstractCreature)?.controlled == true ? "mousedragUnmove" : "mousedragMove",
                        curIconColor = chunk.owner.abstractPhysicalObject is AbstractCreature ? Color.white : Color.grey
                    });
                if (Options.forcefieldMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Forcefield.HasForcefield(chunk) ? "mousedragForceFieldOff" : "mousedragForceFieldOn"
                    });
                if (Options.tameOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragHeart",
                        curIconColor = Tame.IsTamable(game, chunk.owner) ? Color.white : Color.grey
                    });
                if (Options.clearRelOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = "mousedragUnheart",
                        curIconColor = Tame.IsTamable(game, chunk.owner) ? Color.white : Color.grey
                    });
                if (Options.stunOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Stun.IsObjectStunned(chunk.owner) ? "mousedragUnstun" : "mousedragStun",
                        curIconColor = chunk.owner is Oracle || chunk.owner is Creature ? Color.white : Color.grey
                    });
                if (Options.destroyOneMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDestroy" });
                if (Options.lockMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Lock.ListContains(chunk) == null ? "mousedragLocked" : "mousedragUnlocked"
                    });
                if (Options.infoMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragInfo" });

            } else {
                //menu on background
                if (Options.pauseRoomCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragPauseCreatures" });
                if (Options.pauseAllCreaturesMenu?.Value != false) {
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Pause.pauseAllCreatures ? "mousedragPlayGlobal" : "mousedragPauseGlobal"
                    });
                } else if (Options.pauseAllObjectsMenu?.Value != false) {
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Pause.pauseAllObjects ? "mousedragPlayGlobal" : "mousedragPauseGlobal"
                    });
                }
                if (Options.unpauseAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragPlayAll" });
                if (Options.killAllCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragKillCreatures" });
                if (Options.reviveAllCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragReviveCreatures" });
                if (Options.clipboardMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragPaste" });
                if (Options.tpWaypointBgMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragCrosshair" });
                if (Options.tameAllCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragHeartCreatures" });
                if (Options.clearRelAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragUnheartCreatures" });
                if (Options.stunRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragStunAll" });
                if (Options.unstunAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragUnstunAll" });
                if (Options.stunAllMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) {
                        name = Stun.stunAll ? "mousedragUnstunGlobal" : "mousedragStunGlobal"
                    });
                if (Options.destroyAllCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDestroyCreatures" });
                if (Options.destroyAllObjectsMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDestroyItems" });
                if (Options.destroyRoomMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDestroyAll" });
                if (Options.destroyRegionCreaturesMenu?.Value != false || Options.destroyRegionObjectsMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDestroyGlobal" });
                if (Options.destroyAllDeadCreaturesMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragDestroyDeadCreatures" });
                if (Options.gravityRoomMenu?.Value != false) {
                    if (Gravity.gravityType == Gravity.GravityTypes.None) {
                        slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityReset" });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Off) {
                        slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityOff" });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Half) {
                        slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityHalf" });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.On) {
                        slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityOn" });
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Inverse) {
                        slots.Add(new RadialMenu.Slot(menu) { name = "mousedragGravityInverse" });
                    }
                }
                if (Options.infoMenu?.Value != false)
                    slots.Add(new RadialMenu.Slot(menu) { name = "mousedragInfo" });
            }

            //add labels
            if (subMenuType == SubMenuTypes.SafariPlayer) {
                //add all selectable safari control players to submenu
                for (int i = 0; i < game?.rainWorld?.options?.controls?.Length; i++)
                    slots.Add(new RadialMenu.Slot(menu) { name = (i + 1).ToString(), isLabel = true });
                tempText = "Select Safari Player";
            }

            return slots;
        }


        public static int page = 0;
        public static void CreatePage()
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
            slots.Add(new RadialMenu.Slot(menu) { name = "+", isLabel = true });
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

            if (RadialMenu.menuButtonPressed(noRMB: inSandboxAndEditing || beastMasterOpened || devToolsOpened)) {
                shouldOpen = true;
            } else if (beastMasterOpened && RadialMenu.menuButtonPressed()) {
                menu?.Destroy();
                menu = null;
            }

            //also use scroll wheel to navigate pages
            if (UnityEngine.Input.mouseScrollDelta.y < 0) {
                page++;
                reloadSlots = true;
            }
            if (UnityEngine.Input.mouseScrollDelta.y > 0) {
                page--;
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
