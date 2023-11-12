using System.Collections.Generic;

namespace MouseDrag
{
    public static class Control
    {
        public static List<AbstractCreature> controlledCreatures = new List<AbstractCreature>();


        public static void ToggleControl(RainWorldGame game, Creature creature)
        {
            AbstractCreature ac = creature?.abstractCreature;
            if (ac == null || game == null)
                return;

            //if trying to safari control a player, clear all safari controls
            if (creature is Player && !(creature as Player).isNPC) {
                ReleaseControlAll();
                ReturnToCreature(game);
                return;
            }

            ac.controlled = !ac.controlled;

            //keep track of controlled creatures to easily switch or remove control later on
            if (ac.controlled && !controlledCreatures.Contains(ac)) {
                controlledCreatures.Add(ac);
            } else if (!ac.controlled && controlledCreatures.Contains(ac)) {
                controlledCreatures.Remove(ac);
            }

            //get player
            Creature player = creature.room?.game?.Players?.Count > 0 ? creature.room.game.Players[0]?.realizedCreature : null;

            //stun player, because a creature will be safari controlled
            if (ac.controlled && player?.abstractCreature != null)
                if (!Stun.stunnedObjects.Contains(player))
                    Stun.stunnedObjects.Add(player);

            //switch camera to next safari controlled creature
            ReturnToCreature(game);
        }


        public static void CreatureDeletedUpdate(RainWorldGame game)
        {
            if (controlledCreatures.Count <= 0)
                return;
            AbstractCreature ac = controlledCreatures[controlledCreatures.Count - 1];

            //creature not yet deleted
            if (ac.realizedCreature?.slatedForDeletetion == false)
                return;

            //remove creature
            controlledCreatures.Remove(ac);

            //switch camera to next safari controlled creature
            ReturnToCreature(game);
        }


        public static void ReleaseControlAll()
        {
            foreach(AbstractCreature ac in controlledCreatures)
                if (ac != null)
                    ac.controlled = false;
            controlledCreatures.Clear();
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ReleaseControlAll");
        }


        private static void ReturnToCreature(RainWorldGame game)
        {
            //get player
            AbstractCreature player = game?.Players?.Count > 0 ? game.Players[0] : null;

            AbstractCreature ac = null;

            if (controlledCreatures.Count > 0) {
                //there are creatures left to control
                ac = controlledCreatures[controlledCreatures.Count - 1];
            } else {
                //no creatures left, switch back to player
                ac = player;

                //unstun player
                if (Stun.stunnedObjects.Contains(player?.realizedCreature))
                    Stun.stunnedObjects.Remove(player?.realizedCreature);
            }

            SwitchCamera(game, ac);
        }


        private static void SwitchCamera(RainWorldGame game, AbstractCreature ac)
        {
            if (ac?.Room?.world == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("SwitchCamera failed: ac?.Room?.world is null");
                return;
            }

            //try to load room if it is not loaded
            if (ac.Room.realizedRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("SwitchCamera, realizedRoom is null, trying to load room");
                ac.Room.RealizeRoom(ac.Room.world, game);
                if (ac.Room.realizedRoom == null)
                    return;
            }

            if (game?.cameras?.Length <= 0 || game.cameras[0] == null)
                return;

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("SwitchCamera: " + ac.ToString());

            //follow this creature
            game.cameras[0].followAbstractCreature = ac;

            //if cam is in another room, switch rooms
            if (game.cameras[0].room != ac.Room.realizedRoom)
                game.cameras[0].MoveCamera(ac.Room.realizedRoom, -1);
        }
    }
}
