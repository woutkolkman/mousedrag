using UnityEngine;

namespace MouseDrag
{
    public static class Control
    {
        public static AbstractCreature lastControlled;


        public static void ToggleControl(Creature creature)
        {
            AbstractCreature ac = creature?.abstractCreature;
            if (ac == null)
                return;

            ac.controlled = !ac.controlled;
            lastControlled = ac.controlled ? ac : null;

            //get player
            Creature player = creature.room?.game?.Players?.Count > 0 ? creature.room.game.Players[0]?.realizedCreature : null;

            //stun/unstun player
            if (player?.abstractCreature != null) {
                if (Stun.stunnedObjects.Contains(player)) {
                    if (!ac.controlled || creature == player)
                        Stun.stunnedObjects.Remove(player);
                } else {
                    if (ac.controlled && creature != player)
                        Stun.stunnedObjects.Add(player);
                }
            }

            //switch camera
            Creature follow = (ac.controlled || player?.abstractCreature == null) ? creature : player;
            SwitchCamera(follow);
        }


        public static void CreatureDeletedUpdate()
        {
            if (lastControlled == null)
                return;

            if (lastControlled.realizedCreature?.slatedForDeletetion == false)
                return;

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("CreatureDeletedUpdate, returning camera to player");

            //get player
            Creature player = lastControlled.Room?.world?.game?.Players?.Count > 0 ? lastControlled.Room.world.game.Players[0]?.realizedCreature : null;

            lastControlled = null;

            if (player?.abstractCreature == null)
                return;

            //unstun player
            if (Stun.stunnedObjects.Contains(player))
                Stun.stunnedObjects.Remove(player);

            SwitchCamera(player);
        }


        private static void SwitchCamera(Creature creature)
        {
            if (creature?.abstractCreature == null)
                return;
            if (creature?.room?.game?.cameras?.Length <= 0 || creature.room.game.cameras[0] == null)
                return;

            //follow this creature
            creature.room.game.cameras[0].followAbstractCreature = creature.abstractCreature;

            //if cam is in another room, switch rooms
            if (creature.room.game.cameras[0].room != creature.room)
                creature.room.game.cameras[0].MoveCamera(creature.room, -1);
        }
    }
}
