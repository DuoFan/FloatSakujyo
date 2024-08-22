using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace EditorExtension
{
    public class ObjectSelector
    {
        protected const int INDEX_NONE = -1;
        public int index = INDEX_NONE;
        protected Dictionary<string, object> optionMap = new Dictionary<string, object>();
        protected List<string> options = new List<string>();
        string[] displayOptions;
        public Action<string> onOptionChange;
        int lastIndex = -1;
        string filter;
        string lastFilter;
        string name = string.Empty;
        ObjectOptionProvider optionProvider;

        public void SetOptionProvider(ObjectOptionProvider _optionProvider)
        {
            optionProvider = _optionProvider;
            optionProvider.OnOptionsUpdated += SetOptions;
        }

        public void SetName(string _name)
        {
            name = _name;
        }

        public void AddOption(string option, object obj = null)
        {
            options.Add(option);
            optionMap[option] = obj;
            UpdateDisplayOptions();
        }

        public void SetOptions(List<string> _options, Dictionary<string, object> _objectMap)
        {
            options.Clear();
            optionMap.Clear();
            options.AddRange(_options);
            foreach (var item in _objectMap)
            {
                optionMap[item.Key] = item.Value;
            }
            displayOptions = options.ToArray();
        }

        public void Draw()
        {
            if (displayOptions == null)
            {
                EditorGUILayout.HelpBox("没有任何搜索项", MessageType.Warning);
                return;
            }
            EditorGUILayout.BeginVertical();
            filter = EditorGUILayout.TextField($"{(string.IsNullOrEmpty(name) ? string.Empty : $"{name}:")}" +
                                               $"搜索(支持正则表达式)", filter);
            if (filter != lastFilter)
            {
                UpdateDisplayOptions();
                lastFilter = filter;

                //如果只有一个符合项,则修改当前选择项
                if (displayOptions.Length == 1)
                {
                    index = 0;
                    lastIndex = index;
                    onOptionChange?.Invoke(displayOptions[index]);
                }
            }
            index = EditorGUILayout.Popup(index, displayOptions);
            if (lastIndex != index)
            {
                lastIndex = index;
                onOptionChange?.Invoke(displayOptions[index]);
            }
            EditorGUILayout.EndVertical();
        }

        public void SetIndex(int _index)
        {
            if (index != _index)
            {
                index = _index;
                lastIndex = index;
                onOptionChange?.Invoke(displayOptions[index]);
            }
        }

        public void SetIndex(Func<object, bool> prediction)
        {
            if (prediction != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (optionMap[options[i]] != null)
                    {
                        if (prediction(optionMap[options[i]]))
                        {
                            SetIndex(i);
                            break;
                        }
                    }
                }
            }
        }

        public void SetIndexNone()
        {
            index = INDEX_NONE;
            lastIndex = INDEX_NONE;
        }

        public int GetOptionIndex(string option)
        {
            return Array.IndexOf(displayOptions, option);
        }

        public string GetOption() => displayOptions[index];

        public T GetObject<T>()
        {
            return (T)optionMap[displayOptions[index]];
        }

        public bool IsSelected()
        {
            return index != INDEX_NONE;
        }

        void UpdateDisplayOptions()
        {
            if (!string.IsNullOrEmpty(filter))
            {
                try
                {
                    Regex regex = new Regex(filter, RegexOptions.IgnoreCase);
                    displayOptions = options.Where(x => regex.IsMatch(x)).ToArray();
                }
                catch (Exception)
                {
                }
            }
            else
            {
                displayOptions = options.ToArray();
            }
        }
    }

    public class ObjectSelector<T> : ObjectSelector
    {
        public T GetObject()
        {
            return GetObject<T>();
        }

        public void SetIndex(Func<T, bool> prediction)
        {
            SetIndexNone();
            if (prediction != null)
            {
                for (int i = 0; i < options.Count; i++)
                {
                    if (optionMap[options[i]] != null)
                    {
                        if (prediction((T)optionMap[options[i]]))
                        {
                            SetIndex(i);
                            break;
                        }
                    }
                }
            }
        }
    }

    public class OptionProvider
    {
        public event Action<string[]> OnOptionsUpdated;

        private string[] options;

        public string[] Options
        {
            get => options;
            private set
            {
                options = value;
                OnOptionsUpdated?.Invoke(options);
            }
        }

        public void SetOptions(string[] newOptions)
        {
            Options = newOptions;
        }

        public void AddOption(string newOption)
        {
            List<string> tempList = new List<string>(options);
            tempList.Add(newOption);
            Options = tempList.ToArray();
        }

        public void RemoveOption(string optionToRemove)
        {
            List<string> tempList = new List<string>(options);
            tempList.Remove(optionToRemove);
            Options = tempList.ToArray();
        }
    }

    public class ObjectOptionProvider
    {
        public event Action<List<string>, Dictionary<string, object>> OnOptionsUpdated;

        public List<string> Options { get; private set; } = new List<string>();
        public Dictionary<string, object> OptionMap { get; private set; } = new Dictionary<string, object>();

        public void AddOption(string option, object obj = null)
        {
            if (!Options.Contains(option))
            {
                Options.Add(option);
            }
            OptionMap[option] = obj;
            OnOptionsUpdated?.Invoke(Options, OptionMap);
        }

        public void RemoveOption(string optionToRemove)
        {
            Options.Remove(optionToRemove);
            OptionMap.Remove(optionToRemove);
            OnOptionsUpdated?.Invoke(Options, OptionMap);
        }
    }
}