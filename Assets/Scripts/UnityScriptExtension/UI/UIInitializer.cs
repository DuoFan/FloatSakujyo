using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public class UIInitializer : MonoBehaviour
    {
        [SerializeField] GameObject prototype;
        public GameObject Prototype => prototype;   
        public GameObject InitUI()
        {
            var go = GameObject.Instantiate(prototype, transform.parent);
            var initUI = go.GetComponent(typeof(IInitUI)) as IInitUI;
            initUI?.InitUI();
            var siblingIndex = transform.GetSiblingIndex();
            go.transform.SetSiblingIndex(siblingIndex);
            GameObject.Destroy(gameObject);
            return go;
        }
    }

    public interface IInitUI
    {
        void InitUI();
    }

}
