using GameExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public abstract class EditorViewBase<TData> : EditorView<TData> where TData : IConfigData
    {
        protected override string NameSentence => "";
        public int DataID => originalData.ID;
        public IEditorWindowBase baseWindow;
        protected TData originalData;
        public void SetBaseWindow(IEditorWindowBase window)
        {
            baseWindow = window;
        }
        public override void Set(TData obj)
        {
            originalData = obj;
        }
        public abstract override TData Save();
        public override void Dispose()
        {

        }
    }
}
