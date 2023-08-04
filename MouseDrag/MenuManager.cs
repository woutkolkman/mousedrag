﻿using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace MouseDrag
{
    public static class MenuManager
    {
        public static RadialMenu menu = null;
        public static bool shouldOpen = false; //signal from RawUpdate to open menu
        public static bool followsObject = false;
        public static bool prevFollowsObject = false;

        //add-on mods can insert strings in this array to add options to the menu
        public static List<string> followIconNames = new List<string>() { "mousedragPause", "mousedragKill", "mousedragRevive", "mousedragDuplicate", "mousedragDelete" };
        public static List<string> generalIconNames = new List<string>() { "mousedragPauseCreatures", "mousedragPlayAll", "mousedragDeleteCreatures", "mousedragDeleteAll" };

        //add-on mods need to hook the Update() function, and do an action when pressedIdx is their ID
        public static int pressedIdx = -1;


        public static void Update(RainWorldGame game)
        {
            bool reloadSlots = false;
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

            pressedIdx = menu.Update(game);

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject && (!prevFollowsObject || reloadSlots)) {
                menu.LoadSlots(followIconNames);
            }
            if (!followsObject && (prevFollowsObject || reloadSlots)) {
                menu.LoadSlots(generalIconNames);
            }
            prevFollowsObject = followsObject;

            //run commands
            if (followsObject) {
                switch (pressedIdx)
                {
                    case 0: Tools.TogglePauseObject(menu.followChunk?.owner); break; //pauseOneKey
                    case 1: Tools.KillCreature(menu.followChunk?.owner); break; //killOneKey
                    case 2: Tools.ReviveCreature(menu.followChunk?.owner); break; //reviveOneKey
                    case 3: Tools.DuplicateObject(menu.followChunk?.owner); break; //duplicateOneKey
                    case 4: Tools.DeleteObject(menu.followChunk?.owner); break; //deleteOneKey
                }
            } else {
                switch (pressedIdx)
                {
                    case 0: Tools.PauseObjects(game.cameras[0]?.room, true); break; //pauseRoomCreaturesKey
                    case 1: Tools.UnpauseAll(); break; //unpauseAllKey
                    case 2: Tools.DeleteObjects(game.cameras[0]?.room, true); break; //deleteAllCreaturesKey
                    case 3: Tools.DeleteObjects(game.cameras[0]?.room, false); break; //deleteAllObjectsKey
                }
            }
        }


        public static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            if ((Input.GetMouseButtonDown(1) && Options.menuRMB?.Value == true) || 
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