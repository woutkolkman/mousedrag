using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    static partial class Tools
    {
        public static BodyChunk dragChunk; //owner is reference to the physicalobject which is dragged
        public static Vector2 dragOffset;


        public static void DragObject(RainWorldGame game)
        {
            bool stop = false;
            Vector2 mousePos = (Vector2)Futile.mousePosition + game.cameras[0]?.pos ?? new Vector2();

            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                stop = true;

            //room unavailable
            Room room = game.cameras[0]?.room;
            if (room?.physicalObjects == null)
                stop = true;

            //left mouse button not pressed
            if (!Input.GetMouseButton(0))
                stop = true;

            //dragchunk not in this room
            if (dragChunk?.owner?.room != null && dragChunk.owner.room != room)
                stop = true;

            if (stop) {
                dragChunk = null;
                return;
            }

            //object should not be dragged any longer
            bool ShouldRelease(PhysicalObject obj)
            {
                //object is being deleted
                if (obj?.bodyChunks == null || obj.slatedForDeletetion)
                    return true;

                //object is a creature, and is entering a shortcut
                if (obj is Creature && (obj as Creature).enteringShortCut != null)
                    return true;
                return false;
            }

            //check object chunks for closest to mousepointer
            float closest = float.MaxValue;
            void closestChunk(PhysicalObject obj)
            {
                if (ShouldRelease(obj))
                    return;

                for (int k = 0; k < obj.bodyChunks.Length; k++)
                {
                    if (!Custom.DistLess(mousePos, obj.bodyChunks[k].pos, Mathf.Min(obj.bodyChunks[k].rad + 10f, closest)))
                        continue;
                    closest = Vector2.Distance(mousePos, obj.bodyChunks[k].pos);
                    dragChunk = obj.bodyChunks[k];
                    dragOffset = dragChunk.pos - mousePos;
                }
            }

            //search all objects for closest chunk
            if (dragChunk == null)
                for (int i = 0; i < room.physicalObjects.Length; i++)
                    for (int j = 0; j < room.physicalObjects[i].Count; j++)
                        closestChunk(room.physicalObjects[i][j]);

            //drag this chunk
            if (dragChunk != null) {
                if (ShouldRelease(dragChunk.owner)) {
                    dragChunk = null;
                } else { //might affect sandbox mouse
                    dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
                    if (IsObjectPaused(dragChunk.owner)) //do not launch creature after pause
                        dragChunk.vel = new Vector2();
                    dragChunk.pos += mousePos + dragOffset - dragChunk.pos;
                    if (Options.updateLastPos?.Value != false)
                        dragChunk.lastPos = dragChunk.pos; //reduces visual bugs
                }
            }
        }


        public static void ReleaseAllGrasps(PhysicalObject obj)
        {
            if (obj?.grabbedBy != null)
                for (int i = obj.grabbedBy.Count - 1; i >= 0; i--)
                    obj.grabbedBy[i]?.Release();

            if (obj is Creature)
            {
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
