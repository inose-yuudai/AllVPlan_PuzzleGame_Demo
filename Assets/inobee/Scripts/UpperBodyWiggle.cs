using UnityEngine;

[DisallowMultipleComponent]
public class UpperBodyWiggle : MonoBehaviour
{
    private const float k_TwoPi = 6.28318530718f;

    [Header("Targets (胸上だけ/頭だけのレイヤー)")]
    [SerializeField] private Transform _upperLayer; // 胸上だけ見せるレイヤー
    [SerializeField] private Transform _headLayer;  // 頭だけ見せるレイヤー

    [Header("Noise Speeds (不規則ゆらぎの速さ)")]
    [SerializeField] private float _upperNoiseSpeed = 0.35f;
    [SerializeField] private float _headNoiseSpeed  = 0.65f;

    [Header("Upper (上半身の振れ幅)")]
    [SerializeField] private float _upperAngleDeg = 6f;     // Z回転 ±deg
    [SerializeField] private float _upperPosX = 6f;         // ローカルX ±px/uu
    [SerializeField] private float _upperPosY = 2f;         // ローカルY ±px/uu
    [SerializeField] private float _upperBreathScale = 0.012f; // 呼吸の拡縮

    [Header("Head (頭の振れ幅：カウンター気味)")]
    [SerializeField] private float _headAngleDeg = 8f;
    [SerializeField] private float _headPosX = 3f;
    [SerializeField] private float _headPosY = 2f;
    [SerializeField] private float _headScale = 0.006f;

    [Header("Damping (なめらか遷移)")]
    [SerializeField] private float _rotDamp = 12f;  // 角度スムージング
    [SerializeField] private float _posDamp = 12f;  // 位置スムージング
    [SerializeField] private float _sclDamp = 12f;  // スケールスムージング

    [Header("Runtime Options")]
    [SerializeField] private bool _useUnscaledTime = true;
    [SerializeField] private int _seed = 12345; // 規則を変えたい時に

    // 初期Transform保存
    private Vector3 _upperInitPos, _upperInitScale;
    private Quaternion _upperInitRot;
    private Vector3 _headInitPos, _headInitScale;
    private Quaternion _headInitRot;

    // 現在値（スムージング用）
    private float _upperRot, _headRot;
    private Vector2 _upperPos, _headPos;
    private float _upperScl, _headScl;

    private float _t;

    private void Awake()
    {
        if (_upperLayer != null)
        {
            _upperInitPos = _upperLayer.localPosition;
            _upperInitRot = _upperLayer.localRotation;
            _upperInitScale = _upperLayer.localScale;

            _upperRot = 0f;
            _upperPos = Vector2.zero;
            _upperScl = 0f;
        }
        if (_headLayer != null)
        {
            _headInitPos = _headLayer.localPosition;
            _headInitRot = _headLayer.localRotation;
            _headInitScale = _headLayer.localScale;

            _headRot = 0f;
            _headPos = Vector2.zero;
            _headScl = 0f;
        }

        // 乱数シードを少しバラす
        _t = (_seed * 0.1234f) % 1000f;
    }

    private void Update()
    {
        float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _t += dt;

        if (_upperLayer != null)
        {
            // 上半身用ノイズ
            float nA = NoiseSigned(_t * _upperNoiseSpeed, 17.2f + _seed);
            float nX = NoiseSigned(_t * _upperNoiseSpeed, 91.7f + _seed);
            float nY = NoiseSigned(_t * (_upperNoiseSpeed * 0.8f), 44.1f + _seed);
            float nB = NoiseSigned(_t * 0.25f, 3.3f + _seed); // 呼吸はより低速

            // 目標
            float targetRot = _upperAngleDeg * nA;
            Vector2 targetPos = new Vector2(_upperPosX * nX, _upperPosY * nY);
            float targetScl = _upperBreathScale * nB;

            // スムージング
            _upperRot = Mathf.Lerp(_upperRot, targetRot, 1f - Mathf.Exp(-_rotDamp * dt));
            _upperPos = Vector2.Lerp(_upperPos, targetPos, 1f - Mathf.Exp(-_posDamp * dt));
            _upperScl = Mathf.Lerp(_upperScl, targetScl, 1f - Mathf.Exp(-_sclDamp * dt));

            // 適用（初期値を基準に相対で）
            _upperLayer.localRotation = _upperInitRot * Quaternion.Euler(0f, 0f, _upperRot);
            _upperLayer.localPosition = _upperInitPos + new Vector3(_upperPos.x, _upperPos.y, 0f);
            _upperLayer.localScale = _upperInitScale * (1f + _upperScl);
        }

        if (_headLayer != null)
        {
            // 頭は上半身の揺れに対して“やや逆位相”っぽく
            float tHead = _t * _headNoiseSpeed + 10.123f;
            float nA = NoiseSigned(tHead, 7.7f + _seed);
            float nX = NoiseSigned(tHead, 51.9f + _seed);
            float nY = NoiseSigned(tHead * 1.1f, 28.6f + _seed);
            float nS = NoiseSigned(tHead * 0.8f, 99.4f + _seed);

            float targetRot = -_headAngleDeg * nA; // カウンター感を出すためにマイナス
            Vector2 targetPos = new Vector2(-_headPosX * nX, -_headPosY * nY);
            float targetScl = _headScale * nS;

            _headRot = Mathf.Lerp(_headRot, targetRot, 1f - Mathf.Exp(-_rotDamp * dt));
            _headPos = Vector2.Lerp(_headPos, targetPos, 1f - Mathf.Exp(-_posDamp * dt));
            _headScl = Mathf.Lerp(_headScl, targetScl, 1f - Mathf.Exp(-_sclDamp * dt));

            _headLayer.localRotation = _headInitRot * Quaternion.Euler(0f, 0f, _headRot);
            _headLayer.localPosition = _headInitPos + new Vector3(_headPos.x, _headPos.y, 0f);
            _headLayer.localScale = _headInitScale * (1f + _headScl);
        }
    }

    // 0..1 の Perlin を -1..1 へ
    private static float NoiseSigned(float x, float y)
        => (Mathf.PerlinNoise(x, y) * 2f - 1f);
}
