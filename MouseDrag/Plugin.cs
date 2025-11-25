using BepInEx;
using BepInEx.Logging;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MouseDrag
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        //metadata
        public const string GUID = "maxi-mol.mousedrag";
        public const string Name = "Mouse Drag";
        public const string Version = "1.2.0"; //also edit version in "modinfo.json"

        public static new ManualLogSource Logger { get; private set; } = null;
        private static bool isEnabled = false;


        //called when mod is loaded, subscribe functions to methods of the game
        public void OnEnable()
        {
            if (isEnabled) return;
            isEnabled = true;

            Logger = base.Logger;
            Hooks.Apply();
            ILHooks.Apply();
            StandardSlots.RegisterSlots(); //first call is for loading slots, second call via Options EventHandler is for correctly positioning slots in the list

            //Rain Reloader re-initialize Options, sprites and integration
            if (MachineConnector.IsThisModActive(GUID)) {
                Plugin.Logger.LogDebug("OnEnable, re-initializing options interface and sprites");
                MachineConnector.SetRegisteredOI(GUID, new Options());
                MachineConnector.ReloadConfig(MachineConnector.GetRegisteredOI(GUID));
                MenuManager.LoadSprites();
                ForceField.LoadSprites();
                Integration.Hooks.Apply();
                if (Integration.devConsoleEnabled) {
                    try {
                        Integration.DevConsoleRegisterCommands();
                    } catch (System.Exception ex) {
                        Plugin.Logger.LogError("OnEnable exception during registration of commands Dev Console, integration is now disabled: " + ex?.ToString());
                        Integration.devConsoleEnabled = false;
                    }
                }
            }

            Plugin.Logger.LogInfo("OnEnable called");
        }


        //called when mod is unloaded
        public void OnDisable()
        {
            if (!isEnabled) return;
            isEnabled = false;

            Hooks.Unapply();
            ILHooks.Unapply();
            Integration.Hooks.Unapply();
            MenuManager.UnloadSprites();
            ForceField.UnloadSprites();

            Plugin.Logger.LogInfo("OnDisable called");
        }
    }
}
