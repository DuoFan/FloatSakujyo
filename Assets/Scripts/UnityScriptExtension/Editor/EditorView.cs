using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public abstract class EditorView<T>:IDraw,ISetter<T>,ISave<T>,IDisposable
    {

        protected Editor<T> window;
        protected T beEdit;
        protected abstract string NameSentence { get; }
        protected string name;
        public abstract void Init();
        public abstract void Draw();
        public virtual void Update()
        {

        }

        public void SetWindow(Editor<T> window)
        {
            this.window = window;
        }
        public abstract void Set(T obj);
        public abstract T Save();
        public abstract void Dispose();
        public void AddMessage(string key,string message,MessageType messageType)
        {
            window.AddMessage(key, message, messageType);
        }
        public void DismissMessage(string key)
        {
            window.DismissMessage(key);
        }
        protected virtual void DrawName()
        {
            name = EditorGUILayout.TextField(NameSentence, name);
        }
    }
}
