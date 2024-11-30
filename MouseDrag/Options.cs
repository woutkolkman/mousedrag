using Menu.Remix.MixedUI;
using UnityEngine;
using System;

namespace MouseDrag
{
    public class Options : OptionInterface
    {
        public static Configurable<string> activateType;
        public static Configurable<KeyCode> activateKey;

        public static Configurable<bool> deactivateEveryRestart;
        public static Configurable<bool> disVnlMouseDragger;
        public static Configurable<bool> logDebug;
        public static Configurable<bool> dragLMB, dragMMB;
        public static Configurable<KeyCode> drag;
        public static Configurable<bool> disVnlCursor;
        public static Configurable<string> winCursorVisType;

        public static Configurable<bool> throwWithMouse, throwAsPlayer;
        public static Configurable<float> throwThreshold, throwForce;
        public static Configurable<KeyCode> throwWeapon;
        public static Configurable<bool> velocityDrag, velocityDragAtScreenChange;
        public static Configurable<KeyCode> selectCreatures, selectItems;

        public static Configurable<bool> menuOpenRMB, menuOpenMMB;
        public static Configurable<KeyCode> menuOpen;
        public static Configurable<bool> menuSelectLMB, menuSelectMMB;
        public static Configurable<KeyCode> menuSelect;
        public static Configurable<bool> beastMasterIntegration, splitScreenCoopIntegration;

        public static Configurable<bool> showLabel, showTooltips;
        public static Configurable<int> maxOnPage;
        public static Configurable<bool> menuFollows, menuMoveHover;
        public static Configurable<bool> sBCameraScrollIntegration, regionKitIntegration, devConsoleIntegration;

        public static Configurable<KeyCode> pauseOneKey, pauseRoomCreaturesKey, unpauseAllKey;
        public static Configurable<KeyCode> pauseAllCreaturesKey, pauseAllItemsKey;
        public static Configurable<KeyCode> killOneKey, killRoomKey, reviveOneKey, reviveRoomKey;
        public static Configurable<KeyCode> duplicateOneKey, tpCreaturesKey, tpItemsKey, controlKey;

        public static Configurable<KeyCode> forcefieldKey;
        public static Configurable<KeyCode> tameOneKey, tameRoomKey, clearRelOneKey, clearRelRoomKey;
        public static Configurable<KeyCode> stunOneKey, stunRoomKey, unstunAllKey, stunAllKey;
        public static Configurable<KeyCode> destroyOneKey, destroyRoomCreaturesKey, destroyRoomItemsKey;
        public static Configurable<KeyCode> destroyRegionCreaturesKey, destroyRegionItemsKey;

        public static Configurable<KeyCode> destroyRoomDeadCreaturesKey, lockKey, copySelectorKey;

        public static Configurable<KeyCode> gravityRoomKey, infoKey, loadRegionRoomsKey;

        public static Configurable<bool> pauseOneMenu, pauseRoomCreaturesMenu, unpauseAllMenu;
        public static Configurable<bool> pauseAllCreaturesMenu, pauseAllItemsMenu;
        public static Configurable<bool> killOneMenu, killRoomMenu, reviveOneMenu, reviveRoomMenu;
        public static Configurable<bool> duplicateOneMenu;
        public static Configurable<bool> clipboardMenu, clipboardCtrlXCV;
        public static Configurable<bool> tpWaypointBgMenu, tpWaypointCrMenu, controlMenu;

        public static Configurable<bool> forcefieldMenu;
        public static Configurable<bool> tameOneMenu, tameRoomMenu, clearRelOneMenu, clearRelRoomMenu;
        public static Configurable<bool> stunOneMenu, stunRoomMenu, unstunAllMenu, stunAllMenu;
        public static Configurable<bool> destroyOneMenu, destroyRoomCreaturesMenu, destroyRoomItemsMenu, destroyRoomObjectsMenu;
        public static Configurable<bool> destroyRegionCreaturesMenu, destroyRegionItemsMenu;

        public static Configurable<bool> destroyRoomDeadCreaturesMenu, lockMenu, copySelectorMenu;

        public static Configurable<bool> gravityRoomMenu, infoMenu;

        public static Configurable<bool> releaseGraspsPaused, lineageKill, killReleasesMask, healLimbs;
        public static Configurable<bool> adjustableLocks;
        public static Configurable<bool> forcefieldImmunityPlayers, forcefieldImmunityItems;
        public static Configurable<float> forcefieldRadius;
        public static Configurable<int> infoDepth;

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

        public enum CursorVisibilityTypes
        {
            NoChanges,
            ForceVisible,
            ForceInvisible,
            Moved2Seconds
        }


