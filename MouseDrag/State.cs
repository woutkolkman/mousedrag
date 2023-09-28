using System;
using UnityEngine;

namespace MouseDrag
{
    public static class State
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static bool activated = false; //true --> all tools are available
        private static bool prevActivated = false;


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

            if (activated != prevActivated)
            {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("CheckActivated, activated: " + activated);
                if (!activated && Options.undoMouseVisible?.Value == true)
                    Cursor.visible = false;
            }
            prevActivated = activated;
        }
    }
}
