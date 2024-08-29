using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FloatSakujyo.Game
{
    public class Slover : MonoBehaviour
    {
        [SerializeField]
        float waterLevel;
        [SerializeField]
        Transform waterLevelTransform;

        [SerializeField]
        private float buoyancyForce;

        [SerializeField]
        float waveFrequency;
        [SerializeField]
        float waveAmplitude;

        [SerializeField]
        List<Rigidbody> rigidbodies = new List<Rigidbody>();
        List<Mesh> meshes = new List<Mesh>();

        public void SetBuoyancyForce(float force)
        {
            buoyancyForce = force;
        }

        public void AddRigidbody(Rigidbody rigidbody)
        {
            rigidbodies.Add(rigidbody);
            meshes.Add(rigidbody.GetComponent<MeshFilter>().sharedMesh);
        }

        public void RemoveRigidbody(Rigidbody rigidbody)
        {
            var index = rigidbodies.IndexOf(rigidbody);
            rigidbodies.RemoveAt(index);
            meshes.RemoveAt(index);
        }

        public void ClearRigidbodys()
        {
            rigidbodies.Clear();
            meshes.Clear();
        }

        private void FixedUpdate()
        {
            if(rigidbodies == null)
            {
                return;
            }

            var waterLevel = CalculateWaterLevel();
            waterLevelTransform.transform.position = new Vector3(waterLevelTransform.position.x, waterLevel, waterLevelTransform.position.z);

            float minY;
            float maxY;

            void CheckMin_Max_Y(Rigidbody rigidbody,Vector3 vertice)
            {
                var worldVertice = rigidbody.transform.TransformPoint(vertice);
                if (worldVertice.y < minY)
                {
                    minY = worldVertice.y;
                }
                if (worldVertice.y > maxY)
                {
                    maxY = worldVertice.y;
                }
            }

            for (int i = 0; i < rigidbodies.Count; i++)
            {
                var rigidbody = rigidbodies[i];

                minY = float.MaxValue;
                maxY = float.MinValue;

                var mesh = meshes[i];
                var bounds = mesh.bounds;

                CheckMin_Max_Y(rigidbody, bounds.min);
                CheckMin_Max_Y(rigidbody, new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
                CheckMin_Max_Y(rigidbody, new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
                CheckMin_Max_Y(rigidbody, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
                CheckMin_Max_Y(rigidbody, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
                CheckMin_Max_Y(rigidbody, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
                CheckMin_Max_Y(rigidbody, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
                CheckMin_Max_Y(rigidbody, bounds.max);

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

