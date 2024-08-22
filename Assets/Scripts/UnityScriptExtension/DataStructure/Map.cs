using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameExtension
{
    public class Map<T> : ICloneable
    {
        public int Row
        {
            get;
            private set;
        }
        public int Col
        {
            get;
            private set;
        }
        T[,] items;
        public T this[int row, int col]
        {
            get => items[row, col];
            set => items[row, col] = value;
        }
        public Map<T> Set(int _row, int _col)
        {
            Row = _row;
            Col = _col;
            items = new T[Row, Col];
            return this;
        }
        public Map<T> SetValues(T value)
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    items[i, j] = value;
                }
            }
            return this;
        }
        public Map<T> ApplyFunc(Action<T> action)
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    action(items[i, j]);
                }
            }
            return this;
        }
        public int GetCellRow(int index) => index / Col;
        public int GetCellCol(int index) => index % Row;
        public T GetTop(int _row, int _col) => items[_row + 1, _col];
        public T GetBottom(int _row, int _col) => items[_row - 1, _col];
        public T GetRight(int _row, int _col) => items[_row, _col + 1];
        public T GetLeft(int _row, int _col) => items[_row, _col - 1];
        public T GetLeftTop(int _row, int _col) => items[_row + 1, _col - 1];
        public T GetRightTop(int _row, int _col) => items[_row + 1, _col + 1];
        public T GetLeftBottom(int _row, int _col) => items[_row - 1, _col - 1];
        public T GetRightBottom(int _row, int _col) => items[_row - 1, _col + 1];
        public bool IsValidCoordinate(int row, int col) => !(row < 0 || row >= Row || col < 0 || col >= Col);
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    builder.Append($"{items[i, j]} ");
                }
                builder.Append('\n');
            }
            return builder.ToString();
        }
        public object Clone()
        {
            var obj = new Map<T>().Set(Row, Col);
            obj.items = items.Clone() as T[,];
            return obj;
        }
    }
}
