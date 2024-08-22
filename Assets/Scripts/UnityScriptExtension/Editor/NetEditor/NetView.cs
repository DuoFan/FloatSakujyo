using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UIElements;

namespace EditorExtension
{
    public class NetView : GraphView, IDisposable
    {
        public Node Root;
        public NetWindow netWindow;
        public static NetView instance;
        public Func<INodeContent> paste;
        Queue<ExhibitWindow> exhibitWindows;
        public INetProvider NetProvider
        {
            get; private set;
        }
        public EdgeConnectorListener _EdgeConnectorListener
        {
            get; private set;
        }
        PortConnectionMatrix portConnectionMatrix;
        Dictionary<(Type sourceType, Type targetType), Dictionary<object, object>> mapper;
        public void Dispose()
        {
            if (exhibitWindows != null)
            {
                while (exhibitWindows.Count > 0)
                {
                    var window = exhibitWindows.Dequeue();
                    window?.Close();
                }
            }
        }

        public void Initialize(INetProvider netProvider)
        {
            instance = this;
            AddGridBackGround();
            NetProvider = netProvider;
            exhibitWindows = new Queue<ExhibitWindow>();

            //新建搜索菜单
            var menuWindowProvider = ScriptableObject.CreateInstance<SearchMenuWindowProvider>();
            menuWindowProvider.Set(NetProvider.ProvideNodeContentTypes());
            menuWindowProvider.OnSelectEntryHandler = OnMenuSelectEntry;
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
            };

