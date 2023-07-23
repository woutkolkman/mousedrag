using UnityEngine;

namespace MouseDrag
{
    class Hooks
    {
        public static void Apply()
        {
            //initialize options
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;

            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;

            //at framerate
            On.RainWorldGame.RawUpdate += RainWorldGameRawUpdateHook;
        }


        public static void Unapply()
        {
			//TODO
        }


        //initialize options
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
        }


        //at tickrate
        static void RainWorldGameUpdateHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);

            //only active when dev tools is active
            if (!self.devToolsActive)
                return;

            Tools.DragObject(self);
            Cursor.visible = true; //windows cursor visible
        }


        //at framerate
        static void RainWorldGameRawUpdateHook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (self.GamePaused || self.pauseUpdate || !self.processActive || self.pauseMenu != null)
                return;

            //only active when dev tools is active
            if (!self.devToolsActive)
                return;

            if (Options.deleteKey?.Value != null && Input.GetKeyDown(Options.deleteKey.Value))
                Tools.DeleteObject();
        }
    }
}
