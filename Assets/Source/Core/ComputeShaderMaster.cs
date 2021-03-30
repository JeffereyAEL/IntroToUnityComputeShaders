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

        protected abstract void OnRenderImage(RenderTexture Src, RenderTexture Output);

        protected static void dispatchShader(ref ComputeShader Shader, float Divider_x, float Divider_y, int Thread_groups_z = 1,
            int Kernal_index = 0)
        {
            var thread_groups_x = Mathf.CeilToInt(Screen.width / Divider_x);
            var thread_groups_y = Mathf.CeilToInt(Screen.height / Divider_y);
            Shader.Dispatch(Kernal_index, thread_groups_x, thread_groups_y, Thread_groups_z);
        }
        
        protected static void createComputeBuffer<T>(ref ComputeBuffer Buff, List<T> Data, int Stride)
            where T : struct
        {
            // Do we already have a compute buff?
            if (Buff != null)
            {
                // If no data or buff doesn't match the given criteria, release it
                if (Data.Count == 0 || Buff.count != Data.Count || Buff.stride != Stride)
                {
                    Buff.Release();
                    Buff = null;
                }
            }

            if (Data.Count == 0) return;
            
            // If the buff has been released or wasn't there to
            // begin with, create it
            Buff ??= new ComputeBuffer(Data.Count, Stride);
            
            // Set data on the buff
            Buff.SetData(Data);
        }
        
        protected void setComputeBuffer(string Name, ComputeBuffer Buff)
        {
            if (Buff != null)
            {
                ComponentComputeShader.SetBuffer(0, Name, Buff);
            }
        }
        
        protected static void initTexture(ref RenderTexture Tex, int Width, int Height)
        {
            // If RenderTexture already initialized break
            if (Tex != null && Tex.width == Width && Tex.height == Height) return;

            // Release render Texture if already initialized
            if (Tex != null)
                Tex.Release();

            // Get render target for raytracing
            Tex = new RenderTexture(Width, Height, 0,
                RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear) {enableRandomWrite = true};
            Tex.Create();
        }
        protected static void initRenderTexture(ref RenderTexture Tex)
        {
            initTexture(ref Tex, Screen.width, Screen.height);
        }
        protected virtual void Awake()
        {
            RefCam = GetComponent<Camera>();
        }
        protected abstract void setShaderParameters();
        
        }
    }
}