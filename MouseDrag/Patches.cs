using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace MouseDrag
{
    public static class Patches
    {
        public static void Apply()
        {
            //pauses creatures and objects
            IL.Room.Update += RoomUpdateIL;
        }


        public static void Unapply()
        {
            //TODO
        }


        //pauses creatures and objects
        static void RoomUpdateIL(ILContext il)
        {
            //original code:
            //  if ((!this.game.pauseUpdate || updatableAndDeletable is IRunDuringDialog) && !flag)
            //resulting code will be:
            //  if ((!this.game.pauseUpdate || updatableAndDeletable is IRunDuringDialog) && !flag && !IsObjectPaused(updatableAndDeletable))

            ILCursor c = new ILCursor(il);

            try {
                c.GotoNext(MoveType.After,
                    i => i.MatchIsinst<MoreSlugcats.IRunDuringDialog>(),    //isinst        MoreSlugcats.IRunDuringDialog
                    i => i.Match(OpCodes.Brfalse_S),                        //brfalse.s     776 (09DF) ldloc.s V10 (10)
                    i => i.MatchLdloc(11),                                  //ldloc.s       V11 (11)
                    i => i.Match(OpCodes.Brtrue_S)                          //brtrue.s      776 (09DF) ldloc.s V10 (10)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("RoomUpdateIL exception: " + ex.ToString());
                return;
            }

            //get skip instruction to call if object is paused
            ILLabel skipCond = c.Prev.Operand as ILLabel;

            //insert condition
            c.Emit(OpCodes.Ldloc, 10); //push updatableAndDeletable local var on stack
            c.EmitDelegate<Func<UpdatableAndDeletable, bool>>(obj =>
            {
                Stun.UpdateObjectStunned(obj);
                return Pause.IsObjectPaused(obj);
            });

            //if value is true, don't update object
            c.Emit(OpCodes.Brtrue_S, skipCond);

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RoomUpdateIL success");
        }
    }
}
