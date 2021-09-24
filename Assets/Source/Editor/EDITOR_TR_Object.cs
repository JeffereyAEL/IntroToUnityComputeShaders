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
    public class EDITOR_RT_Object : UnityEditor.Editor
    {
        // ENUMS
        /// <summary>
        /// Defines the Unity Editor preset that will be exposed for this RT_Object's material
        /// </summary>
        private enum MaterialType
        {
            Metallic,
            Matte,
            Light
        };
    
        // ATTRIBUTES
        /// <summary>
        /// This RT_Object's material preset
        /// </summary>
        private MaterialType MatType = MaterialType.Matte;

        // METHODS
        /// <summary>
        /// Uses a more user friendly color field for visually representative Vector3 
        /// </summary>
        /// <param name="name"> The name of the field </param>
        /// <param name="Value"> The value to adjust with the editor field </param>
        private static void getVec3FromColorField(string name, ref Vector3 Value)
        {
            var color = EditorGUILayout.ColorField(name, Misc.ColorToRGBA(Value));
            Value = Misc.Vector3ToRGB(color);
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
        
        // EVENTS
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var obj = target as RT_Object;
            if (obj == null) return;
            MatType = (MaterialType)EditorGUILayout.EnumPopup("Material Type", MatType);
            // Switches between different editor presets depending on the material class of the object 
            switch (MatType)
            {
                case MaterialType.Metallic:
                    obj.Albedo = Vector3.one;
                    getVec3FromColorField("Specular", ref obj.Specular);
                    getVec3FromColorField("Emissive", ref obj.Emissive);
                    obj.Roughness = EditorGUILayout.FloatField("Roughness", obj.Roughness);
                    break;

                case MaterialType.Matte:
                    getVec3FromColorField("Albedo", ref obj.Albedo);
                    getVec3FromFloatField("Specular", ref obj.Specular);
                    getVec3FromColorField("Emissive", ref obj.Emissive);
                    obj.Roughness = EditorGUILayout.FloatField("Roughness", obj.Roughness);
                    break;
                
                case MaterialType.Light:
                    obj.Albedo = Vector3.one;
                    obj.Specular = Vector3.zero;
                    getVec3FromFloatField("Emissive", ref obj.Emissive);
                    obj.Roughness = 0.0f;
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