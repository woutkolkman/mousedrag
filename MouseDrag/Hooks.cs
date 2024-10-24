﻿using UnityEngine;

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

            //gravity
            On.Room.Update += RoomUpdateHook;

            //anti smash-scug-into-wall
            On.RoomCamera.ApplyPositionChange += RoomCameraApplyPositionChangeHook;

            //disable vanilla sandbox mouse dragger
            On.Menu.SandboxOverlay.Initiate += MenuSandboxOverlayInitiateHook;
        }


        public static void Unapply()
        {
            On.RainWorld.OnModsInit -= RainWorldOnModsInitHook;
            On.RainWorld.PostModsInit -= RainWorldPostModsInitHook;
            On.RainWorldGame.Update -= RainWorldGameUpdateHook;
            On.RainWorldGame.RawUpdate -= RainWorldGameRawUpdateHook;
            On.RainWorldGame.GrafUpdate -= RainWorldGameGrafUpdateHook;
            On.RainWorldGame.ctor -= RainWorldGameCtorHook;
            On.RainWorldGame.ShutDownProcess -= RainWorldGameShutDownProcessHook;
            On.BodyChunk.Update += BodyChunkUpdateHook;
            On.Creature.SafariControlInputUpdate -= CreatureSafariControlInputUpdateHook;
            On.Room.Update -= RoomUpdateHook;
            On.RoomCamera.ApplyPositionChange -= RoomCameraApplyPositionChangeHook;
            On.Menu.SandboxOverlay.Initiate -= MenuSandboxOverlayInitiateHook;
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

            //always active, so unpause together with deactivate tools works
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

            if (Teleport.UpdateTeleportObject(self))
                Drag.dragChunk = null;

            if (Options.pauseOneKey?.Value != null && Input.GetKeyDown(Options.pauseOneKey.Value))
                Pause.TogglePauseObject(Drag.dragChunk?.owner);

            if (Options.pauseRoomCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseRoomCreaturesKey.Value))
                Pause.PauseObjects(Drag.MouseCamera(self)?.room, true);

            if (Options.pauseAllCreaturesKey?.Value != null && Input.GetKeyDown(Options.pauseAllCreaturesKey.Value)) {
                Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllCreatures: " + Pause.pauseAllCreatures);
            }

            if (Options.pauseAllItemsKey?.Value != null && Input.GetKeyDown(Options.pauseAllItemsKey.Value)) {
                Pause.pauseAllItems = !Pause.pauseAllItems;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("pauseAllItems: " + Pause.pauseAllItems);
            }

            if (Options.killOneKey?.Value != null && Input.GetKeyDown(Options.killOneKey.Value)) {
                Health.KillCreature(self, Drag.dragChunk?.owner);
                Health.TriggerItem(Drag.dragChunk?.owner);
            }

            if (Options.killRoomKey?.Value != null && Input.GetKeyDown(Options.killRoomKey.Value))
                Health.KillCreatures(self, Drag.MouseCamera(self)?.room);

            if (Options.reviveOneKey?.Value != null && Input.GetKeyDown(Options.reviveOneKey.Value)) {
                Health.ReviveCreature(Drag.dragChunk?.owner);
                Health.ResetItem(Drag.dragChunk?.owner);
            }

            if (Options.reviveRoomKey?.Value != null && Input.GetKeyDown(Options.reviveRoomKey.Value))
                Health.ReviveCreatures(Drag.MouseCamera(self)?.room);

            if (Options.duplicateOneKey?.Value != null && Input.GetKeyDown(Options.duplicateOneKey.Value))
                Duplicate.DuplicateObject(Drag.dragChunk?.owner);

            if (Options.clipboardCtrlXCV?.Value == true) {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                    if (Input.GetKeyDown(KeyCode.X))
                        Clipboard.CutObject(Drag.dragChunk?.owner);
                    if (Input.GetKeyDown(KeyCode.C))
                        Clipboard.CopyObject(Drag.dragChunk?.owner);
                    var rcam = Drag.MouseCamera(self);
                    if (Input.GetKeyDown(KeyCode.V) && rcam?.room != null) {
                        Clipboard.PasteObject(
                            self,
                            rcam.room,
                            rcam.room.ToWorldCoordinate(Drag.MousePos(self))
                        );
                    }
                }
            }

            if (Options.tpCreaturesKey?.Value != null && Input.GetKey(Options.tpCreaturesKey.Value))
                Teleport.TeleportObjects(self, Drag.MouseCamera(self)?.room, true, false);

            if (Options.tpItemsKey?.Value != null && Input.GetKey(Options.tpItemsKey.Value))
                Teleport.TeleportObjects(self, Drag.MouseCamera(self)?.room, false, true);

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

            if (Options.tameRoomKey?.Value != null && Input.GetKeyDown(Options.tameRoomKey.Value))
                Tame.TameCreatures(self, Drag.MouseCamera(self)?.room);

            if (Options.clearRelOneKey?.Value != null && Input.GetKeyDown(Options.clearRelOneKey.Value))
                Tame.ClearRelationships(Drag.dragChunk?.owner);

            if (Options.clearRelRoomKey?.Value != null && Input.GetKeyDown(Options.clearRelRoomKey.Value))
                Tame.ClearRelationships(Drag.MouseCamera(self)?.room);

            if (Options.stunOneKey?.Value != null && Input.GetKeyDown(Options.stunOneKey.Value))
                Stun.ToggleStunObject(Drag.dragChunk?.owner);

            if (Options.stunRoomKey?.Value != null && Input.GetKeyDown(Options.stunRoomKey.Value))
                Stun.StunObjects(Drag.MouseCamera(self)?.room);

            if (Options.stunAllKey?.Value != null && Input.GetKeyDown(Options.stunAllKey.Value)) {
                Stun.stunAll = !Stun.stunAll;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("stunAll: " + Stun.stunAll);
            }

            if (Options.destroyOneKey?.Value != null && Input.GetKeyDown(Options.destroyOneKey.Value))
                Destroy.DestroyObject(Drag.dragChunk?.owner);

            if (Options.destroyRoomCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRoomCreaturesKey.Value))
                Destroy.DestroyObjects(Drag.MouseCamera(self)?.room, creatures: true, items: false, onlyDead: false);

            if (Options.destroyRoomItemsKey?.Value != null && Input.GetKeyDown(Options.destroyRoomItemsKey.Value))
                Destroy.DestroyObjects(Drag.MouseCamera(self)?.room, creatures: false, items: true, onlyDead: false);

            if (Options.destroyRegionCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRegionCreaturesKey.Value))
                Destroy.DestroyRegionObjects(self, creatures: true, items: false);

            if (Options.destroyRegionItemsKey?.Value != null && Input.GetKeyDown(Options.destroyRegionItemsKey.Value))
                Destroy.DestroyRegionObjects(self, creatures: false, items: true);

            if (Options.destroyRoomDeadCreaturesKey?.Value != null && Input.GetKeyDown(Options.destroyRoomDeadCreaturesKey.Value))
                Destroy.DestroyObjects(Drag.MouseCamera(self)?.room, creatures: true, items: false, onlyDead: true);

            if (Options.lockKey?.Value != null && Input.GetKeyDown(Options.lockKey.Value))
                Lock.ToggleLock(Drag.dragChunk);

            if (Options.gravityRoomKey?.Value != null && Input.GetKeyDown(Options.gravityRoomKey.Value))
                Gravity.CycleGravity();

            if (Options.infoKey?.Value != null && Input.GetKeyDown(Options.infoKey.Value)) {
                if (Drag.dragChunk?.owner != null) {
                    Info.DumpInfo(Drag.dragChunk.owner);
                } else {
                    Info.DumpInfo(Drag.MouseCamera(self)?.room);
                }
            }
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

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameCtorHook, resetting values");
            Pause.UnpauseAll();
            Stun.UnstunAll();
            Forcefield.ClearForcefields();
            Control.ReleaseControlAll();
            Gravity.gravityType = Gravity.GravityTypes.None;
            Lock.bodyChunks.Clear();
            if (Options.deactivateEveryRestart?.Value != false)
                State.activated = false;

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

            //prevent invisible mouse in main menu
            if (Options.manageMouseVisibility?.Value == true) {
                Cursor.visible = true;
                State.prevMousePos = Vector2.zero;
            }
        }


        //forcefield
        static void BodyChunkUpdateHook(On.BodyChunk.orig_Update orig, BodyChunk self)
        {
            orig(self);
            Forcefield.UpdateForcefield(self);
            Lock.UpdatePosition(self);
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


        //gravity
        static void RoomUpdateHook(On.Room.orig_Update orig, Room self)
        {
            if (self != null) {
                if (Gravity.gravityType == Gravity.GravityTypes.Off)
                    self.gravity = 0f;
                if (Gravity.gravityType == Gravity.GravityTypes.Half)
                    self.gravity = 0.2f;
                if (Gravity.gravityType == Gravity.GravityTypes.On)
                    self.gravity = 1f;
                if (Gravity.gravityType == Gravity.GravityTypes.Inverse)
                    self.gravity = -1f;
            }
            orig(self);
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
    }
}
