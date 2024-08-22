using GameExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public abstract class EditViewBase<TData, TBaseView> : EditorView<TData>
        where TData : IConfigData
        where TBaseView : EditorViewBase<TData>
    {
        protected override string NameSentence => "编辑面板";
        protected IEditorWindowBase baseWindow;
        protected TBaseView baseView;
        public void SetBaseView(TBaseView baseView)
        {
            this.baseView = baseView;
        }

        protected ObjectSelector dataSelector;
        public void SetBaseWindow(IEditorWindowBase _window)
        {
            baseWindow = _window;
            baseWindow.OnSaveNewData += Init;
            baseWindow.OnDeleteData += Init;
            baseWindow.OnEditData += Init;
        }
        public override void Draw()
        {
            if (dataSelector == null)
            {
                Init();
            }

            if (dataSelector.index < 0)
            {
                EditorGUILayout.HelpBox("没有任何可编辑的内容", MessageType.Warning);
                return;
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("选择编辑内容", MessageType.Info);
            dataSelector.Draw();
            GUILayout.EndHorizontal();

            baseView.Draw();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("删除"))
            {
                Delete();
            }
            else if (GUILayout.Button("确认编辑"))
            {
                Save();
            }
            GUILayout.EndHorizontal();
        }
        public override void Update()
        {
            base.Update();
            baseView.Update();
        }
        public virtual void Delete()
        {
            baseWindow.DeleteData<TData>(baseView.DataID);
        }
        public override void Init()
        {
            var datas = baseWindow.GetDatas<TData>();
            List<string> options = new List<string>();
            Dictionary<string, object> dataDic = new Dictionary<string, object>();
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                options.Add(GetDataOption(data));
                dataDic.Add(options[i], data);
            }
            if(dataSelector == null)
            {
                dataSelector = new ObjectSelector();
            }
            dataSelector.SetOptions(options, dataDic);
            dataSelector.onOptionChange = EditData;
            
            if (datas.Length > 0)
            {
                if(dataSelector.index < 0 || dataSelector.index >= datas.Length)
                {
                    dataSelector.SetIndex(0);
                }
                else
                {
                    var oldIndex = dataSelector.index;
                    dataSelector.SetIndexNone();
                    dataSelector.SetIndex(oldIndex);
                }
            }
            else
            {
                dataSelector.SetIndexNone();
            }
        }
        public override void Set(TData obj)
        {
            baseView.Set(obj);
        }
        public override TData Save()
        {
            var data = baseView.Save();
            baseWindow.EditData(data);
            return data;
        }
        public override void Dispose()
        {
            baseView.Dispose();
        }
        protected abstract string GetDataOption(TData data);
        protected void EditData(string option)
        {
            var data = baseWindow.GetDataByIndex<TData>(dataSelector.index);
            Set(data);
        }
    }
}
