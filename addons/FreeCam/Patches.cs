using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace FreeCam
{
    public static class Patches
    {
        public static void Apply()
        {
            //change RoomCamera update
            IL.RoomCamera.Update += RoomCameraUpdateIL;

            //prevent changing rooms from taking over camera control
            IL.ShortcutHandler.Update += ShortcutHandlerUpdateIL;
        }


        public static void Unapply()
        {
            IL.RoomCamera.Update -= RoomCameraUpdateIL;
            IL.ShortcutHandler.Update -= ShortcutHandlerUpdateIL;
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

            //insert condition
            c.EmitDelegate<Func<ShortcutHandler, int, bool>>((obj, k) =>
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

            //if value is true, skip camera control
            //TODO: this does not skip SplitScreenCoop camera control, because that code is inserted after this label
            c.Emit(OpCodes.Brtrue_S, skipCond);

            //push ShortcutHandler on stack for next instructions
            c.Emit(OpCodes.Ldarg_0);

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ShortcutHandlerUpdateIL success");
        }
    }
}
