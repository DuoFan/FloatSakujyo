using System;

namespace GameExtension
{
    public class AVL<T> : BSTree<T>
    {
        public AVL(T _root, IBSElementComparer<T> _comparer) : base(_root, _comparer) { }
        bool IsBalancedNode(BSNode<T> node) =>
            Math.Abs(node.GetLeftHeight() - node.GetRightHeight()) < 2;
        protected override BSNode<T> InsertNode(T e)
        {
            var result = base.InsertNode(e);
            var node = result;
            while (node != null)
            {
                node.UpdateHeight();
                if (!IsBalancedNode(node))
                {
                    SloveUnbalance(node);
                    break;
                }
                node = node.Parent;
            }
            return result;
        }
        void SloveUnbalance(BSNode<T> node)
        {
            var grand = node;
            var parent = grand.IsLeftHigher() ? grand.Left : grand.Right;
            node = parent.IsLeftHigher() ? parent.Left : parent.Right;
            BSNode<T> A, B, C, T1, T2, T3, T4;
            bool isNodeLess = comparer.Compare(node.Value, parent.Value) < 0; ;
            if (comparer.Compare(parent.Value, grand.Value) < 0)
            {
                /*                C              C
                 *              B   T4  ||     A   T4
                 *            A   T3        T1   B
                 *         T1   T2            T2   T3
                 */
                C = grand;
                A = isNodeLess ? node : parent;
                B = isNodeLess ? parent : node;
                T2 = isNodeLess ? A.Right : B.Left;
                T3 = B.Right;
            }
            else
            {
                /*           A                 A      
                 *        T1    B      ||  T1    C
                 *           T2    C           B   T4
                 *              T3   T4     T2   T3
                 */
                A = grand;
                B = isNodeLess ? node : parent;
                C = isNodeLess ? parent : node;
                T2 = B.Left;
                T3 = isNodeLess ? B.Right : C.Left;
            }
            T1 = A.Left;
            T4 = C.Right;
            ReplaceNode(B, grand);
            Rotate34(T1, A, T2, B, T3, C, T4);
        }

        /*         B
               A       C
            T1  T2  T3  T4
        */
        void Rotate34(BSNode<T> T1, BSNode<T> A, BSNode<T> T2,
            BSNode<T> B, BSNode<T> T3, BSNode<T> C, BSNode<T> T4)
        {
            A.InsertLeft(T1);
            A.InsertRight(T2);
            A.UpdateHeight();
            C.InsertLeft(T3);
            C.InsertRight(T4);
            C.UpdateHeight();
            B.InsertLeft(A);
            B.InsertRight(C);
            B.UpdateHeight();
        }
        protected override BSNode<T> RemoveNode(T e)
        {
            var result = base.RemoveNode(e);
            var node = result == null ? null : last;
            while (node != null)
            {
                if (!IsBalancedNode(node))
                    SloveUnbalance(node);
                node = node.Parent;
            }
            return result;
        }
    }

}