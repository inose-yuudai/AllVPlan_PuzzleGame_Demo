using UnityEngine;

namespace EmoteOrchestra.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        [Header("Clips")]
        [SerializeField] private AudioClip _selectClip;
        [SerializeField] private AudioClip _swapClip;
        [SerializeField] private AudioClip _matchClip;
        [SerializeField] private AudioClip _comboStepClip;
        [SerializeField] private AudioClip _subscriberClip;
        [SerializeField] private AudioClip _mainBgmClip;
        [SerializeField] private AudioClip _ButtonClickClip;

        [Header("Volumes")]
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float _bgmVolume = 0.5f;

        [Header("BGM")]
        [SerializeField] private bool _autoPlayBgm = true;

        [Header("Combo Pitch")] 
        [SerializeField] private float _comboBasePitch = 1.0f;
        [SerializeField] private float _comboPitchStep = 0.07f;

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.volume = _sfxVolume;

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.volume = _bgmVolume;
        }

        private void Start()
        {
            if (_autoPlayBgm && _mainBgmClip != null)
            {
                PlayMainBgm();
            }
        }

        public void PlayMainBgm()
        {
            if (_mainBgmClip == null)
                return;

            _bgmSource.clip = _mainBgmClip;
            _bgmSource.volume = _bgmVolume;
            _bgmSource.Play();
        }

        public void StopMainBgm()
        {
            _bgmSource.Stop();
        }

        public void PlaySelect()
        {
            PlayOneShot(_selectClip);
        }

        public void PlaySwap()
        {
            PlayOneShot(_swapClip);
        }
        public void PlayButtonClick()
        {
            PlayOneShot(_ButtonClickClip);
        }

        public void PlayMatch()
        {
            PlayOneShot(_matchClip);
        }

        public void PlayComboStep(int stepIndex)
        {
            if (_comboStepClip == null)
                return;

            float pitch = Mathf.Clamp(_comboBasePitch + _comboPitchStep * Mathf.Max(0, stepIndex), 0.1f, 3.0f);
            _sfxSource.pitch = pitch;
            _sfxSource.volume = _sfxVolume;
            _sfxSource.PlayOneShot(_comboStepClip);
            // すぐに戻すと再生に影響しないが、連続時の揺らぎを避けるために1フレーム後に戻す
            StartCoroutine(ResetPitchNextFrame());
        }

        public void PlaySubscriber()
        {
            PlayOneShot(_subscriberClip);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
                return;
            _sfxSource.pitch = 1f;
            _sfxSource.volume = _sfxVolume;
            _sfxSource.PlayOneShot(clip);
        }

        private System.Collections.IEnumerator ResetPitchNextFrame()
        {
            yield return null;
            _sfxSource.pitch = 1f;
        }
    }
}



