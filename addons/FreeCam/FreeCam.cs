using RWCustom;
using UnityEngine;

namespace FreeCam
{
    public static class FreeCam
    {
        public static bool enabled = false;
        private static bool mousePressed = false; //LMB presseddown signal from RawUpdate for Update
        private static int screenChangeStopTicks = 0;
        private static Room loadingRoom = null; //room will be moved to when it is fully loaded
        private static RoomCamera loadingCam = null; //this camera will move to loadingRoom
        public static bool selectButtonDown() => (
            (Input.GetMouseButtonDown(0) && Options.selectLMB?.Value == true) ||
            (Input.GetMouseButtonDown(2) && Options.selectMMB?.Value == true) ||
            (Options.select?.Value != null && Input.GetKeyDown(Options.select.Value))
        );


        public static void ToggleFreeCam(RainWorldGame game)
        {
            enabled = !enabled;
            if (enabled)
                return;

            if (game?.cameras == null)
                return;

            //move all cameras back to their followAbstractCreature rooms
            for (int i = 0; i < game.cameras.Length; i++) {
                AbstractRoom acRoom = game.cameras[i]?.followAbstractCreature?.Room;
                if (acRoom?.world == null || game.cameras[i].room?.abstractRoom == acRoom)
                    continue;
                if (acRoom.world != game.cameras[i].room?.abstractRoom?.world)
                    continue;
                if (acRoom.realizedRoom == null)
                    acRoom.world.ActivateRoom(acRoom);
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.ToggleFreeCam, move camera back to followAbstractCreature room");
                if (acRoom.realizedRoom == null) {
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogDebug("FreeCam.ToggleFreeCam, realizedRoom still null after ActivateRoom call");
                } else {
                    //TODO room might not be ready? but player was/is in it, so it is always ready?
                    game.cameras[i].MoveCamera(acRoom.realizedRoom, 0); //camPos gets auto corrected after moving to room
                }
            }
        }


        public static void Update(RainWorldGame game)
        {
            if (!enabled || game == null)
                return;

            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                return;

            RoomCamera rcam = Tools.MouseCamera(game);
            if (rcam == null)
                return;

            ScreenChanger(game, rcam);
            PipeSelector(game, rcam);
            RoomChanger();
        }


        public static void RawUpdate(RainWorldGame game)
        {
            if (!enabled)
                return;

            if (selectButtonDown())
                mousePressed = true;
        }


        //move to other screens when mouse is at edge of screen
        private static void ScreenChanger(RainWorldGame game, RoomCamera rcam)
        {
            if (rcam?.room == null || game == null)
                return;

            Vector2 mouse = Futile.mousePosition;
            float minDistFromEdge = 50f;

            Vector2 targetDir = new Vector2();
            if (mouse.x <= minDistFromEdge) //left
                targetDir.x = -1.0f;
            if (mouse.x >= rcam.sSize.x - minDistFromEdge) //right
                targetDir.x = 1.0f;
            if (mouse.y <= minDistFromEdge) //bottom
                targetDir.y = -1.0f;
            if (mouse.y >= rcam.sSize.y - minDistFromEdge) //top
                targetDir.y = 1.0f;

            //lean camera feedback for user
            rcam.leanPos = targetDir;

            if (targetDir == Vector2.zero)
                screenChangeStopTicks = 40;
            if (screenChangeStopTicks > 0) {
                screenChangeStopTicks--;
                return;
            }
            screenChangeStopTicks = 20;

            //estimate next camera position
            Vector2 estCamPos = new Vector2(
                rcam.pos.x + (targetDir.x * 0.75f * rcam.sSize.x), 
                rcam.pos.y + (targetDir.y * 1f * rcam.sSize.y)
            );

            //find closest camera position based on estimated camera position
            int camPos = -1;
            float closestDist = Vector2.Distance(rcam.pos, estCamPos);
            for (int i = 0; i < rcam.room.cameraPositions?.Length; i++) {
                float curDist = Vector2.Distance(rcam.room.cameraPositions[i], estCamPos);
                if (curDist + 50f < closestDist) {
                    closestDist = curDist;
                    camPos = i;
                }
            }
            if (camPos < 0) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.ScreenChanger, current camera is closest to target position");
                return;
            }

            //move camera to new position
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCam.ScreenChanger, move camera to camPos " + camPos);
            rcam.MoveCamera(camPos);
        }


        //check if shortcuts are pressed
        private static void PipeSelector(RainWorldGame game, RoomCamera rcam)
        {
            //mouse is used to select a shortcut
            if (!mousePressed)
                return;
            mousePressed = false;

            if (rcam?.room?.abstractRoom == null || 
                rcam.room.exitAndDenIndex == null || 
                rcam.room.world == null || 
                game == null)
                return;
            Room room = rcam.room;
            Vector2 mousePos = Tools.MousePos(game);

            //get shortcut which was clicked on
            ShortcutData scd = room.shortcutData(mousePos);
            if (!scd.ToNode)
                return;
            IntVector2 destTile = scd.destinationCoord.Tile;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCam.PipeSelector, shortcut selected to tile " + destTile.ToString() + ", node " + scd.destNode);

            //still loading another room
            if (loadingRoom != null || loadingCam != null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.PipeSelector, canceling, a camera is still moving to another room");
                return;
            }

            //get room which shortcut leads to, if any
            int exitOrDenIndex = room.exitAndDenIndex.IndexfOf(destTile);
            AbstractRoom destAR = null;
            if (exitOrDenIndex > -1 && exitOrDenIndex < room.abstractRoom.connections?.Length)
                destAR = room.world.GetAbstractRoom(room.abstractRoom.connections[exitOrDenIndex]);
            if (destAR == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.PipeSelector, invalid destination room");
                return;
            }

            //load room if it is not realized yet
            if (destAR.realizedRoom == null) {
                room.world.ActivateRoom(destAR);
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.PipeSelector, realizedRoom null, activating");
            }

            //move to room once it is ready
            if (destAR.realizedRoom == null)
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.PipeSelector, realizedRoom still null after ActivateRoom call");
            loadingRoom = destAR.realizedRoom;
            loadingCam = rcam;
        }


        //move camera to the room selected by PipeSelector
        private static void RoomChanger() {
            if (loadingRoom == null || loadingCam == null) {
                loadingRoom = null;
                loadingCam = null;
                return;
            }

            //room is still loading
            if (!loadingRoom.fullyLoaded || !loadingRoom.ReadyForPlayer)
                return;

            //defensive programming checks
            if (loadingCam.room?.world == null || 
                loadingCam.room?.abstractRoom == null || 
                loadingRoom.abstractRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.RoomChanger, invalid loadingRoom or loadingCam");
                loadingRoom = null;
                loadingCam = null;
                return;
            }

            //get node in other room leading to this room
            WorldCoordinate wcDestNode = loadingCam.room.world.NodeInALeadingToB(loadingRoom.abstractRoom, loadingCam.room.abstractRoom);
            int destNode = 0; //nodes count from 0 up
            if (wcDestNode.NodeDefined)
                destNode = wcDestNode.abstractNode;

            //move camera to this room at this node/cam position
            int camPos = -1;
            try {
                camPos = loadingRoom.CameraViewingNode(destNode);
            } catch (System.Exception ex) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.RoomChanger, CameraViewingNode call failed: " + ex?.ToString());
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCam.RoomChanger, calling MoveCamera to room " + loadingRoom.abstractRoom.name + " camPos " + camPos);
            loadingCam.MoveCamera(loadingRoom, camPos);

            loadingRoom = null;
            loadingCam = null;
        }
    }
}
