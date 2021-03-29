using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Source
{
    namespace Core
    {

        public abstract class ComputeShaderMaster : MonoBehaviour
        {
            // The target shader to be run per batch of 8x8 pixels
        public ComputeShader ComponentComputeShader;

        // The texture target we output to
        protected RenderTexture Result;

        // the reference camera we're outputting Result to
        protected Camera RefCam;

        protected virtual void prerender() {}
        
        protected virtual void postrender() {}
        
        protected virtual void OnRenderImage(RenderTexture src, RenderTexture output)
        {
            SetShaderParameters();
            prerender();
            render(ref output, 8, 8);
            postrender();
        }
    
        private void render(ref RenderTexture output, int groupsX, int groupsY)
        {
            // Make sure we have a current render target
            InitRenderTexture(ref Result);

            // Set the target and dispatch the compute shader
            ComponentComputeShader.SetTexture(0, "Result", Result);
            
            DispatchShader(ref ComponentComputeShader, groupsX, groupsY);

            // Blit the resulting texture to the screen
            Graphics.Blit(Result, output);
        }

        protected void DispatchShader(ref ComputeShader shader, float dividerX = 8.0f, float dividerY = 8.0f, int threadGroupsZ = 1,
            int kernalIndex = 0)
        {
            int threadGroupsX = Mathf.CeilToInt(Screen.width / dividerX);
            int threadGroupsY = Mathf.CeilToInt(Screen.height / dividerY);
            shader.Dispatch(kernalIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
        }
        
        protected static void InitTexture(ref RenderTexture tex, int width, int height)
        {
            // If RenderTexture already initialized break
            if (tex != null && tex.width == width && tex.height == height) return;

            // Release render Texture if already initialized
            if (tex != null)
                tex.Release();

            // Get render target for raytracing
            tex = new RenderTexture(width, height, 0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            tex.enableRandomWrite = true;
            tex.Create();
        }
        protected void InitRenderTexture(ref RenderTexture tex)
        {
            InitTexture(ref tex, Screen.width, Screen.height);
        }
        protected virtual void Awake()
        {
            RefCam = GetComponent<Camera>();
        }

        protected abstract void SetShaderParameters();
        
        }
    }
}