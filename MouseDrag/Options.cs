using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<KeyCode> activateKey;
        public static Configurable<bool> menuRMB, menuMMB;
        public static Configurable<KeyCode> menuOpen;
        public static Configurable<bool> menuFollows, menuMoveHover;
        public static Configurable<bool> manageMouseVisibility, deactivateEveryRestart, logDebug;
        public static Configurable<bool> throwWithMouse, throwAsPlayer;
        public static Configurable<float> throwThreshold, throwForce;
        public static Configurable<KeyCode> throwWeapon;
        public static Configurable<bool> velocityDrag;
        public static Configurable<KeyCode> selectCreatures, selectObjects;
        public static Configurable<KeyCode> pauseOneKey, pauseRoomCreaturesKey, unpauseAllKey;
        public static Configurable<KeyCode> pauseAllCreaturesKey, pauseAllObjectsKey;
        public static Configurable<KeyCode> killOneKey, killAllCreaturesKey, reviveOneKey, reviveAllCreaturesKey;
        public static Configurable<KeyCode> duplicateOneKey;
        public static Configurable<KeyCode> tpCreaturesKey, tpObjectsKey;
        public static Configurable<KeyCode> controlKey;
        public static Configurable<KeyCode> forcefieldKey;
        public static Configurable<KeyCode> tameOneKey, tameAllCreaturesKey, clearRelOneKey, clearRelAllKey;
        public static Configurable<KeyCode> stunOneKey, stunRoomKey, unstunAllKey, stunAllKey;
        public static Configurable<KeyCode> destroyOneKey, destroyAllCreaturesKey, destroyAllObjectsKey;
        public static Configurable<KeyCode> destroyRegionCreaturesKey, destroyRegionObjectsKey;
        public static Configurable<bool> pauseOneMenu, pauseRoomCreaturesMenu, unpauseAllMenu;
        public static Configurable<bool> pauseAllCreaturesMenu, pauseAllObjectsMenu;
        public static Configurable<bool> killOneMenu, killAllCreaturesMenu, reviveOneMenu, reviveAllCreaturesMenu;
        public static Configurable<bool> duplicateOneMenu;
        public static Configurable<bool> clipboardMenu, clipboardCtrlXCV;
        public static Configurable<bool> tpWaypointBgMenu, tpWaypointCrMenu;
        public static Configurable<bool> controlMenu;
        public static Configurable<bool> forcefieldMenu;
        public static Configurable<bool> tameOneMenu, tameAllCreaturesMenu, clearRelOneMenu, clearRelAllMenu;
        public static Configurable<bool> stunOneMenu, stunRoomMenu, unstunAllMenu, stunAllMenu;
        public static Configurable<bool> destroyOneMenu, destroyAllCreaturesMenu, destroyRoomMenu;
        public static Configurable<bool> destroyRegionCreaturesMenu, destroyRegionObjectsMenu;
        public static Configurable<bool> releaseGraspsPaused, lineageKill, killReleasesMask;
        public static Configurable<bool> forcefieldImmunityPlayers, forcefieldImmunityObjects;
        public static Configurable<float> forcefieldRadius;
        public static Configurable<bool> beastMasterIntegration;
        public static Configurable<bool> copyID, exitGameOverMode, exceptSlugNPC, tameIncreasesRep;
        public static Configurable<bool> controlChangesCamera, controlOnlyOne, controlNoInput, controlStunsPlayers;
        public int curTab;

        public enum ActivateTypes
        {
            DevToolsActive,
            KeyBindPressed,
            AlwaysActive,
            SandboxAndSafari
        }


        public Options()
        {
            activateType = config.Bind(nameof(activateType), defaultValue: ActivateTypes.AlwaysActive.ToString(), new ConfigurableInfo("Controls are active when this condition is met. Always active in sandbox.", null, "", "Active when"));
            activateKey = config.Bind(nameof(activateKey), KeyCode.None, new ConfigurableInfo("KeyBind to activate controls when \"" + ActivateTypes.KeyBindPressed.ToString() + "\" is selected.", null, "", "KeyBind"));

            menuRMB = config.Bind(nameof(menuRMB), defaultValue: true, new ConfigurableInfo("Right mouse button opens menu on object or background.", null, "", "RMB opens menu"));
            menuMMB = config.Bind(nameof(menuMMB), defaultValue: false, new ConfigurableInfo("Middle mouse (scroll) button opens menu on object or background.", null, "", "MMB opens menu"));
            menuOpen = config.Bind(nameof(menuOpen), KeyCode.None, new ConfigurableInfo("KeyBind opens menu on object or background, as an alternative to right mouse button.", null, "", "Open menu"));
            menuFollows = config.Bind(nameof(menuFollows), defaultValue: true, new ConfigurableInfo("If checked, menu follows the target creature/object on which actions are performed.", null, "", "Menu follows target"));
            menuMoveHover = config.Bind(nameof(menuMoveHover), defaultValue: false, new ConfigurableInfo("If checked, menu follows target also when hovering over it. Unused if \"Menu follows target\" is unchecked.", null, "", "Menu moves if hovering"));
            manageMouseVisibility = config.Bind(nameof(manageMouseVisibility), defaultValue: true, new ConfigurableInfo("Show Windows mouse pointer for 2 seconds in-game when mouse moved. Unchecking this option allows other mods to manage cursor visibility.", null, "", "Manage mouse visibility"));
            deactivateEveryRestart = config.Bind(nameof(deactivateEveryRestart), defaultValue: true, new ConfigurableInfo("Deactivate tools when cycle ends or game is restarted, just like Dev Tools. (only used when 'Active when' is 'KeyBindPressed')", null, "", "Deactivate every restart"));
            logDebug = config.Bind(nameof(logDebug), defaultValue: true, new ConfigurableInfo("Useful for debugging if you share your log files.", null, "", "Log debug"));

            throwWithMouse = config.Bind(nameof(throwWithMouse), defaultValue: true, new ConfigurableInfo("Quickly dragging and releasing weapons will throw them in that direction. Alternative to KeyBind.", null, "", "Throw with mouse"));
            throwAsPlayer = config.Bind(nameof(throwAsPlayer), defaultValue: false, new ConfigurableInfo("Throwing weapons with the mouse will use Player as thrower.", null, "", "Throw as Player"));
            throwThreshold = config.Bind(nameof(throwThreshold), defaultValue: 40f, new ConfigurableInfo("Minimum speed at which weapons are thrown when the mouse is released. Not used via KeyBind.", null, "", "Throw threshold"));
            throwForce = config.Bind(nameof(throwForce), defaultValue: 2f, new ConfigurableInfo("Force at which weapons are thrown.", null, "", "Throw force"));
            throwWeapon = config.Bind(nameof(throwWeapon), KeyCode.None, new ConfigurableInfo("KeyBind to throw the weapon which you're currently dragging. Aim is still determined by drag direction. Sandbox mouse might interfere.", null, "", "Throw weapon"));
            velocityDrag = config.Bind(nameof(velocityDrag), defaultValue: false, new ConfigurableInfo("Alternative dragging method using velocity instead of position. Dragged objects/creatures won't (easily) move through walls.\nYou will also always drag the center of a BodyChunk. Sandbox mouse might interfere.", null, "", "Velocity drag"));
            selectCreatures = config.Bind(nameof(selectCreatures), KeyCode.LeftControl, new ConfigurableInfo("Hold this key to only select or drag creatures.", null, "", "Select creatures"));
            selectObjects = config.Bind(nameof(selectObjects), KeyCode.LeftAlt, new ConfigurableInfo("Hold this key to select or drag anything except creatures.", null, "", "Select objects"));

            pauseOneKey = config.Bind(nameof(pauseOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause the object/creature which you're currently dragging.", null, "", "Pause"));
            pauseRoomCreaturesKey = config.Bind(nameof(pauseRoomCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause all creatures except Player and SlugNPC, only currently in this room.\nAllows unpausing individual creatures.", null, "", "Pause creatures\nin room"));
            unpauseAllKey = config.Bind(nameof(unpauseAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to unpause all objects/creatures, including individually paused creatures.", null, "", "Unpause all"));
            pauseAllCreaturesKey = config.Bind(nameof(pauseAllCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all creatures except Player and SlugNPC, including creatures that still need to spawn.\nIndividually (un)paused creatures remain paused.", null, "", "Pause all creatures"));
            pauseAllObjectsKey = config.Bind(nameof(pauseAllObjectsKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all objects except creatures, including objects that still need to spawn.\nIndividually (un)paused objects remain paused.", null, "", "Pause all objects"));
            killOneKey = config.Bind(nameof(killOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to kill the creature which you're currently dragging. Can also trigger objects like bombs.", null, "", "Kill"));
            killAllCreaturesKey = config.Bind(nameof(killAllCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to kill all creatures in current room except Player and SlugNPC.", null, "", "Kill creatures\nin room"));
            reviveOneKey = config.Bind(nameof(reviveOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal the creature which you're currently dragging. Can also reset objects like popcorn plants.", null, "", "Revive/heal"));
            reviveAllCreaturesKey = config.Bind(nameof(reviveAllCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal all creatures in current room.", null, "", "Revive/heal\ncreatures\nin room"));
            duplicateOneKey = config.Bind(nameof(duplicateOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to duplicate the object/creature which you're currently dragging. Hold button to repeat.", null, "", "Duplicate"));
            tpCreaturesKey = config.Bind(nameof(tpCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to teleport all creatures in current room to the mouse position, except Player and SlugNPC.", null, "", "Teleport creatures\nin room"));
            tpObjectsKey = config.Bind(nameof(tpObjectsKey), KeyCode.None, new ConfigurableInfo("KeyBind to teleport all objects except creatures in current room to the mouse position.", null, "", "Teleport objects\nin room"));
            controlKey = config.Bind(nameof(controlKey), KeyCode.None, new ConfigurableInfo("KeyBind to safari-control the creature which you're currently dragging, or to cycle between creatures if not dragging. Requires Downpour DLC. Controlled creatures do not contribute to map discovery.", null, "", "Safari-control"));

            forcefieldKey = config.Bind(nameof(forcefieldKey), KeyCode.None, new ConfigurableInfo("KeyBind to toggle forcefield on the currently dragged bodychunk. Forcefield is lost if bodychunk is reloaded.", null, "", "Forcefield"));
            tameOneKey = config.Bind(nameof(tameOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to tame the creature which you're currently dragging.", null, "", "Tame"));
            tameAllCreaturesKey = config.Bind(nameof(tameAllCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to tame all creatures in current room.", null, "", "Tame creatures in\nroom"));
            clearRelOneKey = config.Bind(nameof(clearRelOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of the creature which you're currently dragging.", null, "", "Clear relationships"));
            clearRelAllKey = config.Bind(nameof(clearRelAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of all creatures in current room except Player and SlugNPC.", null, "", "Clear relationships\nin room"));
            stunOneKey = config.Bind(nameof(stunOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to stun/unstun the object/creature which you're currently dragging.", null, "", "Stun"));
            stunRoomKey = config.Bind(nameof(stunRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to stun all objects/creatures except Player and SlugNPC, only currently in this room.\nAllows unstunning individual objects/creatures.", null, "", "Stun in room"));
            unstunAllKey = config.Bind(nameof(unstunAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to unstun all objects/creatures, including individually stunned objects/creatures.", null, "", "Unstun all"));
            stunAllKey = config.Bind(nameof(stunAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to stun/unstun all objects/creatures except Player and SlugNPC, including objects/creatures that still need to spawn.\nIndividually (un)stunned objects/creatures remain stunned.", null, "", "Stun all"));
            destroyOneKey = config.Bind(nameof(destroyOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy the object/creature which you're currently dragging.\nTo make creatures respawn, kill and then destroy them.", null, "", "Destroy"));
            destroyAllCreaturesKey = config.Bind(nameof(destroyAllCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all creatures in current room except Player and SlugNPC.\nTo make creatures respawn, kill and then destroy them.", null, "", "Destroy creatures\nin room"));
            destroyAllObjectsKey = config.Bind(nameof(destroyAllObjectsKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all objects in current room except creatures.", null, "", "Destroy objects\nin room"));
            destroyRegionCreaturesKey = config.Bind(nameof(destroyRegionCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all creatures in current region except Player and SlugNPC. Some creatures will be re-added automatically, or are added later on.\nWARNING: If you hibernate afterwards, most creatures in the region will also be gone next cycle. Also don't use this in safari.", null, "", "Destroy creatures\nin region"));
            destroyRegionObjectsKey = config.Bind(nameof(destroyRegionObjectsKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all objects in current region except creatures. Most objects will be re-added automatically, or are added later on.", null, "", "Destroy objects\nin region"));

            pauseOneMenu = config.Bind(nameof(pauseOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseRoomCreaturesMenu = config.Bind(nameof(pauseRoomCreaturesMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            unpauseAllMenu = config.Bind(nameof(unpauseAllMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseAllCreaturesMenu = config.Bind(nameof(pauseAllCreaturesMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseAllObjectsMenu = config.Bind(nameof(pauseAllObjectsMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            killOneMenu = config.Bind(nameof(killOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            killAllCreaturesMenu = config.Bind(nameof(killAllCreaturesMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            reviveOneMenu = config.Bind(nameof(reviveOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            reviveAllCreaturesMenu = config.Bind(nameof(reviveAllCreaturesMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            duplicateOneMenu = config.Bind(nameof(duplicateOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clipboardMenu = config.Bind(nameof(clipboardMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.\nCut/paste PhysicalObjects with a clipboard (LIFO buffer). Clipboard is lost when game is closed.", null, "", ""));
            clipboardCtrlXCV = config.Bind(nameof(clipboardCtrlXCV), defaultValue: false, new ConfigurableInfo("Using Control + X/C/V will cut, copy or paste the object/creature which you're currently dragging.", null, "", "Ctrl + X/C/V"));
            tpWaypointBgMenu = config.Bind(nameof(tpWaypointBgMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.\nSet/reset a waypoint using this option. Click any object/creature to teleport them to the waypoint.", null, "", ""));
            tpWaypointCrMenu = config.Bind(nameof(tpWaypointCrMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.\nSame as above, but on a creature.", null, "", ""));
            controlMenu = config.Bind(nameof(controlMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));

            forcefieldMenu = config.Bind(nameof(forcefieldMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            tameOneMenu = config.Bind(nameof(tameOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            tameAllCreaturesMenu = config.Bind(nameof(tameAllCreaturesMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clearRelOneMenu = config.Bind(nameof(clearRelOneMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clearRelAllMenu = config.Bind(nameof(clearRelAllMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunOneMenu = config.Bind(nameof(stunOneMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunRoomMenu = config.Bind(nameof(stunRoomMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            unstunAllMenu = config.Bind(nameof(unstunAllMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunAllMenu = config.Bind(nameof(stunAllMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyOneMenu = config.Bind(nameof(destroyOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyAllCreaturesMenu = config.Bind(nameof(destroyAllCreaturesMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyRoomMenu = config.Bind(nameof(destroyRoomMenu), defaultValue: true, new ConfigurableInfo("Add action to menu. Destroy all objects/creatures in current room except Player and SlugNPC.\nTo make creatures respawn, kill and then destroy them.", null, "", ""));
            destroyRegionCreaturesMenu = config.Bind(nameof(destroyRegionCreaturesMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyRegionObjectsMenu = config.Bind(nameof(destroyRegionObjectsMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));

            releaseGraspsPaused = config.Bind(nameof(releaseGraspsPaused), defaultValue: false, new ConfigurableInfo("When creature is paused, all grasps (creatures/items) are released.", null, "", "Pausing releases grasps"));
            lineageKill = config.Bind(nameof(lineageKill), defaultValue: false, new ConfigurableInfo("When killing creatures using tools, set killTag to first player so creatures can lineage.\nDestroying creatures without killing them does not result in lineage.", null, "", "Lineage when killed"));
            killReleasesMask = config.Bind(nameof(killReleasesMask), defaultValue: true, new ConfigurableInfo("Killing elite scavengers or vultures with this tool will release their masks.", null, "", "Kill releases mask"));
            forcefieldImmunityPlayers = config.Bind(nameof(forcefieldImmunityPlayers), defaultValue: true, new ConfigurableInfo("Players and SlugNPCs are unaffected by forcefields.", null, "", "Forcefield immunity players"));
            forcefieldImmunityObjects = config.Bind(nameof(forcefieldImmunityObjects), defaultValue: false, new ConfigurableInfo("Objects (except thrown weapons) are unaffected by forcefields.", null, "", "Forcefield immunity objects"));
            forcefieldRadius = config.Bind(nameof(forcefieldRadius), defaultValue: 120f, new ConfigurableInfo(null, null, "", "Forcefield radius"));

            beastMasterIntegration = config.Bind(nameof(beastMasterIntegration), defaultValue: true, new ConfigurableInfo("If BeastMaster is enabled, right-clicking on its menu will not open this mod's menu. Requires restart.", null, "", "BeastMaster integration"));

            copyID = config.Bind(nameof(copyID), defaultValue: true, new ConfigurableInfo("Creates an exact copy of the previous object when duplicating.", null, "", "Copy ID duplicate"));
            exitGameOverMode = config.Bind(nameof(exitGameOverMode), defaultValue: true, new ConfigurableInfo("Try to exit game over mode when reviving player. Might be incompatible with some other mods.", null, "", "Exit game over mode"));
            exceptSlugNPC = config.Bind(nameof(exceptSlugNPC), defaultValue: false, new ConfigurableInfo("If checked, do not pause/destroy/kill slugpups when pausing/destroying/killing all creatures.", null, "", "Except SlugNPC"));
            tameIncreasesRep = config.Bind(nameof(tameIncreasesRep), defaultValue: false, new ConfigurableInfo("Taming creatures using this tool also increases global reputation.", null, "", "Taming global +rep"));
            controlChangesCamera = config.Bind(nameof(controlChangesCamera), defaultValue: true, new ConfigurableInfo("Safari-controlling creatures will change which creature the camera follows. Might not work well with other camera/multiplayer mods. Does not work in safari because of the overseer.", null, "", "Safari-control changes camera"));
            controlOnlyOne = config.Bind(nameof(controlOnlyOne), defaultValue: false, new ConfigurableInfo("Safari-controlling another creature (while already controlling a creature) will remove control from the first one, so you will only control one creature at a time.", null, "", "Safari-control only one creature"));
            controlNoInput = config.Bind(nameof(controlNoInput), defaultValue: false, new ConfigurableInfo("While safari-controlling creatures, only the creature which the camera is following will move. Unused if \"Safari-control changes camera\" is unchecked.", null, "", "Reset other safari-control input"));
            controlStunsPlayers = config.Bind(nameof(controlStunsPlayers), defaultValue: true, new ConfigurableInfo("Safari-controlling creatures will stun the (last dragged) player, as this player will now control the creature.", null, "", "Safari-control stuns players"));
        }


        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General"),
                new OpTab(this, "Tools"),
                new OpTab(this, "Other")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            float x = 90f;
            float y = 460f;
            float sepr = 40f;
            AddComboBox(activateType, new Vector2(190f, 493f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddKeyBinder(activateKey, new Vector2(330f, 490f));

            AddCheckbox(menuRMB, new Vector2(x, y -= sepr));
            AddCheckbox(menuMMB, new Vector2(x, y -= sepr));
            AddKeyBinder(menuOpen, new Vector2(x, y -= sepr + 5f));
            AddCheckbox(menuFollows, new Vector2(x, y -= sepr));
            AddCheckbox(menuMoveHover, new Vector2(x, y -= sepr));
            AddCheckbox(manageMouseVisibility, new Vector2(x, y -= sepr));
            AddCheckbox(deactivateEveryRestart, new Vector2(x, y -= sepr));
            AddCheckbox(logDebug, new Vector2(x, y -= sepr));

            x += 250f;
            y = 460f;
            AddCheckbox(throwWithMouse, new Vector2(x, y -= sepr));
            AddCheckbox(throwAsPlayer, new Vector2(x, y -= sepr));
            AddTextBox(throwThreshold, new Vector2(x, y -= sepr), 40f);
            AddTextBox(throwForce, new Vector2(x, y -= sepr), 40f);
            AddKeyBinder(throwWeapon, new Vector2(x, y -= sepr + 5f));
            AddCheckbox(velocityDrag, new Vector2(x, y -= sepr));
            AddKeyBinder(selectCreatures, new Vector2(x, y -= sepr + 5f));
            AddKeyBinder(selectObjects, new Vector2(x, y -= sepr + 5f));

            /**************** Tools ****************/
            curTab++;
            x = 70f;
            y = 600f;
            sepr = 42f;
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPause");
            AddCheckbox(pauseOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseRoomCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseCreatures");
            AddCheckbox(pauseRoomCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPlayAll");
            AddCheckbox(unpauseAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseGlobal");
            AddCheckbox(pauseAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseAllObjectsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseGlobal");
            AddCheckbox(pauseAllObjectsMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(killOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragKill");
            AddCheckbox(killOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(killAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragKillCreatures");
            AddCheckbox(killAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(reviveOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragRevive");
            AddCheckbox(reviveOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(reviveAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragReviveCreatures");
            AddCheckbox(reviveAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(duplicateOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDuplicate");
            AddCheckbox(duplicateOneMenu, new Vector2(x - 56f, y + 3f));
            AddCheckbox(clipboardMenu, new Vector2(x - 56f, (y -= sepr) + 3f));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCut");
            AddIcon(new Vector2(x, y + 6f), "mousedragPaste");
            AddCheckbox(clipboardCtrlXCV, new Vector2(x + 25f + 51f, y + 3f));
            AddKeyBinder(tpCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCrosshair");
            AddCheckbox(tpWaypointBgMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tpObjectsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCrosshair");
            AddCheckbox(tpWaypointCrMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(controlKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragMove");
            AddCheckbox(controlMenu, new Vector2(x - 56f, y + 3f));

            x += 300f;
            y = 600f;
            AddKeyBinder(forcefieldKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragForceFieldOn");
            AddCheckbox(forcefieldMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tameOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragHeart");
            AddCheckbox(tameOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tameAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragHeartCreatures");
            AddCheckbox(tameAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(clearRelOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnheart");
            AddCheckbox(clearRelOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(clearRelAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnheartCreatures");
            AddCheckbox(clearRelAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStun");
            AddCheckbox(stunOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStunAll");
            AddCheckbox(stunRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(unstunAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnstunAll");
            AddCheckbox(unstunAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStunGlobal");
            AddCheckbox(stunAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroy");
            AddCheckbox(destroyOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyCreatures");
            AddCheckbox(destroyAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyAllObjectsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyAll");
            AddCheckbox(destroyRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyRegionCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyGlobal");
            AddCheckbox(destroyRegionCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyRegionObjectsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyGlobal");
            AddCheckbox(destroyRegionObjectsMenu, new Vector2(x - 56f, y + 3f));

            /**************** Other ****************/
            curTab++;
            x = 90f;
            y = 595f;
            sepr = 40f;
            AddCheckbox(releaseGraspsPaused, new Vector2(x, y -= sepr));
            AddCheckbox(lineageKill, new Vector2(x, y -= sepr));
            AddCheckbox(killReleasesMask, new Vector2(x, y -= sepr));
            AddCheckbox(forcefieldImmunityPlayers, new Vector2(x, y -= sepr));
            AddCheckbox(forcefieldImmunityObjects, new Vector2(x, y -= sepr));
            AddTextBox(forcefieldRadius, new Vector2(x, y -= sepr), 50f);

            y = -19f; //from bottom up
            AddCheckbox(beastMasterIntegration, new Vector2(x, y += sepr));

            x += 250f;
            y = 595f;
            AddCheckbox(copyID, new Vector2(x, y -= sepr));
            AddCheckbox(exitGameOverMode, new Vector2(x, y -= sepr));
            AddCheckbox(exceptSlugNPC, new Vector2(x, y -= sepr));
            AddCheckbox(tameIncreasesRep, new Vector2(x, y -= sepr));
            AddCheckbox(controlChangesCamera, new Vector2(x, y -= sepr));
            AddCheckbox(controlOnlyOne, new Vector2(x, y -= sepr));
            AddCheckbox(controlNoInput, new Vector2(x, y -= sepr));
            AddCheckbox(controlStunsPlayers, new Vector2(x, y -= sepr));
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
