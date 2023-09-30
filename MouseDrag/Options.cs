using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<KeyCode> activateKey;
        public static Configurable<bool> menuRMB, menuMMB, menuFollows;
        public static Configurable<bool> forceMouseVisible, undoMouseVisible, releaseGraspsPaused, lineageKill;
        public static Configurable<bool> deactivateEveryRestart, logDebug;
        public static Configurable<bool> copyID, exitGameOverMode, exceptSlugNPC, tameIncreasesRep, throwWithMouse, throwAsPlayer;
        public static Configurable<bool> velocityDrag, killReleasesMask;
        public static Configurable<KeyCode> menuOpen, pauseOneKey, pauseRoomCreaturesKey, unpauseAllKey;
        public static Configurable<KeyCode> pauseAllCreaturesKey, pauseAllObjectsKey;
        public static Configurable<KeyCode> killOneKey, killAllCreaturesKey, reviveOneKey, reviveAllCreaturesKey;
        public static Configurable<KeyCode> duplicateOneKey;
        public static Configurable<KeyCode> tameOneKey, tameAllCreaturesKey, clearRelOneKey, clearRelAllKey;
        public static Configurable<KeyCode> stunOneKey, stunRoomKey, unstunAllKey, stunAllKey;
        public static Configurable<KeyCode> destroyOneKey, destroyAllCreaturesKey, destroyAllObjectsKey;
        public static Configurable<bool> pauseOneMenu, pauseRoomCreaturesMenu, unpauseAllMenu;
        public static Configurable<bool> pauseAllCreaturesMenu, pauseAllObjectsMenu;
        public static Configurable<bool> killOneMenu, killAllCreaturesMenu, reviveOneMenu, reviveAllCreaturesMenu;
        public static Configurable<bool> duplicateOneMenu;
        public static Configurable<bool> clipboardMenu, clipboardCtrlXCV;
        public static Configurable<bool> tameOneMenu, tameAllCreaturesMenu, clearRelOneMenu, clearRelAllMenu;
        public static Configurable<bool> stunOneMenu, stunRoomMenu, unstunAllMenu, stunAllMenu;
        public static Configurable<bool> destroyOneMenu, destroyAllCreaturesMenu, destroyAllObjectsMenu;
        public int curTab;

        public enum ActivateTypes
        {
            DevToolsActive,
            KeyBindPressed,
            AlwaysActive,
            SandboxAndSafari
        }


        public Options()
        {
            activateType = config.Bind("activateType", defaultValue: ActivateTypes.AlwaysActive.ToString(), new ConfigurableInfo("Controls are active when this condition is met. Always active in sandbox.", null, "", "Active when"));
            activateKey = config.Bind("activateKey", KeyCode.None, new ConfigurableInfo("KeyBind to activate controls when \"" + ActivateTypes.KeyBindPressed.ToString() + "\" is selected.", null, "", "KeyBind"));

            menuRMB = config.Bind("menuRMB", defaultValue: true, new ConfigurableInfo("Right mouse button opens menu on object or background.", null, "", "RMB opens menu"));
            menuMMB = config.Bind("menuMMB", defaultValue: false, new ConfigurableInfo("Middle mouse button opens menu on object or background.", null, "", "MMB opens menu"));
            menuFollows = config.Bind("menuFollows", defaultValue: true, new ConfigurableInfo("If checked, menu follows the target creature/object on which actions are performed.", null, "", "Menu follows target"));
            forceMouseVisible = config.Bind("forceMouseVisible", defaultValue: true, new ConfigurableInfo("Makes Windows mouse pointer always be visible in-game when tools are active.", null, "", "Force mouse visible"));
            undoMouseVisible = config.Bind("undoMouseVisible", defaultValue: false, new ConfigurableInfo("Hides Windows mouse pointer in-game when tools become inactive.", null, "", "Hide mouse after"));
            releaseGraspsPaused = config.Bind("releaseGraspsPaused", defaultValue: false, new ConfigurableInfo("When creature is paused, all grasps (creatures/items) are released.", null, "", "Pausing releases grasps"));
            lineageKill = config.Bind("lineageKill", defaultValue: false, new ConfigurableInfo("When killing creatures using tools, set killTag to first player so creatures can lineage.\nDestroying creatures without killing them does not result in lineage.", null, "", "Lineage when killed"));
            deactivateEveryRestart = config.Bind("deactivateEveryRestart", defaultValue: true, new ConfigurableInfo("Deactivate tools when cycle ends or game is restarted, just like Dev Tools. (only used when 'Active when' is 'KeyBindPressed')", null, "", "Deactivate every restart"));
            logDebug = config.Bind("logDebug", defaultValue: true, new ConfigurableInfo("Useful for debugging if you share your log files.", null, "", "Log debug"));

            copyID = config.Bind("copyID", defaultValue: true, new ConfigurableInfo("Creates an exact copy of the previous object when duplicating.", null, "", "Copy ID duplicate"));
            exitGameOverMode = config.Bind("exitGameOverMode", defaultValue: true, new ConfigurableInfo("Try to exit game over mode when reviving player. Might be incompatible with some other mods.\nOnly works in story-mode.", null, "", "Exit game over mode"));
            exceptSlugNPC = config.Bind("exceptSlugNPC", defaultValue: true, new ConfigurableInfo("If checked, do not pause/destroy/kill slugpups when pausing/destroying/killing all creatures.", null, "", "Except SlugNPC"));
            tameIncreasesRep = config.Bind("tameIncreasesRep", defaultValue: false, new ConfigurableInfo("Taming creatures using this tool also increases global reputation.", null, "", "Taming global +rep"));
            throwWithMouse = config.Bind("throwWithMouse", defaultValue: true, new ConfigurableInfo("Quickly dragging and releasing weapons will throw them in that direction.", null, "", "Throw with mouse"));
            throwAsPlayer = config.Bind("throwAsPlayer", defaultValue: false, new ConfigurableInfo("Throwing weapons with the mouse will use Player as thrower.", null, "", "Throw as Player"));
            velocityDrag = config.Bind("velocityDrag", defaultValue: false, new ConfigurableInfo("Alternative dragging method using velocity instead of position. Dragged objects/creatures won't (easily) move through walls.\nYou will also always drag the center of a BodyChunk. Sandbox mouse might interfere.", null, "", "Velocity drag"));
            killReleasesMask = config.Bind("killReleasesMask", defaultValue: true, new ConfigurableInfo("Killing elite scavengers or vultures with this tool will release their masks.", null, "", "Kill releases mask"));

            menuOpen = config.Bind("menuOpen", KeyCode.None, new ConfigurableInfo("KeyBind opens menu on object or background, as an alternative to right mouse button.", null, "", "Open menu"));
            pauseOneKey = config.Bind("pauseOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause"));
            pauseRoomCreaturesKey = config.Bind("pauseRoomCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause all creatures except Player and SlugNPC, only currently in this room.\nAllows unpausing individual creatures.", null, "", "Pause creatures\nin room"));
            unpauseAllKey = config.Bind("unpauseAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to unpause all objects/creatures, including individually paused creatures.", null, "", "Unpause all"));
            pauseAllCreaturesKey = config.Bind("pauseAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all creatures except Player and SlugNPC, including creatures that still need to spawn.\nIndividually (un)paused creatures remain paused.", null, "", "Pause all creatures"));
            pauseAllObjectsKey = config.Bind("pauseAllObjectsKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all objects except creatures, including objects that still need to spawn.\nIndividually (un)paused objects remain paused.", null, "", "Pause all objects"));
            killOneKey = config.Bind("killOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to kill the creature which you're currently dragging. Can also trigger objects like bombs.", null, "", "Kill"));
            killAllCreaturesKey = config.Bind("killAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to kill all creatures in current room except Player and SlugNPC.", null, "", "Kill creatures\nin room"));
            reviveOneKey = config.Bind("reviveOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal the creature which you're currently dragging. Can also reset objects like popcorn plants.", null, "", "Revive/heal"));
            reviveAllCreaturesKey = config.Bind("reviveAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal all creatures in current room.", null, "", "Revive/heal\ncreatures\nin room"));
            duplicateOneKey = config.Bind("duplicateOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to duplicate the object/creature which you're currently dragging. Hold button to repeat.", null, "", "Duplicate"));

            tameOneKey = config.Bind("tameOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to tame the creature which you're currently dragging.", null, "", "Tame"));
            tameAllCreaturesKey = config.Bind("tameAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to tame all creatures in current room.", null, "", "Tame creatures in\nroom"));
            clearRelOneKey = config.Bind("clearRelOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of the creature which you're currently dragging.", null, "", "Clear relationships"));
            clearRelAllKey = config.Bind("clearRelAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of all creatures in current room except Player and SlugNPC.", null, "", "Clear relationships\nin room"));
            stunOneKey = config.Bind("stunOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to stun/unstun the object/creature which you're currently dragging.", null, "", "Stun"));
            stunRoomKey = config.Bind("stunRoomKey", KeyCode.None, new ConfigurableInfo("KeyBind to stun all objects/creatures except Player and SlugNPC, only currently in this room.\nAllows unstunning individual objects/creatures.", null, "", "Stun in room"));
            unstunAllKey = config.Bind("unstunAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to unstun all objects/creatures, including individually stunned objects/creatures.", null, "", "Unstun all"));
            stunAllKey = config.Bind("stunAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to stun/unstun all objects/creatures except Player and SlugNPC, including objects/creatures that still need to spawn.\nIndividually (un)stunned objects/creatures remain stunned.", null, "", "Stun all"));
            destroyOneKey = config.Bind("destroyOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to destroy the object/creature which you're currently dragging.\nTo make creatures respawn, kill and then destroy them.", null, "", "Destroy"));
            destroyAllCreaturesKey = config.Bind("destroyAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to destroy all creatures in current room except Player and SlugNPC.\nTo make creatures respawn, kill and then destroy them.", null, "", "Destroy creatures\nin room"));
            destroyAllObjectsKey = config.Bind("destroyAllObjectsKey", KeyCode.None, new ConfigurableInfo("KeyBind to destroy all objects/creatures in current room except Player and SlugNPC.\nTo make creatures respawn, kill and then destroy them.", null, "", "Destroy objects\nin room"));

            pauseOneMenu = config.Bind("pauseOneMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseRoomCreaturesMenu = config.Bind("pauseRoomCreaturesMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            unpauseAllMenu = config.Bind("unpauseAllMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseAllCreaturesMenu = config.Bind("pauseAllCreaturesMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseAllObjectsMenu = config.Bind("pauseAllObjectsMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            killOneMenu = config.Bind("killOneMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            killAllCreaturesMenu = config.Bind("killAllCreaturesMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            reviveOneMenu = config.Bind("reviveOneMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            reviveAllCreaturesMenu = config.Bind("reviveAllCreaturesMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            duplicateOneMenu = config.Bind("duplicateOneMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clipboardMenu = config.Bind("clipboardMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.\nCut/paste PhysicalObjects with a clipboard (LIFO buffer). Clipboard is lost when game is closed.", null, "", ""));
            clipboardCtrlXCV = config.Bind("clipboardCtrlXCV", defaultValue: false, new ConfigurableInfo("Using Control + X/C/V will cut, copy or paste the object/creature which you're currently dragging.", null, "", "Ctrl + X/C/V"));

            tameOneMenu = config.Bind("tameOneMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            tameAllCreaturesMenu = config.Bind("tameAllCreaturesMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clearRelOneMenu = config.Bind("clearRelOneMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clearRelAllMenu = config.Bind("clearRelAllMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunOneMenu = config.Bind("stunOneMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunRoomMenu = config.Bind("stunRoomMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            unstunAllMenu = config.Bind("unstunAllMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunAllMenu = config.Bind("stunAllMenu", defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyOneMenu = config.Bind("destroyOneMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyAllCreaturesMenu = config.Bind("destroyAllCreaturesMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyAllObjectsMenu = config.Bind("destroyAllObjectsMenu", defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
        }


        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General"),
                new OpTab(this, "KeyBinds")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            float x = 90f;
            float y = 460f;
            float sepr = 40f;
            AddComboBox(activateType, new Vector2(190f, 493f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddKeyBinder(activateKey, new Vector2(330f, 490f));

            AddCheckbox(menuRMB, new Vector2(x, y -= sepr));
            AddCheckbox(menuMMB, new Vector2(x, y -= sepr));
            AddCheckbox(menuFollows, new Vector2(x, y -= sepr));
            AddCheckbox(forceMouseVisible, new Vector2(x, y -= sepr));
            AddCheckbox(undoMouseVisible, new Vector2(x, y -= sepr));
            AddCheckbox(releaseGraspsPaused, new Vector2(x, y -= sepr));
            AddCheckbox(lineageKill, new Vector2(x, y -= sepr));
            AddCheckbox(deactivateEveryRestart, new Vector2(x, y -= sepr));
            AddCheckbox(logDebug, new Vector2(x, y -= sepr));

            x += 250;
            y = 460f;
            AddCheckbox(copyID, new Vector2(x, y -= sepr));
            AddCheckbox(exitGameOverMode, new Vector2(x, y -= sepr));
            AddCheckbox(exceptSlugNPC, new Vector2(x, y -= sepr));
            AddCheckbox(tameIncreasesRep, new Vector2(x, y -= sepr));
            AddCheckbox(throwWithMouse, new Vector2(x, y -= sepr));
            AddCheckbox(throwAsPlayer, new Vector2(x, y -= sepr));
            AddCheckbox(velocityDrag, new Vector2(x, y -= sepr));
            AddCheckbox(killReleasesMask, new Vector2(x, y -= sepr));

            /**************** KeyBinds ****************/
            curTab++;
            x = 70f;
            y = 610f;
            sepr = 50f;
            AddKeyBinder(menuOpen, new Vector2(x, y -= sepr));
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPause");
            AddCheckbox(pauseOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseRoomCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseCreatures");
            AddCheckbox(pauseRoomCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPlayAll");
            AddCheckbox(unpauseAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseGlobal");
            AddCheckbox(pauseAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseAllObjectsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseGlobal");
            AddCheckbox(pauseAllObjectsMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(killOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragKill");
            AddCheckbox(killOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(killAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragKillCreatures");
            AddCheckbox(killAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(reviveOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragRevive");
            AddCheckbox(reviveOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(reviveAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragReviveCreatures");
            AddCheckbox(reviveAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(duplicateOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDuplicate");
            AddCheckbox(duplicateOneMenu, new Vector2(x - 56f, y + 3f));
            AddCheckbox(clipboardMenu, new Vector2(x - 56f, (y -= sepr) + 3f));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCut");
            AddIcon(new Vector2(x, y + 6f), "mousedragPaste");
            AddCheckbox(clipboardCtrlXCV, new Vector2(x + 25f + 51f, y + 3f));

            x += 300f;
            y = 610f;
            AddKeyBinder(tameOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragHeart");
            AddCheckbox(tameOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tameAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragHeartCreatures");
            AddCheckbox(tameAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(clearRelOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnheart");
            AddCheckbox(clearRelOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(clearRelAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnheartCreatures");
            AddCheckbox(clearRelAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStun");
            AddCheckbox(stunOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStunAll");
            AddCheckbox(stunRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(unstunAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnstunAll");
            AddCheckbox(unstunAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStunGlobal");
            AddCheckbox(stunAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroy");
            AddCheckbox(destroyOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyCreatures");
            AddCheckbox(destroyAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyAllObjectsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyAll");
            AddCheckbox(destroyAllObjectsMenu, new Vector2(x - 56f, y + 3f));
        }


        private void AddTitle()
        {
            OpLabel title = new OpLabel(new Vector2(150f, 560f), new Vector2(300f, 30f), Plugin.Name, bigText: true);
            OpLabel version = new OpLabel(new Vector2(150f, 540f), new Vector2(300f, 30f), $"Version {Plugin.Version}");

            Tabs[curTab].AddItems(new UIelement[]
            {
                title,
                version
            });
        }


        private void AddIcon(Vector2 pos, string iconName)
        {
            Tabs[curTab].AddItems(new UIelement[]
            {
                new OpImage(pos, iconName)
            });
        }


        private void AddCheckbox(Configurable<bool> option, Vector2 pos, Color? c = null)
        {
            if (c == null)
                c = Menu.MenuColorEffect.rgbMediumGrey;

            OpCheckBox checkbox = new OpCheckBox(option, pos)
            {
                description = option.info.description,
                colorEdge = (Color)c
            };

            OpLabel label = new OpLabel(pos.x + 40f, pos.y + 2f, option.info.Tags[0] as string)
            {
                description = option.info.description,
                color = (Color)c
            };

            Tabs[curTab].AddItems(new UIelement[]
            {
                checkbox,
                label
            });
        }


        private void AddKeyBinder(Configurable<KeyCode> option, Vector2 pos, Color? c = null)
        {
            if (c == null)
                c = Menu.MenuColorEffect.rgbMediumGrey;

            OpKeyBinder keyBinder = new OpKeyBinder(option, pos, new Vector2(100f, 30f), false)
            {
                description = option.info.description,
                colorEdge = (Color)c
            };

            OpLabel label = new OpLabel(pos.x + 100f + 16f, pos.y + 5f, option.info.Tags[0] as string)
            {
                description = option.info.description,
                color = (Color)c
            };

            Tabs[curTab].AddItems(new UIelement[]
            {
                keyBinder,
                label
            });
        }


        private void AddComboBox(Configurable<string> option, Vector2 pos, string[] array, float width = 80f, FLabelAlignment alH = FLabelAlignment.Center, OpLabel.LabelVAlignment alV = OpLabel.LabelVAlignment.Center)
        {
            OpComboBox box = new OpComboBox(option, pos, width, array)
            {
                description = option.info.description
            };

            Vector2 offset = new Vector2();
            if (alV == OpLabel.LabelVAlignment.Top) {
                offset.y += box.size.y + 5f;
            } else if (alV == OpLabel.LabelVAlignment.Bottom) {
                offset.y += -box.size.y - 5f;
            } else if (alH == FLabelAlignment.Right) {
                offset.x += box.size.x + 20f;
                alH = FLabelAlignment.Left;
            } else if (alH == FLabelAlignment.Left) {
                offset.x += -box.size.x - 20f;
                alH = FLabelAlignment.Right;
            }

            OpLabel label = new OpLabel(pos + offset, box.size, option.info.Tags[0] as string)
            {
                description = option.info.description
            };
            label.alignment = alH;
            label.verticalAlignment = OpLabel.LabelVAlignment.Center;

            Tabs[curTab].AddItems(new UIelement[]
            {
                box,
                label
            });
        }
    }
}
