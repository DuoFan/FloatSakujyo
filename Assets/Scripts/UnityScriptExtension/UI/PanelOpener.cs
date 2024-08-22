using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    [RequireComponent(typeof(Button))]
    public class PanelOpener : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] float duration = 0.25f;
        public PanelEventHandler eventHandler;
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OpenPanel);
        }

        public virtual void OpenPanel()
        {
            if(panel != null)
            {
                UIManager.Instance.OpenPanel(panel, duration, eventHandler);
            }
        }
    }
}
