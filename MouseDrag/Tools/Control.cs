using System.Collections.Generic;

namespace MouseDrag
{
    public static class Control
    {
        //this code works in conjunction with CreatureSafariControlInputUpdateHook
        //TODO this class kind of needs to be rewritten to fully support SplitScreen Co-op and multiple cameras
        //TODO run Update() & manage loadCreatureRoom for all cameras simultaneously instead of only the camera where 
        //the mouse is currently located, so when creatures get destroyed, camera properly goes to the next creature


        public static List<KeyValuePair<AbstractCreature, int>> controlledCreatures = new List<KeyValuePair<AbstractCreature, int>>();
        public static AbstractCreature loadCreatureRoom = null;


        public static void ToggleControl(RainWorldGame game, Creature creature)
        {
            //without downpour DLC the safari-controls just don't work
            if (!ModManager.MSC)
                return;

            AbstractCreature ac = creature?.abstractCreature;
            if (ac == null || game == null)
                return;

            //if trying to safari control a player, clear all safari controls
            if (creature is Player && !(creature as Player).isNPC) {
                ReleaseControlAll(Drag.playerNr);
                ReturnToCreature(game);
                return;
            }

            //variable added in downpour that will immediately enable control, default playerNumber 0
            ac.controlled = !ac.controlled;

            //keep track of controlled creatures to easily switch or remove control later on
            //the player that will control this creature is determined by Drag.playerNr (the last dragged player)
            //also check CreatureSafariControlInputUpdateHook
            if (ac.controlled && (null == ListContains(ac))) {
                if (Options.controlOnlyOne?.Value == true)
                    ReleaseControlAll(Drag.playerNr);
                controlledCreatures.Add(new KeyValuePair<AbstractCreature, int>(ac, Drag.playerNr));

                //activate unused player input
                if (game.rainWorld?.options?.controls?.Length > Drag.playerNr)
                    if (game.rainWorld.options.controls[Drag.playerNr] != null)
                        game.rainWorld.options.controls[Drag.playerNr].active = true;

            } else if (!ac.controlled) {
                ListRemove(ac);
            }

            //move camera to next safari controlled creature, and stun/unstun players
            ReturnToCreature(game);
        }


        public static void Update(RainWorldGame game)
        {
            var camera = GetCamera(game, Drag.playerNr);
            if (camera == null)
                return;

            //wait for room to be loaded before moving camera to loadCreatureRoom
            if (loadCreatureRoom != null) {
                if (loadCreatureRoom.Room?.realizedRoom == null) {
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogWarning("Control.Update, moving camera failed, Room?.realizedRoom is null");
                    loadCreatureRoom = null;
                    return;
                }
                if (!loadCreatureRoom.Room.realizedRoom.ReadyForPlayer)
                    return;
                if (!loadCreatureRoom.Room.realizedRoom.fullyLoaded)
                    return;
                camera.MoveCamera(loadCreatureRoom.Room.realizedRoom, -1);
                loadCreatureRoom = null;
            }

            if (controlledCreatures.Count <= 0)
                return;

            //NOTE: stun and camera follow is only updated when this controlled
            //creature (which is followed by the camera) is deleted or control is revoked
            AbstractCreature ac = null;
            if (ListContains(camera.followAbstractCreature) != null)
                ac = camera.followAbstractCreature;

            //probably player is viewed currently
            if (ac == null)
                return;

            //creature not yet deleted
            if (ac.realizedCreature?.slatedForDeletetion == false)
                return;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Control.Update, creature was deleted, removing from list: " + ac.ToString());

            //remove creature
            ListRemove(ac);

            //move camera to next safari controlled creature, and stun/unstun players
            ReturnToCreature(game);
        }


