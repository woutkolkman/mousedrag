﻿using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    public class RadialMenu
    {
        public bool closed = false; //signal MenuStarter to destroy this
        public Vector2 menuPos, displayPos, prevDisplayPos; //menuPos in room, displayPos on screen
        private Vector2 prevCamPos; //only used to compensate stationary position with screen scrolling mods
        public float outRad = 60f;
        public float inRad = 20f; //same value as Drag.GetClosestChunk rad
        private bool mousePressed = false; //LMB presseddown signal from RawUpdate for Update
        public bool mouseIsWithinMenu { get; private set; }
        public bool mouseIsOnMenuBG { get; private set; }
        private RoomCamera prevRCam = null; //just to detect SplitScreen Co-op camera change
        public List<Slot> slots = new List<Slot>();
        public Select.Crosshair crosshair = null;
        public FLabel label = null;
        public string labelText = ""; //update label text using this field
        public string roomName { get; private set; } = null; //outgoing name of room where menu is located currently
        public FContainer container = null;
        public BodyChunk followChunk = null, prevFollowChunk = null;
        public Vector2 followOffset = new Vector2();
        public bool snapToChunk = true;
        public static bool menuOpenButtonPressed(bool noRMB = false) => ( //stays true while pressed
            (Input.GetMouseButton(1) && Options.menuOpenRMB?.Value == true && !noRMB) ||
            (Input.GetMouseButton(2) && Options.menuOpenMMB?.Value == true) ||
            (Options.menuOpen?.Value != null && Input.GetKey(Options.menuOpen.Value))
        );
        public static bool menuSelectButtonDown() => ( //true for a single frame
            (Input.GetMouseButtonDown(0) && Options.menuSelectLMB?.Value == true) ||
            (Input.GetMouseButtonDown(2) && Options.menuSelectMMB?.Value == true) ||
            (Options.menuSelect?.Value != null && Input.GetKeyDown(Options.menuSelect.Value))
        );


        public RadialMenu(RainWorldGame game)
        {
            var rcam = Drag.MouseCamera(game);
            prevRCam = rcam;
            if (game != null) {
                menuPos = Drag.MousePos(game);
                followChunk = Drag.GetClosestChunk(rcam?.room, menuPos, ref followOffset);
                displayPos = menuPos - rcam?.pos ?? new Vector2();
                prevDisplayPos = displayPos;
            }

            container = new FContainer();
            rcam?.ReturnFContainer("HUD").AddChild(container); //add to RoomCamera so SplitScreen Co-op is supported
            if (rcam == null)
                Futile.stage.AddChild(container); //backup / original code
            container.MoveToFront();

            crosshair = new Select.Crosshair();
            crosshair.prevPos = displayPos;
            crosshair.curPos = displayPos;
            crosshair.InitiateSprites(container, 4);

            label = new FLabel(Custom.GetFont(), "");
            container.AddChild(label);

            //dummy (not needed but makes menu visible if no slots are configured)
            AddSlot(new Slot(this));
        }
        ~RadialMenu() { Destroy(); }


        public void LoadSlots(List<Slot> slots)
        {
            ClearSlots();
            if (slots != null)
                foreach (Slot slot in slots)
                    AddSlot(slot);
        }


        public void AddSlot(Slot s)
        {
            slots.Add(s);
            s.InitiateSprites(container);
        }


        public void ClearSlot(int i)
        {
            slots[i]?.Destroy();
            slots.RemoveAt(i);
        }


        public void ClearSlot(Slot s)
        {
            if (s == null)
                return;
            s.Destroy();
            slots.Remove(s);
        }


        public void ClearSlots()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i]?.Destroy();
            slots.Clear();
        }


        //return slot when icon is clicked on
        public Slot Update(RainWorldGame game, out Slot hoverSlot)
        {
            RoomCamera rcam = Drag.MouseCamera(game, out Vector2 splitScreenOffset);
            if (rcam != null && rcam != prevRCam) { //only occurs when SplitScreen Co-op is used
                container?.RemoveFromContainer();
                rcam.ReturnFContainer("HUD").AddChild(container);
                prevRCam = rcam;
            }
            roomName = rcam?.room?.abstractRoom?.name;

            prevFollowChunk = followChunk;
            if (followChunk != null) {
                if (snapToChunk)
                    followOffset = new Vector2();
                bool followTarget = Options.menuFollows?.Value != false; //menu follows if doing nothing
                followTarget &= 
                    !mouseIsWithinMenu || //move menu if mouse is not within menu, stop follow if hovering over menu
                    Drag.dragChunks.Count > 0 || //move menu if dragging something with your mouse (menu selected object)
                    Options.menuMoveHover?.Value == true; //always move menu even if hovering over it
                followTarget |= menuOpenButtonPressed(); //menu always follows if menu-open button is pressed
                if (followTarget)
                    menuPos = followChunk.pos - followOffset;

                if (Drag.ShouldRelease(followChunk?.owner) || 
                    followChunk?.owner?.room != rcam?.room)
                    closed = true;
            }
            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                closed = true;

            //move menu with camera if mouse hovers & camera scrolls due to another mod
            if (rcam != null) {
                if (mouseIsWithinMenu && Options.menuMoveHover?.Value != true)
                    menuPos += rcam.pos - prevCamPos;
                prevCamPos = rcam.pos;
            }

            prevDisplayPos = displayPos;
            displayPos = menuPos - rcam?.pos ?? new Vector2();
            float bgScale = 1f;
            if (Integration.sBCameraScrollEnabled) {
                try {
                    displayPos -= Integration.SBCameraScrollExtraOffset(rcam, displayPos, out bgScale) / (1f / bgScale);
                } catch {
                    Plugin.Logger.LogError("RadialMenu.Update exception while reading SBCameraScroll, integration is now disabled");
                    Integration.sBCameraScrollEnabled = false;
                    throw; //throw original exception while preserving stack trace
                }
            }
            Vector2 mouse = (Vector2)Futile.mousePosition - splitScreenOffset;
            Vector2 angleVect = (mouse - displayPos).normalized; //angle of mouse from center of menu

            //determine if mouse is in a valid position in the menu
            float? angle = null;
            mouseIsWithinMenu = false;
            mouseIsOnMenuBG = false;
            bool mouseIsWithinInRad = Custom.DistLess(displayPos, mouse, inRad);
            if (angleVect != Vector2.zero && Custom.DistLess(displayPos, mouse, outRad)) {
                mouseIsWithinMenu = true;
                if (!mouseIsWithinInRad) {
                    mouseIsOnMenuBG = true;
                    angle = Custom.VecToDeg(angleVect);
                    if (angle < 0)
                        angle += 360f;
                }
            }

            //determine which slot is hovered over
            int selected = -1;
            if (angle != null && slots.Count > 0)
                selected = (int)(angle.Value / (360f / (slots.Count > 0 ? slots.Count : 1)));

            Slot selectedSlot = null;
            for (int i = 0; i < slots.Count; i++) {
                slots[i].hover = i == selected;
                slots[i].Update();
                if (i == selected)
                    selectedSlot = slots[i];
            }
            hoverSlot = selectedSlot;

            crosshair.Update();

            //scale offset from menu based on SBCameraScroll zoom factor
            Vector2 curCHPos = displayPos;
            if (followChunk != null)
                curCHPos -= (menuPos - followChunk.pos) * bgScale;
            crosshair.curPos = curCHPos;

            //crosshair invisible when followChunk is part of multiple-select
            crosshair.visible = followChunk != null && !Select.selectedChunks.Contains(followChunk);

            if (!mousePressed)
                return null;
            mousePressed = false;

            //close menu if mouse is pressed outside of menu
            if (selected < 0 && !mouseIsWithinInRad)
                closed = true;
            return selectedSlot;
        }


        public void RawUpdate(RainWorldGame game)
        {
            if (menuSelectButtonDown())
                mousePressed = true;
            if (menuOpenButtonPressed()) {
                Vector2 mousePos = Drag.MousePos(game);
                followChunk = Drag.GetClosestChunk(Drag.MouseCamera(game)?.room, mousePos, ref followOffset);
                if (followChunk == null)
                    menuPos = mousePos;
            }
        }


        public void DrawSprites(float timeStacker)
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].DrawSprites(timeStacker);
            crosshair.DrawSprites(timeStacker);
            label.SetPosition(Vector2.Lerp(prevDisplayPos, displayPos, timeStacker) + new Vector2(0f, outRad));
            container.Redraw(shouldForceDirty: true, shouldUpdateDepth: false);

            //updating label after container.Redraw seems to fix some exceptions
            if (Options.showLabel?.Value == true)
                label.text = labelText;
        }


        public void Destroy()
        {
            ClearSlots();
            crosshair?.Destroy();
            label?.RemoveFromContainer();
            container?.RemoveFromContainer();
        }


        public class Slot
        {
            public bool hover;
            public bool actionEnabled = true;
            public RadialMenu menu;
            public TriangleMesh background;
            public Vector2 curPos, prevPos;
            public Color curBgColor, curIconColor = Color.white;
            public FNode icon = null;
            public string name = "pixel";
            public string tooltip = null;
            public bool isLabel = false;
            Color hoverBgColor = new Color(1f, 1f, 1f, 0.4f);
            Color noneBgColor = new Color(0f, 0f, 0f, 0.2f);


            public Slot(RadialMenu menu)
            {
                this.menu = menu;
                prevPos = menu.displayPos;
                curPos = menu.displayPos;
                curBgColor = noneBgColor;
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

                for (int i = 0; i < 9; i++) { //this for-loop is heavily adapted from BeastMaster
                    list.Add(new TriangleMesh.Triangle(i, i + 1, i + 10));
                    list.Add(new TriangleMesh.Triangle(i + 10, i + 10 + 1, i + 1));
                }

                background = new TriangleMesh("Futile_White", list.ToArray(), true, false);
                if (isLabel) {
                    icon = new FLabel(Custom.GetFont(), name);
                } else {
                    icon = new FSprite(name, true);
                }
                container.AddChild(background);
                container.AddChild(icon);
                background.MoveToBack();
            }


            public void DrawSprites(float timeStacker)
            {
                float slotDegrees = (float)(360f / menu.slots.Count);
                int slotIndex = menu.slots.IndexOf(this);
                float start = slotDegrees * slotIndex;
                float end = slotDegrees + start;

                curBgColor = Color.Lerp(curBgColor, hover ? hoverBgColor : noneBgColor, 0.1f);
                background.color = curBgColor;
                if (icon is FLabel)
                    (icon as FLabel).color = curIconColor;
                if (icon is FSprite)
                    (icon as FSprite).color = curIconColor;

                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker);

                for (int i = 0; i < 10; i++) { //this for-loop is heavily adapted from BeastMaster
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
    }
}
