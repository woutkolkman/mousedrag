using System;
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
                menu = new RadialMenu();
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


        public RadialMenu()
        {
            Plugin.Logger.LogDebug("RadialMenu opened");
        }


        public void Update(RainWorldGame game)
        {
            Vector2 mousePos = (Vector2)Futile.mousePosition + game.cameras[0]?.pos ?? new Vector2();
        }


        public void RawUpdate(RainWorldGame game)
        {
            if (Input.GetMouseButtonDown(0))
                closed = true;
        }


        public void InitiateSprites()
        {

        }


        public void DrawSprites()
        {

        }


        public void Destroy()
        {
            Plugin.Logger.LogDebug("RadialMenu closed");
        }
    }
}
