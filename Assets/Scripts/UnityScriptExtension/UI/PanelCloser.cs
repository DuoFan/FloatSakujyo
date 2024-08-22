using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    [RequireComponent(typeof(Button))]
    public class PanelCloser : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] float duration = 0.25f;
        protected void Awake()
        {
            GetComponent<Button>().onClick.AddListener(ClosePanel);
        }

        void ClosePanel()
        {
            if(panel == null)
            {
                UIManager.Instance.ClosePanel(duration);
            }
            else
            {
                UIManager.Instance.ClosePanel(panel, duration);
            }
        }
    }
}
