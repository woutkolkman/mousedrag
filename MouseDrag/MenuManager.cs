using System.Collections.Generic;
using System;
using System.IO;

namespace MouseDrag
{
    public static class MenuManager
    {
        public static RadialMenu menu = null;
        public static bool shouldOpen = false; //signal from RawUpdate to open menu
        public static bool prevFollowsObject = false;
        public static bool reloadSlots = false;
        public static string tempText = "";
        public static List<string> iconNames = new List<string>(){};
        public static List<string> labelNames = new List<string>(){};
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
            string pressedSprite = slot?.name;

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject ^ prevFollowsObject || reloadSlots) {
                ReloadIconNames(game, followsObject);
                ReloadLabelNames(game, followsObject);
                CreatePage();
                menu.LoadSlots(iconNames, labelNames);
            }
            prevFollowsObject = followsObject;
            reloadSlots = false;

            if (!String.IsNullOrEmpty(pressedSprite)) {
                tempText = "";

                //run command if a menu slot was pressed
                RunCommand(game, pressedSprite, followsObject);

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


        //add-on mods need to hook the RunCommand() function, and do an action when spriteName is their sprite
        public static void RunCommand(RainWorldGame game, string spriteName, bool followsObject)
        {
            //next page
            if (spriteName == "+") {
                page++;
                return;
            }

            //submenu for quick select gravity type
            if (subMenuType == SubMenuTypes.Gravity) {
                subMenuType = SubMenuTypes.None;
                switch (spriteName)
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
                if (!Int32.TryParse(spriteName, out int pI)) {
                    Plugin.Logger.LogWarning("MenuManager.RunCommand, parsing playerIndex failed, value is \"" + spriteName + "\"");
                    return;
                }
                Drag.playerNr = pI - 1;
                Control.ToggleControl(game, menu.followChunk?.owner as Creature);
                return;
            }

            if (followsObject) {
                //menu follows object
                switch (spriteName)
                {
                    case "mousedragPause":
                    case "mousedragPlay":           Pause.TogglePauseObject(menu.followChunk?.owner); break;
                    case "mousedragKill":
                        Health.KillCreature(game, menu.followChunk?.owner);
                        Health.TriggerObject(menu.followChunk?.owner);
                        break;
                    case "mousedragRevive":
                        Health.ReviveCreature(menu.followChunk?.owner);
                        Health.ResetObject(menu.followChunk?.owner);
                        break;
                    case "mousedragDuplicate":      Duplicate.DuplicateObject(menu.followChunk?.owner); break;
                    case "mousedragCut":            Clipboard.CutObject(menu.followChunk?.owner); break;
                    case "mousedragCrosshair":      Teleport.SetWaypoint(Drag.MouseCamera(game)?.room, menu.menuPos, menu.followChunk); break;
                    case "mousedragMove":
                        if (menu.followChunk?.owner is Player && !(menu.followChunk.owner as Player).isNPC) {
                            //skip selection and uncontrol all
                            Control.ToggleControl(game, menu.followChunk?.owner as Creature);
                        } else if (menu.followChunk?.owner is Creature) {
                            //creature can be controlled
                            subMenuType = SubMenuTypes.SafariPlayer;
                        }
                        break;
                    case "mousedragUnmove":         Control.ToggleControl(game, menu.followChunk?.owner as Creature); break;
                    case "mousedragForceFieldOn":
                    case "mousedragForceFieldOff":  Forcefield.ToggleForcefield(menu.followChunk); break;
                    case "mousedragHeart":          Tame.TameCreature(game, menu.followChunk?.owner); break;
                    case "mousedragUnheart":        Tame.ClearRelationships(menu.followChunk?.owner); break;
                    case "mousedragStun":
                    case "mousedragUnstun":         Stun.ToggleStunObject(menu.followChunk?.owner); break;
                    case "mousedragDestroy":        Destroy.DestroyObject(menu.followChunk?.owner); break;
                    case "mousedragLocked":
                    case "mousedragUnlocked":       Lock.ToggleLock(menu.followChunk); break;
                    case "mousedragInfo":
                        Info.DumpInfo(menu.followChunk?.owner);
                        tempText = "Copied To Clipboard";
                        break;
                }

            } else {
                var rcam = Drag.MouseCamera(game);

                //menu on background
                switch (spriteName)
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
                }
            }
        }


