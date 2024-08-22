using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKExtension
{
    public class IOAdapter : IIOAdapter
    {
        public static IOAdapter Instance
        {
            get
            {
                if(instance == null)
                {
                    throw new Exception("IOAdapter未初始化");
                }
                return instance;
            }
        }
        static IOAdapter instance;
        public static void Init(IIOAdapter _io)
        {
            if (instance == null)
            {
                instance = new IOAdapter();
                instance.io = _io;
            }
        }
        IIOAdapter io;

        public bool IsExists(string path)
        {
            return io.IsExists(path);
        }

        public void Write(string path, byte[] data)
        {
            io.Write(path, data);
        }

        public void Write(string path, string data)
        {
            io.Write(path, data);
        }

        public IEnumerator ReadBytes(string path, Action<byte[]> success)
        {
            yield return io.ReadBytes(path, success);
        }
        public IEnumerator ReadText(string path, Action<string> success)
        {
            yield return io.ReadText(path, success);
        }

        public bool Delete(string path)
        {
            return io.Delete(path);
        }
    }
}