        public static void ReleaseControlAll(int playerNr = -1)
        {
            for (int i = controlledCreatures.Count - 1; i >= 0; i--) {
                var pair = controlledCreatures[i];
                if (pair.Key != null && (pair.Value == playerNr || playerNr == -1)) {
                    pair.Key.controlled = false;
                    controlledCreatures.Remove(pair);
                }
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ReleaseControlAll" + (playerNr != -1 ? ", player " + playerNr : ""));
        }


        public static bool PlayerHasControl(int playerNr)
        {
            for (int i = controlledCreatures.Count - 1; i >= 0; i--) {
                var pair = controlledCreatures[i];
                if (pair.Key != null && pair.Value == playerNr)
                    return true;
            }
            return false;
        }


        private static void ReturnToCreature(RainWorldGame game)
        {
            AbstractCreature ac = null;

            if (controlledCreatures.Count > 0) {
                //there are creatures left to control
                ac = ListLast();
            } else if (game?.Players != null) {
                //no creatures left, switch back to last dragged player
                foreach (AbstractCreature abst in game.Players)
                    if ((abst?.state as PlayerState)?.playerNumber == Drag.playerNr)
                        ac = abst;
                if (ac == null && game.Players.Count > 0) {
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogDebug("ReturnToCreature, player not available, return to first player");
                    ac = game.Players[0];
                }
            }

            StunPlayers(game, ac);
            MoveCamera(game, ac);
        }


        //refresh stunned players, game.Players array is unordered
        private static void StunPlayers(RainWorldGame game, AbstractCreature except = null)
        {
            if (Options.controlStunsPlayers?.Value != true)
                return;

            if (game?.Players == null)
                return;

            for (int i = 0; i < game.Players.Count; i++)
                if (Stun.stunnedObjects.Contains(game.Players[i]))
                    Stun.stunnedObjects.Remove(game.Players[i]);

            for (int i = 0; i < controlledCreatures.Count; i++)
                foreach (AbstractCreature abst in game.Players)
                    if ((abst?.state as PlayerState)?.playerNumber == controlledCreatures[i].Value && 
                        !Stun.stunnedObjects.Contains(abst) && 
                        abst != except)
                        Stun.stunnedObjects.Add(abst);
        }


        private static void MoveCamera(RainWorldGame game, AbstractCreature ac)
        {
            if (Options.controlChangesCamera?.Value != true)
                return;

            if (ac?.Room?.world == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("MoveCamera failed: ac?.Room?.world is null");
                return;
            }

            if (ac.realizedCreature == null)
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("MoveCamera, note that realizedCreature is null");

            var camera = GetCamera(game, Drag.playerNr);
            if (camera == null)
                return;

            //try to load room if it is not loaded
            if (ac.Room.realizedRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("MoveCamera, realizedRoom is null, trying to load room");
                ac.Room.RealizeRoom(ac.Room.world, game);
                if (ac.Room.realizedRoom == null)
                    return;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("MoveCamera: " + ac.ToString());

            //follow this creature
            camera.followAbstractCreature = ac;

            //if cam is in another room, switch rooms (when room is fully loaded)
            if (camera.room != ac.Room.realizedRoom)
                loadCreatureRoom = ac;
        }


        public static void CycleCamera(RainWorldGame game)
        {
            var camera = GetCamera(game, Drag.playerNr);
            if (camera == null)
                return;

            //still moving camera
            if (loadCreatureRoom != null)
                return;

            var pair = ListContains(camera.followAbstractCreature);

            //return to last controlled creature if camera is probably following player
            if (pair == null) {
                ReturnToCreature(game);
                return;
            }

            //go back one creature if there are more before this one
            int idx = controlledCreatures.IndexOf(pair.Value);
            if (idx > 0) {
                MoveCamera(game, controlledCreatures[idx - 1].Key);
                return;
            }

            //reasign current playerNr based on mouse position in SplitScreen Co-op
            if (Integration.splitScreenCoopEnabled)
                if (camera.cameraNumber >= 0 && camera.cameraNumber < game.Players?.Count)
                    Drag.playerNr = camera.cameraNumber;

            //go back to player
            if (Drag.playerNr >= 0 && Drag.playerNr < game.Players?.Count) {
                StunPlayers(game, game.Players[Drag.playerNr]); //re-stun
                MoveCamera(game, game.Players[Drag.playerNr]);
                return;
            }

            //go back to first player
            if (game.Players?.Count > 0) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("CycleCamera, player not available, return to first player");
                StunPlayers(game, game.Players[0]); //re-stun
                MoveCamera(game, game.Players[0]);
                return;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogWarning("CycleCamera failed");
        }


        public static AbstractCreature ListLast()
        {
            if (controlledCreatures.Count <= 0)
                return null;
            return controlledCreatures[controlledCreatures.Count - 1].Key;
        }


        public static KeyValuePair<AbstractCreature, int>? ListContains(AbstractCreature ac)
        {
            if (ac == null)
                return null;
            foreach (var pair in controlledCreatures)
                if (pair.Key == ac)
                    return pair;
            return null;
        }


        public static void ListRemove(AbstractCreature ac)
        {
            for (int i = 0; i < controlledCreatures.Count; i++) {
                if (controlledCreatures[i].Key == ac) {
                    controlledCreatures.Remove(controlledCreatures[i]);
                    break;
                }
            }
        }


        public static RoomCamera GetCamera(RainWorldGame game, int playerNr = 0)
        {
            if (!(game?.cameras?.Length > 0))
                return null;
            if (Integration.splitScreenCoopEnabled)
                return Drag.MouseCamera(game);
            if (playerNr >= 0 && playerNr < game.cameras.Length)
                return game.cameras[playerNr];
            return game.cameras[0];
        }
    }
}
