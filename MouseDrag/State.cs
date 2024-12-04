using UnityEngine;

namespace MouseDrag
{
    public static class State
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static Options.CursorVisibilityTypes winCursorVisType = Options.CursorVisibilityTypes.Moved2Seconds;
        public static bool activated = false; //true --> all tools & menu are available
        public static Vector2 prevMousePos = Vector2.zero;
        private static int mouseStationaryCount = 0;
        public static int mouseVisibilityTicks = 80; //2s
        public static bool mouseMovedVisibility, prevMouseMovedVisibility;

        public static bool menuToolsDisabled = false; //another mod disables all menu tools
        public static bool draggingDisabled = false; //another mod disables mouse dragging objects
        public static bool keyBindToolsDisabled = false; //another mod disables all keybind tools


        public static void UpdateActivated(RainWorldGame game)
        {
            //set activated controls, keybind is checked in RainWorldGameRawUpdateHook
            if (activeType == Options.ActivateTypes.DevToolsActive)
                activated = game.devToolsActive;
            if (activeType == Options.ActivateTypes.AlwaysActive)
                activated = true;
            if (activeType == Options.ActivateTypes.SandboxAndSafari)
                if (game.rainWorld?.safariMode == true || game.GetArenaGameSession is SandboxGameSession)
                    activated = true;

            //if sandbox is active, always enable (because mouse dragging is also active)
            activated |= (game.GetArenaGameSession as SandboxGameSession)?.overlay?.mouseDragger != null;

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
                Cursor.visible = true;
            if (winCursorVisType == Options.CursorVisibilityTypes.ForceInvisible)
                if (game.devUI == null) //don't set mouse invisible if dev tools menu is visible, or that menu is unusable
                    Cursor.visible = false;
            if (winCursorVisType == Options.CursorVisibilityTypes.Moved2Seconds)
                if (prevMouseMovedVisibility != mouseMovedVisibility || mouseMovedVisibility)
                    Cursor.visible = mouseMovedVisibility;
        }


        public static void GamePaused()
        {
            //prevent invisible mouse in pause menu, also if ForceInvisible is used while Rain World cursor is also invisible
            if (State.winCursorVisType != Options.CursorVisibilityTypes.NoChanges && 
                (State.winCursorVisType != Options.CursorVisibilityTypes.ForceInvisible || Options.disVnlCursor?.Value != false))
            {
                Cursor.visible = true;
                State.prevMousePos = Vector2.zero;
            }
        }


        public static void GameEnded()
        {
            //prevent invisible mouse in main menu, also if ForceInvisible is used while Rain World cursor is also invisible
            if (State.winCursorVisType != Options.CursorVisibilityTypes.NoChanges && 
                (State.winCursorVisType != Options.CursorVisibilityTypes.ForceInvisible || Options.disVnlCursor?.Value != false))
            {
                Cursor.visible = true;
                State.prevMousePos = Vector2.zero;
            }

            if (Options.deactivateEveryRestart?.Value != false)
                State.activated = false;
        }


        public static void InitEnums() //is called via Options EventHandler
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("State.InitEnums called");

            //read enum settings from config
            if (Options.activateType?.Value != null)
                foreach (Options.ActivateTypes val in System.Enum.GetValues(typeof(Options.ActivateTypes)))
                    if (System.String.Equals(Options.activateType.Value, val.ToString()))
                        State.activeType = val;
            if (Options.winCursorVisType?.Value != null)
                foreach (Options.CursorVisibilityTypes val in System.Enum.GetValues(typeof(Options.CursorVisibilityTypes)))
                    if (System.String.Equals(Options.winCursorVisType.Value, val.ToString()))
                        State.winCursorVisType = val;
        }
    }
}
