using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    static class Tools
    {
        public static Options.ActivateTypes activeType = Options.ActivateTypes.DevToolsActive;
        public static bool activated = false; //true --> all tools are available
        public static BodyChunk dragChunk;
        public static Vector2 dragOffset;


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

            //set activated controls, keybind is checked in RainWorldGameRawUpdateHook
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
                    dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
                    if (IsObjectPaused(dragChunk.owner)) //do not launch creature after pause
                        dragChunk.vel = new Vector2();
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


        public static void DuplicateObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (obj?.room?.abstractRoom == null || obj.room.game == null)
                return;

            WorldCoordinate coord = obj.room.GetWorldCoordinate(obj.firstChunk?.pos ?? new Vector2());
            AbstractPhysicalObject oldApo = obj?.abstractPhysicalObject ?? (obj as Creature)?.abstractCreature;
            AbstractPhysicalObject newApo = null;

            if (oldApo == null)
                return;

            if (obj is Creature) {
                EntityID id = Options.copyID?.Value == false ? obj.room.game.GetNewID() : oldApo.ID;
                newApo = new AbstractCreature(oldApo.world, (obj as Creature).Template, null, coord, id);
                (newApo as AbstractCreature).state.LoadFromString(Regex.Split((oldApo as AbstractCreature).state.ToString(), "<cB>"));

                //prevents exception when duplicating player
                if (obj is Player && (obj as Player).playerState != null && !(obj as Player).isNPC) {
                    PlayerState ps = (obj as Player).playerState;
                    (newApo as AbstractCreature).state = new PlayerState(newApo as AbstractCreature, ps.playerNumber, ps.slugcatCharacter, false);
                }

            } else {
                try {
                    newApo = SaveState.AbstractPhysicalObjectFromString(oldApo.world, oldApo.ToString());

                    //specials
                    if (oldApo is SeedCob.AbstractSeedCob) //popcorn plant
                        newApo = DuplicateObjectSeedCob(oldApo, newApo);
                    if (obj is Oracle) //iterator
                        newApo.realizedObject = new Oracle(newApo, obj.room);

                    //must get new id?
                    if (Options.copyID?.Value == false)
                        newApo.ID = obj.room.game.GetNewID();

                } catch (Exception ex) {
                    Plugin.Logger.LogWarning("DuplicateObject exception: " + ex.ToString());
                    return;
                }
                newApo.pos = coord;
            }

            Plugin.Logger.LogDebug("DuplicateObject, AddEntity " + newApo.type + " at " + coord.SaveToString());
            obj.room.abstractRoom.AddEntity(newApo);
            newApo.RealizeInRoom(); //actually places object/creature
        }


        //popcorn plants are not as easy to duplicate because of the way they are added to rooms
        private static AbstractPhysicalObject DuplicateObjectSeedCob(AbstractPhysicalObject oldApo, AbstractPhysicalObject newApo)
        {
            newApo = new SeedCob.AbstractSeedCob(
                oldApo.world, null, oldApo.pos, oldApo.ID,
                oldApo.realizedObject.room.abstractRoom.index, -1, dead: (oldApo as SeedCob.AbstractSeedCob).dead, null
            );
            (newApo as AbstractConsumable).isConsumed = (oldApo as AbstractConsumable).isConsumed;
            oldApo.realizedObject.room.abstractRoom.entities.Add(newApo);
            newApo.Realize();

            string[] duplicateArrayFields = { "seedPositions", "seedsPopped", "leaves" };
            string[] copyFields = { "totalSprites", "stalkSegments", "cobSegments", "placedPos", "rootPos", "rootDir", "cobDir", "stalkLength", "open" };
            FieldInfo[] seedCobObjectFields = oldApo.realizedObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fi in seedCobObjectFields)
            {
                if (duplicateArrayFields.Any(s => fi.Name.Contains(s)))
                    fi.SetValue(newApo.realizedObject, (fi.GetValue(oldApo.realizedObject) as Array).Clone());
                if (copyFields.Any(s => fi.Name.Contains(s)))
                    fi.SetValue(newApo.realizedObject, fi.GetValue(oldApo.realizedObject));
            }
            return newApo;
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


        public static void KillCreature(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (!(obj is Creature))
                return;

            (obj as Creature).Die();
            if ((obj as Creature).abstractCreature?.state is HealthState)
                ((obj as Creature).abstractCreature.state as HealthState).health = 0f;
        }


        //kill all creatures in room
        public static void KillCreatures(Room room)
        {
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature) && 
                        !(room.physicalObjects[i][j] is Player))
                        KillCreature(room.physicalObjects[i][j]);
        }


        public static void ReviveCreature(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (!(obj is Creature))
                return;

            AbstractCreature ac = (obj as Creature).abstractCreature;
            if (ac?.state == null)
                return;

            if (ac.state is HealthState && (ac.state as HealthState).health < 1f)
                (ac.state as HealthState).health = 1f;
            ac.state.alive = true;
            (obj as Creature).dead = false;

            //try to exit game over mode
            if (Options.exitGameOverMode?.Value != false && 
                obj is Player && !(obj as Player).isNPC && 
                obj?.room?.game?.cameras?.Length > 0 && 
                obj.room.game.cameras[0]?.hud?.textPrompt != null)
                obj.room.game.cameras[0].hud.textPrompt.gameOverMode = false;
        }


        //revive all creatures in room
        public static void ReviveCreatures(Room room)
        {
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if ((room.physicalObjects[i][j] is Creature))
                        ReviveCreature(room.physicalObjects[i][j]);
        }
    }
}
