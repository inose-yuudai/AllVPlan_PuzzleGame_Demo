using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using DG.Tweening;
using EmoteOrchestra.UI;
using EmoteOrchestra.Core;

[DisallowMultipleComponent]
public class SwayAndLipSync : MonoBehaviour
{
    [Serializable]
    public struct SpriteSet
    {
        public Sprite _closed; // 口閉じ
        public Sprite _normal; // 通常
        public Sprite _open;   // 口開き
    }

    private enum MouthState
    {
        Closed,
        Normal,
        Open
    }

    // ====== 定数 ======
    private const int k_SampleRate = 44100;
    private const int k_DefaultSampleLength = 256;

    // ====== 参照 ======
    [Header("Target (どちらか自動検出)")]
    [SerializeField] private Image _uiImage;                 // uGUI用
    [SerializeField] private SpriteRenderer _spriteRenderer; // 2D用

    [Header("Sprite Variants (口閉/口開のセットを複数登録可)")]
    [Tooltip("最低1セット（closed/normal/open）。複数登録すると演出中に自動でポーズ変更できます。")]
    [SerializeField] private List<SpriteSet> _variants = new List<SpriteSet>();
    [Header("Fixed Smile (Outroで使用)")]
    [SerializeField] private Sprite _smileSprite;

    [Header("Sway（左右ゆらぎ）")]
    [SerializeField] private float _swayAngle = 5f;           // Z回転±角度（度）
    [SerializeField] private float _swayDuration = 0.8f;      // 片道時間（秒）
    [SerializeField] private float _posAmplitudeX = 6f;       // ローカルX平行移動の振幅（px/uu）
    [SerializeField] private float _scaleAmplitude = 0.015f;  // スケールの微振幅（初期スケール基準）
    [SerializeField] private Ease _swayEase = Ease.InOutSine;

    [Header("Pose Change（ポーズ自動切替・任意）")]
    [Tooltip("0以下なら切替しない。例: 3〜6秒ごとにランダムでバリエーション差し替え等。")]
    [SerializeField] private Vector2 _poseChangeIntervalSec = new Vector2(0f, 0f);

	[Header("Auto Expression")]
	[Tooltip("表情の更新間隔（秒）")]
	[SerializeField] private Vector2 _expressionChangeIntervalSec = new Vector2(0.25f, 0.6f);
	[Tooltip("Openを選ぶ確率")]
	[SerializeField, Range(0f, 1f)] private float _openProbability = 0.35f;
	[Tooltip("Normalを選ぶ確率（Closedは残り）")]
	[SerializeField, Range(0f, 1f)] private float _normalProbability = 0.45f;
	[Tooltip("口を最低維持する時間（Open用）")]
	[SerializeField] private Vector2 _openHoldTimeRange = new Vector2(0.07f, 0.15f);
	[Tooltip("normalを最低維持する時間")]
	[SerializeField] private Vector2 _normalHoldTimeRange = new Vector2(0.05f, 0.1f);

    [Header("Manual (手動制御オプション)")]
    [Tooltip("手動トグル用：キー押下でTalkingオンオフ（デバッグ用）")]
    [SerializeField] private KeyCode _toggleTalkingKey = KeyCode.None;

