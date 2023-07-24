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
        public static Configurable<bool> forceMouseVisible;

        public enum ActivateTypes
        {
            DevToolsActive,
            KeyBindPressed,
            AlwaysOn
        }


        public Options()
        {
            activateType = config.Bind("activateType", defaultValue: ActivateTypes.DevToolsActive.ToString(), new ConfigurableInfo("Controls are active when this condition is met. Always active in sandbox.", null, "", "Active when"));
            activateKey = config.Bind("activateKey", KeyCode.None, new ConfigurableInfo("Keybind to activate controls when \"KeyBind\" is selected.", null, "", "Keybind"));
            pauseOneKey = config.Bind("pauseOneKey", KeyCode.None, new ConfigurableInfo("Keybind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause/unpause"));
            pauseAllCreaturesKey = config.Bind("pauseAllCreaturesKey", KeyCode.None, new ConfigurableInfo("Keybind to pause/unpause all creatures except Player and SlugNPC.\nIndividually paused creatures remain paused.", null, "", "Pause all creatures"));
            unpauseAllKey = config.Bind("unpauseAllKey", KeyCode.None, new ConfigurableInfo("Keybind to unpause all objects/creatures, including individually paused creatures.", null, "", "Unpause all"));
            deleteOneKey = config.Bind("deleteOneKey", KeyCode.None, new ConfigurableInfo("Keybind to delete the object/creature which you're currently dragging.", null, "", "Delete"));
            deleteAllCreaturesKey = config.Bind("deleteAllCreaturesKey", KeyCode.None, new ConfigurableInfo("Keybind to delete all creatures in current room except Player and SlugNPC.", null, "", "Delete all creatures"));
            deleteAllObjectsKey = config.Bind("deleteAllObjectsKey", KeyCode.None, new ConfigurableInfo("Keybind to delete all objects/creatures in current room except Player and SlugNPC.", null, "", "Delete all"));
            forceMouseVisible = config.Bind("forceMouseVisible", defaultValue: true, new ConfigurableInfo("Makes Windows mouse pointer always be visible in-game when tools are active.", null, "", "Force mouse visible"));
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

            float x = 192;
            float y = startHeight;
            AddComboBox(activateType, new Vector2(190, y - 27f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddKeyBinder(activateKey, new Vector2(x + 140f, y -= 30f));
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= 100f));
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteOneKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeyBinder(deleteAllObjectsKey, new Vector2(x, y -= 50f));
            AddCheckbox(forceMouseVisible, new Vector2(x + 38f, y -= 40f));
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

            OpKeyBinder keybinder = new OpKeyBinder(option, pos, new Vector2(100f, 30f), false)
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
                keybinder,
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
