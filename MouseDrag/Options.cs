using Menu.Remix.MixedUI;
using UnityEngine;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<KeyCode> deleteOneKey, pauseOneKey, pauseAllCreaturesKey, unpauseAllKey;
        public static Configurable<bool> forceMouseVisible;


        public Options()
        {
            deleteOneKey = config.Bind("deleteOneKey", KeyCode.None, new ConfigurableInfo("Keybind to delete the object/creature which you're currently dragging.", null, "", "Delete"));
            pauseOneKey = config.Bind("pauseOneKey", KeyCode.None, new ConfigurableInfo("Keybind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause/unpause"));
            pauseAllCreaturesKey = config.Bind("pauseAllCreaturesKey", KeyCode.None, new ConfigurableInfo("Keybind to pause all creatures except Player and SlugNPC.", null, "", "Pause all creatures"));
            unpauseAllKey = config.Bind("unpauseAllKey", KeyCode.O, new ConfigurableInfo("Keybind to unpause all objects/creatures.", null, "", "Unpause all"));
            forceMouseVisible = config.Bind("forceMouseVisible", defaultValue: true, new ConfigurableInfo("Makes Windows mouse pointer visible in-game when Dev Tools is active.", null, "", "Force mouse visible"));
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
            AddKeybinder(deleteOneKey, new Vector2(x, y -= 40f));
            AddKeybinder(pauseOneKey, new Vector2(x, y -= 50f));
            AddKeybinder(pauseAllCreaturesKey, new Vector2(x, y -= 50f));
            AddKeybinder(unpauseAllKey, new Vector2(x, y -= 50f));
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
                colorEdge = (Color) c
            };

            OpLabel label = new OpLabel(pos.x + 40f, pos.y + 2f, option.info.Tags[0] as string)
            {
                description = option.info.description,
                color = (Color) c
            };

            Tabs[0].AddItems(new UIelement[]
            {
                checkbox,
                label
            });
        }


        private void AddKeybinder(Configurable<KeyCode> option, Vector2 pos, Color? c = null)
        {
            if (c == null)
                c = Menu.MenuColorEffect.rgbMediumGrey;

            OpKeyBinder keybinder = new OpKeyBinder(option, pos, new Vector2(100f, 30f), false)
            {
                description = option.info.description,
                colorEdge = (Color) c
            };

            OpLabel label = new OpLabel(pos.x + 100f + 16f, pos.y + 5f, option.info.Tags[0] as string)
            {
                description = option.info.description,
                color = (Color) c
            };

            Tabs[0].AddItems(new UIelement[]
            {
                keybinder,
                label
            });
        }
    }
}
