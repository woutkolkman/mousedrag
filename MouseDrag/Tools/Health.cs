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
            if (!(obj is Creature))
                return;

            AbstractCreature ac = (obj as Creature).abstractCreature;
            if (ac?.state == null)
                return;

            if (ac.state is HealthState && (ac.state as HealthState).health < 1f)
                (ac.state as HealthState).health = 1f;
            ac.state.alive = true;
            (obj as Creature).dead = false;
            (obj as Creature).stun = 0; //makes player immediately controllable

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

            if (obj is Hazer) {
                (obj as Hazer).inkLeft = 1f;
                (obj as Hazer).hasSprayed = false;
                (obj as Hazer).clds = 0;
            }

            if ((obj as MoreSlugcats.StowawayBug)?.AI is MoreSlugcats.StowawayBugAI) {
                (obj as MoreSlugcats.StowawayBug).AI.activeThisCycle = true;
                (obj as MoreSlugcats.StowawayBug).AI.behavior = MoreSlugcats.StowawayBugAI.Behavior.Idle;
            }

            if (obj is Player) {
                //try to exit game over mode
                if (Options.exitGameOverMode?.Value != false && !(obj as Player).isNPC) {
                    //campaign
                    for (int i = 0; i < obj.room?.game?.cameras?.Length; i++)
                        if (obj.room.game.cameras[i]?.hud?.textPrompt != null)
                            obj.room.game.cameras[i].hud.textPrompt.gameOverMode = false;

                    //sandbox & challenges
                    if (obj.room?.game?.arenaOverlay != null) {
                        obj.room.game.arenaOverlay.ShutDownProcess();
                        obj.room.game.manager?.sideProcesses?.Remove(obj.room.game.arenaOverlay);
                        obj.room.game.arenaOverlay = null;
                        if (obj.room.game.session is ArenaGameSession) {
                            (obj.room.game.session as ArenaGameSession).sessionEnded = false;
                            (obj.room.game.session as ArenaGameSession).challengeCompleted = false;
                            (obj.room.game.session as ArenaGameSession).endSessionCounter = -1;
                        }
                    }
                }

                (obj as Player).exhausted = false;
                (obj as Player).lungsExhausted = false;
                (obj as Player).airInLungs = 1f;
                (obj as Player).aerobicLevel = 0f;
                if ((obj as Player).playerState != null) {
                    (obj as Player).playerState.permaDead = false;
                    (obj as Player).playerState.permanentDamageTracking = 0.0; //prevents sickness/waterdrips from revived pups
                }
                (obj as Player).animation = Player.AnimationIndex.None; //prevents lie-down slide
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

            if (obj is SeedCob && (obj as SeedCob).AbstractCob != null) {
                (obj as SeedCob).open = 0f;
                (obj as SeedCob).canBeHitByWeapons = true;
                (obj as SeedCob).freezingCounter = 0f;
                (obj as SeedCob).seedPopCounter = -1;
                (obj as SeedCob).seedsPopped = new bool[(obj as SeedCob).seedPositions.Length];
                (obj as SeedCob).AbstractCob.dead = false;
                (obj as SeedCob).AbstractCob.opened = false;
                (obj as SeedCob).AbstractCob.spawnedUtility = false;

                //makes seedcob return next cycle
                (obj as SeedCob).AbstractCob.isConsumed = false;
                if ((obj as SeedCob).AbstractCob.world?.game?.session is StoryGameSession) {
                    ((obj as SeedCob).AbstractCob.world.game.session as StoryGameSession).saveState?.ReportConsumedItem(
                        (obj as SeedCob).AbstractCob.world,
                        false,
                        (obj as SeedCob).AbstractCob.originRoom,
                        (obj as SeedCob).AbstractCob.placedObjectIndex,
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

            if (obj is Oracle)
            {
                if (obj.room?.game?.GetStorySession?.saveState?.deathPersistentSaveData != null) {
                    if ((obj as Oracle).ID == Oracle.OracleID.SS || 
                        (obj as Oracle).ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.CL)
                        obj.room.game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles = false;
                    if ((obj as Oracle).ID == Oracle.OracleID.SL) {
                        obj.room.game.GetStorySession.saveState.deathPersistentSaveData.ripMoon = false;
                        if (obj.room.game.GetStorySession.saveState.miscWorldSaveData?.SLOracleState?.neuronsLeft <= 0)
                            obj.room.game.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft++;
                    }
                }
                if ((obj as Oracle).ID != MoreSlugcats.MoreSlugcatsEnums.OracleID.ST) {
                    (obj as Oracle).health = 1f;
                    (obj as Oracle).stun = 0;
                }
                if ((obj as Oracle).ID == Oracle.OracleID.SL &&
                    (obj as Oracle).mySwarmers?.Count <= 0)
                    (obj as Oracle).SetUpSwarmers();

                /*//reset challenge #70
                if ((obj as Oracle).ID == MoreSlugcats.MoreSlugcatsEnums.OracleID.ST && 
                    (obj as Oracle).oracleBehavior is MoreSlugcats.STOracleBehavior) {
                    ((obj as Oracle).oracleBehavior as MoreSlugcats.STOracleBehavior).curPhase = MoreSlugcats.STOracleBehavior.Phase.Inactive;
                }*/ //TODO?, bugs, or just restart the challenge
            }
        }
    }
}
