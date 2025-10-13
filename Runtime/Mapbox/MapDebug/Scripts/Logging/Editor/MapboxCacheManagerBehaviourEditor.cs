using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mapbox.BaseModule.Data.Platform.Cache;
using Mapbox.Example.Scripts.ModuleBehaviours;

[CustomEditor(typeof(MapboxCacheManagerBehaviour))]
public class MapboxCacheManagerBehaviourEditor : Editor
{
    private bool _memoryCacheFoldout;
    private Dictionary<int, ITypeCache> _subCaches;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Cache Details", EditorStyles.boldLabel);

        var behaviour = (MapboxCacheManagerBehaviour)target;
        var memoryCache = behaviour.MemoryCache;

        _memoryCacheFoldout = EditorGUILayout.Foldout(_memoryCacheFoldout, "MemoryCache (runtime)", true);
        if (!_memoryCacheFoldout)
            return;

        EditorGUI.indentLevel++;

        if (memoryCache == null)
        {
            EditorGUILayout.HelpBox("MemoryCache is null. Run the scene to inspect it.", MessageType.Info);
            EditorGUI.indentLevel--;
            return;
        }

        // Get private _subCaches field
        var field = typeof(MemoryCache).GetField("_subCaches", BindingFlags.NonPublic | BindingFlags.Instance);
        _subCaches = field?.GetValue(memoryCache) as Dictionary<int, ITypeCache>;

        if (_subCaches == null)
        {
            EditorGUILayout.HelpBox("Could not access _subCaches.", MessageType.Warning);
            EditorGUI.indentLevel--;
            return;
        }

        if (_subCaches.Count == 0)
        {
            EditorGUILayout.LabelField("No sub-caches.");
        }
        else
        {
            foreach (var kvp in _subCaches)
            {
                DrawSubCache(kvp.Key, kvp.Value);
            }
        }

        EditorGUI.indentLevel--;

        if (Application.isPlaying)
            Repaint();
    }

    private void DrawSubCache(int key, object cache)
    {
        if (cache == null)
            return;

        var type = cache.GetType();
        if (!type.IsGenericType)
            return;

        // Must implement ITypeCache
        if (!type.GetInterfaces().Any(i => i.Name == "ITypeCache"))
            return;

        // Get generic argument type T
        var genericArg = type.GetGenericArguments().FirstOrDefault();
        string typeName = genericArg != null ? genericArg.Name : "(Unknown)";

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"[{key}] {typeName} Cache", EditorStyles.boldLabel);

        // Look for ActiveCount and InactiveCount properties
        var activeProp = type.GetProperty("ActiveCount", BindingFlags.Public | BindingFlags.Instance);
        var inactiveProp = type.GetProperty("InactiveCount", BindingFlags.Public | BindingFlags.Instance);

        if (activeProp != null)
        {
            var activeVal = activeProp.GetValue(cache);
            EditorGUILayout.LabelField("ActiveCount", activeVal?.ToString() ?? "N/A");
        }

        if (inactiveProp != null)
        {
            var inactiveVal = inactiveProp.GetValue(cache);
            EditorGUILayout.LabelField("InactiveCount", inactiveVal?.ToString() ?? "N/A");
        }

        EditorGUILayout.EndVertical();
    }
}
