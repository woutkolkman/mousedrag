﻿using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace FreeCam
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        //metadata
        public const string GUID = "maxi-mol.freecam";
        public const string Name = "FreeCam";
        public const string Version = "0.1.0"; //also edit version in "modinfo.json"

        public static new ManualLogSource Logger { get; private set; } = null;
        private static bool isEnabled = false;


        //called when mod is loaded, subscribe functions to methods of the game
        public void OnEnable()
        {
            if (isEnabled) return;
            isEnabled = true;

            Logger = base.Logger;
            Hooks.Apply();
            Patches.Apply();

            //Rain Reloader re-initialize Options and sprites
            if (MachineConnector.IsThisModActive(GUID)) {
                Plugin.Logger.LogDebug("OnEnable, re-initializing options interface and sprites");
                MachineConnector.SetRegisteredOI(GUID, new Options());
                MachineConnector.ReloadConfig(MachineConnector.GetRegisteredOI(GUID));
                Integration.RefreshActiveMods();
                Integration.Hooks.Apply();
            }

            Plugin.Logger.LogInfo("OnEnable called");
        }


        //called when mod is unloaded
        public void OnDisable()
        {
            if (!isEnabled) return;
            isEnabled = false;

            Hooks.Unapply();
            Patches.Unapply();
            Integration.Hooks.Unapply();

            Plugin.Logger.LogInfo("OnDisable called");
        }
    }
}