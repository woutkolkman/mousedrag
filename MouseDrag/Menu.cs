using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace MouseDrag
{
    public static class MenuStarter
    {
        public static RadialMenu menu = null;
        public static bool shouldOpen = false; //signal from RawUpdate to open menu


        public static void Update(RainWorldGame game)
        {
            menu?.Update(game);

            if (shouldOpen && menu == null)
                menu = new RadialMenu(game);
            shouldOpen = false;

            if (menu?.closed == true) {
                menu.Destroy();
                menu = null;
            }
        }


        public static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            if (Input.GetMouseButtonDown(1))
                shouldOpen = true;
        }
    }


    public class RadialMenu {
        public bool closed = false; //signal MenuStarter to destroy this
        public Vector2 pos;
        public float rad = 80f;
        private bool mousePressed = false;
        public float? angle = null;
        public Vector2 mousePos(RainWorldGame game) => (Vector2)Futile.mousePosition + game.cameras[0]?.pos ?? new Vector2();


        public RadialMenu(RainWorldGame game)
        {
            Vector2 mouse = mousePos(game);
            pos = mouse;

            Plugin.Logger.LogDebug("RadialMenu opened");
        }


        //return true when item is clicked on
        public bool Update(RainWorldGame game)
        {
            Vector2 mouse = mousePos(game);
            Vector2 angleVect = (mouse - pos).normalized;

            if (game.GamePaused || game.pauseUpdate || !game.processActive)
                closed = true;

            if (angleVect == Vector2.zero) {
                angle = null;
            } else if (!Custom.DistLess(pos, mouse, rad)) {
                angle = null;
            } else {
                angle = Custom.VecToDeg(angleVect);
                if (angle < 0)
                    angle += 360f;
            }

            if (!mousePressed)
                return false;
            mousePressed = false;

            if (angle == null) {
                Plugin.Logger.LogDebug("angle null");
                closed = true;
                return false;
            } else {
                Plugin.Logger.LogDebug("angle: " + angle);
                return true;
            }
        }


        public void RawUpdate(RainWorldGame game)
        {
            if (Input.GetMouseButtonDown(0))
                mousePressed = true;
            if (Input.GetMouseButtonDown(1))
                pos = mousePos(game);
        }


        public void Destroy()
        {
            Plugin.Logger.LogDebug("RadialMenu closed");
        }
    }
}
