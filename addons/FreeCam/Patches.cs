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
        }


        public static void Unapply()
        {
            IL.RoomCamera.Update -= RoomCameraUpdateIL;
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
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception: " + ex.ToString());
                return;
            }

            //create label to jump to if function call is skipped
            ILLabel gcbiSkipCond = c.MarkLabel();

            //move cursor before function call
            c.Index--;

            //insert condition (uses 'this' (RoomCamera) which is already on stack)
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
                return FreeCam.enabled;
            });

            //if value is true, don't update object
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
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception: " + ex.ToString());
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
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception: " + ex.ToString());
                return;
            }

            //push 'this' (RoomCamera) on stack
            c.Emit(OpCodes.Ldarg_0);

            //insert condition
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
                return FreeCam.enabled;
            });

            //if value is true, don't update object
            c.Emit(OpCodes.Brtrue_S, jlcpSkipCond);
            //=============================================================================================

            //Plugin.Logger.LogDebug(il.ToString());
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RoomCameraUpdateIL success");
        }
    }
}
