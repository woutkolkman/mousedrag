using UnityEngine;

namespace MouseDrag
{
    public static class Teleport
    {
        public static void TeleportObjects(RainWorldGame game, Room room, bool creatures, bool objects, Vector2? pos = null)
        {
            if (pos == null)
                pos = (Vector2)Futile.mousePosition + game.cameras[0]?.pos ?? new Vector2();

            void SetPosition(PhysicalObject obj, Vector2 newPos)
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

            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if (!(room.physicalObjects[i][j] is Player && //don't teleport when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        if ((room.physicalObjects[i][j] is Creature && creatures) ||
                            (!(room.physicalObjects[i][j] is Creature) && objects))
                            SetPosition(room.physicalObjects[i][j], pos.Value);
        }
    }
}
