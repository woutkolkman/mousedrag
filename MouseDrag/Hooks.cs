using UnityEngine;

namespace MouseDrag
{
    class Hooks
    {
        public static void Apply()
        {
            //initialize options & load sprites
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;

            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;

            //at framerate
            On.RainWorldGame.RawUpdate += RainWorldGameRawUpdateHook;

            //draw menu graphics with timestacker
            On.RainWorldGame.GrafUpdate += RainWorldGameGrafUpdateHook;

            //at new game
            On.RainWorldGame.ctor += RainWorldGameCtorHook;
        }


        public static void Unapply()
        {
            //TODO
        }


        //initialize options & load sprites
        static void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            MachineConnector.SetRegisteredOI(Plugin.GUID, new Options());
            MenuManager.LoadSprites();
        }


        //at tickrate
        public static int duplicateHoldCount = 0;
        public static int duplicateHoldMin = 40;
        static void RainWorldGameUpdateHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);

            State.UpdateActivated(self);
            MenuManager.Update(self);

            if (!State.activated)
                return;

            Tools.DragObject(self);

            //rapidly duplicate after one second feature
            if (Options.duplicateOneKey?.Value != null && Input.GetKey(Options.duplicateOneKey.Value)) {
                if (duplicateHoldCount >= duplicateHoldMin)
                    Tools.DuplicateObject();
                duplicateHoldCount++;
            } else {
                duplicateHoldCount = 0;
            }

            //windows cursor visible
            if (Options.forceMouseVisible?.Value != false)
                Cursor.visible = true;
        }


        //at framerate
        static void RainWorldGameRawUpdateHook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);

            if (self.GamePaused || self.pauseUpdate || !self.processActive)
                return;

            MenuManager.RawUpdate(self);

            //other checks are found in Tools.UpdateActivated
            if (State.activeType == Options.ActivateTypes.KeyBindPressed)
                if (Options.activateKey?.Value != null && Input.GetKeyDown(Options.activateKey.Value))
                    State.activated = !State.activated;

            //always active, so unpause together with deactivate dev tools works
            if (Options.unpauseAllKey?.Value != null && Input.GetKeyDown(Options.unpauseAllKey.Value))
                Tools.UnpauseAll();

            if (!State.activated)
                return;

            if (Options.pauseOneKey?.Value != null && Input.GetKeyDown(Options.pauseOneKey.Value))
                Tools.TogglePauseObject();

            if (Options.pauseRoomCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseRoomCreaturesKey.Value))
                Tools.PauseObjects(self.cameras[0]?.room, true);

            if (Options.deleteOneKey?.Value != null && Input.GetKeyDown(Options.deleteOneKey.Value))
                Tools.DeleteObject();

            if (Options.deleteAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.deleteAllCreaturesKey.Value))
                Tools.DeleteObjects(self.cameras[0]?.room, true);

            if (Options.deleteAllObjectsKey?.Value != null && Input.GetKeyDown(Options.deleteAllObjectsKey.Value))
                Tools.DeleteObjects(self.cameras[0]?.room, false);

            if (Options.killOneKey?.Value != null && Input.GetKeyDown(Options.killOneKey.Value)) {
                Tools.KillCreature(self);
                Tools.TriggerObject();
            }

            if (Options.killAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.killAllCreaturesKey.Value))
                Tools.KillCreatures(self, self.cameras[0]?.room);

            if (Options.reviveOneKey?.Value != null && Input.GetKeyDown(Options.reviveOneKey.Value)) {
                Tools.ReviveCreature();
                Tools.ResetObject();
            }

            if (Options.reviveAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.reviveAllCreaturesKey.Value))
                Tools.ReviveCreatures(self.cameras[0]?.room);

            if (Options.pauseAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseAllCreaturesKey.Value)) {
                Tools.pauseAllCreatures = !Tools.pauseAllCreatures;
                Plugin.Logger.LogDebug("pauseAllCreatures: " + Tools.pauseAllCreatures);
            }

            if (Options.pauseAllObjectsKey?.Value != null && Input.GetKeyDown(Options.pauseAllObjectsKey.Value)) {
                Tools.pauseAllObjects = !Tools.pauseAllObjects;
                Plugin.Logger.LogDebug("pauseAllObjects: " + Tools.pauseAllObjects);
            }

            if (Options.tameOneKey?.Value != null && Input.GetKeyDown(Options.tameOneKey.Value))
                Tools.TameCreature(self);

            if (Options.tameAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.tameAllCreaturesKey.Value))
                Tools.TameCreatures(self, self.cameras[0]?.room);

            if (Options.clearRelOneKey?.Value != null && Input.GetKeyDown(Options.clearRelOneKey.Value))
                Tools.ClearRelationships();

            if (Options.clearRelAllKey?.Value != null && Input.GetKeyDown(Options.clearRelAllKey.Value))
                Tools.ClearRelationships(self.cameras[0]?.room);

            if (Options.duplicateOneKey?.Value != null && Input.GetKeyDown(Options.duplicateOneKey.Value))
                Tools.DuplicateObject();
        }


        //draw menu graphics with timestacker
        static void RainWorldGameGrafUpdateHook(On.RainWorldGame.orig_GrafUpdate orig, RainWorldGame self, float timeStacker)
        {
            orig(self, timeStacker);
            MenuManager.DrawSprites(timeStacker);
        }


        //at new game
        static void RainWorldGameCtorHook(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);

            Tools.UnpauseAll();
            State.activated = false;
            Plugin.Logger.LogDebug("RainWorldGameCtorHook, resetting values");
        }
    }
}
