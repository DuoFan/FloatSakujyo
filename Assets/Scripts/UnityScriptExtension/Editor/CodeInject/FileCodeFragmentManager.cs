using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class FileCodeFragmentManager : MultiManager<FileCodeFragment, (string, string)>
    {
        int randomCount = 0;
        public override void Draw()
        {
            pos = EditorGUILayout.BeginScrollView(pos);
            for (int i = managers.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                managers[i].Draw();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            randomCount = EditorGUILayout.IntSlider("随机选择数量", randomCount, 1, managers.Count);
            if (GUILayout.Button("随机选择"))
            {
                RandomSelect();
            }
            if (GUILayout.Button("取消全部"))
            {
                DisableAll();
            }
            if (GUILayout.Button("选择全部"))
            {
                EnableAll();
            }
        }
        protected override FileCodeFragment AddNewManager()
        {
            return null;
        }

        protected override FileCodeFragment AddNewManager((string, string) obj)
        {
            return null;
        }

        protected override void BeforeAdd()
        {

        }
        public List<FileCodeFragment> GetEnableFragments()
        {
            List<FileCodeFragment> enableFragments = new List<FileCodeFragment>();
            for (int i = 0; i < managers.Count; i++)
            {
                if (managers[i].IsEnable)
                {
                    enableFragments.Add(managers[i]);
                }
            }
            return enableFragments;
        }
        void RandomSelect()
        {
            DisableAll();
            List<int> selectableIndex = new List<int>();
            for (int i = 0; i < managers.Count; i++)
            {
                selectableIndex.Add(i);
            }
            int count = 0;
            while (count < randomCount)
            {
                int select = Random.Range(0, selectableIndex.Count);
                int selectIndex = selectableIndex[select];
                selectableIndex.RemoveAt(select);
                managers[selectIndex].Enable();
                count++;
            }
        }
        void DisableAll()
        {
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].Disable();
            }
        }
        void EnableAll()
        {
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].Enable();
            }
        }
    }
}
