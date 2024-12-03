using System.Collections.Generic;

namespace MouseDrag
{
    public static class Stun
    {
        public static List<AbstractPhysicalObject> stunnedObjects = new List<AbstractPhysicalObject>();
        public static bool stunAll = false;


        public static void UpdateObjectStunned(UpdatableAndDeletable uad)
        {
            if (!IsObjectStunned(uad))
                return;

            if (uad is Oracle)
                (uad as Oracle).stun = 40;

            if (uad is Creature)
                (uad as Creature).stun = UnityEngine.Mathf.Max((uad as Creature).stun, 40);
//                (uad as Creature).Stun(40);

            //DLL cannot be stunned, so deafen those
            if (uad is DaddyLongLegs && (uad as DaddyLongLegs).deaf < 100)
                (uad as DaddyLongLegs).Deafen(10);
        }


        public static bool IsObjectStunned(UpdatableAndDeletable uad)
        {
            if ((uad as PhysicalObject)?.abstractPhysicalObject == null)
                return false;
            if (!(uad is Oracle) && !(uad is Creature))
                return false;

            bool shouldStun = stunnedObjects.Contains((uad as PhysicalObject).abstractPhysicalObject);

            shouldStun |= stunAll && !( //not stunned when: creature is player and player is not SlugNPC (optional)
                uad is Player && (Options.exceptSlugNPC?.Value != false || !(uad as Player).isNPC)
            );

            return shouldStun;
        }


        public static void StunObject(AbstractPhysicalObject apo, bool toggle, bool apply)
        {
            if (apo?.type == null)
                return;
            if (apo.type != AbstractPhysicalObject.AbstractObjectType.Oracle && 
                apo.type != AbstractPhysicalObject.AbstractObjectType.Creature)
                return;

            bool contains = stunnedObjects.Contains(apo);
            if (contains)
                if (toggle || (!toggle && !apply))
                    stunnedObjects.Remove(apo);
            if (!contains)
                if (toggle || (!toggle && apply))
                    stunnedObjects.Add(apo);
        }


        public static void UnstunAll()
        {
            stunnedObjects.Clear();
            stunAll = false;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("UnstunAll");
        }


        public static void StunObjects(Room room)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("StunObjects, stun all in room");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Oracle) || (room.physicalObjects[i][j] is Creature))
                        if (room.physicalObjects[i][j]?.abstractPhysicalObject != null && //null safety check
                            !(room.physicalObjects[i][j] is Player && //don't stun when: object is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                            if (!stunnedObjects.Contains(room.physicalObjects[i][j].abstractPhysicalObject))
                                stunnedObjects.Add(room.physicalObjects[i][j].abstractPhysicalObject);
        }
    }
}
