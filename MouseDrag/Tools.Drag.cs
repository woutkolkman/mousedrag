using UnityEngine;
using RWCustom;
using System;
using System.Collections.Generic;

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
            if (!Input.GetMouseButton(0)) {
                if (Options.throwWithMouse?.Value != false)
                    TryThrow(game, dragChunk?.owner);
                stop = true;
            }

            //dragchunk not in this room
            if (dragChunk?.owner?.room != null && dragChunk.owner.room != room)
                stop = true;

            if (stop) {
                dragChunk = null;
                return;
            }

            //search all objects for closest chunk
            if (dragChunk == null)
                dragChunk = GetClosestChunk(room, mousePos, ref dragOffset);

            //drag this chunk
            if (dragChunk != null) {
                if (ShouldRelease(dragChunk.owner)) {
                    dragChunk = null;
                } else { //might affect sandbox mouse
                    dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
                    dragChunk.pos += mousePos + dragOffset - dragChunk.pos;

                    if (IsObjectPaused(dragChunk.owner)) {
                        //do not launch creature after pause
                        dragChunk.vel = new Vector2();

                        //reduces visual bugs
                        dragChunk.lastPos = dragChunk.pos;
                    }

                    //pull spears from walls & grasps
                    if (dragChunk.owner is Weapon &&
                        (dragChunk.owner as Weapon).mode != Weapon.Mode.Free &&
                        Custom.Dist(dragChunk.pos, dragChunk.lastPos) > 20f) {
                        if (dragChunk.owner is Spear) //prevent spear leaving invisible beams behind
                            (dragChunk.owner as Spear).resetHorizontalBeamState();
                        (dragChunk.owner as Weapon).ChangeMode(Weapon.Mode.Free);
                        dragChunk.owner.AllGraspsLetGoOfThisObject(true);
                    }
                }
            }
        }


        //search all objects for closest chunk
        public static BodyChunk GetClosestChunk(Room room, Vector2 pos, ref Vector2 offset)
        {
            if (room == null)
                return null;
            BodyChunk ret = null;
            Vector2 offs = new Vector2();

            //check object chunks for closest to mousepointer
            float closest = float.MaxValue;
            void closestChunk(PhysicalObject obj)
            {
                if (ShouldRelease(obj))
                    return;

                for (int k = 0; k < obj.bodyChunks.Length; k++)
                {
                    if (!Custom.DistLess(pos, obj.bodyChunks[k].pos, Mathf.Min(obj.bodyChunks[k].rad + 10f, closest)))
                        continue;
                    closest = Vector2.Distance(pos, obj.bodyChunks[k].pos);
                    ret = obj.bodyChunks[k];
                    offs = ret.pos - pos;
                }
            }

            for (int i = 0; i < room.physicalObjects.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    closestChunk(room.physicalObjects[i][j]);

            offset = offs;
            return ret;
        }


        //object should not be dragged any longer
        public static bool ShouldRelease(PhysicalObject obj)
        {
            //object is being deleted
            if (obj?.bodyChunks == null || obj.slatedForDeletetion)
                return true;

            //object is a creature, and is entering a shortcut
            if (obj is Creature && (obj as Creature).enteringShortCut != null)
                return true;
            return false;
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


        //try throwing object in the direction of mouse movement
        public static void TryThrow(RainWorldGame game, PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (obj?.firstChunk == null)
                return;

            //throw only if mouse moved fast enough
            if (Custom.Dist(obj.firstChunk.lastPos, obj.firstChunk.pos) < 40f)
                return;

            //don't throw if item is grabbed
            if (obj.grabbedBy?.Count > 0)
                return;

            //force release object from mouse drag
            //useful if keybind is used in the future
            if (obj is Weapon && obj == dragChunk?.owner)
                dragChunk = null;

            //vulture grub is special
            if (obj is VultureGrub)
                (obj as VultureGrub).InitiateSignalCountDown();

            if (!(obj is Weapon))
                return;
            Weapon weapon = obj as Weapon;
            if (weapon.abstractPhysicalObject == null)
                return;

            Creature thrower = game?.FirstAlivePlayer?.realizedCreature;
            bool deleteCreatureAfter = false;

            //temporary creature that receives a force on bodychunk
            if (thrower == null || Options.throwAsPlayer?.Value != true) {
                CreatureTemplate ct = new CreatureTemplate(CreatureTemplate.Type.Fly, null, new List<TileTypeResistance>(), new List<TileConnectionResistance>(), new CreatureTemplate.Relationship());
                AbstractCreature ac = new AbstractCreature(null, ct, null, weapon.abstractPhysicalObject.pos, new EntityID());
                ac.state = new NoHealthState(ac);
                thrower = new Fly(ac, weapon.abstractPhysicalObject.world);
                deleteCreatureAfter = true;
            }

            Vector2 dir = weapon.firstChunk.vel.normalized;

            //snap to horizontal/vertical
            if (weapon is Spear) {
                if (Math.Abs(dir.x) < 0.2f)
                    dir.x = 0f;
                if (Math.Abs(dir.x) > 0.8f)
                    dir.x = dir.x > 0 ? 1f : -1f;
                if (Math.Abs(dir.y) < 0.2f)
                    dir.y = 0f;
                if (Math.Abs(dir.y) > 0.8f)
                    dir.y = dir.y > 0 ? 1f : -1f;

                //prevent spear leaving invisible beams behind
                (weapon as Spear).resetHorizontalBeamState();
            }

            IntVector2 throwDir = new IntVector2(Math.Sign(dir.x), Math.Sign(dir.y));
            float force = 2f;

            //activate bombs etc.
            weapon.Thrown(thrower, weapon.firstChunk.pos, null, throwDir, force, false);
            weapon.changeDirCounter = 0; //don't change direction afterwards

            //set correct angles
            weapon.firstChunk.vel = dir * 40f * force;
            weapon.setRotation = new Vector2?(dir);

            //delete temporary creature
            if (deleteCreatureAfter) {
                thrower.Destroy();
                weapon.thrownBy = null;
            }
        }
    }
}
