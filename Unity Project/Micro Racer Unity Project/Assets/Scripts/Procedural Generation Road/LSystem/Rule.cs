using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural_Generation_Road.LSystem
{
    [CreateAssetMenu(menuName = "Procedural Generation/Road/Rule")]
    public class Rule : ScriptableObject
    {
        public char letter;
        [SerializeField] private string[] results;

        public string GetResult()
        {
            return results[0];
        }
    }
}
