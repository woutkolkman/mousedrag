﻿using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<KeyCode> activateKey;
        public static Configurable<bool> forceMouseVisible, undoMouseVisible, releaseGraspsPaused;
        public static Configurable<bool> updateLastPos, copyID, exitGameOverMode, exceptSlugNPC;
        public static Configurable<KeyCode> pauseOneKey, pauseRoomCreaturesKey, pauseAllCreaturesKey, unpauseAllKey;
        public static Configurable<KeyCode> deleteOneKey, deleteAllCreaturesKey, deleteAllObjectsKey;
        public static Configurable<KeyCode> killOneKey, reviveOneKey, duplicateOneKey;
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
            forceMouseVisible = config.Bind("forceMouseVisible", defaultValue: true, new ConfigurableInfo("Makes Windows mouse pointer always be visible in-game when tools are active.", null, "", "Force mouse visible"));
            undoMouseVisible = config.Bind("undoMouseVisible", defaultValue: false, new ConfigurableInfo("Hides Windows mouse pointer in-game when tools become inactive.", null, "", "Hide mouse after"));
            releaseGraspsPaused = config.Bind("releaseGraspsPaused", defaultValue: true, new ConfigurableInfo("When creature is paused, all grasps (creatures/items) are released.", null, "", "Pausing releases grasps"));
            updateLastPos = config.Bind("updateLastPos", defaultValue: true, new ConfigurableInfo("Reduces visual bugs when object is paused, but slightly affects drag behavior.", null, "", "Update BodyChunk.lastPos"));
            copyID = config.Bind("copyID", defaultValue: true, new ConfigurableInfo("Creates an exact duplicate of the previous object.", null, "", "Copy ID duplicate"));
            exitGameOverMode = config.Bind("exitGameOverMode", defaultValue: true, new ConfigurableInfo("Try to exit game over mode when reviving player. Might be incompatible with some other mods.\nOnly works in story-mode.", null, "", "Exit game over mode"));
            exceptSlugNPC = config.Bind("exceptSlugNPC", defaultValue: true, new ConfigurableInfo("If checked, do not pause/delete slugpups when pausing/deleting all creatures.", null, "", "Except SlugNPC"));
            pauseOneKey = config.Bind("pauseOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause"));
            pauseRoomCreaturesKey = config.Bind("pauseRoomCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause all creatures except Player and SlugNPC, only currently in this room.", null, "", "Pause creatures in room"));
            pauseAllCreaturesKey = config.Bind("pauseAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all creatures except Player and SlugNPC, including creatures that still need to spawn.\nIndividually paused creatures remain paused.", null, "", "Pause all creatures"));
            unpauseAllKey = config.Bind("unpauseAllKey", KeyCode.None, new ConfigurableInfo("KeyBind to unpause all objects/creatures, including individually paused creatures.", null, "", "Unpause all"));
            deleteOneKey = config.Bind("deleteOneKey", KeyCode.None, new ConfigurableInfo("KeyBind to delete the object/creature which you're currently dragging.", null, "", "Delete"));
            deleteAllCreaturesKey = config.Bind("deleteAllCreaturesKey", KeyCode.None, new ConfigurableInfo("KeyBind to delete all creatures in current room except Player and SlugNPC.", null, "", "Delete all creatures"));
            deleteAllObjectsKey = config.Bind("deleteAllObjectsKey", KeyCode.None, new ConfigurableInfo("KeyBind to delete all objects/creatures in current room except Player and SlugNPC.", null, "", "Delete all"));
            killOneKey = config.Bind("killOneKey", KeyCode.None, new ConfigurableInfo("Kill the creature which you're currently dragging.", null, "", "Kill"));
            reviveOneKey = config.Bind("reviveOneKey", KeyCode.None, new ConfigurableInfo("Revive and heal the creature which you're currently dragging.", null, "", "Revive/heal"));
            duplicateOneKey = config.Bind("duplicateOneKey", KeyCode.None, new ConfigurableInfo("Duplicate the object/creature which you're currently dragging.", null, "", "Duplicate"));
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
            AddCheckbox(forceMouseVisible, new Vector2(x, y -= 100f));
            AddCheckbox(undoMouseVisible, new Vector2(x, y -= 40f));
            AddCheckbox(releaseGraspsPaused, new Vector2(x, y -= 40f));

            x += 250;
            y = startHeight;
            AddCheckbox(updateLastPos, new Vector2(x, y -= 100f));
            AddCheckbox(copyID, new Vector2(x, y -= 40f));
            AddCheckbox(exitGameOverMode, new Vector2(x, y -= 40f));
            AddCheckbox(exceptSlugNPC, new Vector2(x, y -= 40f));

            /**************** KeyBinds ****************/
            curTab++;
            AddTitle();
            x = 65;
            y = startHeight;
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(pauseRoomCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllObjectsKey, new Vector2(x, y -= 50f));

            x += 300;
            y = startHeight;
            AddKeyBinder(killOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(reviveOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(duplicateOneKey, new Vector2(x, y -= 50f));
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
