using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    public static class MenuStarter
    {
        public static RadialMenu menu = null;
        public static bool shouldOpen = false; //signal from RawUpdate to open menu
        public static bool followsObject = false;
        public static bool prevFollowsObject = false;
        public static string[] followIconNames = { "Kill_Slugcat", "Kill_Jetfish", "Kill_Slugcat", "Kill_Slugcat", "Kill_Slugcat", "Kill_Slugcat" };
        public static string[] generalIconNames = { "Kill_Slugcat", "Kill_Slugcat", "Kill_Slugcat", "Kill_Slugcat", "Kill_Slugcat" };


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

            int pressedIdx = menu.Update(game);

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
                    case 1: {
                            Tools.pauseAllCreatures = !Tools.pauseAllCreatures; //pauseAllCreaturesKey
                            Plugin.Logger.LogDebug("pauseAllCreatures: " + Tools.pauseAllCreatures);
                            break;
                        };
                    case 2: Tools.UnpauseAll(); break; //unpauseAllKey
                    case 3: Tools.DeleteObjects(game.cameras[0]?.room, true); break; //deleteAllCreaturesKey
                    case 4: Tools.DeleteObjects(game.cameras[0]?.room, false); break; //deleteAllObjectsKey
                }
            }
        }


        public static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            if (Input.GetMouseButtonDown(1))
                shouldOpen = true;
        }
    }
}
