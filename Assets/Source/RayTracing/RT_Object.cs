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
        /// <summary>
        /// Defines the Unity Editor preset that will be exposed for this RT_Object's material
        /// </summary>
        public enum MaterialType
        {
            Metallic,
            Matte,
            Light
        }

        /// <summary>
        /// This RT_Object's material preset
        /// </summary>
        public MaterialType MatType = MaterialType.Light;

        [HideInInspector] public Vector3 Albedo;
        [HideInInspector] public Vector3 Specular;
        [HideInInspector] public Vector3 Emissive;
        [HideInInspector] public float Roughness;
        
        /// <summary>
        /// Whether this material has been changed since last rendering began
        /// </summary>s
        [NonSerialized] public bool bMaterialChanged = true;
        
        /// <summary>
        /// This RT_object's rendering material
        /// </summary>
        public ShaderMaterial Mat;
        
        /// <summary>
        /// Whether this RT_Object's material has been initialized (prevents dirty rendering)
        /// </summary>
        private bool bUninitialized = true;

        /// <summary>
        /// The Axis Aligned Bounding Box of this RT_Object's mesh
        /// </summary>
        private Bounds AABounding;
        
        private void Awake()
        {
            bUninitialized = true;
            
        }
        
        private void Start()
        {
            if (!bUninitialized) return;
            Mat.Albedo = Vector3.one;
            Mat.Specular = Vector3.zero;
            Mat.Emissive = Vector3.zero;
            Mat.Roughness = 0.5f;
            bUninitialized = false;
        }

        private void OnEnable()
        {
            initData();
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

            initData();

            RT_Master.registerObject(this);
            transform.hasChanged = false;
        }

        private void initData()
        {
            AABounding = GetComponent<MeshFilter>().sharedMesh.bounds;
            Mat = new ShaderMaterial
            {
                Albedo = Albedo,
                Emissive = Emissive,
                Roughness = Roughness,
                Specular = Specular
            };
            bMaterialChanged = false;
        }
        
        /// <summary>
        /// returns this RT_Object's ShaderMaterial
        /// </summary>
        /// <returns> RT_Object's Material </returns>
        public ShaderMaterial getMaterial()
        {
            return Mat;
        }
    }
}