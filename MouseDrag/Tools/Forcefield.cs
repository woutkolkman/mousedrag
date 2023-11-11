using System.Collections.Generic;
using RWCustom;

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
                    if (Options.forcefieldImmunityObjects?.Value != false && !(po is Creature) && //creatures aren't immune
                        !(po is Weapon && (po as Weapon).mode == Weapon.Mode.Thrown)) //thrown weapons aren't immune
                        continue;
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
