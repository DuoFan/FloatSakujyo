using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class Splay<T> : BSTree<T>
    {
        public Splay(T _root, IBSElementComparer<T> _comparer) : base(_root, _comparer) { }
        protected override BSNode<T> SearchNode(T target)
        {
            var result = base.SearchNode(target);
            var node = result ?? last;
            while (node != Root)
            {
                var parent = node.Parent;
                var grand = parent.Parent;
                if (grand != null)
                {
                    if (node == parent.Left)
                    {
                        if (parent == grand.Left)
                        {
                            Zig(parent);
                            Zig(node);
                        }
                        else
                        {
                            Zig(node);
                            Zag(node);
                        }
                    }
                    else
                    {
                        if (parent == grand.Right)
                        {
                            Zag(parent);
                            Zag(node);
                        }
                        else
                        {
                            Zag(node);
                            Zig(node);
                        }
                    }
                }
                else
                {
                    if (node == parent.Left) Zig(node);
                    else Zag(node);
                }
            }
            return result;
        }
        protected override BSNode<T> InsertNode(T e)
        {
            var node = SearchNode(e);
            if (node == null)
            {
                node = new BSNode<T>(e);
                if (Root != null)
                {
                    int compareResult = comparer.Compare(e, Root.Value);    
                    if (compareResult > 0)
                    {
                        node.InsertLeft(Root);
                        node.InsertRight(Root.Right);
                        Root.Right = null;
                    }
                    else if(compareResult < 0)
                    {
                        node.InsertRight(Root);
                        node.InsertLeft(Root.Left);
                        Root.Left = null;
                    }
                    else 
                    {
                        var error = $"Splay插入失败,重复的键值:{e}:{last}";
                        GameExtension.Logger.Exception(error);
                        throw new System.Exception(error);
                    }
                }
                SetRoot(node);
            }
            return node;
        }
        void Zig(BSNode<T> node)
        {
            var parent = node.Parent;
            parent.InsertLeft(node.Right);
            ReplaceNode(node, parent);
            node.InsertRight(parent);
        }

        void Zag(BSNode<T> node)
        {
            var parent = node.Parent;
            parent.InsertRight(node.Left);
            ReplaceNode(node, parent);
            node.InsertLeft(parent);
        }
    }
}
