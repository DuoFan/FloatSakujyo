using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public class EditorLogger : IDraw
    {
        Dictionary<MessageType, List<EditorMessage>> messages;
        public EditorLogger() 
        {
            messages = new Dictionary<MessageType, List<EditorMessage>>();
            for (int i = (int)MessageType.None; i <= (int)MessageType.Error; i++) 
            {
                var type = (MessageType)i;
                messages[type] = new List<EditorMessage>();
            }
        }
        public void AddMessage(string _tag,string _message,MessageType messageType)
        {
            EditorMessage message = new EditorMessage()
            {
                tag = _tag,
                message = _message
            };
            messages[messageType].Add(message);
        }

        public void DismissMessage(string _tag)
        {
            for (int i = (int)MessageType.None; i <= (int)MessageType.Error; i++)
            {
                var type = (MessageType)i;
                var messageList = messages[type];
                for (int j = messageList.Count - 1; j >= 0; j--)
                {
                    if (messageList[j].tag == _tag)
                    {
                        messageList.RemoveAt(j);    
                    }
                }
            }
        }

        public void Draw()
        {
            for (int i = (int)MessageType.None; i <= (int)MessageType.Error; i++)
            {
                var type = (MessageType)i;
                var messageList = messages[type];
                for (int j = 0; j < messageList.Count; j++)
                {
                    EditorGUILayout.HelpBox(messageList[j].message, type);
                }
            }
        }

        struct EditorMessage
        {
            public string tag;
            public string message;
        }
    }
}

