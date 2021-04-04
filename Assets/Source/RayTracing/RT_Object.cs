using System;
using System.Numerics;
using Source.Utilities;
using UnityEditor.Rendering;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
        // STRUCTSs
        // ATTRIBUTES
        [HideInInspector] public Vector3 Albedo;
        [HideInInspector] public Vector3 Specular;
        [HideInInspector] public Vector3 Emissive;
        [HideInInspector] public float Roughness;
        
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
        /// Whether this material has been changed since last rendering began
        /// </summary>s
        [NonSerialized] public bool bMaterialChanged = true;

        /// <summary>
        /// This RT_object's rendering material
        /// </summary>
        private ShaderMaterial Mat;

        private MeshFilter MeshFilter;
        
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
            var m = MeshFilter.sharedMesh;
            Indices = m.GetIndices(0);
            Vertices = m.vertices;
            var local_to_world = transform.localToWorldMatrix;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), 
                max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (var i = 0; i < Vertices.Length; ++i)
            {
                Vertices[i] = local_to_world.MultiplyPoint(Vertices[i]);
                for (var o = 0; o < 3; ++o)
                {
                    if (Vertices[i][o] < min[o])
                        min[o] = Vertices[i][o];
                    if (Vertices[i][o] > max[o])
                        max[o] = Vertices[i][o];
                }
            }
            AABounding = new ShaderAABox{Max = max, Min = min};
            
            transform.hasChanged = false;
        }

        private void reconstructMaterialData()
        {
            print("reconstructing material data");
            Mat = new ShaderMaterial{Albedo = Albedo, Specular = Specular, Emissive = Emissive, Roughness = Roughness};
            bMaterialChanged = false;
        }
        
        // EVENTS
        private void Awake()
        {
            MeshFilter = GetComponent<MeshFilter>();
            bMaterialChanged = true;
        }

        private void OnEnable()
        {
            reconstructMeshData();
            reconstructMaterialData();
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
            if (bMaterialChanged)
                reconstructMaterialData();
            
            RT_Master.registerObject(this);
        } 
        
    }
}