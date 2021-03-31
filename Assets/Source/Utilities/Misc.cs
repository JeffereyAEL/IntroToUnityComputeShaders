using UnityEngine;

namespace Source.Utilities
{
    public static class Misc
    {
        /// <summary>
        /// Converts a Color into a Vector3 by dropping the alpha
        /// </summary>
        /// <param name="C"> the color to drop the alpha from </param>
        /// <returns> A Vector3(c.r, c.g, r.b) </returns>
        public static Vector3 _ToRGB(Color C)
        {
            return new Vector3(C.r, C.g, C.b);
        }

        /// <summary>
        /// Converts a Vector3 into a Color by appending an alpha
        /// </summary>
        /// <param name="V"> The Vector to append an alpha to </param>
        /// <returns> A Color(v.x, v.y, v.z, 1.0f) </returns>
        public static Color _ToRGBA(Vector3 V)
        {
            return new Color(V.x, V.y, V.z, 1.0f);
        }
    }
    
    
}