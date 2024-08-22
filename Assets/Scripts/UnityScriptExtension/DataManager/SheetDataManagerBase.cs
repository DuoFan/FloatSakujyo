using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameExtension
{
    public abstract class SheetDataManagerBase
    {
        public abstract void Init(ES3Spreadsheet spreadsheet);

        public abstract IEnumerator IEInit(ES3Spreadsheet spreadsheet);
    }
    public abstract class SheetDataManagerBase<T> : SheetDataManagerBase where T : IConfigData
    {
        public static SheetDataManagerBase<T> Instance { get; protected set; }
        protected Dictionary<int, T> datas;

        public override void Init(ES3Spreadsheet spreadsheet)
        {
            Instance = this;
            datas = new Dictionary<int, T>();
            for (int i = 0; i < spreadsheet.RowCount; i++)
            {
                if (i == spreadsheet.RowCount - 1 && i != 0)
                {
                    var lastRowLength = spreadsheet.GetRowLength(i);
                    if (lastRowLength <= 1)
                    {
                        var test = spreadsheet.GetCell<string>(0, i);
                        if (string.IsNullOrEmpty(test))
                        {
                            break;
                        }
                    }
                }
                var obj = ParseSheetData(spreadsheet, i);
                if (obj != null && !obj.Equals(default))
                {
                    datas[obj.ID] = obj;
                }
            }
        }

        public override IEnumerator IEInit(ES3Spreadsheet spreadsheet)
        {
            Instance = this;
            datas = new Dictionary<int, T>();
            for (int i = 0; i < spreadsheet.RowCount; i++)
            {
                if (i == spreadsheet.RowCount - 1 && i != 0)
                {
                    var lastRowLength = spreadsheet.GetRowLength(i);
                    if (lastRowLength <= 1)
                    {
                        var test = spreadsheet.GetCell<string>(0, i);
                        if (string.IsNullOrEmpty(test))
                        {
                            break;
                        }
                    }
                }
                var obj = ParseSheetData(spreadsheet, i);
                if (obj != null && !obj.Equals(default))
                {
                    datas[obj.ID] = obj;
                    yield return null;
                }
            }
        }
        public T GetData(int id)
        {
            if (datas.TryGetValue(id, out T result))
            {
                return result;
            }
            else
            {
                var error = $"{nameof(SheetDataManagerBase<T>)}:不存在ID为{id}的配置数据";
                GameExtension.Logger.Error(error);
                throw new System.Exception(error);
            }
        }


        public T[] GetDatas()
        {
            var result = new T[datas.Count];
            datas.Values.CopyTo(result, 0);
            return result;
        }

        public bool TryGetData(int id, out T data)
        {
            return datas.TryGetValue(id, out data);
        }

        protected abstract T ParseSheetData(ES3Spreadsheet spreadsheet, int row);
    }
}
