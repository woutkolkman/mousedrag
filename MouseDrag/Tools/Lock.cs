using System.Collections.Generic;
using UnityEngine;

namespace MouseDrag
{
    public static class Lock
    {
        public static List<KeyValuePair<BodyChunk, Vector2>> bodyChunks = new List<KeyValuePair<BodyChunk, Vector2>>();
        public static bool hoversOverSlot = false, showSprites = false;


        public static KeyValuePair<BodyChunk, Vector2>? ListContains(BodyChunk bc)
        {
            if (bc == null)
                return null;
            foreach (var pair in bodyChunks)
                if (pair.Key == bc)
                    return pair;
            return null;
        }


        public static void SetLock(BodyChunk bc, bool toggle, bool apply)
        {
            if (bc == null)
                return;

            bool contains = ListContains(bc) != null;
            if (contains)
                if (toggle || (!toggle && !apply))
                    ListRemove(bc);
            if (!contains) {
                if (toggle || (!toggle && apply)) {
                    bodyChunks.Add(new KeyValuePair<BodyChunk, Vector2>(bc, bc.pos));
                    LockSprite ls = new LockSprite(bc);
                    bc.owner?.room?.AddObject(ls);
                }
            }
        }


        public static bool ListRemove(BodyChunk bc)
        {
            for (int i = 0; i < bodyChunks.Count; i++) {
                if (bodyChunks[i].Key == bc) {
                    bodyChunks.Remove(bodyChunks[i]);
                    return true;
                }
            }
            return false;
        }


        public static void UpdatePosition(BodyChunk bc)
        {
            var pair = ListContains(bc);
            bool dragging = false;
            foreach (var draggable in Drag.dragChunks)
                if (draggable.chunk == bc)
                    dragging = true;
            if (pair == null || bc == null || dragging)
                return;
            bc.pos = pair.Value.Value;
            bc.vel = Vector2.zero;
        }


        public static void ResetLock(BodyChunk bc)
        {
            if (bc == null)
                return;
            if (ListRemove(bc))
                SetLock(bc, toggle: false, apply: true);
        }


        public class LockSprite : UpdatableAndDeletable, IDrawable
        {
            //NOTE: this object requires that the previous room of the bodychunk stays loaded after the followed bodychunk 
            //moved to a new room, so this object is able to move itself to the new room when this object is updated


            public Vector2 curPos, prevPos;
            public BodyChunk followChunk;
            private bool visible;


            public LockSprite(BodyChunk bc)
            {
                room = bc?.owner?.room;
                prevPos = bc?.pos ?? new Vector2();
                curPos = bc?.pos ?? new Vector2();
                followChunk = bc;
            }
            ~LockSprite() { Destroy(); }


            public override void Update(bool eu)
            {
                base.Update(eu);
                if (ListContains(followChunk) == null || followChunk?.owner?.slatedForDeletetion != false) {
                    followChunk = null;
                    Destroy();
                    return;
                }
                if (followChunk.owner?.room != null && followChunk.owner.room != room) {
                    RemoveFromRoom();
                    room = followChunk.owner.room;
                    room.AddObject(this);
                    curPos = followChunk.pos; //prevent sprite shooting across screen
                }
                visible = (showSprites || hoversOverSlot)
                    && followChunk.owner?.room != null
                    && Drag.MouseCamera(room?.game)?.room == room;
                prevPos = curPos;
                if (!Drag.ShouldRelease(followChunk?.owner))
                    curPos = followChunk.pos;
            }


            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.RemoveAllSpritesFromContainer();
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("mousedragLocked", true);
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
                        Plugin.Logger.LogError("Lock.LockSprite.DrawSprites exception while reading SBCameraScroll, integration is now disabled");
                        Integration.sBCameraScrollEnabled = false;
                        throw; //throw original exception while preserving stack trace
                    }
                }

                sLeaser.sprites[0].x = tsPos.x;
                sLeaser.sprites[0].y = tsPos.y;
                sLeaser.sprites[0].isVisible = visible;
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
