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
        public Vector2Int CellSize;

        // Modifies the rate at which time affects the cells
        public float TimeScale = 0.2f;

        // The color of a living cell in the game of life
        public Color AliveColorA;

        // The color of a living cell in the game of life
        public Color AliveColorB;

        // The color of a dead cell in the game of life
        public Color DeadColorA;

        // The color of a dead cell in the game of life
        public Color DeadColorB;

        public int GOLSeed = 0;
        
        // Handles changing the state of the cells once ever life cycle
        public ComputeShader GOLStateChange;

        public ComputeShader GOLUpdate;
        
        // Initializes a StateMap texture
        public ComputeShader GOLInitializer;
        
        // The state map for the gol in-between (for updating)
        private RenderTexture TempStateMap;

        // The current state of the game of life
        private RenderTexture CurrStateMap;
        
        private Vector4 UVCoordinates = new Vector4(0, 1, 0, 1);

        private float UVScale = 1.0f;
            
        public float StateChangeWait = 1.0f;
        
        private float SinceStateChange;

        private bool bShiftDown = false;
        
        private Vector3 PrevMousePos = Vector3.zero;
        private static readonly int _UVCoordinates = Shader.PropertyToID("_UVCoordinates");

        private static readonly int _StateMap = Shader.PropertyToID("_StateMap");

        private static readonly int _AliveColorA = Shader.PropertyToID("_AliveColorA");
        private static readonly int _AliveColorB = Shader.PropertyToID("_AliveColorB");
        private static readonly int _DeadColorA = Shader.PropertyToID("_DeadColorA");
        private static readonly int _DeadColorB = Shader.PropertyToID("_DeadColorB");
        private static readonly int _UVScale = Shader.PropertyToID("_UVScale");

        protected override void setShaderParameters()
        {
            
        } 
        
        protected override void OnRenderImage(RenderTexture Src, RenderTexture Output)
        {
            setShaderParameters();
            
            // render the StateMap as a colored, oriented PPE
            GOLCameraPPE.SetVector(_UVCoordinates, UVCoordinates);
            GOLCameraPPE.SetFloat(_UVScale, UVScale);
            GOLCameraPPE.SetTexture(_StateMap, CurrStateMap);
            GOLCameraPPE.SetColor(_AliveColorA, AliveColorA);
            GOLCameraPPE.SetColor(_AliveColorB, AliveColorB);
            GOLCameraPPE.SetColor(_DeadColorA, DeadColorA);
            GOLCameraPPE.SetColor(_DeadColorB, DeadColorB);
            Graphics.Blit(CurrStateMap, Output, GOLCameraPPE);
            
            //saveTexture(Output, "C:/Users/Jefferey Schlueter/Pictures/GOL_Debugging/result_render_texture.png");
             
        }

        private void UpdateGameOfLife(bool bChanging_state)
        {
            if (bChanging_state)
            {
                initRenderTexture(ref TempStateMap, CurrStateMap.width, CurrStateMap.height);

                GOLStateChange.SetTexture(0, "_OldStateMap", CurrStateMap);
                GOLStateChange.SetTexture(0, "_NewStateMap", TempStateMap);
                GOLStateChange.SetInt("_MapWidth", CurrStateMap.width);
                GOLStateChange.SetInt("_MapHeight", CurrStateMap.height);
                GOLStateChange.SetFloat("_DeltaTime", Time.deltaTime * TimeScale);
                
                var thread_groups_x = Mathf.CeilToInt(CurrStateMap.width / 8.0f);
                var thread_groups_y = Mathf.CeilToInt(CurrStateMap.height / 8.0f);
                GOLStateChange.Dispatch(0, thread_groups_x, thread_groups_y, 1);
            }
            else
            {
                initRenderTexture(ref TempStateMap, CurrStateMap.width, CurrStateMap.height);

                GOLUpdate.SetTexture(0, "_OldStateMap", CurrStateMap);
                GOLUpdate.SetTexture(0, "_NewStateMap", TempStateMap);
                GOLUpdate.SetFloat("_DeltaTime", Time.deltaTime * TimeScale);

                var thread_groups_x = Mathf.CeilToInt(CurrStateMap.width / 8.0f);
                var thread_groups_y = Mathf.CeilToInt(CurrStateMap.height / 8.0f);
                GOLUpdate.Dispatch(0, thread_groups_x, thread_groups_y, 1);
            }

            Graphics.Blit(TempStateMap, CurrStateMap);
            
        }
        private void Update()
        {
            SinceStateChange += Time.deltaTime * TimeScale;

            bool state_changing = SinceStateChange >= StateChangeWait;
            UpdateGameOfLife(state_changing);
            if (state_changing) SinceStateChange = 0.0f;

            var mouse_pos = Input.mousePosition;
            if (Input.GetMouseButton(2))
            {
                //print("Updating Mouse Position:");
                Vector2 uv_mouse_delta = new Vector2((PrevMousePos.x - mouse_pos.x) / Screen.width,
                (PrevMousePos.y - mouse_pos.y) / Screen.height);
                Vector4 temp = new Vector4(uv_mouse_delta.x, uv_mouse_delta.x, uv_mouse_delta.y, uv_mouse_delta.y);
                UVCoordinates += temp * UVScale;
                //print($"UV_Coordinates = {UVCoordinates}");
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
                bShiftDown = true;
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                bShiftDown = false;
            
            if (bShiftDown)
                UVScale -= Input.mouseScrollDelta.y / 5.0f * UVScale;
            else
                UVScale -= Input.mouseScrollDelta.y / 100.0f * UVScale;
            
            PrevMousePos = mouse_pos;
        }
    
        
        protected void Start()
        {
            //Randomly generate the board state
            
            // var temp = new Texture2D(CellSize.x, CellSize.y, TextureFormat.ARGB32, true);
            // print($"Temp texture is of the format '{temp.format}'");
            //
            // var pixels = new Color[CellSize.x * CellSize.y];
            // for (var i = 0; i < CellSize.x * CellSize.y; ++i)
            //     pixels[i] = new Color(Random.Range(0,2), 0, 0, 0);
            //
            // temp.SetPixels(pixels);
            // temp.Apply();
            
            initRenderTexture(ref CurrStateMap, CellSize.x, CellSize.y, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var thread_groups_x = Mathf.CeilToInt(CurrStateMap.width / 8.0f);
            var thread_groups_y = Mathf.CeilToInt(CurrStateMap.height / 8.0f);
            GOLInitializer.SetTexture(0, "_StateMap", CurrStateMap);
            GOLSeed = GOLSeed == 0 ? Random.Range(1, 10000) : GOLSeed;
            GOLInitializer.SetInt("_Seed", GOLSeed);
            GOLInitializer.Dispatch(0, thread_groups_x, thread_groups_y, 1);
            // RenderTexture.active = CurrStateMap;
            //
            // Graphics.Blit(temp, CurrStateMap);
            saveTexture(CurrStateMap, "C:/Users/Jefferey Schlueter/Pictures/GOL_Debugging/initialed_state_render_texture.png");
        }
    }
}