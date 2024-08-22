using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    public interface IIconAddressHolder
    {
        string IconAddress { get; }

        public static LoadAssetHandle<Sprite> LoadIconForSpriteRenderer<T>(T data, SpriteRenderer renderer, bool isCheckShow = true) where T : IIconAddressHolder
        {
            return LoadIconForSpriteRenderer(data.IconAddress, renderer, isCheckShow);
        }

        public static LoadAssetHandle<Sprite> LoadIconForSpriteRenderer(string iconAddress, SpriteRenderer renderer, bool isCheckShow = true)
        {
            if(renderer == null)
            {
                GameExtension.Logger.Log($"加载{iconAddress}时SpriteRenderer为空");
                return null;
            }
            var handler = AddressableManager.Instance.LoadAssetAsync<Sprite>(iconAddress.Trim());
            handler.Completed += (s) =>
            {
                if (renderer == null)
                {
                    GameExtension.Logger.Log($"加载{iconAddress}后SpriteRenderer可能已被销毁");
                    return;
                }
                renderer.sprite = s;
                if (isCheckShow)
                {
                    renderer.gameObject.CheckActiveSelf(true);
                    if (!renderer.enabled)
                    {
                        renderer.enabled = true;
                    }
                }
            };
            return handler;
        }

        public static LoadAssetHandle<Sprite> LoadIconForImage<T>(T data, Image image, bool isNatieSize = true, bool isCheckShow = true) where T : IIconAddressHolder
        {
            return LoadIconForImage(data.IconAddress, image, isNatieSize, isCheckShow);
        }

        public static LoadAssetHandle<Sprite> LoadIconForImage(string iconAddress, Image image, bool isNatieSize = true, bool isCheckShow = true)
        {
            if(image == null)
            {
                GameExtension.Logger.Log($"加载{iconAddress}时Image为空");
                return null;
            }
            var handler = AddressableManager.Instance.LoadAssetAsync<Sprite>(iconAddress.Trim());
            handler.Completed += (s) =>
            {
                if (image == null)
                {
                    GameExtension.Logger.Log($"加载{iconAddress}后Image可能已被销毁");
                    return;
                }
                image.sprite = s;
                if (isNatieSize)
                {
                    image.SetNativeSize();
                }
                if (isCheckShow)
                {
                    image.gameObject.CheckActiveSelf(true);
                    if (!image.enabled)
                    {
                        image.enabled = true;
                    }
                }
            };
            return handler;
        }
    }
}
