using UnityEngine;

namespace FreeCam
{
    public static class Tools
    {
        //get mouse position hovering over any camera
        public static Vector2 MousePos(RainWorldGame game)
        {
            RoomCamera rcam = MouseCamera(game, out Vector2 offset);
            Vector2 pos = (Vector2)Futile.mousePosition - offset + rcam?.pos ?? new Vector2();
            if (!Integration.sBCameraScrollEnabled)
                return pos;
            try {
                pos += Integration.SBCameraScrollExtraOffset(rcam, Futile.mousePosition, out _);
            } catch {
                Plugin.Logger.LogError("Drag.MousePos exception while reading SBCameraScroll, integration is now disabled");
                Integration.sBCameraScrollEnabled = false;
                throw; //throw original exception while preserving stack trace
            }
            return pos;
        }


        //get camera where mouse is currently located
        public static RoomCamera MouseCamera(RainWorldGame game) { return MouseCamera(game, out _); }
        public static RoomCamera MouseCamera(RainWorldGame game, out Vector2 offset)
        {
            offset = Vector2.zero;
            if (!(game?.cameras?.Length > 0))
                return null;
            if (!Integration.splitScreenCoopEnabled)
                return game.cameras[0];
            try {
                return Integration.SplitScreenCoopCam(game, out offset);
            } catch {
                Plugin.Logger.LogError("Drag.MouseCamera exception while reading SplitScreen Co-op, integration is now disabled");
                Integration.splitScreenCoopEnabled = false;
                throw; //throw original exception while preserving stack trace
            }
        }
    }
}
