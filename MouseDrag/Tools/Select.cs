using UnityEngine;
using System.Collections.Generic;
using RWCustom;

namespace MouseDrag
{
    public static class Select
    {
        //TODO fix graphics when using SplitScreen Co-op and moving selection-rectangle over to 
        //another screen? or player should just cancel and restart selection like it is now


        public static List<BodyChunk> selectedChunks = new List<BodyChunk>(); //used for multi-select objects
        public static List<PhysicalObject> selectedObjects {
            get {
                List<PhysicalObject> ret = new List<PhysicalObject>();
                foreach (var data in selectedChunks)
                    if (data.owner != null && !ret.Contains(data.owner))
                        ret.Add(data.owner);
                return ret;
            }
        }


        public static Vector2 rectStartPos;
        public static Rectangle selectRect = null;
        public static List<Circle> visuals = new List<Circle>(); //shows which objects are selected
        private static bool refreshCrosshairs;


        public static void Update(RainWorldGame game)
        {
            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                return;

            var rcam = Drag.MouseCamera(game, out Vector2 offset);
            bool cancelRect = Options.multipleSelect?.Value == null || !Input.GetKey(Options.multipleSelect.Value) || !State.activated;

            UpdateCrosshairs(rcam, selectActive: !cancelRect);

            //selection-rectangle canceled
            if (cancelRect) {
                selectRect?.Destroy();
                selectRect = null;
                return;
            }

            //select multiple objects using selection-rectangle
            bool dragPressed = Drag.dragButtonPressed();

            //selection-rectangle started
            if (dragPressed && selectRect == null) {
                selectRect = new Rectangle();
                selectRect.InitiateSprites(rcam?.ReturnFContainer("HUD"));
                rectStartPos = Drag.MousePos(game);

            //selection-rectangle ended
            } else if (!dragPressed && selectRect != null) {
                selectRect.Destroy();
                selectRect = null;
                Room room = rcam?.room;
                Vector2 rectEndPos = Drag.MousePos(game);
                List<BodyChunk> tempChunks = new List<BodyChunk>(); //fixes SeedCob select bug

                void bodyChunkSelection(BodyChunk bc)
                {
                    if (bc?.owner == null)
                        return;
                    if (!IsWithinRect(bc, rectStartPos, rectEndPos))
                        return;
                    if (Vector2.Distance(rectStartPos, rectEndPos) < 10f)
                        return;
                    if (tempChunks.Contains(bc))
                        return;

                    //only select creatures or items
                    if (Options.onlySelectCreatures?.Value != null && Input.GetKey(Options.onlySelectCreatures.Value))
                        if (!(bc.owner is Creature))
                            return;
                    if (Options.onlySelectItems?.Value != null && Input.GetKey(Options.onlySelectItems.Value))
                        if (bc.owner is Creature)
                            return;

                    //invert selection
                    if (selectedChunks.Contains(bc)) {
                        if (Options.selectionXOR?.Value == true)
                            selectedChunks.Remove(bc);
                    } else {
                        selectedChunks.Add(bc);
                    }
                    tempChunks.Add(bc);
                }

                //add bodychunks in room within rectangle to selection
                for (int i = 0; i < room?.physicalObjects?.Length; i++)
                    for (int j = 0; j < room.physicalObjects[i]?.Count; j++)
                        for (int k = 0; k < room.physicalObjects[i][j]?.bodyChunks?.Length; k++)
                            bodyChunkSelection(room.physicalObjects[i][j].bodyChunks[k]);
            }

            //update selection-rectangle positions
            if (selectRect != null) {
                selectRect.Update();
                selectRect.start = rectStartPos - rcam?.pos ?? new Vector2() - offset;
                selectRect.end = (Vector2)Futile.mousePosition - offset;

                if (Integration.sBCameraScrollEnabled) {
                    try {
                        selectRect.start -= Integration.SBCameraScrollExtraOffset(rcam, selectRect.start, out float scale) / (1f / scale);
                    } catch {
                        Plugin.Logger.LogError("Select.Update exception while reading SBCameraScroll, integration is now disabled");
                        Integration.sBCameraScrollEnabled = false;
                        throw; //throw original exception while preserving stack trace
                    }
                }
            }
        }


