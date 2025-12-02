using UnityEngine;

namespace MouseDrag
{
    public static class Health
    {
        public static void KillCreature(RainWorldGame game, PhysicalObject obj)
        {
            if (!(obj is Creature))
                return;

            //lineage kill option
            if (Options.lineageKill?.Value == true && game?.FirstAlivePlayer != null)
                (obj as Creature).SetKillTag(game.FirstAlivePlayer);

            (obj as Creature).Die();
            if ((obj as Creature).abstractCreature?.state is HealthState)
                ((obj as Creature).abstractCreature.state as HealthState).health = 0f;

            //drop mask option
            if (Options.killReleasesMask?.Value == true) {
                if (obj is Scavenger && (obj as Scavenger).Elite)
                    (obj as Scavenger).Violence(obj.firstChunk, null, obj.firstChunk, null, Creature.DamageType.Stab, 0f, 0f);
                (obj as Vulture)?.DropMask(new Vector2());
            }
        }


        //kill all creatures in room
        public static void KillCreatures(RainWorldGame game, Room room)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("KillCreatures");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i]?.Count; j++)
                    if (!(room.physicalObjects[i][j] is Player && //don't kill when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        KillCreature(game, room.physicalObjects[i][j]);
        }


        public static void ReviveCreature(PhysicalObject obj)
        {
            if (!(obj is Creature rc))
                return;

            AbstractCreature ac = rc.abstractCreature;
            if (ac?.state == null)
                return;

            if (ac.state is HealthState hs && hs.health < 1f)
                hs.health = 1f;
            ac.state.alive = true;
            rc.dead = false;
            rc.stun = 0; //makes player immediately controllable
            rc.Hypothermia = 0f;
            rc.HypothermiaExposure = 0f;
            rc.injectedPoison = 0f;
            rc.leechedOut = false;

            //heal limbs/tentacles/wings
            if (Options.healLimbs?.Value != false) {
                if (ac.state is LizardState) {
                    for (int i = 0; i < (ac.state as LizardState).limbHealth?.Length; i++)
                        if ((ac.state as LizardState).limbHealth[i] < 1f)
                            (ac.state as LizardState).limbHealth[i] = 1f;
                    if ((ac.state as LizardState).throatHealth < 1f)
                        (ac.state as LizardState).throatHealth = 1f;
                }
                if (ac.state is Vulture.VultureState)
                    for (int i = 0; i < (ac.state as Vulture.VultureState).wingHealth?.Length; i++)
                        if ((ac.state as Vulture.VultureState).wingHealth[i] < 1f)
                            (ac.state as Vulture.VultureState).wingHealth[i] = 1f;
                if (ac.state is DaddyLongLegs.DaddyState)
                    for (int i = 0; i < (ac.state as DaddyLongLegs.DaddyState).tentacleHealth?.Length; i++)
                        if ((ac.state as DaddyLongLegs.DaddyState).tentacleHealth[i] < 1f)
                            (ac.state as DaddyLongLegs.DaddyState).tentacleHealth[i] = 1f;
                if (ac.state is Centipede.CentipedeState)
                    for (int i = 0; i < (ac.state as Centipede.CentipedeState).shells?.Length; i++)
                        (ac.state as Centipede.CentipedeState).shells[i] = true;
                if (ac.state is MoreSlugcats.Inspector.InspectorState)
                    for (int i = 0; i < (ac.state as MoreSlugcats.Inspector.InspectorState).headHealth?.Length; i++)
                        if ((ac.state as MoreSlugcats.Inspector.InspectorState).headHealth[i] < 3f)
                            (ac.state as MoreSlugcats.Inspector.InspectorState).headHealth[i] = 3f; //special, not 1f
            }

            //reset destination so creature does not start running immediately
            (obj.abstractPhysicalObject as AbstractCreature)?.abstractAI?.SetDestination(obj.abstractPhysicalObject.pos);

            if (obj is Hazer hz) {
                hz.inkLeft = 1f;
                hz.hasSprayed = false;
                hz.clds = 0;
            }

            if ((obj as MoreSlugcats.StowawayBug)?.AI is MoreSlugcats.StowawayBugAI) {
                (obj as MoreSlugcats.StowawayBug).AI.activeThisCycle = true;
                (obj as MoreSlugcats.StowawayBug).AI.behavior = MoreSlugcats.StowawayBugAI.Behavior.Idle;
            }

            if (obj is Player p) {
                //try to exit game over mode
                if (Options.exitGameOverMode?.Value != false && !p.isNPC) {
                    //campaign
                    for (int i = 0; i < obj.room?.game?.cameras?.Length; i++)
                        if (obj.room.game.cameras[i]?.hud?.textPrompt != null)
                            obj.room.game.cameras[i].hud.textPrompt.gameOverMode = false;

                    //sandbox & challenges
                    if (obj.room?.game?.arenaOverlay != null) {
                        obj.room.game.arenaOverlay.ShutDownProcess();
                        obj.room.game.manager?.sideProcesses?.Remove(obj.room.game.arenaOverlay);
                        obj.room.game.arenaOverlay = null;
                        if (obj.room.game.session is ArenaGameSession ags) {
                            ags.sessionEnded = false;
                            ags.challengeCompleted = false;
                            ags.endSessionCounter = -1;
                        }
                    }
                }

                p.exhausted = false;
                p.lungsExhausted = false;
                p.airInLungs = 1f;
                p.aerobicLevel = 0f;
                if (p.playerState != null) {
                    p.playerState.permaDead = false;
                    p.playerState.permanentDamageTracking = 0.0; //prevents sickness/waterdrips from revived pups
                }
                p.animation = Player.AnimationIndex.None; //prevents lie-down slide
            }
        }


        //revive all creatures in room
        public static void ReviveCreatures(Room room)
        {
            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ReviveCreatures");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i]?.Count; j++)
                    ReviveCreature(room.physicalObjects[i][j]);
        }


        public static void TriggerItem(PhysicalObject obj)
        {
            if (obj is Creature || obj == null)
                return;

            if (obj is SeedCob)
                (obj as SeedCob).Open();

            if (obj is SporePlant)
                (obj as SporePlant).BeeTrigger();

            if (obj is DangleFruit)
                (obj as DangleFruit).bites = 1;

            if (obj is KarmaFlower)
                (obj as KarmaFlower).bites = 1;

            if (obj is JellyFish) {
                (obj as JellyFish).Tossed(null);
                (obj as JellyFish).bites = 1;
            }

            if (obj is SlimeMold)
                (obj as SlimeMold).bites = 1;

            if (obj is WaterNut)
                (obj as WaterNut).Swell();

            if (obj is BubbleGrass && (obj as BubbleGrass).AbstrBubbleGrass != null)
                (obj as BubbleGrass).AbstrBubbleGrass.oxygenLeft = 0f;

            if (obj is JokeRifle && obj.grabbedBy?.Count > 0)
                (obj as JokeRifle).Use(false);
            //TODO?, JokeRifle firing without grabber generates exceptions (and you cannot aim, and you have infinite ammo)

            if (obj is MoreSlugcats.ElectricSpear)
                (obj as MoreSlugcats.ElectricSpear).Zap();

            if (obj is MoreSlugcats.GooieDuck)
                (obj as MoreSlugcats.GooieDuck).bites = 1;

            if (obj is MoreSlugcats.GlowWeed)
                (obj as MoreSlugcats.GlowWeed).bites = 1;

            if (obj is MoreSlugcats.EnergyCell) {
                if ((obj as MoreSlugcats.EnergyCell).usingTime > 0f)
                    (obj as MoreSlugcats.EnergyCell).Explode();
                (obj as MoreSlugcats.EnergyCell).Use(true);
            }

            if (obj is MoreSlugcats.LillyPuck && (obj as MoreSlugcats.LillyPuck).AbstrLillyPuck != null)
                (obj as MoreSlugcats.LillyPuck).AbstrLillyPuck.bites = 1;

            if (obj is Pomegranate)
                (obj as Pomegranate).EnterSmashedMode();

            if (obj is Watcher.UrbanToys.SpinToy) {
                (obj as Watcher.UrbanToys.SpinToy).ChargeSpin(
                    Random.Range((obj as Watcher.UrbanToys.SpinToy).minSpinTime, (obj as Watcher.UrbanToys.SpinToy).maxSpinTime)
                );
            }

            if (obj is Oracle)
            {
                if (obj.room?.game?.GetStorySession?.saveState?.deathPersistentSaveData != null) {
                    if ((obj as Oracle).ID == Oracle.OracleID.SS || 
                        (obj as Oracle).ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.CL)
                        obj.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles = true;
                    if ((obj as Oracle).ID == Oracle.OracleID.SL)
                        obj.room.game.GetStorySession.saveState.deathPersistentSaveData.ripMoon = true;
                }
                if ((obj as Oracle).ID != MoreSlugcats.MoreSlugcatsEnums.OracleID.ST)
                    (obj as Oracle).health = 0f;

                //remember kids, cheating is bad
                //advance phase challenge #70
                if ((obj as Oracle).ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.ST && 
                    (obj as Oracle).oracleBehavior is MoreSlugcats.STOracleBehavior)
                    ((obj as Oracle).oracleBehavior as MoreSlugcats.STOracleBehavior).AdvancePhase();
            }

            //============== single use ==============

            if (obj is Weapon) {
                (obj as Weapon).HitWall();
                (obj as Weapon).HitByWeapon(obj as Weapon);
            }

            if (obj is ScavengerBomb)
                (obj as ScavengerBomb).InitiateBurn();

            if (obj is ExplosiveSpear)
                (obj as ExplosiveSpear).Ignite();

            if (obj is FirecrackerPlant)
                (obj as FirecrackerPlant).Ignite();

            if (obj is MoreSlugcats.SingularityBomb)
                (obj as MoreSlugcats.SingularityBomb).ignited = true;

            if (obj is MoreSlugcats.FireEgg)
                (obj as MoreSlugcats.FireEgg).activeCounter = 1;
        }


        public static void ResetItem(PhysicalObject obj)
        {
            if (obj is Creature || obj == null)
                return;

            if (obj is SeedCob sc && sc.AbstractCob != null) {
                sc.open = 0f;
                sc.canBeHitByWeapons = true;
                sc.freezingCounter = 0f;
                sc.seedPopCounter = -1;
                sc.seedsPopped = new bool[sc.seedPositions.Length];
                sc.AbstractCob.dead = false;
                sc.AbstractCob.opened = false;
                sc.AbstractCob.spawnedUtility = false;

                //makes seedcob return next cycle
                sc.AbstractCob.isConsumed = false;
                if (sc.AbstractCob.world?.game?.session is StoryGameSession) {
                    (sc.AbstractCob.world.game.session as StoryGameSession).saveState?.ReportConsumedItem(
                        sc.AbstractCob.world,
                        false,
                        sc.AbstractCob.originRoom,
                        sc.AbstractCob.placedObjectIndex,
                        0
                    );
                }
                //NOTE: might have effect on this seedcob in the future, because it technically cannot be consumed twice in same cycle
            }
            //TODO?, reset consumed state of other types (food)? makes items re-appear next cycle

            if (obj is SporePlant)
                (obj as SporePlant).Used = false;

            if (obj is DangleFruit)
                (obj as DangleFruit).bites = 3;

            if (obj is KarmaFlower)
                (obj as KarmaFlower).bites = 4;

            if (obj is JellyFish)
                (obj as JellyFish).bites = 3;

            if (obj is SlimeMold)
                (obj as SlimeMold).bites = 3;

            if (obj is SwollenWaterNut) {
                WaterNut wn = new WaterNut(obj.abstractPhysicalObject);
                wn.AbstrNut.swollen = false;
                obj.room?.AddObject(wn);
                wn.firstChunk.HardSetPosition(obj.firstChunk.pos);
                obj.Destroy();
            }

            if (obj is BubbleGrass && (obj as BubbleGrass).AbstrBubbleGrass != null)
                (obj as BubbleGrass).AbstrBubbleGrass.oxygenLeft = 1f;

            if (obj is JokeRifle)
                (obj as JokeRifle).ReloadRifle(new Rock(null, obj.room?.world));

            if (obj is MoreSlugcats.ElectricSpear)
                (obj as MoreSlugcats.ElectricSpear).Recharge();

            if (obj is MoreSlugcats.GooieDuck)
                (obj as MoreSlugcats.GooieDuck).bites = 6;

            if (obj is MoreSlugcats.GlowWeed)
                (obj as MoreSlugcats.GlowWeed).bites = 3;

            if (obj is MoreSlugcats.EnergyCell) {
                if ((obj as MoreSlugcats.EnergyCell).usingTime > 0f) {
                    (obj as MoreSlugcats.EnergyCell).usingTime = 1f;
                } else {
                    (obj as MoreSlugcats.EnergyCell).recharging = 1f;
                }
            }

            if (obj is MoreSlugcats.LillyPuck && (obj as MoreSlugcats.LillyPuck).AbstrLillyPuck != null)
                (obj as MoreSlugcats.LillyPuck).AbstrLillyPuck.bites = 3;

            if (obj is Pomegranate pg) {
                pg.refreshSprites = true;
                pg.smashed = false;
//                pg.disconnected = false;
//                pg.spearmasterStabbed = false;
                if (pg.AbstrPomegranate != null) {
                    pg.AbstrPomegranate.smashed = false;
//                    pg.AbstrPomegranate.disconnected = false;
//                    pg.AbstrPomegranate.spearmasterStabbed = false;
                }
                //TODO reset ReportConsumedItem
            }

            if (obj is Watcher.UrbanToys.SpinToy st) {
                st.randomOffset = 0f;
                st.backForth = 0f;
                st.leftRight = 0f;
                st.unstable = 0f;
                st.spin = 0f;
                st.topBottom = 0f;
                st.spinTimer?.SetToMin();
            }

            if (obj is Oracle o)
            {
                if (obj.room?.game?.GetStorySession?.saveState?.deathPersistentSaveData != null) {
                    if (o.ID == Oracle.OracleID.SS ||
                        o.ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.CL)
                        obj.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles = false;
                    if (o.ID == Oracle.OracleID.SL) {
                        obj.room.game.GetStorySession.saveState.deathPersistentSaveData.ripMoon = false;
                        if (obj.room.game.GetStorySession.saveState.miscWorldSaveData?.SLOracleState?.neuronsLeft <= 0)
                            obj.room.game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft++;
                    }
                }
                if (o.ID != MoreSlugcats.MoreSlugcatsEnums.OracleID.ST) {
                    o.health = 1f;
                    o.stun = 0;
                }
                if (o.ID == Oracle.OracleID.SL &&
                    o.mySwarmers?.Count <= 0)
                    o.SetUpSwarmers();

                //reset challenge #70
                /*if (o.ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.ST && 
                    o.oracleBehavior is MoreSlugcats.STOracleBehavior stob) {
                    stob.curPhase = MoreSlugcats.STOracleBehavior.Phase.Inactive;
                }*/ //TODO?, bugs, or just restart the challenge
            }
        }
    }
}
