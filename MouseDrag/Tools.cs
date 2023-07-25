using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    static class Tools
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static bool activated = false; //true --> all tools are available
        private static BodyChunk dragChunk;
        private static Vector2 dragOffset;


        private static bool prevActivated = false, prevPaused = true;
        public static void UpdateActivated(RainWorldGame game)
        {
            bool paused = (game.GamePaused || game.pauseUpdate || !game.processActive || game.pauseMenu != null);

            //read activeType from config when game is unpaused
            if (!paused && prevPaused && Options.activateType?.Value != null) {
                foreach (Options.ActivateTypes val in Enum.GetValues(typeof(Options.ActivateTypes)))
                    if (String.Equals(Options.activateType.Value, val.ToString()))
                        activeType = val;
                Plugin.Logger.LogDebug("CheckActivated, activeType: " + activeType.ToString());
            }
            prevPaused = paused;

            //set activated controls, key bind is checked in RainWorldGameRawUpdateHook
            if (activeType == Options.ActivateTypes.DevToolsActive)
                activated = game.devToolsActive;
            if (activeType == Options.ActivateTypes.AlwaysActive)
                activated = true;

            //if sandbox is active, always enable (because mouse drag is also active)
            activated |= (game.GetArenaGameSession as SandboxGameSession)?.overlay?.mouseDragger != null;

            if (activated != prevActivated)
                Plugin.Logger.LogDebug("CheckActivated, activated: " + activated);
            prevActivated = activated;
        }


        public static void DragObject(RainWorldGame game)
        {
            bool stop = false;
            Vector2 mousePos = (Vector2)Input.mousePosition + game.cameras[0]?.pos ?? new Vector2();

            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive || game.pauseMenu != null)
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
                    if (!IsObjectPaused(dragChunk.owner)) //do not launch creature
                        dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
                    dragChunk.pos += mousePos + dragOffset - dragChunk.pos;
                    if (Options.updateLastPos?.Value != false)
                        dragChunk.lastPos = dragChunk.pos; //reduces visual bugs
                }
            }
        }


        public static void DeleteObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;

            ReleaseAllGrasps(obj);

            obj?.RemoveFromRoom();
            obj?.abstractPhysicalObject?.Room?.RemoveEntity(obj.abstractPhysicalObject); //prevent realizing after hibernation

            if (!(obj is Player)) //Jolly Co-op's Destoy kills player
                obj?.Destroy(); //prevent realizing after hibernation
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


        //delete all objects from room
        public static void DeleteObjects(Room room, bool onlyCreatures)
        {
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature || !onlyCreatures) && 
                        !(room.physicalObjects[i][j] is Player))
                        DeleteObject(room.physicalObjects[i][j]);
        }


        //pause/unpause objects
        private static List<PhysicalObject> pausedObjects = new List<PhysicalObject>();
        public static bool pauseAllCreatures = false;
        public static bool IsObjectPaused(UpdatableAndDeletable uad)
        {
            if (!(uad is PhysicalObject))
                return false;
            bool shouldPause = pausedObjects.Contains(uad as PhysicalObject);

            if (uad is Creature) {
                shouldPause |= (pauseAllCreatures && !(uad is Player));

                if (shouldPause && Options.releaseGraspsPaused?.Value != false)
                    ReleaseAllGrasps(uad as Creature);
            }
            return shouldPause;
        }
        public static void TogglePauseObject()
        {
            if (!(dragChunk?.owner is PhysicalObject))
                return;
            PhysicalObject c = dragChunk.owner as PhysicalObject;

            if (pausedObjects.Contains(c)) {
                pausedObjects.Remove(c);
            } else {
                pausedObjects.Add(c);
            }
        }
        public static void UnpauseAll()
        {
            pausedObjects.Clear();
            pauseAllCreatures = false;
        }
    }
}
