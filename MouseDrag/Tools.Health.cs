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
            if (Options.lineageKill?.Value == true && game?.Players?.Count > 0)
                (obj as Creature).SetKillTag(game.Players[0]);

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
    }
}
