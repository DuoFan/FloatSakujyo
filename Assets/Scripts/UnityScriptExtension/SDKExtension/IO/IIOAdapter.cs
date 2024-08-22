using DG.Tweening.Plugins.Core.PathCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDKExtension
{
    public interface IIOAdapter
    {
        public bool IsExists(string path);
        public void Write(string path, byte[] data);
        public void Write(string path, string text);
        public IEnumerator ReadBytes(string path, Action<byte[]> success);
        public IEnumerator ReadText(string path, Action<string> success);
        public bool Delete(string path);
    }

}