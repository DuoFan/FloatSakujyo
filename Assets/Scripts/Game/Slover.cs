using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace FloatSakujyo.Game
{
    public class Slover : MonoBehaviour
    {
        [SerializeField]
        float waterLevel;
        [SerializeField]
        private float buoyancyForce;

        [SerializeField]
        float waveFrequency;
        [SerializeField]
        float waveAmplitude;

        List<Rigidbody> rigidbodies;

        Vector3[] vertices = new Vector3[8];

        public void SetBuoyancyForce(float force)
        {
            buoyancyForce = force;
        }

        public void AddRigidbody(Rigidbody rigidbody)
        {
            if(rigidbodies == null)
            {
                rigidbodies = new List<Rigidbody>();
            }
            rigidbodies.Add(rigidbody);
            rigidbody.transform.SetParent(transform);
        }

        public void RemoveRigidbody(Rigidbody rigidbody)
        {
            if (rigidbodies == null)
            {
                return;
            }
            rigidbodies.Remove(rigidbody);
        }

        private void FixedUpdate()
        {
            if(rigidbodies == null)
            {
                return;
            }

            var waterLevel = CalculateWaterLevel();
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                var rigidbody = rigidbodies[i];

                var mesh = rigidbody.GetComponent<MeshFilter>().sharedMesh;
                var bounds = mesh.bounds;
                
                vertices[0] = rigidbody.transform.TransformPoint(bounds.min);
                vertices[1] = rigidbody.transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
                vertices[2] = rigidbody.transform.TransformPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
                vertices[3] = rigidbody.transform.TransformPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
                vertices[4] = rigidbody.transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
                vertices[5] = rigidbody.transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
                vertices[6] = rigidbody.transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
                vertices[7] = rigidbody.transform.TransformPoint(bounds.max);

                float minY = vertices.Min(v => v.y);
                float maxY = vertices.Max(v => v.y);

                if (minY < waterLevel)
                {
                    float submergedPercentage = (waterLevel - minY) / (maxY - minY);
                    rigidbody.AddForce(Vector3.up * buoyancyForce * Mathf.Clamp01(submergedPercentage));
                }
            }
        }

        public float CalculateWaterLevel()
        {
            return waterLevel + Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        }
    }
}

