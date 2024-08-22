using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorExtension
{
    public class CodeFragment : IDraw, ISetter<string>, ISave<string>
    {
        public string Code { get; private set; }

        public virtual void Draw()
        {
            Code = EditorGUILayout.TextField("Inject Code:", Code);
        }

        public string Save()
        {
            return Code;
        }

        public void Set(string obj)
        {
            Code = obj;
        }
    }
}
