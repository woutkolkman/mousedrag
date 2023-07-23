using UnityEngine;

namespace MouseDrag
{
    class Hooks
    {
        public static void Apply()
        {
            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;

            //at framerate
            On.RainWorldGame.RawUpdate += RainWorldGameRawUpdateHook;
        }


        public static void Unapply()
        {
			//TODO
        }


        //at tickrate
        static void RainWorldGameUpdateHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            Tools.DragObject(self);
        }


        //at framerate
        static void RainWorldGameRawUpdateHook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (self.GamePaused || self.pauseUpdate || !self.processActive || self.pauseMenu != null)
                return;

            /*if (Options.deleteKey?.Value != null && Input.GetKeyDown(Options.deleteKey.Value))
            {

            }*/
        }
    }
}
