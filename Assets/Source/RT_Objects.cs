using Source.Utilities;
using UnityEngine;

namespace Source.RayTracing
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class RayTracingObject : MonoBehaviour
    {
        public ShaderMaterial Mat;

        private readonly Mesh UnityMesh;

        public RayTracingObject()
        {
            UnityMesh = GetComponent<MeshFilter>().sharedMesh;
        }

        private void OnEnable()
        {
            RayTracing.registerObject(this);
        }

        private void OnDisable()
        {
            RayTracing.unregisterObject(this);
        }
        
        public Mesh getMesh() {
            return UnityMesh; 
        }
    }
}