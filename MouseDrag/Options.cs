using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<KeyCode> activateKey;
        public static Configurable<KeyCode> pauseOneKey, pauseAllCreaturesKey, unpauseAllKey;
        public static Configurable<KeyCode> deleteOneKey, deleteAllCreaturesKey, deleteAllObjectsKey;
        public static Configurable<bool> forceMouseVisible, releaseGraspsPaused, updateLastPos;
        public static Configurable<KeyCode> killOneKey, reviveOneKey, duplicateOneKey;

        public enum ActivateTypes
        {
            DevToolsActive,
            KeyBindPressed,
            AlwaysActive
        }


        public Options()
        {
            activateType = config.Bind("activateType", defaultValue: ActivateTypes.AlwaysActive.ToString(), new ConfigurableInfo("Controls are active when this condition is met. Always active in sandbox.", null, "", "Active when"));
            activateKey = config.Bind("activateKey", KeyCode.None, new ConfigurableInfo("Key bind to activate controls when \"" + ActivateTypes.KeyBindPressed.ToString() + "\" is selected.", null, "", "Key bind"));
            pauseOneKey = config.Bind("pauseOneKey", KeyCode.None, new ConfigurableInfo("Key bind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause/unpause"));
            pauseAllCreaturesKey = config.Bind("pauseAllCreaturesKey", KeyCode.None, new ConfigurableInfo("Key bind to pause/unpause all creatures except Player and SlugNPC.\nIndividually paused creatures remain paused.", null, "", "Pause all creatures"));
            unpauseAllKey = config.Bind("unpauseAllKey", KeyCode.None, new ConfigurableInfo("Key bind to unpause all objects/creatures, including individually paused creatures.", null, "", "Unpause all"));
            deleteOneKey = config.Bind("deleteOneKey", KeyCode.None, new ConfigurableInfo("Key bind to delete the object/creature which you're currently dragging.", null, "", "Delete"));
            deleteAllCreaturesKey = config.Bind("deleteAllCreaturesKey", KeyCode.None, new ConfigurableInfo("Key bind to delete all creatures in current room except Player and SlugNPC.", null, "", "Delete all creatures"));
            deleteAllObjectsKey = config.Bind("deleteAllObjectsKey", KeyCode.None, new ConfigurableInfo("Key bind to delete all objects/creatures in current room except Player and SlugNPC.", null, "", "Delete all"));
            forceMouseVisible = config.Bind("forceMouseVisible", defaultValue: true, new ConfigurableInfo("Makes Windows mouse pointer always be visible in-game when tools are active.", null, "", "Force mouse visible"));
            releaseGraspsPaused = config.Bind("releaseGraspsPaused", defaultValue: true, new ConfigurableInfo("When creature is paused, all grasps (creatures/items) are released.", null, "", "Pausing releases grasps"));
            updateLastPos = config.Bind("updateLastPos", defaultValue: true, new ConfigurableInfo("Reduces visual bugs when object is paused, but slightly affects drag behavior.", null, "", "Update BodyChunk.lastPos"));
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
                new OpTab(this, "Options")
            };
            AddTitle();

            float x = 65;
            float y = startHeight;
            AddComboBox(activateType, new Vector2(190f, y - 27f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddKeyBinder(activateKey, new Vector2(330f, y -= 30f));
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= 100f));
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllObjectsKey, new Vector2(x, y -= 50f));
            AddCheckbox(forceMouseVisible, new Vector2(x + 38f, y -= 40f));
            AddCheckbox(releaseGraspsPaused, new Vector2(x + 38f, y -= 40f));
            AddCheckbox(updateLastPos, new Vector2(x + 38f, y -= 40f));

            x += 300;
            y = startHeight - 30f;
            AddKeyBinder(killOneKey, new Vector2(x, y -= 100f));
            AddKeyBinder(reviveOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(duplicateOneKey, new Vector2(x, y -= 50f));
        }


        private void AddTitle()
        {
            OpLabel title = new OpLabel(new Vector2(150f, 560f), new Vector2(300f, 30f), Plugin.Name, bigText: true);
            OpLabel version = new OpLabel(new Vector2(150f, 540f), new Vector2(300f, 30f), $"Version {Plugin.Version}");

            Tabs[0].AddItems(new UIelement[]
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

            Tabs[0].AddItems(new UIelement[]
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

            Tabs[0].AddItems(new UIelement[]
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

            Tabs[0].AddItems(new UIelement[]
            {
                box,
                label
            });
        }
    }
}
