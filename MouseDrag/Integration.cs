using UnityEngine;

namespace MouseDrag
{
    public static class Integration
    {
        public static bool beastMasterEnabled = false;
        public static bool splitScreenCoopEnabled = false;
        public static bool sBCameraScrollEnabled = false;


        public static void RefreshActiveMods()
        {
            //check if mods are enabled
            for (int i = 0; i < ModManager.ActiveMods.Count; i++) {
                if (ModManager.ActiveMods[i].id == "fyre.BeastMaster")
                    beastMasterEnabled = Options.beastMasterIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "henpemaz_splitscreencoop")
                    splitScreenCoopEnabled = Options.splitScreenCoopIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "SBCameraScroll")
                    sBCameraScrollEnabled = Options.sBCameraScrollIntegration?.Value ?? true;
            }
        }


        //use in try/catch so missing assembly does not crash the game
        public static RoomCamera SplitScreenCoopCam(RainWorldGame game, out Vector2 offset)
        {
            offset = Vector2.zero;
            var mode = SplitScreenCoop.SplitScreenCoop.CurrentSplitMode;

            if (!(game?.cameras?.Length > 0))
                return null;
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.NoSplit)
                return game.cameras[0];

            int mousePosToCam = 0;
            if (Futile.mousePosition.x > game.cameras[0].sSize.x / 2f)
                mousePosToCam += 1;
            if (Futile.mousePosition.y < game.cameras[0].sSize.y / 2f)
                mousePosToCam += 2;

            if (mousePosToCam % 2 == 0 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.x -= game.cameras[0].sSize.x / 4f;

            if (mousePosToCam % 2 == 1 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.x += game.cameras[0].sSize.x / 4f;

            if (mousePosToCam > 1 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.y -= game.cameras[0].sSize.y / 4f;

            if (mousePosToCam < 2 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.y += game.cameras[0].sSize.y / 4f;

            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal)
                mousePosToCam /= 2;
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical)
                mousePosToCam %= 2;

            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen &&
                SplitScreenCoop.SplitScreenCoop.cameraZoomed[mousePosToCam])
                offset = Vector2.zero;

            return game.cameras[mousePosToCam];
        }


        //use in try/catch so missing assembly does not crash the game
        public static Vector2 SBCameraScrollExtraOffset(RoomCamera rcam, Vector2 pos, out float scale)
        {
            scale = 1f;
            if (!SBCameraScroll.RoomCameraMod.Is_Camera_Zoom_Enabled || !(rcam?.SpriteLayers?.Length > 0))
                return Vector2.zero;
            Vector2 offset = pos - (0.5f * rcam.sSize);
            scale = rcam.SpriteLayers[0].scale;
            return (offset * (1f / scale)) - offset;
        }


        //use in try/catch so missing assembly does not crash the game
        public static bool BeastMasterUsesRMB(RainWorldGame game) {
            if (BeastMaster.BeastMaster.BMSInstance?.isMenuOpen != true)
                return false;

            //check if mouse is far enough from BeastMaster menu at player or center of screen
            Player player = BeastMaster.BeastMaster.BMSInstance.lastPlayer;
            Vector2 mid = game.rainWorld.options.ScreenSize / 2f + game.cameras[0].pos;
            float magnitude = ((Vector2)Futile.mousePosition + game.cameras[0].pos - (player != null ? player.mainBodyChunk.pos : mid)).magnitude;
            //TODO change game.cameras[0] in this function if beastmaster is updated with SplitScreen Co-op support

            //return true if mouse is inside menu + extra depth around it
            return magnitude > 50f && magnitude < (float)(50 + 50 * (2 + BeastMaster.BeastMaster.BMSInstance.currentDepth));
        }
    }
}
