using System.Linq;
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


        public static void RefreshActiveMods()
        {
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
            .Help("md_pause_all [position] [types]")
            .RunGame((game, args) => {
                if (args.Length != 2) {
                    DevConsole.GameConsole.WriteLine("Expected 2 arguments");
                    return;
                }
                bool creatures = args[1] == "creatures" || args[1] == "objects";
                bool items = args[1] == "items" || args[1] == "objects";
                if (args[0] == "room") {
                    Pause.PauseObjects(Drag.MouseCamera(game)?.room, creatures, items);
                } else if (args[0] == "any") {
                    if (creatures)
                        Pause.pauseAllCreatures = true;
                    if (items)
                        Pause.pauseAllItems = true;
                } else {
                    DevConsole.GameConsole.WriteLine("Unknown argument(s)");
                }
            })
            .AutoComplete(args => {
                if (args.Length == 0) return new string[] { "room", "any" };
                if (args.Length == 1) return new string[] { "creatures", "items", "objects" };
                return null;
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_unpause_all")
            .Run((args) => {
                Pause.UnpauseAll();
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

            new DevConsole.Commands.CommandBuilder("md_forcefield_toggle")
            .Help("md_forcefield_toggle [selector]")
            .RunGame((game, args) => {
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 argument");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--) {
                    BodyChunk bc = list.ElementAt(i)?.realizedObject?.firstChunk;
                    if (list.ElementAt(i)?.realizedObject is Creature)
                        bc = (list.ElementAt(i).realizedObject as Creature).mainBodyChunk ?? bc;
                    Forcefield.ToggleForcefield(bc);
                }
            })
            .AutoComplete(new string[][]{ DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_forcefield_clear_all")
            .Run((args) => {
                Forcefield.ClearForcefields();
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

            new DevConsole.Commands.CommandBuilder("md_stun_toggle")
            .Help("md_stun_toggle [selector]")
            .RunGame((game, args) => {
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 argument");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--)
                    Stun.ToggleStunObject(list.ElementAt(i)?.realizedObject);
            })
            .AutoComplete(new string[][]{ DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_unstun_all")
            .Run((args) => {
                Stun.UnstunAll();
            })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_lock_toggle")
            .Help("md_lock_toggle [selector]")
            .RunGame((game, args) => {
                if (args.Length != 1) {
                    DevConsole.GameConsole.WriteLine("Expected 1 argument");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[0]);
                for (int i = list.Count() - 1; i >= 0; i--) {
                    BodyChunk bc = list.ElementAt(i)?.realizedObject?.firstChunk;
                    if (list.ElementAt(i)?.realizedObject is Creature)
                        bc = (list.ElementAt(i).realizedObject as Creature).mainBodyChunk ?? bc;
                    Lock.ToggleLock(bc);
                }
            })
            .AutoComplete(new string[][] { DevConsole.Selection.Autocomplete })
            .Register();

            new DevConsole.Commands.CommandBuilder("md_lock_clear_all")
            .Run((args) => {
                Lock.bodyChunks.Clear();
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
                    Info.DumpInfo(Drag.MouseCamera(game)?.room, maxLevel);
                    DevConsole.GameConsole.WriteLine("Copied room data to clipboard.");
                    return;
                }
                var list = DevConsole.Selection.SelectAbstractObjects(game, args[1]);
                if (list.Count() <= 0)
                    return;
                //only copy one object, because clipboard would be overwritten otherwise
                Info.DumpInfo(list.ElementAt(0)?.realizedObject, maxLevel);
                DevConsole.GameConsole.WriteLine("Copied object data of first match to clipboard.");
                //TODO maybe append strings? might become too resource intensive, or too much lag
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

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("DevConsoleRegisterCommands, finished registration of commands");
        }
    }
}
