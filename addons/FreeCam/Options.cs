using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace FreeCam
{
    public class Options : OptionInterface
    {
        public static Configurable<KeyCode> toggleKey, holdKey;
        public static Configurable<bool> selectLMB, selectMMB;
        public static Configurable<KeyCode> select;
        public static Configurable<bool> cursorMovesScreen;
        public static Configurable<KeyCode> up, right, down, left;
        public static Configurable<string> winCursorVisType;
        public static Configurable<bool> logDebug;
        public static Configurable<bool> splitScreenCoopIntegration, sBCameraScrollIntegration, mouseDragIntegration, devConsoleIntegration;

        public int curTab;

        public enum CursorVisibilityTypes
        {
            NoChanges,
            ForceVisible,
            ForceInvisible,
            Moved2Seconds
        }


        public Options()
        {
            toggleKey = config.Bind(nameof(toggleKey), KeyCode.Home, new ConfigurableInfo("KeyBind to toggle FreeCam. Works on all cameras.", null, "", "Toggle KeyBind"));
            holdKey = config.Bind(nameof(holdKey), KeyCode.None, new ConfigurableInfo("Enable FreeCam while KeyBind is pressed. Works only on first camera.", null, "", "Hold KeyBind"));
            selectLMB = config.Bind(nameof(selectLMB), defaultValue: true, new ConfigurableInfo("Left mouse button is used to select pipes.", null, "", "LMB selects"));
            selectMMB = config.Bind(nameof(selectMMB), defaultValue: false, new ConfigurableInfo("Middle mouse (scroll) button is used to select pipes.", null, "", "MMB selects"));
            select = config.Bind(nameof(select), KeyCode.None, new ConfigurableInfo("KeyBind is used to select pipes, as an alternative to left mouse button.", null, "", "Select"));
            cursorMovesScreen = config.Bind(nameof(cursorMovesScreen), defaultValue: true, new ConfigurableInfo("Moving the cursor to an edge of your screen will move the camera in that direction.", null, "", "Cursor moves screen"));
            up = config.Bind(nameof(up), KeyCode.UpArrow, new ConfigurableInfo("KeyBind to move screen up.", null, "", "Up"));
            right = config.Bind(nameof(right), KeyCode.RightArrow, new ConfigurableInfo("KeyBind to move screen right.", null, "", "Right"));
            down = config.Bind(nameof(down), KeyCode.DownArrow, new ConfigurableInfo("KeyBind to move screen down.", null, "", "Down"));
            left = config.Bind(nameof(left), KeyCode.LeftArrow, new ConfigurableInfo("KeyBind to move screen left.", null, "", "Left"));
            winCursorVisType = config.Bind(nameof(winCursorVisType), defaultValue: CursorVisibilityTypes.NoChanges.ToString(), new ConfigurableInfo("Change visibility of Windows cursor in-game. Set to \"" + CursorVisibilityTypes.NoChanges.ToString() + "\" to allow other mods to manage cursor visibility.", null, "", "Windows\ncursor"));
            logDebug = config.Bind(nameof(logDebug), defaultValue: true, new ConfigurableInfo("Useful for debugging if you share your log files.", null, "", "Log debug"));
            splitScreenCoopIntegration = config.Bind(nameof(splitScreenCoopIntegration), defaultValue: true, new ConfigurableInfo("If SplitScreen Co-op is enabled, multiple cameras are supported.", null, "", "SplitScreen Co-op integration"));
            sBCameraScrollIntegration = config.Bind(nameof(sBCameraScrollIntegration), defaultValue: true, new ConfigurableInfo("If SBCameraScroll is enabled, alternative camera zoom and camera scrolling are supported. Requires restart.", null, "", "SBCameraScroll integration"));
            mouseDragIntegration = config.Bind(nameof(mouseDragIntegration), defaultValue: true, new ConfigurableInfo("If Mouse Drag is enabled, a new slot is added to the radial menu. Requires restart.", null, "", "Mouse Drag integration"));
            devConsoleIntegration = config.Bind(nameof(devConsoleIntegration), defaultValue: true, new ConfigurableInfo("If Dev Console is enabled, additional commands are available via the console. Requires restart.\nAll commands added by " + Plugin.Name + " start with \"fc_\".", null, "", "Dev Console integration"));

            //refresh activated mods when config changes
            var onConfigChanged = typeof(OptionInterface).GetEvent("OnConfigChanged");
            onConfigChanged.AddEventHandler(this, Delegate.CreateDelegate(onConfigChanged.EventHandlerType, typeof(Integration).GetMethod("RefreshActiveMods")));
        }


        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            float x = 90f;
            float y = 540f;
            float sepr = 40f;
            AddKeyBinder(toggleKey, new Vector2(x, y -= sepr + 5f));
            AddKeyBinder(holdKey, new Vector2(x, y -= sepr + 5f));
            AddCheckbox(selectLMB, new Vector2(x, y -= sepr));
            AddCheckbox(selectMMB, new Vector2(x, y -= sepr));
            AddKeyBinder(select, new Vector2(x, y -= sepr + 5f));
            AddCheckbox(cursorMovesScreen, new Vector2(x, y -= sepr));
            AddKeyBinder(up, new Vector2(x, y -= sepr + 5f));
            AddKeyBinder(right, new Vector2(x, y -= sepr + 5f));
            AddKeyBinder(down, new Vector2(x, y -= sepr + 5f));
            AddKeyBinder(left, new Vector2(x, y -= sepr + 5f));
            AddComboBox(winCursorVisType, new Vector2(x, y -= sepr), Enum.GetNames(typeof(CursorVisibilityTypes)), alH: FLabelAlignment.Right, width: 120f);

            x += 250f;
            y = 540f;
            AddCheckbox(logDebug, new Vector2(x, y -= sepr));
            AddCheckbox(splitScreenCoopIntegration, new Vector2(x, y -= sepr));
            AddCheckbox(sBCameraScrollIntegration, new Vector2(x, y -= sepr));
            AddCheckbox(mouseDragIntegration, new Vector2(x, y -= sepr));
            AddCheckbox(devConsoleIntegration, new Vector2(x, y -= sepr));
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


        private void AddTextBox<T>(Configurable<T> option, Vector2 pos, float width = 150f)
        {
            OpTextBox component = new OpTextBox(option, pos, width)
            {
                allowSpace = true,
                description = option.info.description
            };

            OpLabel label = new OpLabel(pos.x + width + 18f, pos.y + 2f, option.info.Tags[0] as string)
            {
                description = option.info.description
            };

            Tabs[curTab].AddItems(new UIelement[]
            {
                component,
                label
            });
        }
    }
}
