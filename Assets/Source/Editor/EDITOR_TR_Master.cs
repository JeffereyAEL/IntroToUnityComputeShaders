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
    [CustomEditor(typeof(RT_Master))]
    public class EDITOR_RT_Master : UnityEditor.Editor
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
            var obj = target as RT_Master;
            
            // obj.SkyBoxSrc is serialized and goes first
            obj.bUsingSkybox = EditorGUILayout.ToggleLeft("Using Skybox texture", obj.bUsingSkybox);
            if (!obj.bUsingSkybox)
            {
                obj.SkyColor = EditorGUILayout.ColorField("Sky Color", obj.SkyColor);
            }
            EditorGUILayout.Space();
            
            obj.bAdvancedLighting = EditorGUILayout.ToggleLeft("Advanced Lighting Options", obj.bAdvancedLighting);
            if (obj.bAdvancedLighting)
            {
                obj.MaxBounce = EditorGUILayout.IntSlider("Light Bounces", obj.MaxBounce, 0, 30);
                obj.PhongAlpha = EditorGUILayout.Slider("Phong Alpha", obj.PhongAlpha, 0.0f, 300.0f);
            }
            EditorGUILayout.Space();

            obj.bExposeSpheres = EditorGUILayout.ToggleLeft("Show Sphere Fields", obj.bExposeSpheres);
            if (obj.bExposeSpheres)
            {
                obj.SphereNumMax = EditorGUILayout.IntField("Number of Spheres", obj.SphereNumMax);
                if (obj.SphereNumMax > 0)
                {
                    obj.SphereRad = EditorGUILayout.Vector2Field("Sphere Radius Bounds", obj.SphereRad);
                    obj.SpherePlacementRad = EditorGUILayout.FloatField("Sphere Spawn Radius", obj.SpherePlacementRad);
                    obj.bSphereBobbing = EditorGUILayout.Toggle("Sphere Bobbing", obj.bSphereBobbing);
                    if (obj.bSphereBobbing)
                    {
                        obj.MaxRelitiveBob = EditorGUILayout.FloatField("Max Relative Bob", obj.MaxRelitiveBob);
                    }

                    obj.RandomSeed = EditorGUILayout.IntField("Unity Random seed", obj.RandomSeed);
                }
            }

            // Setting the dirty state of an RT_Object when their material has changed
            if (!GUI.changed) return;
            EditorUtility.SetDirty(target);
            obj.bEditorChanges = true;
        }
    }
}