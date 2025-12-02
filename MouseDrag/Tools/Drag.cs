using UnityEngine;
using RWCustom;
using System;
using System.Collections.Generic;
using DevInterface;

namespace MouseDrag
{
    public static class Drag
    {
        public class DragData {
            public BodyChunk chunk = null; //chunk which is dragged
            public PhysicalObject owner => chunk?.owner; //reference to the physicalobject which is dragged
            public Vector2 offset = Vector2.zero; //offset from mouse position
        }


        public static List<DragData> dragChunks = new List<DragData>(); //chunks in this list are actively being dragged
        public static BodyChunk dragChunk { //backwards compatibility, first and only chunk being dragged
            get {
                return dragChunks.Count > 0 ? dragChunks[0].chunk : null;
            }
            set {
                if (value != null)
                    dragChunks.Add(new DragData { chunk = value });
                if (value == null)
                    dragChunks.Clear();
            }
        }
        public static List<PhysicalObject> dragObjects {
            get {
                List<PhysicalObject> ret = new List<PhysicalObject>();
                foreach (var data in dragChunks)
                    if (data.owner != null && !ret.Contains(data.owner))
                        ret.Add(data.owner);
                return ret;
            }
        }


        private static Vector2 dampingPos; //only used when velocityDrag == true
        private static float scrollDir;
        public static float maxVelocityPlayer = 25f; //only used when velocityDrag == true
        public static bool tempVelocityDrag; //temporary velocityDrag until LMB is released
        public static bool keyVelocityDrag; //key toggle velocityDrag
        public static int tempStopDragTicks = 0; //temporarily deactivate drag and drop all
        public static int tempStopGrabTicks = 0; //temporarily deactivate grab/drag new chunks
        public static int playerNr = 0; //last dragged or selected player
        public static void SetPlayerNr(int i) => playerNr = i; //dev console tool
        public static bool dragButtonPressed(bool noLMB = false) => ( //stays true while pressed
            (Input.GetMouseButton(0) && Options.dragLMB?.Value == true && !noLMB) ||
            (Input.GetMouseButton(2) && Options.dragMMB?.Value == true) ||
            (Options.drag?.Value != null && Input.GetKey(Options.drag.Value))
        );
        public static bool dragButtonDown() => ( //true for a single frame
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


        public static void Update(RainWorldGame game)
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

            //dragging disabled when dev tools menu is opened, because it interferes with dragging sliders and decals
            bool devToolsOpened = game.devUI != null;

            //drag button not pressed
            if (!dragButtonPressed(noLMB: devToolsOpened)) {
                if (Options.throwWithMouse?.Value != false)
                    foreach (var obj in dragObjects)
                        TryThrow(game, obj);
                stop = true;
            }

            //temporarily deactivated
            if (tempStopGrabTicks > 0)
                tempStopGrabTicks--;
            if (tempStopDragTicks > 0) {
                tempStopDragTicks--;
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
                    foreach (var draggable in dragChunks)
                        Lock.ResetLock(draggable.chunk);
                ReleaseAll();
                tempVelocityDrag = false;
                return;
            }

            //temporarily disable dragging new chunks
            bool preventNewDrag = tempStopGrabTicks > 0;

            //prevent dragging new chunks if mouse is selecting a button on the radialmenu
            //allow dragging new chunks if drag button is different from menu select button
            preventNewDrag |= MenuManager.menu?.mouseIsOnMenuBG == true && (
                (Options.dragLMB?.Value == true && Options.menuSelectLMB?.Value == true) 
                || (Options.dragMMB?.Value == true && Options.menuSelectMMB?.Value == true) 
                || (
                    Options.menuSelect?.Value != null && Options.menuSelect.Value != KeyCode.None && 
                    Options.drag?.Value != null && Options.drag.Value != KeyCode.None && 
                    Options.menuSelect.Value == Options.drag.Value
                )
            );

            //don't drag new bodychunks when selection can be made with either multiselect + drag button or selection-rectangle
            preventNewDrag |= Options.multipleSelect?.Value != null && Input.GetKey(Options.multipleSelect.Value);

            //grab new bodychunks to drag, if one is close enough
            if (dragChunks.Count <= 0 && !preventNewDrag) {
                //search all objects for closest chunk
                Vector2 offset = Vector2.zero;
                BodyChunk chunk = GetClosestChunk(room, mousePos, ref offset);

                //selection is being dragged, fill dragChunks with all selectedChunks
                //Select.RawUpdate (runs first) made sure that the BodyChunk where the mouse is currently located is part of selectedChunks, OR selectedChunks is empty
                if (chunk != null && Select.selectedChunks.Contains(chunk)) {
                    foreach (var bc in Select.selectedChunks)
                        dragChunks.Add(new DragData { chunk = bc, offset = bc.pos - mousePos });

                //nothing was selected, so only drag the closest chunk
                } else if (chunk != null) {
                    dragChunks.Add(new DragData { chunk = chunk, offset = offset });

                    //selectedChunks did not contain this chunk, so add newly dragged chunk to selected chunks for tools
                    if (Select.selectedChunks.Count <= 0)
                        Select.selectedChunks.Add(chunk);
                }
            }

            //release current objects not in this room
            for (int i = dragChunks.Count - 1; i >= 0; i--)
                if (dragChunks[i].owner?.room == null || dragChunks[i].owner.room != room)
                    dragChunks.RemoveAt(i);

            for (int i = dragChunks.Count - 1; i >= 0; i--)
                if (ShouldRelease(dragChunks[i].owner))
                    dragChunks.RemoveAt(i);

            //check if tempVelocityDrag bodychunk is close enough, then switch back to positional drag
            if (tempVelocityDrag && dragChunks.Count > 0 && dragChunks[0].chunk != null)
                if (Vector2.Distance(mousePos, dragChunks[0].chunk.pos - dragChunks[0].offset) < 10f)
                    tempVelocityDrag = false;

            bool velocityDrag = Options.velocityDrag?.Value == true || tempVelocityDrag || keyVelocityDrag;

            //drag remaining chunks
            foreach (var draggable in dragChunks) {
                BodyChunk bc = draggable.chunk;

                bool paused = Pause.IsObjectPaused(bc.owner);
                bool isWeaponAndNotFree = bc.owner is Weapon && (bc.owner as Weapon).mode != Weapon.Mode.Free;

                //this drag functionality might (be) affect(ed by) sandbox mouse
                bc.vel += mousePos + draggable.offset - bc.pos;
                if (!velocityDrag || paused || isWeaponAndNotFree)
                    bc.pos += mousePos + draggable.offset - bc.pos;

                if (paused) {
                    //do not launch creature after pause
                    bc.vel = new Vector2();

                    //reduces visual bugs
                    bc.lastPos = bc.pos;
                }

                //velocity dragging
                if (velocityDrag) {
                    bc.vel = (dampingPos + draggable.offset - bc.pos) / 2f;

                    //reduce max speed of player
                    if (bc.owner is Player && (!(bc.owner as Player).isNPC || Options.exceptSlugNPC?.Value != false))
                        bc.vel = Vector2.ClampMagnitude(bc.vel, maxVelocityPlayer);
                }

                //pull spears from walls & grasps
                if (Custom.Dist(bc.pos, bc.lastPos) > 15f) {
                    if (isWeaponAndNotFree) {
                        if (bc.owner is Spear) {
                            //prevent spear leaving invisible beams behind
                            (bc.owner as Spear).resetHorizontalBeamState();

                            //drop spear from back
                            if (game.Players != null)
                                foreach (AbstractCreature ac in game.Players)
                                    if (bc.owner == (ac?.realizedCreature as Player)?.spearOnBack?.spear)
                                        (ac.realizedCreature as Player).spearOnBack.DropSpear();
                        }
                        (bc.owner as Weapon).ChangeMode(Weapon.Mode.Free);
                    }
                    bc.owner.AllGraspsLetGoOfThisObject(true);
                }
            }

            //rotate chunks around mouse position using scroll wheel
            if (scrollDir != 0f) {
                foreach (var draggable in dragChunks)
                    draggable.offset = Custom.RotateAroundOrigo(draggable.offset, scrollDir * 5f);
                scrollDir = 0f;
            }
        }


