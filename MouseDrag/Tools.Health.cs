namespace MouseDrag
{
    static partial class Tools
    {
        public static void KillCreature(RainWorldGame game, PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (!(obj is Creature))
                return;
            if (Options.lineageKill?.Value == true && game?.FirstAlivePlayer != null)
                (obj as Creature).SetKillTag(game.FirstAlivePlayer);

            (obj as Creature).Die();
            if ((obj as Creature).abstractCreature?.state is HealthState)
                ((obj as Creature).abstractCreature.state as HealthState).health = 0f;
        }


        //kill all creatures in room
        public static void KillCreatures(RainWorldGame game, Room room)
        {
            Plugin.Logger.LogDebug("KillCreatures");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    if (!(room.physicalObjects[i][j] is Player && //don't kill when: creature is player and player is not SlugNPC (optional)
                        (Options.exceptSlugNPC?.Value != false || !(room.physicalObjects[i][j] as Player).isNPC)))
                        KillCreature(game, room.physicalObjects[i][j]);
        }


        public static void ReviveCreature(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
            if (!(obj is Creature))
                return;

            AbstractCreature ac = (obj as Creature).abstractCreature;
            if (ac?.state == null)
                return;

            if (ac.state is HealthState && (ac.state as HealthState).health < 1f)
                (ac.state as HealthState).health = 1f;
            ac.state.alive = true;
            (obj as Creature).dead = false;

            //try to exit game over mode
            if (Options.exitGameOverMode?.Value != false && 
                obj is Player && !(obj as Player).isNPC && 
                obj?.room?.game?.cameras?.Length > 0 && 
                obj.room.game.cameras[0]?.hud?.textPrompt != null)
                obj.room.game.cameras[0].hud.textPrompt.gameOverMode = false;
        }


        //revive all creatures in room
        public static void ReviveCreatures(Room room)
        {
            Plugin.Logger.LogDebug("ReviveCreatures");
            for (int i = 0; i < room?.physicalObjects?.Length; i++)
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                    ReviveCreature(room.physicalObjects[i][j]);
        }


        public static void TriggerObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
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

            if (obj is JellyFish)
                (obj as JellyFish).bites = 1;

            if (obj is WaterNut)
                (obj as WaterNut).Swell();

            if (obj is BubbleGrass && (obj as BubbleGrass).AbstrBubbleGrass != null)
                (obj as BubbleGrass).AbstrBubbleGrass.oxygenLeft = 0f;

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


        public static void ResetObject(PhysicalObject obj = null)
        {
            if (obj == null)
                obj = dragChunk?.owner;
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
            //TODO, reset consumed state of other types (food)?

            if (obj is SporePlant)
                (obj as SporePlant).Used = false;

            if (obj is DangleFruit)
                (obj as DangleFruit).bites = 3;

            if (obj is KarmaFlower)
                (obj as KarmaFlower).bites = 4;

            if (obj is JellyFish)
                (obj as JellyFish).bites = 3;

            if (obj is SwollenWaterNut) {
                WaterNut wn = new WaterNut(obj.abstractPhysicalObject);
                wn.AbstrNut.swollen = false;
                obj.room?.AddObject(wn);
                wn.firstChunk.HardSetPosition(obj.firstChunk.pos);
                obj.Destroy();
            }

            if (obj is BubbleGrass && (obj as BubbleGrass).AbstrBubbleGrass != null)
                (obj as BubbleGrass).AbstrBubbleGrass.oxygenLeft = 1f;

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
        }
    }
}
