using UnityEngine;
using RWCustom;
using System;
using System.Collections.Generic;
using DevInterface;

namespace MouseDrag
{
    public static class Drag
    {
        public static BodyChunk dragChunk; //owner is reference to the physicalobject which is dragged
        public static Vector2 dragOffset;
        private static Vector2 dampingPos; //only used when velocityDrag == true
        public static float maxVelocityPlayer = 25f; //only used when velocityDrag == true
        public static bool tempVelocityDrag; //temporary velocityDrag until LMB is released
        public static int tempStopTicks = 0; //temporarily deactivate drag
        public static int playerNr = 0; //last dragged or selected player
        public static void SetPlayerNr(int i) => playerNr = i; //dev console tool
        public static bool dragButtonPressed(bool noRMB = false) => (
            (Input.GetMouseButton(0) && Options.dragLMB?.Value == true) ||
            (Input.GetMouseButton(2) && Options.dragMMB?.Value == true) ||
            (Options.drag?.Value != null && Input.GetKey(Options.drag.Value))
        );
        public static bool dragButtonDown() => (
            (Input.GetMouseButtonDown(0) && Options.dragLMB?.Value == true) ||
            (Input.GetMouseButtonDown(2) && Options.dragMMB?.Value == true) ||
            (Options.drag?.Value != null && Input.GetKeyDown(Options.drag.Value))
        );


        //get mouse position hovering over any camera
        public static Vector2 MousePos(RainWorldGame game)
        {
            RoomCamera rcam = MouseCamera(game, out Vector2 offset);
            Vector2 pos = (Vector2)Futile.mousePosition - offset + rcam?.pos ?? new Vector2();
            if (!Integration.sBCameraScrollEnabled)
                return pos;
            try {
                pos += Integration.SBCameraScrollExtraOffset(rcam, Futile.mousePosition, out _);
            } catch {
                Plugin.Logger.LogError("Drag.MousePos exception while reading SBCameraScroll, integration is now disabled");
                Integration.sBCameraScrollEnabled = false;
                throw; //throw original exception while preserving stack trace
            }
            return pos;
        }


        //get camera where mouse is currently located
        public static RoomCamera MouseCamera(RainWorldGame game) { return MouseCamera(game, out _); }
        public static RoomCamera MouseCamera(RainWorldGame game, out Vector2 offset)
        {
            offset = Vector2.zero;
            if (!(game?.cameras?.Length > 0))
                return null;
            if (!Integration.splitScreenCoopEnabled)
                return game.cameras[0];
            try {
                return Integration.SplitScreenCoopCam(game, out offset);
            } catch {
                Plugin.Logger.LogError("Drag.MouseCamera exception while reading SplitScreen Co-op, integration is now disabled");
                Integration.splitScreenCoopEnabled = false;
                throw; //throw original exception while preserving stack trace
            }
        }


