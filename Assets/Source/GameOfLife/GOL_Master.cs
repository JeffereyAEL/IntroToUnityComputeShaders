using System;
using UnityEngine;
using Source.Core;
using TMPro;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

namespace Source.Core
{
    public class GOL_Master : ComputeShaderMaster
    {
        public Material GOLCameraPPE;
        
        // The size of a cell in the game of life
        public int CellSize = 20;
        
        // The color of a living cell in the game of life
        public Color AliveColor;
        // The color of a dead cell in the game of life
        public Color DeadColor;
        
        // // Life Length
        // public float LifeLength;
        
        // // The Cut off for when a randomGen state is 0 or 1
        // public float Barrier;
        //
        // Handles changing the state of the cells once ever life cycle
        public ComputeShader GOLStateChange;

        private RenderTexture TempStateMap;
        //
        // // // updates every cell once per frame
        // // public ComputeShader GOLUpdate;

        // The current state of the game of life
        private RenderTexture StateCurr;

        public float StateChangeWait = 1.0f;
        
        private float SinceStateChange;
        
        private Vector3 PrevMousePos = Vector3.zero;

        private bool bIsUpdatingState = false;
        
        private static readonly int _UVCoordinates = Shader.PropertyToID("_UVCoordinates");

        private static readonly int _StateMap = Shader.PropertyToID("_StateMap");

        private static readonly int _AliveColor = Shader.PropertyToID("_AliveColor");

        private static readonly int _DeadColor = Shader.PropertyToID("_DeadColor");

        protected override void setShaderParameters()
        {
            ComponentComputeShader.SetVector("_AliveColor", AliveColor);
            ComponentComputeShader.SetVector("_DeadColor", DeadColor);
            ComponentComputeShader.SetFloat("_CellSize", CellSize);
            ComponentComputeShader.SetInts("_ScreenResolution", new []{Screen.width, Screen.height});
        } 
        
        protected override void OnRenderImage(RenderTexture Src, RenderTexture Output)
        {
            setShaderParameters();
            
            if (bIsUpdatingState)
            {
                initRenderTexture(ref TempStateMap, StateCurr.width, StateCurr.height);
                
                GOLStateChange.SetTexture(0, "_OldStateMap", StateCurr);
                GOLStateChange.SetTexture(0, "_NewStateMap", TempStateMap);
                GOLStateChange.SetInt("_MapWidth", StateCurr.width);
                GOLStateChange.SetInt("_MapHeight", StateCurr.height);
                
                var thread_groups_x = Mathf.CeilToInt(StateCurr.width / 8);
                var thread_groups_y = Mathf.CeilToInt(StateCurr.height / 8);
                GOLStateChange.Dispatch(0, thread_groups_x, thread_groups_y, 1);
                RenderTexture.active = StateCurr;
                
                Graphics.Blit(TempStateMap, StateCurr);
                
                SinceStateChange = 0.0f;
            }
            
            // render the StateMap as a colored, oriented PPE
            GOLCameraPPE.SetVector(_UVCoordinates, new Vector4(0,1,0,1));
            GOLCameraPPE.SetTexture(_StateMap, StateCurr);
            GOLCameraPPE.SetColor(_AliveColor, AliveColor);
            GOLCameraPPE.SetColor(_DeadColor, DeadColor);
            Graphics.Blit(StateCurr, Output, GOLCameraPPE);
            
            //saveTexture(Output, "C:/Users/Jefferey Schlueter/Pictures/GOL_Debugging/result_render_texture.png");
             
        }

        private void Update()
        {
            SinceStateChange += Time.deltaTime;
            bIsUpdatingState = SinceStateChange >= StateChangeWait;
            
            
        }
        // private void gameOfLife();
        // {
        // TODO: implement these compute shaders when everything else is working
        //     // call update CompShader every frame
        //     // at the beginning of every life cycle call the state change CompShader
        // }

        protected void Start()
        {
            //Randomly generate the board state
            var w = Screen.width / CellSize;
            var h = Screen.height / CellSize;
            
            var temp = new Texture2D(w, h, TextureFormat.ARGB32, true);
            print($"Temp texture is of the format '{temp.format}'");
            
            var pixels = new Color[w * h];
            for (var i = 0; i < w * h; ++i)
                if (Random.value < 0.5)
                    pixels[i] = Color.white;
                else
                    pixels[i] = Color.black;
            
            temp.SetPixels(pixels);
            temp.Apply();
            
            initRenderTexture(ref StateCurr, temp.width, temp.height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = StateCurr;
            
            Graphics.Blit(temp, StateCurr);
            saveTexture(StateCurr, "C:/Users/Jefferey Schlueter/Pictures/GOL_Debugging/initialed_state_render_texture.png");
            
            saveTexture(temp, "C:/Users/Jefferey Schlueter/Pictures/GOL_Debugging/initialed_state_texture_2d.png");
        }
    }
}