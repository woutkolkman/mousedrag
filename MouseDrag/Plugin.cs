using System;
using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MouseDrag
{
    //also edit version in "modinfo.json"
    [BepInPlugin("maxi-mol.mousedrag", "Mouse Drag", "0.2.2")] //(GUID, mod name, mod version)
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null;

        //reference metadata
        public static string GUID;
        public static string Name;
        public static string Version;

        private static bool IsEnabled = false;


        //called when mod is loaded, subscribe functions to methods of the game
        public void OnEnable()
        {
            if (IsEnabled) return;
            IsEnabled = true;

            Logger = base.Logger;
            Hooks.Apply();
            Patches.Apply();

            GUID = Info.Metadata.GUID;
            Name = Info.Metadata.Name;
            Version = Info.Metadata.Version.ToString();

            Plugin.Logger.LogInfo("OnEnable called");
        }


        //called when mod is unloaded
        public void OnDisable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;

            Hooks.Unapply();
            Patches.Unapply();

            Plugin.Logger.LogInfo("OnDisable called");
        }
    }
}
