using UnityEngine;
using UnityEngine.UI;

namespace GameExtension
{
    public class OffsetFlow : MonoBehaviour
    {
        [SerializeField]
        private float speed;

        [SerializeField]
        private Vector2 direction;

        [SerializeField]
        private GameObject target;

        private Material mat;

        private void Update()
        {
            if (mat == null)
            {
                if (target == null)
                {
                    target = gameObject;
                }

                if (target.GetComponent<Renderer>() != null)
                {
                    mat = target.GetComponent<Renderer>().material;
                }
                else if (target.GetComponent<Image>() != null)
                {
                    mat = target.GetComponent<Image>().material;
                }
                else if (target.GetComponent<RawImage>() != null)
                {
                    mat = target.GetComponent<RawImage>().material;
                }

                if(mat != null)
                {
                    mat = new Material(mat);
                    if(target.GetComponent<Renderer>() != null)
                    {
                        target.GetComponent<Renderer>().material = mat;
                    }
                    else if(target.GetComponent<Image>() != null)
                    {
                        target.GetComponent<Image>().material = mat;
                    }
                    else if(target.GetComponent<RawImage>() != null)
                    {
                        target.GetComponent<RawImage>().material = mat;
                    }
                }
            }

            if(mat != null)
            {
                mat.mainTextureOffset += direction * speed * Time.deltaTime;
            }
        }
    }
}
