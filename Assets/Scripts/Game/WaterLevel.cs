using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class WaterLevel : MonoBehaviour
    {
        [SerializeField]
        Transform waveParent;

        [SerializeField]
        ParticleSystem splash;

        public void Init(float waveInterval)
        {
            StartCoroutine(QueueToPlayWave(waveInterval));
        }

        IEnumerator QueueToPlayWave(float waveInterval)
        {
            var waves = new List<ParticleSystem>(waveParent.GetComponentsInChildren<ParticleSystem>());
            while (waves.Count > 0)
            {
                var index = Random.Range(0, waves.Count);
                var wave = waves[index];
                wave.Play();    
                waves.RemoveAt(index);
                yield return new WaitForSecondsRealtime(waveInterval);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var rigidBody = other.GetComponent<Rigidbody>();
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        public void PlaySplash(Vector3 position)
        {
            splash.transform.position = position;
            splash.Play();
        }
    }
}

