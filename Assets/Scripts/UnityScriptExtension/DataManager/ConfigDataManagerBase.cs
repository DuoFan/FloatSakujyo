using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace GameExtension
{
    public abstract class ConfigDataManagerBase
    {
        public abstract void Init(object[] _datas);
    }
    public class ConfigDataManagerBase<T>: ConfigDataManagerBase where T : IConfigData
    {
        public static ConfigDataManagerBase<T> Instance { get; private set; }
        protected SortedList<int, T> datas;
        public override void Init(object[] _datas)
        {
            try
            {
                Instance = this;
                datas = new SortedList<int, T>();
                if (_datas != null)
                {
                    for (int i = 0; i < _datas.Length; i++)
                    {
                        var data = (T)_datas[i];
                        datas[data.ID] = data;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        public T GetData(int id)
        {
            return datas[id];
        }
        public T[] GetDatas()
        {
            return datas.Values.ToArray();
        }

        public bool TryGetData(int id, out T data)
        {
            return datas.TryGetValue(id, out data);
        }
    }
}

