using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// ポップな曲がったゲージ（枠付き・状態変化対応）
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class BentGaugeMesh : MaskableGraphic
    {
        [System.Serializable]
        public class BendPoint
        {
            public Vector2 position;
            public float width = 20f;
            
            [HideInInspector]
            public Vector2 direction;
            [HideInInspector]
            public Vector2 normal;
        }

        [Header("ベンドポイント")]
        [SerializeField] private List<BendPoint> _bendPoints = new List<BendPoint>();

        [Header("ゲージ設定")]
        [SerializeField, Range(0f, 1f)] private float _fillAmount = 1f;
        [SerializeField] private int _segmentsPerPoint = 10;
        [SerializeField] private bool _smoothNormals = true;

        [Header("影/背景モード")]
        [SerializeField] private bool _isShadowMode = false;

        [Header("ストライプ設定（画像風）")]
        [SerializeField] private bool _useStripes = true;
        [SerializeField] private Color _stripeColor1 = new Color(1f, 0.8f, 0.2f); // 黄色
        [SerializeField] private Color _stripeColor2 = new Color(1f, 0.6f, 0f);   // オレンジ
        [SerializeField] private float _stripeWidth = 15f;
        [SerializeField] private float _stripeAngle = 45f; // 斜め縞の角度

        [Header("エディタープレビュー")]
        [SerializeField] private bool _enableEditorPreview = true;
        [SerializeField, Range(0f, 1f)] private float _previewFillAmount = 0.5f;

        public float FillAmount
        {
            get => _isShadowMode ? 1f : _fillAmount;
            set
            {
                if (!_isShadowMode)
                {
                    _fillAmount = Mathf.Clamp01(value);
                    SetVerticesDirty();
                }
            }
        }

        public bool IsShadowMode
        {
            get => _isShadowMode;
            set
            {
                _isShadowMode = value;
                SetVerticesDirty();
            }
        }

        public List<BendPoint> BendPoints => _bendPoints;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_bendPoints.Count < 2)
            {
                return;
            }

            CalculateNormals();
            GenerateMesh(vh);
        }

        private void CalculateNormals()
        {
            for (int i = 0; i < _bendPoints.Count; i++)
            {
                Vector2 direction;

                if (i < _bendPoints.Count - 1)
                {
                    direction = (_bendPoints[i + 1].position - _bendPoints[i].position).normalized;
                }
                else
                {
                    direction = (_bendPoints[i].position - _bendPoints[i - 1].position).normalized;
                }

                _bendPoints[i].direction = direction;
                _bendPoints[i].normal = new Vector2(-direction.y, direction.x);
            }

            if (_smoothNormals && _bendPoints.Count > 2)
            {
                for (int i = 1; i < _bendPoints.Count - 1; i++)
                {
                    Vector2 prevNormal = _bendPoints[i - 1].normal;
                    Vector2 nextNormal = _bendPoints[i + 1].normal;
                    _bendPoints[i].normal = ((prevNormal + nextNormal) / 2f).normalized;
                }
            }
        }

        private void GenerateMesh(VertexHelper vh)
        {
            float totalLength = CalculateTotalLength();
            float currentLength = 0f;
            
            float effectiveFillAmount = _isShadowMode ? 1f : _fillAmount;
            float fillLength = totalLength * effectiveFillAmount;

            int vertexIndex = 0;

            for (int i = 0; i < _bendPoints.Count - 1; i++)
            {
                BendPoint p1 = _bendPoints[i];
                BendPoint p2 = _bendPoints[i + 1];

                float segmentLength = Vector2.Distance(p1.position, p2.position);

                for (int seg = 0; seg < _segmentsPerPoint; seg++)
                {
                    float t1 = (float)seg / _segmentsPerPoint;
                    float t2 = (float)(seg + 1) / _segmentsPerPoint;

                    float segStart = currentLength + segmentLength * t1;
                    float segEnd = currentLength + segmentLength * t2;

                    if (segStart > fillLength)
                        break;

                    if (segEnd > fillLength)
                    {
                        float clipT = (fillLength - segStart) / (segEnd - segStart);
                        t2 = t1 + (t2 - t1) * clipT;
                        segEnd = fillLength;
                    }

                    Vector2 pos1 = Vector2.Lerp(p1.position, p2.position, t1);
                    Vector2 pos2 = Vector2.Lerp(p1.position, p2.position, t2);

                    Vector2 normal1 = Vector2.Lerp(p1.normal, p2.normal, t1).normalized;
                    Vector2 normal2 = Vector2.Lerp(p1.normal, p2.normal, t2).normalized;

                    float width1 = Mathf.Lerp(p1.width, p2.width, t1);
                    float width2 = Mathf.Lerp(p1.width, p2.width, t2);

                    Vector2 v0 = pos1 - normal1 * width1 * 0.5f;
                    Vector2 v1 = pos1 + normal1 * width1 * 0.5f;
                    Vector2 v2 = pos2 + normal2 * width2 * 0.5f;
                    Vector2 v3 = pos2 - normal2 * width2 * 0.5f;

                    // ストライプの色を計算
                    Color c0 = CalculateStripeColor(v0, segStart);
                    Color c1 = CalculateStripeColor(v1, segStart);
                    Color c2 = CalculateStripeColor(v2, segEnd);
                    Color c3 = CalculateStripeColor(v3, segEnd);

                    Vector2 uv0 = new Vector2(segStart / totalLength, 0f);
                    Vector2 uv1 = new Vector2(segStart / totalLength, 1f);
                    Vector2 uv2 = new Vector2(segEnd / totalLength, 1f);
                    Vector2 uv3 = new Vector2(segEnd / totalLength, 0f);

                    vh.AddVert(v0, c0, uv0);
                    vh.AddVert(v1, c1, uv1);
                    vh.AddVert(v2, c2, uv2);
                    vh.AddVert(v3, c3, uv3);

                    vh.AddTriangle(vertexIndex + 0, vertexIndex + 1, vertexIndex + 2);
                    vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 0);

                    vertexIndex += 4;
                }

                currentLength += segmentLength;
            }
        }

        /// <summary>
        /// 斜めストライプの色を計算
        /// </summary>
        private Color CalculateStripeColor(Vector2 pos, float worldPos)
        {
            if (!_useStripes)
            {
                return color;
            }
            if(_isShadowMode)
            {
                return new Color(0f, 0f, 0f, 1f);
            }

            // 斜め方向のストライプ計算
            float angleRad = _stripeAngle * Mathf.Deg2Rad;
            float rotatedPos = pos.x * Mathf.Cos(angleRad) + pos.y * Mathf.Sin(angleRad);
            
            float stripePos = rotatedPos / _stripeWidth;
            bool isStripe = (Mathf.FloorToInt(stripePos) % 2) == 0;

            Color stripeColor = isStripe ? _stripeColor1 : _stripeColor2;
            return stripeColor * color;
        }

        private float CalculateTotalLength()
        {
            float length = 0f;
            for (int i = 0; i < _bendPoints.Count - 1; i++)
            {
                length += Vector2.Distance(_bendPoints[i].position, _bendPoints[i + 1].position);
            }
            return length;
        }

        public void AddBendPoint(Vector2 position, float width)
        {
            _bendPoints.Add(new BendPoint { position = position, width = width });
            SetVerticesDirty();
        }

        public void ClearBendPoints()
        {
            _bendPoints.Clear();
            SetVerticesDirty();
        }

        public void SetBendPoints(List<Vector2> positions, float width = 20f)
        {
            _bendPoints.Clear();
            foreach (var pos in positions)
            {
                _bendPoints.Add(new BendPoint { position = pos, width = width });
            }
            SetVerticesDirty();
        }

        /// <summary>
        /// 色を変更（状態に応じて）
        /// </summary>
        public void SetColors(Color color1, Color color2)
        {
            _stripeColor1 = color1;
            _stripeColor2 = color2;
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (_enableEditorPreview && !Application.isPlaying)
            {
                if (!_isShadowMode)
                {
                    _fillAmount = _previewFillAmount;
                }
            }
            
            SetVerticesDirty();
        }

        private void OnDrawGizmos()
        {
            if (_bendPoints == null || _bendPoints.Count == 0)
                return;

            Gizmos.color = _isShadowMode ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.yellow;

            for (int i = 0; i < _bendPoints.Count; i++)
            {
                Vector3 worldPos = transform.TransformPoint(_bendPoints[i].position);
                Gizmos.DrawWireSphere(worldPos, 5f);

                string label = _isShadowMode ? $"S{i}" : $"P{i}";
                UnityEditor.Handles.Label(worldPos, label);

                if (i < _bendPoints.Count - 1)
                {
                    Vector3 nextWorldPos = transform.TransformPoint(_bendPoints[i + 1].position);
                    Gizmos.DrawLine(worldPos, nextWorldPos);
                }
            }

            if (!_isShadowMode)
            {
                float totalLength = CalculateTotalLength();
                float fillLength = totalLength * _fillAmount;
                float currentLength = 0f;

                for (int i = 0; i < _bendPoints.Count - 1; i++)
                {
                    float segmentLength = Vector2.Distance(_bendPoints[i].position, _bendPoints[i + 1].position);
                    
                    if (currentLength + segmentLength >= fillLength)
                    {
                        float t = (fillLength - currentLength) / segmentLength;
                        Vector2 fillPos = Vector2.Lerp(_bendPoints[i].position, _bendPoints[i + 1].position, t);
                        Vector3 worldFillPos = transform.TransformPoint(fillPos);
                        
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(worldFillPos, 8f);
                        UnityEditor.Handles.Label(worldFillPos, $"Fill: {_fillAmount:P0}");
                        break;
                    }
                    
                    currentLength += segmentLength;
                }
            }
        }
#endif
    }
}