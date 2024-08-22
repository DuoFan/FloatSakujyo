using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static EditorExtension.NetView;

namespace EditorExtension
{
    public class NodePort : Port
    {
        public PortType PortType { get; private set; }
        private NodePort(Orientation orientation, Direction direction, Capacity capacity, System.Type type) :
            base(orientation, direction, capacity, type) { }
        public static NodePort CreatePort(PortType portType, IEdgeConnectorListener edgeConnectorListener)
        {
            var port = new NodePort(portType.orientation, portType.Direction, portType.capacity, typeof(int));
            port.portColor = portType.color;
            port.PortType = portType;
            port.portName = portType.portName;
            port.m_EdgeConnector = new EdgeConnector<Edge>(edgeConnectorListener);
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }
    }

    public struct PortType : IEquatable<PortType>
    {
        public int type;
        public string portName; 
        public Orientation orientation;
        public Direction Direction;
        public Port.Capacity capacity;
        public Color color;

        public static bool operator ==(PortType left, PortType right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PortType left, PortType right)
        {
            return !left.Equals(right);
        }
        public override bool Equals(object obj)
        {
            return obj is PortType type && Equals(type);
        }
        public bool Equals(PortType other)
        {
            return type.Equals(other.type);
        }
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(portName);
            hashCode = hashCode * -1521134295 + orientation.GetHashCode();
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            hashCode = hashCode * -1521134295 + capacity.GetHashCode();
            hashCode = hashCode * -1521134295 + color.GetHashCode();
            return hashCode;
        }
    }
}

