using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Procedural_Generation_Road.LSystem
{
    public class SimpleVisualiser : MonoBehaviour
    {
        [SerializeField] private LSystemGenerator lSystem;
        [SerializeField] private GameObject prefab;

        [Header("Line parameters")]
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color lineColor = Color.red;
        [SerializeField] private float lineWidth = .2f;

        private List<Vector3> positions = new List<Vector3>();
        private int length = 8;
        private float angle = 90f;

        public int Length
        {
            get
            {
                if (length > 0) return length;
                else return 1;
            }
            set => length = value;
        }

        private void Start()
        {
            string sequence = lSystem.GenerateSentence();

            VisualizeSequence(sequence);
        }

        private void VisualizeSequence(string sequence)
        {
            Stack<AgentParameters> savePoints = new Stack<AgentParameters>();
            var currentPosition = Vector3.zero;

            Vector3 direction = Vector3.forward;
            Vector3 tempPosition = Vector3.zero;

            positions.Add(currentPosition);

            foreach (var letter in sequence)
            {
                EncodingLetters encoding = (EncodingLetters)letter;

                switch (encoding)
                {
                    case EncodingLetters.save:
                        savePoints.Push(new AgentParameters
                        {
                            position = currentPosition,
                            direction = direction,
                            length = length
                        });
                        break;
                    case EncodingLetters.load:
                        if (savePoints.Count > 0)
                        {
                            var agentParameter = savePoints.Pop();
                            currentPosition = agentParameter.position;
                            direction = agentParameter.direction;
                            Length = agentParameter.length;
                        }
                        else
                        {
                            throw new System.Exception("Don't have saved point in our stack");
                        }
                        break;
                    case EncodingLetters.draw:
                        tempPosition = currentPosition;
                        currentPosition += direction * length;
                        DrawLine(tempPosition, currentPosition, lineColor);
                        Length -= 2;
                        positions.Add(currentPosition);
                        break;
                    case EncodingLetters.turnRight:
                        direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                        break;
                    case EncodingLetters.turnLeft:
                        direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                        break;
                }
            }

            foreach (var position in positions)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
        }

        private void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            GameObject line = new GameObject("Line");
            line.transform.position = start;

            var lineRenderer = line.AddComponent<LineRenderer>();

            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = lineRenderer.endColor = color;
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
            lineRenderer.SetPositions(new Vector3[] { start, end });
        }

        public enum EncodingLetters
        {
            unknown = '1',
            save = '[',
            load = ']',
            draw = 'F',
            turnRight = '+',
            turnLeft = '-'
        }
    }
}