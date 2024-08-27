using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatSakujyo.UI
{
    public class ItemNeedView : MonoBehaviour
    {
        [SerializeField]
        Image avatar;
        public Image Avatar => avatar;

        [SerializeField]
        GameObject view;
        public GameObject View => view;

        [SerializeField]
        Transform itemPlace;
        public Transform ItemPlace => itemPlace;
    }
}

