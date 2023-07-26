﻿using UnityEngine;

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

            Tools.UpdateActivated(self);

            if (!Tools.activated)
                return;

            Tools.DragObject(self);

            //windows cursor visible
            if (Options.forceMouseVisible?.Value != false)
                Cursor.visible = true;
        }


        //at framerate
        static void RainWorldGameRawUpdateHook(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (self.GamePaused || self.pauseUpdate || !self.processActive || self.pauseMenu != null)
                return;

            //other checks are found in Tools.UpdateActivated
            if (Tools.activeType == Options.ActivateTypes.KeyBindPressed)
                if (Options.activateKey?.Value != null && Input.GetKeyDown(Options.activateKey.Value))
                    Tools.activated = !Tools.activated;

            //always active, so unpause together with deactivate dev tools works
            if (Options.unpauseAllKey?.Value != null && Input.GetKeyDown(Options.unpauseAllKey.Value))
                Tools.UnpauseAll();

            if (!Tools.activated)
                return;

            if (Options.pauseOneKey?.Value != null && Input.GetKeyDown(Options.pauseOneKey.Value))
                Tools.TogglePauseObject();

            if (Options.pauseAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseAllCreaturesKey.Value))
                Tools.pauseAllCreatures = !Tools.pauseAllCreatures;

            if (Options.deleteOneKey?.Value != null && Input.GetKeyDown(Options.deleteOneKey.Value))
                Tools.DeleteObject();

            if (Options.deleteAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.deleteAllCreaturesKey.Value))
                Tools.DeleteObjects(self.cameras[0]?.room, true);

            if (Options.deleteAllObjectsKey?.Value != null && Input.GetKeyDown(Options.deleteAllObjectsKey.Value))
                Tools.DeleteObjects(self.cameras[0]?.room, false);

            if (Options.killOneKey?.Value != null && Input.GetKeyDown(Options.killOneKey.Value))
                Tools.KillCreature();

            if (Options.reviveOneKey?.Value != null && Input.GetKeyDown(Options.reviveOneKey.Value))
                Tools.ReviveCreature();

            if (Options.duplicateOneKey?.Value != null && Input.GetKeyDown(Options.duplicateOneKey.Value))
                Tools.DuplicateObject();
        }
    }
}
