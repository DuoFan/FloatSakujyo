using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IExtensionItemCollection
    {
        ExtensionItemAmountInfo[] Items { get; }
    }
    public interface IExtensionItemHolder
    {
        ExtensionItemAmountInfo Item { get; }
    }
}