        public static void UpdateCrosshairs(RoomCamera rcam, bool selectActive)
        {
            //create/destroy selection crosshairs
            if (selectedChunks.Count != visuals.Count || refreshCrosshairs) {
                for (int i = visuals.Count; i > selectedChunks.Count; i--)
                    visuals.Pop().Destroy();
                var container = rcam?.ReturnFContainer("HUD");
                for (int i = visuals.Count; i < selectedChunks.Count; i++) {
                    var ch = new Circle();
                    visuals.Add(ch);
                }
                for (int i = 0; i < visuals.Count; i++) {
                    int spriteCount = 5 + (int)(selectedChunks[i].rad / 6f);
                    visuals[i].InitiateSprites(container, spriteCount);
                    visuals[i].rotationSpeed = 3f * (10f / selectedChunks[i].rad);
                }
                refreshCrosshairs = false;
            }

            bool selectedByMenu = MenuManager.menu?.followChunk != null && selectedChunks.Contains(MenuManager.menu.followChunk);

            //update selection crosshairs
            for (int i = 0; i < visuals.Count; i++) {
                visuals[i].Update();
                visuals[i].curPos = selectedChunks[i].pos - rcam?.pos ?? new Vector2();
                visuals[i].radius = selectedChunks[i].rad;
                visuals[i].visible = 
                    visuals[i].prevPos != Vector2.zero && 
                    (selectActive || selectedByMenu) && 
                    selectedChunks[i]?.owner?.room != null && 
                    selectedChunks[i].owner.room == rcam?.room;

                float bgScale = 1f;
                if (Integration.sBCameraScrollEnabled) {
                    try {
                        visuals[i].curPos -= Integration.SBCameraScrollExtraOffset(rcam, visuals[i].curPos, out bgScale) / (1f / bgScale);
                    } catch {
                        Plugin.Logger.LogError("Select.UpdateCrosshairs exception while reading SBCameraScroll, integration is now disabled");
                        Integration.sBCameraScrollEnabled = false;
                        throw; //throw original exception while preserving stack trace
                    }
                }
                visuals[i].radius *= bgScale;
            }
        }


        public static void RawUpdate(RainWorldGame game)
        {
            //game is paused
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                return;

            if (!State.activated)
                return;

            //select individual objects using multiselect + drag button
            if (Drag.dragButtonDown()) {
                if (Options.multipleSelect?.Value != null && Input.GetKey(Options.multipleSelect.Value)) {
                    Vector2 offset = Vector2.zero;
                    BodyChunk bc = Drag.GetClosestChunk(Drag.MouseCamera(game)?.room, Drag.MousePos(game), ref offset);
                    if (bc != null) {
                        if (selectedChunks.Contains(bc)) {
                            selectedChunks.Remove(bc);
                        } else {
                            selectedChunks.Add(bc);
                        }
                    }
                //mouse clicking within radialmenu won't remove selection
                } else if (selectedChunks.Count > 0 && MenuManager.menu?.mouseIsWithinMenu != true) {
                    //clear selection if current selection wasn't clicked on
                    Vector2 offset = Vector2.zero;
                    BodyChunk bc = Drag.GetClosestChunk(Drag.MouseCamera(game)?.room, Drag.MousePos(game), ref offset);
                    if (bc == null || !selectedChunks.Contains(bc))
                        selectedChunks.Clear();
                }
                refreshCrosshairs = true; //refresh in the case where new bodychunk count is the same as the old count
            }
        }


        public static void DrawSprites(float timeStacker)
        {
            selectRect?.DrawSprites(timeStacker);
            foreach (var ch in visuals)
                ch.DrawSprites(timeStacker);
        }


        public static bool IsWithinRect(BodyChunk bc, Vector2 start, Vector2 end)
        {
            if (bc == null)
                return false;
            return bc.pos.x >= Mathf.Min(start.x, end.x) && bc.pos.x <= Mathf.Max(start.x, end.x) &&
                bc.pos.y >= Mathf.Min(start.y, end.y) && bc.pos.y <= Mathf.Max(start.y, end.y);
        }


        public class Circle
        {
            public Vector2 curPos, prevPos;
            public bool visible;
            public float rotation = 0f;
            public float rotationSpeed = 3f;
            public float radius = 16f, prevRadius;
            public FSprite[] sprites = null;
            public FContainer container = null;
            public Color color = Color.white;


            public Circle() {}
            ~Circle() { Destroy(); }


            public void Update()
            {
                prevPos = curPos;
                prevRadius = radius;
                rotation += rotationSpeed;
            }


            public void InitiateSprites(FContainer container, int spriteCount)
            {
                if (this.container == null)
                    this.container = new FContainer();
                for (int i = 0; i < sprites?.Length; i++)
                    sprites[i].RemoveFromContainer();
                sprites = new FSprite[spriteCount];
                for (int i = 0; i < sprites.Length; i++) {
                    sprites[i] = new FSprite("pixel", true);
                    this.container.AddChild(sprites[i]);
                    sprites[i].isVisible = true;
//                    if (RWCustom.Custom.rainWorld?.Shaders?.Count > 0)
//                        sprites[i].shader = RWCustom.Custom.rainWorld.Shaders["HologramBothSides"];
                }
                container.AddChild(this.container);
                this.container.MoveToBack();
            }


