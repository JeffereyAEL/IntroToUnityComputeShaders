using System;
using Source.RayTracing;
using UnityEngine;
using UnityEditor;
using Source.Utilities;
using Debug = System.Diagnostics.Debug;

namespace Source.Editor
{
    /// <summary>
    /// Handles custom editor presets and dirty flag functionality related to RT_Objects and RT_Master
    /// </summary>
    [CustomEditor(typeof(RT_Master))]
    public class EDITOR_RT_Master : UnityEditor.Editor
    {
        /// <summary>
        ///  dirty flag for global scene change
        /// </summary>
        private bool bGUIChanged;
        
        /// Whether to expose advanced lighting editor details
        [NonSerialized] private bool bAdvancedLighting;
        
        /// whether to expose advanced optimizations fields in the GUI
        [NonSerialized] private bool bExposeOptimizations;
        
        /// whether to expose the sphere fields in the GUI
        [NonSerialized] private bool bExposeSpheres;
        
        /// <summary>
        /// Allows for testing of attribute changes of a specific section of code and the setting of a dirty flag
        /// related to those attributes
        /// </summary>
        /// <param name="Dirty_flag"> the dirty flag to be set if previous GUI attributes have been changed </param>
        private void hasGUIChanged(ref bool Dirty_flag)
        {
            if (!GUI.changed) return;
            bGUIChanged = true;
            GUI.changed = false;
            Dirty_flag = true;
        }

        private void ignoreGUIChanged()
        {
            bGUIChanged = GUI.changed;
            GUI.changed = false;
        }
        
        /// <summary>
        /// Allows you to adjust the value of an attribute, and trigger GUI changes w/o triggering any false calls to
        /// a dirty flag setter using hasGUIChanged()
        /// </summary>
        /// <param name="Src"> the value to set </param>
        /// <param name="Incoming"> the result of a EditorGUILayout function </param>
        /// <typeparam name="T"> the type of the Src variable </typeparam>
        // ReSharper disable once RedundantAssignment
        private void objIrrelevantChange<T>(ref T Src, T Incoming)
        {
            Src = Incoming;
            if (!GUI.changed) return;
            bGUIChanged = true;
            GUI.changed = false;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var obj = target as RT_Master;
            bGUIChanged = false;
            
            // lighting checks go first
            obj.IsUsingSkybox = EditorGUILayout.ToggleLeft("Using Skybox texture", obj.IsUsingSkybox);
            if (!obj.IsUsingSkybox)
            {
                obj.SkyColor = EditorGUILayout.ColorField("Sky Color", obj.SkyColor);
            }
            EditorGUILayout.Space();
            hasGUIChanged(ref obj.bLightingChanged);
                
            objIrrelevantChange(ref bAdvancedLighting ,
                EditorGUILayout.ToggleLeft("Advanced Lighting Options", bAdvancedLighting));
            if (bAdvancedLighting)
            {
                obj.MaxBounce = EditorGUILayout.IntSlider("Light Bounces", obj.MaxBounce, 0, 30);
                obj.PhongAlpha = EditorGUILayout.Slider("Phong Alpha", obj.PhongAlpha, 0.0f, 300.0f);
                obj.LightingMode = (LightingType)EditorGUILayout.EnumPopup("Mesh Collision Mode", obj.LightingMode);
            }
            EditorGUILayout.Space();
            hasGUIChanged(ref obj.bLightingChanged);
            
            objIrrelevantChange(ref bExposeOptimizations, 
                EditorGUILayout.ToggleLeft("Optimization Options", bExposeOptimizations));
            if (bExposeOptimizations)
            {
                obj.MeshCollisionMode = (MeshCollisionType)EditorGUILayout.EnumPopup("Mesh Collision Mode", obj.MeshCollisionMode);
            }
            EditorGUILayout.Space();
            ignoreGUIChanged();
            
            // scene checks go second
            objIrrelevantChange(ref bExposeSpheres, 
                EditorGUILayout.ToggleLeft("Show Sphere Fields", bExposeSpheres));
            if (bExposeSpheres)
            {
                obj.SphereNumMax = EditorGUILayout.IntField("Number of Spheres", obj.SphereNumMax);
                if (obj.SphereNumMax > 0)
                {
                    obj.SphereRad = EditorGUILayout.Vector2Field("Sphere Radius Bounds", obj.SphereRad);
                    obj.SpherePlacementRad = EditorGUILayout.FloatField("Sphere Spawn Radius", obj.SpherePlacementRad);
                    obj.IsSphereBobbing = EditorGUILayout.Toggle("Sphere Bobbing", obj.IsSphereBobbing);
                    if (obj.IsSphereBobbing)
                    {
                        obj.MaxRelativeBob = EditorGUILayout.FloatField("Max Relative Bob", obj.MaxRelativeBob);
                    }

                    obj.RandomSeed = EditorGUILayout.IntField("Unity Random seed", obj.RandomSeed);
                }
            }
            hasGUIChanged(ref obj.bSpheresChanged);
            
            // Setting the dirty state of an RT_Object when their material has changed
            GUI.changed = bGUIChanged; // to preserve original code style
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}