using System;
using UnityEngine;

namespace MouseDrag
{
    static class State
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static bool activated = false; //true --> all tools are available
        private static bool prevActivated = false, prevPaused = true;


        public static void UpdateActivated(RainWorldGame game)
        {
            bool paused = (game.GamePaused || game.pauseUpdate || !game.processActive);

            //read activeType from config when game is unpaused
            if (!paused && prevPaused && Options.activateType?.Value != null)
            {
                foreach (Options.ActivateTypes val in Enum.GetValues(typeof(Options.ActivateTypes)))
                    if (String.Equals(Options.activateType.Value, val.ToString()))
                        activeType = val;
                Plugin.Logger.LogDebug("CheckActivated, activeType: " + activeType.ToString());
            }
            prevPaused = paused;

            //set activated controls, keybind is checked in RainWorldGameRawUpdateHook
            if (activeType == Options.ActivateTypes.DevToolsActive)
                activated = game.devToolsActive;
            if (activeType == Options.ActivateTypes.AlwaysActive)
                activated = true;

            //if sandbox is active, always enable (because mouse drag is also active)
            activated |= (game.GetArenaGameSession as SandboxGameSession)?.overlay?.mouseDragger != null;

            if (activated != prevActivated)
            {
                Plugin.Logger.LogDebug("CheckActivated, activated: " + activated);
                if (!activated && Options.undoMouseVisible?.Value == true)
                    Cursor.visible = false;
            }
            prevActivated = activated;
        }
    }
}
