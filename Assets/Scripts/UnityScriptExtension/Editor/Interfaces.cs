using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorExtension
{
    public interface IDraw
    {
        void Draw();
    }
    public interface IDraw<T>
    {
        void Draw(T input);
    }
    public interface ISetter<T>
    {
        void Set(T input);
    }
    public interface ISave<T>
    {
        T Save();
    }
    public interface IDescription
    {
        string Description { get; }
    }
    public interface IBuild<T>
    {
        string Build();
    }
    public interface IDynamicValue<T>
    {
        public T Value { get; }
        public T LastValue { get; }
        public event Action<T> OnValueChange;
    }
    public interface IIDProvider
    {
        public int ID { get; set; }
    }
}
