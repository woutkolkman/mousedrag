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


        private static void SwitchCamera(AbstractCreature ac)
        {
            if (ac?.Room?.realizedRoom == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogWarning("SwitchCamera not switching: ac?.Room?.realizedRoom == null");
                return;
            }
            if (ac?.Room?.world?.game?.cameras?.Length <= 0)
                return;
            RoomCamera camera = ac.Room.world.game.cameras[0];
            if (camera == null)
                return;

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("SwitchCamera: " + ac.realizedCreature?.ToString());

            //follow this creature
            camera.followAbstractCreature = ac;

            //if cam is in another room, switch rooms
            if (camera.room != ac.Room.realizedRoom)
                camera.MoveCamera(ac.Room.realizedRoom, -1);
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

            SwitchCamera(ac);
        }
    }
}
