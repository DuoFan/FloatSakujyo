using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class BTreeNode<T> where T : IEqualityComparer<T>
    {
        public BTreeNode<T> Parent
        {
            get; internal set;
        }
        public List<Key> Keys
        {
            get; private set;
        }
        public class Key : IEqualityComparer<Key>
        {
            public T Value
            {
                get; internal set;
            }
            public Key(T _value) => Value = _value;

            public bool Equals(Key x, Key y)
            {
                return x.Value.Equals(y.Value);
            }

            public int GetHashCode(Key obj)
            {
                return obj.Value.GetHashCode();
            }
        }
        public List<BTreeNode<T>> Children
        {
            get; private set;
        }
        public BTreeNode()
        {
            Keys = new List<Key>();
            Children = new List<BTreeNode<T>>();
        }
        public BTreeNode(T _value) : this()
        {
            InsertValue(_value);
        }
        public BTreeNode<T> FindChild(int index)
        {
            if (index >= Children.Count || index < 0) return null;
            else return Children[index];
        }
        internal void InsertChild(int index, BTreeNode<T> node)
        {
            if (node == null) return;
            Children.Insert(index, node);
            node.Parent = this;
        }
        internal BTreeNode<T> RemoveChild(int index)
        {
            var node = FindChild(index);
            if (node != null)
            {
                node.Parent = null;
                Children.RemoveAt(index);
            }
            return node;
        }
        internal Key FindKey(T e)
        {
            foreach (var key in Keys)
            {
                if (key.Value.Equals(e)) return key;
            }
            return null;
        }
        internal void RemoveKey(Key key)
        {
            Keys.Remove(key);
        }
        internal bool FindValue(int index, out T result)
        {
            result = default;
            if (index < Keys.Count)
            {
                result = Keys[index].Value;
                return true;
            }
            else return false;
        }
        internal int InsertValue(T e)
        {
            int index;
            for (index = 0; index < Keys.Count; index++)
            {
                var key = Keys[index];
                if (key.Value.GetHashCode(key.Value) >
                    e.GetHashCode(e))
                {
                    break;
                }
            }
            Keys.Insert(index, new Key(e));
            return index;
        }
        internal T RemoveValue(int index)
        {
            var value = Keys[index].Value;
            Keys.RemoveAt(index);
            return value;
        }
        internal BTreeNode<T> FindSucc(int index) => FindChild(index + 1);

        internal BTreeNode<T> FindSucc(T e)
        {
            BTreeNode<T> succ = null;
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Value.Equals(e))
                {
                    succ = FindSucc(i);
                    if (succ != null)
                    {
                        while (succ.Children.Count > 0)
                            succ = succ.Children[0];
                    }
                    break;
                }
            }
            return succ;
        }

        public void InOrderVisit(Action<T> visit)
        {
            int index = 0;
            int childIndex = 0;
            BTreeNode<T> child;
            while (index < Keys.Count)
            {
                child = FindChild(childIndex);
                if (child != null && child.Keys[0].Value.GetHashCode(child.Keys[0].Value) 
                    < Keys[index].Value.GetHashCode(Keys[index].Value))
                {
                    child.InOrderVisit(visit);
                    childIndex++;
                }
                visit(Keys[index].Value);
                index++;
            }
            child = FindChild(childIndex);
            child?.InOrderVisit(visit);
        }
    }
    public class BTree<T> where T : IEqualityComparer<T>
    {
        public BTreeNode<T> Root
        {
            get; private set;
        }
        BTreeNode<T> last;
        int rank;
        public BTree(T _root, int _rank)
        {
            Root = last = new BTreeNode<T>(_root);
            rank = _rank;
        }
        public BTreeNode<T> Search(T target)
        {
            var node = Root;
            last = Root;
            while (node != null)
            {
                int index = 0;
                foreach (var key in node.Keys)
                {
                    if (key.Value.GetHashCode(key.Value) > target.GetHashCode(target)) break;
                    else if (key.Value.Equals(target)) goto END;
                    index++;
                }
                last = node;
                node = node.FindSucc(index - 1);
            }
        END:
            return node;
        }
        public void Insert(T e)
        {
            var node = Search(e);

            if (node == null)
            {
                if (last != null)
                {
                    last.InsertValue(e);
                    if (IsOverFlow(last)) SolveOverFlow(last);
                }
                else
                {
                    node = new BTreeNode<T>(e);
                    SetRoot(node);
                }
            }
        }
        bool IsOverFlow(BTreeNode<T> node) => node.Keys.Count >= rank;
        void SolveOverFlow(BTreeNode<T> node)
        {
            while (node != null)
            {
                int index = Mathf.CeilToInt(rank / 2);
                T value = node.RemoveValue(index);
                var another = new BTreeNode<T>();

                int cur = 0;
                while (node.Keys.Count > index)
                {
                    another.InsertChild(cur, node.RemoveChild(index + 1));
                    another.InsertValue(node.RemoveValue(index));
                    cur++;
                }
                another.InsertChild(cur, node.RemoveChild(index + 1));

                var parent = node.Parent;
                if (parent != null)
                {
                    index = parent.InsertValue(value);
                    parent.InsertChild(index + 1, another);
                }
                else
                {
                    parent = node.Parent = new BTreeNode<T>(value);
                    parent.InsertChild(0, node);
                    parent.InsertChild(1, another);
                    SetRoot(parent);
                }

                node = IsOverFlow(parent) ? parent : null;
            }
        }
        public void Remove(T e)
        {
            var node = Search(e);
            if (node != null) RemoveNodeValue(node, e);
        }
        void RemoveNodeValue(BTreeNode<T> node, T e)
        {
            BTreeNode<T>.Key key = node.FindKey(e);
            while (true)
            {
                var succ = node.FindSucc(e);
                if (succ == null) break;
                key.Value = succ.Keys[0].Value;
                node = succ;
                key = succ.Keys[0];
                e = key.Value;
            }
            node.RemoveKey(key);
            if (IsUnderFlow(node)) SolveUnderFlow(node);
        }
        bool IsUnderFlow(BTreeNode<T> node) => node.Keys.Count < Mathf.Ceil(rank / 2f) - 1 && Root != node;
        void SolveUnderFlow(BTreeNode<T> node)
        {
            bool IsNull_Or_IsDanger(BTreeNode<T> _node) => _node == null ||
                _node.Keys.Count == Mathf.Ceil(rank / 2f) - 1;

            var parent = node.Parent;
            int index = parent.Children.IndexOf(node);

            BTreeNode<T> bro = parent.FindChild(index - 1);
            if (!IsNull_Or_IsDanger(bro))
            {   //左顾
                BorrowLeft(bro, parent.Keys[index - 1], node);
            }
            else if (!IsNull_Or_IsDanger(bro = parent.FindChild(index + 1)))
            {   //右盼
                BorrowRight(node, parent.Keys[index], bro);
            }
            else
            {   //上并
                MergeUp(node, index);
            }
        }
        void BorrowLeft(BTreeNode<T> bro, BTreeNode<T>.Key parentKey, BTreeNode<T> node)
        {
            node.InsertValue(parentKey.Value);
            parentKey.Value = bro.Keys[bro.Keys.Count - 1].Value;
            bro.RemoveValue(bro.Keys.Count - 1);
        }
        void BorrowRight(BTreeNode<T> node, BTreeNode<T>.Key parentKey, BTreeNode<T> bro)
        {
            node.InsertValue(parentKey.Value);
            parentKey.Value = bro.Keys[0].Value;
            bro.RemoveValue(0);
        }
        void MergeUp(BTreeNode<T> node, int childIndex)
        {
            var parent = node.Parent;
            parent.RemoveChild(childIndex);
            foreach (var key in node.Keys)
            {
                parent.InsertValue(key.Value);
                if (IsOverFlow(parent)) SolveOverFlow(parent);
            }
        }
        BTreeNode<T> SetRoot(BTreeNode<T> _root)
        {
            Root = _root;
            if (Root != null) Root.Parent = null;
            return Root;
        }
    }
}