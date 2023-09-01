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

            cutObjects.Add(obj.abstractPhysicalObject);

            //store player stomach object
            if (obj is Player && (obj as Player).playerState != null)
                (obj as Player).playerState.swallowedItem = (obj as Player).objectInStomach?.ToString();

            Tools.DestroyObject(obj);
        }


        public static void PasteObject(RainWorldGame game, Room room, WorldCoordinate pos)
        {
            if (room?.world == null || room?.abstractRoom == null)
                return;
            if (cutObjects.Count <= 0)
                return;

            AbstractPhysicalObject apo = cutObjects.Pop();
            apo.pos = pos;
            apo.world = room.world;
            (apo as AbstractCreature)?.abstractAI?.NewWorld(room.world);
            apo.Abstractize(pos);
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
