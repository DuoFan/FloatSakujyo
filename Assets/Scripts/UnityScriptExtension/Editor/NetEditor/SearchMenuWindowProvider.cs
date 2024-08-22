using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace EditorExtension
{
    public class SearchMenuWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        NodeContentType[] nodeContentTypes;
        public void Set(NodeContentType[] nodeContentTypes)
        {
            this.nodeContentTypes = nodeContentTypes;
            Array.Sort(this.nodeContentTypes, (x, y) => x.type.CompareTo(y.type));
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("创建节点")));

            for (var i = 0; i < nodeContentTypes.Length; i++)
            {
                var contentType = nodeContentTypes[i];
                var title = $"[{contentType.type}]:{contentType.contentName}";
                entries.Add(new SearchTreeEntry(new GUIContent(title)) { level = 1, userData = contentType });
            }

            return entries;
        }


        public delegate bool SerchMenuWindowOnSelectEntryDelegate(SearchTreeEntry searchTreeEntry, SearchWindowContext context);

        public SerchMenuWindowOnSelectEntryDelegate OnSelectEntryHandler;

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(searchTreeEntry, context);
        }
    }
}
