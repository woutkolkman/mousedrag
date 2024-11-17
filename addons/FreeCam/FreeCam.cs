using RWCustom;
using UnityEngine;

namespace FreeCam
{
    public class FreeCam
    {
        public bool enabled = false;
        public bool mousePressed = false; //LMB presseddown signal from RawUpdate for Update
        private int screenChangeStopTicks = 0;
        private Room loadingRoom = null; //room will be moved to when it is fully loaded
        private RoomCamera rcam;
        public Vector2? sBCameraScrollNewPos = null; //used to correctly position screen in new room


        public FreeCam(RoomCamera rcam)
        {
            if (rcam == null)
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("FreeCam ctor, RoomCamera value null");
            this.rcam = rcam;
        }


        public void Toggle()
        {
            enabled = !enabled;
            if (enabled)
                return;

            //move this camera back to the followAbstractCreature room
            AbstractRoom acRoom = rcam?.followAbstractCreature?.Room;
            if (acRoom?.world == null || rcam.room?.abstractRoom == acRoom)
                return;
            if (acRoom.world != rcam.room?.abstractRoom?.world)
                return;
            if (acRoom.realizedRoom == null)
                acRoom.world.ActivateRoom(acRoom);
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCam.Toggle, move camera back to followAbstractCreature room");
            if (acRoom.realizedRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.Toggle, realizedRoom still null after ActivateRoom call");
            } else {
                //TODO, room might not be ready? but player was/is in it, so it is always ready?
                rcam.MoveCamera(acRoom.realizedRoom, 0); //camPos gets auto corrected after moving to room
            }
        }


        public void Update(RainWorldGame game)
        {
            if (!enabled || game == null)
                return;

            if (Integration.sBCameraScrollEnabled) {
                ScrollScreenChanger(game);
            } else {
                DefaultScreenChanger(game);
            }
            PipeSelector(game);
            RoomChanger();
        }


        //move screen using SBCameraScroll
        private void ScrollScreenChanger(RainWorldGame game)
        {
            if (rcam?.room == null || game == null)
                return;

            if (rcam.voidSeaMode) {
                VoidSeaMode();
                return;
            }

            float minDistFromEdge = 120f;
            float speed = 25f;
            Vector2 movement = MouseDirectionMethodB(minDistFromEdge) * speed;

            //mouse not near edge, no movement required
            if (movement == Vector2.zero)
                return;

            //actually move screen
            try {
                Integration.SBCameraScrollMoveScreen(rcam, movement);
            } catch {
                Plugin.Logger.LogError("FreeCam.ScrollScreenChanger exception while writing SBCameraScroll, integration is now disabled");
                Integration.sBCameraScrollEnabled = false;
                throw; //throw original exception while preserving stack trace
            }
        }


        //in void sea mode there are no predefined camera positions
        private void VoidSeaMode()
        {
            float minDistFromEdge = 50f;
            float speed = 60f;
            float maxY = 240f - rcam.sSize.y; //magic number from game
            Vector2 newPos = rcam.pos + MouseDirectionMethodB(minDistFromEdge) * speed;
            newPos.y = Mathf.Min(newPos.y, maxY);
            rcam.pos = newPos;
            //TODO, getting the camera into void sea mode without having the player there first is not implemented yet
            //TODO, offset maxY maybe not correct in some SplitScreen Co-op SplitModes, but you can't really co-op down there anyway so why bother
        }


        //move to other screens when mouse is at edge of screen
        private void DefaultScreenChanger(RainWorldGame game)
        {
            if (rcam?.room == null || game == null)
                return;

            float minDistFromEdge = 50f;
            Vector2 targetDir = MouseDirectionMethodC(minDistFromEdge);

            if (rcam.voidSeaMode) {
                VoidSeaMode();
                return;
            }

            //before changing position, scroll camera using SplitScreen Co-op
            if (Integration.splitScreenCoopEnabled && !Integration.sBCameraScrollEnabled) {
                float speed = 15f;
                Vector2 tryNewPos = rcam.pos + (MouseDirectionMethodA(minDistFromEdge) * speed);
                bool inASplitScreenMode;
                try {
                    inASplitScreenMode = Integration.SplitScreenCoopCheckBorders(rcam, ref tryNewPos);
                } catch {
                    Plugin.Logger.LogError("FreeCam.DefaultScreenChanger exception while reading SplitScreen Co-op, integration is now disabled");
                    Integration.splitScreenCoopEnabled = false;
                    throw; //throw original exception while preserving stack trace
                }
                if (inASplitScreenMode) {
                    if (rcam.pos != tryNewPos)
                        screenChangeStopTicks = 20;
                    rcam.pos = tryNewPos;
                }
                //TODO, after changing the screen position, the screen is auto-centered
                //TODO, maybe change the scroll to start nearby previous position?
                //TODO, that way the screen scrolls more naturally, or just enable SBCameraScroll
            }

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
                    Plugin.Logger.LogDebug("FreeCam.DefaultScreenChanger, current camera is closest to target position");
                return;
            }

            //move camera to new position
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCam.DefaultScreenChanger, move camera to camPos " + camPos);
            rcam.MoveCamera(camPos);
        }


