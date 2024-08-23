using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class WaterLevel : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var rigidBody = other.GetComponent<Rigidbody>();
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }
    }
}