            public void DrawSprites(float timeStacker)
            {
                float tsRadius = Mathf.Lerp(prevRadius, radius, timeStacker);
                float angleDiff = 360f / sprites.Length;
                float tsRotation = Mathf.Lerp(rotation - rotationSpeed, rotation, timeStacker) % 360f;

                Vector2 a = new Vector2(0f, tsRadius);
                Vector2 b = Custom.RotateAroundOrigo(Vector2.up * tsRadius, angleDiff);
                float lineLength = Vector2.Distance(a, b);

                //position lines around center of circle
                for (int i = 0; i < sprites.Length; i++) {
                    Vector2 pos = Custom.RotateAroundOrigo(Vector2.up * tsRadius, angleDiff * i);
                    Vector2 nextPos = Custom.RotateAroundOrigo(Vector2.up * tsRadius, angleDiff * (i + 1));
                    sprites[i].rotation = Custom.AimFromOneVectorToAnother(pos, nextPos);
                    sprites[i].SetPosition(Vector2.Lerp(pos, nextPos, 0.5f));
                    sprites[i].scaleY = lineLength;
                    sprites[i].color = color;
                }

                //apply position
                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker);
                container.isVisible = visible;
                container.SetPosition(tsPos);
                container.rotation = tsRotation;
            }


            public void Destroy()
            {
                for (int i = 0; i < sprites?.Length; i++)
                    sprites[i].RemoveFromContainer();
                sprites = null;
                container?.RemoveFromContainer();
                container = null;
            }
        }


        public class Rectangle
        {
            public TriangleMesh rect;
            public Vector2 start, end, prevStart, prevEnd;
            public Color color = new Color(0f, 0f, 0f, 0.2f);


            public Rectangle() { }
            ~Rectangle() { Destroy(); }


            public void Update()
            {
                prevStart = start;
                prevEnd = end;
            }


            public void InitiateSprites(FContainer container)
            {
                List<TriangleMesh.Triangle> list = new List<TriangleMesh.Triangle>();
                list.Add(new TriangleMesh.Triangle(0, 1, 2));
                list.Add(new TriangleMesh.Triangle(1, 2, 3));
                rect = new TriangleMesh("Futile_White", list.ToArray(), true, false);
                container.AddChild(rect);
                rect.MoveToBack();
            }


            public void DrawSprites(float timeStacker)
            {
                Vector2 tsStart = Vector2.Lerp(prevStart, start, timeStacker);
                Vector2 tsEnd = Vector2.Lerp(prevEnd, end, timeStacker);
                rect.vertices[0] = tsStart;
                rect.vertices[1] = new Vector2(tsStart.x, tsEnd.y);
                rect.vertices[2] = new Vector2(tsEnd.x, tsStart.y);
                rect.vertices[3] = tsEnd;
                rect.color = color;
                rect.Redraw(shouldForceDirty: true, shouldUpdateDepth: false);
            }


            public void Destroy()
            {
                rect?.RemoveFromContainer();
                rect = null;
            }
        }


        public class Crosshair
        {
            public Vector2 curPos, prevPos;
            public bool visible;
            public float rotation = 0f;
            public float rotationSpeed = 3f;
            public float radius = 16f;
            public float scale = 0.5f;
            public FSprite[] sprites = null;
            public string spriteName { get; private set; }
            public Color color = Color.white;


            public Crosshair(string spriteName = "mousedragArrow")
            {
                this.spriteName = spriteName;
            }
            ~Crosshair() { Destroy(); }


            public void Update()
            {
                prevPos = curPos;
                rotation += rotationSpeed;
            }


            public void InitiateSprites(FContainer container, int spriteCount)
            {
                for (int i = 0; i < sprites?.Length; i++)
                    sprites[i].RemoveFromContainer();
                sprites = new FSprite[spriteCount];
                for (int i = 0; i < sprites.Length; i++) {
                    sprites[i] = new FSprite(spriteName, true);
                    container.AddChild(sprites[i]);
                }
            }


            public void DrawSprites(float timeStacker)
            {
                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker);
                float tsRotation = Mathf.Lerp(rotation - rotationSpeed, rotation, timeStacker) % 360f;
                float angleOffset = sprites.Length > 0 ? (360f / (float) sprites.Length) : 360f; //degrees
                for (int i = 0; i < sprites.Length; i++) {
                    sprites[i].isVisible = visible;
                    sprites[i].SetPosition(tsPos + (Custom.RotateAroundOrigo(Vector2.up, angleOffset * i + tsRotation) * radius));
                    sprites[i].rotation = angleOffset * i + tsRotation;
                    sprites[i].scale = scale;
                    sprites[i].color = color;
                }
            }


            public void Destroy()
            {
                for (int i = 0; i < sprites?.Length; i++)
                    sprites[i].RemoveFromContainer();
                sprites = null;
            }
        }
    }
}
