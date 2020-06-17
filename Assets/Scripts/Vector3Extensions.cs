using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace VSB
{
    public static class Vector3Extensions
    {
        public static Vector3 With(this Vector3 self, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x.HasValue ? x.Value : self.x,
                y.HasValue ? y.Value : self.y,
                z.HasValue ? z.Value : self.z
                );
        }

        public static Vector3 Add(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(
                vector3.x + (x.HasValue ? x.Value : 0f),
                vector3.y + (y.HasValue ? y.Value : 0f),
                vector3.z + (z.HasValue ? z.Value : 0f));
        }
        
        public static float DistanceSqrTo(this Vector3 fromPoint, Vector3 toPoint)
        {
            return (fromPoint.x - toPoint.x) * (fromPoint.x - toPoint.x) +
                (fromPoint.y - toPoint.y) * (fromPoint.y - toPoint.y) +
                (fromPoint.z - toPoint.z) * (fromPoint.z - toPoint.z);
        }

    }
}