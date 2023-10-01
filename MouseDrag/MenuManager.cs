using System.Collections.Generic;
using UnityEngine;
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
        public static List<string> iconNames = new List<string>(){};


        public static void Update(RainWorldGame game)
        {
            if (shouldOpen && menu == null && State.activated) {
                menu = new RadialMenu(game);
                reloadSlots = true;
            }
            shouldOpen = false;

            if (menu?.closed == true || !State.activated) {
                menu?.Destroy();
                menu = null;
            }

            if (menu == null)
                return;

            //reload slots if followchunk changed
            reloadSlots |= menu.prevFollowChunk != menu.followChunk;

            RadialMenu.Slot slot = menu.Update(game);
            string pressedSprite = slot?.iconName;

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject ^ prevFollowsObject || reloadSlots) {
                ReloadIconNames(followsObject);
                menu.LoadSlots(iconNames);
            }
            prevFollowsObject = followsObject;
            reloadSlots = false;

            //reload slots if command is executed
            reloadSlots |= !String.IsNullOrEmpty(pressedSprite);

            //run commands
            if (String.IsNullOrEmpty(pressedSprite))
                return;
            RunCommand(game, pressedSprite, followsObject);
        }


        //add-on mods need to hook the RunCommand() function, and do an action when spriteName is their sprite
        public static void RunCommand(RainWorldGame game, string spriteName, bool followsObject)
        {
            if (followsObject) {
                //menu follows object
                switch (spriteName)
                {
                    case "mousedragPause":
                    case "mousedragPlay":       Pause.TogglePauseObject(menu.followChunk?.owner); break;
                    case "mousedragStun":
                    case "mousedragUnstun":     Stun.ToggleStunObject(menu.followChunk?.owner); break;
                    case "mousedragKill":
                        Health.KillCreature(game, menu.followChunk?.owner);
                        Health.TriggerObject(menu.followChunk?.owner);
                        break;
                    case "mousedragRevive":
                        Health.ReviveCreature(menu.followChunk?.owner);
                        Health.ResetObject(menu.followChunk?.owner);
                        break;
                    case "mousedragHeart":      Tame.TameCreature(game, menu.followChunk?.owner); break;
                    case "mousedragUnheart":    Tame.ClearRelationships(menu.followChunk?.owner); break;
                    case "mousedragDuplicate":  Duplicate.DuplicateObject(menu.followChunk?.owner); break;
                    case "mousedragCut":        Clipboard.CutObject(menu.followChunk?.owner); break;
                    case "mousedragDestroy":    Destroy.DestroyObject(menu.followChunk?.owner); break;
                    case "mousedragCrosshair":  Teleport.SetWaypoint(game.cameras[0]?.room, menu.menuPos, menu.followChunk); break;
                }

            } else {
                //menu on background
                switch (spriteName)
                {
                    case "mousedragPauseCreatures":     Pause.PauseObjects(game.cameras[0]?.room, true); break;
                    case "mousedragPlayAll":            Pause.UnpauseAll(); break;
                    case "mousedragStunAll":            Stun.StunObjects(game.cameras[0]?.room); break;
                    case "mousedragUnstunAll":          Stun.UnstunAll(); break;
                    case "mousedragStunGlobal":
                    case "mousedragUnstunGlobal":
                        Stun.stunAll = !Stun.stunAll;
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
                        break;
                    case "mousedragKillCreatures":      Health.KillCreatures(game, game.cameras[0]?.room); break;
                    case "mousedragReviveCreatures":    Health.ReviveCreatures(game.cameras[0]?.room); break;
                    case "mousedragHeartCreatures":     Tame.TameCreatures(game, game.cameras[0]?.room); break;
                    case "mousedragUnheartCreatures":   Tame.ClearRelationships(game.cameras[0]?.room); break;
                    case "mousedragPaste":
                        if (game.cameras[0]?.room != null)
                            Clipboard.PasteObject(game, game.cameras[0].room, game.cameras[0].room.ToWorldCoordinate(menu.menuPos));
                        break;
                    case "mousedragDestroyCreatures":   Destroy.DestroyObjects(game.cameras[0]?.room, creatures: true, objects: false); break;
                    case "mousedragDestroyAll":         Destroy.DestroyObjects(game.cameras[0]?.room, creatures: true, objects: true); break;
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
                    case "mousedragCrosshair":          Teleport.SetWaypoint(game.cameras[0]?.room, menu.menuPos); break;
                }
            }
        }


        //add-on mods need to hook the ReloadIconNames() function, and insert their sprite names in iconNames afterwards
        public static List<string> ReloadIconNames(bool followsObject)
        {
            iconNames.Clear();

            if (followsObject) {
                //menu follows object
                if (Options.pauseOneMenu?.Value != false)
                    iconNames.Add(Pause.IsObjectPaused(menu.followChunk?.owner) ? "mousedragPlay" : "mousedragPause");
                if (Options.stunOneMenu?.Value != false)
                    iconNames.Add(Stun.IsObjectStunned(menu.followChunk?.owner) ? "mousedragUnstun" : "mousedragStun");
                if (Options.killOneMenu?.Value != false)
                    iconNames.Add("mousedragKill");
                if (Options.reviveOneMenu?.Value != false)
                    iconNames.Add("mousedragRevive");
                if (Options.tameOneMenu?.Value != false)
                    iconNames.Add("mousedragHeart");
                if (Options.clearRelOneMenu?.Value != false)
                    iconNames.Add("mousedragUnheart");
                if (Options.duplicateOneMenu?.Value != false)
                    iconNames.Add("mousedragDuplicate");
                if (Options.clipboardMenu?.Value != false)
                    iconNames.Add("mousedragCut");
                if (Options.destroyOneMenu?.Value != false)
                    iconNames.Add("mousedragDestroy");
                if (Options.tpWaypointMenu?.Value != false)
                    iconNames.Add("mousedragCrosshair");

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
                if (Options.stunRoomMenu?.Value != false)
                    iconNames.Add("mousedragStunAll");
                if (Options.unstunAllMenu?.Value != false)
                    iconNames.Add("mousedragUnstunAll");
                if (Options.stunAllMenu?.Value != false)
                    iconNames.Add(Stun.stunAll ? "mousedragUnstunGlobal" : "mousedragStunGlobal");
                if (Options.killAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragKillCreatures");
                if (Options.reviveAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragReviveCreatures");
                if (Options.tameAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragHeartCreatures");
                if (Options.clearRelAllMenu?.Value != false)
                    iconNames.Add("mousedragUnheartCreatures");
                if (Options.clipboardMenu?.Value != false)
                    iconNames.Add("mousedragPaste");
                if (Options.destroyAllCreaturesMenu?.Value != false)
                    iconNames.Add("mousedragDestroyCreatures");
                if (Options.destroyRoomMenu?.Value != false)
                    iconNames.Add("mousedragDestroyAll");
                if (Options.tpWaypointMenu?.Value != false)
                    iconNames.Add("mousedragCrosshair");
            }

            return iconNames;
        }


        public static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            //if editing in sandbox, disable open menu with right mouse button
            bool inSandboxAndEditing = (game.GetArenaGameSession as SandboxGameSession)?.overlay?.playMode == false;

            if (RadialMenu.menuButtonPressed(noRMB: inSandboxAndEditing))
                shouldOpen = true;
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
                Plugin.Logger.LogError("LoadSprites exception: " + ex.ToString());
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("LoadSprites called");
        }
    }
}
