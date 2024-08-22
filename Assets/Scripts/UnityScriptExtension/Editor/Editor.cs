using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorExtension
{
    public enum State
    {
        Create, Edit
    }
    public abstract class Editor<T> : EditorWindow
    {
        Vector2 scrollPos;
        public State state;
        protected EditorView<T> createView;
        protected EditorView<T> editView;

        protected EditorLogger logger;

        public void AddMessage(string tag, string message, MessageType messageType)
        {
            logger.AddMessage(tag, message, messageType);
        }
        public void DismissMessage(string tag)
        {
            logger.DismissMessage(tag);
        }
        public abstract EditorView<T> GetCreateView();
        public abstract EditorView<T> GetEditView();


        protected virtual void OnEnable()
        {
            logger = new EditorLogger();
        }

        protected virtual void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(900));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("创建"))
            {
                state = State.Create;
            }
            else if (GUILayout.Button("编辑"))
            {
                state = State.Edit;
            }
            EditorGUILayout.EndHorizontal();
            if (state == State.Create)
            {
                if (createView == null)
                    createView = GetCreateView();
                createView.Draw();
            }
            else if (state == State.Edit)
            {
                if (editView == null)
                    editView = GetEditView();
                editView.Draw();
            }

            logger.Draw();

            GUILayout.EndScrollView();
        }
        protected virtual void OnDestroy()
        {
            createView?.Dispose();
            editView?.Dispose();
        }

        protected virtual void Update()
        {
            switch (state)
            {
                case State.Create:
                    createView?.Update();
                    break;
                case State.Edit:
                    editView?.Update();
                    break;
            }
        }
    }
}

