using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MouseDrag
{
    public static class Forcefield
    {
        public static List<BodyChunk> forcefieldChunks = new List<BodyChunk>();
        public static bool hoversOverSlot = false, showSprites = false, scaleSpritesToSize = false, customSprites = false;
        public static readonly string customSpritePath = "sprites" + Path.DirectorySeparatorChar + "mousedragForceField";


        public static void UpdateForcefield(BodyChunk bodyChunk)
        {
            if (bodyChunk?.owner == null || !forcefieldChunks.Contains(bodyChunk))
                return;

            for (int i = 0; i < bodyChunk.owner?.room?.physicalObjects?.Length; i++) {
                if (bodyChunk.owner.room.physicalObjects[i] == null)
                    continue;
                foreach (PhysicalObject po in bodyChunk.owner.room.physicalObjects[i]) {
                    if (po == bodyChunk.owner || po == null)
                        continue;
                    if (Options.forcefieldImmunityPlayers?.Value != false && po is Player)
                        continue;
                    if (Options.forcefieldImmunityItems?.Value != false) {
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
                    po.PushOutOf(bodyChunk.pos, Options.forcefieldRadius?.Value ?? 120f, -1);
                }
            }
        }


        public static bool HasForcefield(BodyChunk bodyChunk)
        {
            if (bodyChunk == null)
                return false;
            return forcefieldChunks.Contains(bodyChunk);
        }


        public static void SetForcefield(BodyChunk bodyChunk, bool toggle, bool apply)
        {
            if (bodyChunk?.owner == null)
                return;

            bool contains = forcefieldChunks.Contains(bodyChunk);
            if (contains)
                if (toggle || (!toggle && !apply))
                    forcefieldChunks.Remove(bodyChunk);
            if (!contains) {
                if (toggle || (!toggle && apply)) {
                    forcefieldChunks.Add(bodyChunk);
                    ForcefieldSprite ffs = new ForcefieldSprite(bodyChunk);
                    bodyChunk.owner.room?.AddObject(ffs);
                }
            }
        }


        public static void ClearForcefields()
        {
            forcefieldChunks.Clear();
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ClearForcefields");
        }


        public class ForcefieldSprite : UpdatableAndDeletable, IDrawable
        {
            //NOTE: this object requires that the previous room of the bodychunk stays loaded after the followed bodychunk 
            //moved to a new room, so this object is able to move itself to the new room when this object is updated


            public Vector2 curPos, prevPos;
            public BodyChunk followChunk;
            private bool visible;


            public ForcefieldSprite(BodyChunk bc)
            {
                room = bc?.owner?.room;
                prevPos = bc?.pos ?? new Vector2();
                curPos = bc?.pos ?? new Vector2();
                followChunk = bc;
            }
            ~ForcefieldSprite() { Destroy(); }


            public override void Update(bool eu)
            {
                base.Update(eu);
                if (!HasForcefield(followChunk) || followChunk?.owner?.slatedForDeletetion != false) {
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
                visible = (showSprites || hoversOverSlot) 
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
                    float rad = (Options.forcefieldRadius?.Value ?? 120f) / ((sLeaser.sprites[0].localRect.width + sLeaser.sprites[0].localRect.height) / 4f);
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
                        Plugin.Logger.LogError("Forcefield.ForcefieldSprite.DrawSprites exception while reading SBCameraScroll, integration is now disabled");
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
                if (newContainer == null)
                    newContainer = rCam.ReturnFContainer("HUD");
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
                //Plugin.Logger.LogError("Forcefield.LoadSprites exception: " + ex?.ToString());
                //if loading failed, no custom sprite is available
                return;
            }
            scaleSpritesToSize = true;
            customSprites = true;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Forcefield.LoadSprites loaded a custom sprite");
        }


        internal static void UnloadSprites()
        {
            if (!Futile.atlasManager.DoesContainElementWithName(customSpritePath))
                return;
            try {
                Futile.atlasManager.UnloadImage(customSpritePath);
            } catch (Exception ex) {
                Plugin.Logger.LogError("Forcefield.UnloadSprites exception: " + ex?.ToString());
                return;
            }
            scaleSpritesToSize = false;
            customSprites = false;
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("Forcefield.UnloadSprites unloaded a custom sprite");
        }
    }
}
