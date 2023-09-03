using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MouseDrag
{
    public static class Duplicate
    {
        public static void DuplicateObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = Drag.dragChunk?.owner;
            if (obj?.room?.abstractRoom == null || obj.room.game == null)
                return;

            WorldCoordinate coord = obj.room.GetWorldCoordinate(obj.firstChunk?.pos ?? new Vector2());
            AbstractPhysicalObject oldApo = obj?.abstractPhysicalObject ?? (obj as Creature)?.abstractCreature;
            AbstractPhysicalObject newApo = null;

            if (oldApo == null)
                return;

            if (obj is Creature) {
                EntityID id = Options.copyID?.Value == false ? obj.room.game.GetNewID() : oldApo.ID;
                newApo = new AbstractCreature(oldApo.world, (obj as Creature).Template, null, coord, id);
                if ((oldApo as AbstractCreature)?.state != null)
                    (newApo as AbstractCreature).state?.LoadFromString(Regex.Split((oldApo as AbstractCreature).state.ToString(), "<cB>"));

                //prevents exception when duplicating player
                if (obj is Player && (obj as Player).playerState != null && !(obj as Player).isNPC) {
                    PlayerState ps = (obj as Player).playerState;
                    (newApo as AbstractCreature).state = new PlayerState(newApo as AbstractCreature, ps.playerNumber, ps.slugcatCharacter, false);
                }

            } else {
                try {
                    newApo = SaveState.AbstractPhysicalObjectFromString(oldApo.world, oldApo.ToString());

                    //specials
                    if (oldApo is SeedCob.AbstractSeedCob) //popcorn plant
                        newApo = DuplicateObjectSeedCob(oldApo);
                    if (obj is Oracle) //iterator
                        newApo.realizedObject = new Oracle(newApo, obj.room);

                    //must get new id?
                    if (Options.copyID?.Value == false)
                        newApo.ID = obj.room.game.GetNewID();

                } catch (Exception ex) {
                    Plugin.Logger.LogWarning("DuplicateObject exception: " + ex.ToString());
                    return;
                }
                newApo.pos = coord;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DuplicateObject, AddEntity " + newApo.type + " at " + coord.SaveToString());
            obj.room.abstractRoom.AddEntity(newApo);
            newApo.RealizeInRoom(); //actually places object/creature
        }


        //popcorn plants are not as easy to duplicate because of the way they are added to rooms
        private static AbstractPhysicalObject DuplicateObjectSeedCob(AbstractPhysicalObject oldApo)
        {
            AbstractPhysicalObject newApo = new SeedCob.AbstractSeedCob(
                oldApo.world, null, oldApo.pos, oldApo.ID,
                oldApo.realizedObject.room.abstractRoom.index, -1, dead: (oldApo as SeedCob.AbstractSeedCob).dead, null
            );
            (newApo as AbstractConsumable).isConsumed = (oldApo as AbstractConsumable).isConsumed;
            oldApo.realizedObject.room.abstractRoom.entities.Add(newApo);
            newApo.Realize();

            string[] duplicateArrayFields = { "seedPositions", "seedsPopped", "leaves" };
            string[] copyFields = { "totalSprites", "stalkSegments", "cobSegments", "placedPos", "rootPos", "rootDir", "cobDir", "stalkLength", "open" };
            FieldInfo[] seedCobObjectFields = oldApo.realizedObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fi in seedCobObjectFields) {
                if (duplicateArrayFields.Any(s => fi.Name.Contains(s)))
                    fi.SetValue(newApo.realizedObject, (fi.GetValue(oldApo.realizedObject) as Array).Clone());
                if (copyFields.Any(s => fi.Name.Contains(s)))
                    fi.SetValue(newApo.realizedObject, fi.GetValue(oldApo.realizedObject));
            }
            return newApo;
        }
    }
}
