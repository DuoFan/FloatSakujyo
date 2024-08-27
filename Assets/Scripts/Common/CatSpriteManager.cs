using GameExtension;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FloatSakujyo
{
    public class CatSpriteManager : MonoBehaviour,IGameInitializer
    {
        public static CatSpriteManager Instance { get; private set; }

        Sprite[] catSprites;

        public IEnumerator InitializeGame()
        {
            Instance = this;
            var handle = AddressableManager.Instance.LoadAssetsAsync<Sprite>("Cat");
            yield return handle.WaitForCompletion();
            catSprites = handle.Result.ToArray();
        }

        public Sprite GetRandomCatSprite()
        {
            return catSprites[Random.Range(0, catSprites.Length)];
        }
    }
}

