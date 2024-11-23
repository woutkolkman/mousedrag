namespace MouseDrag
{
    public static class Special
    {
        //activate/load all rooms in current region
        public static void ActivateRegionRooms(RainWorldGame self)
        {
            if (!(self?.world?.abstractRooms?.Length > 0))
                return;

            int totR = 0;
            foreach (AbstractRoom ar in self.world.abstractRooms) {
                if (ar == null || ar.realizedRoom != null)
                    continue;
                self.world.ActivateRoom(ar);
                totR++;
            }

            if (Options.logDebug?.Value != false)
                Plugin.Logger.LogDebug("ActivateRegionRooms, region: " + self.world.name + ", total activated rooms: " + totR);
        }
    }
}
