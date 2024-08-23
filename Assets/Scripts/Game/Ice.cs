using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class Ice : MonoBehaviour
    {

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.collider.GetComponent<Item>() != null)
            {
                GetComponent<Rigidbody>().isKinematic = false;
                Destroy(this);
            }
        }
    }
}

