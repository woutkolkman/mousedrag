using UnityEngine;

namespace MouseDrag
{
    public static class State
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static Options.CursorVisibilityTypes rwCursorVisType = Options.CursorVisibilityTypes.NoChanges;
        public static Options.CursorVisibilityTypes winCursorVisType = Options.CursorVisibilityTypes.Moved2Seconds;
        public static bool activated = false; //true --> all tools are available
        public static Vector2 prevMousePos = Vector2.zero;
        private static int mouseStationaryCount = 0;
        public static int mouseVisibilityTicks = 80; //2s


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

            if (winCursorVisType == Options.CursorVisibilityTypes.ForceVisible) {
                //forced visibility
                Cursor.visible = true;

            } else if (winCursorVisType == Options.CursorVisibilityTypes.ForceInvisible) {
                //forced visibility
                Cursor.visible = false;

            } else if (winCursorVisType == Options.CursorVisibilityTypes.Moved2Seconds) {
                //windows cursor only visible if mouse moved
                if (prevMousePos != (Vector2)Futile.mousePosition) {
                    mouseStationaryCount = 0;
                    Cursor.visible = true;
                }
                prevMousePos = Futile.mousePosition;
                if (mouseStationaryCount <= mouseVisibilityTicks)
                    mouseStationaryCount++;
                if (mouseStationaryCount == mouseVisibilityTicks)
                    Cursor.visible = false;
            }
        }


        public static void GameStarted()
        {
            if (Options.deactivateEveryRestart?.Value != false)
                State.activated = false;

            //read enum settings from config
            if (Options.activateType?.Value != null)
                foreach (Options.ActivateTypes val in System.Enum.GetValues(typeof(Options.ActivateTypes)))
                    if (System.String.Equals(Options.activateType.Value, val.ToString()))
                        State.activeType = val;
            if (Options.rwCursorVisType?.Value != null)
                foreach (Options.CursorVisibilityTypes val in System.Enum.GetValues(typeof(Options.CursorVisibilityTypes)))
                    if (System.String.Equals(Options.rwCursorVisType.Value, val.ToString()))
                        State.rwCursorVisType = val;
            if (Options.winCursorVisType?.Value != null)
                foreach (Options.CursorVisibilityTypes val in System.Enum.GetValues(typeof(Options.CursorVisibilityTypes)))
                    if (System.String.Equals(Options.winCursorVisType.Value, val.ToString()))
                        State.winCursorVisType = val;
        }


        public static void GameEnded()
        {
            //prevent invisible mouse in main menu
            if (State.winCursorVisType != Options.CursorVisibilityTypes.NoChanges &&
                State.winCursorVisType != Options.CursorVisibilityTypes.ForceInvisible) {
                Cursor.visible = true;
                State.prevMousePos = Vector2.zero;
            }
        }
    }
}
