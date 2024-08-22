using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorExtension
{

    public class Node : UnityEditor.Experimental.GraphView.Node, ISetter<INodeContent>
    {
        const float NOT_TREE_HEIGHT = -1;

        public INodeContent NodeContent
        {
            get; private set;
        }
        protected bool isSeted;
        protected NodeContext nodeContext;
        float treeHeight = NOT_TREE_HEIGHT;
        Vector2 pos;
        public Node()
        {
            NetView.instance.AddElement(this);
            nodeContext = new NodeContext();
        }
        public void Set(INodeContent content)
        {
            if (isSeted) return;
            isSeted = true;
            NodeContent = content;
            Update();
        }
        public void Update()
        {
            NodeContent.UpdateNodeContext(ref nodeContext);

            title = nodeContext.title;
            AdjustTitle();
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var menu = evt.menu;
            menu.AppendAction("显示细节", x => NetView.instance.DrawNode(this));
            menu.AppendAction("删除", x => DeleteSelf());
            menu.AppendAction("复制", x => NetView.instance.SetPaste(CopySelf));
        }
        public Node[] GetOutputNodes()
        {
            List<Node> nodes = new List<Node>();
            foreach (NodePort port in outputContainer.Children())
            {
                var edges = port.connections;
                foreach (Edge edge in edges)
                {
                    nodes.Add(edge.input.node as Node);
                }
            }
            return nodes.ToArray();
        }
        public Node[] GetInputNodes()
        {
            List<Node> nodes = new List<Node>();
            foreach (NodePort port in inputContainer.Children())
            {
                var edges = port.connections;
                foreach (Edge edge in edges)
                {
                    nodes.Add(edge.output.node as Node);
                }
            }
            return nodes.ToArray();
        }
        public override void SetPosition(Rect newPos)
        {
            pos = newPos.position;
            base.SetPosition(newPos);
        }
        public override Rect GetPosition()
        {
            return new Rect(pos, layout.size);
        }
        public float GetTreeHeight()
        {
            if (treeHeight != NOT_TREE_HEIGHT)
            {
                return treeHeight;
            }

            Queue<Node> waitToPush = new Queue<Node>();
            Stack<Node> stack = new Stack<Node>();
            HashSet<Node> visitedNodes = new HashSet<Node>();
            waitToPush.Enqueue(this);

            while(waitToPush.Count > 0)
            {
                var node = waitToPush.Dequeue();
                if (visitedNodes.Contains(node))
                {
                    continue;
                }
                visitedNodes.Add(node);

                stack.Push(node);
                var outputNodes = node.GetOutputNodes();
                for (int i = outputNodes.Length - 1; i >= 0; i--)
                {
                    waitToPush.Enqueue(outputNodes[i]);
                }
            }

            visitedNodes.Clear();
            while(stack.Count > 0)
            {
                var node = stack.Pop();
                if (visitedNodes.Contains(node))
                {
                    continue;
                }
                visitedNodes.Add(node);

                if(node.treeHeight != NOT_TREE_HEIGHT)
                {
                    continue;
                }

                node.treeHeight = 0;
                var outputNodes = node.GetOutputNodes();
                for (int i = outputNodes.Length - 1; i >= 0; i--)
                {
                    node.treeHeight += outputNodes[i].treeHeight;
                }
                node.treeHeight = Mathf.Max(node.treeHeight, node.layout.height);
            }

            return treeHeight;
        }

        protected INodeContent CopySelf()
        {
            var newContent = NodeContent.Clone() as INodeContent;
            return newContent;
        }

        protected void DeleteSelf()
        {
            if (IsRoot())
            {
                Debug.LogWarning("根节点不可删除");
                return;
            }

            NetView.instance.RemoveNode(this);
        }

        protected void AdjustTitle()
        {
            int interval = 20;
            if (title.Length > interval)
            {
                int length = title.Length;
                while (length > interval)
                {
                    title = title.Insert(length / interval * interval, "\n");
                    length -= interval;
                }
                RefreshExpandedState();
            }
        }
        protected bool IsRoot() => this == NetView.instance.Root;
        protected float GetOffset<T>(float offsetBase, Queue<T> queue, Action<T> EnqueueNext)
        {
            int maxDepth = 1;
            Queue<T> curQueue = new Queue<T>();
            while (queue.Count > 0)
            {
                while (queue.Count > 0)
                {
                    curQueue.Enqueue(queue.Dequeue());
                }
                while (curQueue.Count > 0)
                {
                    maxDepth++;
                    EnqueueNext(curQueue.Dequeue());
                }
            }
            return offsetBase * maxDepth;
        }
    }
    public class NodeContext
    {
        public string title;
    }
}

