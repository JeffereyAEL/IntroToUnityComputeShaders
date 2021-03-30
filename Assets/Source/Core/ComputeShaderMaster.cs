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

        protected void DispatchShader(ref ComputeShader Shader, float Divider_x = 8.0f, float Divider_y = 8.0f, int Thread_groups_z = 1,
            int kernalIndex = 0)
        {
            int threadGroupsX = Mathf.CeilToInt(Screen.width / Divider_x);
            int threadGroupsY = Mathf.CeilToInt(Screen.height / Divider_y);
            Shader.Dispatch(kernalIndex, threadGroupsX, threadGroupsY, Thread_groups_z);
        }
        
        protected static void InitTexture(ref RenderTexture Tex, int Width, int Height)
        {
            // If RenderTexture already initialized break
            if (Tex != null && Tex.width == Width && Tex.height == Height) return;

            // Release render Texture if already initialized
            if (Tex != null)
                Tex.Release();

            // Get render target for raytracing
            Tex = new RenderTexture(Width, Height, 0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            Tex.enableRandomWrite = true;
            Tex.Create();
        }
        protected void InitRenderTexture(ref RenderTexture Tex)
        {
            InitTexture(ref Tex, Screen.width, Screen.height);
        }
        protected virtual void Awake()
        {
            RefCam = GetComponent<Camera>();
        }

        protected abstract void SetShaderParameters();
        
        }
    }
}