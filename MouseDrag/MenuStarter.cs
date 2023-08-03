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


        public static void Update(RainWorldGame game)
        {
            if (shouldOpen && menu == null)
                menu = new RadialMenu(game);
            shouldOpen = false;

            if (menu?.closed == true) {
                menu.Destroy();
                menu = null;
            }

            if (menu == null)
                return;

            int pressedIdx = menu.Update(game);

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject && !prevFollowsObject) {
                //TODO switch sprites & reload slots
            }
            if (!followsObject && prevFollowsObject) {
                //TODO switch sprites & reload slots
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
