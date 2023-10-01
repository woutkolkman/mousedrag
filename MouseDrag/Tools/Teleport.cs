using UnityEngine;

namespace MouseDrag
{
    public static class Teleport
    {
        public static void SetObjectPosition(PhysicalObject obj, Vector2 newPos)
        {
            if (!(obj is Creature))
                Destroy.ReleaseAllGrasps(obj);
            for (int i = 0; i < obj?.bodyChunks?.Length; i++) {
                if (obj.bodyChunks[i] == null)
                    continue;
                obj.bodyChunks[i].HardSetPosition(newPos);
                obj.bodyChunks[i].vel = new Vector2();
            }
        }


        public static Room room; //if not null, waypoint is set
        public static Vector2 pos;
        public static void SetWaypoint(Room room, Vector2 pos)
        {
            //remove waypoint if previously set
            if (Teleport.room != null || room == null) {
                Teleport.room = null;
                return;
            }
            Teleport.room = room;
            Teleport.pos = pos;
        }


        //returns true if object is teleported
        public static bool UpdateTeleportObject(PhysicalObject obj)
        {
            if (room == null)
                return false;

            //remove waypoint if dragging in another room
            if (room != obj?.room) {
                room = null;
                return false;
            }

            SetObjectPosition(obj, pos);
            return true;
        }


        public static void TeleportObjects(RainWorldGame game, Room room, bool creatures, bool objects, Vector2? pos = null)
        {
            if (pos == null)
                pos = (Vector2)Futile.mousePosition + game.cameras[0]?.pos ?? new Vector2();

            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if (!(room.physicalObjects[i][j] is Player && //don't teleport when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        if ((room.physicalObjects[i][j] is Creature && creatures) ||
                            (!(room.physicalObjects[i][j] is Creature) && objects))
                            SetObjectPosition(room.physicalObjects[i][j], pos.Value);
        }
    }
}
