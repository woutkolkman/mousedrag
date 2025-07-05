using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MouseDrag
{
    public static class MenuManager
    {
        //================================================== Backwards Compatibility ==================================================
        [Obsolete("SubMenuTypes are deprecated, please use GetSubMenuID() instead.")]
        public static SubMenuTypes subMenuType; //do not use in new code or add-ons
        [Obsolete("SubMenuTypes are deprecated, please use GetSubMenuID() instead.")]
        public enum SubMenuTypes { None, Any } //do not use in new code or add-ons
        //=============================================================================================================================


        internal static RadialMenu menu = null;
        private static bool shouldOpen = false; //signal from RawUpdate to open menu
        private static bool prevFollowsObject = false; //detect if slots must be reloaded
        public static bool reloadSlots = false;
        public static string lowPrioText, highPrioText;
        private static string labelText = string.Empty, prevLabelText = string.Empty; //detect if label text has changed
        private static Dictionary<string, int> pageIndexes = new Dictionary<string, int>(); //for every subMenuID, keeps track of current page index
        public static List<RadialMenu.Slot> registeredSlots = new List<RadialMenu.Slot>(); //list of all registered slots in any (sub)menu that can be conditionally added to the menu


        private static string subMenuID = string.Empty;
        public static string GetSubMenuID() => subMenuID;
        public static bool InRootMenu() => string.IsNullOrEmpty(subMenuID);
        public static void GoToRootMenu() => subMenuID = string.Empty;
        public static bool InSubMenu(string id) => subMenuID == id;
        public static void SetSubMenuID(string id)
        {
            if (id == null)
                id = string.Empty;
            subMenuID = id;
#pragma warning disable CS0618 // Type or member is obsolete
            subMenuType = id == string.Empty ? SubMenuTypes.None : SubMenuTypes.Any;
#pragma warning restore CS0618 // Type or member is obsolete
        }


        internal static void Update(RainWorldGame game)
        {
            if (shouldOpen && menu == null && State.activated) {
                menu = new RadialMenu(game);
                reloadSlots = true;
                GoToRootMenu();
                lowPrioText = null;
                highPrioText = null;
            }
            shouldOpen = false;

            if (menu?.closed == true || !State.activated) {
                menu?.Destroy();
                menu = null;
            }

            //menu closed
            if (menu == null) {
                ClearPageIndexes();
                return;
            }

            //followchunk changed
            if (menu.prevFollowChunk != menu.followChunk) {
                reloadSlots = true;
                GoToRootMenu();
                lowPrioText = null;
                highPrioText = null;

                //menu switched from bodychunk to background or vice versa
                if (menu.prevFollowChunk == null || menu.followChunk == null)
                    ClearPageIndexes();
            }

            RadialMenu.Slot slot = menu.Update(game, out RadialMenu.Slot hoverSlot);

            //switch slots
            bool followsObject = menu.followChunk != null;
            if (followsObject ^ prevFollowsObject || reloadSlots) {
                var slots = ReloadSlots(game, menu, menu.followChunk);
                CreatePage(ref slots);
                if (State.menuToolsDisabled)
                    DisableMenu(ref slots);
                menu.LoadSlots(slots);
            }
            prevFollowsObject = followsObject;
            reloadSlots = false;

            if (slot != null && slot.actionEnabled) {
                lowPrioText = null;
                highPrioText = null;

                //run command if a menu slot was pressed
                RunAction(game, slot, menu.followChunk);

                //reload slots if command is executed
                reloadSlots = true;
            }

            //change label color
            if (menu.label != null)
                menu.label.color = hoverSlot?.tooltipColor ?? Color.white;

            //change label text
            bool translate = false;
            if (highPrioText?.Length > 0) {
                labelText = highPrioText;
                translate = true;
            } else if (!string.IsNullOrEmpty(hoverSlot?.tooltip) && Options.showTooltips?.Value != false) {
                labelText = hoverSlot.tooltip;
                translate = !hoverSlot.skipTranslateTooltip;
            } else if (lowPrioText?.Length > 0) {
                labelText = lowPrioText;
                translate = true;
            } else if (menu.followChunk?.owner != null) {
                labelText = menu.followChunk.owner.ToString(); //fallback if abstractPhysicalObject is somehow null (impossible?)
                string text = Special.ConsistentName(menu.followChunk.owner.abstractPhysicalObject);
                if (!string.IsNullOrEmpty(text))
                    labelText = text;
            } else if (!string.IsNullOrEmpty(menu.roomName)) {
                labelText = menu.roomName;
            } else {
                labelText = "";
            }
            bool labelTextChanged = labelText != prevLabelText || String.IsNullOrEmpty(menu.labelText);
            prevLabelText = labelText;

            //live translate label text
            if (labelTextChanged) { //not translating every tick saves performance
                if (translate && !String.IsNullOrEmpty(labelText) && game?.rainWorld?.inGameTranslator != null) {
                    menu.labelText = game.rainWorld.inGameTranslator.Translate(labelText.Replace("\n", "<LINE>")).Replace("<LINE>", "\n");
                } else {
                    menu.labelText = labelText;
                }
            }
        }


        //legacy add-on mods need to hook the RunAction() function, and do an action when their slot was pressed
        public static void RunAction(RainWorldGame game, RadialMenu.Slot slot, BodyChunk chunk)
        {
            if (slot?.name == null) {
                if (Options.logDebug?.Value != false)
                    Plugin.Logger.LogDebug("MenuManager.RunAction, slot null or invalid name");
                return;
            }

            //next page
            if (slot.name == "+") {
                SetPageIndex(GetPageIndex() + 1);
                return;
            }

            //multi-select, get list of objects to edit
            List<BodyChunk> chunks = new List<BodyChunk>();
            List<PhysicalObject> objects = new List<PhysicalObject>();
            if (chunk?.owner != null) {
                if (Select.selectedChunks.Contains(chunk)) {
                    chunks = Select.selectedChunks;
                    objects = Select.selectedObjects;
                } else {
                    chunks.Add(chunk);
                    objects.Add(chunk.owner);
                }
            }

            try {
                if (chunks.Count > 0) { //menu follows object
                    if (slot.actionOnASingleObject) {
                        slot.actionBC?.Invoke(game, slot, chunk);
                        slot.actionPO?.Invoke(game, slot, chunk?.owner);
                    } else {
                        foreach (BodyChunk bc in chunks)
                            slot.actionBC?.Invoke(game, slot, bc);
                        foreach (PhysicalObject po in objects)
                            slot.actionPO?.Invoke(game, slot, po);
                    }
                } else { //menu on background
                    slot.actionBC?.Invoke(game, slot, null);
                    slot.actionPO?.Invoke(game, slot, null);
                }
                slot.finalize?.Invoke(game, slot);
            } catch (Exception ex) {
                Plugin.Logger.LogError("MenuManager.RunAction exception: " + ex?.ToString());
                slot.noneBgColor = new Color(255f, 0f, 0f, 0.2f);
                highPrioText = ex?.GetType()?.Name;
            }
        }


        //legacy add-on mods need to hook the ReloadSlots() function, and insert their slots afterwards
        public static List<RadialMenu.Slot> ReloadSlots(RainWorldGame game, RadialMenu menu, BodyChunk chunk)
        {
            List<RadialMenu.Slot> slots = registeredSlots.Where(s => s.subMenuID == subMenuID).ToList();
            slots = slots.Where(s => s.requiresBodyChunk == null || s.requiresBodyChunk == (chunk != null)).ToList();

            foreach (RadialMenu.Slot slot in slots) {
                try {
                    slot.reload?.Invoke(game, slot, chunk);
                } catch (Exception ex) {
                    Plugin.Logger.LogError("MenuManager.ReloadSlots exception: " + ex?.ToString());
                    slot.noneBgColor = new Color(255f, 0f, 0f, 0.2f);
                    string exceptionName = ex?.GetType()?.Name;
                    if (!string.IsNullOrEmpty(exceptionName)) {
                        if (string.IsNullOrEmpty(slot.tooltip)) {
                            slot.tooltip = exceptionName;
                        } else if (!slot.tooltip.Contains(exceptionName)) {
                            slot.tooltip += " | " + exceptionName;
                        }
                    }
                }
            }

            slots.RemoveAll(s => s.hideInMenu);
            return slots;
        }


        internal static int GetPageIndex(string id = null)
        {
            if (id == null)
                id = GetSubMenuID();
            if (pageIndexes.TryGetValue(id, out int index))
                return index;
            pageIndexes.Add(id, 0);
            return 0;
        }


        internal static void SetPageIndex(int value, string id = null)
        {
            if (id == null)
                id = GetSubMenuID();
            if (pageIndexes.ContainsKey(id)) {
                pageIndexes[id] = value;
            } else {
                pageIndexes.Add(id, value);
            }
        }


        internal static void ClearPageIndexes()
        {
            pageIndexes.Clear();
        }


        internal static void CreatePage(ref List<RadialMenu.Slot> slots)
        {
            int maxOnPage = Options.maxOnPage?.Value ?? 7;
            int count = slots.Count;
            int page = GetPageIndex();

            //go to last page if negative
            if (page < 0)
                page = (count - 1) / maxOnPage;

            //reset page if out of bounds
            if (count <= maxOnPage * page)
                page = 0;

            SetPageIndex(page);

            //no page slot is required
            if (count <= maxOnPage)
                return;

            for (int i = count - 1; i >= 0; i--) {
                if (i < (maxOnPage * page) + maxOnPage && 
                    i >= maxOnPage * page)
                    continue;
                slots.RemoveAt(i);
            }
            string tooltip = RWCustom.Custom.rainWorld?.inGameTranslator?.Translate("Next page") ?? "Next page";
            tooltip += " (" + (page + 1) + "/" + (((count - 1) / maxOnPage) + 1) + ")";
            slots.Add(new RadialMenu.Slot() {
                name = "+",
                tooltip = tooltip,
                isLabel = true,
                skipTranslateTooltip = true
            });
        }


        internal static void DisableMenu(ref List<RadialMenu.Slot> slots)
        {
            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].name == "+") //for pages to work
                    continue;
                slots[i].curIconColor = Color.grey;
                slots[i].tooltip = "Disabled";
                slots[i].actionEnabled = false;
                slots[i].skipTranslateTooltip = false;
            }
        }


        internal static void RawUpdate(RainWorldGame game)
        {
            menu?.RawUpdate(game);

            //if editing in sandbox, disable open menu with right mouse button
            bool inSandboxAndEditing = (game.GetArenaGameSession as SandboxGameSession)?.overlay?.playMode == false;

            //if BeastMaster is enabled and opened, don't use right mouse button
            bool beastMasterOpened = false;
            if (Integration.beastMasterEnabled) {
                try {
                    beastMasterOpened = Integration.BeastMasterUsesRMB(game);
                } catch {
                    Plugin.Logger.LogError("MenuManager.RawUpdate exception while reading BeastMaster state, integration is now disabled");
                    Integration.beastMasterEnabled = false;
                    throw; //throw original exception while preserving stack trace
                }
            }

            //if regionkit is enabled and dev tools menu is opened, don't use right mouse button (because of Iggy)
            bool devToolsOpened = Integration.regionKitEnabled && game.devUI != null;

            if (RadialMenu.menuOpenButtonPressed(noRMB: inSandboxAndEditing || beastMasterOpened || devToolsOpened)) {
                shouldOpen = true;
            } else if (beastMasterOpened && RadialMenu.menuOpenButtonPressed()) {
                menu?.Destroy();
                menu = null;
            }

            //also use scroll wheel to navigate pages
            if (UnityEngine.Input.mouseScrollDelta.y < 0 && menu?.mouseIsOnMenuBG == true) {
                SetPageIndex(GetPageIndex() + 1);
                reloadSlots = true;
            }
            if (UnityEngine.Input.mouseScrollDelta.y > 0 && menu?.mouseIsOnMenuBG == true) {
                SetPageIndex(GetPageIndex() - 1);
                reloadSlots = true;
            }
        }


        internal static void DrawSprites(float timeStacker)
        {
            menu?.DrawSprites(timeStacker);
        }


        internal static void LoadSprites()
        {
            try {
                Futile.atlasManager.LoadAtlas("sprites" + Path.DirectorySeparatorChar + "mousedrag");
            } catch (Exception ex) {
                Plugin.Logger.LogError("MenuManager.LoadSprites exception: " + ex?.ToString());
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("LoadSprites called");
        }


        internal static void UnloadSprites()
        {
            try {
                Futile.atlasManager.UnloadAtlas("sprites" + Path.DirectorySeparatorChar + "mousedrag");
            } catch (Exception ex) {
                Plugin.Logger.LogError("MenuManager.UnloadSprites exception: " + ex?.ToString());
            }
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("UnloadSprites called");
        }
    }
}
