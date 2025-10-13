using Mapbox.MapDebug.Scripts.Logging;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LoggingDataFetchingManagerBehaviour))]
public class LoggingDataFetchingManagerBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector first (shows Fetcher field and others)
        DrawDefaultInspector();

        var behaviour = (LoggingDataFetchingManagerBehaviour)target;
        var fetcher = behaviour.Fetcher;

        // Add some space and a header
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Logging Data Fetching Info", EditorStyles.boldLabel);

        if (fetcher == null)
        {
            EditorGUILayout.HelpBox("Fetcher is not initialized (null). Run the scene to see runtime stats.", MessageType.Info);
        }
        else
        {
            // Show the requested properties
            EditorGUILayout.LabelField("Added Count", fetcher.AddedCount.ToString());
            EditorGUILayout.LabelField("Initialized Count", fetcher.InitializedCount.ToString());
            EditorGUILayout.LabelField("Cancelled Count", fetcher.CancelledCount.ToString());
        }

        // Refresh the inspector during play mode
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}