using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Source.Core
{
    /// <summary>
    /// Base class for ComputeShader Handlers that streamlines the creation of new ComputeShader -> Unity Interfacing
    /// </summary>
    public abstract class ComputeShaderMaster : MonoBehaviour
    {
        /// <summary>
        /// The base rendering shader ( for compute shaders that push textures to a camera)
        /// </summary>
        public ComputeShader ComponentComputeShader;

        /// <summary>
        /// The texture that will be blit'd rendered to the Unity Camera every frame
        /// </summary>
        protected RenderTexture Result;

        /// <summary>
        /// The camera we'll be outputing to
        /// </summary>
        protected Camera RefCam;
        
        /// <summary>
        /// Required for this class. where the bulk of ComputeBuffer and logic should be handled
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="Output"></param>
        protected abstract void OnRenderImage(RenderTexture Src, RenderTexture Output);

        /// <summary>
        /// A handler that cleans up dispatching shaders to the GPU with reference to Screen computations
        /// </summary>
        /// <param name="Shader"> the compute shader to be dispatched </param>
        /// <param name="Divider_x"> splitting the width of the screen by the # Dividers to find threadGroupsX </param>
        /// <param name="Divider_y"> splitting the height of the screen by the # Dividers to find threadGroupsY </param>
        /// <param name="Thread_groups_z"> Number of Z threadGroups, usually 1 for graphical computations </param>
        /// <param name="Kernal_index"> Kernal Index, 1 for most basic compute shaders </param>
        protected static void dispatchShader(ref ComputeShader Shader, float Divider_x, float Divider_y, int Thread_groups_z = 1,
            int Kernal_index = 0)
        {
            var thread_groups_x = Mathf.CeilToInt(Screen.width / Divider_x);
            var thread_groups_y = Mathf.CeilToInt(Screen.height / Divider_y);
            Shader.Dispatch(Kernal_index, thread_groups_x, thread_groups_y, Thread_groups_z);
        }
        
        /// <summary>
        /// Handler for the creation of Compute Buffers
        /// </summary>
        /// <param name="Buff"> the compute buffer to be (re)instantiated </param>
        /// <param name="Data"> the data to be written to 'Buff' </param>
        /// <param name="Stride"> the byte size of an element in 'Data' </param>
        /// <typeparam name="T"> the class of templated list 'Data' </typeparam>
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

            // TODO: should always try and initialize buffer if it's been cleared
            //if (Data.Count == 0) return; // can't initialize buffers with count = 0
            
            // If the buff has been released or wasn't there to
            // begin with, create it
            try
            {
                Buff ??= new ComputeBuffer(Data.Count, Stride);

                // Set data on the buff
                Buff.SetData(Data);
            }
            catch (ArgumentException)
            {
                return;
            }
        }
        
        /// <summary>
        /// A handler for setting an uniform of a compute shader given an instantiated compute buffer
        /// </summary>
        /// <param name="Name"> the name of the uniform to set </param>
        /// <param name="Buff"> the buffer to write to the uniform </param>
        protected void setComputeBuffer(string Name, ComputeBuffer Buff)
        {
            if (Buff == null) return;//throw new Exception("Compute Buffer is null");
            ComponentComputeShader.SetBuffer(0, Name, Buff);
        }
        
        /// <summary>
        /// A handler for initializing a RenderTexture
        /// </summary>
        /// <param name="Tex"> The RenderTexture to initialize </param>
        /// <param name="Width"> The desired width of the RenderTexture in Vector4s </param>
        /// <param name="Height"> The desired height of the RenderTexture in Vector4s </param>
        protected static void initRenderTexture(ref RenderTexture Tex, int Width, int Height)
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
        
        /// <summary>
        /// A handler for initializing RenderTextures that presumes the texture to be the size of the Screen
        /// </summary>
        /// <param name="Tex"> The RenderTexture to initialize </param>
        protected static void initRenderTexture(ref RenderTexture Tex)
        {
            initRenderTexture(ref Tex, Screen.width, Screen.height);
        }
        
        protected virtual void Awake()
        {
            RefCam = GetComponent<Camera>();
        }
        
        /// <summary>
        /// A tidying function. Encourages the user to set compute shader uniforms all in one place
        /// </summary>
        protected abstract void setShaderParameters();
    
    }
}
