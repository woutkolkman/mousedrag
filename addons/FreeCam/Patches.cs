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
                    i => i.MatchLdarg(0),
                    i => i.MatchCall<RoomCamera>("GetCameraBestIndex")
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception: " + ex.ToString());
                return;
            }

            //create label to jump to if function call is skipped
            ILLabel gcbiSkipCond = c.MarkLabel();

            //move cursor before function call
            try {
                c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomCameraUpdateIL exception: " + ex.ToString());
                return;
            }

            c.Emit(OpCodes.Ldarg_0); //push 'this' (RoomCamera) on stack

            //insert condition
            c.EmitDelegate<Func<RoomCamera, bool>>((obj) =>
            {
//                Plugin.Logger.LogDebug("delegate called");
                return FreeCam.enabled;
            });

            //if value is true, don't update object
            c.Emit(OpCodes.Brtrue_S, gcbiSkipCond);

//            Plugin.Logger.LogDebug(il.ToString());
            //=============================================================================================

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RoomCameraUpdateIL success");
        }
    }
}
