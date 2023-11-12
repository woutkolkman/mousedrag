using UnityEngine;

namespace MouseDrag
{
    public static class Control
    {
        public static void ToggleControl(Creature creature)
        {
            AbstractCreature ac = creature?.abstractCreature;
            if (ac == null)
                return;

            ac.controlled = !ac.controlled;

            //get player
            Creature player = creature.room?.game?.Players?.Count > 0 ? creature.room.game.Players[0]?.realizedCreature : null;

            //stun/unstun player
            if (player?.abstractCreature != null && player != creature) {
                if (Stun.stunnedObjects.Contains(player)) {
                    if (!ac.controlled)
                        Stun.stunnedObjects.Remove(player);
                } else {
                    if (ac.controlled)
                        Stun.stunnedObjects.Add(player);
                }
            }

            //switch camera
            Creature follow = (ac.controlled || player?.abstractCreature == null) ? creature : player;
            if (creature.room?.game?.cameras?.Length <= 0 || creature.room.game.cameras[0] == null || follow.room == null)
                return;
            creature.room.game.cameras[0].followAbstractCreature = follow.abstractCreature;

            //if cam is in another room
            if (creature.room.game.cameras[0].room != follow.room)
                creature.room.game.cameras[0].MoveCamera(follow.room, -1);
        }
    }
}
