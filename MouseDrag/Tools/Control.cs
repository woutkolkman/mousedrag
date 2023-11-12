using UnityEngine;

namespace MouseDrag
{
    public static class Control
    {
        public static void ToggleControl(PhysicalObject po)
        {
            AbstractCreature ac = po?.abstractPhysicalObject as AbstractCreature;
            if (ac == null)
                return;

            ac.controlled = !ac.controlled;
        }
    }
}