            InitNet(NetProvider);
        }
        //初始化已经包含的内容
        Node InitNode(INodeContent nodeContent)
        {
            if (HasObjectMap(nodeContent, typeof(Node)))
            {
                return GetObjectMap<Node>(nodeContent);
            }

            Node node = new Node();

            var inputPortTypes = nodeContent.ProvideInputTypes();
            if (inputPortTypes != null)
            {
                for (int i = 0; i < inputPortTypes.Length; i++)
                {
                    var port = NodePort.CreatePort(inputPortTypes[i], _EdgeConnectorListener);
                    node.inputContainer.Add(port);
                }
            }

            var outputPortTypes = nodeContent.ProvideOutputTypes();
            if (outputPortTypes != null)
            {
                for (int i = 0; i < outputPortTypes.Length; i++)
                {
                    var port = NodePort.CreatePort(outputPortTypes[i], _EdgeConnectorListener);
                    node.outputContainer.Add(port);
                }
            }

            node.Set(nodeContent);

            SetObjectMap(node, nodeContent);
            SetObjectMap(nodeContent, node);

            AddElement(node);

            return node;
        }
        //添加新内容
        public Node AddContent(INodeContent nodeContent)
        {
            var node = InitNode(nodeContent);
            NetProvider.NodeContents.Add(nodeContent);
            return node;
        }
        public void RemoveNode(Node node)
        {
            for (int i = 0; i < node.inputContainer.childCount; i++)
            {
                var port = node.inputContainer[i] as Port;
                var edges = port.connections.ToArray();
                for (int j = 0; j < edges.Length; j++)
                {
                    RemoveEdge(edges[j]);
                }
            }

            for (int i = 0; i < node.outputContainer.childCount; i++)
            {
                var port = node.outputContainer[i] as Port;
                var edges = port.connections.ToArray();
                for (int j = 0; j < edges.Length; j++)
                {
                    RemoveEdge(edges[j]);
                }
            }

            NetProvider.NodeContents.Remove(node.NodeContent);

            RemoveElement(node);
        }
        public void DrawNode(Node node)
        {
            var window = ExhibitWindow.Draw(node);
            exhibitWindows.Enqueue(window);
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (evt.target is GraphView)
            {
                evt.menu.AppendAction("刷新", a =>
                {
                    nodes.ForEach(n =>
                    {
                        RemoveObjectMap<Node>((n as Node).NodeContent);
                        RemoveElement(n);
                    });
                    edges.ForEach(e =>
                    {
                        RemoveElement(e);
                    });
                    AddGridBackGround();
                    InitNet(NetProvider);
                },
                a => Root == null ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal,
                evt.target
                );

                evt.menu.AppendAction("粘贴", a =>
                {
                    var content = paste();
                    var node = AddContent(content);
                    SetNodePosition(node, new Rect(this.WorldToLocal(a.eventInfo.localMousePosition), Vector2.one));
                },
                a => paste != null ? DropdownMenuAction.Status.Normal :
                DropdownMenuAction.Status.Disabled,
                evt.target
                );
            }
        }
        public void SetPaste(Func<INodeContent> action)
        {
            paste = action;
        }
        public bool TryConnectPort(Port output, Port input)
        {
            var edges = output.connections;
            foreach (var edge in edges)
            {
                if (edge.input == input)
                    return false;
            }
            Edge tempEdge = new Edge()
            {
                output = output,
                input = input
            };
            bool result = TryConnectEdge(tempEdge);
            if (!result)
            {
                RemoveElement(tempEdge);
            }
            return result;
        }
        public bool TryConnectEdge(Edge edge)
        {
            ContentConnection connection = new ContentConnection()
            {
                connectionSource = (edge.output.node as Node).NodeContent,
                connectionTarget = (edge.input.node as Node).NodeContent,
                sourceOutput = (edge.output as NodePort).PortType,
                targetInput = (edge.input as NodePort).PortType
            };
            if (!(edge.output.node as Node).NodeContent.TryConnectToContent(connection))
            {
                return false;
            }
            (edge.input.node as Node).NodeContent.ConnectFromContent(connection);
            edge.output.Connect(edge);
            edge.input.Connect(edge);
            AddElement(edge);
            return true;
        }
        void DisconnectEdge(Edge edge)
        {
            ContentConnection connection = new ContentConnection()
            {
                connectionSource = (edge.output.node as Node).NodeContent,
                connectionTarget = (edge.input.node as Node).NodeContent,
                sourceOutput = (edge.output as NodePort).PortType,
                targetInput = (edge.input as NodePort).PortType
            };
            (edge.output.node as Node).NodeContent.DisconnectToContent(connection);
            (edge.input.node as Node).NodeContent.DisconnectFromContent(connection);
            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);
        }
        public void RemoveEdge(Edge edge)
        {
            DisconnectEdge(edge);
            RemoveElement(edge);
        }
        public bool HasObjectMap(object source, Type targetType)
        {
            var sourceType = source.GetType();
            if (!HasTypeMap(sourceType, targetType))
            {
                return false;
            }

            var map = mapper[(sourceType, targetType)];
            return map.ContainsKey(source);
        }
        public T GetObjectMap<T>(object source)
        {
            var sourceType = source.GetType();
            var targetType = typeof(T);
            if (!HasTypeMap(sourceType, targetType))
            {
                return default;
            }

            var map = mapper[(sourceType, targetType)];
            if (!map.ContainsKey(source))
            {
                return default;
            }

            return (T)map[source];
        }
        public void SetObjectMap(object source, object dest)
        {
            var sourceType = source.GetType();
            var targetType = dest.GetType();
            if (!HasTypeMap(sourceType, targetType))
            {
                AddTypeMap(sourceType, targetType);
            }
            var dict = mapper[(sourceType, targetType)];
            dict[source] = dest;
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var nodePort = startPort as NodePort;
            var connectablePortTypes = portConnectionMatrix.GetConnectablePortTypes(nodePort.PortType);
            var direction = nodePort.direction == Direction.Input ? Direction.Output : Direction.Input;
            return FindPortByPortTypes(direction, connectablePortTypes);
        }

        public class EdgeConnectorListener : IEdgeConnectorListener
        {
            PortConnectionMatrix portConnectionMatrix;
            public EdgeConnectorListener(PortConnectionMatrix portConnectionMatrix)
            {
                this.portConnectionMatrix = portConnectionMatrix;
            }
            public void OnDrop(GraphView graphView, Edge edge)
            {
                var inputPort = edge.input as NodePort;
                var outputPort = edge.output as NodePort;

                if (inputPort == null || outputPort == null)
                {
                    return;
                }

                var inputPortType = inputPort.PortType;
                var outputPortType = outputPort.PortType;

                if (!portConnectionMatrix.IsConnectable(outputPortType, inputPortType))
                {
                    return;
                }

                if (outputPort.capacity == Port.Capacity.Single && outputPort.connected)
                {
                    instance.RemoveEdge(outputPort.connections.Single());
                }

                if (inputPort.capacity == Port.Capacity.Single && inputPort.connected)
                {
                    instance.RemoveEdge(inputPort.connections.Single());
                }

                instance.TryConnectEdge(edge);
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                var inputPort = edge.input as NodePort;
                var outputPort = edge.output as NodePort;

                if (inputPort == null || outputPort == null)
                {
                    return;
                }

                instance.RemoveEdge(edge);
            }
        }
        protected void RequestNodeCreation(VisualElement target, int index, Vector2 position)
        {
            if (nodeCreationRequest == null)
                return;

            Vector2 screenPoint = this.WorldToLocal(position);
            nodeCreationRequest(new NodeCreationContext() { screenMousePosition = screenPoint, target = target, index = index });
        }
        private void InitNet(INetProvider netProvider)
        {
            NetProvider = netProvider;
            portConnectionMatrix = NetProvider.ProvidePortConnectionMatrix();
            _EdgeConnectorListener = new EdgeConnectorListener(portConnectionMatrix);

            var rootContent = NetProvider.ProvideRootContent();
            Root = InitNode(rootContent);

            var contentArray = NetProvider.NodeContents.ToArray();

            for (int i = 0; i < contentArray.Length; i++)
            {
                var content = contentArray[i];
                //跳过根节点
                if (content == rootContent)
                {
                    continue;
                }
                InitNode(content);
            }

            for (int i = 0; i < contentArray.Length; i++)
            {
                InitConnectionOfContent(contentArray[i], contentArray);
            }


            EditorUtils.DelayTask(() => SortContents(contentArray), 10 * contentArray.Length);
        }
        void InitConnectionOfContent(INodeContent content, INodeContent[] contentArray)
        {
            var connections = content.FindConnections(contentArray);

            if (connections == null)
            {
                return;
            }

            for (int i = 0; i < connections.Length; i++)
            {
                var connectionSource = connections[i].connectionSource;
                var connectionTarget = connections[i].connectionTarget;
                var sourceNode = GetObjectMap<Node>(connectionSource);
                var targetNode = GetObjectMap<Node>(connectionTarget);
                NodePort output = null;
                NodePort input = null;
                for (int j = 0; j < sourceNode.outputContainer.childCount; j++)
                {
                    var port = sourceNode.outputContainer[j] as NodePort;
                    if (port.PortType.Equals(connections[i].sourceOutput))
                    {
                        output = port;
                        break;
                    }
                }
                for (int j = 0; j < targetNode.inputContainer.childCount; j++)
                {
                    var port = targetNode.inputContainer[j] as NodePort;
                    if (port.PortType.Equals(connections[i].targetInput))
                    {
                        input = port;
                        break;
                    }
                }
                if (output != null && input != null)
                {
                    var edge = new Edge()
                    {
                        output = output,
                        input = input
                    };
                    edge.output.Connect(edge);
                    edge.input.Connect(edge);
                    AddElement(edge);
                }
            }
        }
        void SortContents(INodeContent[] contentArray)
        {
            GameExtension.UnionFind contentUnionFind = new GameExtension.UnionFind(contentArray.Length);
            for (int i = 0; i < contentArray.Length; i++)
            {
                var content = contentArray[i];
                var connections = content.FindConnections(contentArray);
                if (connections == null)
                {
                    continue;
                }
                for (int j = 0; j < connections.Length; j++)
                {
                    var connectionTarget = connections[j].connectionTarget;
                    int index = Array.IndexOf(contentArray, connectionTarget);
                    if (contentUnionFind.Find(index) != index)
                    {
                        contentUnionFind.Union(i, Array.IndexOf(contentArray, connectionTarget));
                    }
                }
            }

            ContentConnection[] FilterThenSortConnectionByOutputPort(INodeContent content, ContentConnection[] connections)
            {
                if (connections == null)
                {
                    return null;
                }

                List<ContentConnection> list = new List<ContentConnection>();
                for (int i = 0; i < connections.Length; i++)
                {
                    if (connections[i].connectionTarget == content)
                    {
                        continue;
                    }
                    list.Add(connections[i]);
                }
                var outputPorts = content.ProvideOutputTypes();
                list.Sort((x, y) =>
                {
                    return Array.IndexOf(outputPorts, x.sourceOutput) - Array.IndexOf(outputPorts, y.sourceOutput);
                });
                return list.ToArray();
            }

            var groups = contentUnionFind.GetAllGroups();
            foreach (var group in groups)
            {
                var root = contentArray[group.Key];
                Queue<INodeContent> waitToPush = new Queue<INodeContent>();
                Stack<INodeContent> waitToSort = new Stack<INodeContent>();
                waitToPush.Enqueue(root);
                HashSet<INodeContent> visited = new HashSet<INodeContent>();
                while (waitToPush.Count > 0)
                {
                    var content = waitToPush.Dequeue();

                    if (visited.Contains(content))
                    {
                        continue;
                    }

                    visited.Add(content);

                    waitToSort.Push(content);
                    var connections = content.FindConnections(contentArray);
                    connections = FilterThenSortConnectionByOutputPort(content, connections);
                    if (connections == null)
                    {
                        continue;
                    }
                    for (int i = 0; i < connections.Length; i++)
                    {
                        var connectionTarget = connections[i].connectionTarget;
                        if (connectionTarget != content)
                        {
                            waitToPush.Enqueue(connectionTarget);
                        }
                        else
                        {
                            waitToPush.Enqueue(connections[i].connectionSource);
                        }
                    }
                }

                visited.Clear();
                while (waitToSort.Count > 0)
                {
                    var content = waitToSort.Pop();

                    if (visited.Contains(content))
                    {
                        continue;
                    }

                    visited.Add(content);

                    var connections = content.FindConnections(contentArray);
                    connections = FilterThenSortConnectionByOutputPort(content, connections);
                    if (connections == null || connections.Length <= 0)
                    {
                        continue;
                    }

                    float xInterval = 0;
                    for (int i = 0; i < connections.Length; i++)
                    {
                        var connectionTarget = connections[i].connectionTarget;
                        var targetNode = GetObjectMap<Node>(connectionTarget);
                        xInterval = Mathf.Max(xInterval, targetNode.layout.width + 50);
                    }

                    var node = GetObjectMap<Node>(content);
                    var pos = node.GetPosition().position;
                    float totalHeight = 0f;

                    // 计算所有子节点的总树高度和它们之间的总间隔
                    for (int i = 0; i < connections.Length; i++)
                    {
                        var connectionTarget = connections[i].connectionTarget;
                        var targetNode = GetObjectMap<Node>(connectionTarget);
                        totalHeight += targetNode.GetTreeHeight();
                    }

                    float spacing = connections.Length > 1 ? totalHeight / connections.Length : 0;

                    // 计算总间隔，间隔数量比输出节点数量少1
                    totalHeight += spacing * (connections.Length - 1);

                    // 计算开始的y位置
                    float yStart = pos.y - totalHeight / 2;

                    float yOffset = 0;
                    for (int i = 0; i < connections.Length; i++)
                    {
                        var connectionTarget = connections[i].connectionTarget;

                        var targetNode = GetObjectMap<Node>(connectionTarget);

                        float xPosition = pos.x + xInterval;
                        float yPosition = yStart + yOffset + targetNode.GetTreeHeight() / 2; // 由于Rect中的y是从中心开始的，所以我们添加树的一半高度

                        SetNodePosition(targetNode, new Rect(new Vector2(xPosition, yPosition), targetNode.GetPosition().size));

                        // 更新yOffset
                        yOffset += targetNode.GetTreeHeight() + spacing;
                    }
                }
            }

            EditorUtils.DelayTask(() =>
            {
                List<Node> roots = new List<Node>();

                for (int i = 0; i < contentArray.Length; i++)
                {
                    var node = GetObjectMap<Node>(contentArray[i]);
                    if (node.GetInputNodes().Length <= 0)
                    {
                        roots.Add(node);
                    }
                }
                roots.Sort((x, y) =>
                {
                    if(x == Root)
                    {
                        return -1;
                    }
                    else
                    {
                        return x.GetOutputNodes().Length == 0 ? 1 : -1;
                    }
                });
                var rootPos = roots[0].GetPosition().position;
                for (int i = 1; i < roots.Count; i++)
                {
                    var root = roots[i];
                    var lastRoot = roots[i - 1];
                    rootPos.y = rootPos.y + (lastRoot.GetTreeHeight() + root.GetTreeHeight()) / 1.5f;
                    SetNodePosition(root, new Rect(rootPos, root.GetPosition().size));
                }
            }, 100);
        }
        void SetNodePosition(Node startNode, Rect newPos)
        {
            Stack<Node> stack = new Stack<Node>();
            HashSet<Node> visitedNodes = new HashSet<Node>();

            stack.Push(startNode);

            var startPos = startNode.GetPosition();
            var offset = newPos.position - startPos.position;

            while (stack.Count > 0)
            {
                Node currentNode = stack.Pop();

                if (visitedNodes.Contains(currentNode))
                {
                    continue;
                }

                visitedNodes.Add(currentNode);

                var oldPos = currentNode.GetPosition();
                var newRect = new Rect(oldPos.position + offset, oldPos.size);
                currentNode.SetPosition(newRect);

                var outputNodes = currentNode.GetOutputNodes();
                for (int i = outputNodes.Length - 1; i >= 0; i--)
                {
                    stack.Push(outputNodes[i]);
                }
            }
        }

        private class TempGridBackground : GridBackground { }
        void AddGridBackGround()
        {
            //添加网格背景
            GridBackground gridBackground = new TempGridBackground();
            //直接使用GridBackground 不会出现网格 ？TODO
            //GridBackground gridBackground = new GridBackground();
            gridBackground.name = "GridBackground";
            Insert(0, gridBackground);

            this.SetupZoom(0.25f, 2.0f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new FreehandSelector());
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();
        }
        bool HasTypeMap(Type sourceType, Type targetType)
        {
            if (mapper == null)
            {
                return false;
            }
            return mapper.ContainsKey((sourceType, targetType));
        }
        void AddTypeMap(Type sourceType, Type targetType)
        {
            if (mapper == null)
            {
                mapper = new Dictionary<(Type sourceType, Type targetType), Dictionary<object, object>>();
            }
            if (!mapper.ContainsKey((sourceType, targetType)))
            {
                mapper.Add((sourceType, targetType), new Dictionary<object, object>());
            }
        }
        void RemoveObjectMap<T>(object source)
        {
            var sourceType = source.GetType();
            var targetType = typeof(T);
            if (!HasTypeMap(sourceType, targetType))
            {
                return;
            }
            var dict = mapper[(sourceType, targetType)];
            dict.Remove(source);
        }
        private bool OnMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = netWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, context.screenMousePosition - netWindow.position.position);
            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            var contentType = (NodeContentType)searchTreeEntry.userData;
            var node = AddContent(contentType.provideContent());
            node.SetPosition(new Rect(graphMousePosition, Vector2.zero));
            return true;
        }
        List<Port> FindPortByPortTypes(Direction direction, params PortType[] portTypes)
        {
            List<Port> ports = new List<Port>();
            nodes.ForEach(x =>
            {
                var node = x as Node;
                if (direction == Direction.Input)
                {
                    for (int i = 0; i < x.inputContainer.childCount; i++)
                    {
                        var port = x.inputContainer[i] as NodePort;
                        if (Array.IndexOf(portTypes, port.PortType) >= 0)
                        {
                            ports.Add(port);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < x.outputContainer.childCount; i++)
                    {
                        var port = x.outputContainer[i] as NodePort;
                        if (Array.IndexOf(portTypes, port.PortType) >= 0)
                        {
                            ports.Add(port);
                        }
                    }
                }
            });
            return ports;
        }
    }
}