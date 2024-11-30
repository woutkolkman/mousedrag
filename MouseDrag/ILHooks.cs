using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace MouseDrag
{
    public static class ILHooks
    {
        public static void Apply()
        {
            //pauses creatures and objects (& gravity update)
            IL.Room.Update += RoomUpdateIL;

            //pause creatures and objects in not-loaded rooms
            IL.AbstractRoom.Update += AbstractRoomUpdateIL;

            //draw menu graphics with timestacker
            IL.RainWorldGame.GrafUpdate += RainWorldGameGrafUpdateIL;

            //forcefield & locks
            IL.BodyChunk.Update += BodyChunkUpdateIL;

            //delegate at framerate
            IL.RainWorldGame.RawUpdate += RainWorldGameRawUpdateIL;
        }


        public static void Unapply()
        {
            IL.Room.Update -= RoomUpdateIL;
            IL.AbstractRoom.Update -= AbstractRoomUpdateIL;
            IL.RainWorldGame.GrafUpdate -= RainWorldGameGrafUpdateIL;
            IL.BodyChunk.Update -= BodyChunkUpdateIL;
            IL.RainWorldGame.RawUpdate -= RainWorldGameRawUpdateIL;
        }


        //pauses creatures and objects (& gravity update)
        static void RoomUpdateIL(ILContext il)
        {
            //original code:
            //  if ((!this.game.pauseUpdate || updatableAndDeletable is IRunDuringDialog) && !flag)
            //resulting code will be similar to:
            //  if ((!this.game.pauseUpdate || updatableAndDeletable is IRunDuringDialog) && !flag && !IsObjectPaused(updatableAndDeletable))

            ILCursor c = new ILCursor(il);

            //changes for gravity update
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<Room>>((self) =>
            {
                Gravity.Update(self);
            });

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


        //pause creatures and objects in not-loaded rooms
        static void AbstractRoomUpdateIL(ILContext il)
        {
            //original code:
            //  this.entities[i].Update(timePassed);
            //resulting code will be similar to:
            //  if (!IsObjectPaused(this.entities[i]))
            //      this.entities[i].Update(timePassed);

            ILCursor c = new ILCursor(il);

            //move cursor after update call
            try {
                c.GotoNext(MoveType.After,
                    i => i.MatchLdarg(1),                                   //ldarg.1       (int timePassed)
                    i => i.MatchCallvirt("AbstractWorldEntity", "Update")   //callvirt      instance void AbstractWorldEntity::Update(int32)
                );
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("AbstractRoomUpdateIL exception: " + ex.ToString());
                return;
            }

            //create label to jump to if update is skipped
            ILLabel skipCond = c.MarkLabel();

            //move cursor before update call
            try {
                c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
            } catch (Exception ex) {
                Plugin.Logger.LogWarning("AbstractRoomUpdateIL exception: " + ex.ToString());
                return;
            }

            c.Emit(OpCodes.Ldarg_0); //push 'this' (AbstractRoom) on stack
            c.Emit(OpCodes.Ldloc_1); //push index of loop local var on stack

            //insert condition
            c.EmitDelegate<Func<AbstractRoom, int, bool>>((obj, i) =>
            {
                return Pause.IsObjectPaused(obj.entities[i]);
            });

            //if value is true, don't update object
            c.Emit(OpCodes.Brtrue_S, skipCond);

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("AbstractRoomUpdateIL success");
        }


        //draw menu graphics with timestacker
        static void RainWorldGameGrafUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<RainWorldGame, float>>((self, timeStacker) =>
            {
                MenuManager.DrawSprites(timeStacker);
            });
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameGrafUpdateIL success");
        }


        //forcefield & locks
        static void BodyChunkUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<BodyChunk>>((self) =>
            {
                Forcefield.UpdateForcefield(self);
                Lock.UpdatePosition(self);
            });
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("BodyChunkUpdateIL success");
        }


        //delegate at framerate
        static void RainWorldGameRawUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<RainWorldGame, float>>((self, dt) =>
            {
                if (self.GamePaused || self.pauseUpdate || !self.processActive)
                    return;

                MenuManager.RawUpdate(self);

                if (State.activated && !State.keyBindToolsDisabled) {
                    KeyBinds.RawUpdate(self);

                    if (Teleport.UpdateTeleportObject(self))
                        Drag.dragChunk = null;
                }

                //other checks are found in State.UpdateActivated
                if (State.activeType == Options.ActivateTypes.KeyBindPressed)
                    if (Options.activateKey?.Value != null && Input.GetKeyDown(Options.activateKey.Value))
                        State.activated = !State.activated;
            });
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("RainWorldGameRawUpdateIL success");
        }
    }
}