    [Header("Singing Notes")]
    [SerializeField] private bool _notesEnabled = true;
    [Tooltip("UIではImage等、2DではSpriteRenderer付きのPrefabを想定")]
    [SerializeField] private GameObject _notePrefab;
    [Tooltip("候補からランダムに選択。未設定なら単一Prefabを使用")]
    [SerializeField] private List<GameObject> _notePrefabs = new List<GameObject>();
    [Tooltip("未設定なら自身のTransform/RectTransform配下に生成")]
    [SerializeField] private Transform _noteSpawnRoot;
    [Tooltip("未設定ならキャラ中心から。UIならRectTransform推奨")]
    [SerializeField] private Transform _noteEmitPoint;
    [Tooltip("次のスポーンまでのランダム範囲（秒）")]
    [SerializeField] private Vector2 _noteSpawnInterval = new Vector2(0.35f, 0.6f);
	[Tooltip("テンション最小時に保証する最小出現間隔（秒）")]
	[SerializeField] private float _minNoteIntervalAtLowTension = 4f;
    [Tooltip("ローカル空間の移動方向（正規化されます）")]
    [SerializeField] private Vector2 _noteDirection = new Vector2(1f, 1.3f);
    [Tooltip("飛ばす距離のランダム範囲（UI: px, 2D: uu）")]
    [SerializeField] private Vector2 _noteDistanceRange = new Vector2(140f, 200f);
    [Tooltip("寿命（秒）のランダム範囲）")]
    [SerializeField] private Vector2 _noteLifetimeRange = new Vector2(1.1f, 1.8f);
    [Tooltip("開始スケールのランダム範囲")]
    [SerializeField] private Vector2 _noteScaleRange = new Vector2(0.7f, 1.1f);
    [Tooltip("発生位置の半径ランダム（口付近に散らす）")]
    [SerializeField] private float _noteSpawnRadius = 24f;
    [Tooltip("初期色（アルファはフェードアウト開始値）")]
    [SerializeField] private Color _noteColor = new Color(1f, 1f, 1f, 0.9f);
    [Tooltip("テンション最大時のスポーン間隔短縮率（0〜0.9程度推奨）")]
    [SerializeField, Range(0f, 0.9f)] private float _tensionSpawnBoost = 0.5f;

    // ====== 内部状態 ======
    private RectTransform _rect;
    private Transform _t;
    private Sequence _swaySeq;
    private int _currentVariantIndex = 0;

    private MouthState _mouthState = MouthState.Closed;
    private float _mouthOpenUntil = 0f;
    private float _mouthNormalUntil = 0f;
    private float _nextPoseChangeAt = Mathf.Infinity;
    private bool _talkingManual = false;

	private float _nextExpressionAt = 0f;

    // 追加：初期スケール保存（現在の大きさを基準化）
    private Vector3 _initialScale;

    // Notes 内部状態
    private float _nextNoteAt = Mathf.Infinity;
    private float _currentTensionRatio = 0f;

    // ====== Tension Integration ======
    [Header("Tension Integration")]
    [SerializeField] private PopTensionGauge _tensionGauge; // 任意参照（未設定なら自動検索）
    [SerializeField] private GameObject _hyperEffectPrefab; // ハイテンション突入時の単発エフェクト
    [SerializeField] private Transform _effectAttachRoot;    // エフェクトをぶら下げるルート（未設定なら自身）
    [SerializeField, Tooltip("ハイテンション時に使うバリエーション。-1で無効")] private int _hyperVariantIndex = -1;

    [Header("Hyper Reaction Settings")]
    [SerializeField] private float _spinDuration = 0.6f;
    [SerializeField] private int _spinTurns = 1;
    [SerializeField] private float _hopHeight = 60f;
    [SerializeField] private float _hopDuration = 0.25f;
    [SerializeField] private float _hyperPunchScale = 0.15f;

    private Sequence _hyperSeq;
    private TensionState _lastTensionState;

    // Sway base values for tension scaling
    private float _baseSwayAngle;
    private float _basePosAmplitudeX;
    private float _baseScaleAmplitude;
    private float _baseSwayDuration;
    private int _defaultVariantIndex;
    private float _baseAnchoredPosX;
    private float _baseLocalPosX;

    // ====== Stream End Outro ======
    [Header("Stream End Outro")]
    [SerializeField] private GameObject _speechBubble;    // 吹き出し（子にTMPがある想定）
    [SerializeField] private TextMeshProUGUI _speechText; // 任意: セリフ表示用（_speechBubble未設定なら直接指定）
     private string _outroMessage = "配信終了！\nまた来てね〜";
    [SerializeField] private float _speechFadeIn = 0.25f;
    [SerializeField] private float _speechDuration = 1.2f;
    [SerializeField] private float _speechFadeOut = 0.25f;
    [SerializeField] private float _outroJumpHeight = 80f;
    [SerializeField] private float _outroJumpDuration = 0.42f;
    [SerializeField] private float _outroSinkDistance = 600f; // 下方向へ潜る距離
    [SerializeField] private float _outroSinkDuration = 0.5f;
    private bool _isPlayingOutro;