        public static void DragObject(RainWorldGame game)
        {
            bool stop = false;
            Vector2 mousePos = MousePos(game);
            dampingPos = Vector2.Lerp(dampingPos, mousePos, 0.3f);

            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                stop = true;

            //room unavailable
            Room room = MouseCamera(game)?.room;
            if (room?.physicalObjects == null)
                stop = true;

            //drag button not pressed
            if (!dragButtonPressed()) {
                if (Options.throwWithMouse?.Value != false)
                    TryThrow(game, dragChunk?.owner);
                stop = true;
            }

            //dragchunk not in this room
            if (dragChunk?.owner?.room != null && dragChunk.owner.room != room)
                stop = true;

            //temporarily deactivated
            if (tempStopTicks > 0) {
                tempStopTicks--;
                stop = true;
            }

            //devtools interface opened specifically on map page, to prevent accidental dragging
            if (game.devUI?.activePage is MapPage)
                stop = true;

            //mouse dragging disabled by another mod
            if (State.draggingDisabled)
                stop = true;

            if (stop) {
                if (Options.adjustableLocks?.Value != false)
                    Lock.ResetLock(dragChunk);
                dragChunk = null;
                tempVelocityDrag = false;
                return;
            }

            //search all objects for closest chunk
            if (dragChunk == null)
                dragChunk = GetClosestChunk(room, mousePos, ref dragOffset);

            //drag this chunk
            if (dragChunk == null)
                return;

            if (ShouldRelease(dragChunk.owner)) {
                dragChunk = null;
                return;
            }

            bool paused = Pause.IsObjectPaused(dragChunk.owner);
            bool isWeaponAndNotFree = dragChunk.owner is Weapon && (dragChunk.owner as Weapon).mode != Weapon.Mode.Free;

            //this drag functionality might (be) affect(ed by) sandbox mouse
            dragChunk.vel += mousePos + dragOffset - dragChunk.pos;
            if ((Options.velocityDrag?.Value != true && !tempVelocityDrag) || paused || isWeaponAndNotFree)
                dragChunk.pos += mousePos + dragOffset - dragChunk.pos;

            if (paused) {
                //do not launch creature after pause
                dragChunk.vel = new Vector2();

                //reduces visual bugs
                dragChunk.lastPos = dragChunk.pos;
            }

            //velocity drag with BodyChunk at center of mousePos
            if (Options.velocityDrag?.Value == true || tempVelocityDrag) {
                dragChunk.vel = (dampingPos - dragChunk.pos) / 2f;

                //reduce max speed of player
                if (dragChunk.owner is Player && (!(dragChunk.owner as Player).isNPC || Options.exceptSlugNPC?.Value != false))
                    dragChunk.vel = Vector2.ClampMagnitude(dragChunk.vel, maxVelocityPlayer);
            }

            //pull spears from walls & grasps
            if (Custom.Dist(dragChunk.pos, dragChunk.lastPos) > 15f) {
                if (isWeaponAndNotFree) {
                    if (dragChunk.owner is Spear) {
                        //prevent spear leaving invisible beams behind
                        (dragChunk.owner as Spear).resetHorizontalBeamState();

                        //drop spear from back
                        if (game.Players != null)
                            foreach (AbstractCreature ac in game.Players)
                                if (dragChunk.owner == (ac?.realizedCreature as Player)?.spearOnBack?.spear)
                                    (ac.realizedCreature as Player).spearOnBack.DropSpear();
                    }
                    (dragChunk.owner as Weapon).ChangeMode(Weapon.Mode.Free);
                }
                dragChunk.owner.AllGraspsLetGoOfThisObject(true);
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

                //only select or drag creatures or objects
                if (Options.selectCreatures?.Value != null && Input.GetKey(Options.selectCreatures.Value))
                    if (!(obj is Creature))
                        return;
                if (Options.selectItems?.Value != null && Input.GetKey(Options.selectItems.Value))
                    if (obj is Creature)
                        return;

                for (int k = 0; k < obj.bodyChunks.Length; k++)
                {
                    float rad = Mathf.Max(obj.bodyChunks[k].rad, 20f); //same value as RadialMenu.inRad
                    if (!Custom.DistLess(pos, obj.bodyChunks[k].pos, Mathf.Min(rad, closest)))
                        continue;
                    closest = Vector2.Distance(pos, obj.bodyChunks[k].pos);
                    ret = obj.bodyChunks[k];
                    offs = ret.pos - pos;
                }
            }

            for (int i = 0; i < room.physicalObjects.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    closestChunk(room.physicalObjects[i][j]);

            //store current player nr
            if (ret?.owner is Player && !(ret.owner as Player).isNPC)
                playerNr = (ret.owner as Player).playerState?.playerNumber ?? 0;
            var pair = Control.ListContains(ret?.owner?.abstractPhysicalObject as AbstractCreature);
            if (pair != null)
                playerNr = pair.Value.Value;

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


        //try throwing object in the direction of mouse movement
        public static void TryThrow(RainWorldGame game, PhysicalObject obj, bool overrideThreshold = false)
        {
            Creature thrower = game?.FirstAlivePlayer?.realizedCreature;

            if (obj?.firstChunk == null)
                return;

            //throw only if mouse moved fast enough
            if (!overrideThreshold && Custom.Dist(obj.firstChunk.lastPos, obj.firstChunk.pos) < (Options.throwThreshold?.Value ?? 40f))
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

            //jellyfish is special
            if (obj is JellyFish) {
                (obj as JellyFish).Tossed(thrower);
                if (Options.throwAsPlayer?.Value != true)
                    (obj as JellyFish).thrownBy = null;
            }

            //fireegg is special
            if (obj is MoreSlugcats.FireEgg) {
                (obj as MoreSlugcats.FireEgg).Tossed(thrower);
                if (Options.throwAsPlayer?.Value != true)
                    (obj as MoreSlugcats.FireEgg).thrownBy = null;
            }

            //hazer is special, alternative is using kill/revive options
            if (obj is Hazer)
                (obj as Hazer).tossed = true;

            if (!(obj is Weapon))
                return;
            Weapon weapon = obj as Weapon;
            if (weapon.abstractPhysicalObject == null)
                return;

            //temporary creature that receives a force on bodychunk
            bool deleteCreatureAfter = false;
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
            float force = Options.throwForce?.Value ?? 2f;

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
