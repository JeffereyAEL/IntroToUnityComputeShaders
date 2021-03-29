using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using UnityEngine;
using Source.Core;
using UnityEngine.Serialization;

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
        // Life Length
        public float LifeLength;
        
        // Time since last update
        private float SinceLastUpdate;
        
        // The Cut off for when a randomGen state is 0 or 1
        public float Barrier;
        
        // // Handles changing the state of the cells once ever life cycle
        // public ComputeShader GOLStateChange;
        
        // // updates every cell once per frame
        // public ComputeShader GOLUpdate;

        // The current state of the game of life
        private RenderTexture StateCurr;
        // The previous state of the game of life
        private RenderTexture StatePrev;

        protected override void SetShaderParameters()
        {
            ComponentComputeShader.SetVector("_AliveColor", AliveColor);
            ComponentComputeShader.SetVector("_DeadColor", DeadColor);
            ComponentComputeShader.SetFloat("_CellSize", CellSize);
        }

        protected override void prerender()
        {
            // gameOfLife();
            ComponentComputeShader.SetTexture(0, "_StateMap", StateCurr);
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
            int w = Screen.width / CellSize;
            int h = Screen.height / CellSize;
            
            var temp = new Texture2D(w, h);
            var pixs = new Color[w * h];
            for (int i = 0; i < w * h; ++i)
                pixs[i] = new Color(Mathf.RoundToInt(Random.value), 0, 0, 0);

            temp.SetPixels(pixs);
            
            InitTexture(ref StateCurr, temp.width, temp.height);
            RenderTexture.active = StateCurr;
            
            Graphics.Blit(temp, StateCurr);
        }
    }
}