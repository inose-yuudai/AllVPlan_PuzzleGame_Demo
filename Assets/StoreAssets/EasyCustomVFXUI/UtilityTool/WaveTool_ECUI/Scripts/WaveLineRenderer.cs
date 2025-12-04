using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class WaveLineRenderer : MonoBehaviour
{
    public enum LineStyle
    {
        Zigzag,
        Wave
    }

    public LineStyle lineStyle;
    public Transform startPoint;
    public Transform endPoint;
    public int waveCount = 5;
    public float waveHeight = 1.0f;
    public float lineWidth = 0.1f;

    [HideInInspector]
    public int smoothness = 10;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdateLineWidth();
    }

    void Update()
    {
        if (lineStyle == LineStyle.Zigzag)
        {
            smoothness = 1;
        }
        DrawLine();
        UpdateLineWidth();
    }

    void DrawLine()
    {
        if (startPoint == null || endPoint == null) return;

        int pointCount = smoothness * (int)waveCount;

        lineRenderer.positionCount = pointCount + 1;
        lineRenderer.SetPosition(0, startPoint.position);

        if (lineStyle == LineStyle.Wave)
        {
            for (int i = 1; i <= pointCount; i++)
            {
                float t = i / (float)pointCount;
                Vector3 position = Vector3.Lerp(startPoint.position, endPoint.position, t);

                position.y += Mathf.Sin(t * waveCount * 2 * Mathf.PI) * waveHeight;
                lineRenderer.SetPosition(i, position);
            }
        }
        else if (lineStyle == LineStyle.Zigzag)
        {
            for (int i = 1; i <= pointCount; i++)
            {
                float t = i / (float)pointCount;
                Vector3 position = Vector3.Lerp(startPoint.position, endPoint.position, t);

                position.y += (i % 2 == 0 ? waveHeight : -waveHeight);
                lineRenderer.SetPosition(i, position);
            }
        }

        lineRenderer.SetPosition(pointCount, endPoint.position);
    }

    void UpdateLineWidth()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }
}
