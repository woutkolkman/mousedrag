using System;
using MonoMod.RuntimeDetour;
using System.Reflection;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace FreeCam
{
    public static class Integration
    {
        public static bool splitScreenCoopEnabled = false;
        public static bool sBCameraScrollEnabled = false;
        public static bool mouseDragEnabled = false;
        public static bool jollyCoopEnabled = false;


        public static void RefreshActiveMods()
        {
            //check if mods are enabled
            for (int i = 0; i < ModManager.ActiveMods.Count; i++) {
                if (ModManager.ActiveMods[i].id == "henpemaz_splitscreencoop")
                    splitScreenCoopEnabled = Options.splitScreenCoopIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "SBCameraScroll")
                    sBCameraScrollEnabled = Options.sBCameraScrollIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "maxi-mol.mousedrag")
                    mouseDragEnabled = Options.mouseDragIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "jollycoop")
                    jollyCoopEnabled = true;
            }
        }


        //use in try/catch so missing assembly does not crash the game
        public static bool SplitScreenCoopCheckBorders(RoomCamera rcam, ref Vector2 pos)
        {
            if (rcam == null)
                return false;

            var mode = SplitScreenCoop.SplitScreenCoop.CurrentSplitMode;
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.NoSplit)
                return false; //we are not in a split mode

            Vector2 vanillaPos = rcam.CamPos(rcam.currentCameraPosition);

            //NOTE, might not entirely be the correct implementation, but it works fine
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical || 
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen) {
                pos.x = Mathf.Clamp(
                    pos.x,
                    vanillaPos.x - rcam.sSize.x / 4f,
                    vanillaPos.x + rcam.sSize.x / 4f
                );
            } else {
                pos.x = vanillaPos.x;
            }
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal || 
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen) {
                pos.y = Mathf.Clamp(
                    pos.y,
                    vanillaPos.y - rcam.sSize.y / 4f,
                    vanillaPos.y + rcam.sSize.y / 4f
                );
            } else {
                pos.y = vanillaPos.y;
            }

            return true; //we are in a split mode
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
        public static void SBCameraScrollMoveScreen(RoomCamera rcam, Vector2 movement)
        {
            var af = SBCameraScroll.RoomCameraMod.Get_Attached_Fields(rcam);
            if (af == null || rcam == null) //this code not working will be obvious
                return;

            //write new RoomCamera position within borders checked by SBCameraScroll
            rcam.lastPos = rcam.pos;
            Vector2 newPos = rcam.pos + movement;
            SBCameraScroll.RoomCameraMod.CheckBorders(rcam, ref newPos);
            rcam.pos = newPos;

            //also write SBCameraScroll camera position
            af.last_on_screen_position = rcam.lastPos;
            af.on_screen_position = rcam.pos;
        }


        //use in try/catch so missing assembly does not crash the game
        public static void SBCameraScrollPreparePos(RoomCamera rcam, int loadingCameraPos)
        {
            var af = SBCameraScroll.RoomCameraMod.Get_Attached_Fields(rcam);
            if (af == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("Integration.SBCameraScrollPreparePos, unable to set pre_loaded_camera_index");
                return;
            }
            af.pre_loaded_camera_index = loadingCameraPos;
        }


        //===================================================== Integration Hooks =====================================================
        public static class Hooks {
            static IDetour sBCameraScrollUpdateOnScreenPosition;
            static IDetour detourMouseDragRunAction, detourMouseDragReloadSlots;


            public static void Apply()
            {
                if (sBCameraScrollEnabled) {
                    try {
                        ApplySBCameraScroll();
                    } catch (System.Exception ex) {
                        Plugin.Logger.LogError("Integration.Hooks.Apply exception, unable to apply SBCameraScroll hooks: " + ex?.ToString());
                    }
                }
                if (mouseDragEnabled) {
                    try {
                        ApplyMouseDrag();
                    } catch (System.Exception ex) {
                        Plugin.Logger.LogError("Integration.Hooks.Apply exception, unable to apply Mouse Drag hooks: " + ex?.ToString());
                    }
                }
                Plugin.Logger.LogDebug("Integration.Hooks.Apply, finished applying enabled integration hook(s)");
            }


            //use in try/catch so missing assembly does not crash the game
            public static void ApplySBCameraScroll()
            {
                //hook to prevent camera from following creature with SBCameraScroll
                sBCameraScrollUpdateOnScreenPosition = new Hook(
                    typeof(SBCameraScroll.RoomCameraMod).GetMethod("UpdateOnScreenPosition", BindingFlags.Static | BindingFlags.Public),
                    typeof(Integration.Hooks).GetMethod("SBCameraScrollRoomCameraMod_UpdateOnScreenPosition_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
                );
            }


            //use in try/catch so missing assembly does not crash the game
            public static void ApplyMouseDrag()
            {
                //hook for running actions via Mouse Drag
                detourMouseDragRunAction = new Hook(
                    typeof(MouseDrag.MenuManager).GetMethod("RunAction", BindingFlags.Static | BindingFlags.Public),
                    typeof(Integration.Hooks).GetMethod("MouseDragMenuManager_RunAction_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
                );

                //hook for adding slots in Mouse Drag menu
                detourMouseDragReloadSlots = new Hook(
                    typeof(MouseDrag.MenuManager).GetMethod("ReloadSlots", BindingFlags.Static | BindingFlags.Public),
                    typeof(Integration.Hooks).GetMethod("MouseDragMenuManager_ReloadSlots_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
                );
            }


            public static void Unapply()
            {
                if (sBCameraScrollUpdateOnScreenPosition?.IsValid == true && sBCameraScrollUpdateOnScreenPosition?.IsApplied == true)
                    sBCameraScrollUpdateOnScreenPosition?.Dispose();
                if (detourMouseDragRunAction?.IsValid == true && detourMouseDragRunAction?.IsApplied == true)
                    detourMouseDragRunAction?.Dispose();
                if (detourMouseDragReloadSlots?.IsValid == true && detourMouseDragReloadSlots?.IsApplied == true)
                    detourMouseDragReloadSlots?.Dispose();
            }


            //hook to prevent camera from following creature with SBCameraScroll
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void SBCameraScrollRoomCameraMod_UpdateOnScreenPosition_RuntimeDetour(Action<RoomCamera> orig, RoomCamera room_camera)
            {
                if (FreeCamManager.IsEnabled(room_camera?.cameraNumber ?? -1))
                    return;
                orig(room_camera);
            }


            //hook for running actions via Mouse Drag
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void MouseDragMenuManager_RunAction_RuntimeDetour(Action<RainWorldGame, MouseDrag.RadialMenu.Slot, BodyChunk> orig, RainWorldGame game, MouseDrag.RadialMenu.Slot slot, BodyChunk chunk)
            {
                orig(game, slot, chunk);
                if (slot?.name == null)
                    return;
                if (slot.name == "mousedragMove" || slot.name == "mousedragUnmove")
                    FreeCamManager.Toggle(game);
            }


            //hook for adding slots in Mouse Drag menu
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static List<MouseDrag.RadialMenu.Slot> MouseDragMenuManager_ReloadSlots_RuntimeDetour(Func<RainWorldGame, MouseDrag.RadialMenu, BodyChunk, List<MouseDrag.RadialMenu.Slot>> orig, RainWorldGame game, MouseDrag.RadialMenu menu, BodyChunk chunk)
            {
                List<MouseDrag.RadialMenu.Slot> returnable = orig(game, menu, chunk);
                if (MouseDrag.MenuManager.subMenuType == MouseDrag.MenuManager.SubMenuTypes.None && chunk == null) {
                    if (Options.mouseDragIntegration?.Value != false) {
                        bool enabled = FreeCamManager.IsEnabled(game);
                        returnable.Add(new MouseDrag.RadialMenu.Slot(menu) {
                            name = enabled ? "mousedragUnmove" : "mousedragMove", //sprite already loaded via Mouse Drag spritesheet
                            tooltip = enabled ? "Disable FreeCam" : "Enable FreeCam"
                        });
                    }
                }
                return returnable;
            }
        }
        //=============================================================================================================================
    }
}
