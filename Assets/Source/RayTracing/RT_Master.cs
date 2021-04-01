using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Source.Core;
using Source.Utilities;
using Random = UnityEngine.Random;

namespace Source.RayTracing
{
    public class RT_Master : ComputeShaderMaster
    {
        /// The type of lighting being used
        public LightingType LightingMode = LightingType.ChanceDiffSpec;

        /// The scene directional light
        public Light DirectionalLight;

        /// Skybox ref texture
        public Texture SkyboxSrc;

        /// Whether we're sampling from the skybox texture
        [HideInInspector]  public bool bUsingSkybox;

        /// The color of the sky w/o a texture
        [HideInInspector]  public Color SkyColor = Color.black;

        /// Whether to expose advanced lighting editor details
        [HideInInspector]  public bool bAdvancedLighting = false;
        
        /// The amount of times Rays bounce to generate reflections
        [HideInInspector]  public int MaxBounce = 10;

        /// Phong Shading Alpha
        [HideInInspector]  public float PhongAlpha = 15.0f;
        
        /// the number of Spheres to try an generate on Awake
        [HideInInspector]  public bool bExposeSpheres;
        
        /// the number of Spheres to try an generate on Awake
        [HideInInspector]  public int SphereNumMax = 10000;
        
        /// vec2(min_bound, max_bound) for the radius of a sphere
        [HideInInspector] public Vector2 SphereRad = new Vector2(0.3f, 1.5f);
        
        /// The placement radius of a given sphere
        [HideInInspector] public float SpherePlacementRad = 100.0f;

        /// Is Sphere bobbing enabled
        [HideInInspector] public bool bSphereBobbing;
        
        /// Percent of the radius that a sphere will bob
        [HideInInspector] public float MaxRelitiveBob = 3.0f;
        
        /// <summary>
        /// The seed for Unity's Random engine
        /// </summary>
        [HideInInspector] public int RandomSeed;

        /// <summary>
        /// Whether the editor has changed and the scene needs to be re-rendered
        /// </summary>
        [NonSerialized] public bool bSpheresChanged = true;

        /// <summary>
        /// Whether the Lighting has changed and the sampling needs to be restarted
        /// </summary>
        [NonSerialized] public bool bLightingChanged = true;
        
        // PRIVATE
        /// <summary>
        ///  references to all RayTracingObjects in the scene
        /// </summary>
        private static List<RT_Object> RayTracingObjects = new List<RT_Object>();
        
        /// <summary>
        /// Whether the ShaderMesh Buffers need to be rebuilt
        /// </summary>
        private static bool bMeshBuffersDirty;

        private static List<ShaderSphere> Spheres = new List<ShaderSphere>();
        private static List<ShaderMesh> Meshes = new List<ShaderMesh>();
        private static List<Vector3> Vertices = new List<Vector3>();
        private static List<int> Indices = new List<int>();
        
        /// Compute Buffers for shape data
        private ComputeBuffer SphereBuffer;
        private ComputeBuffer MeshBuffer;
        private ComputeBuffer VertexBuffer;
        private ComputeBuffer IndexBuffer;
        
        /// The current sample; used to track anti-aliasing
        private uint CurrentSample;

        /// the material used to smooth edges with anti-aliasing
        private Material MaterialAdditive;

        /// The buff used to accumulation
        private RenderTexture Converged;

        private static readonly int _Sample = Shader.PropertyToID("_Sample");

        /// x = Spheres idx, y = time offset, z = original y, w = radius rel max y offset  
        private List<Vector4> BobbingSpheres = new List<Vector4>(); 
        
        /// <summary>
        /// The types of lighting the ray tracer can compute
        /// </summary>
        public enum LightingType 
        {
            [UsedImplicitly] LambertDiffuse = 1,
            [UsedImplicitly] PhongSpecular,
            [UsedImplicitly] ChanceDiffSpec,

            [UsedImplicitly] LenOfTypes
        };
        
        /// <summary>
        /// (re)instantiates buffers related to RT_Object rendering
        /// </summary>
        private void rebuildMeshObjectBuffers()
        {
            if (!bMeshBuffersDirty) return;
            CurrentSample = 0;
            bMeshBuffersDirty = false;
            
            // clear all lists
            Meshes.Clear();
            Vertices.Clear();
            Indices.Clear();
            
            foreach(var obj in RayTracingObjects)
            {
                var mesh = obj.GetComponent<MeshFilter>().sharedMesh;
                
                // add vertex data
                var vertex_offset = Vertices.Count;
                Vertices.AddRange(mesh.vertices);
                
                // add index data - if the vertex buff wasn't empty before it needs to be offset
                var index_offset = Indices.Count;
                var indices = mesh.GetIndices(0);
                Indices.AddRange(indices.Select(Index => Index + vertex_offset));
                
                // add the object itself
                Meshes.Add(new ShaderMesh()
                {
                    LocalToWorld = obj.transform.localToWorldMatrix,
                    IndicesOffset = index_offset,
                    IndicesCount = indices.Length,
                    Mat =  obj.getMaterial()
                });
            }

            createComputeBuffer(ref MeshBuffer, Meshes, 112);
            createComputeBuffer(ref VertexBuffer, Vertices, 12);
            createComputeBuffer(ref IndexBuffer, Indices, 4);
        }
        
        /// <summary>
        /// Registers a given RT_Object to this script so that the scene can be re-rendered w/ it
        /// </summary>
        /// <param name="Obj"> The object to add to the registry </param>
        public static void registerObject(RT_Object Obj)
        {
            RT_Master.RayTracingObjects.Add(Obj);
            RT_Master.bMeshBuffersDirty = true;
        }
    
