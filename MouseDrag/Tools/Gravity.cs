using System;
using System.Collections.Generic;

namespace MouseDrag
{
    public static class Gravity
    {
        public static List<KeyValuePair<AbstractPhysicalObject, GravityTypes>> gravityStates = new List<KeyValuePair<AbstractPhysicalObject, GravityTypes>>();

        public enum GravityTypes
        {
            Off,
            On
        }


        public static void CycleObjectGravity(PhysicalObject po)
        {
            if (po?.abstractPhysicalObject == null)
                return;
            var apo = po.abstractPhysicalObject;
            var pair = ListContains(apo);
            if (pair == null) {
                gravityStates.Add(new KeyValuePair<AbstractPhysicalObject, GravityTypes>(apo, GravityTypes.Off));
                po.gravity = 0f;
            } else if (pair.Value.Value == GravityTypes.Off) {
                ListChange(apo, GravityTypes.On);
                po.gravity = 1f;
            } else if (pair.Value.Value == GravityTypes.On) {
                ListRemove(apo);
                po.gravity = po.EffectiveRoomGravity;
            }
        }


        public static KeyValuePair<AbstractPhysicalObject, GravityTypes>? ListContains(AbstractPhysicalObject apo)
        {
            if (apo == null)
                return null;
            foreach (var pair in gravityStates)
                if (pair.Key == apo)
                    return pair;
            return null;
        }


        public static void ListRemove(AbstractPhysicalObject apo)
        {
            for (int i = 0; i < gravityStates.Count; i++) {
                if (gravityStates[i].Key == apo) {
                    gravityStates.Remove(gravityStates[i]);
                    break;
                }
            }
        }


        public static void ListChange(AbstractPhysicalObject apo, GravityTypes gt)
        {
            for (int i = 0; i < gravityStates.Count; i++) {
                if (gravityStates[i].Key == apo) {
                    gravityStates[i] = new KeyValuePair<AbstractPhysicalObject, GravityTypes>(apo, gt);
                    break;
                }
            }
        }
    }
}
