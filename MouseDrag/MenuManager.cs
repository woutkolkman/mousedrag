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
                    case "mousedragPlay":       Tools.TogglePauseObject(menu.followChunk?.owner); break;
                    case "mousedragKill":       Tools.KillCreature(game, menu.followChunk?.owner); break;
                    case "mousedragRevive":     Tools.ReviveCreature(menu.followChunk?.owner); break;
                    case "mousedragHeart":      Tools.TameCreature(game, menu.followChunk?.owner); break;
                    case "mousedragDuplicate":  Tools.DuplicateObject(menu.followChunk?.owner); break;
                    case "mousedragDelete":     Tools.DeleteObject(menu.followChunk?.owner); break;
                }

            } else {
                //menu on background
                switch (spriteName)
                {
                    case "mousedragPauseCreatures":     Tools.PauseObjects(game.cameras[0]?.room, true); break;
                    case "mousedragPlayAll":            Tools.UnpauseAll(); break;
                    case "mousedragKillCreatures":      Tools.KillCreatures(game, game.cameras[0]?.room); break;
                    case "mousedragReviveCreatures":    Tools.ReviveCreatures(game.cameras[0]?.room); break;
                    case "mousedragDeleteCreatures":    Tools.DeleteObjects(game.cameras[0]?.room, true); break;
                    case "mousedragDeleteAll":          Tools.DeleteObjects(game.cameras[0]?.room, false); break;
                }
            }
        }


        //add-on mods need to hook the ReloadIconNames() function, and insert their sprite names in iconNames afterwards
        public static List<string> ReloadIconNames(bool followsObject)
        {
            iconNames.Clear();

            if (followsObject) {
                //menu follows object
                iconNames.Add(Tools.IsObjectPaused(menu.followChunk?.owner) ? "mousedragPlay" : "mousedragPause");
                iconNames.Add("mousedragKill");
                iconNames.Add("mousedragRevive");
                iconNames.Add("mousedragHeart");
                iconNames.Add("mousedragDuplicate");
                iconNames.Add("mousedragDelete");

            } else {
                //menu on background
                iconNames.Add("mousedragPauseCreatures");
                iconNames.Add("mousedragPlayAll");
                iconNames.Add("mousedragKillCreatures");
                iconNames.Add("mousedragReviveCreatures");
                iconNames.Add("mousedragDeleteCreatures");
                iconNames.Add("mousedragDeleteAll");
            }

            return iconNames;
        }


        public static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            //if editing in sandbox, disable open menu with right mouse button
            bool inSandboxAndEditing = (game.GetArenaGameSession as SandboxGameSession)?.overlay?.playMode == false;

            if ((Input.GetMouseButtonDown(1) && Options.menuRMB?.Value == true && !inSandboxAndEditing) || 
                (Options.menuOpen?.Value != null && Input.GetKeyDown(Options.menuOpen.Value)))
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
            Plugin.Logger.LogDebug("LoadSprites called");
        }
    }
}
