using UnityEngine;

namespace EternalDread3RMOD
{
    public static class DodgeJumpInertia
    {
        public static Vector3 velocity = Vector3.zero;
        public static float startTime = 0f;
        public static bool active = false;
        public static bool prevOnRising = false;
    }
}