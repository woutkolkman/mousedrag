﻿using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using UnityEngine;

namespace MouseDrag
{
    public static class Integration
    {
        public static bool beastMasterEnabled = false;
        public static bool splitScreenCoopEnabled = false;
        public static bool sBCameraScrollEnabled = false;
        public static bool regionKitEnabled = false;
        public static bool devConsoleEnabled = false;


        public static void RefreshActiveMods() //is called via Options EventHandler
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Integration.RefreshActiveMods called");

            //check if mods are enabled
            for (int i = 0; i < ModManager.ActiveMods.Count; i++) {
                if (ModManager.ActiveMods[i].id == "fyre.BeastMaster")
                    beastMasterEnabled = Options.beastMasterIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "henpemaz_splitscreencoop")
                    splitScreenCoopEnabled = Options.splitScreenCoopIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "SBCameraScroll")
                    sBCameraScrollEnabled = Options.sBCameraScrollIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "regionkit")
                    regionKitEnabled = Options.regionKitIntegration?.Value ?? true;
                if (ModManager.ActiveMods[i].id == "slime-cubed.devconsole")
                    devConsoleEnabled = Options.devConsoleIntegration?.Value ?? true;
            }
        }


        //use in try/catch so missing assembly does not crash the game
        public static RoomCamera SplitScreenCoopCam(RainWorldGame game, out Vector2 offset)
        {
            offset = Vector2.zero;
            var mode = SplitScreenCoop.SplitScreenCoop.CurrentSplitMode;

            if (!(game?.cameras?.Length > 0))
                return null;
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.NoSplit)
                return game.cameras[0];

            int mousePosToCam = 0;
            if (Futile.mousePosition.x > game.cameras[0].sSize.x / 2f)
                mousePosToCam += 1;
            if (Futile.mousePosition.y < game.cameras[0].sSize.y / 2f)
                mousePosToCam += 2;

            if (mousePosToCam % 2 == 0 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.x -= game.cameras[0].sSize.x / 4f;

            if (mousePosToCam % 2 == 1 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.x += game.cameras[0].sSize.x / 4f;

            if (mousePosToCam > 1 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.y -= game.cameras[0].sSize.y / 4f;

            if (mousePosToCam < 2 &&
                (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal ||
                mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen))
                offset.y += game.cameras[0].sSize.y / 4f;

            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitHorizontal)
                mousePosToCam /= 2;
            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.SplitVertical)
                mousePosToCam %= 2;

            if (mode == SplitScreenCoop.SplitScreenCoop.SplitMode.Split4Screen &&
                SplitScreenCoop.SplitScreenCoop.cameraZoomed[mousePosToCam])
                offset = Vector2.zero;

            return game.cameras[mousePosToCam];
        }


        //use in try/catch so missing assembly does not crash the game
        public static Vector2 SBCameraScrollExtraOffset(RoomCamera rcam, Vector2 pos, out float scale)
        {
            scale = 1f;
            if (!SBCameraScroll.RoomCameraMod.Is_Camera_Zoom_Enabled || !(rcam?.SpriteLayers?.Length > 0))
                return Vector2.zero;
            Vector2 offset = pos - (0.5f * rcam.sSize);
            scale = rcam.SpriteLayers[0].scale;
            return (offset * (1f / scale)) - offset;
        }


        //use in try/catch so missing assembly does not crash the game
        public static bool BeastMasterUsesRMB(RainWorldGame game) {
            if (BeastMaster.BeastMaster.BMSInstance?.isMenuOpen != true)
                return false;

            //check if mouse is far enough from BeastMaster menu at player or center of screen
            Player player = BeastMaster.BeastMaster.BMSInstance.lastPlayer;
            Vector2 mid = game.rainWorld.options.ScreenSize / 2f + game.cameras[0].pos;
            float magnitude = ((Vector2)Futile.mousePosition + game.cameras[0].pos - (player != null ? player.mainBodyChunk.pos : mid)).magnitude;
            //TODO change game.cameras[0] in this function if beastmaster is updated with SplitScreen Co-op support

            //return true if mouse is inside menu + extra depth around it
            return magnitude > 50f && magnitude < (float)(50 + 50 * (2 + BeastMaster.BeastMaster.BMSInstance.currentDepth));
        }


        //value can be used in Dev Console to execute commands for a specific object
        public static string DevConsoleGetSelector(AbstractPhysicalObject apo)
        {
            if (apo == null)
                return "";
            string selector = apo.ID.ToString();
            if (apo is AbstractCreature && (apo as AbstractCreature).creatureTemplate != null) {
                selector += ",type=" + (apo as AbstractCreature).creatureTemplate.type;
            } else if (apo.type != null) {
                selector += ",type=" + apo.type.ToString();
            }
            if (apo is AbstractCreature && (apo as AbstractCreature).state != null)
                selector += (apo as AbstractCreature).state.alive ? ",alive" : ",dead";
            return selector;
        }


        //use in try/catch so missing assembly does not crash the game
        public static void DevConsoleOpen(string selector = null)
        {
            if (!string.IsNullOrEmpty(selector)) {
                Menu.Remix.UniClipboard.SetText(selector);
                DevConsole.GameConsole.WriteLine("Copied object selector to clipboard: " + selector);
            }
            DevConsole.GameConsole.ForceOpen();
        }


        //use in try/catch so missing assembly does not crash the game
        public static void DevConsoleRegisterCommands()
        {
            new DevConsole.Commands.CommandBuilder("md_pause")
            .Help("md_pause [selector] [action]")
            .RunGame((game, args) => {
                if (args.Length != 2) {
                    DevConsole.GameConsole.WriteLine("Expected 2 arguments");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                foreach (var apo in list) {
                    if (args[1] == "toggle") {
                        Pause.TogglePauseObject(apo);
                    } else if (args[1] == "on") {
                        if (!Pause.IsObjectPaused(apo))
                            Pause.TogglePauseObject(apo);
                    } else if (args[1] == "off") {
                        if (Pause.IsObjectPaused(apo))
                            Pause.TogglePauseObject(apo);
                    } else {
                        DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                        break;
                    }
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return DevConsole.Selection.Autocomplete;
                if (args.Length == 1) return new string[] { "on", "off", "toggle" };
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_pause_all")
            .Help("md_pause_all [types] [action]")
            .RunGame((game, args) => {
                if (args.Length != 2) {
                    DevConsole.GameConsole.WriteLine("Expected 2 arguments");
                    return;
                }
                bool creatures = args[0] == "creatures" || args[0] == "objects";
                bool items = args[0] == "items" || args[0] == "objects";
                if (args[1] == "toggle") {
                    if (creatures) Pause.pauseAllCreatures = !Pause.pauseAllCreatures;
                    if (items) Pause.pauseAllItems = !Pause.pauseAllItems;
                } else if (args[1] == "on") {
                    if (creatures) Pause.pauseAllCreatures = true;
                    if (items) Pause.pauseAllItems = true;
                } else if (args[1] == "off") {
                    if (creatures) Pause.pauseAllCreatures = false;
                    if (items) Pause.pauseAllItems = false;
                } else {
                    DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return new string[] { "creatures", "items", "objects" };
                if (args.Length == 1) return new string[] { "on", "off", "toggle" };
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_duplicate")
            .Help("md_duplicate [selector]")
            .RunGame((game, args) => {
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 argument");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--)
                    Duplicate.DuplicateObject(list.ElementAt(i)?.realizedObject);
            })
            .AutoComplete(new string[][] { DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_clipboard")
            .Help("md_clipboard [action?] [selector?/pos?]")
            .RunGame((game, args) => {
                if (args.Length == 0) {
                    foreach (AbstractPhysicalObject apo in Clipboard.cutObjects)
                        DevConsole.GameConsole.WriteLine(Special.ConsistentName(apo));
                    return;
                }
                if (args.Length != 2) {
                    DevConsole.GameConsole.WriteLine("Expected 0 or 2 arguments");
                    return;
                }
                if (args[0] == "cut") {
                    var list = DevConsole.Selection.SelectAbstractObjects(game, args[1]);
                    for (int i = list.Count() - 1; i >= 0; i--)
                        Clipboard.CutObject(list.ElementAt(i)?.realizedObject);
                } else if (args[0] == "copy") {
                    var list = DevConsole.Selection.SelectAbstractObjects(game, args[1]);
                    for (int i = list.Count() - 1; i >= 0; i--)
                        Clipboard.CopyObject(list.ElementAt(i)?.realizedObject);
                } else if (args[0] == "paste") {
                    if (!DevConsole.Positioning.TryGetPosition(game, args[1], out var pos) || pos.Room?.realizedRoom == null)
                        return;
                    Clipboard.PasteObject(game, pos.Room.realizedRoom, pos.Room.realizedRoom.GetWorldCoordinate(pos.Pos));
                } else {
                    DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return new string[] { "cut", "copy", "paste" };
                if (args.Length == 1) {
                    if (args[0] == "cut" || args[0] == "copy") {
                        return DevConsole.Selection.Autocomplete;
                    } else if (args[0] == "paste") {
                        return DevConsole.Positioning.Autocomplete;
                    }
                }
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_safari_toggle")
            .Help("md_safari_toggle [selector] [player?]")
            .RunGame((game, args) => {
                if (args.Length < 1 || args.Length > 2) {
                    DevConsole.GameConsole.WriteLine("Expected 1 or 2 arguments");
                    return;
                }
                int pNr = -1;
                if (args.Length > 1 && int.TryParse(args[1], out int temp))
                    pNr = temp;
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--)
                    Control.ToggleControl(game, list.ElementAt(i)?.realizedObject as Creature, pNr);
            })
            .AutoComplete(new string[][]{ DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_safari_release_all")
            .Help("md_safari_release_all [player?]")
            .Run((args) => {
                int pNr = -1;
                if (args.Length > 0 && int.TryParse(args[0], out int temp))
                    pNr = temp;
                Control.ReleaseControlAll(pNr);
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_safari_cycle_camera")
            .RunGame((game, args) => {
                Control.CycleCamera(game);
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_forcefield")
            .Help("md_forcefield [selector] [action] [bodychunk?]")
            .RunGame((game, args) => {
                if (args.Length < 2 || args.Length > 3) {
                    DevConsole.GameConsole.WriteLine("Expected 2 or 3 arguments");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--) {
                    if (list.ElementAt(i)?.realizedObject?.bodyChunks == null)
                        continue;
                    BodyChunk[] bcs = null;
                    if (args.Length < 3 || args[2] == "main") {
                        bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.firstChunk };
                        if (list.ElementAt(i).realizedObject is Creature && 
                            (list.ElementAt(i).realizedObject as Creature).mainBodyChunk != null)
                            bcs = new BodyChunk[] { (list.ElementAt(i).realizedObject as Creature).mainBodyChunk };
                    } else if (args[2] == "first") {
                        bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.firstChunk };
                    } else if (args[2] == "random") {
                        bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.RandomChunk };
                    } else if (args[2] == "all") {
                        bcs = list.ElementAt(i).realizedObject.bodyChunks;
                    } else if (int.TryParse(args[2], out int bcI)) {
                        if (bcI >= 0 && bcI < list.ElementAt(i).realizedObject.bodyChunks.Length) {
                            bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.bodyChunks[bcI] };
                        } else {
                            DevConsole.GameConsole.WriteLine("BodyChunk with index " + bcI + " does not exist for " + Special.ConsistentName(list.ElementAt(i)));
                            continue;
                        }
                    } else {
                        DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                        break;
                    }
                    if (args[1] != "toggle" && args[1] != "on" && args[1] != "off") {
                        DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                        break;
                    }
                    for (int j = 0; j < bcs?.Length; j++) {
                        if (args[1] == "toggle") {
                            Forcefield.SetForcefield(bcs[j], toggle: true, apply: true);
                        } else if (args[1] == "on") {
                            Forcefield.SetForcefield(bcs[j], toggle: false, apply: true);
                        } else if (args[1] == "off") {
                            Forcefield.SetForcefield(bcs[j], toggle: false, apply: false);
                        }
                    }
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return DevConsole.Selection.Autocomplete;
                if (args.Length == 1) return new string[] { "on", "off", "toggle" };
                if (args.Length == 2) return new string[] { "main", "first", "random", "all" };
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_tame")
            .Help("md_tame [selector]")
            .RunGame((game, args) => {
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 argument");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--)
                    Tame.TameCreature(game, list.ElementAt(i)?.realizedObject);
            })
            .AutoComplete(new string[][]{ DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_untame")
            .Help("md_untame [selector]")
            .RunGame((game, args) => {
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 argument");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--)
                    Tame.ClearRelationships(list.ElementAt(i)?.realizedObject);
            })
            .AutoComplete(new string[][]{ DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_stun")
            .Help("md_stun [selector] [action]")
            .RunGame((game, args) => {
                if (args.Length != 2) {
                    DevConsole.GameConsole.WriteLine("Expected 2 arguments");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                if (args[1] == "toggle") {
                    for (int i = list.Count() - 1; i >= 0; i--)
                        Stun.StunObject(list.ElementAt(i), toggle: true, apply: true);
                } else if (args[1] == "on") {
                    for (int i = list.Count() - 1; i >= 0; i--)
                        Stun.StunObject(list.ElementAt(i), toggle: false, apply: true);
                } else if (args[1] == "off") {
                    for (int i = list.Count() - 1; i >= 0; i--)
                        Stun.StunObject(list.ElementAt(i), toggle: false, apply: false);
                } else {
                    DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return DevConsole.Selection.Autocomplete;
                if (args.Length == 1) return new string[] { "on", "off", "toggle" };
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_lock")
            .Help("md_lock [selector] [action] [bodychunk?]")
            .RunGame((game, args) => {
                if (args.Length < 2 || args.Length > 3) {
                    DevConsole.GameConsole.WriteLine("Expected 2 or 3 arguments");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--) {
                    if (list.ElementAt(i)?.realizedObject?.bodyChunks == null)
                        continue;
                    BodyChunk[] bcs = null;
                    if (args.Length < 3 || args[2] == "all") {
                        bcs = list.ElementAt(i).realizedObject.bodyChunks;
                    } else if (args[2] == "main") {
                        bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.firstChunk };
                        if (list.ElementAt(i).realizedObject is Creature && 
                            (list.ElementAt(i).realizedObject as Creature).mainBodyChunk != null)
                            bcs = new BodyChunk[] { (list.ElementAt(i).realizedObject as Creature).mainBodyChunk };
                    } else if (args[2] == "first") {
                        bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.firstChunk };
                    } else if (args[2] == "random") {
                        bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.RandomChunk };
                    } else if (int.TryParse(args[2], out int bcI)) {
                        if (bcI >= 0 && bcI < list.ElementAt(i).realizedObject.bodyChunks.Length) {
                            bcs = new BodyChunk[] { list.ElementAt(i).realizedObject.bodyChunks[bcI] };
                        } else {
                            DevConsole.GameConsole.WriteLine("BodyChunk with index " + bcI + " does not exist for " + Special.ConsistentName(list.ElementAt(i)));
                            continue;
                        }
                    } else {
                        DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                        break;
                    }
                    if (args[1] != "toggle" && args[1] != "on" && args[1] != "off") {
                        DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                        break;
                    }
                    for (int j = 0; j < bcs?.Length; j++) {
                        if (args[1] == "toggle") {
                            Lock.SetLock(bcs[j], toggle: true, apply: true);
                        } else if (args[1] == "on") {
                            Lock.SetLock(bcs[j], toggle: false, apply: true);
                        } else if (args[1] == "off") {
                            Lock.SetLock(bcs[j], toggle: false, apply: false);
                        }
                    }
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return DevConsole.Selection.Autocomplete;
                if (args.Length == 1) return new string[] { "on", "off", "toggle" };
                if (args.Length == 2) return new string[] { "all", "main", "first", "random" };
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_gravity")
            .Help("md_gravity [type?]")
            .Run((args) => {
                if (args.Length > 1) {
                    DevConsole.GameConsole.WriteLine("Expected 0 or 1 arguments");
                    return;
                }
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine(Gravity.gravityType.ToString());
                    return;
                }
                bool valChanged = false;
                foreach (Gravity.GravityTypes val in System.Enum.GetValues(typeof(Gravity.GravityTypes))) {
                    if (System.String.Equals(args[0], val.ToString())) {
                        Gravity.gravityType = val;
                        valChanged = true;
                    }
                }
                if (float.TryParse(args[0], out float custom)) {
                    Gravity.gravityType = Gravity.GravityTypes.Custom;
                    Gravity.custom = custom;
                    valChanged = true;
                }
                if (!valChanged)
                    DevConsole.GameConsole.WriteLine("Unknown argument");
            })
            .AutoComplete(args => {
                if (args.Length == 0) return System.Enum.GetNames(typeof(Gravity.GravityTypes));
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_info")
            .Help("md_info [max_level] [selector?]")
            .RunGame((game, args) => {
                if (args.Length < 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 or 2 arguments");
                    return;
                }
                if (!int.TryParse(args[0], out int maxLevel)) {
                    DevConsole.GameConsole.WriteLine("Parse max_level failed");
                    return;
                }
                if (args.Length == 1) {
                    Info.CopyToClipboard(Info.GetInfo(Drag.MouseCamera(game)?.room, maxLevel));
                    DevConsole.GameConsole.WriteLine("Copied room data to clipboard.");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[1]);
                if (!(list?.Count() > 0))
                    return;
                string allInfo = "";
                foreach (var obj in list)
                    allInfo += Info.GetInfo(obj?.realizedObject, maxLevel);
                Info.CopyToClipboard(allInfo);
                DevConsole.GameConsole.WriteLine("Copied data from object" + (list.Count() > 1 ? "s " : " ") + "to clipboard.");
            })
            .AutoComplete(args => {
                if (args.Length == 0) return new string[] { "2", "3", "4" };
                if (args.Length == 1) return DevConsole.Selection.Autocomplete;
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_load_region_rooms")
            .RunGame((game, args) => {
                DevConsole.GameConsole.WriteLine("Activating all rooms in current region. This might take a while.");
                Special.ActivateRegionRooms(game);
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_select")
            .Help("md_select [selector] [bodychunk?]")
            .RunGame((game, args) => {
                if (args.Length < 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 or 2 arguments");
                    return;
                }
                Select.selectedChunks = new List<BodyChunk>(); //clear current selection
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--) {
                    if (list.ElementAt(i)?.realizedObject?.bodyChunks == null)
                        continue;
                    if (args.Length < 2 || args[1] == "all") {
                        foreach (var bc in list.ElementAt(i).realizedObject.bodyChunks)
                            Select.selectedChunks.Add(bc);
                    } else if (args[1] == "main") {
                        var bc = list.ElementAt(i).realizedObject.firstChunk;
                        if (list.ElementAt(i).realizedObject is Creature &&
                            (list.ElementAt(i).realizedObject as Creature).mainBodyChunk != null)
                            bc = (list.ElementAt(i).realizedObject as Creature).mainBodyChunk;
                        Select.selectedChunks.Add(bc);
                    } else if (args[1] == "first") {
                        Select.selectedChunks.Add(list.ElementAt(i).realizedObject.firstChunk);
                    } else if (args[1] == "random") {
                        Select.selectedChunks.Add(list.ElementAt(i).realizedObject.RandomChunk);
                    } else if (int.TryParse(args[1], out int bcI)) {
                        if (bcI >= 0 && bcI < list.ElementAt(i).realizedObject.bodyChunks.Length) {
                            Select.selectedChunks.Add(list.ElementAt(i).realizedObject.bodyChunks[bcI]);
                        } else {
                            DevConsole.GameConsole.WriteLine("BodyChunk with index " + bcI + " does not exist for " + Special.ConsistentName(list.ElementAt(i)));
                        }
                    } else {
                        DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                        break;
                    }
                }
                DevConsole.GameConsole.WriteLine("Selected " + Select.selectedObjects.Count + " PhysicalObjects, " + Select.selectedChunks.Count + " BodyChunks.");
            })
            .AutoComplete(args => {
                if (args.Length == 0) return DevConsole.Selection.Autocomplete;
                if (args.Length == 1) return new string[] { "all", "main", "first", "random" };
                return null;
            })
            .Register();

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DevConsoleRegisterCommands, finished registration of commands");
        }


        //===================================================== Integration Hooks =====================================================
        public static class Hooks
        {
            static IDetour devConsoleSelection_FindBaseAbstractObjects;
            static IDetour devConsoleSelection_Autocomplete;


            public static void Apply()
            {
                if (devConsoleEnabled) {
                    try {
                        ApplyDevConsole();
                    } catch (System.Exception ex) {
                        Plugin.Logger.LogError("Integration.Hooks.Apply exception, unable to apply Dev Console hooks: " + ex?.ToString());
                    }
                }
                Plugin.Logger.LogDebug("Integration.Hooks.Apply, finished applying enabled integration hook(s)");
            }


            //use in try/catch so missing assembly does not crash the game
            public static void ApplyDevConsole()
            {
                //hook returns this mod's selection when md_sel is used as selector
                devConsoleSelection_FindBaseAbstractObjects = new Hook(
                    typeof(DevConsole.Selection).GetMethod("FindBaseAbstractObjects", BindingFlags.Static | BindingFlags.NonPublic),
                    typeof(Integration.Hooks).GetMethod(nameof(DevConsoleSelection_FindBaseAbstractObjects_RuntimeDetour), BindingFlags.Static | BindingFlags.Public)
                );

                //hook adds a new selector to Autocomplete for newly created commands
                //TODO at this point the built-in Dev Console commands are already initialized and won't receive the updated Autocomplete list
                devConsoleSelection_Autocomplete = new Hook(
                    typeof(DevConsole.Selection).GetProperty("Autocomplete", BindingFlags.Static | BindingFlags.Public).GetGetMethod(),
                    typeof(Integration.Hooks).GetMethod(nameof(DevConsoleSelection_Autocomplete_get), BindingFlags.Static | BindingFlags.Public)
                );
            }


            public static void Unapply()
            {
                if (devConsoleSelection_FindBaseAbstractObjects?.IsValid == true && devConsoleSelection_FindBaseAbstractObjects?.IsApplied == true)
                    devConsoleSelection_FindBaseAbstractObjects?.Dispose();
                if (devConsoleSelection_Autocomplete?.IsValid == true && devConsoleSelection_Autocomplete?.IsApplied == true)
                    devConsoleSelection_Autocomplete?.Dispose();
            }


            //hook returns this mod's selection when md_sel is used as selector
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static IEnumerable<AbstractPhysicalObject> DevConsoleSelection_FindBaseAbstractObjects_RuntimeDetour(Func<RainWorldGame, string, IEnumerable<AbstractPhysicalObject>> orig, RainWorldGame game, string arg)
            {
                var ret = orig(game, arg);
                if ((ret == null || ret.Count() <= 0) && arg?.ToLower() == "md_sel")
                    ret = Select.selectedObjects.Select(po => po.abstractPhysicalObject);
                return ret;
            }


            //hook adds a new selector to Autocomplete for newly created commands
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static string[] DevConsoleSelection_Autocomplete_get(Func<string[]> orig)
            {
                var ret = orig();
                if (ret != null)
                    return ret.Concat(new string[] { "md_sel" }).ToArray();
                return ret;
            }
        }
        //=============================================================================================================================
    }
}
