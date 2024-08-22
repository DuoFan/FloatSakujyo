using System;
using System.Collections.Generic;

namespace GameExtension
{
    public class BSNode<T> 
    {
        public T Value
        {
            get; internal set;
        }
        public BSNode<T> Parent
        {
            get; internal set;
        }
        public BSNode<T> Left
        {
            get; internal set;
        }
        public BSNode<T> Right
        {
            get; internal set;
        }
        public BSNode(T _value)
        {
            Value = _value;
            Height = 0;
        }
        public int Height
        {
            get; private set;
        }

        public int GetLeftHeight() => Left == null ? 0 : Left.Height;
        public int GetRightHeight() => Right == null ? 0 : Right.Height;
        public bool IsLeftHigher() => GetLeftHeight() > GetRightHeight();
        internal void UpdateHeight()
        {
            Height = 1 + Math.Max(GetLeftHeight(), GetRightHeight());
        }
        internal void UpdateHeightAbove()
        {
            var node = this;
            while (node != null)
            {
                node.UpdateHeight();
                node = node.Parent;
            }
        }
        internal void InsertRight(BSNode<T> _right)
        {
            Right = _right;
            if (Right != null)
                Right.Parent = this;
        }
        internal void InsertLeft(BSNode<T> _left)
        {
            Left = _left;
            if (Left != null)
                Left.Parent = this;
        }
        public void PreOrderVisit(Action<T> visit)
        {
            BSNode<T> node = this;
            Stack<BSNode<T>> stack = new Stack<BSNode<T>>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                node = stack.Pop();
                visit(node.Value);
                if (node.Right != null) stack.Push(node.Right);
                if (node.Left != null) stack.Push(node.Left);
            }
        }
        public void InOrderVisit(Action<T> visit)
        {
            BSNode<T> node = this;
            Stack<BSNode<T>> stack = new Stack<BSNode<T>>();

            while (node != null || stack.Count > 0)
            {
                GetLeftLink(node, stack);
                if (stack.Count > 0)
                {
                    node = stack.Pop();
                    visit(node.Value);
                    node = node.Right;
                }
            }
        }
        void GetLeftLink(BSNode<T> node, Stack<BSNode<T>> stack)
        {
            while (node != null)
            {
                stack.Push(node);
                node = node.Left;
            }
        }
        internal BSNode<T> GetSucc()
        {
            var node = Right;
            if (node != null)
            {
                while (node.Left != null)
                    node = node.Left;
            }
            return node;
        }
    }
    public class BSTree<T> 
    {
        protected IBSElementComparer<T> comparer;
        public BSNode<T> Root
        {
            get; protected set;
        }
        protected BSNode<T> last;

        public BSTree(T _root, IBSElementComparer<T> _comparer)
        {
            comparer = _comparer;
            if (_root != null)
            {
                Root = last = new BSNode<T>(_root);
            }
        }
        public void Insert(T e)
        {
            InsertNode(e);
        }
        protected virtual BSNode<T> InsertNode(T e)
        {
            BSNode<T> node = SearchNode(e);
            if (node == null)
            {
                node = new BSNode<T>(e);
                if (last == null) SetRoot(node);
                else
                {
                    int compareResult = comparer.Compare(e, last.Value);
                    if(compareResult > 0)
                    {
                        last.InsertRight(node);
                    }
                    else if(compareResult < 0)
                    {
                        last.InsertLeft(node);
                    }
                    else
                    {
                        var error = $"BSTree插入失败,重复的键值:{e}:{last}";
                        GameExtension.Logger.Exception(error);
                        throw new System.Exception(error);
                    }
                }
            }
            return node;
        }
        public void Remove(T e)
        {
            RemoveNode(e);
        }
        protected virtual BSNode<T> RemoveNode(T e)
        {
            BSNode<T> node = SearchNode(e);
            if (node != null)
            {
                BSNode<T> child;
                if (node.Left != null && node.Right != null)
                {
                    child = node.GetSucc();
                    T temp = node.Value;
                    node.Value = child.Value;
                    child.Value = temp;
                    node = child;
                    child = child.Right;
                }
                else child = node.Left != null ? node.Left : node.Right;

                last = node.Parent;
                ReplaceNode(child, node);

                if (child != null) child.UpdateHeightAbove();
            }
            return node;
        }
        public T Search(int index)
        {
            var node = SearchNode(index);
            return node == null ? default : node.Value;
        }
        protected virtual BSNode<T> SearchNode(T target)
        {
            BSNode<T> node = Root;
            last = Root;
            while (node != null)
            {
                int compareResult = comparer.Compare(target, node.Value);
                if (compareResult == 0) break;
                last = node;
                node = comparer.Compare(target, node.Value) > 0 ? node.Right : node.Left;
            }
            return node;
        }
        protected BSNode<T> SearchNode(int index)
        {
            BSNode<T> node = Root;
            last = Root;
            while (node != null)
            {
                int compareResult = index.CompareTo(comparer.GetElementIndex(node.Value));
                if (compareResult == 0) break;
                last = node;
                node = compareResult > 0 ? node.Right : node.Left;
            }
            return node;
        }
        public void ShowOrder() => InOrderVisit(node => GameExtension.Logger.Log($"{node}"));
        public void InOrderVisit(Action<T> visit) => Root.InOrderVisit(visit);
        public void PreOrderVisit(Action<T> visit) => Root.PreOrderVisit(visit);
        protected BSNode<T> SetRoot(BSNode<T> _root)
        {
            Root = _root;
            if (Root != null) Root.Parent = null;
            return Root;
        }
        protected void ReplaceNode(BSNode<T> node, BSNode<T> beReplaced)
        {
            if (beReplaced != Root)
            {
                var parent = beReplaced.Parent;
                if (beReplaced == parent.Left) parent.InsertLeft(node);
                else parent.InsertRight(node);
            }
            else SetRoot(node);
        }
        public void Clear()
        {
            Root = null;
        }
    }
    public interface IBSElementComparer<in T>:IComparer<T>
    {
        int GetElementIndex(T e);
    }
}
