using UnityEngine;

namespace FreeCam
{
    public static class Cursor
    {
        public static Options.CursorVisibilityTypes winCursorVisType = Options.CursorVisibilityTypes.NoChanges;
        public static Vector2 prevMousePos = Vector2.zero;
        private static int mouseStationaryCount = 0;
        public static int mouseVisibilityTicks = 80; //2s
        public static bool mouseMovedVisibility, prevMouseMovedVisibility;


        public static void Update(RainWorldGame game)
        {
            //visibility based on movement of mouse, if mouse moved, cursor is visible for a short time
            prevMouseMovedVisibility = mouseMovedVisibility;
            if (prevMousePos != (Vector2)Futile.mousePosition) {
                mouseStationaryCount = 0;
                mouseMovedVisibility = true;
            }
            prevMousePos = Futile.mousePosition;
            if (mouseStationaryCount <= mouseVisibilityTicks)
                mouseStationaryCount++;
            if (mouseStationaryCount == mouseVisibilityTicks)
                mouseMovedVisibility = false;

            //actually apply visibility
            if (winCursorVisType == Options.CursorVisibilityTypes.ForceVisible)
                UnityEngine.Cursor.visible = true;
            if (winCursorVisType == Options.CursorVisibilityTypes.ForceInvisible)
                if (game.devUI == null) //don't set mouse invisible if dev tools menu is visible, or that menu is unusable
                    UnityEngine.Cursor.visible = false;
            if (winCursorVisType == Options.CursorVisibilityTypes.Moved2Seconds)
                if (prevMouseMovedVisibility != mouseMovedVisibility || mouseMovedVisibility)
                    UnityEngine.Cursor.visible = mouseMovedVisibility;
        }


        public static void Init()
        {
            //read enum settings from config
            if (Options.winCursorVisType?.Value != null)
                foreach (Options.CursorVisibilityTypes val in System.Enum.GetValues(typeof(Options.CursorVisibilityTypes)))
                    if (System.String.Equals(Options.winCursorVisType.Value, val.ToString()))
                        Cursor.winCursorVisType = val;
        }


        public static void Deinit()
        {
            //prevent invisible mouse in main menu, also if ForceInvisible is used while Rain World cursor is also invisible
            if (winCursorVisType != Options.CursorVisibilityTypes.NoChanges) {
                UnityEngine.Cursor.visible = true;
                prevMousePos = Vector2.zero;
            }
        }


        //get mouse position hovering over any camera
        public static Vector2 MousePos(RainWorldGame game)
        {
            RoomCamera rcam = MouseCamera(game, out Vector2 offset);
            Vector2 pos = (Vector2)Futile.mousePosition - offset + rcam?.pos ?? new Vector2();
            if (!Integration.sBCameraScrollEnabled)
                return pos;
            try
            {
                pos += Integration.SBCameraScrollExtraOffset(rcam, Futile.mousePosition, out _);
            }
            catch
            {
                Plugin.Logger.LogError("Tools.MousePos exception while reading SBCameraScroll, integration is now disabled");
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
            try
            {
                return Integration.SplitScreenCoopCam(game, out offset);
            }
            catch
            {
                Plugin.Logger.LogError("Tools.MouseCamera exception while reading SplitScreen Co-op, integration is now disabled");
                Integration.splitScreenCoopEnabled = false;
                throw; //throw original exception while preserving stack trace
            }
        }
    }
}
