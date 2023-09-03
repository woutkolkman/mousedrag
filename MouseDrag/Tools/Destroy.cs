namespace MouseDrag
{
    public static class Destroy
    {
        public static void DestroyObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = Drag.dragChunk?.owner;

            Destroy.ReleaseAllGrasps(obj);

            if (obj is Oracle) //prevent loitering sprites
                obj.Destroy();

            //prevent beehive crashing game
            if (obj is SporePlant && (obj as SporePlant).stalk != null) {
                (obj as SporePlant).stalk.sporePlant = null;
                (obj as SporePlant).stalk = null;
            }

            if (obj is Spear) //prevent spear leaving invisible beams behind
                (obj as Spear).resetHorizontalBeamState();

            obj?.RemoveFromRoom();
            obj?.abstractPhysicalObject?.Room?.RemoveEntity(obj.abstractPhysicalObject); //prevent realizing after hibernation

            if (!(obj is Player)) //Jolly Co-op's Destoy kills player
                obj?.Destroy(); //prevent realizing after hibernation
        }


        //destroy all objects in room
        public static void DestroyObjects(Room room, bool onlyCreatures)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DestroyObjects, destroy " + (onlyCreatures ? "creatures" : "objects") + " in room");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature || !onlyCreatures))
                        if (!(room.physicalObjects[i][j] is Player && //don't destroy when: creature is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                            DestroyObject(room.physicalObjects[i][j]);
        }


        public static void ReleaseAllGrasps(PhysicalObject obj)
        {
            if (obj?.grabbedBy != null)
                for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
                    obj.grabbedBy[i]?.Release();

            if (obj is Creature) {
                if (obj is Player) {
                    //drop slugcats
                    (obj as Player).slugOnBack?.DropSlug();
                    (obj as Player).onBack?.slugOnBack?.DropSlug();
                    (obj as Player).slugOnBack = null;
                    (obj as Player).onBack = null;
                }

                (obj as Creature).LoseAllGrasps();
            }
        }
    }
}
