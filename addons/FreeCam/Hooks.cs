using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace FreeCam
{
    public static class Hooks
    {
        public static void Apply()
        {
            //initialize options & load sprites
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;

            //after mods initialized
            On.RainWorld.PostModsInit += RainWorldPostModsInitHook;

            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;

            //at framerate
            On.RainWorldGame.RawUpdate += RainWorldGameRawUpdateHook;

            //at new game
            On.RainWorldGame.ctor += RainWorldGameCtorHook;

            //at hibernate etc.
            On.RainWorldGame.ShutDownProcess += RainWorldGameShutDownProcessHook;

            //freecam disables a RoomCamera function
            On.RoomCamera.GetCameraBestIndex += RoomCameraGetCameraBestIndexHook;
        }


        public static void Unapply()
        {
            On.RainWorld.OnModsInit -= RainWorldOnModsInitHook;
            On.RainWorld.PostModsInit -= RainWorldPostModsInitHook;
            On.RainWorldGame.Update -= RainWorldGameUpdateHook;
            On.RainWorldGame.RawUpdate -= RainWorldGameRawUpdateHook;
            On.RainWorldGame.ctor -= RainWorldGameCtorHook;
            On.RainWorldGame.ShutDownProcess -= RainWorldGameShutDownProcessHook;
            On.RoomCamera.GetCameraBestIndex -= RoomCameraGetCameraBestIndexHook;
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            //hook gets called (for this mod) only when not using Rain Reloader
            Plugin.Logger.LogDebug("RainWorldOnModsInitHook, first time initializing options interface");
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
        }


        //after mods initialized
        static void RainWorldPostModsInitHook(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            //hook gets called (for this mod) only when not using Rain Reloader
            Integration.RefreshActiveMods();
            Integration.Hooks.Apply();
        }


        //at tickrate
        static void RainWorldGameUpdateHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            FreeCam.Update(self);
            Cursor.Update(self);
        }


        //at framerate
        static void RainWorldGameRawUpdateHook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);

            if (self.GamePaused || self.pauseUpdate || !self.processActive)
                return;

            FreeCam.RawUpdate(self);

            if (Options.activateKey?.Value != null && Input.GetKeyDown(Options.activateKey.Value))
                FreeCam.ToggleFreeCam(self);
        }


        //at new game
        static void RainWorldGameCtorHook(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            FreeCam.enabled = false;
            Cursor.Init();
        }


        //at hibernate etc.
        static void RainWorldGameShutDownProcessHook(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);
            FreeCam.enabled = false;
            Cursor.Deinit();
        }


        //freecam disables a RoomCamera function
        public static void RoomCameraGetCameraBestIndexHook(On.RoomCamera.orig_GetCameraBestIndex orig, RoomCamera self)
        {
            if (!FreeCam.enabled)
                orig(self);
        }
    }
}
