using UnityEngine;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Core;
using EmoteOrchestra.Audio;

namespace EmoteOrchestra.UI
{
	public class SubscriberCountManager : MonoBehaviour
	{
		[Header("UI要素")]
		[SerializeField] private TextMeshProUGUI _subscriberCountText;
		[SerializeField] private TextMeshProUGUI _subscriberChangeText;

		[Header("登録者数の設定")]
		[SerializeField] private int _initialSubscriberCount = 123;
		[SerializeField] private int _maxSubscriberCount = 9999999;
		[SerializeField] private int _minSubscriberCount = 0;
		[SerializeField, Tooltip("ゲーム開始時に SubscribeManager.mainSubscriberCount を初期値として読み込む")]
		private bool _useSubscribeManagerInitial = true;
		[SerializeField, Tooltip("終了時に増加分を SubscribeManager.mainAddSubscriberCount に書き戻す")]
		private bool _exportDeltaOnGameEnd = true;

		[Header("アニメーション設定")]
		[SerializeField] private float _countAnimationDuration = 0.5f;
		[SerializeField] private bool _enableShakeOnDecrease = true;

		[Header("表示設定")]
		[SerializeField] private int _commaFontSize = 20;

		private static int _currentSubscriberCount;
		private int _displaySubscriberCount;
		private int _sessionInitialSubscriberCount;

		private Tweener _countTweener;
		private Sequence _changeTextSequence;
		private Tweener _shakeTweener;

		// 元の位置を保存
		private Vector3 _originalSubscriberCountPosition;
		private Vector3 _originalSubscriberChangePosition;

		public int CurrentSubscriberCount => _currentSubscriberCount;

		private void Start()
		{
			// セッション初期値の決定（SubscribeManager優先）
			int startValue = _initialSubscriberCount;
			if (_useSubscribeManagerInitial)
			{
				startValue = Mathf.Max(0, SubscribeManager.mainSubscriberCount);
			}
			_sessionInitialSubscriberCount = startValue;
			_currentSubscriberCount = startValue;
			_displaySubscriberCount = startValue;

			if (_subscriberCountText != null)
			{
				_originalSubscriberCountPosition = _subscriberCountText.transform.localPosition;
			}

			if (_subscriberChangeText != null)
			{
				_originalSubscriberChangePosition = _subscriberChangeText.transform.localPosition;
				_subscriberChangeText.gameObject.SetActive(false);
			}

			UpdateSubscriberCountImmediate(_currentSubscriberCount);

			// タイムアップ時に増加分を書き戻す
			if (_exportDeltaOnGameEnd)
			{
				var tm = FindObjectOfType<TimeManager>();
				if (tm != null)
				{
					tm.SetOnTimeUpListener(ExportDeltaToSubscribeManager);
				}
			}
		}

		private string FormatWithSizedCommas(string numericText)
		{
			if (string.IsNullOrEmpty(numericText)) return numericText;
			string sizedComma = $"<size={_commaFontSize}>,</size>";
			return numericText.Replace(",", sizedComma);
		}

		private string FormatNumberForTMP(int value)
		{
			return FormatWithSizedCommas(value.ToString("N0"));
		}

		private void UpdateSubscriberCountImmediate(int count)
		{
			_currentSubscriberCount = Mathf.Clamp(count, _minSubscriberCount, _maxSubscriberCount);
			_displaySubscriberCount = _currentSubscriberCount;

			if (_subscriberCountText != null)
			{
				_subscriberCountText.text = FormatNumberForTMP(_displaySubscriberCount);
			}
		}

		public void AddSubscribers(int amount)
		{
			int newCount = _currentSubscriberCount + amount;
			UpdateSubscriberCountAnimated(newCount, amount);
			AudioManager.Instance?.PlaySubscriber();
		}

		public void DecreaseSubscribers(int amount)
		{
			// ゲーム開始時の値からは絶対に減らさない（下限をセッション初期値に固定）
			int proposed = _currentSubscriberCount - amount;
			int clamped = Mathf.Max(_sessionInitialSubscriberCount, proposed);
			// 実際の変化量（負にならないように）
			int effectiveChange = clamped - _currentSubscriberCount;
			if (effectiveChange != 0)
			{
				UpdateSubscriberCountAnimated(clamped, effectiveChange);
			}
			// 減少しない場合は何もしない
		}

