using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MouseDrag
{
    public static class ForceField
    {
        public static List<BodyChunk> forceFieldChunks = new List<BodyChunk>();
        public static bool hoversOverSlot = false, showSprites = false, radMSelectsAForceField = false;
        public static bool spritesBehindCreatures = false, scaleSpritesToSize = false, customSprites = false;
        public static readonly string customSpritePath = "sprites" + Path.DirectorySeparatorChar + "mousedragForceField";


        public static void UpdateForceField(BodyChunk bodyChunk)
        {
            if (bodyChunk?.owner == null || !forceFieldChunks.Contains(bodyChunk))
                return;

            for (int i = 0; i < bodyChunk.owner?.room?.physicalObjects?.Length; i++) {
                if (bodyChunk.owner.room.physicalObjects[i] == null)
                    continue;
                foreach (PhysicalObject po in bodyChunk.owner.room.physicalObjects[i]) {
                    if (po == bodyChunk.owner || po == null)
                        continue;
                    if (Options.forceFieldImmunityPlayers?.Value != false && po is Player)
                        continue;
                    if (Options.forceFieldImmunityItems?.Value != false) {
                        if (po is Weapon) {
                            //not immune when ignited
                            if (po is ScavengerBomb && (po as ScavengerBomb).ignited) {
                            } else if (po is ExplosiveSpear && (po as ExplosiveSpear).Ignited) {
                            } else if (po is FirecrackerPlant && (po as FirecrackerPlant).fuseCounter > 0) {
                            } else if (po is MoreSlugcats.SingularityBomb && (po as MoreSlugcats.SingularityBomb).ignited) {
                            } else if (po is MoreSlugcats.FireEgg && (po as MoreSlugcats.FireEgg).activeCounter > 0) {

                            //free weapons are immune
                            } else if ((po as Weapon).mode != Weapon.Mode.Thrown) {
                                continue;
                            }

                            //weapons thrown by creature who threw them are immune
                            if ((po as Weapon).thrownBy == bodyChunk.owner)
                                continue;

                        } else if (!(po is Creature)) {
                            //any non-creature object is immune
                            continue;
                        }

                        //creature that grabs this object is immune (more fun when disabled)
                        //for (int j = 0; j < bodyChunk.owner.grabbedBy?.Count; j++)
                        //    if (bodyChunk.owner.grabbedBy[j]?.grabbed == po)
                        //        continue;
                    }
                    po.PushOutOf(bodyChunk.pos, Options.forceFieldRadius?.Value ?? 120f, -1);
                }
            }
        }


        public static bool HasForceField(BodyChunk bodyChunk)
        {
            if (bodyChunk == null)
                return false;
            return forceFieldChunks.Contains(bodyChunk);
        }


        public static void SetForceField(BodyChunk bodyChunk, bool toggle, bool apply)
        {
            if (bodyChunk?.owner == null)
                return;

            bool contains = forceFieldChunks.Contains(bodyChunk);
            if (contains)
                if (toggle || (!toggle && !apply))
                    forceFieldChunks.Remove(bodyChunk);
            if (!contains) {
                if (toggle || (!toggle && apply)) {
                    forceFieldChunks.Add(bodyChunk);
                    ForceFieldSprite ffs = new ForceFieldSprite(bodyChunk);
                    bodyChunk.owner.room?.AddObject(ffs);
                }
            }
        }


        public static void ClearForceFields()
        {
            forceFieldChunks.Clear();
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ClearForceFields");
        }


        public class ForceFieldSprite : UpdatableAndDeletable, IDrawable
        {
            //NOTE: this object requires that the previous room of the bodychunk stays loaded after the followed bodychunk 
            //moved to a new room, so this object is able to move itself to the new room when this object is updated


            public Vector2 curPos, prevPos;
            public BodyChunk followChunk;
            private bool visible;


            public ForceFieldSprite(BodyChunk bc)
            {
                room = bc?.owner?.room;
                prevPos = bc?.pos ?? new Vector2();
                curPos = bc?.pos ?? new Vector2();
                followChunk = bc;
            }
            ~ForceFieldSprite() { Destroy(); }


            public override void Update(bool eu)
            {
                base.Update(eu);
                if (!HasForceField(followChunk) || followChunk?.owner?.slatedForDeletetion != false) {
                    followChunk = null;
                    Destroy();
                    return;
                }
                if (followChunk.owner?.room != null && followChunk.owner.room != room) {
                    RemoveFromRoom();
                    room = followChunk.owner.room;
                    room.AddObject(this);
                    curPos = followChunk.pos; //prevent sprite shooting across screen
                }
                radMSelectsAForceField &= MenuManager.menu?.followChunk != null;
                visible = (showSprites || hoversOverSlot || radMSelectsAForceField) 
                    && followChunk.owner?.room != null 
                    && Drag.MouseCamera(room?.game)?.room == room;
                prevPos = curPos;
                if (!Drag.ShouldRelease(followChunk?.owner))
                    curPos = followChunk.pos;
            }


            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.RemoveAllSpritesFromContainer();
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite(customSprites ? customSpritePath : "mousedragForceFieldOn", true);
                if (scaleSpritesToSize) {
                    float rad = (Options.forceFieldRadius?.Value ?? 120f) / ((sLeaser.sprites[0].localRect.width + sLeaser.sprites[0].localRect.height) / 4f);
                    sLeaser.sprites[0].ScaleAroundPointRelative(Vector2.zero, rad, rad);
                }
                this.AddToContainer(sLeaser, rCam, null);
            }


            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                if (slatedForDeletetion) {
                    sLeaser?.CleanSpritesAndRemove();
                    return;
                }
                Vector2 tsPos = Vector2.Lerp(prevPos, curPos, timeStacker) - camPos;

                if (Integration.sBCameraScrollEnabled) {
                    try {
                        tsPos -= Integration.SBCameraScrollExtraOffset(rCam, tsPos, out float scale) / (1f / scale);
                    } catch {
                        Plugin.Logger.LogError("ForceField.ForceFieldSprite.DrawSprites exception while reading SBCameraScroll, integration is now disabled");
                        Integration.sBCameraScrollEnabled = false;
                        throw; //throw original exception while preserving stack trace
                    }
                }

                sLeaser.sprites[0].x = tsPos.x;
                sLeaser.sprites[0].y = tsPos.y;
                sLeaser.sprites[0].isVisible = visible;
            }


            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
            {
                if (newContainer == null) {
                    if (spritesBehindCreatures) {
                        newContainer = rCam.ReturnFContainer("Background");
                    } else {
                        newContainer = rCam.ReturnFContainer("HUD");
                    }
                }
                newContainer.AddChild(sLeaser.sprites[0]);
            }


            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
            }
        }


        internal static void LoadSprites()
        {
            try {
                Futile.atlasManager.LoadImage(customSpritePath);
            } catch (Exception ex) {
                //Plugin.Logger.LogError("ForceField.LoadSprites exception: " + ex?.ToString());
                //if loading failed, no custom sprite is available
                return;
            }
            scaleSpritesToSize = true;
            customSprites = true;
            showSprites = true; //constantly show forcefield sprites for players messing around
            spritesBehindCreatures = true;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ForceField.LoadSprites loaded a custom sprite");
        }


        internal static void UnloadSprites()
        {
            if (!Futile.atlasManager.DoesContainElementWithName(customSpritePath))
                return;
            try {
                Futile.atlasManager.UnloadImage(customSpritePath);
            } catch (Exception ex) {
                Plugin.Logger.LogError("ForceField.UnloadSprites exception: " + ex?.ToString());
                return;
            }
            scaleSpritesToSize = false;
            customSprites = false;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ForceField.UnloadSprites unloaded a custom sprite");
        }
    }
}
