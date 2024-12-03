using System.Collections.Generic;

namespace MouseDrag
{
    public static class Pause
    {
        public static List<AbstractPhysicalObject> pausedObjects = new List<AbstractPhysicalObject>();
        public static bool pauseAllCreatures = false;
        public static bool pauseAllItems = false;


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
                shouldPause |= pauseAllItems;
            }

            //update physicalobject at least once
            if (shouldPause && apo.timeSpentHere == 0) {
                apo.timeSpentHere++;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("IsObjectPaused, updating object " + Special.ConsistentName(apo) + " once before pausing");
                return false;
            }
            return shouldPause;
        }


        public static void TogglePauseObject(PhysicalObject obj)
        {
            TogglePauseObject(obj?.abstractPhysicalObject);
        }


        public static void TogglePauseObject(AbstractPhysicalObject c)
        {
            if (c == null)
                return;
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
            pauseAllItems = false;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("UnpauseAll");
        }


        public static void PauseObjects(Room room, bool creatures, bool items)
        {
            if (Options.logDebug?.Value != false) {
                string logMsg = "PauseObjects, pause";
                if (creatures && !items)
                    logMsg += " creatures";
                if (!creatures && items)
                    logMsg += " items";
                if (creatures && items)
                    logMsg += " objects";
                if (!creatures && !items)
                    logMsg += " none";
                logMsg += " in room";
                Plugin.Logger.LogDebug(logMsg);
            }
            for (int i = 0; i < room?.physicalObjects?.Length; i++) {
                for (int j = 0; j < room.physicalObjects[i].Count; j++) {
                    if (room.physicalObjects[i][j] is Creature) {
                        if (!creatures)
                            continue;
                    } else {
                        if (!items)
                            continue;
                    }
                    if (room.physicalObjects[i][j]?.abstractPhysicalObject == null) //null safety check
                        continue;
                    if (!(room.physicalObjects[i][j] is Player && //don't pause when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        if (!pausedObjects.Contains(room.physicalObjects[i][j].abstractPhysicalObject))
                            pausedObjects.Add(room.physicalObjects[i][j].abstractPhysicalObject);
                }
            }
        }
    }
}
