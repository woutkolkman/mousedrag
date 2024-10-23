using System.Collections.Generic;

namespace MouseDrag
{
    public static class Forcefield
    {
        public static List<BodyChunk> forcefieldChunks = new List<BodyChunk>();


        public static void UpdateForcefield(BodyChunk bodyChunk)
        {
            if (bodyChunk?.owner == null || !forcefieldChunks.Contains(bodyChunk))
                return;

            for (int i = 0; i < bodyChunk.owner?.room?.physicalObjects?.Length; i++) {
                foreach (PhysicalObject po in bodyChunk.owner.room.physicalObjects[i]) {
                    if (po == bodyChunk.owner || po == null)
                        continue;
                    if (Options.forcefieldImmunityPlayers?.Value != false && po is Player)
                        continue;
                    if (Options.forcefieldImmunityItems?.Value != false) {
                        if (po is Weapon) {
                            //not immune when ignited
                            if (po is ScavengerBomb && (po as ScavengerBomb).ignited) {
                            } else if (po is ExplosiveSpear && (po as ExplosiveSpear).Ignited) {
                            } else if (po is FirecrackerPlant && (po as FirecrackerPlant).fuseCounter > 0) {
                            } else if (po is MoreSlugcats.SingularityBomb && (po as MoreSlugcats.SingularityBomb).ignited) {
                            } else if (po is MoreSlugcats.FireEgg && (po as MoreSlugcats.FireEgg).activeCounter > 0) {

                            //free weapons are immune
                            } else if ((po as Weapon).mode != Weapon.Mode.Thrown) {
                                continue;
                            }

                            //weapons thrown by creature who threw them are immune
                            if ((po as Weapon).thrownBy == bodyChunk.owner)
                                continue;

                        } else if (!(po is Creature)) {
                            //any non-creature object is immune
                            continue;
                        }

                        //creature that grabs this object is immune (more fun when disabled)
                        //for (int j = 0; j < bodyChunk.owner.grabbedBy?.Count; j++)
                        //    if (bodyChunk.owner.grabbedBy[j]?.grabbed == po)
                        //        continue;
                    }
                    po.PushOutOf(bodyChunk.pos, Options.forcefieldRadius?.Value ?? 120f, -1);
                }
            }
        }


        public static bool HasForcefield(BodyChunk bodyChunk)
        {
            if (bodyChunk == null)
                return false;
            return forcefieldChunks.Contains(bodyChunk);
        }


        public static void ToggleForcefield(BodyChunk bodyChunk)
        {
            if (bodyChunk?.owner == null)
                return;

            if (forcefieldChunks.Contains(bodyChunk)) {
                forcefieldChunks.Remove(bodyChunk);
            } else {
                forcefieldChunks.Add(bodyChunk);
            }
        }


        public static void ClearForcefields()
        {
            forcefieldChunks.Clear();
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ClearForcefields");
        }
    }
}
