using UnityEngine;

namespace MouseDrag
{
    public static class Teleport
    {
        public static Crosshair crosshair; //if not null, waypoint is set


        public static void SetWaypoint(Room room, Vector2 pos, BodyChunk bc = null)
        {
            //remove waypoint if previously set
            if (crosshair != null || room == null) {
                crosshair.Destroy();
                crosshair = null;
                return;
            }

            crosshair = new Crosshair(room, pos);
            crosshair.followChunk = bc;
            room.AddObject(crosshair);
        }


        //returns true if object is teleported
        public static bool UpdateTeleportObject(RainWorldGame game)
        {
            //no waypoint is assigned
            if (crosshair?.room == null)
                return false;

            //only run when drag button is pressed
            if (!Drag.dragButtonDown()) //TODO, should be changed (or ||) to new menuSelectButton?
                return false;

            var rcam = Drag.MouseCamera(game);
            Vector2 offs = Vector2.zero;
            BodyChunk chunk = Drag.GetClosestChunk(rcam?.room, Drag.MousePos(game), ref offs);
            PhysicalObject obj = chunk?.owner;

            //obj is not valid
            if (obj?.room == null)
                return false;

            //remove waypoint if dragging in another room
            if (obj.room != crosshair.room) {
                crosshair.Destroy();
                crosshair = null;
                return false;
            }

            SetObjectPosition(obj, crosshair.curPos);
            return true;
        }


        public class Crosshair : UpdatableAndDeletable, IDrawable
        {
            public Vector2 curPos, prevPos;
            public BodyChunk followChunk;


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
                if (!Drag.ShouldRelease(followChunk?.owner) && followChunk?.owner?.room == room)
                    curPos = followChunk.pos;
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
                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker) - camPos;

                if (Integration.sBCameraScrollEnabled) {
                    try {
                        tsPos -= Integration.SBCameraScrollExtraOffset(rCam, tsPos, out float scale) / (1f / scale);
                    } catch {
                        Plugin.Logger.LogError("Teleport.DrawSprites exception while reading SBCameraScroll, integration is now disabled");
                        Integration.sBCameraScrollEnabled = false;
                        throw; //throw original exception while preserving stack trace
                    }
                }

                sLeaser.sprites[0].x = tsPos.x;
                sLeaser.sprites[0].y = tsPos.y;
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


        //teleport all objects via keybind
        public static void TeleportObjects(RainWorldGame game, Room room, bool creatures, bool objects, Vector2? pos = null)
        {
            if (pos == null)
                pos = Drag.MousePos(game);

            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if (!(room.physicalObjects[i][j] is Player && //don't teleport when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        if ((room.physicalObjects[i][j] is Creature && creatures) ||
                            (!(room.physicalObjects[i][j] is Creature) && objects))
                            SetObjectPosition(room.physicalObjects[i][j], pos.Value);
        }


        //move all bodychunks of object to new position
        public static void SetObjectPosition(PhysicalObject obj, Vector2 newPos)
        {
            //creatures release this object
            if (!(obj is Creature))
                Destroy.ReleaseAllGrasps(obj);

            for (int i = 0; i < obj?.bodyChunks?.Length; i++) {
                if (obj.bodyChunks[i] == null)
                    continue;
                obj.bodyChunks[i].pos = newPos;
                obj.bodyChunks[i].lastPos = newPos;
                obj.bodyChunks[i].lastLastPos = newPos;
                obj.bodyChunks[i].vel = new Vector2();

                //allow clipping into terrain
                if (obj is PlayerCarryableItem)
                    (obj as PlayerCarryableItem).lastOutsideTerrainPos = null;
            }
        }
    }
}