        /// <summary>
        /// Unregisters a given RT_Object from this script so that the scene can be re-rendered
        /// </summary>
        /// <param name="Obj"> The object to remove from the registry </param>
        public static void unregisterObject(RT_Object Obj)
        {
            RT_Master.RayTracingObjects.Remove(Obj);
            RT_Master.bMeshBuffersDirty = true;
        }
        
        /// <summary>
        /// Constructor for the random ShaderSphere structs. Removes spheres that would overlap
        /// </summary>
        /// <param name="New_sphere"></param>
        /// <returns> whether the sphere intersects another sphere in list </returns>
        private bool sphereConstructor(ref ShaderSphere New_sphere)
        {
            New_sphere.Rad = SphereRad.x + Random.value * (SphereRad.y - SphereRad.x);
            var pos = Random.insideUnitCircle * SpherePlacementRad;
            New_sphere.Pos = new Vector3(pos.x, New_sphere.Rad, pos.y);

            foreach (var other in Spheres)
            {
                var min_dist = New_sphere.Rad + other.Rad;
                if (Vector3.SqrMagnitude(New_sphere.Pos - other.Pos) < min_dist * min_dist)
                    return true;
            }
            
            // Albedo and Specular color
            var color = Random.ColorHSV();
            var me = Random.value < 0.5f;
            var em = Random.value < 0.25f;
            
            New_sphere.Mat.Albedo = me ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            New_sphere.Mat.Specular = me ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            New_sphere.Mat.Emissive = em ? new Vector3(Random.value, Random.value, Random.value) : Vector3.zero;
            New_sphere.Mat.Roughness = Random.value;
            print("Created a sphere");
            return false;
        }
        protected override void setShaderParameters()
        {
            ComponentComputeShader.SetMatrix("unity_CameraToWorld", RefCam.cameraToWorldMatrix);
            ComponentComputeShader.SetMatrix("_CameraInverseProjection", RefCam.projectionMatrix.inverse);
            ComponentComputeShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
            ComponentComputeShader.SetInt("_MaxBounce", MaxBounce);
            var l = DirectionalLight.transform.forward;
            ComponentComputeShader.SetVector("_DirectionalLight", 
                new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
            ComponentComputeShader.SetFloat("_Seed", Random.value);
            ComponentComputeShader.SetFloat("_PhongAlpha", PhongAlpha);
            ComponentComputeShader.SetTexture(0, "_SkyboxTexture", SkyboxSrc);
            ComponentComputeShader.SetInt("_LightingMode", (int)LightingMode);
            ComponentComputeShader.SetFloat("_UsingSkybox", bUsingSkybox ? 1.0f : 0.0f);
            ComponentComputeShader.SetVector("_SkyColor", SkyColor); 
            setComputeBuffer("_Meshes", MeshBuffer);
            setComputeBuffer("_Vertices", VertexBuffer);
            setComputeBuffer("_Indices", IndexBuffer);
            setComputeBuffer("_Spheres", SphereBuffer);
        }

        protected override void OnRenderImage(RenderTexture Src, RenderTexture Output)
        {
            rebuildMeshObjectBuffers();
            
            setShaderParameters();
            
            // Make sure we have a current render target
            // and the accumulated texture result
            initRenderTexture(ref Result);
            initRenderTexture(ref Converged);

            // Set the target and dispatch the compute shader
            ComponentComputeShader.SetTexture(0, "Result", Result);
            
            dispatchShader(ref ComponentComputeShader, 16.0f, 16.0f);

            // Blit the resulting texture to the screen
            if (MaterialAdditive == null)
            {
                MaterialAdditive = new Material(Shader.Find("Hidden/IES_AddShader"));
            }
            
            MaterialAdditive.SetFloat(_Sample, CurrentSample);
            Graphics.Blit(Result, Converged, MaterialAdditive);
            Graphics.Blit(Converged, Output);
            CurrentSample++;
        }

        private void OnEnable()
        {
            CurrentSample = 0;
            setupScene();
        }

        private void OnDisable()
        {
           SphereBuffer?.Release();
           MeshBuffer?.Release();
           VertexBuffer?.Release();
           IndexBuffer?.Release();
        }

        private void Update()
        {
            if (transform.hasChanged || bSpheresChanged || bLightingChanged)
            {
                CurrentSample = 0;
                bLightingChanged = false;
                transform.hasChanged = false;
                
                if (bSpheresChanged)
                {
                    setupScene();
                }
                bSpheresChanged = false;
            }

            if (!bSphereBobbing) return;

            foreach (var data in BobbingSpheres)
            {
                var s = Spheres[(int)data.x];
                float per_bob = (Mathf.Sin(UnityEngine.Time.time + data.y) + 1) / 2;
                float y_offset = per_bob * data.w * s.Rad;
                s.Pos.y = data.z + y_offset;
            }
        }

        /// <summary>
        /// (re)instantiates the scene to render
        /// </summary>
        private void setupScene()
        {
            if (RandomSeed != 0) Random.InitState(RandomSeed);
            
            BobbingSpheres.Clear();
            Spheres = new List<ShaderSphere>();
            for (var i = 0; i < SphereNumMax; ++i)
            {
                var new_sphere = new ShaderSphere();
                if (!sphereConstructor(ref new_sphere))
                {
                    Spheres.Add(new_sphere);
                    if (new_sphere.Mat.Emissive != Vector3.zero) // if it's emissive
                    {
                        BobbingSpheres.Add(new Vector4(Spheres.Count-1, Random.value, new_sphere.Rad, Random.value * new_sphere.Rad * MaxRelitiveBob));
                    }
                }
            }
            
            createComputeBuffer(ref SphereBuffer, Spheres, 56);
        }
        
    }
}