        public static void RawUpdate(RainWorldGame game)
        {
            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                return;

            //record scroll wheel direction for rotating chunks around mouse position
            if (scrollDir == 0f)
                scrollDir = Input.mouseScrollDelta.y;

            //toggle velocityDrag
            if (Options.velocityDragKey?.Value != null && Input.GetKeyDown(Options.velocityDragKey.Value))
                keyVelocityDrag = !keyVelocityDrag;
        }


        //search all objects for closest chunk
        public static float minimumChunkRad_GetClosestChunk = 20f; //same value as RadialMenu.inRad
        public static BodyChunk GetClosestChunk(Room room, Vector2 pos, ref Vector2 offset, float extraRad = 0f)
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

                //only select or drag creatures or items
                if (Options.onlySelectCreatures?.Value != null && Input.GetKey(Options.onlySelectCreatures.Value))
                    if (!(obj is Creature))
                        return;
                if (Options.onlySelectItems?.Value != null && Input.GetKey(Options.onlySelectItems.Value))
                    if (obj is Creature)
                        return;

                for (int k = 0; k < obj.bodyChunks.Length; k++)
                {
                    float rad = Mathf.Max(obj.bodyChunks[k].rad + extraRad, minimumChunkRad_GetClosestChunk);
                    if (!Custom.DistLess(pos, obj.bodyChunks[k].pos, Mathf.Min(rad, closest)))
                        continue;
                    closest = Vector2.Distance(pos, obj.bodyChunks[k].pos);
                    ret = obj.bodyChunks[k];
                    offs = ret.pos - pos;
                }
            }

            for (int i = 0; i < room.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i]?.Count; j++)
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


        //check if object should not be dragged any longer
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


        public static void ReleaseAll()
        {
            dragChunks.Clear();
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
            if (obj is Weapon)
                for (int i = dragChunks.Count - 1; i >= 0; i--)
                    if (obj == dragChunks[i].owner)
                        dragChunks.RemoveAt(i);

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
