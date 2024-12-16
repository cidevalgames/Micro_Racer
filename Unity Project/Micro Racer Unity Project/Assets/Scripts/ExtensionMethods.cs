using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public static class ExtensionMethods
    {
        public static bool ContainsMask(this LayerMask mask, GameObject objectToCheck)
        {
            return (mask.value & (1 << objectToCheck.layer)) != 0;
        }
    }
}