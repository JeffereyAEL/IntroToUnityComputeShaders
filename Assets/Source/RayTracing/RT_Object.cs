using System;
using Source.Utilities;
using UnityEditor.Rendering;
using UnityEngine;

namespace Source.RayTracing
{
    /// <summary>
    /// The component that communicates to RT_Master that this object is to be rendered in the RayTracing.
    /// Exposes editor friendly Material presets to the users
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class RT_Object : MonoBehaviour
    {
        // ENUMS
        /// <summary>
        /// Defines the Unity Editor preset that will be exposed for this RT_Object's material
        /// </summary>
        public enum MaterialType
        {
            Metallic,
            Matte,
            Light
        }

        // STRUCTSs
        // ATTRIBUTES
        /// <summary>
        /// Mesh vertex * localToWorld data
        /// </summary>
        private Vector3[] Vertices;

        /// <summary>
        /// Mesh index data
        /// </summary>
        private int[] Indices;

        /// <summary>
        /// The Axis Aligned Bounding Box of this RT_Object's mesh
        /// </summary>
        private ShaderAABox AABounding;
        
        /// <summary>
        /// This RT_Object's material preset
        /// </summary>
        public MaterialType MatType = MaterialType.Light;

        /// <summary>
        /// Whether this material has been changed since last rendering began
        /// </summary>s
        [NonSerialized] public bool bMaterialChanged = true;
        
        /// <summary>
        /// This RT_object's rendering material
        /// </summary>
        [NonSerialized] public ShaderMaterial Mat;

        // GETTERS_/_SETTERS
        /// <summary>
        /// returns this RT_Object's ShaderMaterial
        /// </summary>
        /// <returns> RT_Object's Material </returns>
        public ShaderMaterial getMaterial()
        {
            return Mat;
        }

        /// <summary>
        /// getter for the axis-aligned bounds of the transformed mesh
        /// </summary>
        public ShaderAABox getBounds()
        {
            return AABounding;
        }

        /// <summary>
        /// getter for the vertex * localToWorld data of the mesh
        /// </summary>
        public Vector3[] getVertices()
        {
            return Vertices;
        }

        /// <summary>
        /// getter for the index data of the mesh
        /// </summary>
        public int[] getIndices()
        {
            return Indices;
        }
        
        // METHODS
        /// <summary>
        /// constructs compute shader friendly mesh data from unity defaults
        /// </summary>
        private void reconstructMeshData()
        {
            var m = GetComponent<MeshFilter>().sharedMesh;
            Indices = m.GetIndices(0);
            Vertices = m.vertices;
            var l2w = transform.localToWorldMatrix;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), 
                max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (var i = 0; i < Vertices.Length; ++i)
            {
                Vertices[i] = l2w.MultiplyPoint(Vertices[i]);
                for (var o = 0; o < 3; ++o)
                {
                    if (Vertices[i][o] < min[o])
                        min[o] = Vertices[i][o];
                    if (Vertices[i][o] > max[o])
                        max[o] = Vertices[i][o];
                }
            }
            AABounding = new ShaderAABox{Max = max, Min = min, Ref = 0};
            
            transform.hasChanged = false;
        }
        
        // EVENTS
        private void Start()
        {
            reconstructMeshData();
        }

        private void OnEnable()
        {
            RT_Master.registerObject(this);
        }

        private void OnDisable()
        {
            RT_Master.unregisterObject(this);
        }

        private void Update()
        {
            if (!transform.hasChanged && !bMaterialChanged) return;
            RT_Master.unregisterObject(this);
            
            if (transform.hasChanged)
                reconstructMeshData();
            
            RT_Master.registerObject(this);
        } 
        
    }
}