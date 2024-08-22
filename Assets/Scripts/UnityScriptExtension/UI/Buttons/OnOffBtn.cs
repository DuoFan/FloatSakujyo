using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static UnityEngine.UI.Button;

namespace GameExtension
{
    [RequireComponent(typeof(Button))]
    public class OnOffBtn : MonoBehaviour
    {
        public Button Button { get; private set; }
        [SerializeField] Image target;
        [SerializeField] Sprite offSprite;
        [SerializeField] Sprite onSprite;

        [SerializeField] bool isON;
        public bool IsOn => isON;

        public Action onSetOn;
        public Action onSetOff;
        public Action<bool> onSwitch;

        protected virtual void Awake()
        {
            Button = GetComponent<Button>();
            target.sprite = isON ? onSprite : offSprite;
            Button.onClick.AddListener(Switch);
        }

        public void Switch()
        {
            if (isON)
            {
                SetOff();
            }
            else
            {
                SetOn();
            }
            onSwitch?.Invoke(IsOn);
        }

        public void SetOn()
        {
            if (!isON)
            {
                isON = true;
                target.sprite = onSprite;
                onSetOn?.Invoke();
            }
        }
        public void SetOff()
        {
            if (isON)
            {
                isON = false;
                target.sprite = offSprite;
                onSetOff?.Invoke();
            }
        }
    }
}