        //add-on mods need to hook the ReloadIconNames() function, and insert their sprite names in iconNames afterwards
        public static List<string> ReloadIconNames(RainWorldGame game, bool followsObject)
        {
            iconNames.Clear();

            if (subMenuType == SubMenuTypes.Gravity) {
                //add all selectable gravity types to submenu
                iconNames.Add("mousedragGravityReset");
                iconNames.Add("mousedragGravityOff");
                iconNames.Add("mousedragGravityHalf");
                iconNames.Add("mousedragGravityOn");
                iconNames.Add("mousedragGravityInverse");
                tempText = "Select Gravity Type";

            } else if (subMenuType == SubMenuTypes.SafariPlayer) {

            } else if (followsObject) {
                //menu follows object
                if (Options.pauseOneMenu?.Value != false)
                    iconNames.Add(Pause.IsObjectPaused(menu.followChunk?.owner) ? "mousedragPlay" : "mousedragPause");
                if (Options.killOneMenu?.Value != false)
                    iconNames.Add("mousedragKill");
                if (Options.reviveOneMenu?.Value != false)
                    iconNames.Add("mousedragRevive");
                if (Options.duplicateOneMenu?.Value != false)
                    iconNames.Add("mousedragDuplicate");
                if (Options.clipboardMenu?.Value != false)
                    iconNames.Add("mousedragCut");
                if (Options.tpWaypointCrMenu?.Value != false)
                    iconNames.Add("mousedragCrosshair");
                if (Options.controlMenu?.Value != false)
                    iconNames.Add((menu.followChunk?.owner?.abstractPhysicalObject as AbstractCreature)?.controlled == true ? "mousedragUnmove" : "mousedragMove");
                if (Options.forcefieldMenu?.Value != false)
                    iconNames.Add(Forcefield.HasForcefield(menu.followChunk) ? "mousedragForceFieldOff" : "mousedragForceFieldOn");
                if (Options.tameOneMenu?.Value != false)
                    iconNames.Add("mousedragHeart");
                if (Options.clearRelOneMenu?.Value != false)
                    iconNames.Add("mousedragUnheart");
                if (Options.stunOneMenu?.Value != false)
                    iconNames.Add(Stun.IsObjectStunned(menu.followChunk?.owner) ? "mousedragUnstun" : "mousedragStun");
                if (Options.destroyOneMenu?.Value != false)
                    iconNames.Add("mousedragDestroy");
                if (Options.lockMenu?.Value != false)
                    iconNames.Add(Lock.ListContains(menu.followChunk) == null ? "mousedragLocked" : "mousedragUnlocked");
                if (Options.infoMenu?.Value != false)
                    iconNames.Add("mousedragInfo");

            } else {
                //menu on background
                if (Options.pauseRoomCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragPauseCreatures");
                if (Options.pauseAllCreaturesMenu?.Value != false) {
                    iconNames.Add(Pause.pauseAllCreatures ? "mousedragPlayGlobal" : "mousedragPauseGlobal");
                } else if (Options.pauseAllObjectsMenu?.Value != false) {
                    iconNames.Add(Pause.pauseAllObjects ? "mousedragPlayGlobal" : "mousedragPauseGlobal");
                }
                if (Options.unpauseAllMenu?.Value != false)
                    iconNames.Add("mousedragPlayAll");
                if (Options.killAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragKillCreatures");
                if (Options.reviveAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragReviveCreatures");
                if (Options.clipboardMenu?.Value != false)
                    iconNames.Add("mousedragPaste");
                if (Options.tpWaypointBgMenu?.Value != false)
                    iconNames.Add("mousedragCrosshair");
                if (Options.tameAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragHeartCreatures");
                if (Options.clearRelAllMenu?.Value != false)
                    iconNames.Add("mousedragUnheartCreatures");
                if (Options.stunRoomMenu?.Value != false)
                    iconNames.Add("mousedragStunAll");
                if (Options.unstunAllMenu?.Value != false)
                    iconNames.Add("mousedragUnstunAll");
                if (Options.stunAllMenu?.Value != false)
                    iconNames.Add(Stun.stunAll ? "mousedragUnstunGlobal" : "mousedragStunGlobal");
                if (Options.destroyAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragDestroyCreatures");
                if (Options.destroyAllObjectsMenu?.Value != false)
                    iconNames.Add("mousedragDestroyItems");
                if (Options.destroyRoomMenu?.Value != false)
                    iconNames.Add("mousedragDestroyAll");
                if (Options.destroyRegionCreaturesMenu?.Value != false || Options.destroyRegionObjectsMenu?.Value != false)
                    iconNames.Add("mousedragDestroyGlobal");
                if (Options.destroyAllDeadCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragDestroyDeadCreatures");
                if (Options.gravityRoomMenu?.Value != false) {
                    if (Gravity.gravityType == Gravity.GravityTypes.None) {
                        iconNames.Add("mousedragGravityReset");
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Off) {
                        iconNames.Add("mousedragGravityOff");
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Half) {
                        iconNames.Add("mousedragGravityHalf");
                    } else if (Gravity.gravityType == Gravity.GravityTypes.On) {
                        iconNames.Add("mousedragGravityOn");
                    } else if (Gravity.gravityType == Gravity.GravityTypes.Inverse) {
                        iconNames.Add("mousedragGravityInverse");
                    }
                }
            }

            return iconNames;
        }


        //add-on mods need to hook the ReloadLabelNames() function, and insert their text in labelNames afterwards
        public static List<string> ReloadLabelNames(RainWorldGame game, bool followsObject)
        {
            labelNames.Clear();

            if (subMenuType == SubMenuTypes.Gravity) {

            } else if (subMenuType == SubMenuTypes.SafariPlayer) {
                //add all selectable safari control players to submenu
                for (int i = 0; i < game?.rainWorld?.options?.controls?.Length; i++)
                    labelNames.Add((i + 1).ToString());
                tempText = "Select Safari Player";

            } else if (followsObject) {
                //menu follows object

            } else {
                //menu on background
            }

            return labelNames;
        }


        public static int page = 0;
        public static void CreatePage()
        {
            int maxOnPage = Options.maxOnPage?.Value ?? 7;
            int iconCount = iconNames.Count;
            int labelCount = labelNames.Count;

            //reset page if out of bounds
            if (iconCount + labelCount <= maxOnPage * page)
                page = 0;

            //no page slot is required
            if (iconCount + labelCount <= maxOnPage)
                return;

            for (int i = iconCount + labelCount - 1; i >= 0; i--) {
                if (i < (maxOnPage * page) + maxOnPage && 
                    i >= maxOnPage * page)
                    continue;
                if (i > iconCount - 1) {
                    labelNames.RemoveAt(i - iconCount);
                } else {
                    iconNames.RemoveAt(i);
                }
            }
            labelNames.Add("+");
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
    }
}