        //check if shortcuts are pressed
        private void PipeSelector(RainWorldGame game)
        {
            //mouse is used to select a shortcut
            if (!mousePressed)
                return;
            mousePressed = false;

            //defensive programming checks
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
            if (loadingRoom != null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.PipeSelector, canceling, the RoomCamera is still moving to another room");
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
            //NOTE, rooms activated/loaded via this method stay loaded unless you visit the room afterwards with a player
            //NOTE, this is because RoomRealizer uses the player position to track and load/unload rooms, not the camera position
            //NOTE, if using RoomRealizer anyway, the room the camera would be in would randomly unload?
            //TODO, maybe manage own activated rooms? unless added to a RoomRealizer.realizedRooms?

            //move to room once it is ready
            if (destAR.realizedRoom == null)
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.PipeSelector, realizedRoom still null after ActivateRoom call");
            loadingRoom = destAR.realizedRoom;
        }


        //move camera to the room selected by PipeSelector
        private void RoomChanger() {
            if (loadingRoom == null)
                return;

            //room is still loading
            if (!loadingRoom.fullyLoaded || !loadingRoom.ReadyForPlayer)
                return;

            //defensive programming checks
            if (rcam?.room?.world == null || 
                rcam.room?.abstractRoom == null || 
                loadingRoom.abstractRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("FreeCam.RoomChanger, invalid loadingRoom or RoomCamera location");
                loadingRoom = null;
                return;
            }

            //get node in other room leading to this room
            WorldCoordinate wcDestNode = rcam.room.world.NodeInALeadingToB(loadingRoom.abstractRoom, rcam.room.abstractRoom);
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

            //NOTE, SBCameraScroll ignores the camera position parameter passed to MoveCamera
            //change camera position in new room after SBCameraScroll in SBCameraScrollRoomCameraMod_RoomCamera_ApplyPositionChange_RuntimeDetour
            if (Integration.sBCameraScrollEnabled)
                if (camPos >= 0 && camPos < loadingRoom.cameraPositions?.Length)
                    sBCameraScrollNewPos = loadingRoom.cameraPositions[camPos];

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("FreeCam.RoomChanger, calling MoveCamera to room " + loadingRoom.abstractRoom.name + " camPos " + camPos);
            rcam.MoveCamera(loadingRoom, camPos);

            loadingRoom = null;
        }


        //easier to move screen in a straight line
        private Vector2 MouseDirectionMethodA(float minDistFromEdge)
        {
            Vector2 mouse = Futile.mousePosition;
            Vector2 distFromEdge = new Vector2(float.MaxValue, float.MaxValue);

            if (mouse.x < rcam.sSize.x / 2f) {
                //mouse on left of screen
                if (mouse.x >= 0f)
                    distFromEdge.x = -mouse.x;
            } else {
                //mouse on right of screen
                if (mouse.x <= rcam.sSize.x)
                    distFromEdge.x = Mathf.Abs(mouse.x - rcam.sSize.x);
            }

            if (mouse.y < rcam.sSize.y / 2f) {
                //mouse on bottom of screen
                if (mouse.y >= 0f)
                    distFromEdge.y = -mouse.y;
            } else {
                //mouse on top of screen
                if (mouse.y <= rcam.sSize.y)
                    distFromEdge.y = Mathf.Abs(mouse.y - rcam.sSize.y);
            }

            //mouse not within screen
            if (distFromEdge.x == float.MaxValue || distFromEdge.y == float.MaxValue)
                return Vector2.zero;

            //calculate movement speed based on mouse distance from edge
            Vector2 direction = new Vector2(
                distFromEdge.x > 0f ? -Mathf.Min(0f, distFromEdge.x - minDistFromEdge) : -Mathf.Max(0f, distFromEdge.x + minDistFromEdge),
                distFromEdge.y > 0f ? -Mathf.Min(0f, distFromEdge.y - minDistFromEdge) : -Mathf.Max(0f, distFromEdge.y + minDistFromEdge)
            ) / minDistFromEdge;

            return direction;
        }


        //easier to move screen at an angle
        private Vector2 MouseDirectionMethodB(float minDistFromEdge)
        {
            Vector2 mouse = Futile.mousePosition;
            Vector2 centerOfScreen = rcam.sSize / 2f;

            Vector2 direction = (mouse - centerOfScreen).normalized;
            float distFromEdgeX = Mathf.Abs(Mathf.Abs(mouse.x - centerOfScreen.x) - centerOfScreen.x);
            float distFromEdgeY = Mathf.Abs(Mathf.Abs(mouse.y - centerOfScreen.y) - centerOfScreen.y);
            distFromEdgeX = -distFromEdgeX + minDistFromEdge;
            distFromEdgeY = -distFromEdgeY + minDistFromEdge;
            float speed = Mathf.Max(distFromEdgeX, distFromEdgeY) / minDistFromEdge;
            speed = Mathf.Max(speed, 0f);

            return direction * speed;
        }


        //digital direction, no gradient
        private Vector2 MouseDirectionMethodC(float minDistFromEdge)
        {
            Vector2 mouse = Futile.mousePosition;
            Vector2 ret = new Vector2();

            if (mouse.x <= minDistFromEdge) //left
                ret.x = -1.0f;
            if (mouse.x >= rcam.sSize.x - minDistFromEdge) //right
                ret.x = 1.0f;
            if (mouse.y <= minDistFromEdge) //bottom
                ret.y = -1.0f;
            if (mouse.y >= rcam.sSize.y - minDistFromEdge) //top
                ret.y = 1.0f;

            return ret;
        }
    }
}
