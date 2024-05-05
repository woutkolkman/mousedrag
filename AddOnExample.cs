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

            //hook for running commands
            IDetour detourRunCommand = new Hook(
                typeof(MouseDrag.MenuManager).GetMethod("RunCommand", BindingFlags.Static | BindingFlags.Public),
                typeof(Plugin).GetMethod("MouseDragMenuManager_RunCommand_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );

            //hook for adding sprites
            IDetour detourReloadIconNames = new Hook(
                typeof(MouseDrag.MenuManager).GetMethod("ReloadIconNames", BindingFlags.Static | BindingFlags.Public),
                typeof(Plugin).GetMethod("MouseDragMenuManager_ReloadIconNames_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );

            //hook for adding labels
            IDetour detourReloadLabelNames = new Hook(
                typeof(MouseDrag.MenuManager).GetMethod("ReloadLabelNames", BindingFlags.Static | BindingFlags.Public),
                typeof(Plugin).GetMethod("MouseDragMenuManager_ReloadLabelNames_RuntimeDetour", BindingFlags.Static | BindingFlags.Public)
            );

            //also hook RainWorld.OnModsInit to load your custom sprite, for example use Futile.atlasManager.LoadImage
            //mousedrag sprites are 18x18 pixels with a 1px transparent border (so 16x16 icon)

            GUID = Info.Metadata.GUID;
            Name = Info.Metadata.Name;
            Version = Info.Metadata.Version.ToString();

            Plugin.Logger.LogInfo("OnEnable called");
        }


        //called when mod is unloaded
        public void OnDisable()
        {
            if (!isEnabled) return;
            isEnabled = false;

            Plugin.Logger.LogInfo("OnDisable called");
        }


        //hook for running commands
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MouseDragMenuManager_RunCommand_RuntimeDetour(Action<RainWorldGame, string, bool> orig, RainWorldGame game, string spriteName, bool followsObject)
        {
            orig(game, spriteName, followsObject);
            if (spriteName == "CentipedeSegment") //temporary spritename
                Plugin.Logger.LogInfo("your icon code will run here");
            if (spriteName == "Hello World!")
                Plugin.Logger.LogInfo("your label code will run here");

            //you can reference MouseDrag.MenuManager.menu?.followChunk?.owner to get the object on which actions are performed
            PhysicalObject obj = MouseDrag.MenuManager.menu?.followChunk?.owner;
        }


        //hook for adding sprites
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static List<string> MouseDragMenuManager_ReloadIconNames_RuntimeDetour(Func<RainWorldGame, bool, List<string>> orig, RainWorldGame game, bool followsObject)
        {
            List<string> returnable = orig(game, followsObject);
            if (MouseDrag.MenuManager.subMenuType == MouseDrag.MenuManager.SubMenuTypes.None)
                returnable.Add("CentipedeSegment"); //temporary spritename
            return returnable;
        }


        //hook for adding labels
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static List<string> MouseDragMenuManager_ReloadLabelNames_RuntimeDetour(Func<RainWorldGame, bool, List<string>> orig, RainWorldGame game, bool followsObject)
        {
            List<string> returnable = orig(game, followsObject);
            if (MouseDrag.MenuManager.subMenuType == MouseDrag.MenuManager.SubMenuTypes.None)
                returnable.Add("Hello World!"); //temporary spritename
            return returnable;
        }
    }
}
