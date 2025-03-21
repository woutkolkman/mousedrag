using System.Collections.Generic;
using UnityEngine;

namespace MouseDrag
{
    public static class Lock
    {
        public static List<KeyValuePair<BodyChunk, Vector2>> bodyChunks = new List<KeyValuePair<BodyChunk, Vector2>>();


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
            if (!contains)
                if (toggle || (!toggle && apply))
                    bodyChunks.Add(new KeyValuePair<BodyChunk, Vector2>(bc, bc.pos));
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
    }
}
