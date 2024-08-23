using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class ItemGrid
    {
        public Vector2 Position { get; private set; }
        public Item Item { get; private set; }

        public ItemGrid(Vector2 position)
        {
            Position = position;
        }

        public void PutItem(Item item)
        {
            if(item.ItemGeneration == ItemGeneration.Eden)
            {
                if(Item == null)
                {
                    Item = item;
                }
                else
                {
                    throw new System.Exception("尝试将Item放置到一个已满的格子中");
                }
            }
        }
        public void ReleaseItem()
        {
            Item = null;
        }
    }
}

