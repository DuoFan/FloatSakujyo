using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExtension
{
    public struct Area
    {
        public int areaId;
        public Vector2[] points;
        public Vector2? Center
        {
            get
            {
                if (center == null)
                {
                    Vector2 sum = Vector2.zero;
                    if (points == null || points.Length == 0)
                    {
                        return null;
                    }

                    for (int i = 0; i < points.Length; i++)
                    {
                        sum += points[i];
                    }
                    center = sum / points.Length;
                }
                return center;
            }
        }
        Vector2? center;
        public Area(int areaId, Vector2[] points, Vector2? center = null)
        {
            this.areaId = areaId;
            this.points = points;
            this.center = center;
        }
    }
}