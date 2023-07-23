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

            Vector2 mousePos = (Vector2)Input.mousePosition + self.cameras[0]?.pos ?? new Vector2();

            //get chunk to drag
            if (dragChunk == null) {
                float dist = float.MaxValue;

                for (int i = 0; i < room.physicalObjects.Length; i++) {
                    for (int j = 0; j < room.physicalObjects[i].Count; j++) {
                        PhysicalObject obj = room.physicalObjects[i][j];

                        //object is being deleted
                        if (obj?.bodyChunks == null || obj.slatedForDeletetion)
                            continue;

                        //object is a creature, and is entering a shortcut
                        if (obj is Creature && (obj as Creature).enteringShortCut != null)
                            continue;

                        for (int k = 0; k < obj.bodyChunks.Length; k++)
                        {
                            if (Custom.DistLess(mousePos, obj.bodyChunks[k].pos, Mathf.Min(obj.bodyChunks[k].rad + 10f, dist)))
                            {
                                dist = Vector2.Distance(mousePos, room.physicalObjects[i][j].bodyChunks[k].pos);
                                dragChunk = room.physicalObjects[i][j].bodyChunks[k];
                                dragOffset = dragChunk.pos - mousePos;
                            }
                        }
                    }
                }

            //drag chunk
            } else {
                if (dragChunk.owner?.slatedForDeletetion != false ||
                    (dragChunk.owner is Creature && (dragChunk.owner as Creature).enteringShortCut != null)) {
                    dragChunk = null;
                } else {
                    dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
                    dragChunk.pos += mousePos + dragOffset - dragChunk.pos;
                }
            }
        }
    }
}
