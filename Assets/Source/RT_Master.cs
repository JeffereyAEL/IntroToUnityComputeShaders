using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Source.Core;
using Source.Utilities;
using Random = UnityEngine.Random;

namespace Source.RayTracing
{
    public class RayTracing : ComputeShaderMaster
    {
        /// Skybox ref texture
        public Texture SkyboxSrc;
    
        /// The scene directional light
        public Light DirectionalLight;
        
        // Random ShaderSphere params
        /// the number of Spheres to try an generate on Awake
        public int SphereNumMax = 10000;

        /// vec2(min_bound, max_bound) for the radius of a sphere
        public Vector2 SphereRad = new Vector2(0.3f, 1.5f);

        /// The placement radius of a given sphere
        public float SpherePlacementRad = 100.0f;
        
        /// The amount of times Rays bounce to generate reflections
        public int MaxBounce = 10;

        /// The seed for Unity's Random engine
        public int RandomSeed;

        /// Phong Shading Alpha
        public float PhongAlpha = 15.0f;
        
        /// Whether we're sampling from the skybox texture
        public bool UsingSkybox;

        /// The color of the sky w/o a texture
        public Color SkyColor = Color.black;
        
        /// The type of lighting being used
        public LightingType LightingMode = LightingType.ChanceDiffSpec;
        
        // PRIVATE
        /// <summary>
        ///  Local references to all RayTracingObjects in the scene
        /// </summary>
        private static readonly List<RayTracingObject> _RayTracingObjects = new List<RayTracingObject>();
        
        /// <summary>
        /// Whether the ShaderMesh Buffers need to be rebuilt
        /// </summary>
        private static bool bMeshBuffersDirty;

        private static readonly List<ShaderMesh> _Meshes = new List<ShaderMesh>();
        
        private static readonly List<Vector3> _Vertices = new List<Vector3>();
        
        private static readonly List<int> _Indices = new List<int>();

        /// the generated array of Spheres from Awake vec4( vec3 pos, float radius)
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
        
        private void rebuildMeshObjectBuffers()
        {
            if (!bMeshBuffersDirty) return;

            // clear all lists
            _Meshes.Clear();
            _Vertices.Clear();
            _Indices.Clear();

            foreach(var obj in _RayTracingObjects)
            {
                var mesh = obj.getMesh();
                
                // add vertex data
                var vertex_offset = _Vertices.Count;
                _Vertices.AddRange(mesh.vertices);
                
                // add index data - if the vertex buff wasn't empty before it needs to be offset
                var index_offset = _Indices.Count;
                var indices = mesh.GetIndices(0);
                _Indices.AddRange(indices.Select(Index => Index + vertex_offset));
                
                // add the object itself
                _Meshes.Add(new ShaderMesh()
                {
                    LocalToWorld = obj.transform.localToWorldMatrix,
                    IndicesOffset = index_offset,
                    IndicesCount = indices.Length 
                });
            }

            createComputeBuffer(ref MeshBuffer, _Meshes, 72);
            createComputeBuffer(ref VertexBuffer, _Vertices, 12);
            createComputeBuffer(ref IndexBuffer, _Indices, 4);
            
            CurrentSample = 0;
            bMeshBuffersDirty = false;
        }
        
        public static void registerObject(RayTracingObject Obj)
        {
            RayTracing._RayTracingObjects.Add(Obj);
            RayTracing.bMeshBuffersDirty = true;
        }

        public static void unregisterObject(RayTracingObject Obj)
        {
            RayTracing._RayTracingObjects.Remove(Obj);
            RayTracing.bMeshBuffersDirty = true;
        }
        
        /// <summary>
        /// Constructor for the ShaderSphere struct
        /// </summary>
        /// <param name="New_shader_sphere"></param>
        /// <param name="Spheres"></param>
        /// <returns> whether the sphere intersects another sphere in list </returns>
        private bool Sphere_Construct(ref ShaderSphere New_shader_sphere, List<ShaderSphere> Spheres)
        {
            New_shader_sphere.Rad = SphereRad.x + Random.value * (SphereRad.y - SphereRad.x);
            var pos = Random.insideUnitCircle * SpherePlacementRad;
            New_shader_sphere.Pos = new Vector3(pos.x, New_shader_sphere.Rad, pos.y);

            foreach (var other in Spheres)
            {
                var min_dist = New_shader_sphere.Rad + other.Rad;
                if (Vector3.SqrMagnitude(New_shader_sphere.Pos - other.Pos) < min_dist * min_dist)
                    return true;
            }
            
            // Albedo and Specular color
            var color = Random.ColorHSV();
            var metal = Random.value < 0.0f;
            New_shader_sphere.Mat.Albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            New_shader_sphere.Mat.Specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            New_shader_sphere.Mat.Emission = new Vector3(Random.value, Random.value, Random.value);
            New_shader_sphere.Mat.Roughness = Random.value;
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
            ComponentComputeShader.SetFloat("_UsingSkybox", UsingSkybox ? 1.0f : 0.0f);
            ComponentComputeShader.SetVector("_SkyColor", SkyColor); 
            setComputeBuffer("_Spheres", SphereBuffer);
            setComputeBuffer("_Meshes", MeshBuffer);
            setComputeBuffer("_Vertices", VertexBuffer);
            setComputeBuffer("_Indices", IndexBuffer);
        }

        protected override void OnRenderImage(RenderTexture Src, RenderTexture Output)
        {
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
            if (!transform.hasChanged) return;
            CurrentSample = 0;
            transform.hasChanged = false;
            rebuildMeshObjectBuffers();
        }

        private void setupScene()
        {
            Random.InitState(RandomSeed);
            var temp = new List<ShaderSphere>();
            for (var i = 0; i < SphereNumMax; ++i)
            {
                var new_sphere = new ShaderSphere();
                if (!Sphere_Construct(ref new_sphere, temp))
                    temp.Add(new_sphere);
            }
            
            // Compute the new Spheres buff 
            SphereBuffer = new ComputeBuffer(temp.Count, 56);
            SphereBuffer.SetData(temp);
        }
        
    }
}
