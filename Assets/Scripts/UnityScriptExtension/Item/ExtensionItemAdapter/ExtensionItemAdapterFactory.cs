using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public partial class ExtensionItemAdapterFactory
    {
        static ExtensionItemAdapterFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExtensionItemAdapterFactory();
                    instance.extensionItemAdapters = new Dictionary<int, IExtensionItemAdapter>();
                    instance.RegisterExtensionItemAdapter(ExtensionItemType.ITEM, new ExtensionItemAdapter());
                    instance.Init();
                }
                return instance;
            }
        }
        static ExtensionItemAdapterFactory instance;
        Dictionary<int, IExtensionItemAdapter> extensionItemAdapters;
        partial void Init();
        void RegisterExtensionItemAdapter(int type, IExtensionItemAdapter adapter)
        {
            extensionItemAdapters[type] = adapter;
        }

        public static IExtensionItemAdapter GetExtensionItemAdapter(int type)
        {
            if (!Instance.extensionItemAdapters.TryGetValue(type, out var adapter))
            {
                var error = $"未找到与{type}匹配的ExtensionItemAdapter";
                GameExtension.Logger.Error(error);
                throw new System.Exception(error);
            }
            return adapter;
        }
    }
}

