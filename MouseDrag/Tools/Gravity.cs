using System;

namespace MouseDrag
{
    public static class Gravity
    {
        public static GravityTypes gravityType;

        public enum GravityTypes
        {
            None, 
            Off, 
            Half, 
            On,
            Inverse
        }


        public static void CycleGravity()
        {
            //gravityType = gravityType.Next();

            //exclude half and inverse to avoid confusion about current active type
            if (gravityType == GravityTypes.None) {
                gravityType = GravityTypes.Off;
            } else if (gravityType == GravityTypes.Off) {
                gravityType = GravityTypes.On;
            } else {
                gravityType = GravityTypes.None;
            }
        }


        //https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));
            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }


        public static void Update(Room room)
        {
            if (room == null)
                return;
            if (Gravity.gravityType == Gravity.GravityTypes.Off)
                room.gravity = 0f;
            if (Gravity.gravityType == Gravity.GravityTypes.Half)
                room.gravity = 0.2f;
            if (Gravity.gravityType == Gravity.GravityTypes.On)
                room.gravity = 1f;
            if (Gravity.gravityType == Gravity.GravityTypes.Inverse)
                room.gravity = -1f;
        }
    }
}
