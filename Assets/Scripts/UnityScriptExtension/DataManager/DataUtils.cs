using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameExtension
{
    public static partial class DataUtils
    {
        public static T GetRandomData<T>(T[] datas, RareData rare) where T : IRareHolder
        {
            List<int> index = new List<int>();
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].Rare == rare)
                {
                    index.Add(i);
                }
            }
            if (index.Count == 0)
            {
                return default;
            }
            return datas[index[UnityEngine.Random.Range(0, index.Count)]];
        }

        public static T FindData<T>(IEnumerable<T> elements, int id) where T : IIDProvider
        {
            foreach (var element in elements)
            {
                if (element.ID == id)
                {
                    return element;
                }
            }
            return default;
        }

        public static Parent FindParentData<Parent, Child>(IEnumerable<Parent> parents, Child child)
            where Child : IParentIndexData
            where Parent : IIDProvider
        {
            return FindData(parents, child.ParentIndex);
        }

        public static void SortByPriority<T>(T[] datas) where T : IPrioirtyData
        {
            System.Array.Sort(datas, PriorityComparision);
        }

        public static void ForceSortByPriority<T>(T[] datas)
        {
            System.Array.Sort(datas, ForcePriorityComparision);
        }

        public static void SortByPriority<T>(List<T> datas) where T : IPrioirtyData
        {
            datas.Sort(PriorityComparision);
        }

        static int PriorityComparision<T>(T a, T b) where T : IPrioirtyData
        {
            return b.Priority - a.Priority;
        }

        static int ForcePriorityComparision<T>(T a, T b)
        {
            return ((IPrioirtyData)b).Priority - ((IPrioirtyData)a).Priority;
        }

        public static Dictionary<Parent, Child[]> GetParentChildrenMap<Parent, Child>(IEnumerable<Parent> parents, IEnumerable<Child> children)
            where Parent : IIDProvider
            where Child : IParentIndexData
        {
            Dictionary<int, int> indexChildrenCountMap = new Dictionary<int, int>();
            foreach (var child in children)
            {
                if (!indexChildrenCountMap.ContainsKey(child.ParentIndex))
                {
                    indexChildrenCountMap[child.ParentIndex] = 1;
                }
                else
                {
                    indexChildrenCountMap[child.ParentIndex] += 1;
                }
            }
            Dictionary<Parent, Child[]> parentChildrenMap = new Dictionary<Parent, Child[]>();
            Dictionary<int, Parent> idParentMap = new Dictionary<int, Parent>();
            foreach (var parent in parents)
            {
                if (indexChildrenCountMap.ContainsKey(parent.ID))
                {
                    parentChildrenMap[parent] = new Child[indexChildrenCountMap[parent.ID]];
                    idParentMap[parent.ID] = parent;
                    indexChildrenCountMap[parent.ID] = 0;
                }
                else
                {
                    var error = $"父元素{parent.ID}未被任何子元素索引";
                    GameExtension.Logger.Error(error);
                    throw new Exception(error);
                }
            }
            bool isPriority = false;
            bool isCheckPriority = false;
            foreach (var child in children)
            {
                var parent = idParentMap[child.ParentIndex];
                var array = parentChildrenMap[parent];
                var index = indexChildrenCountMap[child.ParentIndex];
                array[index] = child;
                indexChildrenCountMap[child.ParentIndex] += 1;
                if (!isCheckPriority)
                {
                    isPriority = child is IPrioirtyData;
                    isCheckPriority = true;
                }
            }
            if (isPriority)
            {
                foreach (var array in parentChildrenMap.Values)
                {
                    ForceSortByPriority(array);
                }
            }
            return parentChildrenMap;
        }

        public static T[] ParseSheetData<T>(TextAsset textAsset, Func<ES3Spreadsheet, int, T> parser)
        {
            var spreadSheet = new ES3Spreadsheet();
            spreadSheet.LoadRaw(textAsset.text);
            T[] result = new T[spreadSheet.RowCount];
            for (int i = 0; i < spreadSheet.RowCount; i++)
            {
                result[i] = parser(spreadSheet, i);
            }
            return result;
        }

        /*public static bool TryGetItemAmount(int itemID, int extensionItemType, object target, out int itemAmount)
        {
            itemAmount = 0;
            bool result = false;
            if (target is IExtensionItemCollection itemCollection)
            {
                for (int i = 0; i < itemCollection.Items.Length; i++)
                {
                    var item = itemCollection.Items[i];
                    if (item.ID == itemID && item.ItemType == extensionItemType)
                    {
                        itemAmount += item.Amount;
                        result = true;
                    }
                }
            }

            if (target is IExtensionItemHolder itemHolder)
            {
                var item = itemHolder.Item;
                if (item.ID == itemID && item.ItemType == extensionItemType)
                {
                    itemAmount += item.Amount;
                    result = true;
                }
            }

            return result;
        }*/

        public static void ShuffleData<T>(IList<T> list,int needCount = -1)
        {
            if(needCount <= 0)
            {
                needCount = list.Count;
            }
            else
            {
                needCount = Mathf.Min(needCount, list.Count);
            }
            for (int i = 0; i < needCount; i++)
            {
                var index = UnityEngine.Random.Range(i + 1, list.Count);
                var temp = list[i];
                list[i] = list[index];
                list[index] = temp;
            }
        }
    }
}