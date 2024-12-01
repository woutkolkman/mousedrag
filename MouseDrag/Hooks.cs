using MonoMod.RuntimeDetour;
using System.Reflection;

namespace MouseDrag
{
    public static class Hooks
    {
        public static Hook MenuShowCursorHook;


        public static void Apply()
        {
            //initialize options & load sprites
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;

            //after mods initialized
            On.RainWorld.PostModsInit += RainWorldPostModsInitHook;

            //at new game
            On.RainWorldGame.ctor += RainWorldGameCtorHook;

            //at hibernate etc.
            On.RainWorldGame.ShutDownProcess += RainWorldGameShutDownProcessHook;

            //jolly co-op multiplayer safari control
            On.Creature.SafariControlInputUpdate += CreatureSafariControlInputUpdateHook;

            //anti smash-scug-into-wall
            On.RoomCamera.ApplyPositionChange += RoomCameraApplyPositionChangeHook;

            //disable vanilla sandbox mouse dragger
            On.Menu.SandboxOverlay.Initiate += MenuSandboxOverlayInitiateHook;

            //change visibility Rain World cursor
            MenuShowCursorHook = new Hook(
                typeof(Menu.Menu).GetProperty("ShowCursor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                typeof(Hooks).GetMethod("Menu_ShowCursor_get", BindingFlags.Static | BindingFlags.Public)
            );
        }


        public static void Unapply()
        {
            On.RainWorld.OnModsInit -= RainWorldOnModsInitHook;
            On.RainWorld.PostModsInit -= RainWorldPostModsInitHook;
            On.RainWorldGame.ctor -= RainWorldGameCtorHook;
            On.RainWorldGame.ShutDownProcess -= RainWorldGameShutDownProcessHook;
            On.Creature.SafariControlInputUpdate -= CreatureSafariControlInputUpdateHook;
            On.RoomCamera.ApplyPositionChange -= RoomCameraApplyPositionChangeHook;
            On.Menu.SandboxOverlay.Initiate -= MenuSandboxOverlayInitiateHook;
            if (MenuShowCursorHook.IsValid)
                MenuShowCursorHook.Dispose();
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            //hook gets called (for this mod) only when not using Rain Reloader
            Plugin.Logger.LogDebug("RainWorldOnModsInitHook, first time initializing options interface and sprites");
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
            MenuManager.LoadSprites();
        }


        //after mods initialized
        static void RainWorldPostModsInitHook(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            //hook gets called (for this mod) only when not using Rain Reloader
            Integration.RefreshActiveMods();

            if (Integration.devConsoleEnabled) {
                try {
                    Integration.DevConsoleRegisterCommands();
                } catch (System.Exception ex) {
                    Plugin.Logger.LogError("RainWorldPostModsInitHook exception during registration of commands Dev Console, integration is now disabled: " + ex?.ToString());
                    Integration.devConsoleEnabled = false;
                }
            }
        }


        //at new game
        static void RainWorldGameCtorHook(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameCtorHook, resetting values");
            Pause.UnpauseAll();
            Stun.UnstunAll();
            Forcefield.ClearForcefields();
            Control.ReleaseControlAll();
            Gravity.gravityType = Gravity.GravityTypes.None;
            Lock.bodyChunks.Clear();
            State.GameStarted();
        }


        //at hibernate etc.
        static void RainWorldGameShutDownProcessHook(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);
            MenuManager.menu?.Destroy();
            MenuManager.menu = null;
            State.GameEnded();
        }


        //jolly co-op multiplayer safari control
        static void CreatureSafariControlInputUpdateHook(On.Creature.orig_SafariControlInputUpdate orig, Creature self, int playerIndex)
        {
            int pI = playerIndex;

            //if safari was activated via this mod, the creature was stored in this list
            var pair = Control.ListContains(self?.abstractCreature);
            if (pair != null && pair.Value.Value >= 0 && //sanitize input to avoid crashes
                pair.Value.Value < self?.room?.game?.rainWorld?.options?.controls?.Length)
                pI = pair.Value.Value; //use assigned playernumber for control

            orig(self, pI);

            //if abstractcreature is not in list, no other code will run
            if (pair == null || self?.abstractCreature == null)
                return;

            //check if any camera is currently in the same room as this creature
            bool isInCamRoom = false;
            bool isFollowed = false;
            for (int i = 0; i < self.room?.game?.cameras?.Length; i++) {
                RoomCamera camera = self.room.game.cameras[i];
                if (camera?.room == self.room)
                    isInCamRoom = true;
                if (camera?.followAbstractCreature == self.abstractCreature)
                    isFollowed = true;
            }

            //no player input if creature is in another room, because that crashes the game apparently
            //do still allow directional input so other safari players can still follow you through pipes
            if (!isInCamRoom || self.room == null) {
                Player.InputPackage? FilterInput(Player.InputPackage? risk) {
                    if (risk == null)
                        return null;
                    var pip = risk.Value;
                    pip.pckp = false;
                    pip.jmp = false;
                    pip.mp = false;
                    pip.thrw = false;
                    return pip;
                }
                self.inputWithoutDiagonals = FilterInput(self.inputWithoutDiagonals);
                self.lastInputWithoutDiagonals = FilterInput(self.lastInputWithoutDiagonals);
                self.inputWithDiagonals = FilterInput(self.inputWithDiagonals);
                self.lastInputWithDiagonals = FilterInput(self.lastInputWithDiagonals);
            }

            //creatures that aren't followed will not move option, only valid if camera can switch to creatures
            if (Options.controlNoInput?.Value == true && 
                Options.controlChangesCamera?.Value == true && 
                !isFollowed) {
                self.inputWithoutDiagonals = null;
                self.lastInputWithoutDiagonals = null;
                self.inputWithDiagonals = null;
                self.lastInputWithDiagonals = null;
            }
        }


        //anti smash-scug-into-wall
        static void RoomCameraApplyPositionChangeHook(On.RoomCamera.orig_ApplyPositionChange orig, RoomCamera self)
        {
            if (Options.velocityDragAtScreenChange?.Value != false)
                if (self != null && Drag.MouseCamera(self.game)?.cameraNumber == self.cameraNumber)
                    Drag.tempVelocityDrag = true;
            orig(self);
        }


        //disable vanilla sandbox mouse dragger
        static void MenuSandboxOverlayInitiateHook(On.Menu.SandboxOverlay.orig_Initiate orig, Menu.SandboxOverlay self, bool playMode)
        {
            orig(self, playMode);

            if (Options.disVnlMouseDragger?.Value != true)
                return;
            if (self?.mouseDragger == null || !(self.pages?.Count > 0) || self.pages[0].subObjects == null)
                return;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("MenuSandboxOverlayInitiateHook, removing Menu.SandboxOverlay.mouseDragger");
            if (self.pages[0].subObjects.Contains(self.mouseDragger))
                self.pages[0].subObjects.Remove(self.mouseDragger);
            self.mouseDragger = null;
        }


        //change visibility Rain World cursor
        public delegate bool orig_ShowCursor(Menu.Menu self);
        public static bool Menu_ShowCursor_get(orig_ShowCursor orig, Menu.Menu self)
        {
            bool ret = orig(self);
            if (Options.disVnlCursor?.Value == true)
                ret = false;
            return ret;
        }
    }
}
