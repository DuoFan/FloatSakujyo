using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace EditorExtension
{
    public class PortConnectionMatrix
    {
        private bool[,] matrix;
        private Dictionary<int, int> typeToIndex = new Dictionary<int, int>();
        private Dictionary<int, int> indexToType = new Dictionary<int, int>();
        private int currentIndex = 0;

        public PortConnectionMatrix()
        {
            int numPortTypes = 100; 
            matrix = new bool[numPortTypes, numPortTypes];
        }

        private int GetIndexForType(int type)
        {
            int index = -1;
            if (!typeToIndex.TryGetValue(type, out index))
            {
                index = currentIndex;
                typeToIndex[type] = index;
                indexToType[index] = type;
                currentIndex++;
            }
            return index;
        }

        public void AddConnection(PortType output, PortType input)
        {
            int outputIndex = GetIndexForType(output.type);
            int inputIndex = GetIndexForType(input.type);

            matrix[outputIndex, inputIndex] = true;
            matrix[inputIndex, outputIndex] = true;  // Ensure bidirectional connection
        }

        public bool IsConnectable(PortType output, PortType input)
        {
            int outputIndex = GetIndexForType(output.type);
            int inputIndex = GetIndexForType(input.type);
            return matrix[outputIndex, inputIndex];
        }

        public PortType[] GetConnectablePortTypes(PortType portType)
        {
            int portTypeIndex = GetIndexForType(portType.type);
            List<PortType> connectablePorts = new List<PortType>();
            for (int i = 0; i < currentIndex; i++)
            {
                if (matrix[portTypeIndex, i])
                {
                    connectablePorts.Add(new PortType() { type = indexToType[i] });
                }
            }
            return connectablePorts.ToArray();
        }
    }

}
