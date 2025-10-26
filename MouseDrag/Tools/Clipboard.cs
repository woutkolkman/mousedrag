using System.Collections.Generic;
using System;

namespace MouseDrag
{
    public static class Clipboard
    {
        public static List<AbstractPhysicalObject> cutObjects = new List<AbstractPhysicalObject>();


        public static void CutObject(PhysicalObject obj)
        {
            if (obj?.room?.game == null || obj.abstractPhysicalObject == null)
                return;

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("CutObject: " + Special.ConsistentName(obj.abstractPhysicalObject));

            cutObjects.Add(obj.abstractPhysicalObject);

            //specials
            if (obj is Oracle) {
                //cannot destroy, or game crashes on paste (result: loitering sprites when cut)
                obj.RemoveFromRoom();
                obj.abstractPhysicalObject?.Room?.RemoveEntity(obj.abstractPhysicalObject);
                return;
            }

            //store player stomach object
            if (obj is Player && (obj as Player).playerState != null) {
                try {
                    if ((obj as Player).objectInStomach is AbstractCreature) {
                        AbstractCreature swallowedObj = (obj as Player).objectInStomach as AbstractCreature;
                        if ((obj as Player).abstractCreature?.world?.GetAbstractRoom(swallowedObj.pos.room) == null)
                            swallowedObj.pos = (obj as Player).coord;

                        if (swallowedObj.world?.game?.IsStorySession == true) {
                            (obj as Player).playerState.swallowedItem = SaveState.AbstractCreatureToStringStoryWorld(swallowedObj);
                        } else if (swallowedObj.world?.game?.IsArenaSession == true) {
                            //TODO you cannot retrieve a creature from string in arena, because the 
                            //following error will occur "spawn room does not exist, creature not spawning", 
                            //same limitation as Pokéball mod
                            (obj as Player).playerState.swallowedItem = SaveState.AbstractCreatureToStringSingleRoomWorld(swallowedObj);
                        } else {
                            Plugin.Logger.LogWarning("Clipboard.CutObject, could not store swallowed creature");
                        }
                    } else {
                        (obj as Player).playerState.swallowedItem = (obj as Player).objectInStomach?.ToString();
                    }
                } catch (Exception ex) {
                    Plugin.Logger.LogWarning("Clipboard.CutObject exception: " + ex?.ToString());
                }
            }

            Destroy.DestroyObject(obj);
        }


        public static void CopyObject(PhysicalObject obj)
        {
            PhysicalObject dup = Duplicate.DuplicateObject(obj);
            CutObject(dup);
        }


        public static void PasteObject(RainWorldGame game, Room room, WorldCoordinate pos)
        {
            if (room?.world == null || room?.abstractRoom == null || game == null)
                return;
            if (cutObjects.Count <= 0)
                return;

            AbstractPhysicalObject apo = cutObjects.Pop();

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("PasteObject: " + Special.ConsistentName(apo));

            if (apo == null)
                return;
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

            //TODO, workaround bug since v1.11.3, remove when no longer bugged (or object might be updated twice every tick?)
            if (apo is SeedCob.AbstractSeedCob)// && apo.realizedObject.room != room)
                room.AddObject(apo.realizedObject);

            //restore player stomach object
            if (apo.realizedObject is Player) {
                try {
                    if (String.IsNullOrEmpty((apo.realizedObject as Player).playerState?.swallowedItem)) {
                        (apo.realizedObject as Player).objectInStomach = null;
                    } else {
                        AbstractPhysicalObject swallowedObj = null;
                        string swallowedString = (apo.realizedObject as Player).playerState.swallowedItem;

                        if (swallowedString.Contains("<oA>")) {
                            swallowedObj = SaveState.AbstractPhysicalObjectFromString(apo.world, swallowedString);
                        } else if (swallowedString.Contains("<cA>")) {
                            string[] p = swallowedString.Split(new[] {"<cA>"}, StringSplitOptions.None);
                            swallowedString = swallowedString.Replace(
                                p[2], string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", pos.ResolveRoomName() ?? pos.room.ToString(), pos.abstractNode)
                            );
                            swallowedObj = SaveState.AbstractCreatureFromString(apo.world, swallowedString, onlyInCurrentRegion: false);
                        }
                        if (swallowedObj != null)
                            swallowedObj.pos = apo.pos;

                        (apo.realizedObject as Player).objectInStomach = swallowedObj;
                        if (swallowedObj == null && !String.IsNullOrEmpty(swallowedString))
                            Plugin.Logger.LogWarning("Clipboard.PasteObject, swallowedItem string available but objectInStomach became null");
                        (apo.realizedObject as Player).playerState.swallowedItem = "";
                    }
                } catch (Exception ex) {
                    Plugin.Logger.LogWarning("Clipboard.PasteObject exception: " + ex?.ToString());
                }
            }

            if (apo.realizedObject is Watcher.SandGrub)
                Duplicate.BigSandGrubPostRealization(apo.realizedObject as Watcher.SandGrub);
        }
    }
}
