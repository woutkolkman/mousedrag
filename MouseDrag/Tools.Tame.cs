using System.Collections.Generic;

namespace MouseDrag
{
    static partial class Tools
    {
        //code from Pokéballs
        public static void TameCreature(RainWorldGame game, PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (!(obj is Creature) || game?.Players == null || game.world == null || game.Players.Count <= 0)
                return;

            AbstractCreature player = game.Players[0];
            AbstractCreature creature = (obj as Creature).abstractCreature;
            if (!(player?.realizedCreature is Player) || creature == null)
                return;
            ArtificialIntelligence ai = creature.abstractAI?.RealAI;

            //initiate relationship
            if (ai is IUseARelationshipTracker) {
                //code from [Creature]PlayerRelationChange() but without the creatureCommunities.InfluenceLikeOfPlayer() part
                SocialMemory.Relationship rel = creature.state?.socialMemory?.GetOrInitiateRelationship(player.ID);
                if (rel?.tempLike < 1f)
                    rel.InfluenceTempLike(2f); //force max
                if (rel?.like < 1f)
                    rel.InfluenceLike(2f); //force max
                if (rel?.know < 0.25f)
                    rel.InfluenceKnow(0.25f);
                if (rel != null) {
                    if (Options.tameIncreasesRep?.Value == true)
                        game.session?.creatureCommunities?.InfluenceLikeOfPlayer(
                            creature.creatureTemplate.communityID, 
                            game.world.RegionNumber, 
                            (player.state as PlayerState).playerNumber, 
                            0.03f, 0.15f, 0.2f
                        );
                    Plugin.Logger.LogDebug("TameCreature, SocialMemory.Relationship data set: like=" + rel.like + ", tempLike=" + rel.tempLike + ", know=" + rel.know);
                }

                if (ai is FriendTracker.IHaveFriendTracker && ai.friendTracker != null) {
                    ai.friendTracker.friend = player.realizedCreature;
                    if (ai.friendTracker.friendRel == null)
                        ai.friendTracker.friendRel = rel;
                    Plugin.Logger.LogDebug("TameCreature, FriendTracker data set");
                }
            }
        }


        //tame all creatures in room
        public static void TameCreatures(RainWorldGame game, Room room)
        {
            Plugin.Logger.LogDebug("TameCreatures");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    TameCreature(game, room.physicalObjects[i][j]);
        }
    }
}
