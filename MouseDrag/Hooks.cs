using UnityEngine;

namespace MouseDrag
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

            //draw menu graphics with timestacker
            On.RainWorldGame.GrafUpdate += RainWorldGameGrafUpdateHook;

            //at new game
            On.RainWorldGame.ctor += RainWorldGameCtorHook;

            //at hibernate etc.
            On.RainWorldGame.ShutDownProcess += RainWorldGameShutDownProcessHook;

            //forcefield
            On.BodyChunk.Update += BodyChunkUpdateHook;

            //jolly co-op multiplayer safari control
            On.Creature.SafariControlInputUpdate += CreatureSafariControlInputUpdateHook;
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


        //after mods initialized
        static void RainWorldPostModsInitHook(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            //check if BeastMaster is enabled
            for (int i = 0; i < ModManager.ActiveMods.Count; i++) {
                if (ModManager.ActiveMods[i].id == "fyre.BeastMaster")
                    MenuManager.beastMasterEnabled = Options.beastMasterIntegration?.Value ?? true;
            }
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

            Drag.DragObject(self);
            Control.Update(self);

            //rapidly duplicate after one second feature
            if (Options.duplicateOneKey?.Value != null && Input.GetKey(Options.duplicateOneKey.Value)) {
                if (duplicateHoldCount >= duplicateHoldMin)
                    Duplicate.DuplicateObject(Drag.dragChunk?.owner);
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

            //other checks are found in State.UpdateActivated
            if (State.activeType == Options.ActivateTypes.KeyBindPressed)
                if (Options.activateKey?.Value != null && Input.GetKeyDown(Options.activateKey.Value))
                    State.activated = !State.activated;

            //always active, so unpause together with deactivate dev tools works
            if (Options.unpauseAllKey?.Value != null && Input.GetKeyDown(Options.unpauseAllKey.Value))
                Pause.UnpauseAll();

            if (Options.unstunAllKey?.Value != null && Input.GetKeyDown(Options.unstunAllKey.Value))
                Stun.UnstunAll();

            if (!State.activated)
                return;

            if (Options.throwWeapon?.Value != null && Input.GetKeyDown(Options.throwWeapon.Value)) {
                Drag.TryThrow(self, Drag.dragChunk?.owner, overrideThreshold: true);
                Drag.tempStopTicks = 20;
            }

            if (Options.pauseOneKey?.Value != null && Input.GetKeyDown(Options.pauseOneKey.Value))
                Pause.TogglePauseObject(Drag.dragChunk?.owner);

            if (Options.pauseRoomCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseRoomCreaturesKey.Value))
                Pause.PauseObjects(self.cameras[0]?.room, true);

            if (Options.pauseAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseAllCreaturesKey.Value)) {
                Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllCreatures: " + Pause.pauseAllCreatures);
            }

            if (Options.pauseAllObjectsKey?.Value != null && Input.GetKeyDown(Options.pauseAllObjectsKey.Value)) {
                Pause.pauseAllObjects = !Pause.pauseAllObjects;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllObjects: " + Pause.pauseAllObjects);
            }

            if (Options.killOneKey?.Value != null && Input.GetKeyDown(Options.killOneKey.Value)) {
                Health.KillCreature(self, Drag.dragChunk?.owner);
                Health.TriggerObject(Drag.dragChunk?.owner);
            }

            if (Options.killAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.killAllCreaturesKey.Value))
                Health.KillCreatures(self, self.cameras[0]?.room);

            if (Options.reviveOneKey?.Value != null && Input.GetKeyDown(Options.reviveOneKey.Value)) {
                Health.ReviveCreature(Drag.dragChunk?.owner);
                Health.ResetObject(Drag.dragChunk?.owner);
            }

            if (Options.reviveAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.reviveAllCreaturesKey.Value))
                Health.ReviveCreatures(self.cameras[0]?.room);

            if (Options.duplicateOneKey?.Value != null && Input.GetKeyDown(Options.duplicateOneKey.Value))
                Duplicate.DuplicateObject(Drag.dragChunk?.owner);

            if (Options.clipboardCtrlXCV?.Value == true) {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                    if (Input.GetKeyDown(KeyCode.X))
                        Clipboard.CutObject(Drag.dragChunk?.owner);
                    if (Input.GetKeyDown(KeyCode.C))
                        Clipboard.CopyObject(Drag.dragChunk?.owner);
                    if (Input.GetKeyDown(KeyCode.V) && self.cameras[0]?.room != null) {
                        Clipboard.PasteObject(
                            self, 
                            self.cameras[0].room, 
                            self.cameras[0].room.ToWorldCoordinate((Vector2)Futile.mousePosition + self.cameras[0].pos)
                        );
                    }
                }
            }

            if (Options.tpCreaturesKey?.Value != null && Input.GetKey(Options.tpCreaturesKey.Value))
                Teleport.TeleportObjects(self, self.cameras[0]?.room, true, false);

            if (Options.tpObjectsKey?.Value != null && Input.GetKey(Options.tpObjectsKey.Value))
                Teleport.TeleportObjects(self, self.cameras[0]?.room, false, true);

            if (Options.controlKey?.Value != null && Input.GetKeyDown(Options.controlKey.Value)) {
                if (Drag.dragChunk?.owner != null) {
                    Control.ToggleControl(self, Drag.dragChunk?.owner as Creature);
                } else {
                    Control.CycleCamera(self);
                }
            }

            if (Options.forcefieldKey?.Value != null && Input.GetKeyDown(Options.forcefieldKey.Value))
                Forcefield.ToggleForcefield(Drag.dragChunk);

            if (Options.tameOneKey?.Value != null && Input.GetKeyDown(Options.tameOneKey.Value))
                Tame.TameCreature(self, Drag.dragChunk?.owner);

            if (Options.tameAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.tameAllCreaturesKey.Value))
                Tame.TameCreatures(self, self.cameras[0]?.room);

            if (Options.clearRelOneKey?.Value != null && Input.GetKeyDown(Options.clearRelOneKey.Value))
                Tame.ClearRelationships(Drag.dragChunk?.owner);

            if (Options.clearRelAllKey?.Value != null && Input.GetKeyDown(Options.clearRelAllKey.Value))
                Tame.ClearRelationships(self.cameras[0]?.room);

            if (Options.stunOneKey?.Value != null && Input.GetKeyDown(Options.stunOneKey.Value))
                Stun.ToggleStunObject(Drag.dragChunk?.owner);

            if (Options.stunRoomKey?.Value != null && Input.GetKeyDown(Options.stunRoomKey.Value))
                Stun.StunObjects(self.cameras[0]?.room);

            if (Options.stunAllKey?.Value != null && Input.GetKeyDown(Options.stunAllKey.Value)) {
                Stun.stunAll = !Stun.stunAll;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
            }

            if (Options.destroyOneKey?.Value != null && Input.GetKeyDown(Options.destroyOneKey.Value))
                Destroy.DestroyObject(Drag.dragChunk?.owner);

            if (Options.destroyAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyAllCreaturesKey.Value))
                Destroy.DestroyObjects(self.cameras[0]?.room, creatures: true, objects: false);

            if (Options.destroyAllObjectsKey?.Value != null && Input.GetKeyDown(Options.destroyAllObjectsKey.Value))
                Destroy.DestroyObjects(self.cameras[0]?.room, creatures: false, objects: true);

            if (Options.destroyRegionCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRegionCreaturesKey.Value))
                Destroy.DestroyRegionObjects(self, creatures: true, objects: false);

            if (Options.destroyRegionObjectsKey?.Value != null && Input.GetKeyDown(Options.destroyRegionObjectsKey.Value))
                Destroy.DestroyRegionObjects(self, creatures: false, objects: true);
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

            Pause.UnpauseAll();
            Stun.UnstunAll();
            Forcefield.ClearForcefields();
            Control.ReleaseControlAll();
            if (Options.deactivateEveryRestart?.Value != false)
                State.activated = false;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameCtorHook, resetting values");

            //read activeType from config when game is started
            if (Options.activateType?.Value != null) {
                foreach (Options.ActivateTypes val in System.Enum.GetValues(typeof(Options.ActivateTypes)))
                    if (System.String.Equals(Options.activateType.Value, val.ToString()))
                        State.activeType = val;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("RainWorldGameCtorHook, activeType: " + State.activeType.ToString());
            }
        }


        //at hibernate etc.
        static void RainWorldGameShutDownProcessHook(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);
            MenuManager.menu?.Destroy();
            MenuManager.menu = null;
        }


        //forcefield
        static void BodyChunkUpdateHook(On.BodyChunk.orig_Update orig, BodyChunk self)
        {
            orig(self);
            Forcefield.UpdateForcefield(self);
        }


        //jolly co-op multiplayer safari control
        static void CreatureSafariControlInputUpdateHook(On.Creature.orig_SafariControlInputUpdate orig, Creature self, int playerIndex)
        {
            int pI = playerIndex;

            var pair = Control.ListContains(self?.abstractCreature);
            if (pair != null && pair.Value.Value >= 0 && //sanitize input to avoid crashes
                pair.Value.Value < self?.room?.game?.rainWorld?.options?.controls?.Length)
                pI = pair.Value.Value; //use assigned playernumber for control

            orig(self, pI);

            if (self?.room?.game?.cameras?.Length <= 0 || self.room.game.cameras[0] == null)
                return;

            //no player input if creature is in another room, because that crashes the game
            if (pair != null && (self.room == null || self.room.game.cameras[0].room != self.room)) {
                self.inputWithoutDiagonals = null;
                self.lastInputWithoutDiagonals = null;
                self.inputWithDiagonals = null;
                self.lastInputWithDiagonals = null;
            }
        }
    }
}
