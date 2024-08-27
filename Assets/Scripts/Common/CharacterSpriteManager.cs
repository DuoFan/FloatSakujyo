using GameExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo
{
    public class CharacterSpriteManager : SingletonMonoBase<CharacterSpriteManager>
    {
        [SerializeField]
        Sprite[] numSprites;
        [SerializeField]
        Sprite dotSprite;

        public List<Sprite> GetNumberSprites(float num)
        {
            List<Sprite> sprites = new List<Sprite>();
            int intNum = (int)num;
            if (intNum == 0)
            {
                sprites.Add(numSprites[0]);
            }
            else
            {
                while (intNum > 0)
                {
                    sprites.Add(numSprites[intNum % 10]);
                    intNum /= 10;
                }
            }
            sprites.Reverse();
            intNum = (int)((num - (int)num) * 10);
            if(intNum != 0)
            {
                sprites.Add(dotSprite);
                sprites.Add(numSprites[intNum]);
            }
            return sprites;
        }
    }
}

