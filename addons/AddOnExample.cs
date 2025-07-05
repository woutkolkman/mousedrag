using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MouseDragHelper
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        //metadata
        public const string GUID = "yourname.mousedraghelper"; //change global unique identifier
        public const string Name = "Mouse Drag Helper"; //change name of mod
        public const string Version = "0.1.0"; //also edit version and name in "modinfo.json"

        public static new ManualLogSource Logger { get; private set; } = null;
        private static bool isEnabled = false;


        //called when mod is loaded, subscribe functions to methods of the game
        public void OnEnable()
        {
            if (isEnabled) return;
            isEnabled = true;

            Logger = base.Logger;

            try {
                RegisterSlots();
            } catch (System.Exception ex) {
                Plugin.Logger.LogWarning("MouseDrag assembly not loaded, or another issue: " + ex?.ToString());
            }

            //also hook RainWorld.OnModsInit to load your custom sprite, for example use Futile.atlasManager.LoadImage
            //mousedrag sprites are 18x18 pixels with a 1px transparent border (so 16x16 icon)

            Plugin.Logger.LogInfo("OnEnable called");
        }


        //called when mod is unloaded
        public void OnDisable()
        {
            if (!isEnabled) return;
            isEnabled = false;
            Plugin.Logger.LogInfo("OnDisable called");
        }


        //only run this function if you know that the other mod is loaded
        //or use in try/catch so missing assembly does not crash the game
        public static void RegisterSlots()
        {
            //example 1: sprite
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                requiresBodyChunk = null, //replace with 'true' to make this slot visible when a BodyChunk is selected, 'false' only on background, 'null' both
                name = "CentipedeSegment", //name of example sprite already loaded via the Rain World sprite sheet
                actionPO = (game, slot, po) => { //this action will run for every PhysicalObject in the current radialmenu selection
                    //you can easily view log messages by going to C:\Program Files (x86)\Steam\SteamApps\common\Rain World\BepInEx\config\BepInEx.cfg
                    //and changing [Logging.Console] Enabled = true
                    Plugin.Logger.LogInfo("Example 1");

                    //po is the PhysicalObject selected with the menu
                    //if po is null, the menu is on the background instead of an object
                    PhysicalObject obj = po;
                }
            });

            //example 2: label and reload action
            int exclamationMarks = 0; //static/local variable defined outside the delegates
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                isLabel = true, //interpret name as label instead of sprite
                reload = (game, slot, chunk) => { //this action will allow you to swap sprites or texts
                    slot.name = "Hello World"; //example dynamic label text
                    for (int i = 0; i < exclamationMarks; i++)
                        slot.name += "!";
                    slot.tooltip = "Very helpful text"; //MenuManager will automatically attempt translation if a translation for this tooltip text is loaded
                },
                actionBC = (game, slot, chunk) => { //this action will run for every BodyChunk in the current radialmenu selection
                    Plugin.Logger.LogInfo("Example 2");
                    exclamationMarks++; //increase count of exclamation marks when pressed
                    if (exclamationMarks > 9)
                        exclamationMarks = 0;

                    //you can reference chunk?.owner to get the object on which actions are performed
                    //if chunk is null, the menu is on the background instead of an object
                    PhysicalObject obj = chunk?.owner;
                }
            });

            //example 3: submenus
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                requiresBodyChunk = false, //on background
                name = "To Submenu",
                isLabel = true,
                tooltip = "Navigate to submenu",
                actionBC = (game, slot, chunk) => {
                    Plugin.Logger.LogInfo("Example 3");
                    MouseDrag.MenuManager.SetSubMenuID("CustomSubmenu1");
                }
            });
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                subMenuID = "CustomSubmenu1",
                name = "Back To\nRoot Menu",
                isLabel = true,
                tooltip = "Navigate back",
                reload = (game, slot, chunk) => {
                    MouseDrag.MenuManager.lowPrioText = "In submenu";
                },
                actionBC = (game, slot, chunk) => {
                    MouseDrag.MenuManager.GoToRootMenu();
                }
            });
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                subMenuID = "CustomSubmenu1",
                name = "To Nested\nSubmenu",
                isLabel = true,
                tooltip = "Navigate to nested submenu",
                actionBC = (game, slot, chunk) => {
                    MouseDrag.MenuManager.SetSubMenuID("CustomSubmenu2");
                }
            });
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                subMenuID = "CustomSubmenu2",
                name = "Back To\nFirst Submenu",
                isLabel = true,
                tooltip = "Navigate back",
                reload = (game, slot, chunk) => {
                    MouseDrag.MenuManager.lowPrioText = "In nested submenu";
                },
                actionBC = (game, slot, chunk) => {
                    MouseDrag.MenuManager.SetSubMenuID("CustomSubmenu1");
                }
            });

            //example 4: color cycle
            MouseDrag.MenuManager.registeredSlots.Add(new MouseDrag.RadialMenu.Slot() {
                name = "Menu_Symbol_Repeats", //any random sprite
                tooltip = "Gae",
                reload = (game, slot, chunk) => {
                    slot.tooltipColor = UnityEngine.Color.red;
                },
                actionPO = (game, slot, po) => {
                    Plugin.Logger.LogInfo("Example 4");
                },
                update = (game, slot, chunk) => { //40Hz (tickrate), only runs when on-screen
                    UnityEngine.Vector3 HSL = RWCustom.Custom.RGB2HSL(slot.tooltipColor);
                    HSL.x += 0.01f;
                    if (HSL.x > 1f)
                        HSL.x = 0f;
                    slot.tooltipColor = RWCustom.Custom.HSL2RGB(HSL.x, HSL.y, HSL.z);
                    slot.curIconColor = slot.tooltipColor;
                }
            });
        }
    }
}
