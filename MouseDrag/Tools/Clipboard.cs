using System.Collections.Generic;
using System;

namespace MouseDrag
{
    static class Clipboard
    {
        public static List<AbstractPhysicalObject> cutObjects = new List<AbstractPhysicalObject>();


        public static void CutObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = Tools.dragChunk?.owner;
            if (obj?.room?.game == null || obj.abstractPhysicalObject == null)
                return;

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("CutObject: " + obj.abstractPhysicalObject);

            cutObjects.Add(obj.abstractPhysicalObject);

            //specials
            if (obj is Oracle) {
                //cannot destroy, or game crashes on paste (result: loitering sprites when cut)
                obj.RemoveFromRoom();
                obj.abstractPhysicalObject?.Room?.RemoveEntity(obj.abstractPhysicalObject);
                return;
            }

            //store player stomach object
            if (obj is Player && (obj as Player).playerState != null)
                (obj as Player).playerState.swallowedItem = (obj as Player).objectInStomach?.ToString();

            Tools.DestroyObject(obj);
        }


        public static void PasteObject(RainWorldGame game, Room room, WorldCoordinate pos)
        {
            if (room?.world == null || room?.abstractRoom == null || game == null)
                return;
            if (cutObjects.Count <= 0)
                return;

            AbstractPhysicalObject apo = cutObjects.Pop();

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("PasteObject: " + apo?.ToString());

            apo.pos = pos;
            apo.world = room.world;
            (apo as AbstractCreature)?.abstractAI?.NewWorld(room.world);

            //prevents many warnings/exceptions with creatures by reinstantiating realizedObject
            if (apo is AbstractCreature)
                apo.Abstractize(pos);

            //undelete
            if (apo.realizedObject != null) {
                apo.realizedObject.slatedForDeletetion = false;
                apo.realizedObject.room = room;
            }

            //specials
            if (apo.realizedObject is Oracle) {
                Oracle o = apo.realizedObject as Oracle;
                if (o.myScreen != null)
                    o.myScreen.room = room;
                if (o.oracleBehavior != null && game.FirstAlivePlayer?.realizedCreature is Player)
                    o.oracleBehavior.player = game.FirstAlivePlayer?.realizedCreature as Player;
            }

            room.abstractRoom.AddEntity(apo);
            apo.RealizeInRoom();

            //restore player stomach object
            if (apo.realizedObject is Player) {
                if (String.IsNullOrEmpty((apo.realizedObject as Player).playerState?.swallowedItem)) {
                    (apo.realizedObject as Player).objectInStomach = null;
                } else {
                    (apo.realizedObject as Player).objectInStomach = SaveState.AbstractPhysicalObjectFromString(apo.world, (apo.realizedObject as Player).playerState.swallowedItem);
                    (apo.realizedObject as Player).playerState.swallowedItem = "";
                }
            }
        }
    }
}
