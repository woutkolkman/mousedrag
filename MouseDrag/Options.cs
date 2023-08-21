using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<KeyCode> activateKey;
        public static Configurable<bool> menuRMB, menuFollows;
        public static Configurable<bool> forceMouseVisible, undoMouseVisible, releaseGraspsPaused, lineageKill;
        public static Configurable<bool> copyID, exitGameOverMode, exceptSlugNPC, tameIncreasesRep, throwWithMouse, throwAsPlayer;
        public static Configurable<KeyCode> menuOpen, pauseOneKey, pauseRoomCreaturesKey, unpauseAllKey;
        public static Configurable<KeyCode> deleteOneKey, deleteAllCreaturesKey, deleteAllObjectsKey;
        public static Configurable<KeyCode> killOneKey, killAllCreaturesKey, reviveOneKey, reviveAllCreaturesKey;
        public static Configurable<KeyCode> pauseAllCreaturesKey, pauseAllObjectsKey;
        public static Configurable<KeyCode> tameOneKey, tameAllCreaturesKey, clearRelOneKey, clearRelAllKey;
        public static Configurable<KeyCode> duplicateOneKey;
        public static Configurable<KeyCode> stunOneKey, stunRoomKey, unstunAllKey, stunAllKey;
        public int curTab;

        public enum ActivateTypes
        {
            DevToolsActive,
            KeyBindPressed,
            AlwaysActive
        }


        public Options()
        {
            activateType = config.Bind("activateType", defaultValue: ActivateTypes.AlwaysActive.ToString(), new ConfigurableInfo("Controls are active when this condition is met. Always active in sandbox.", null, "", "Active when"));
            activateKey = config.Bind("activateKey", KeyCode.None, new ConfigurableInfo("KeyBind to activate controls when \"" + ActivateTypes.KeyBindPressed.ToString() + "\" is selected.", null, "", "KeyBind"));

            menuRMB = config.Bind("menuRMB", defaultValue: true, new ConfigurableInfo("Right mouse button opens menu on object or background.", null, "", "RMB opens menu"));
            menuFollows = config.Bind("menuFollows", defaultValue: true, new ConfigurableInfo("If checked, menu follows the target creature/object on which actions are performed.", null, "", "Menu follows target"));
            forceMouseVisible = config.Bind("forceMouseVisible", defaultValue: true, new ConfigurableInfo("Makes Windows mouse pointer always be visible in-game when tools are active.", null, "", "Force mouse visible"));
            undoMouseVisible = config.Bind("undoMouseVisible", defaultValue: false, new ConfigurableInfo("Hides Windows mouse pointer in-game when tools become inactive.", null, "", "Hide mouse after"));
            releaseGraspsPaused = config.Bind("releaseGraspsPaused", defaultValue: true, new ConfigurableInfo("When creature is paused, all grasps (creatures/items) are released.", null, "", "Pausing releases grasps"));
            lineageKill = config.Bind("lineageKill", defaultValue: false, new ConfigurableInfo("When killing creatures using tools, set killTag to first player so creatures can lineage.\nDeleting creatures without killing them does not affect lineage.", null, "", "Lineage when killed"));

            copyID = config.Bind("copyID", defaultValue: true, new ConfigurableInfo("Creates an exact copy of the previous object when duplicating.", null, "", "Copy ID duplicate"));
            exitGameOverMode = config.Bind("exitGameOverMode", defaultValue: true, new ConfigurableInfo("Try to exit game over mode when reviving player. Might be incompatible with some other mods.\nOnly works in story-mode.", null, "", "Exit game over mode"));
            exceptSlugNPC = config.Bind("exceptSlugNPC", defaultValue: true, new ConfigurableInfo("If checked, do not pause/delete/kill slugpups when pausing/deleting/killing all creatures.", null, "", "Except SlugNPC"));
            tameIncreasesRep = config.Bind("tameIncreasesRep", defaultValue: false, new ConfigurableInfo("Taming creatures using this tool also increases global reputation.", null, "", "Taming global +rep"));
            throwWithMouse = config.Bind("throwWithMouse", defaultValue: true, new ConfigurableInfo("Quickly dragging and releasing weapons will throw them in that direction.", null, "", "Throw with mouse"));
            throwAsPlayer = config.Bind("throwAsPlayer", defaultValue: false, new ConfigurableInfo("Throwing weapons with the mouse will use Player as thrower.", null, "", "Throw as Player"));

            menuOpen = config.Bind("menuOpen", KeyCode.None, new ConfigurableInfo("KeyBind opens menu on object or background, as an alternative to right mouse button.", null, "", "Open menu"));
            pauseOneKey = config.Bind("pauseOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause"));
            pauseRoomCreaturesKey = config.Bind("pauseRoomCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause all creatures except Player and SlugNPC, only currently in this room.\nAllows unpausing individual creatures.", null, "", "Pause creatures in room"));
            unpauseAllKey = config.Bind("unpauseAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to unpause all objects/creatures, including individually paused creatures.", null, "", "Unpause all"));
            deleteOneKey = config.Bind("deleteOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to delete the object/creature which you're currently dragging.\nTo make creatures respawn, kill and then delete them.", null, "", "Delete"));
            deleteAllCreaturesKey = config.Bind("deleteAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to delete all creatures in current room except Player and SlugNPC.\nTo make creatures respawn, kill and then delete them.", null, "", "Delete creatures in\nroom"));
            deleteAllObjectsKey = config.Bind("deleteAllObjectsKey", KeyCode.None, new ConfigurableInfo("KeyBind to delete all objects/creatures in current room except Player and SlugNPC.\nTo make creatures respawn, kill and then delete them.", null, "", "Delete objects in room"));
            killOneKey = config.Bind("killOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to kill the creature which you're currently dragging.", null, "", "Kill"));
            killAllCreaturesKey = config.Bind("killAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to kill all creatures in current room except Player and SlugNPC.", null, "", "Kill creatures in room"));
            reviveOneKey = config.Bind("reviveOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal the creature which you're currently dragging.", null, "", "Revive/heal"));
            reviveAllCreaturesKey = config.Bind("reviveAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal all creatures in current room.", null, "", "Revive/heal creatures\nin room"));

            pauseAllCreaturesKey = config.Bind("pauseAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all creatures except Player and SlugNPC, including creatures that still need to spawn.\nIndividually (un)paused creatures remain paused.", null, "", "Pause all creatures"));
            pauseAllObjectsKey = config.Bind("pauseAllObjectsKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all objects except creatures, including objects that still need to spawn.\nIndividually (un)paused objects remain paused.", null, "", "Pause all objects"));
            tameOneKey = config.Bind("tameOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to tame the creature which you're currently dragging.", null, "", "Tame"));
            tameAllCreaturesKey = config.Bind("tameAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to tame all creatures in current room.", null, "", "Tame creatures in\nroom"));
            clearRelOneKey = config.Bind("clearRelOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of the creature which you're currently dragging.", null, "", "Clear relationships"));
            clearRelAllKey = config.Bind("clearRelAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of all creatures in current room except Player and SlugNPC.", null, "", "Clear relationships\nin room"));
            duplicateOneKey = config.Bind("duplicateOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to duplicate the object/creature which you're currently dragging. Hold button to repeat.", null, "", "Duplicate"));
            stunOneKey = config.Bind("stunOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to stun the object/creature which you're currently dragging.", null, "", "Stun"));
            stunRoomKey = config.Bind("stunRoomKey", KeyCode.None, new ConfigurableInfo("KeyBind to stun all objects/creatures except Player and SlugNPC, only currently in this room.\nAllows unstunning individual objects/creatures.", null, "", "Stun in room"));
            unstunAllKey = config.Bind("unstunAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to unstun all objects/creatures, including individually stunned objects/creatures.", null, "", "Unstun all"));
            stunAllKey = config.Bind("stunAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to stun all objects/creatures except Player and SlugNPC, including objects/creatures that still need to spawn.\nIndividually (un)stunned objects/creatures remain stunned.", null, "", "Stun all"));
        }


        public override void Initialize()
        {
            const float startHeight = 520f;

            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General"),
                new OpTab(this, "KeyBinds")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            float x = 90;
            float y = startHeight;
            AddComboBox(activateType, new Vector2(190f, y - 27f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddKeyBinder(activateKey, new Vector2(330f, y - 30f));

            AddCheckbox(menuRMB, new Vector2(x, y -= 100f));
            AddCheckbox(menuFollows, new Vector2(x, y -= 40f));
            AddCheckbox(forceMouseVisible, new Vector2(x, y -= 40f));
            AddCheckbox(undoMouseVisible, new Vector2(x, y -= 40f));
            AddCheckbox(releaseGraspsPaused, new Vector2(x, y -= 40f));
            AddCheckbox(lineageKill, new Vector2(x, y -= 40f));

            x += 250;
            y = startHeight;
            AddCheckbox(copyID, new Vector2(x, y -= 100f));
            AddCheckbox(exitGameOverMode, new Vector2(x, y -= 40f));
            AddCheckbox(exceptSlugNPC, new Vector2(x, y -= 40f));
            AddCheckbox(tameIncreasesRep, new Vector2(x, y -= 40f));
            AddCheckbox(throwWithMouse, new Vector2(x, y -= 40f));
            AddCheckbox(throwAsPlayer, new Vector2(x, y -= 40f));

            /**************** KeyBinds ****************/
            curTab++;
            x = 40;
            y = 600f;
            AddKeyBinder(menuOpen, new Vector2(x, y -= 50f));
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(pauseRoomCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllObjectsKey, new Vector2(x, y -= 50f));
            AddKeyBinder(killOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(killAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(reviveOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(reviveAllCreaturesKey, new Vector2(x, y -= 50f));

            x += 280;
            y = 600f;
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(pauseAllObjectsKey, new Vector2(x, y -= 50f));
            AddKeyBinder(tameOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(tameAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(clearRelOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(clearRelAllKey, new Vector2(x, y -= 50f));
            AddKeyBinder(duplicateOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(stunOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(stunRoomKey, new Vector2(x, y -= 50f));
            AddKeyBinder(unstunAllKey, new Vector2(x, y -= 50f));
            AddKeyBinder(stunAllKey, new Vector2(x, y -= 50f));
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
