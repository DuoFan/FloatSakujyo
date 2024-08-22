using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public interface IRareHolder
    {
        public RareData Rare { get; }
    }

    [System.Serializable]
    public struct RareData
    {
        public static RareData None => new RareData()
        {
            rareName = "无",
            rareColor = Color.white,
            rareValue = 0
        };  

        [SerializeField]
        string rareName;
        public string RareName => rareName;

        [SerializeField]
        Color rareColor;
        public Color RareColor => rareColor;

        [SerializeField]
        int rareValue;
        public int RareValue => rareValue;

        public override bool Equals(object obj)
        {
            if (obj is RareData data)
            {
                return data.RareName == rareName && data.rareValue == rareValue;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return rareName.GetHashCode() ^ rareValue.GetHashCode();
        }

        public static bool operator == (RareData a, RareData b)
        {
            return a.Equals(b);
        }
        public static bool operator != (RareData a, RareData b)
        {
            return !a.Equals(b);
        }
    }
}

