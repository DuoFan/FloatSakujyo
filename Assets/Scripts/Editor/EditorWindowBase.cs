using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public abstract partial class EditorWindowBase<TData>
    {
        protected partial Dictionary<Type, ConfigDataManagerInitContext> ProvideFileConfigPathes()
        {
            return null;
        }
        protected partial Dictionary<Type, ConfigDataManagerInitContext> ProvideDirectoryConfigPathes()
        {
            return null;
        }
        protected partial Dictionary<Type, SheetDataManagerInitContext> ProvideSheetDataPathes()
        {
            return null;
        }
    }
}
