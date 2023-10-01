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
            room.AddObject(new Crosshair(room, pos));
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


        public class Crosshair : UpdatableAndDeletable, IDrawable
        {
            public Vector2 curPos, prevPos;


            public Crosshair(Room room, Vector2 pos)
            {
                this.room = room;
                prevPos = pos;
                curPos = pos;
            }
            ~Crosshair() { Destroy(); }


            public override void Update(bool eu)
            {
                base.Update(eu);
                prevPos = curPos;
                if (Teleport.room != this.room)
                    this.Destroy();
            }


            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("mousedragCrosshair", true);
                this.AddToContainer(sLeaser, rCam, null);
            }


            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                if (slatedForDeletetion) {
                    sLeaser?.CleanSpritesAndRemove();
                    return;
                }
                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker);
                sLeaser.sprites[0].x = tsPos.x - camPos.x;
                sLeaser.sprites[0].y = tsPos.y - camPos.y;
            }


            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
            {
                if (newContainer == null)
                    newContainer = rCam.ReturnFContainer("HUD");
                newContainer.AddChild(sLeaser.sprites[0]);
            }


            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
            }
        }
    }
}
