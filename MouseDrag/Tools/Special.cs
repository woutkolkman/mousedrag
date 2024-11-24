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
    }
}
