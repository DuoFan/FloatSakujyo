using GameExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public abstract class CreateViewBase<TData, TBaseView> : EditorView<TData>
        where TData : IConfigData
        where TBaseView : EditorViewBase<TData>
    {
        protected override string NameSentence => "创建面板";
        protected IEditorWindowBase baseWindow;
        protected TBaseView baseView;

        public void SetBaseWindow(IEditorWindowBase _window)
        {
            baseWindow = _window;
            baseWindow.OnSaveNewData += Init;
        }

        public void SetBaseView(TBaseView baseView)
        {
            this.baseView = baseView;
        }

        public override void Draw()
        {
            baseView.Draw();
            if (GUILayout.Button("确认创建"))
            {
                Save();
            }
        }
        public override void Update()
        {
            base.Update();
            baseView.Update();
        }

        public override void Set(TData obj)
        {
            baseView.Set(obj);
        }
        public override TData Save()
        {
            var data = baseView.Save();
            baseWindow.SaveNewData(data);
            return data;
        }

        public override void Dispose()
        {
            baseView.Dispose();
        }
    }
}
