using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Runtime.CompilerServices;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    class Hooks
    {
        public static void Apply()
        {
            //at tickrate
            On.RainWorldGame.Update += RainWorldGameUpdateHook;
        }


        public static void Unapply()
        {
			//TODO
        }


        //at tickrate
        private static BodyChunk dragChunk;
        private static Vector2 dragOffset;
        static void RainWorldGameUpdateHook(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);

            bool stop = false;
            Vector2 mousePos = (Vector2)Input.mousePosition + self.cameras[0]?.pos ?? new Vector2();

            //game is paused
            if (self.GamePaused || self.pauseUpdate || !self.processActive || self.pauseMenu != null)
                stop = true;

            //only active when dev tools is active, not active in arena
            if (!self.devToolsActive || self.IsArenaSession)
                stop = true;

            //room unavailable
            Room room = self.RealizedPlayerFollowedByCamera?.room;
            if (room?.physicalObjects == null)
                stop = true;

            //left mouse button not pressed
            if (!Input.GetMouseButton(0))
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
            if (dragChunk == null) {
                for (int i = 0; i < room.physicalObjects.Length; i++)
                    for (int j = 0; j < room.physicalObjects[i].Count; j++)
                        closestChunk(room.physicalObjects[i][j]);

            //drag this chunk
            } else {
                if (ShouldRelease(dragChunk.owner)) {
                    dragChunk = null;
                } else {
                    dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
                    dragChunk.pos += mousePos + dragOffset - dragChunk.pos;
                }
            }
        }
    }
}
