using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class RBTree<T> : BSTree<T>
    {
        public class RBNode : BSNode<T>
        {
            internal bool IsRed
            {
                get; private set;
            }
            public RBNode(T _value) : base(_value)
                => IsRed = true;
            internal void SetRed() => IsRed = true;
            internal void SetBlack() => IsRed = false;
            internal bool IsDoubleRed()
            {
                if (Parent != null) return (Parent as RBNode).IsRed && IsRed;
                else return false;
            }
        }
        public RBTree(T _root, IBSElementComparer<T> _comparer) : base(_root, _comparer)
        {
            Root = last = new RBNode(_root);
            (Root as RBNode).SetBlack();
        }
        protected override BSNode<T> InsertNode(T e)
        {
            BSNode<T> node = SearchNode(e);
            if (node == null)
            {
                node = new RBNode(e);
                if (last == null) SetRoot(node as RBNode);
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
                        var error = $"RBTree插入失败,重复的键值:{e}:{last}";
                        GameExtension.Logger.Exception(error);
                        throw new System.Exception(error);
                    }
                }
                if ((node as RBNode).IsDoubleRed())
                    SolveDoubleRed((RBNode)node);
                node = Root;
            }
            return node;
        }
        void SolveDoubleRed(RBNode node)
        {
            while (node != null)
            {
                var parent = node.Parent as RBNode;
                var grand = parent.Parent as RBNode;
                var uncle = (parent == grand.Left ? grand.Right : grand.Left) as RBNode;

                if (uncle == null || !uncle.IsRed)
                {
                    grand.SetRed();
                    if ((node == parent.Left) == (parent == grand.Left))
                    {
                        parent.SetBlack();
                        ReplaceNode(parent, grand);
                        if (node == parent.Left)
                        {
                            grand.InsertLeft(parent.Right);
                            parent.InsertRight(grand);
                        }
                        else
                        {
                            grand.InsertRight(parent.Left);
                            parent.InsertLeft(grand);
                        }
                        grand = parent;
                    }
                    else
                    {
                        node.SetBlack();
                        ReplaceNode(node, grand);
                        if (parent == grand.Left)
                        {
                            parent.InsertRight(node.Left);
                            node.InsertLeft(parent);
                            grand.InsertLeft(node.Right);
                            node.InsertRight(grand);
                        }
                        else
                        {
                            parent.InsertLeft(node.Right);
                            node.InsertRight(parent);
                            grand.InsertRight(node.Left);
                            node.InsertLeft(grand);
                        }
                        grand = node;
                    }
                }
                else
                {
                    if (grand != Root)
                        grand.SetRed();
                    parent.SetBlack();
                    uncle.SetBlack();
                }
                node = grand.IsDoubleRed() ? grand : null;
            }
        }
        protected override BSNode<T> RemoveNode(T e)
        {
            BSNode<T> node = RemoveNode(e);
            if (node != null)
            {
                while (true)
                {
                    var succ = node.GetSucc();
                    if (succ == null) break;
                    node.Value = succ.Value;
                    node = succ;
                    last = node.Parent;
                }
                if ((node as RBNode).IsRed || node.Right != null)
                    ReplaceNode(node.Right, node);
                else
                {
                    /*
                     *        R(last)         ||      B(last)
                     *    B       B(node)         B       B(node)
                     * ... ...                 ... ...    
                     */
                    var parent = last as RBNode;
                    RBNode bro = parent.Left as RBNode;
                    bro = bro.Right == null ? bro.Left as RBNode : bro.Right as RBNode;
                    if (bro != null)
                    {
                        /*
                         *        R/B(last)               R/B(last)
                         *    B        B(node)  =>     B        B(node)
                         * R1   R2                  R1
                         */
                        node.Value = parent.Value;
                        parent.Value = bro.Parent.Value;
                        bro.Parent.Value = bro.Value;
                        ReplaceNode(null, bro);
                    }
                    else
                    {
                        parent.InsertRight(null);
                        if (parent.IsRed)
                        {
                            /*
                             *        R(last)        =>        B(last)
                             *    B       B(node)           R     null(node)
                             */
                            (parent.Left as RBNode).SetRed();
                            parent.SetBlack();
                        }
                        else
                        {

                        }
                    }
                }
            }
            return node;
        }
        void SetRoot(RBNode _root)
        {
            base.SetRoot(_root);
            _root.SetBlack();
        }
    }
}
