using System;
using Source.RayTracing;
using UnityEngine;
using UnityEditor;
using Source.Utilities;

namespace Source.Editor
{
    /// <summary>
    /// Handles custom editor presets and dirty flag functionality related to RT_Objects and RT_Master
    /// </summary>
    [CustomEditor(typeof(RT_Object))]
    public class MyScriptInspector : UnityEditor.Editor
    {
        /// <summary>
        /// Uses a more user friendly color field for visually representative Vector3 
        /// </summary>
        /// <param name="name"> The name of the field </param>
        /// <param name="Value"> The value to adjust with the editor field </param>
        private static void getVec3FromColorField(string name, ref Vector3 Value)
        {
            var color = EditorGUILayout.ColorField(name, Misc._ToRGBA(Value));
            Value = Misc._ToRGB(color);
        }

        /// <summary>
        /// Uses a more user friendly color field for visually representative Vector3 
        /// </summary>
        /// <param name="name"> The name of the field </param>
        /// <param name="Value"> The value to adjust with the editor field </param>
        private static void getVec3FromFloatField(string name, ref Vector3 Value)
        {
            var uni = EditorGUILayout.FloatField(name, Value.x);
            Value = new Vector3(uni, uni, uni);
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var obj = target as RT_Object;

            // Switches between different editor presets depending on the material class of the object 
            switch (obj.MatType)
            {
                case RT_Object.MaterialType.Metallic:
                    obj.Mat.Albedo = Vector3.one;
                    getVec3FromColorField("Specular", ref obj.Mat.Specular);
                    getVec3FromColorField("Emissive", ref obj.Mat.Emissive);
                    obj.Mat.Roughness = EditorGUILayout.FloatField("Roughness", obj.Mat.Roughness);
                    break;

                case RT_Object.MaterialType.Matte:
                    getVec3FromColorField("Albedo", ref obj.Mat.Albedo);
                    getVec3FromFloatField("Specular", ref obj.Mat.Specular);
                    getVec3FromColorField("Emissive", ref obj.Mat.Emissive);
                    obj.Mat.Roughness = EditorGUILayout.FloatField("Roughness", obj.Mat.Roughness);
                    break;
                
                case RT_Object.MaterialType.Light:
                    obj.Mat.Albedo = Vector3.one;
                    obj.Mat.Specular = Vector3.zero;
                    getVec3FromFloatField("Emissive", ref obj.Mat.Emissive);
                    obj.Mat.Roughness = 0.0f;
                    break;
                
                default:
                    throw new Exception("Invalid Material Type on Object");
            }
            
            
            // Setting the dirty state of an RT_Object when their material has changed
            if (!GUI.changed) return;
            EditorUtility.SetDirty(target);
            obj.bMaterialChanged = true;
        }
    }
}