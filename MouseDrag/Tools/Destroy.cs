namespace MouseDrag
{
    public static class Destroy
    {
        public static void DestroyObject(PhysicalObject obj)
        {
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
        public static void DestroyObjects(Room room, bool creatures, bool objects, bool onlyDead)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DestroyObjects, destroy in room" + 
                    ": creatures?" + creatures.ToString() + 
                    ", objects?" + objects.ToString() + 
                    ", onlyDead?" + onlyDead.ToString());
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature && creatures && 
                        (!onlyDead || (room.physicalObjects[i][j] as Creature).dead)) || 
                        (!(room.physicalObjects[i][j] is Creature) && objects))
                        if (!(room.physicalObjects[i][j] is Player && //don't destroy when: creature is player and player is not SlugNPC (optional)
                            (Options.exceptSlugNPC?.Value != false || 
                            !(room.physicalObjects[i][j] as Player).isNPC)))
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
                    (obj as Player).spearOnBack?.DropSpear();
                }

                (obj as Creature).LoseAllGrasps();
            }
        }


        //destroy all objects in all rooms in current region
        public static void DestroyRegionObjects(RainWorldGame self, bool creatures, bool objects)
        {
            if (!(self?.world?.abstractRooms?.Length > 0))
                return;

            int totC = 0;
            int totD = 0;

            foreach (AbstractRoom ar in self.world.abstractRooms) {
                if (ar == null)
                    continue;
                int clearedC = 0;
                int clearedD = 0;

                //return true if object is deleted
                bool handleObject(AbstractPhysicalObject ac)
                {
                    if (ac == null)
                        return false;
                    if ((ac is AbstractCreature && !creatures) || (!(ac is AbstractCreature) && !objects))
                        return false;

                    //don't destroy players
                    if ((ac as AbstractCreature)?.creatureTemplate?.type == CreatureTemplate.Type.Slugcat)
                        return false;

                    //don't destroy slugpups (optional)
                    if (Options.exceptSlugNPC?.Value != false && MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC != null && 
                        (ac as AbstractCreature)?.creatureTemplate?.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                        return false;

                    if (ac.realizedObject != null) {
                        DestroyObject(ac.realizedObject);
                    } else {
                        ar.RemoveEntity(ac);
                    }
                    return true;
                }

                for (int i = ar.entities.Count - 1; i >= 0; i--)
                    if (handleObject(ar.entities[i] as AbstractPhysicalObject))
                        clearedC++;
                for (int i = ar.entitiesInDens.Count - 1; i >= 0; i--)
                    if (handleObject(ar.entitiesInDens[i] as AbstractPhysicalObject))
                        clearedD++;

                if (Options.logDebug?.Value != false && (clearedC > 0 || clearedD > 0))
                    Plugin.Logger.LogDebug("DestroyRegionObjects, room: " + ar.name + ", cleared objects not in den: " + clearedC + ", and in den: " + clearedD);
                totC += clearedC;
                totD += clearedD;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DestroyRegionObjects, region: " + self.world.name + ", total cleared objects not in den: " + totC + ", and in den: " + totD);
        }
    }
}
