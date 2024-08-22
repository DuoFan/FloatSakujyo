using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;

namespace EditorExtension
{
    public class ObjectDisplayer
    {
        protected List<IDraw> draws = new List<IDraw>();
        protected IDraw[] displayObjects;
        string filter;
        string lastFilter;
        string name = string.Empty;
        protected Comparison<IDraw> comparison;
        protected Func<IDraw, Regex, bool> filterFunc;
        public ObjectDisplayer(Func<IDraw, Regex, bool> _filterFunc, Comparison<IDraw> _comparison = null)
        {
            filterFunc = _filterFunc;
            comparison = _comparison;
        }
        public void SetName(string _name)
        {
            name = _name;
        }
        public void AddObject(IDraw draw)
        {
            draws.Add(draw);
            if (comparison != null)
            {
                draws.Sort(comparison);
            }
            UpdateDisplayOptions();
        }
        public void SetObjects(IEnumerable<IDraw> _draws)
        {
            foreach (var item in _draws)
            {
                draws.Add(item);
            }
            if (comparison != null)
            {
                draws.Sort(comparison);
            }
            UpdateDisplayOptions();
        }
        public void Draw()
        {
            if (draws == null)
            {
                EditorGUILayout.HelpBox("没有任何展示项", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical();

            filter = EditorGUILayout.TextField($"{(string.IsNullOrEmpty(name) ? string.Empty : $"{name}:")}" +
                $"搜索(支持正则表达式)", filter);
            if (filter != lastFilter)
            {
                UpdateDisplayOptions();
                lastFilter = filter;
            }
            for (int i = 0; i < displayObjects.Length; i++)
            {
                displayObjects[i].Draw();
            }

            EditorGUILayout.EndVertical();
        }
        void UpdateDisplayOptions()
        {
            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    Regex regex = new Regex(filter,RegexOptions.IgnoreCase);
                    displayObjects = draws.Where(x => filterFunc(x, regex)).ToArray();
                }
                catch (Exception)
                {

                }
            }
            else
            {
                displayObjects = draws.ToArray();
            }
        }

        public void Foreach(Action<IDraw> action)
        {
            foreach (var item in draws)
            {
                action(item);
            }
        }

        public void ForeachFilter(Action<IDraw> action)
        {
            foreach (var item in displayObjects)
            {
                action(item);
            }
        }
    }
    public class ObjectDisplayer<T> : ObjectDisplayer where T : IDraw
    {
        public ObjectDisplayer(Func<T, Regex, bool> _filterFunc, Comparison<T> _comparison = null) :base((x, y) => _filterFunc((T)x, y))
        {
            if(_comparison != null)
            {
                comparison = (x, y) => _comparison((T)x, (T)y);
            }
        }
        public void Foreach(Action<T> action)
        {
            foreach (var item in draws)
            {
                action((T)item);
            }
        }
        public void ForeachFilter(Action<T> action)
        {
            foreach (var item in displayObjects)
            {
                action((T)item);
            }
        }
    }
}