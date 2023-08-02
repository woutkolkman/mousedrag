using System.Collections.Generic;

namespace MouseDrag
{
    static partial class Tools
    {
        private static List<PhysicalObject> pausedObjects = new List<PhysicalObject>();
        public static bool pauseAllCreatures = false;


        public static bool IsObjectPaused(UpdatableAndDeletable uad)
        {
            if (!(uad is PhysicalObject))
                return false;
            bool shouldPause = pausedObjects.Contains(uad as PhysicalObject);

            if (uad is Creature) {
                shouldPause |= (pauseAllCreatures && !( //don't pause when: creature is player and player is not SlugNPC (optional)
                    uad is Player && (Options.exceptSlugNPC?.Value != false || !(uad as Player).isNPC)
                ));

                if (shouldPause && Options.releaseGraspsPaused?.Value != false)
                    ReleaseAllGrasps(uad as Creature);
            }

            //update physicalobject at least once
            if (shouldPause && (uad as PhysicalObject).abstractPhysicalObject?.timeSpentHere == 0) {
                (uad as PhysicalObject).abstractPhysicalObject.timeSpentHere++;
                Plugin.Logger.LogDebug("IsObjectPaused, updating object " + (uad as PhysicalObject).abstractPhysicalObject.ID.ToString() + " once before pausing");
                return false;
            }
            return shouldPause;
        }


        public static void TogglePauseObject()
        {
            if (!(dragChunk?.owner is PhysicalObject))
                return;
            PhysicalObject c = dragChunk.owner as PhysicalObject;

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
            Plugin.Logger.LogDebug("UnpauseAll");
        }


        public static void PauseObjects(Room room, bool onlyCreatures)
        {
            Plugin.Logger.LogDebug("PauseObjects, pause " + (onlyCreatures ? "creatures" : "objects") + " in room");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature || !onlyCreatures))
                        if (!(room.physicalObjects[i][j] is Player && //don't pause when: creature is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                            if (!pausedObjects.Contains(room.physicalObjects[i][j]))
                                pausedObjects.Add(room.physicalObjects[i][j]);
        }
    }
}
