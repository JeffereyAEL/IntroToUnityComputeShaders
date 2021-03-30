using UnityEngine;
using Source.Core;

namespace Source
{
    public class TheGameOfLife : ComputeShaderMaster
    {
        // The size of a cell in the game of life
        public int CellSize = 8;
        
        // The color of a living cell in the game of life
        public Color AliveColor;
        // The color of a dead cell in the game of life
        public Color DeadColor;
        // // Life Length
        // public float LifeLength;
        //
        // // // Time since last update
        // // private float SinceLastUpdate;
        //
        // // The Cut off for when a randomGen state is 0 or 1
        // public float Barrier;
        //
        // // // Handles changing the state of the cells once ever life cycle
        // // public ComputeShader GOLStateChange;
        //
        // // // updates every cell once per frame
        // // public ComputeShader GOLUpdate;

        // The current state of the game of life
        private RenderTexture StateCurr;
        // // The previous state of the game of life
        // private RenderTexture StatePrev;

        protected override void setShaderParameters()
        {
            ComponentComputeShader.SetVector("_AliveColor", AliveColor);
            ComponentComputeShader.SetVector("_DeadColor", DeadColor);
            ComponentComputeShader.SetFloat("_CellSize", CellSize);
        } 
        
        protected override void OnRenderImage(RenderTexture Src, RenderTexture Output)
        {
            setShaderParameters();
            
            ComponentComputeShader.SetTexture(0, "_StateMap", StateCurr);
            
            // Make sure we have a current render target
            initRenderTexture(ref Result);

            // Set the target and dispatch the compute shader
            ComponentComputeShader.SetTexture(0, "Result", Result);
            
            dispatchShader(ref ComponentComputeShader, 1024.0f, 1.0f);

            // Blit the resulting texture to the screen
            Graphics.Blit(Result, Output);
        }
        
        // private void gameOfLife();
        // {
        //     // call update CompShader every frame
        //     // at the beginning of every life cycle call the state change CompShader
        // }
        
        protected override void Awake()
        {
            base.Awake();
            
            // Randomly generate the board state
            var w = Screen.width / CellSize;
            var h = Screen.height / CellSize;
            
            var temp = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (var i = 0; i < w * h; ++i)
                pixels[i] = new Color(Mathf.RoundToInt(Random.value), 0, 0, 0);

            temp.SetPixels(pixels);
            
            initTexture(ref StateCurr, temp.width, temp.height);
            RenderTexture.active = StateCurr;
            
            Graphics.Blit(temp, StateCurr);
        }
    }
}