        public Options()
        {
            activateType = config.Bind(nameof(activateType), defaultValue: ActivateTypes.AlwaysActive.ToString(), new ConfigurableInfo("Controls are active when this condition is met. Always active if sandbox mouse dragger is available.", null, "", "Active when"));
            activateKey = config.Bind(nameof(activateKey), KeyCode.None, new ConfigurableInfo("KeyBind to activate controls when \"" + ActivateTypes.KeyBindPressed.ToString() + "\" is selected.", null, "", "KeyBind"));

            deactivateEveryRestart = config.Bind(nameof(deactivateEveryRestart), defaultValue: true, new ConfigurableInfo("Deactivate tools when cycle ends or game is restarted, just like Dev Tools. (only used when 'Active when' is 'KeyBindPressed')", null, "", "Deactivate every restart"));
            disVnlMouseDragger = config.Bind(nameof(disVnlMouseDragger), defaultValue: true, new ConfigurableInfo("Disable vanilla sandbox mouse dragger, because it is replaced by this mod. Can solve some rare issues while dragging in sandbox.", null, "", "Disable sandbox mouse"));
            logDebug = config.Bind(nameof(logDebug), defaultValue: true, new ConfigurableInfo("Useful for debugging if you share your log files.", null, "", "Log debug"));
            dragLMB = config.Bind(nameof(dragLMB), defaultValue: true, new ConfigurableInfo("Left mouse button is used to drag objects.", null, "", "LMB drags"));
            dragMMB = config.Bind(nameof(dragMMB), defaultValue: false, new ConfigurableInfo("Middle mouse (scroll) button is used to drag objects.", null, "", "MMB drags"));
            drag = config.Bind(nameof(drag), KeyCode.None, new ConfigurableInfo("KeyBind is used to drag objects, as an alternative to left mouse button.", null, "", "Drag"));
            disVnlCursor = config.Bind(nameof(disVnlCursor), defaultValue: false, new ConfigurableInfo("Disables Rain World cursor. Solves the double mouse pointers in sandbox.", null, "", "Hide Rain World cursor"));
            winCursorVisType = config.Bind(nameof(winCursorVisType), defaultValue: CursorVisibilityTypes.Moved2Seconds.ToString(), new ConfigurableInfo("Change visibility of Windows cursor in-game. Set to \"" + CursorVisibilityTypes.NoChanges.ToString() + "\" to allow other mods to manage cursor visibility.", null, "", "Windows\ncursor"));

            throwWithMouse = config.Bind(nameof(throwWithMouse), defaultValue: true, new ConfigurableInfo("Quickly dragging and releasing weapons will throw them in that direction. Alternative to KeyBind.", null, "", "Throw with mouse"));
            throwAsPlayer = config.Bind(nameof(throwAsPlayer), defaultValue: false, new ConfigurableInfo("Throwing weapons with the mouse will use Player as thrower.", null, "", "Throw as Player"));
            throwThreshold = config.Bind(nameof(throwThreshold), defaultValue: 40f, new ConfigurableInfo("Minimum speed at which weapons are thrown when the mouse is released. Not used via KeyBind.", null, "", "Throw threshold"));
            throwForce = config.Bind(nameof(throwForce), defaultValue: 2f, new ConfigurableInfo("Force at which weapons are thrown.", null, "", "Throw force"));
            throwWeapon = config.Bind(nameof(throwWeapon), KeyCode.None, new ConfigurableInfo("KeyBind to throw the weapon which you're currently dragging. Aim is still determined by drag direction. Sandbox mouse might interfere (if initialized).", null, "", "Throw weapon"));
            velocityDrag = config.Bind(nameof(velocityDrag), defaultValue: false, new ConfigurableInfo("Alternative dragging method using velocity instead of position. Dragged objects won't (easily) move through walls.\nYou will also always drag the center of a BodyChunk. Sandbox mouse might interfere (if initialized).", null, "", "Velocity drag"));
            velocityDragAtScreenChange = config.Bind(nameof(velocityDragAtScreenChange), defaultValue: true, new ConfigurableInfo("Temporarily enable velocity drag when screen changes until you release LMB. This way you won't smash your scug into a wall.", null, "", "Velocity drag at screen change"));
            selectCreatures = config.Bind(nameof(selectCreatures), KeyCode.LeftControl, new ConfigurableInfo("Hold this key to only select or drag creatures.", null, "", "Select creatures"));
            selectItems = config.Bind(nameof(selectItems), KeyCode.LeftAlt, new ConfigurableInfo("Hold this key to select or drag anything except creatures.", null, "", "Select items"));

            menuOpenRMB = config.Bind(nameof(menuOpenRMB), defaultValue: true, new ConfigurableInfo("Right mouse button opens menu on object or background.", null, "", "RMB opens menu"));
            menuOpenMMB = config.Bind(nameof(menuOpenMMB), defaultValue: false, new ConfigurableInfo("Middle mouse (scroll) button opens menu on object or background.", null, "", "MMB opens menu"));
            menuOpen = config.Bind(nameof(menuOpen), KeyCode.None, new ConfigurableInfo("KeyBind opens menu on object or background, as an alternative to right mouse button.", null, "", "Open menu"));
            menuSelectLMB = config.Bind(nameof(menuSelectLMB), defaultValue: true, new ConfigurableInfo("Left mouse button selects actions in menu.", null, "", "LMB selects in menu"));
            menuSelectMMB = config.Bind(nameof(menuSelectMMB), defaultValue: false, new ConfigurableInfo("Middle mouse (scroll) button selects actions in menu.", null, "", "MMB selects in menu"));
            menuSelect = config.Bind(nameof(menuSelect), KeyCode.None, new ConfigurableInfo("KeyBind selects actions in menu, as an alternative to left mouse button.", null, "", "Select in menu"));
            beastMasterIntegration = config.Bind(nameof(beastMasterIntegration), defaultValue: true, new ConfigurableInfo("If BeastMaster is enabled, right-clicking on its menu will not open this mod's menu.", null, "", "BeastMaster integration"));
            splitScreenCoopIntegration = config.Bind(nameof(splitScreenCoopIntegration), defaultValue: true, new ConfigurableInfo("If SplitScreen Co-op is enabled, dragging on other cameras is supported.", null, "", "SplitScreen Co-op integration"));

            showLabel = config.Bind(nameof(showLabel), defaultValue: true, new ConfigurableInfo("Show label above menu with name of selected object, and other text.", null, "", "Show label"));
            showTooltips = config.Bind(nameof(showTooltips), defaultValue: true, new ConfigurableInfo("The label above menu will also show information about the selected tool or action. Disable this if it is too distracting.", null, "", "Show tooltips"));
            maxOnPage = config.Bind(nameof(maxOnPage), defaultValue: 7, new ConfigurableInfo("Max amount of tools on a single menu page.", new ConfigAcceptableRange<int>(1, 999), "", "Max on page"));
            menuFollows = config.Bind(nameof(menuFollows), defaultValue: true, new ConfigurableInfo("If checked, menu follows the target object on which actions are performed.", null, "", "Menu follows target"));
            menuMoveHover = config.Bind(nameof(menuMoveHover), defaultValue: false, new ConfigurableInfo("If checked, menu follows target also when hovering over it. Unused if \"Menu follows target\" is unchecked.", null, "", "Menu moves if hovering"));
            sBCameraScrollIntegration = config.Bind(nameof(sBCameraScrollIntegration), defaultValue: true, new ConfigurableInfo("If SBCameraScroll is enabled, dragging with alternative camera zoom is supported.", null, "", "SBCameraScroll integration"));
            regionKitIntegration = config.Bind(nameof(regionKitIntegration), defaultValue: true, new ConfigurableInfo("If RegionKit is enabled, right mouse button will not open the radialmenu behind the Dev Tools menu.\nThis is added because Iggy uses right mouse button to display tips.", null, "", "RegionKit integration"));
            devConsoleIntegration = config.Bind(nameof(devConsoleIntegration), defaultValue: true, new ConfigurableInfo("If Dev Console is enabled, additional commands are available via the console. Requires restart.\nAll commands added by " + Plugin.Name + " start with \"md_\".", null, "", "Dev Console integration"));

            pauseOneKey = config.Bind(nameof(pauseOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause the object which you're currently dragging.", null, "", "Pause"));
            pauseRoomCreaturesKey = config.Bind(nameof(pauseRoomCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause all creatures except Player and SlugNPC, only currently in this room.\nAllows unpausing individual creatures.", null, "", "Pause creatures\nin room"));
            unpauseAllKey = config.Bind(nameof(unpauseAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to unpause all objects, including individually paused objects.", null, "", "Unpause all"));
            pauseAllCreaturesKey = config.Bind(nameof(pauseAllCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all creatures except Player and SlugNPC, including creatures that still need to spawn.\nIndividually (un)paused creatures remain paused.", null, "", "Pause all creatures"));
            pauseAllItemsKey = config.Bind(nameof(pauseAllItemsKey), KeyCode.None, new ConfigurableInfo("KeyBind to pause/unpause all items, including items that still need to spawn.\nIndividually (un)paused items remain paused.", null, "", "Pause all items"));
            killOneKey = config.Bind(nameof(killOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to kill the creature which you're currently dragging. Can also trigger items like bombs.", null, "", "Kill"));
            killRoomKey = config.Bind(nameof(killRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to kill all creatures in current room except Player and SlugNPC.", null, "", "Kill in room"));
            reviveOneKey = config.Bind(nameof(reviveOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal the creature which you're currently dragging. Can also reset items like popcorn plants.", null, "", "Revive/heal"));
            reviveRoomKey = config.Bind(nameof(reviveRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to revive and heal all creatures in current room.", null, "", "Revive/heal\nin room"));
            duplicateOneKey = config.Bind(nameof(duplicateOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to duplicate the object which you're currently dragging. Hold button to repeat.", null, "", "Duplicate"));
            tpCreaturesKey = config.Bind(nameof(tpCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to teleport all creatures in current room to the mouse position, except Player and SlugNPC.", null, "", "Teleport creatures\nin room"));
            tpItemsKey = config.Bind(nameof(tpItemsKey), KeyCode.None, new ConfigurableInfo("KeyBind to teleport all items in current room to the mouse position.", null, "", "Teleport items\nin room"));
            controlKey = config.Bind(nameof(controlKey), KeyCode.None, new ConfigurableInfo("KeyBind to safari-control the creature which you're currently dragging, or to cycle between creatures if not dragging. Requires Downpour DLC. Controlled creatures do not contribute to map discovery.", null, "", "Safari-control"));

            forcefieldKey = config.Bind(nameof(forcefieldKey), KeyCode.None, new ConfigurableInfo("KeyBind to toggle forcefield on the currently dragged BodyChunk. Forcefield is lost if BodyChunk is reloaded.\nA forcefield will push away objects within the configured radius.", null, "", "Forcefield"));
            tameOneKey = config.Bind(nameof(tameOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to tame the creature which you're currently dragging.", null, "", "Tame"));
            tameRoomKey = config.Bind(nameof(tameRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to tame all creatures in current room.", null, "", "Tame in room"));
            clearRelOneKey = config.Bind(nameof(clearRelOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of the creature which you're currently dragging.", null, "", "Clear relationships"));
            clearRelRoomKey = config.Bind(nameof(clearRelRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to clear all relationships of all creatures in current room except Player and SlugNPC.", null, "", "Clear relationships\nin room"));
            stunOneKey = config.Bind(nameof(stunOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to stun/unstun the object which you're currently dragging.", null, "", "Stun"));
            stunRoomKey = config.Bind(nameof(stunRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to stun all objects except Player and SlugNPC, only currently in this room.\nAllows unstunning individual objects.", null, "", "Stun in room"));
            unstunAllKey = config.Bind(nameof(unstunAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to unstun all objects, including individually stunned objects.", null, "", "Unstun all"));
            stunAllKey = config.Bind(nameof(stunAllKey), KeyCode.None, new ConfigurableInfo("KeyBind to stun/unstun all objects except Player and SlugNPC, including objects that still need to spawn.\nIndividually (un)stunned objects remain stunned.", null, "", "Stun all"));
            destroyOneKey = config.Bind(nameof(destroyOneKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy the object which you're currently dragging.\nTo make creatures respawn, or to properly update trackers, kill and then destroy them.", null, "", "Destroy"));
            destroyRoomCreaturesKey = config.Bind(nameof(destroyRoomCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all creatures in current room except Player and SlugNPC.\nTo make creatures respawn, or to properly update trackers, kill and then destroy them.", null, "", "Destroy creatures\nin room"));
            destroyRoomItemsKey = config.Bind(nameof(destroyRoomItemsKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all items in current room.", null, "", "Destroy items\nin room"));
            destroyRegionCreaturesKey = config.Bind(nameof(destroyRegionCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all creatures in current region except Player and SlugNPC. Some creatures will be re-added automatically, or are added later on.\nWARNING: If you hibernate afterwards, most creatures in the region will also be gone next cycle. It's better to not use this in safari.", null, "", "Destroy creatures\nin region"));
            destroyRegionItemsKey = config.Bind(nameof(destroyRegionItemsKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all items in current region. Most items will be re-added automatically, or are added later on.", null, "", "Destroy items\nin region"));

            destroyRoomDeadCreaturesKey = config.Bind(nameof(destroyRoomDeadCreaturesKey), KeyCode.None, new ConfigurableInfo("KeyBind to destroy all dead creatures in current room except Player and SlugNPC.", null, "", "Destroy dead\ncreatures in room"));
            lockKey = config.Bind(nameof(lockKey), KeyCode.None, new ConfigurableInfo("KeyBind to apply a position lock to the BodyChunk which you're currently dragging. A lock is lost if the object is reloaded.", null, "", "Lock position"));
            copySelectorKey = config.Bind(nameof(copySelectorKey), KeyCode.None, new ConfigurableInfo("KeyBind to copy the selector of the object which you're currently dragging. Dev Console will also automatically be opened.\nOnly works if Dev Console integration is enabled.", null, "", "Copy selector &\nopen Dev Console"));

            gravityRoomKey = config.Bind(nameof(gravityRoomKey), KeyCode.None, new ConfigurableInfo("KeyBind to toggle gravity in all rooms. 5 states can be assigned: None/Off/Low/On/Inverse.", null, "", "Gravity"));
            infoKey = config.Bind(nameof(infoKey), KeyCode.None, new ConfigurableInfo("KeyBind to dump all data to your clipboard of the object which you're currently dragging, or the room if nothing is being dragged.", null, "", "Info"));
            loadRegionRoomsKey = config.Bind(nameof(loadRegionRoomsKey), KeyCode.None, new ConfigurableInfo("KeyBind to activate/load all rooms (including objects!) in current region. Rooms visited by a player\nwill still get tracked and unloaded when leaving. WARNING: This WILL lag your PC.", null, "", "Activate rooms\nin region"));

            pauseOneMenu = config.Bind(nameof(pauseOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseRoomCreaturesMenu = config.Bind(nameof(pauseRoomCreaturesMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            unpauseAllMenu = config.Bind(nameof(unpauseAllMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseAllCreaturesMenu = config.Bind(nameof(pauseAllCreaturesMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            pauseAllItemsMenu = config.Bind(nameof(pauseAllItemsMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            killOneMenu = config.Bind(nameof(killOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            killRoomMenu = config.Bind(nameof(killRoomMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            reviveOneMenu = config.Bind(nameof(reviveOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            reviveRoomMenu = config.Bind(nameof(reviveRoomMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            duplicateOneMenu = config.Bind(nameof(duplicateOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clipboardMenu = config.Bind(nameof(clipboardMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.\nCut/paste objects with a clipboard (LIFO buffer). Clipboard is lost when game is closed.", null, "", ""));
            clipboardCtrlXCV = config.Bind(nameof(clipboardCtrlXCV), defaultValue: false, new ConfigurableInfo("Using Control + X/C/V will cut, copy or paste the object which you're currently dragging.", null, "", "Ctrl + X/C/V"));
            tpWaypointBgMenu = config.Bind(nameof(tpWaypointBgMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.\nSet/reset a waypoint using this option. Click any object to teleport it to this waypoint.", null, "", ""));
            tpWaypointCrMenu = config.Bind(nameof(tpWaypointCrMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.\nSame as above, but on a BodyChunk.", null, "", ""));
            controlMenu = config.Bind(nameof(controlMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));

            forcefieldMenu = config.Bind(nameof(forcefieldMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            tameOneMenu = config.Bind(nameof(tameOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            tameRoomMenu = config.Bind(nameof(tameRoomMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clearRelOneMenu = config.Bind(nameof(clearRelOneMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            clearRelRoomMenu = config.Bind(nameof(clearRelRoomMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunOneMenu = config.Bind(nameof(stunOneMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunRoomMenu = config.Bind(nameof(stunRoomMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            unstunAllMenu = config.Bind(nameof(unstunAllMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            stunAllMenu = config.Bind(nameof(stunAllMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyOneMenu = config.Bind(nameof(destroyOneMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyRoomCreaturesMenu = config.Bind(nameof(destroyRoomCreaturesMenu), defaultValue: true, new ConfigurableInfo("Add action to menu. Destroy all creatures in current room except Player and SlugNPC.\nTo make creatures respawn, or to properly update trackers, kill and then destroy them.", null, "", ""));
            destroyRoomItemsMenu = config.Bind(nameof(destroyRoomItemsMenu), defaultValue: false, new ConfigurableInfo("Add action to menu. Destroy all items in current room.", null, "", ""));
            destroyRoomObjectsMenu = config.Bind(nameof(destroyRoomObjectsMenu), defaultValue: true, new ConfigurableInfo("Add action to menu. Destroy all objects in current room except Player and SlugNPC.\nTo make creatures respawn, or to properly update trackers, kill and then destroy them.", null, "", ""));
            destroyRegionCreaturesMenu = config.Bind(nameof(destroyRegionCreaturesMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            destroyRegionItemsMenu = config.Bind(nameof(destroyRegionItemsMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));

            destroyRoomDeadCreaturesMenu = config.Bind(nameof(destroyRoomDeadCreaturesMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            lockMenu = config.Bind(nameof(lockMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            copySelectorMenu = config.Bind(nameof(copySelectorMenu), defaultValue: true, new ConfigurableInfo("Add action to menu.", null, "", ""));

            gravityRoomMenu = config.Bind(nameof(gravityRoomMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));
            infoMenu = config.Bind(nameof(infoMenu), defaultValue: false, new ConfigurableInfo("Add action to menu.", null, "", ""));

            releaseGraspsPaused = config.Bind(nameof(releaseGraspsPaused), defaultValue: false, new ConfigurableInfo("When creature is paused, all grasps (creatures/items) are released.", null, "", "Pausing releases grasps"));
            lineageKill = config.Bind(nameof(lineageKill), defaultValue: false, new ConfigurableInfo("When killing creatures using tools, set killTag to first player so creatures can lineage.\nDestroying creatures without killing them does not result in lineage.", null, "", "Lineage when killed"));
            killReleasesMask = config.Bind(nameof(killReleasesMask), defaultValue: true, new ConfigurableInfo("Killing elite scavengers or vultures with this tool will release their masks.", null, "", "Kill releases mask"));
            healLimbs = config.Bind(nameof(healLimbs), defaultValue: true, new ConfigurableInfo("Healing or reviving creatures using tools will also heal their limbs/wings/tentacles.", null, "", "Heal limbs"));
            adjustableLocks = config.Bind(nameof(adjustableLocks), defaultValue: true, new ConfigurableInfo("BodyChunks can be adjusted while locked in-place.", null, "", "Adjustable locks"));
            forcefieldImmunityPlayers = config.Bind(nameof(forcefieldImmunityPlayers), defaultValue: true, new ConfigurableInfo("Players and SlugNPCs are unaffected by forcefields.", null, "", "Forcefield immunity players"));
            forcefieldImmunityItems = config.Bind(nameof(forcefieldImmunityItems), defaultValue: false, new ConfigurableInfo("Items (except thrown weapons) are unaffected by forcefields.", null, "", "Forcefield immunity items"));
            forcefieldRadius = config.Bind(nameof(forcefieldRadius), defaultValue: 120f, new ConfigurableInfo(null, null, "", "Forcefield radius"));
            infoDepth = config.Bind(nameof(infoDepth), defaultValue: 3, new ConfigurableInfo("Max level that the ObjectDumper can reach using the info tool.\nKeep this value low to avoid copying 'the whole game' to your clipboard.", null, "", "Info depth"));

            copyID = config.Bind(nameof(copyID), defaultValue: true, new ConfigurableInfo("Creates an exact copy of the previous object when duplicating.", null, "", "Copy ID duplicate"));
            exitGameOverMode = config.Bind(nameof(exitGameOverMode), defaultValue: true, new ConfigurableInfo("Try to exit game over mode when reviving player. Might be incompatible with some other mods.", null, "", "Exit game over mode"));
            exceptSlugNPC = config.Bind(nameof(exceptSlugNPC), defaultValue: true, new ConfigurableInfo("If checked, do not pause/destroy/kill slugpups when pausing/destroying/killing all creatures.", null, "", "Except SlugNPC"));
            tameIncreasesRep = config.Bind(nameof(tameIncreasesRep), defaultValue: false, new ConfigurableInfo("Taming creatures using this tool also increases global reputation.", null, "", "Taming global +rep"));
            controlChangesCamera = config.Bind(nameof(controlChangesCamera), defaultValue: true, new ConfigurableInfo("Safari-controlling creatures will change which creature the camera follows. Might not work well with other camera/multiplayer mods. Does not work in safari because of the overseer (unless deleted).", null, "", "Safari-control changes camera"));
            controlOnlyOne = config.Bind(nameof(controlOnlyOne), defaultValue: false, new ConfigurableInfo("Safari-controlling another creature (while already controlling a creature) will remove control from the first one, so you will only control one creature at a time.", null, "", "Safari-control only one creature"));
            controlNoInput = config.Bind(nameof(controlNoInput), defaultValue: false, new ConfigurableInfo("While safari-controlling creatures, only the creature which a camera is following will move. Unused if \"Safari-control changes camera\" is unchecked.", null, "", "Reset other safari-control input"));
            controlStunsPlayers = config.Bind(nameof(controlStunsPlayers), defaultValue: true, new ConfigurableInfo("Safari-controlling creatures will stun the (last dragged) player, as this player will now control the creature.", null, "", "Safari-control stuns players"));

            //refresh activated mods when config changes
            var onConfigChanged = typeof(OptionInterface).GetEvent("OnConfigChanged");
            onConfigChanged.AddEventHandler(this, Delegate.CreateDelegate(onConfigChanged.EventHandlerType, typeof(Integration).GetMethod("RefreshActiveMods")));
        }


        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "General 1"),
                new OpTab(this, "General 2"),
                new OpTab(this, "Tools 1"),
                new OpTab(this, "Tools 2"),
                new OpTab(this, "Tool Settings")
            };

            /**************** General ****************/
            curTab = 0;
            AddTitle();
            float x = 90f;
            float y = 460f;
            float sepr = 40f;
            AddComboBox(activateType, new Vector2(190f, 503f), Enum.GetNames(typeof(ActivateTypes)), alH: FLabelAlignment.Left, width: 120f);
            AddKeyBinder(activateKey, new Vector2(330f, 500f));

            AddCheckBox(deactivateEveryRestart, new Vector2(x, y -= sepr));
            AddCheckBox(disVnlMouseDragger, new Vector2(x, y -= sepr));
            AddCheckBox(logDebug, new Vector2(x, y -= sepr));
            AddCheckBox(dragLMB, new Vector2(x, y -= sepr));
            AddCheckBox(dragMMB, new Vector2(x, y -= sepr));
            AddKeyBinder(drag, new Vector2(x, y -= sepr + 5f));
            AddCheckBox(disVnlCursor, new Vector2(x, y -= sepr));
            AddComboBox(winCursorVisType, new Vector2(x, y -= sepr), Enum.GetNames(typeof(CursorVisibilityTypes)), alH: FLabelAlignment.Right, width: 120f);

            x += 250f;
            y = 460f;
            AddCheckBox(throwWithMouse, new Vector2(x, y -= sepr));
            AddCheckBox(throwAsPlayer, new Vector2(x, y -= sepr));
            AddTextBox(throwThreshold, new Vector2(x, y -= sepr), 40f);
            AddTextBox(throwForce, new Vector2(x, y -= sepr), 40f);
            AddKeyBinder(throwWeapon, new Vector2(x, y -= sepr + 5f));
            AddCheckBox(velocityDrag, new Vector2(x, y -= sepr));
            AddCheckBox(velocityDragAtScreenChange, new Vector2(x, y -= sepr));
            AddKeyBinder(selectCreatures, new Vector2(x, y -= sepr + 5f));
            AddKeyBinder(selectItems, new Vector2(x, y -= sepr + 5f));

            /**************** General ****************/
            curTab++;
            x = 90f;
            y = 595f;
            sepr = 40f;
            AddCheckBox(menuOpenRMB, new Vector2(x, y -= sepr));
            AddCheckBox(menuOpenMMB, new Vector2(x, y -= sepr));
            AddKeyBinder(menuOpen, new Vector2(x, y -= sepr + 5f));
            AddCheckBox(menuSelectLMB, new Vector2(x, y -= sepr));
            AddCheckBox(menuSelectMMB, new Vector2(x, y -= sepr));
            AddKeyBinder(menuSelect, new Vector2(x, y -= sepr + 5f));

            y = -19f; //from bottom up
            AddCheckBox(beastMasterIntegration, new Vector2(x, y += sepr));
            AddCheckBox(splitScreenCoopIntegration, new Vector2(x, y += sepr));

            x += 250f;
            y = 595f;
            AddCheckBox(showLabel, new Vector2(x, y -= sepr));
            AddCheckBox(showTooltips, new Vector2(x, y -= sepr));
            AddTextBox(maxOnPage, new Vector2(x, y -= sepr), 40f);
            AddCheckBox(menuFollows, new Vector2(x, y -= sepr));
            AddCheckBox(menuMoveHover, new Vector2(x, y -= sepr));

            y = -19f; //from bottom up
            AddCheckBox(sBCameraScrollIntegration, new Vector2(x, y += sepr));
            AddCheckBox(regionKitIntegration, new Vector2(x, y += sepr));
            AddCheckBox(devConsoleIntegration, new Vector2(x, y += sepr));

            /**************** Tools ****************/
            curTab++;
            x = 70f;
            y = 600f;
            sepr = 42f;
            AddKeyBinder(pauseOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPause");
            AddCheckBox(pauseOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseRoomCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseCreatures");
            AddCheckBox(pauseRoomCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(unpauseAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPlayAll");
            AddCheckBox(unpauseAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseAllCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseGlobal");
            AddCheckBox(pauseAllCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(pauseAllItemsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragPauseGlobal");
            AddCheckBox(pauseAllItemsMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(killOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragKill");
            AddCheckBox(killOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(killRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragKillCreatures");
            AddCheckBox(killRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(reviveOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragRevive");
            AddCheckBox(reviveOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(reviveRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragReviveCreatures");
            AddCheckBox(reviveRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(duplicateOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDuplicate");
            AddCheckBox(duplicateOneMenu, new Vector2(x - 56f, y + 3f));
            AddCheckBox(clipboardMenu, new Vector2(x - 56f, (y -= sepr) + 3f));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCut");
            AddIcon(new Vector2(x, y + 6f), "mousedragPaste");
            AddCheckBox(clipboardCtrlXCV, new Vector2(x + 25f + 51f, y + 3f));
            AddKeyBinder(tpCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCrosshair");
            AddCheckBox(tpWaypointBgMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tpItemsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCrosshair");
            AddCheckBox(tpWaypointCrMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(controlKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragControl");
            AddCheckBox(controlMenu, new Vector2(x - 56f, y + 3f));

            x += 300f;
            y = 600f;
            AddKeyBinder(forcefieldKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragForceFieldOn");
            AddCheckBox(forcefieldMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tameOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragHeart");
            AddCheckBox(tameOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(tameRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragHeartCreatures");
            AddCheckBox(tameRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(clearRelOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnheart");
            AddCheckBox(clearRelOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(clearRelRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnheartCreatures");
            AddCheckBox(clearRelRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStun");
            AddCheckBox(stunOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStunAll");
            AddCheckBox(stunRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(unstunAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragUnstunAll");
            AddCheckBox(unstunAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(stunAllKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragStunGlobal");
            AddCheckBox(stunAllMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyOneKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroy");
            AddCheckBox(destroyOneMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyRoomCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 9f), "mousedragDestroyCreatures");
            AddCheckBox(destroyRoomCreaturesMenu, new Vector2(x - 56f, y + 6f));
            AddIcon(new Vector2(x - 25f, y + (6f - sepr / 2f)), "mousedragDestroyItems");
            AddCheckBox(destroyRoomItemsMenu, new Vector2(x - 56f, y + (3f - sepr / 2f)));
            AddIcon(new Vector2(x - 25f, y + (3f - sepr)), "mousedragDestroyAll");
            AddCheckBox(destroyRoomObjectsMenu, new Vector2(x - 56f, y - sepr));
            AddKeyBinder(destroyRoomItemsKey, new Vector2(x, y -= sepr));
            AddKeyBinder(destroyRegionCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyGlobal");
            AddCheckBox(destroyRegionCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(destroyRegionItemsKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyGlobal");
            AddCheckBox(destroyRegionItemsMenu, new Vector2(x - 56f, y + 3f));

            /**************** Tools ****************/
            curTab++;
            x = 70f;
            y = 600f;
            sepr = 42f;
            AddKeyBinder(destroyRoomDeadCreaturesKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragDestroyDeadCreatures");
            AddCheckBox(destroyRoomDeadCreaturesMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(lockKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragLocked");
            AddCheckBox(lockMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(copySelectorKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragCLI");
            AddCheckBox(copySelectorMenu, new Vector2(x - 56f, y + 3f));

            x += 300f;
            y = 600f;
            AddKeyBinder(gravityRoomKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragGravityOff");
            AddCheckBox(gravityRoomMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(infoKey, new Vector2(x, y -= sepr));
            AddIcon(new Vector2(x - 25f, y + 6f), "mousedragInfo");
            AddCheckBox(infoMenu, new Vector2(x - 56f, y + 3f));
            AddKeyBinder(loadRegionRoomsKey, new Vector2(x, y -= sepr));

            /**************** Tool Settings ****************/
            curTab++;
            x = 90f;
            y = 595f;
            sepr = 40f;
            AddCheckBox(releaseGraspsPaused, new Vector2(x, y -= sepr));
            AddCheckBox(lineageKill, new Vector2(x, y -= sepr));
            AddCheckBox(killReleasesMask, new Vector2(x, y -= sepr));
            AddCheckBox(healLimbs, new Vector2(x, y -= sepr));
            AddCheckBox(adjustableLocks, new Vector2(x, y -= sepr));
            AddCheckBox(forcefieldImmunityPlayers, new Vector2(x, y -= sepr));
            AddCheckBox(forcefieldImmunityItems, new Vector2(x, y -= sepr));
            AddTextBox(forcefieldRadius, new Vector2(x, y -= sepr), 50f);
            AddTextBox(infoDepth, new Vector2(x, y -= sepr), 40f);

            x += 250f;
            y = 595f;
            AddCheckBox(copyID, new Vector2(x, y -= sepr));
            AddCheckBox(exitGameOverMode, new Vector2(x, y -= sepr));
            AddCheckBox(exceptSlugNPC, new Vector2(x, y -= sepr));
            AddCheckBox(tameIncreasesRep, new Vector2(x, y -= sepr));
            AddCheckBox(controlChangesCamera, new Vector2(x, y -= sepr));
            AddCheckBox(controlOnlyOne, new Vector2(x, y -= sepr));
            AddCheckBox(controlNoInput, new Vector2(x, y -= sepr));
            AddCheckBox(controlStunsPlayers, new Vector2(x, y -= sepr));
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


        private void AddCheckBox(Configurable<bool> option, Vector2 pos, Color? c = null)
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
