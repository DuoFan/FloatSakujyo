using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorExtension
{
    public abstract class MultiManager<T1, T2> : IDraw, ISetter<IEnumerable<T2>>, ISave<List<T2>>
        where T1 : IDraw, ISave<T2>, ISetter<T2>
    {
        public List<T1> managers = new List<T1>();
        protected Vector2 pos;
        public virtual void Draw()
        {
            pos = EditorGUILayout.BeginScrollView(pos, GUILayout.Height(100));
            for (int i = 0; i < managers.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                managers[i].Draw();
                if (GUILayout.Button("删除该项"))
                {
                    managers.Remove(managers[i]);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            BeforeAdd();
            if (GUILayout.Button("添加"))
            {
                managers.Add(AddNewManager());
            }
        }
        protected abstract void BeforeAdd();
        protected abstract T1 AddNewManager();
        protected abstract T1 AddNewManager(T2 obj);
        public virtual void Set(IEnumerable<T2> beSets)
        {
            managers = new List<T1>();
            if (beSets != null)
            {
                foreach (var beSet in beSets)
                {
                    managers.Add(AddNewManager(beSet));
                }
            }
        }
        public virtual List<T2> Save()
        {
            List<T2> results = new List<T2>();
            foreach (var manager in managers)
            {
                results.Add(manager.Save());
            }
            return results;
        }
        public void Remove(T1 obj) => managers.Remove(obj);
        public void Add(T1 obj)
        {
            if (!managers.Contains(obj))
                managers.Add(obj);
        }
    }
}

