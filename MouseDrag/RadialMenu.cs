using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    public class RadialMenu
    {
        public bool closed = false; //signal MenuStarter to destroy this
        public Vector2 menuPos, displayPos;
        public float outRad = 60f;
        public float inRad = 20f;
        private bool mousePressed = false;
        public Vector2 mousePos(RainWorldGame game) => (Vector2)Futile.mousePosition + game.cameras[0]?.pos ?? new Vector2();
        public List<Slot> slots = new List<Slot>();
        public Crosshair crosshair = null;
        public FContainer container = null;
        public BodyChunk followChunk = null;
        public Vector2 followOffset = new Vector2();


        public RadialMenu(RainWorldGame game)
        {
            menuPos = mousePos(game);
            followChunk = Tools.GetClosestChunk(game.cameras[0]?.room, menuPos, ref followOffset);
            displayPos = menuPos - game.cameras[0]?.pos ?? new Vector2();

            container = new FContainer();
            Futile.stage.AddChild(container);
            container.MoveToFront();

            crosshair = new Crosshair(this);
            crosshair.InitiateSprites(container);

            //dummy (not needed but makes menu visible if no slots are configured)
            AddSlot();
        }
        ~RadialMenu() { Destroy(); }


        public void LoadSlots(List<string> iconNames)
        {
            ClearSlots();
            foreach (string iconName in iconNames)
                AddSlot(iconName);
        }


        public void AddSlot(string iconName = "")
        {
            Slot s = new Slot(this);
            slots.Add(s);
            if (!string.IsNullOrEmpty(iconName))
                s.iconName = iconName;
            s.InitiateSprites(container);
        }


        public void ClearSlot(int i)
        {
            slots[i]?.Destroy();
            slots.RemoveAt(i);
        }


        public void ClearSlots()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i]?.Destroy();
            slots.Clear();
        }


        //return index when item is clicked on
        public int Update(RainWorldGame game)
        {
            if (followChunk != null) {
                if (Options.menuFollows?.Value != false)
                    menuPos = followChunk.pos - followOffset;
                crosshair.enabled = Options.menuFollows?.Value ?? true;

                if (Tools.ShouldRelease(followChunk?.owner) ||
                    followChunk?.owner?.room != game.cameras[0]?.room)
                    closed = true;
            }

            displayPos = menuPos - game.cameras[0]?.pos ?? new Vector2();
            container.MoveToFront(); //also refreshes container
            Vector2 mouse = mousePos(game);
            Vector2 angleVect = (mouse - menuPos).normalized;

            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                closed = true;

            float? angle = null;
            if (angleVect != Vector2.zero && 
                Custom.DistLess(menuPos, mouse, outRad) && 
                !Custom.DistLess(menuPos, mouse, inRad)) {
                angle = Custom.VecToDeg(angleVect);
                if (angle < 0)
                    angle += 360f;
            }

            int selected = -1;
            if (angle != null && slots.Count > 0)
                selected = (int)(angle.Value / (360 / (slots.Count > 0 ? slots.Count : 1)));

            for (int i = 0; i < slots.Count; i++) {
                slots[i].hover = i == selected;
                slots[i].Update();
            }
            crosshair.Update();

            if (!mousePressed)
                return -1;
            mousePressed = false;

            //close menu if mouse is pressed outside of menu
            if (selected < 0 && !Custom.DistLess(menuPos, mouse, inRad))
                closed = true;
            return selected;
        }


        public void RawUpdate(RainWorldGame game)
        {
            if (Input.GetMouseButtonDown(0))
                mousePressed = true;
            if ((Input.GetMouseButton(1) && Options.menuRMB?.Value == true) || 
                (Options.menuOpen?.Value != null && Input.GetKey(Options.menuOpen.Value))) {
                menuPos = mousePos(game);
                followChunk = Tools.GetClosestChunk(game.cameras[0]?.room, menuPos, ref followOffset);
            }
        }


        public void DrawSprites(float timeStacker)
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].DrawSprites(container, timeStacker);
            crosshair.DrawSprites(container, timeStacker);
        }


        public void Destroy()
        {
            ClearSlots();
            crosshair?.Destroy();
            container?.RemoveFromContainer();
        }


        public class Slot
        {
            public bool hover;
            public RadialMenu menu;
            public TriangleMesh background;
            public Vector2 curPos, prevPos;
            public Color curColor;
            public FSprite icon = null;
            public string iconName = "pixel";
            Color hoverColor = new Color(1f, 1f, 1f, 0.4f);
            Color noneColor = new Color(0f, 0f, 0f, 0.2f);


            public Slot(RadialMenu menu)
            {
                this.menu = menu;
                prevPos = menu.displayPos;
                curPos = menu.displayPos;
                curColor = noneColor;
            }
            ~Slot() { Destroy(); }


            public void Update()
            {
                prevPos = curPos;
                curPos = menu.displayPos;
            }


            public void InitiateSprites(FContainer container)
            {
                List<TriangleMesh.Triangle> list = new List<TriangleMesh.Triangle>();

                for (int i = 0; i < 9; i++) { //this for-loop is heavily adapted from beastmaster
                    list.Add(new TriangleMesh.Triangle(i, i + 1, i + 10));
                    list.Add(new TriangleMesh.Triangle(i + 10, i + 10 + 1, i + 1));
                }

                background = new TriangleMesh("Futile_White", list.ToArray(), true, false);
                icon = new FSprite(iconName, true);
                container.AddChild(background);
                container.AddChild(icon);
            }


            public void DrawSprites(FContainer container, float timeStacker)
            {
                float slotDegrees = (float)(360 / menu.slots.Count);
                int slotIndex = menu.slots.IndexOf(this);
                float start = slotDegrees * slotIndex;
                float end = slotDegrees + start;

                curColor = Color.Lerp(curColor, hover ? hoverColor : noneColor, 0.1f);
                background.color = curColor;

                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker);

                for (int i = 0; i < 10; i++) { //this for-loop is heavily adapted from beastmaster
                    float angle = Mathf.Lerp(start, end, (float)i / 9f);
                    background.vertices[i] = tsPos + (Custom.RotateAroundOrigo(Vector2.up, angle) * menu.inRad);
                    background.vertices[i + 10] = tsPos + (Custom.RotateAroundOrigo(Vector2.up, angle) * menu.outRad);
                }

                icon.SetPosition(tsPos + (Custom.RotateAroundOrigo(Vector2.up, Mathf.Lerp(start, end, 0.5f)) * Mathf.Lerp(menu.inRad, menu.outRad, 0.5f)));
            }


            public void Destroy()
            {
                icon?.RemoveFromContainer();
                background?.RemoveFromContainer();
            }
        }


        public class Crosshair
        {
            public RadialMenu menu;
            public Vector2 curPos, prevPos;
            public bool enabled = true;
            private bool visible;
            private int rotation = 0;
            public float radius = 16f;
            public float scale = 0.5f;
            public FSprite[] icons = new FSprite[4];


            public Crosshair(RadialMenu menu)
            {
                this.menu = menu;
                prevPos = menu.displayPos;
                curPos = menu.displayPos;
            }
            ~Crosshair() { Destroy(); }


            public void Update()
            {
                prevPos = curPos;
                curPos = menu.displayPos;
                visible = menu.followChunk != null && enabled;
                rotation++;
            }


            public void InitiateSprites(FContainer container)
            {
                for (int i = 0; i < icons.Length; i++)
                {
                    icons[i] = new FSprite("mousedragArrow", true);
                    container.AddChild(icons[i]);
                }
            }


            public void DrawSprites(FContainer container, float timeStacker)
            {
                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker);
                float tsRotation = Mathf.Lerp(rotation - 1, rotation, timeStacker) % 360f;
                for (int i = 0; i < icons.Length; i++) {
                    icons[i].isVisible = visible;
                    icons[i].SetPosition(tsPos + (Custom.RotateAroundOrigo(Vector2.up, 90f * i + tsRotation) * radius));
                    icons[i].rotation = 90f * i + tsRotation;
                    icons[i].scale = scale;
                }
            }


            public void Destroy()
            {
                for (int i = 0; i < icons.Length; i++)
                    icons[i].RemoveFromContainer();
            }
        }
    }
}
