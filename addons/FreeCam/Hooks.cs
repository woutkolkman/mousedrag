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

            //at new game
            On.RainWorldGame.ctor += RainWorldGameCtorHook;

            //at pause game
            On.Menu.PauseMenu.ctor += MenuPauseMenuCtorHook;

            //at hibernate etc.
            On.RainWorldGame.ShutDownProcess += RainWorldGameShutDownProcessHook;
        }


        public static void Unapply()
        {
            On.RainWorld.OnModsInit -= RainWorldOnModsInitHook;
            On.RainWorld.PostModsInit -= RainWorldPostModsInitHook;
            On.RainWorldGame.ctor -= RainWorldGameCtorHook;
            On.Menu.PauseMenu.ctor -= MenuPauseMenuCtorHook;
            On.RainWorldGame.ShutDownProcess -= RainWorldGameShutDownProcessHook;
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            //hook gets called (for this mod) only when not using Rain Reloader

            orig(self);

            Plugin.Logger.LogDebug("RainWorldOnModsInitHook, first time initializing options interface");
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
        }


        //after mods initialized
        static void RainWorldPostModsInitHook(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            //hook gets called (for this mod) only when not using Rain Reloader

            orig(self);

            Integration.Hooks.Apply();
        }


        //at new game
        static void RainWorldGameCtorHook(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            FreeCamManager.Init(self);
            Cursor.Init();
        }


        //at pause game
        static void MenuPauseMenuCtorHook(On.Menu.PauseMenu.orig_ctor orig, Menu.PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig(self, manager, game);
            Cursor.NoUpdateShow();
        }


        //at hibernate etc.
        static void RainWorldGameShutDownProcessHook(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);
            FreeCamManager.Deinit();
            Cursor.NoUpdateShow();
        }
    }
}