    private void Reset()
    {
        _uiImage = GetComponent<Image>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        _t = transform;
        _rect = GetComponent<RectTransform>();
        if (_uiImage == null) _uiImage = GetComponent<Image>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        _initialScale = _t.localScale; // ← Inspectorの現在値を保持

        // Save base sway values to scale by tension later
        _baseSwayAngle = _swayAngle;
        _basePosAmplitudeX = _posAmplitudeX;
        _baseScaleAmplitude = _scaleAmplitude;
        _baseSwayDuration = _swayDuration;
        _defaultVariantIndex = _currentVariantIndex;

        if (_rect != null)
        {
            _baseAnchoredPosX = _rect.anchoredPosition.x;
        }
        else
        {
            _baseLocalPosX = _t.localPosition.x;
        }

        // 最低1セットは必要
        if (_variants == null || _variants.Count == 0)
        {
            Debug.LogWarning("[SwayAndLipSync] _variants が未設定です。口開閉は無効になります。");
        }

        // 吹き出し/テキストはゲーム開始時は非表示
        if (_speechBubble != null)
        {
            _speechBubble.SetActive(false);
        }
        else if (_speechText != null)
        {
            _speechText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SetupSway();
        ApplyClosedSprite();

		

        // Notes スケジュール開始
        ScheduleNextNote();

        ScheduleNextPoseChange();

        // Tension subscribe
        if (_tensionGauge == null)
        {
            _tensionGauge = FindObjectOfType<PopTensionGauge>();
        }
        if (_tensionGauge != null)
        {
            _tensionGauge.OnStateChanged += HandleTensionStateChanged;
            _tensionGauge.OnTensionChanged += HandleTensionValueChanged;
            _lastTensionState = _tensionGauge.CurrentState;
            ApplyTension(_tensionGauge.TensionRatio, _tensionGauge.CurrentState);
            _currentTensionRatio = _tensionGauge.TensionRatio;
        }
    }

    private void OnDisable()
    {
        _swaySeq?.Kill();
        _hyperSeq?.Kill();
		

        if (_tensionGauge != null)
        {
            _tensionGauge.OnStateChanged -= HandleTensionStateChanged;
            _tensionGauge.OnTensionChanged -= HandleTensionValueChanged;
        }
        _nextNoteAt = Mathf.Infinity;
    }

    // ====== Sway（左右ゆらぎ） ======
    private void SetupSway()
    {
        _swaySeq?.Kill();
        _t.DOKill();
        _rect?.DOKill();

        // 初期スケールを尊重（Vector3.oneに戻さない）
        _t.localScale = _initialScale;

        if (_rect != null)
        {
            // RectTransform の場合：PositionXをゆらす（基準Xに毎回リセット）
            var startAnchored = _rect.anchoredPosition;
            startAnchored.x = _baseAnchoredPosX;
            _rect.anchoredPosition = startAnchored;
            float startX = _baseAnchoredPosX;
            _swaySeq = DOTween.Sequence();
            _swaySeq.Append(_t.DORotate(new Vector3(0f, 0f, _swayAngle), _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_rect.DOAnchorPosX(startX + _posAmplitudeX, _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_t.DOScale(_initialScale * (1f + _scaleAmplitude), _swayDuration).SetEase(_swayEase));
            _swaySeq.Append(_t.DORotate(new Vector3(0f, 0f, -_swayAngle), _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_rect.DOAnchorPosX(startX - _posAmplitudeX, _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_t.DOScale(_initialScale * (1f - _scaleAmplitude), _swayDuration).SetEase(_swayEase));
            _swaySeq.SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            // Transform/SpriteRenderer の場合：LocalPositionXをゆらす（基準Xに毎回リセット）
            var startLocal = _t.localPosition;
            startLocal.x = _baseLocalPosX;
            _t.localPosition = startLocal;
            float startLocalX = _baseLocalPosX;
            _swaySeq = DOTween.Sequence();
            _swaySeq.Append(_t.DORotate(new Vector3(0f, 0f, _swayAngle), _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_t.DOLocalMoveX(startLocalX + _posAmplitudeX, _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_t.DOScale(_initialScale * (1f + _scaleAmplitude), _swayDuration).SetEase(_swayEase));
            _swaySeq.Append(_t.DORotate(new Vector3(0f, 0f, -_swayAngle), _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_t.DOLocalMoveX(startLocalX - _posAmplitudeX, _swayDuration).SetEase(_swayEase));
            _swaySeq.Join(_t.DOScale(_initialScale * (1f - _scaleAmplitude), _swayDuration).SetEase(_swayEase));
            _swaySeq.SetLoops(-1, LoopType.Yoyo);
        }
    }

    // ====== Update ======
    private void Update()
    {
        // アウトロ中は左右の揺れ・表情変化（口パク/ポーズ変更/テンション反映）を停止
        if (_isPlayingOutro)
        {
            return;
        }

        // デバッグ：手動トグル
        if (_toggleTalkingKey != KeyCode.None && Input.GetKeyDown(_toggleTalkingKey))
        {
            _talkingManual = !_talkingManual;
        }

        // ノーツの自動生成
        if (_notesEnabled && HasNotePrefab() && Time.unscaledTime >= _nextNoteAt)
        {
            SpawnNote();
            ScheduleNextNote();
        }

        // ポーズの自動切替
        if (Time.unscaledTime >= _nextPoseChangeAt)
        {
            if (_poseChangeIntervalSec.y > 0f && _variants.Count > 1)
            {
                _currentVariantIndex = UnityEngine.Random.Range(0, _variants.Count);
                // 口の状態を保ったまま対応スプライトへ
                switch (_mouthState)
                {
                    case MouthState.Open:
                        ApplyOpenSprite();
                        break;
                    case MouthState.Normal:
                        ApplyNormalSprite();
                        break;
                    default:
                        ApplyClosedSprite();
                        break;
                }
            }
            ScheduleNextPoseChange();
        }

		// 表情制御（秒数で適当に変化）
		if (_variants.Count > 0)
		{
			float now = Time.unscaledTime;

			// デバッグの手動トグルがONなら強制的にOpenを維持延長
			bool forceOpen = (_toggleTalkingKey != KeyCode.None && _talkingManual);
			if (forceOpen)
			{
				float hold = UnityEngine.Random.Range(_openHoldTimeRange.x, _openHoldTimeRange.y);
				_mouthOpenUntil = now + hold;
			}

			// 次の表情更新タイミング
			if (now >= _nextExpressionAt)
			{
				// ランダムに状態を選択（Open/Normal/Closed）
				float r = UnityEngine.Random.value;
				if (r < _openProbability)
				{
					float hold = UnityEngine.Random.Range(_openHoldTimeRange.x, _openHoldTimeRange.y);
					_mouthOpenUntil = now + hold;
				}
				else if (r < _openProbability + _normalProbability)
				{
					float holdN = UnityEngine.Random.Range(_normalHoldTimeRange.x, _normalHoldTimeRange.y);
					_mouthNormalUntil = now + holdN;
				}
				else
				{
					_mouthOpenUntil = 0f;
					_mouthNormalUntil = 0f;
				}

				// 次の更新までの時間を設定
				float next = UnityEngine.Random.Range(_expressionChangeIntervalSec.x, _expressionChangeIntervalSec.y);
				_nextExpressionAt = now + Mathf.Max(0.05f, next);
			}

			// 実効ステートを決める（Open優先 → Normal → Closed）
			MouthState effectiveState = MouthState.Closed;
			if (now < _mouthOpenUntil)
			{
				effectiveState = MouthState.Open;
			}
			else if (now < _mouthNormalUntil)
			{
				effectiveState = MouthState.Normal;
			}
			else
			{
				effectiveState = MouthState.Closed;
			}

			// 状態が変わったときだけスプライト差し替え
			if (effectiveState != _mouthState)
			{
				_mouthState = effectiveState;
				switch (_mouthState)
				{
					case MouthState.Open:
						ApplyOpenSprite();
						break;
					case MouthState.Normal:
						ApplyNormalSprite();
						break;
					default:
						ApplyClosedSprite();
						break;
				}
			}
		}
    }

    // Mic 入力は廃止

    // ====== Sprites ======
    private void ApplyClosedSprite()
    {
        if (_variants.Count == 0) return;
        var set = _variants[_currentVariantIndex];
        var s = set._closed;
        if (_uiImage != null) _uiImage.sprite = s;
        if (_spriteRenderer != null) _spriteRenderer.sprite = s;
    }

    private void ApplyNormalSprite()
    {
        if (_variants.Count == 0) return;
        var set = _variants[_currentVariantIndex];
        var s = set._normal != null ? set._normal : set._closed;
        if (_uiImage != null) _uiImage.sprite = s;
        if (_spriteRenderer != null) _spriteRenderer.sprite = s;
    }

    private void ApplyOpenSprite()
    {
        if (_variants.Count == 0) return;
        var set = _variants[_currentVariantIndex];
        var s = set._open != null ? set._open : set._closed;
        if (_uiImage != null) _uiImage.sprite = s;
        if (_spriteRenderer != null) _spriteRenderer.sprite = s;
    }

    private void ApplySmileSprite()
    {
        if (_smileSprite == null)
        {
            // フォールバック: 現在のnormal/closedのどちらか
            if (_variants.Count > 0)
            {
                var set = _variants[_currentVariantIndex];
                var s = set._normal != null ? set._normal : set._closed;
                if (_uiImage != null) _uiImage.sprite = s;
                if (_spriteRenderer != null) _spriteRenderer.sprite = s;
            }
            return;
        }

        if (_uiImage != null) _uiImage.sprite = _smileSprite;
        if (_spriteRenderer != null) _spriteRenderer.sprite = _smileSprite;
    }

    private void ScheduleNextPoseChange()
    {
        if (_poseChangeIntervalSec.y <= 0f)
        {
            _nextPoseChangeAt = Mathf.Infinity;
            return;
        }
        float t = UnityEngine.Random.Range(_poseChangeIntervalSec.x, _poseChangeIntervalSec.y);
        _nextPoseChangeAt = Time.unscaledTime + Mathf.Max(0.5f, t);
    }

    // ====== Public API ======
    /// <summary>外部から「今は喋ってる扱い」にする（マイク無効時の簡易口パク用）</summary>
    public void SetTalking(bool talking)
    {
        if (talking)
        {
            float hold = UnityEngine.Random.Range(_openHoldTimeRange.x, _openHoldTimeRange.y);
            _mouthOpenUntil = Time.unscaledTime + hold;
        }
        else
        {
            _mouthOpenUntil = 0f;
            _mouthNormalUntil = 0f;
        }
    }

    /// <summary>バリエーションを次へ（手動）</summary>
    public void NextVariant()
    {
        if (_variants.Count <= 1) return;
        _currentVariantIndex = (_currentVariantIndex + 1) % _variants.Count;
        switch (_mouthState)
        {
            case MouthState.Open:
                ApplyOpenSprite();
                break;
            case MouthState.Normal:
                ApplyNormalSprite();
                break;
            default:
                ApplyClosedSprite();
                break;
        }
    }

    /// <summary>バリエーションを指定番号に</summary>
    public void SetVariant(int index)
    {
        if (_variants.Count == 0) return;
        _currentVariantIndex = Mathf.Clamp(index, 0, _variants.Count - 1);
        switch (_mouthState)
        {
            case MouthState.Open:
                ApplyOpenSprite();
                break;
            case MouthState.Normal:
                ApplyNormalSprite();
                break;
            default:
                ApplyClosedSprite();
                break;
        }
    }

    // ====== Tension Hooks ======
    private void HandleTensionStateChanged(TensionState newState)
    {
        if (_isPlayingOutro) return;
        // Variant swap
        if (_hyperVariantIndex >= 0)
        {
            if (newState == TensionState.Hyper)
            {
                SetVariant(Mathf.Clamp(_hyperVariantIndex, 0, Mathf.Max(0, _variants.Count - 1)));
            }
            else
            {
                SetVariant(Mathf.Clamp(_defaultVariantIndex, 0, Mathf.Max(0, _variants.Count - 1)));
            }
        }

        if (newState == TensionState.Hyper && _lastTensionState != TensionState.Hyper)
        {
            PlayHyperReaction();
        }

        _lastTensionState = newState;
    }

    private void HandleTensionValueChanged(float _)
    {
        if (_isPlayingOutro) return;
        if (_tensionGauge == null) return;
        ApplyTension(_tensionGauge.TensionRatio, _tensionGauge.CurrentState);
    }

    private void ApplyTension(float ratio, TensionState state)
    {
        if (_isPlayingOutro) return;
        _currentTensionRatio = ratio;
        // Scale sway intensity/speed by tension ratio
			// 現在のInspector値を「最大」とし、テンションが下がると小さくなるだけ
			_swayAngle = _baseSwayAngle * Mathf.Lerp(0.5f, 1.0f, ratio);
			_posAmplitudeX = _basePosAmplitudeX * Mathf.Lerp(0.5f, 1.0f, ratio);
			_scaleAmplitude = _baseScaleAmplitude * Mathf.Lerp(0.5f, 1.0f, ratio);
			// 速度は最大（=現在値）を上限に、テンション低下でゆっくりに
			_swayDuration = _baseSwayDuration * Mathf.Lerp(1.2f, 1.0f, ratio);

        SetupSway();
    }

    private void PlayHyperReaction()
    {
        _hyperSeq?.Kill();
        _swaySeq?.Kill();
        _t.DOKill();
        _rect?.DOKill();
        _t.localRotation = Quaternion.identity;
        _t.localScale = _initialScale;

        float ratio = (_tensionGauge != null) ? _tensionGauge.TensionRatio : 1f;
        float hop = _hopHeight * Mathf.Lerp(0.6f, 1.2f, ratio);

        var seq = DOTween.Sequence();
        seq.AppendInterval(0.05f); // 一瞬静止

        if (_rect != null)
        {
            float startY = _rect.anchoredPosition.y;
            seq.Append(_rect.DOAnchorPosY(startY + hop, _hopDuration).SetEase(Ease.OutQuad));
            seq.AppendInterval(0.05f); // 頂点で少し止める
            seq.Append(_rect.DOAnchorPosY(startY, _hopDuration).SetEase(Ease.InQuad));
        }
        else
        {
            float startY = _t.localPosition.y;
            seq.Append(_t.DOLocalMoveY(startY + hop, _hopDuration).SetEase(Ease.OutQuad));
            seq.AppendInterval(0.05f); // 頂点で少し止める
            seq.Append(_t.DOLocalMoveY(startY, _hopDuration).SetEase(Ease.InQuad));
        }

        // Punch scale
        if (_hyperPunchScale > 0f)
        {
            seq.Join(_t.DOPunchScale(Vector3.one * _hyperPunchScale, _hopDuration + 0.1f, 8, 0.9f));
        }

        // After reaction, resume sway
        _hyperSeq = seq.OnComplete(() =>
        {
            SetupSway();
        });

        // One-shot effect
        if (_hyperEffectPrefab != null)
        {
            Transform root = _effectAttachRoot != null ? _effectAttachRoot : _t;
            var fx = Instantiate(_hyperEffectPrefab, root);
            fx.transform.localPosition = Vector3.zero;
            Destroy(fx, 2f);
        }
    }

    // ====== Notes Utilities ======
    private void ScheduleNextNote()
    {
        if (!_notesEnabled || !HasNotePrefab())
        {
            _nextNoteAt = Mathf.Infinity;
            return;
        }
        float min = Mathf.Max(0.05f, _noteSpawnInterval.x);
        float max = Mathf.Max(min + 0.001f, _noteSpawnInterval.y);
        float baseInterval = UnityEngine.Random.Range(min, max);
			// 現在値（baseInterval）を最大出現頻度とし、テンションが下がるとゆっくり（間隔を長く）する
			float slow = Mathf.Clamp01(_tensionSpawnBoost); // 0〜1: 低テンション時の追加遅延量
			// 低テンションでの最低出現間隔を保証（例: 4秒）
			float minIntervalTarget = Mathf.Max(0.05f, _minNoteIntervalAtLowTension);
			float factorLow = Mathf.Max(1f + slow, minIntervalTarget / Mathf.Max(0.01f, baseInterval));
			float factor = Mathf.Lerp(factorLow, 1f, Mathf.Clamp01(_currentTensionRatio));
        float next = baseInterval * factor;
        _nextNoteAt = Time.unscaledTime + next;
    }

    private void SpawnNote()
    {
        Transform parent = _noteSpawnRoot != null ? _noteSpawnRoot : (_rect != null ? (Transform)_rect : _t);
        if (parent == null) parent = _t;

        Vector2 dir2 = _noteDirection.sqrMagnitude < 1e-4f ? new Vector2(1f, 1f) : _noteDirection.normalized;
        float distance = UnityEngine.Random.Range(_noteDistanceRange.x, _noteDistanceRange.y);
        float life = UnityEngine.Random.Range(_noteLifetimeRange.x, _noteLifetimeRange.y);
        float startScale = UnityEngine.Random.Range(_noteScaleRange.x, _noteScaleRange.y);
        Vector2 offset2 = UnityEngine.Random.insideUnitCircle * _noteSpawnRadius;

        var prefab = ChooseNotePrefab();
        if (prefab == null) return;
        var go = Instantiate(prefab, parent);

        if (_rect != null)
        {
            // UI: RectTransform空間で移動
            var emitRt = _noteEmitPoint as RectTransform;
            Vector2 start = (emitRt != null) ? emitRt.anchoredPosition : Vector2.zero;
            start += offset2;
            Vector2 end = start + dir2 * distance;

            var noteRt = go.GetComponent<RectTransform>();
            if (noteRt == null) noteRt = go.AddComponent<RectTransform>();
            noteRt.anchorMin = noteRt.anchorMax = _rect.pivot;
            noteRt.anchoredPosition = start;
            noteRt.localScale = Vector3.one * startScale;

            // 色・フェード
            var cg = go.GetComponent<CanvasGroup>();
            var g = go.GetComponent<Graphic>();
            if (g != null)
            {
                g.color = _noteColor;
                g.DOFade(0f, life * 0.6f).SetDelay(life * 0.4f);
            }
            else if (cg != null)
            {
                cg.alpha = _noteColor.a;
                cg.DOFade(0f, life * 0.6f).SetDelay(life * 0.4f);
            }

            // 移動・スケール・回転
            noteRt.DOAnchorPos(end, life).SetEase(Ease.OutQuad);
            noteRt.DOScale(startScale * 0.9f, life).SetEase(Ease.OutSine);
            noteRt.DORotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-45f, 45f)), life).SetEase(Ease.Linear);
        }
        else
        {
            // 2D: ローカル空間で移動
            var tr = go.transform;
            Vector3 startLocal = Vector3.zero + new Vector3(offset2.x, offset2.y, 0f);
            tr.localPosition = startLocal;
            tr.localScale = Vector3.one * startScale;

            Vector3 endLocal = startLocal + new Vector3(dir2.x, dir2.y, 0f) * distance;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = _noteColor;
                if (sr.color.a != c.a || sr.color != c)
                {
                    sr.color = c;
                }
                sr.DOFade(0f, life * 0.6f).SetDelay(life * 0.4f);
            }

            tr.DOLocalMove(endLocal, life).SetEase(Ease.OutQuad);
            tr.DOScale(startScale * 0.9f, life).SetEase(Ease.OutSine);
            tr.DORotate(new Vector3(0f, 0f, UnityEngine.Random.Range(60f, 140f)), life, RotateMode.FastBeyond360).SetEase(Ease.Linear);
        }

        Destroy(go, life + 0.1f);
    }

    private bool HasNotePrefab()
    {
        return (_notePrefabs != null && _notePrefabs.Count > 0) || _notePrefab != null;
    }

    private GameObject ChooseNotePrefab()
    {
        if (_notePrefabs != null && _notePrefabs.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, _notePrefabs.Count);
            return _notePrefabs[idx];
        }
        return _notePrefab;
    }

    // ====== Public Outro API ======
    /// <summary>
    /// タイムアップ後の演出：セリフ表示 → ちょいジャンプ → 下へ潜って非表示。
    /// </summary>
    public async Task PlayStreamEndOutroAsync()
    {
        if (_isPlayingOutro) return;
        _isPlayingOutro = true;

        // 停止・初期化
        _swaySeq?.Kill();
        _hyperSeq?.Kill();
        _t.DOKill();
        _rect?.DOKill();

		// マイク未使用

        // 表情をスマイルに固定
        ApplySmileSprite();

        // セリフ表示（吹き出し優先）
        if (_speechText == null && _speechBubble != null)
        {
            _speechText = _speechBubble.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        GameObject bubbleGo = _speechBubble != null ? _speechBubble : (_speechText != null ? _speechText.gameObject : null);

        if (_speechText != null && bubbleGo != null)
        {
            bubbleGo.SetActive(true);
            _speechText.text = _outroMessage;
            var rt = _speechText.rectTransform;
            float startScale = 0.85f;
            Color c = _speechText.color; c.a = 0f; _speechText.color = c;
            rt.localScale = Vector3.one * startScale;

            // フェードイン + スケールアップ（口パクは行わない）
            float tIn = 0f;
            while (tIn < _speechFadeIn)
            {
                tIn += Time.unscaledDeltaTime;
                float r = Mathf.Clamp01(tIn / _speechFadeIn);
                _speechText.color = new Color(c.r, c.g, c.b, r);
                rt.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, r);
                await Task.Yield();
            }

            // 表示維持（口パクは行わない）
            float tHold = 0f;
            while (tHold < _speechDuration)
            {
                tHold += Time.unscaledDeltaTime;
                await Task.Yield();
            }

            // フェードアウト
            float tOut = 0f;
            Color c2 = _speechText.color;
            while (tOut < _speechFadeOut)
            {
                tOut += Time.unscaledDeltaTime;
                float r = 1f - Mathf.Clamp01(tOut / _speechFadeOut);
                _speechText.color = new Color(c2.r, c2.g, c2.b, r);
                await Task.Yield();
            }
            bubbleGo.SetActive(false);
        }

        // ちょいジャンプ
        if (_rect != null)
        {
            float y0 = _rect.anchoredPosition.y;
            var upDown = _rect
                .DOAnchorPosY(y0 + _outroJumpHeight, _outroJumpDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo);
            await upDown.AsyncWaitForCompletion();

            // 下へ潜る
            await _rect.DOAnchorPosY(y0 - _outroSinkDistance, _outroSinkDuration).SetEase(Ease.InCubic).AsyncWaitForCompletion();
        }
        else
        {
            float y0 = _t.localPosition.y;
            var upDown = _t
                .DOLocalMoveY(y0 + _outroJumpHeight, _outroJumpDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo);
            await upDown.AsyncWaitForCompletion();

            // 下へ潜る
            await _t.DOLocalMoveY(y0 - _outroSinkDistance, _outroSinkDuration).SetEase(Ease.InCubic).AsyncWaitForCompletion();
        }

		// 下へ潜った後に、TimeManager経由でゲームオーバーパネルをスライド表示（GameManagerは触らない）
		var tm = FindObjectOfType<TimeManager>();
		if (tm != null)
		{
			tm.ShowGameOverPanelSlideIn();
		}

        // 最後に非表示（お好みで）
        gameObject.SetActive(false);
    }
}
