using System.Linq;

namespace MouseDrag
{
    public static class Tame
    {
        //check if creature can be tamed
        public static bool IsTamable(RainWorldGame game, PhysicalObject obj)
        {
            if (!(obj is Creature))
                return false;
            if (game?.FirstAlivePlayer == null)
                return false;
            ArtificialIntelligence ai = (obj as Creature)?.abstractCreature?.abstractAI?.RealAI;
            return ai is IUseARelationshipTracker || ai is FriendTracker.IHaveFriendTracker;
        }


        //code from Pokéballs
        public static void TameCreature(RainWorldGame game, PhysicalObject obj)
        {
            if (!(obj is Creature) || game.world == null)
                return;

            AbstractCreature player = game?.FirstAlivePlayer;
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
                if (rel?.know < 1f)
                    rel.InfluenceKnow(0.9f); //max possible influence
                if (rel != null) {
                    if (Options.tameIncreasesRep?.Value == true)
                        game.session?.creatureCommunities?.InfluenceLikeOfPlayer(
                            creature.creatureTemplate.communityID, 
                            game.world.RegionNumber, 
                            (player.state as PlayerState).playerNumber, 
                            0.03f, 0.15f, 0.2f
                        );
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", SocialMemory.Relationship data set: like=" + rel.like + ", tempLike=" + rel.tempLike + ", know=" + rel.know);
                }
            }

            if (ai is FriendTracker.IHaveFriendTracker && ai.friendTracker != null) {
                SocialMemory.Relationship rel = creature.state?.socialMemory?.GetOrInitiateRelationship(player.ID);
                bool somethingSet = false;
                if (ai.friendTracker.friend != player.realizedCreature) {
                    ai.friendTracker.friend = player.realizedCreature;
                    somethingSet = true;
                }
                if (ai.friendTracker.friendRel != rel) {
                    ai.friendTracker.friendRel = rel;
                    somethingSet = true;
                }
                if (Options.logDebug?.Value != false && somethingSet)
                    Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", FriendTracker data set with creature: " + Special.ConsistentName(player));
            }

            //cancel friendly attempted bites
            if (ai is LizardAI) {
                if ((ai as LizardAI).casualAggressionTarget?.representedCreature == player) {
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", removed LizardAI.casualAggressionTarget for creature: " + Special.ConsistentName((ai as LizardAI).casualAggressionTarget.representedCreature));
                    (ai as LizardAI).casualAggressionTarget = null; //NOTE: it is possible that this tracker is restored in a future update tick
                }
                if ((ai as LizardAI).lizard?.lizardParams != null && (ai as LizardAI).lizard.lizardParams.attemptBiteRadius > 0f && Options.cheatTameLizards?.Value == true) {
                    (ai as LizardAI).lizard.lizardParams.attemptBiteRadius = 0f;
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", set LizardBreedParams.attemptBiteRadius to 0f");
                }
            }

            //remove trackers for this creature (if they are not removed automatically already)
            foreach (AIModule module in ai.modules) {
                if (module is ThreatTracker) {
                    foreach (ThreatTracker.ThreatCreature t in (module as ThreatTracker).threatCreatures.Where(c => c?.creature?.representedCreature == player)) {
                        if (t.creature.deleteMeNextFrame)
                            continue;
                        t.creature.Destroy();
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", destroyed ThreatTracker for creature: " + Special.ConsistentName(player));
                    }
                }
                if (module is PreyTracker) {
                    foreach (PreyTracker.TrackedPrey t in (module as PreyTracker).prey.Where(c => c?.critRep?.representedCreature == player)) {
                        if (t.critRep.deleteMeNextFrame)
                            continue;
                        t.critRep.Destroy();
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", destroyed TrackedPrey for creature: " + Special.ConsistentName(player));
                    }
                }
                if (module is AgressionTracker) {
                    foreach (AgressionTracker.AngerRelationship t in (module as AgressionTracker).creatures.Where(c => c?.crit?.representedCreature == player)) {
                        if (t.crit.deleteMeNextFrame)
                            continue;
                        t.crit.Destroy();
                        if (Options.logDebug?.Value != false)
                            Plugin.Logger.LogDebug("TameCreature, " + Special.ConsistentName(creature) + ", destroyed AgressionTracker for creature: " + Special.ConsistentName(player));
                    }
                }
            }
        }


        //tame all creatures in room
        public static void TameCreatures(RainWorldGame game, Room room)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("TameCreatures");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i]?.Count; j++)
                    TameCreature(game, room.physicalObjects[i][j]);
        }


        //clear relationships of creature
        public static void ClearRelationships(PhysicalObject obj)
        {
            if (!(obj is Creature))
                return;

            AbstractCreature creature = (obj as Creature).abstractCreature;
            ArtificialIntelligence ai = creature?.abstractAI?.RealAI;

            if (ai is IUseARelationshipTracker) {
                int count = creature.state?.socialMemory?.relationShips?.Count ?? 0;
                creature.state?.socialMemory?.relationShips?.Clear();
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("ClearRelationships, " + Special.ConsistentName(creature) + ", cleared " + count + " relationship" + (count == 1 ? "" : "s"));
            }

            if (ai is FriendTracker.IHaveFriendTracker && ai.friendTracker != null) {
                bool somethingReset = ai.friendTracker.friend != null || ai.friendTracker.friendRel != null;
                if (Options.logDebug?.Value != false && somethingReset)
                    Plugin.Logger.LogDebug("ClearRelationships, " + Special.ConsistentName(creature) + ", FriendTracker data reset with creature: " + Special.ConsistentName(ai.friendTracker.friend?.abstractCreature));
                ai.friendTracker.friend = null;
                ai.friendTracker.friendRel = null;
            }
        }


        //clear all relationships of all creatures in room
        public static void ClearRelationships(Room room)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ClearRelationships");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i]?.Count; j++)
                    if (!(room.physicalObjects[i][j] is Player && //don't clear when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        ClearRelationships(room.physicalObjects[i][j]);
        }
    }
}
