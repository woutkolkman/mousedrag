using System.Collections.Generic;

namespace MouseDrag
{
    public static class Pause
    {
        public static List<AbstractPhysicalObject> pausedObjects = new List<AbstractPhysicalObject>();
        public static bool pauseAllCreatures = false;
        public static bool pauseAllObjects = false;


        public static bool IsObjectPaused(AbstractWorldEntity awe)
        {
            if (!(awe is AbstractPhysicalObject))
                return false;
            return IsObjectPaused(awe as AbstractPhysicalObject);
        }


        public static bool IsObjectPaused(UpdatableAndDeletable uad)
        {
            if ((uad as PhysicalObject)?.abstractPhysicalObject == null)
                return false;
            return IsObjectPaused((uad as PhysicalObject).abstractPhysicalObject);
        }


        public static bool IsObjectPaused(AbstractPhysicalObject apo)
        {
            if (apo == null)
                return false;
            bool shouldPause = pausedObjects.Contains(apo);

            if (apo.realizedObject is Creature) {
                shouldPause |= pauseAllCreatures && !( //don't pause when: creature is player and player is not SlugNPC (optional)
                    apo.realizedObject is Player && (
                        Options.exceptSlugNPC?.Value != false || 
                        !(apo.realizedObject as Player).isNPC
                    )
                );

                if (shouldPause && Options.releaseGraspsPaused?.Value != false)
                    Destroy.ReleaseAllGrasps(apo.realizedObject as Creature);
            } else {
                shouldPause |= pauseAllObjects;
            }

            //update physicalobject at least once
            if (shouldPause && apo.timeSpentHere == 0) {
                apo.timeSpentHere++;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("IsObjectPaused, updating object " + apo.ID.ToString() + " once before pausing");
                return false;
            }
            return shouldPause;
        }


        public static void TogglePauseObject(PhysicalObject obj)
        {
            if (obj?.abstractPhysicalObject == null)
                return;
            AbstractPhysicalObject c = obj.abstractPhysicalObject;

            if (pausedObjects.Contains(c)) {
                pausedObjects.Remove(c);
            } else {
                pausedObjects.Add(c);
            }
        }


        public static void UnpauseAll()
        {
            pausedObjects.Clear();
            pauseAllCreatures = false;
            pauseAllObjects = false;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("UnpauseAll");
        }


        public static void PauseObjects(Room room, bool onlyCreatures)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("PauseObjects, pause " + (onlyCreatures ? "creatures" : "objects") + " in room");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature || !onlyCreatures))
                        if (room.physicalObjects[i][j]?.abstractPhysicalObject != null && //null safety check
                            !(room.physicalObjects[i][j] is Player && //don't pause when: creature is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                            if (!pausedObjects.Contains(room.physicalObjects[i][j].abstractPhysicalObject))
                                pausedObjects.Add(room.physicalObjects[i][j].abstractPhysicalObject);
        }
    }
}
