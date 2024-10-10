using System.Text;
using UnityEngine;

namespace Procedural_Generation_Road.LSystem
{
    public class LSystemGenerator : MonoBehaviour
    {
        [SerializeField] private Rule[] rules;
        [SerializeField] private string rootSentence;
        [SerializeField, Range(0, 10)] private int iterationLimit = 1;

        private void Start()
        {
            GenerateSentence();
        }

        [ContextMenu("Generate sentence")]
        public void GenerateSentence()
        {
            Debug.Log(ProcessWord(rootSentence));
        }

        public string ProcessWord(string word, int iterationIndex = 0)
        {
            if (iterationIndex >= iterationLimit)
            {
                return word;
            }

            StringBuilder newWord = new StringBuilder();

            foreach (char c in word)
            {
                newWord.Append(c);
                CheckRules(newWord, c, iterationIndex);
            }

            return newWord.ToString();
        }

        private void CheckRules(StringBuilder newWord, char c, int iterationIndex)
        {
            foreach (Rule rule in rules)
            {
                if (rule.letter == c)
                {
                    newWord.Append(ProcessWord(rule.GetResult(), iterationIndex + 1));
                }
            }
        }
    }
}
