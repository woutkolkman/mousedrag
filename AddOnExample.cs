using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MouseDragHelper
{
    //also edit version in "modinfo.json"
    [BepInPlugin("yourname.mousedraghelper", "Mouse Drag Helper", "0.1.0")] //(GUID, mod name, mod version)
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null;

        //reference metadata
        public static string GUID;
        public static string Name;
        public static string Version;

        private static bool isEnabled = false;

        //hooks
        IDetour detourRunAction, detourReloadSlots;


        //called when mod is loaded, subscribe functions to methods of the game
        public void OnEnable()
        {
            if (isEnabled) return;
            isEnabled = true;

            Logger = base.Logger;

            //check if MouseDrag is enabled
            bool mouseDragEnabled = false;
            for (int i = 0; i < ModManager.ActiveMods.Count; i++) {
                if (ModManager.ActiveMods[i].id == "maxi-mol.mousedrag")
                    mouseDragEnabled = true;
            }

            //hook for running actions
            detourRunAction = new Hook(
                typeof(MouseDrag.MenuManager).GetMethod("RunAction", BindingFlags.Static | BindingFlags.Public),
                typeof(Plugin).GetMethod("MouseDragMenuManager_RunAction_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );

            //hook for adding slots
            detourReloadSlots = new Hook(
                typeof(MouseDrag.MenuManager).GetMethod("ReloadSlots", BindingFlags.Static | BindingFlags.Public),
                typeof(Plugin).GetMethod("MouseDragMenuManager_ReloadSlots_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );

            //also hook RainWorld.OnModsInit to load your custom sprite, for example use Futile.atlasManager.LoadImage
            //mousedrag sprites are 18x18 pixels with a 1px transparent border (so 16x16 icon)

            GUID = Info?.Metadata?.GUID;
            Name = Info?.Metadata?.Name;
            Version = Info?.Metadata?.Version?.ToString();

            Plugin.Logger.LogInfo("OnEnable called");
        }


        //called when mod is unloaded
        public void OnDisable()
        {
            if (!isEnabled) return;
            isEnabled = false;

            //optionally dispose of hooks for Rain Reload support
            if (detourRunAction.IsValid)
                detourRunAction.Dispose();
            if (detourReloadSlots.IsValid)
                detourReloadSlots.Dispose();

            Plugin.Logger.LogInfo("OnDisable called");
        }


        //hook for running actions
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MouseDragMenuManager_RunAction_RuntimeDetour(Action<RainWorldGame, MouseDrag.RadialMenu.Slot, BodyChunk> orig, RainWorldGame game, MouseDrag.RadialMenu.Slot slot, BodyChunk chunk)
        {
            orig(game, slot, chunk);
            if (slot?.name == null)
                return;
            if (slot.name == "CentipedeSegment") //temporary spritename
                Plugin.Logger.LogInfo("your icon code will run here");
            if (slot.name == "Hello World!")
                Plugin.Logger.LogInfo("your label code will run here");

            //you can reference chunk?.owner to get the object on which actions are performed
            //if chunk is null, the menu is on the background instead of an object
            PhysicalObject obj = chunk?.owner;
        }


        //hook for adding slots
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static List<MouseDrag.RadialMenu.Slot> MouseDragMenuManager_ReloadSlots_RuntimeDetour(Func<RainWorldGame, MouseDrag.RadialMenu, BodyChunk, List<MouseDrag.RadialMenu.Slot>> orig, RainWorldGame game, MouseDrag.RadialMenu menu, BodyChunk chunk)
        {
            List<MouseDrag.RadialMenu.Slot> returnable = orig(game, menu, chunk);
            if (MouseDrag.MenuManager.subMenuType == MouseDrag.MenuManager.SubMenuTypes.None) {
                returnable.Add(new MouseDrag.RadialMenu.Slot(menu) { name = "CentipedeSegment" }); //temporary sprite name
                returnable.Add(new MouseDrag.RadialMenu.Slot(menu) { name = "Hello World!", isLabel = true }); //temporary label text
            }
            return returnable;
        }
    }
}
