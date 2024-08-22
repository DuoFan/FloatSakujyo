using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SDKExtension
{
    public class SystemIOAdapter : IIOAdapter
    {
        public bool Delete(string path)
        {
            if (IsExists(path))
            {
                File.Delete(path);
                GameExtension.Logger.Log($"已删除{path}");
                return true;
            }
            else
            {
                GameExtension.Logger.Log($"不存在{path}");
                return false;
            }
        }

        public bool IsExists(string path)
        {
            bool isExist = File.Exists(path);
            GameExtension.Logger.Log($"{path}是否存在:{isExist}");
            return isExist;
        }

        public IEnumerator ReadBytes(string path, Action<byte[]> success)
        {
            var data = File.ReadAllBytes(path);
            success?.Invoke(data);
            yield break;
        }

        public IEnumerator ReadText(string path, Action<string> success)
        {
            var data = File.ReadAllText(path);
            success?.Invoke(data);
            yield break;
        }

        public void Write(string path, byte[] data)
        {
            using (var write = File.Create(path))
            {
                write.Write(data);
                write.Flush();
            }
        }

        public void Write(string path, string data)
        {
            using (var write = File.CreateText(path))
            {
                write.Write(data);
                write.Flush();
            }
        }
    }
}
