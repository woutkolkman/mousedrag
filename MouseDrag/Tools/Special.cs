namespace MouseDrag
{
    public static class Special
    {
        //activate/load all rooms in current region
        public static void ActivateRegionRooms(RainWorldGame self)
        {
            if (!(self?.world?.abstractRooms?.Length > 0))
                return;

            int newR = 0;
            int totR = 0;

            foreach (AbstractRoom ar in self.world.abstractRooms) {
                if (ar == null) //safety
                    continue;
                totR++;
                if (ar.realizedRoom != null) //already activated
                    continue;
                self.world.ActivateRoom(ar);
                newR++;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ActivateRegionRooms, region: " + self.world.name + 
                    ", newly activated room count: " + newR + ", total room count: " + totR);
        }


        //abstract creature and abstract item printed with same format
        public static string ConsistentName(AbstractPhysicalObject apo)
        {
            if (apo is AbstractCreature)
                return (apo as AbstractCreature).creatureTemplate?.name + " " + apo.ID.ToString() + (apo.ID.altSeed > -1 ? "." + apo.ID.altSeed : "");
            if (apo != null && !(apo is AbstractCreature))
                return apo.type?.ToString() + " " + apo.ID.ToString() + (apo.ID.altSeed > -1 ? "." + apo.ID.altSeed : "");
            return string.Empty;
        }
    }
}
