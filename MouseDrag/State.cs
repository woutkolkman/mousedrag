using UnityEngine;

namespace MouseDrag
{
    public static class State
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static bool activated = false; //true --> all tools are available
        private static Vector2 prevMousePos = Vector2.zero;
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
                if (game.rainWorld?.safariMode == true)
                    activated = true;

            //if sandbox is active, always enable (because mouse drag is also active)
            activated |= (game.GetArenaGameSession as SandboxGameSession)?.overlay?.mouseDragger != null;

            if (Options.manageMouseVisibility?.Value != true)
                return;

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

            //forced visibility
            if (Options.forceMouseVisibility?.Value == true)
                Cursor.visible = true;
        }
    }
}
