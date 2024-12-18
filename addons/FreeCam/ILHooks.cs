﻿using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace FreeCam
{
    public static class ILHooks
    {
        public static void Apply()
        {
            //change RoomCamera update
            IL.RoomCamera.Update += RoomCameraUpdateIL;

            //prevent changing rooms from taking over camera control
            IL.ShortcutHandler.Update += ShortcutHandlerUpdateIL;

            //prevent forced VirtualMicrophone position
            IL.VirtualMicrophone.Update += VirtualMicrophoneUpdateIL;

            //delegate at framerate
            IL.RainWorldGame.RawUpdate += RainWorldGameRawUpdateIL;

            //delegate at tickrate
            IL.RainWorldGame.Update += RainWorldGameUpdateIL;
        }


        public static void Unapply()
        {
            IL.RoomCamera.Update -= RoomCameraUpdateIL;
            IL.ShortcutHandler.Update -= ShortcutHandlerUpdateIL;
            IL.VirtualMicrophone.Update -= VirtualMicrophoneUpdateIL;
            IL.RainWorldGame.RawUpdate -= RainWorldGameRawUpdateIL;
            IL.RainWorldGame.Update -= RainWorldGameUpdateIL;
        }


        //change RoomCamera update
        static void RoomCameraUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //============================= disable GetCameraBestIndex() call =============================
            //move cursor after function call
            try {
                c.GotoNext(MoveType.After,
                    i => i.MatchCall<RoomCamera>("GetCameraBestIndex")
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception pt1: " + ex.ToString());
                return;
            }

            //create label to jump to if function call is skipped
            ILLabel gcbiSkipCond = c.MarkLabel();

            //move cursor before function call
            c.Index--;

            //insert condition (uses 'this' (RoomCamera) which is already on stack)
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
                return FreeCamManager.IsEnabled(obj?.cameraNumber ?? -1);
            });

            //if value is true, don't change camera within room
            c.Emit(OpCodes.Brtrue_S, gcbiSkipCond);

            //push 'this' (RoomCamera) on stack for GetCameraBestIndex() call
            c.Emit(OpCodes.Ldarg_0);
            //=============================================================================================

            //=========================== disable Jolly Co-op camera switching ============================
            c = new ILCursor(il);

            //go to if-condition where Jolly Co-op camera switching is active
            try {
                c.GotoNext(MoveType.After,
                    i => i.MatchLdfld<RainWorldGame>("wasAnArtificerDream")
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception pt2: " + ex.ToString());
                return;
            }

            //get skip instruction to call if freecam is enabled
            ILLabel jlcpSkipCond = c.Next.Operand as ILLabel;

            //go to start of if-condition
            try {
                c.GotoPrev(MoveType.Before,
                    i => i.MatchLdsfld<ModManager>("CoopAvailable"),
                    i => i.Match(OpCodes.Brfalse)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception pt3: " + ex.ToString());
                return;
            }

            //push 'this' (RoomCamera) on stack
            c.Emit(OpCodes.Ldarg_0);

            //insert condition
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
                return FreeCamManager.IsEnabled(obj?.cameraNumber ?? -1);
            });

            //if value is true, don't auto switch camera to rooms
            c.Emit(OpCodes.Brtrue_S, jlcpSkipCond);
            //=============================================================================================

            //========================= disable SplitScreen Co-op camera control ==========================
            c = new ILCursor(il);

            //go to end of if-condition where camera position is set
            try {
                c.GotoNext(MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchCall<RoomCamera>("get_screenShake"),
                    i => i.MatchStloc(out _)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception pt4: " + ex.ToString());
                return;
            }

            //create label to jump to if camera position code is skipped
            ILLabel sscoSkipCond = c.MarkLabel();

            //go to start of entire if/else-condition
            try {
                c.GotoPrev(MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<RoomCamera>("voidSeaMode"),
                    i => i.Match(OpCodes.Brfalse_S)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception pt5: " + ex.ToString());
                return;
            }

            //push 'this' (RoomCamera) on stack
            c.Emit(OpCodes.Ldarg_0);

            //insert condition
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
                return FreeCamManager.IsEnabled(obj?.cameraNumber ?? -1);
            });

            //if value is true, don't run camera position code
            c.Emit(OpCodes.Brtrue_S, sscoSkipCond);
            //=============================================================================================

            //Plugin.Logger.LogDebug(il.ToString());
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RoomCameraUpdateIL success");
        }


        //prevent changing rooms from taking over camera control
        static void ShortcutHandlerUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //move after MoveCamera() call
            try {
                c.GotoNext(MoveType.After,
                    i => i.MatchCallvirt<RoomCamera>("MoveCamera")
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("ShortcutHandlerUpdateIL exception pt1: " + ex.ToString());
                return;
            }

            //create label to jump to if freecam is enabled
            ILLabel skipCond = c.MarkLabel();

            //get index of local variable called "k" (in dnSpy) from for-loop
            int kIdx = 0;
            try {
                c.GotoPrev(MoveType.After,
                    i => i.MatchLdfld<ShortcutHandler>("betweenRoomsWaitingLobby"),
                    i => i.MatchLdloc(out kIdx)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("ShortcutHandlerUpdateIL exception pt2: " + ex.ToString());
                return;
            }

            //go to start of if-statement containing MoveCamera() call
            try {
                c.GotoPrev(MoveType.After,
                    i => i.MatchCall<ShortcutHandler>("PopOutOfBatHive"),
                    i => i.MatchLdarg(0)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("ShortcutHandlerUpdateIL exception pt3: " + ex.ToString());
                return;
            }

            //ShortcutHandler already on stack
            //push local variable "k" on stack
            c.Emit(OpCodes.Ldloc, kIdx);

            //create delegate
            Func<ShortcutHandler, int, bool> fcDelegate = ((obj, k) =>
            {
                //safety checks
                if (!(obj?.betweenRoomsWaitingLobby?.Count > k) ||
                    obj.betweenRoomsWaitingLobby[k]?.creature?.abstractCreature == null)
                    return false;

                //get camera number for this creature
                int cam = -1;
                for (int i = 0; i < obj.game?.cameras?.Length; i++)
                    if (obj.betweenRoomsWaitingLobby[k].creature.abstractCreature.FollowedByCamera(i))
                        cam = i;

                return FreeCamManager.IsEnabled(cam);
            });

            //insert condition
            c.EmitDelegate(fcDelegate);

            //if value is true, skip camera control
            //TODO, this does not skip SplitScreenCoop camera control, because that code is inserted after this label
            //TODO, if changing this for SplitScreenCoop, a load order probably has to be applied also
            //TODO, this means that currently secondary cameras change room when their followAbstractCreature changes room
            c.Emit(OpCodes.Brtrue_S, skipCond);

            //push ShortcutHandler on stack for next instructions
            c.Emit(OpCodes.Ldarg_0);

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ShortcutHandlerUpdateIL success");
        }


        //prevent forced VirtualMicrophone position
        static void VirtualMicrophoneUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //go to OnScreenPositionOfInShortCutCreature() call
            try {
                c.GotoNext(MoveType.Before,
                    i => i.MatchCallvirt<ShortcutHandler>("OnScreenPositionOfInShortCutCreature")
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("VirtualMicrophoneUpdateIL exception pt1: " + ex.ToString());
                return;
            }

            //go back to if-statement
            try {
                c.GotoPrev(MoveType.After,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<VirtualMicrophone>("camera"),
                    i => i.MatchLdfld<RoomCamera>("followAbstractCreature"),
                    i => i.Match(OpCodes.Brfalse)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("VirtualMicrophoneUpdateIL exception pt2: " + ex.ToString());
                return;
            }

            //get skip instruction to call if freecam is enabled
            ILLabel skipCond = c.Prev.Operand as ILLabel;

            //push RoomCamera on stack
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<VirtualMicrophone>(OpCodes.Ldfld, "camera");

            //insert condition
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
                return FreeCamManager.IsEnabled(obj?.cameraNumber ?? -1);
            });

            //if value is true, skip VirtualMicrophone position assignment
            c.Emit(OpCodes.Brtrue_S, skipCond);

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("VirtualMicrophoneUpdateIL success");
        }


        //delegate at framerate
        static void RainWorldGameRawUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<RainWorldGame, float>>((self, dt) =>
            {
                if (self == null || self.GamePaused || self.pauseUpdate || !self.processActive)
                    return;

                FreeCamManager.RawUpdate(self);

                if (Options.toggleKey?.Value != null && Input.GetKeyDown(Options.toggleKey.Value))
                    FreeCamManager.Toggle(self);
            });
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameRawUpdateIL success");
        }


        //delegate at tickrate
        static void RainWorldGameUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //move cursor somewhere after room update loop
            bool failed = false;
            try {
                c.GotoNext(MoveType.Before,
                    //Ldarg_0 (RainWorldGame)
                    i => i.MatchLdfld<MainLoopProcess>("manager"),
                    i => i.MatchLdfld<ProcessManager>("menuSetup"),
                    i => i.MatchCallvirt<ProcessManager.MenuSetup>("get_FastTravelInitCondition")
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RainWorldGameUpdateIL exception: " + ex?.ToString());
                failed = true;
            }
            if (failed) {
                failed = false;
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("RainWorldGameUpdateIL, first GotoNext failed, trying to recover");
                try {
                    c.GotoNext(MoveType.Before,
                        //Ldarg_0 (RainWorldGame)
                        i => i.MatchCall<RainWorldGame>("get_FirstAlivePlayer"),
                        i => i.MatchStloc(1)
                    );
                } catch (Exception ex) {
                    Plugin.Logger.LogWarning("RainWorldGameUpdateIL exception: " + ex?.ToString());
                    return;
                }
            }

            //use existing Ldarg_0 (RainWorldGame) on stack
            c.EmitDelegate<Action<RainWorldGame>>((self) =>
            {
                FreeCamManager.Update(self);
                Cursor.Update(self);
            });

            //emit Ldarg_0 (RainWorldGame) for next statement
            c.Emit(OpCodes.Ldarg_0);

            //Plugin.Logger.LogDebug(il.ToString());
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameUpdateIL success");
        }
    }
}
