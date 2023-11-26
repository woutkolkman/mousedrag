using System.Collections.Generic;

namespace MouseDrag
{
    public static class Control
    {
        public static List<KeyValuePair<AbstractCreature, int>> controlledCreatures = new List<KeyValuePair<AbstractCreature, int>>();


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
            } else if (!ac.controlled) {
                ListRemove(ac);
            }

            //switch camera to next safari controlled creature, and stun/unstun players
            ReturnToCreature(game);
        }


        public static void CreatureDeletedUpdate(RainWorldGame game)
        {
            if (controlledCreatures.Count <= 0)
                return;

            //some rooms are still loading
            if (game?.world?.loadingRooms?.Count > 0)
                return;

            //NOTE: stun and camera follow is only updated when the last controlled creature is deleted or control is revoked

            AbstractCreature ac = ListLast();

            //creature not yet deleted
            if (ac.realizedCreature?.slatedForDeletetion == false)
                return;

            //remove creature
            ListRemove(ac);

            //switch camera to next safari controlled creature, and stun/unstun players
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
            }

            StunPlayers(game);
            SwitchCamera(game, ac);
        }


        //refresh stunned players, game.Players array is unordered
        private static void StunPlayers(RainWorldGame game)
        {
            if (game?.Players == null)
                return;

            for (int i = 0; i < game.Players.Count; i++)
                if (Stun.stunnedObjects.Contains(game.Players[i]))
                    Stun.stunnedObjects.Remove(game.Players[i]);

            for (int i = 0; i < controlledCreatures.Count; i++)
                foreach (AbstractCreature abst in game.Players)
                    if ((abst?.state as PlayerState)?.playerNumber == controlledCreatures[i].Value &&
                        !Stun.stunnedObjects.Contains(abst))
                        Stun.stunnedObjects.Add(abst);
        }


        private static void SwitchCamera(RainWorldGame game, AbstractCreature ac)
        {
            if (Options.controlChangesCamera?.Value != true)
                return;

            if (ac?.Room?.world == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("SwitchCamera failed: ac?.Room?.world is null");
                return;
            }

            if (game?.cameras?.Length <= 0 || game.cameras[0] == null)
                return;

            //try to load room if it is not loaded
            if (ac.Room.realizedRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("SwitchCamera, realizedRoom is null, trying to load room");
                ac.Room.RealizeRoom(ac.Room.world, game);
                if (ac.Room.realizedRoom == null)
                    return;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("SwitchCamera: " + ac.ToString());

            //follow this creature
            game.cameras[0].followAbstractCreature = ac;

            //if cam is in another room, switch rooms
            if (game.cameras[0].room != ac.Room.realizedRoom)
                game.cameras[0].MoveCamera(ac.Room.realizedRoom, -1);
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
    }
}
