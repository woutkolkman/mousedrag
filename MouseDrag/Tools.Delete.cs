namespace MouseDrag
{
    static partial class Tools
    {
        public static void DeleteObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;

            ReleaseAllGrasps(obj);

            if (obj is Oracle) //prevent loitering sprites
                obj.Destroy();

            obj?.RemoveFromRoom();
            obj?.abstractPhysicalObject?.Room?.RemoveEntity(obj.abstractPhysicalObject); //prevent realizing after hibernation

            if (!(obj is Player)) //Jolly Co-op's Destoy kills player
                obj?.Destroy(); //prevent realizing after hibernation
        }


        //delete all objects from room
        public static void DeleteObjects(Room room, bool onlyCreatures)
        {
            Plugin.Logger.LogDebug("DeleteObjects, delete " + (onlyCreatures ? "creatures" : "objects") + " in room");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature || !onlyCreatures))
                        if (!(room.physicalObjects[i][j] is Player && //don't delete when: creature is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                            DeleteObject(room.physicalObjects[i][j]);
        }
    }
}
