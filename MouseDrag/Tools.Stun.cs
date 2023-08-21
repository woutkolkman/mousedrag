using System.Collections.Generic;

namespace MouseDrag
{
    static partial class Tools
    {
        private static List<PhysicalObject> stunnedObjects = new List<PhysicalObject>();
        public static bool stunAll = false;


        public static void UpdateObjectStunned(UpdatableAndDeletable uad)
        {
            if (!IsObjectStunned(uad))
                return;

            if (uad is Oracle)
                (uad as Oracle).stun = 40;

            if (uad is Creature)
                (uad as Creature).Stun(40);

            //DLL cannot be stunned, so deafen those
            if (uad is DaddyLongLegs)
                (uad as DaddyLongLegs).Deafen(20);
        }


        public static bool IsObjectStunned(UpdatableAndDeletable uad)
        {
            if (!(uad is PhysicalObject))
                return false;
            if (!(uad is Oracle) && !(uad is Creature))
                return false;

            bool shouldStun = stunnedObjects.Contains(uad as PhysicalObject);

            shouldStun |= stunAll && !( //not stunned when: creature is player and player is not SlugNPC (optional)
                uad is Player && (Options.exceptSlugNPC?.Value != false || !(uad as Player).isNPC)
            );

            return shouldStun;
        }


        public static void ToggleStunObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (!(obj is PhysicalObject))
                return;
            PhysicalObject c = obj as PhysicalObject;
            if (!(c is Oracle) && !(c is Creature))
                return;

            if (stunnedObjects.Contains(c)) {
                stunnedObjects.Remove(c);
            } else {
                stunnedObjects.Add(c);
            }
        }


        public static void UnstunAll()
        {
            stunnedObjects.Clear();
            stunAll = false;
            Plugin.Logger.LogDebug("UnstunAll");
        }


        public static void StunObjects(Room room)
        {
            Plugin.Logger.LogDebug("StunObjects, stun all in room");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Oracle) || (room.physicalObjects[i][j] is Creature))
                        if (!(room.physicalObjects[i][j] is Player && //don't stun when: object is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                            if (!stunnedObjects.Contains(room.physicalObjects[i][j]))
                                stunnedObjects.Add(room.physicalObjects[i][j]);
        }
    }
}
