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
        public static bool devConsoleEnabled = false;


        public static void RefreshActiveMods() //is called via Options EventHandler
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Integration.RefreshActiveMods called");

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
                if (ModManager.ActiveMods[i].id == "slime-cubed.devconsole")
                    devConsoleEnabled = Options.devConsoleIntegration?.Value ?? true;
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
            var af = SBCameraScroll.RoomCameraMod.GetFields(rcam);
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
        public static void DevConsoleRegisterCommands()
        {
            new DevConsole.Commands.CommandBuilder("fc_freecam")
            .Help("fc_freecam [camera?]")
            .RunGame((game, args) => {
                int camNr = 0;
                if (args.Length > 0) {
                    if (int.TryParse(args[0], out int temp)) {
                        camNr = temp;
                    } else {
                        DevConsole.GameConsole.WriteLine("Parse camera failed");
                        return;
                    }
                }
                if (camNr < 0 || camNr >= FreeCamManager.freeCams.Length) {
                    DevConsole.GameConsole.WriteLine("No FreeCam object initialized for RoomCamera " + camNr + ".");
                    return;
                }
                FreeCamManager.Toggle(game, camNr);
                if (FreeCamManager.freeCams[camNr].enabled) {
                    DevConsole.GameConsole.WriteLine("Enabled freecam for RoomCamera " + camNr + ".");
                } else {
                    DevConsole.GameConsole.WriteLine("Disabled freecam for RoomCamera " + camNr + ".");
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return new string[] { "0", "1", "2", "3" };
                return null;
            })
            .Register();

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DevConsoleRegisterCommands, finished registration of commands");
        }


        //===================================================== Integration Hooks =====================================================
        public static class Hooks {
            static IDetour sBCameraScrollUpdateOnScreenPosition;
            static IDetour sBCameraScrollRoomCamera_ApplyPositionChange;
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

                //hook that applies correct camera position after camera moved to a new room
                sBCameraScrollRoomCamera_ApplyPositionChange = new Hook(
                    typeof(SBCameraScroll.RoomCameraMod).GetMethod("RoomCamera_ApplyPositionChange", BindingFlags.Static | BindingFlags.NonPublic),
                    typeof(Integration.Hooks).GetMethod("SBCameraScrollRoomCameraMod_RoomCamera_ApplyPositionChange_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
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
                if (sBCameraScrollRoomCamera_ApplyPositionChange?.IsValid == true && sBCameraScrollRoomCamera_ApplyPositionChange?.IsApplied == true)
                    sBCameraScrollRoomCamera_ApplyPositionChange?.Dispose();
                if (detourMouseDragRunAction?.IsValid == true && detourMouseDragRunAction?.IsApplied == true)
                    detourMouseDragRunAction?.Dispose();
                if (detourMouseDragReloadSlots?.IsValid == true && detourMouseDragReloadSlots?.IsApplied == true)
                    detourMouseDragReloadSlots?.Dispose();
            }


            //hook to prevent camera from following creature with SBCameraScroll
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void SBCameraScrollRoomCameraMod_UpdateOnScreenPosition_RuntimeDetour(Action<RoomCamera> orig, RoomCamera room_camera)
            {
                //NOTE, not calling orig is not preferred, but SBCameraScroll does it a lot by itself so it should be fine here
                if (FreeCamManager.IsEnabled(room_camera?.cameraNumber ?? -1))
                    return;
                orig(room_camera);
            }


            //hook that applies correct camera position after camera moved to a new room
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void SBCameraScrollRoomCameraMod_RoomCamera_ApplyPositionChange_RuntimeDetour(Action<On.RoomCamera.orig_ApplyPositionChange, RoomCamera> orig, On.RoomCamera.orig_ApplyPositionChange csOrig, RoomCamera room_camera)
            {
                //this hook hooks another hook, maybe bad code but i couldn't think of another solution
                orig(csOrig, room_camera);

                //check if FreeCam is enabled, and a new position can be assigned
                if (!FreeCamManager.IsEnabled(room_camera?.cameraNumber ?? -1))
                    return;
                FreeCam fc = FreeCamManager.freeCams[room_camera.cameraNumber];
                if (fc?.sBCameraScrollNewPos == null)
                    return;
                Vector2 newPos = fc.sBCameraScrollNewPos.Value;
                fc.sBCameraScrollNewPos = null;

                //get attached fields from SBCameraScroll
                var af = SBCameraScroll.RoomCameraMod.GetFields(room_camera);
                if (af == null) {
                    if (Options.logDebug?.Value != false)
                        Plugin.Logger.LogDebug("Integration.Hooks.SBCameraScrollRoomCameraMod_RoomCamera_ApplyPositionChange_RuntimeDetour, unable to set RoomCamera position");
                    return;
                }

                //apply position
                room_camera.lastPos = newPos;
                room_camera.pos = newPos;
                af.last_on_screen_position = newPos;
                af.on_screen_position = newPos;
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
