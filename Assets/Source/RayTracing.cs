using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Source.Core;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Source
{
    public class RayTracing : ComputeShaderMaster
    {
        /// Skybox ref texture
        public Texture SkyboxSrc;
    
        /// The scene directional light
        public Light DirectionalLight;
        
        // Random Sphere params
        /// the number of spheres to try an generate on Awake
        public int NumSpheresMax = 50;

        /// vec2(min_bound, max_bound) for the radius of a sphere
        public Vector2 SphereRad = new Vector2(0.3f, 1.5f);

        /// The placement radius of a given sphere
        public float SpherePlacementRad = 100.0f;
        
        /// The amount of times Rays bounce to generate reflections
        public int MaxBounce = 10;

        /// The seed for Unity's random engine
        public int RandomSeed;
        
        // PRIVATE
        /// the generated array of spheres from Awake vec4( vec3 pos, float radius)
        private ComputeBuffer SphereBuffer;

        /// The current sample; used to track anti-aliasing
        private uint CurrentSample = 0;

        /// the material used to smooth edges with anti-aliasing
        private Material AddMaterial;

        /// The buffer used to accumulation
        private RenderTexture Converged;
        
        // TODO: make a material struct that can be attached to any shape / object
        
        /// <summary>
        /// A mirrored structure for the CS_RayTracing.compute shader
        /// </summary>
        private struct Sphere
        {
            public Vector3 Pos;
            public float Rad;
            
            // Should be moved into material struct
            public Vector3 Albedo;
            public Vector3 Specular;
        }
    
        /// <summary>
        /// Constructor for the Sphere struct
        /// </summary>
        /// <param name="s"></param>
        /// <param name="spheres"></param>
        /// <returns> whether the sphere intersects another sphere in list </returns>
        private bool Sphere_Construct(ref Sphere s, List<Sphere> spheres)
        {
            s.Rad = SphereRad.x + Random.value * (SphereRad.y - SphereRad.x);
            Vector2 pos = Random.insideUnitCircle * SpherePlacementRad;
            s.Pos = new Vector3(pos.x, s.Rad, pos.y);

            foreach (var other in spheres)
            {
                float min_dist = s.Rad + other.Rad;
                if (Vector3.SqrMagnitude(s.Pos - other.Pos) < min_dist * min_dist)
                    return true;
            }
            
            // Albedo and Specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.0f;
            s.Albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            s.Specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            
            return false;
        }
        protected override void SetShaderParameters()
        {
            ComponentComputeShader.SetMatrix("unity_CameraToWorld", RefCam.cameraToWorldMatrix);
            ComponentComputeShader.SetMatrix("_CameraInverseProjection", RefCam.projectionMatrix.inverse);
            ComponentComputeShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
            ComponentComputeShader.SetInt("_MaxBounce", MaxBounce);
            Vector3 l = DirectionalLight.transform.forward;
            ComponentComputeShader.SetVector("_DirectionalLight", 
                new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
            ComponentComputeShader.SetBuffer(0, "_Spheres", SphereBuffer);
            ComponentComputeShader.SetFloat("_Seed", Random.value);
        }

        protected override void OnRenderImage(RenderTexture src, RenderTexture output)
        {
            SetShaderParameters();
            
            // Make sure we have a current render target
            // and the accumulated texture result
            InitRenderTexture(ref Result);
            InitRenderTexture(ref Converged);

            // Set the target and dispatch the compute shader
            ComponentComputeShader.SetTexture(0, "Result", Result);
            
            DispatchShader(ref ComponentComputeShader, 32, 32);

            // Blit the resulting texture to the screen
            if (AddMaterial == null)
            {
                AddMaterial = new Material(Shader.Find("Hidden/IES_AddShader"));
            }
            AddMaterial.SetFloat("_Sample", CurrentSample);
            Graphics.Blit(Result, Converged, AddMaterial);
            Graphics.Blit(Converged, output);
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
        }


        private void Update()
        {
            if (!transform.hasChanged) return;
            CurrentSample = 0;
            transform.hasChanged = false;
        }

        private void setupScene()
        {
            Random.InitState(RandomSeed);
            var temp = new List<Sphere>();
            for (int i = 0; i < NumSpheresMax; ++i)
            {
                Sphere new_sphere = new Sphere();
                if (!Sphere_Construct(ref new_sphere, temp))
                    temp.Add(new_sphere);
            }
            
            // Shader Parameters that only need to be set on enable
            ComponentComputeShader.SetTexture(0, "_SkyboxTexture", SkyboxSrc);
            ComponentComputeShader.SetInt("_NumSpheres", temp.Count);    
            SphereBuffer = new ComputeBuffer(temp.Count, 40);
            SphereBuffer.SetData(temp);
        }
        
    }
}
