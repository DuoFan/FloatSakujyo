using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorExtension
{
    public interface IManipulators<T>
    {
        List<Manipulator<T>> Manipulators { get; set; }
        void AddManipulator(Manipulator<T> manipulator);
    }
    public abstract class Manipulator<T> : ISetter<T>, ISave<T>, IDraw
    {
        protected T effect;
        public virtual void Set(T obj)
        {
            effect = obj;
        }
        public virtual T Save() => effect;
        public abstract void Draw();
    }
}
