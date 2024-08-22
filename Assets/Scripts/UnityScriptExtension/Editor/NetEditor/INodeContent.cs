using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public struct ContentConnection
    {
        public INodeContent connectionSource;
        public INodeContent connectionTarget;
        public PortType sourceOutput;
        public PortType targetInput;
    }
    public interface INodeContent : IDraw, ICloneable
    {
        NodeContentType ContentType { get; }
        //提供输入端口类型
        PortType[] ProvideInputTypes();
        //提供输出端口类型
        PortType[] ProvideOutputTypes();
        //查找端口连接
        ContentConnection[] FindConnections(INodeContent[] nodeContents);
        //更新节点上下文
        void UpdateNodeContext(ref NodeContext nodeContext);
        //某一个输出端口连接时,执行与之连接的另外一个Content的连接操作,并返回是否成功连接
        bool TryConnectToContent(ContentConnection connection);
        //某一个输入端口连接时,执行与之连接的另外一个Content的连接操作
        void ConnectFromContent(ContentConnection connection);
        //某一个输出端口断开连接时,执行与之连接的另外一个Content的断开连接操作
        void DisconnectToContent(ContentConnection connection);
        //某一个输入端口断开连接时,执行与之连接的另外一个Content的断开连接操作
        void DisconnectFromContent(ContentConnection connection);
    }

    public struct NodeContentType : IEquatable<NodeContentType>
    {
        public int type;
        public string contentName;

        public bool Equals(NodeContentType other)
        {
            return type.Equals(other.type);
        }
        public Func<INodeContent> provideContent;
    }
}