		private void UpdateSubscriberCountAnimated(int newCount, int change)
		{
			_currentSubscriberCount = Mathf.Clamp(newCount, _minSubscriberCount, _maxSubscriberCount);

			_countTweener?.Kill();
			_countTweener = DOTween.To(
				() => _displaySubscriberCount,
				x =>
				{
					_displaySubscriberCount = x;
					if (_subscriberCountText != null)
					{
						_subscriberCountText.text = FormatNumberForTMP(_displaySubscriberCount);
					}
				},
				_currentSubscriberCount,
				_countAnimationDuration
			).SetEase(Ease.OutQuad);

			ShowSubscriberChange(change);

			if (change < 0 && _enableShakeOnDecrease && _subscriberCountText != null)
			{
				PlayShakeEffect();
			}
		}

		private void PlayShakeEffect()
		{
			if (_subscriberCountText == null)
				return;

			_shakeTweener?.Kill();

			_subscriberCountText.transform.localPosition = _originalSubscriberCountPosition;

			_shakeTweener = _subscriberCountText.transform.DOShakePosition(
				duration: 0.3f,
				strength: 10f,
				vibrato: 20,
				randomness: 90,
				snapping: false,
				fadeOut: true
			).OnComplete(() =>
			{
				if (_subscriberCountText != null)
				{
					_subscriberCountText.transform.localPosition = _originalSubscriberCountPosition;
				}
			});
		}

		private void ShowSubscriberChange(int change)
		{
			if (_subscriberChangeText == null)
				return;

			_changeTextSequence?.Kill();

			string sign = change > 0 ? "+" : "";
			int abs = Mathf.Abs(change);
			string body = FormatWithSizedCommas(abs.ToString("N0"));
			_subscriberChangeText.text = sign + body;

			// 位置は触らず、その場から出す
			_subscriberChangeText.transform.localScale = Vector3.one * 0.5f;
			_subscriberChangeText.alpha = 1f;
			_subscriberChangeText.gameObject.SetActive(true);

			Vector3 startPos = _subscriberChangeText.transform.localPosition;
			Vector3 endPos = startPos + Vector3.up * 50f;

			_changeTextSequence = DOTween.Sequence()
				.Append(_subscriberChangeText.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack))
				.Join(_subscriberChangeText.transform.DOLocalMove(endPos, 1.5f).SetEase(Ease.OutQuad))
				.Join(_subscriberChangeText.DOFade(0f, 1.5f).SetEase(Ease.InQuad))
				.OnComplete(() =>
				{
					_subscriberChangeText.gameObject.SetActive(false);
					_subscriberChangeText.transform.localPosition = startPos;
					_subscriberChangeText.alpha = 1f;
				});
		}

		public void ResetSubscriberCount()
		{
			_sessionInitialSubscriberCount = _useSubscribeManagerInitial
				? Mathf.Max(0, SubscribeManager.mainSubscriberCount)
				: _initialSubscriberCount;
			UpdateSubscriberCountImmediate(_sessionInitialSubscriberCount);

			if (_subscriberCountText != null)
			{
				_subscriberCountText.transform.localPosition = _originalSubscriberCountPosition;
			}

			if (_subscriberChangeText != null)
			{
				_subscriberChangeText.gameObject.SetActive(false);
				_subscriberChangeText.transform.localPosition = _originalSubscriberChangePosition;
				_subscriberChangeText.alpha = 1f;
			}
		}

		/// <summary>
		/// セッションで増えた登録者数を SubscribeManager.mainAddSubscriberCount に書き戻す
		/// </summary>
		public void ExportDeltaToSubscribeManager()
		{
			int delta = Mathf.Max(0, _currentSubscriberCount - _sessionInitialSubscriberCount);
			SubscribeManager.mainAddSubscriberCount = delta;
		}

		private void OnDestroy()
		{
			_countTweener?.Kill();
			_changeTextSequence?.Kill();
			_shakeTweener?.Kill();
		}

		[ContextMenu("Reset Original Position")]
		public void ResetOriginalPosition()
		{
			if (_subscriberCountText != null)
			{
				_originalSubscriberCountPosition = _subscriberCountText.transform.localPosition;
			}

			if (_subscriberChangeText != null)
			{
				_originalSubscriberChangePosition = _subscriberChangeText.transform.localPosition;
			}
		}
	}
}


