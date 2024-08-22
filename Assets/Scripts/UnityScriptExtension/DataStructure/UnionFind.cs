using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class UnionFind
    {
        private int[] parent;
        private int[] size;

        public UnionFind(int n)
        {
            SetCapacity(n);
        }

        public int Find(int x)
        {
            int root = x;
            while (parent[root] != root)
            {
                root = parent[root];
            }

            // 路径压缩
            int current = x;
            int parentOfCurrent;
            while (current != root)
            {
                parentOfCurrent = parent[current];
                parent[current] = root;
                current = parentOfCurrent;
            }

            return root;
        }


        public void Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);

            if (rootX == rootY) return;

            if (size[rootX] >= size[rootY])
            {
                parent[rootY] = rootX;
                size[rootX] += size[rootY];
            }
            else
            {
                parent[rootX] = rootY;
                size[rootY] += size[rootX];
            }
        }

        public int MaxGroupSize()
        {
            int maxSize = 0;
            for (int i = 0; i < size.Length; i++)
            {
                maxSize = Math.Max(maxSize, size[i]);
            }
            return maxSize;
        }

        public Dictionary<int, List<int>> GetAllGroups()
        {
            Dictionary<int, List<int>> groups = new Dictionary<int, List<int>>();

            for (int i = 0; i < parent.Length; i++)
            {
                int root = Find(i);

                if (!groups.ContainsKey(root))
                {
                    groups[root] = new List<int>();
                }

                groups[root].Add(i);
            }

            return groups;
        }

        public List<int> GetMaxGroup()
        {
            var groups = GetAllGroups();
            int maxSize = 0;
            List<int> maxGroup = null;
            foreach (var group in groups)
            {
                if(group.Value.Count > maxSize)
                {
                    maxSize = group.Value.Count;
                    maxGroup = group.Value;
                }
            }
            return maxGroup;
        }

        public void SetCapacity(int n)
        {
            parent = new int[n];
            size = new int[n];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
                size[i] = 1;
            }
        }

    }


}
