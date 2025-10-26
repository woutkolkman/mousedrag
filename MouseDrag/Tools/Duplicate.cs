using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MouseDrag
{
    public static class Duplicate
    {
        public static PhysicalObject DuplicateObject(PhysicalObject obj)
        {
            if (obj?.room?.abstractRoom == null || obj.room.game == null)
                return null;

            WorldCoordinate coord = obj.room.GetWorldCoordinate(obj.firstChunk?.pos ?? new Vector2());
            AbstractPhysicalObject oldApo = obj?.abstractPhysicalObject ?? (obj as Creature)?.abstractCreature;
            AbstractPhysicalObject newApo = null;

            if (oldApo == null)
                return null;

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
                    if (oldApo is SeedCob.AbstractSeedCob) { //popcorn plant
                        newApo = DuplicateObjectSeedCob(oldApo);
                    } else if (oldApo is Pomegranate.AbstractPomegranate) { //vine watermelon
                        newApo = new Pomegranate.AbstractPomegranate(
                            oldApo.world, null, oldApo.pos, oldApo.ID, (oldApo as Pomegranate.AbstractPomegranate).originRoom, 
                            -1, null, (oldApo as Pomegranate.AbstractPomegranate).smashed, 
                            (oldApo as Pomegranate.AbstractPomegranate).disconnected, 
                            (oldApo as Pomegranate.AbstractPomegranate).spearmasterStabbed
                        );
                    } else if (oldApo is LobeTree.AbstractLobeTree) { //outer rim trees
                        PlacedObject newPo = new PlacedObject(PlacedObject.Type.LobeTree, null);
                        if ((oldApo as LobeTree.AbstractLobeTree).placedObjectIndex >= 0 &&
                            (oldApo as LobeTree.AbstractLobeTree).placedObjectIndex < obj.room.roomSettings?.placedObjects?.Count) {
                            var oldPo = obj.room.roomSettings.placedObjects[(oldApo as LobeTree.AbstractLobeTree).placedObjectIndex];
                            newPo.pos = oldPo.pos;
                            newPo.data = oldPo.data;
                        } else {
                            //peeked at Dev Console code
                            float radius = 60f;
                            float rotation = 0f;
                            float stemX = -200f;
                            float stemY = 0f;
                            newPo.pos = oldApo.pos.Tile.ToVector2() * 20f + new Vector2(10f, 10f);
                            (newPo.data as LobeTree.LobeTreeData).handlePos = RWCustom.Custom.DegToVec(rotation) * radius;
                            (newPo.data as LobeTree.LobeTreeData).rootOffset = new Vector2(stemX, stemY);
                        }
                        newApo = new LobeTree.AbstractLobeTree(
                            oldApo.world, AbstractPhysicalObject.AbstractObjectType.LobeTree, null, oldApo.pos, oldApo.ID, newPo
                        );
                    } else { //everything else
                        newApo = SaveState.AbstractPhysicalObjectFromString(oldApo.world, oldApo.ToString());
                    }
                    if (obj is Oracle) //iterator extra step
                        newApo.realizedObject = new Oracle(newApo, obj.room);

                    //must get new id?
                    if (Options.copyID?.Value == false)
                        newApo.ID = obj.room.game.GetNewID();

                } catch (Exception ex) {
                    Plugin.Logger.LogWarning("DuplicateObject exception: " + ex?.ToString());
                    return null;
                }
                newApo.pos = coord;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DuplicateObject, AddEntity " + newApo.type + " at " + coord.SaveToString());
            obj.room.abstractRoom.AddEntity(newApo);
            newApo.RealizeInRoom(); //actually places object/creature

            //TODO, workaround bug since v1.11.3, remove when no longer bugged (or object might be updated twice every tick?)
            if (newApo is SeedCob.AbstractSeedCob && newApo.realizedObject.room != obj.room)
                obj.room.AddObject(newApo.realizedObject);

            if (newApo.realizedObject is Watcher.SandGrub)
                BigSandGrubPostRealization(newApo.realizedObject as Watcher.SandGrub);

            return newApo.realizedObject;
        }


        public static void BigSandGrubPostRealization(Watcher.SandGrub po)
        {
            if (po?.room == null || po?.abstractCreature?.creatureTemplate != StaticWorld.GetCreatureTemplate(Watcher.WatcherEnums.CreatureTemplateType.BigSandGrub))
                return;
            Room room = po.room;
            Vector2 pos = po.DangerPos;

            //get or create a sandgrub network if it doesn't exist yet
            Watcher.SandGrubNetwork sgn = (Watcher.SandGrubNetwork)room?.updateList?.FirstOrDefault(x => x is Watcher.SandGrubNetwork);
            if (sgn == null) {
                sgn = new Watcher.SandGrubNetwork(pos, 0f, 0f, 0f, false);
                sgn.scanForBurrows = false;
                room.AddObject(sgn);
            }

            //create a new burrow at the current position
            Watcher.SandGrubBurrow sgb = new Watcher.SandGrubBurrow(null, null);
            sgb.pos = room.FindGroundBelow(pos, out sgb.dir, 200f);
            sgb.room = room;
            room.AddObject(sgb);

            //add burrow to network, an empty network is destroyed automatically in the first update call
            sgb.SetNetwork(sgn);
            sgn.burrows.Add(sgb);

            //add BigSandGrub to burrow
            sgb.grub = po;
            po.burrow = sgb;

            //TODO burrow and network are deleted from room after room is reloaded, 
            //this causes the grub to bug out and fall out of the room
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


        public static int duplicateHoldCount = 0;
        public static int duplicateHoldMin = 40;
        public static void Update()
        {
            //rapidly duplicate after one second feature
            if (Options.duplicateOneKey?.Value != null && Input.GetKey(Options.duplicateOneKey.Value)) {
                if (duplicateHoldCount >= duplicateHoldMin)
                    foreach (var obj in Select.selectedObjects)
                        Duplicate.DuplicateObject(obj);
                duplicateHoldCount++;
            } else {
                duplicateHoldCount = 0;
            }
        }
    }
}
