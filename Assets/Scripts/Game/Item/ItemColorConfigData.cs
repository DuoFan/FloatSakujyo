using UnityEngine;

namespace FloatSakujyo.Game
{
    [System.Serializable]
    public class ItemColorConfigData
    {
        [SerializeField]
        ItemColor itemColor;
        public ItemColor ItemColor => itemColor;

        [SerializeField]
        Item item;
        public Item Item => item;

        [SerializeField]
        Texture boxTexture;
        public Texture BoxTexture => boxTexture;

        [SerializeField]
        Texture coverTexture;
        public Texture CoverTexture => coverTexture;

    }
}

