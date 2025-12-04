using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(WaveLineRenderer))]
public class WaveLineRendererEditor : Editor
{
    // Save Path
    private string savePath = "Resources/ECUI/ECUI_Sprites/LineRendererTexture.png";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WaveLineRenderer script = (WaveLineRenderer)target;

        if (script.lineStyle == WaveLineRenderer.LineStyle.Wave)
        {
            script.smoothness = EditorGUILayout.IntSlider("Smoothness", script.smoothness, 1, 100);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path", GUILayout.Width(70));
        savePath = EditorGUILayout.TextField(savePath);
        EditorGUILayout.EndHorizontal();

        // Editor Button
        if (GUILayout.Button("Save LineRenderer as Texture"))
        {
            SaveLineRendererAsTexture(script);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void SaveLineRendererAsTexture(WaveLineRenderer waveLineRenderer)
    {
        GameObject selectedObject = Selection.activeGameObject;
        LineRenderer lineRenderer = selectedObject.GetComponent<LineRenderer>();

        RenderTexture rt = new RenderTexture(256, 256, 24);
        rt.Create();

        Camera cam = new GameObject("LineRendererCam").AddComponent<Camera>();
        cam.backgroundColor = Color.clear;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
        cam.targetTexture = rt;

        Bounds bounds = CalculateLineRendererBounds(lineRenderer);

        cam.transform.position = bounds.center + Vector3.back * 10;
        cam.transform.LookAt(bounds.center);
        cam.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y);
        cam.aspect = (float)rt.width / (float)rt.height;

        cam.Render();

        RenderTexture.active = rt;
        Texture2D texture2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        texture2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture2D.Apply();

        // Relative Path
        string relativePath = savePath.StartsWith("Assets/") ? savePath.Substring("Assets/".Length) : savePath;
        string fullPath = Path.Combine(Application.dataPath, relativePath);
        string directoryPath = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        byte[] bytes = texture2D.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        Debug.Log($"Saved LineRenderer Texture to {fullPath}");

        RenderTexture.active = null;
        rt.Release();
        DestroyImmediate(cam.gameObject);
        AssetDatabase.Refresh();
    }

    private Bounds CalculateLineRendererBounds(LineRenderer lineRenderer)
    {
        Vector3 center = lineRenderer.bounds.center;
        float maxX = lineRenderer.bounds.max.x;
        float maxY = lineRenderer.bounds.max.y;
        float maxZ = lineRenderer.bounds.max.z;
        float minX = lineRenderer.bounds.min.x;
        float minY = lineRenderer.bounds.min.y;
        float minZ = lineRenderer.bounds.min.z;
        Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

        return new Bounds(center, size);
    }
}
