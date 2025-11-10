using System;
using System.Collections.Generic;
using _Workspace._Scripts.ISingleton;
using UnityEngine;

namespace _Workspace._Scripts.Managers
{
    public class ScAudioManager : ISingleton<ScAudioManager>
    {
        [field: Header("Event Actions")]
        public event Action<float> OnBGMVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;

        [Header("Audio Source")]
        public AudioSource bgmSource;
        [SerializeField] protected AudioSource sfxSource;

        [Header("Volumes")]
        [SerializeField, Range(0f, 1f)] private float bgmVolume;
        [SerializeField, Range(0f, 1f)] private float sfxVolume;

        [Header("SFX Setting")]
        [SerializeField] private SfxSound[] sfxClips;
        private Dictionary<string, AudioClip> _sfxDict;
        
        [Header("BGM Setting")]
        [SerializeField] private BGM[] bgmClips;
        private Dictionary<string, AudioClip> _bgmDict;

        private float _lastBgmVolume;
        private float _lastSfxVolume;

        public bool IsBgmMuted { get; private set; }
        public bool IsSfxMuted { get; private set; }
        public bool IsBgmPlaying { get; private set; }
        public float SfxVolume => sfxVolume;
        public float BgmVolume => bgmVolume;


        [Serializable]
        public class SfxSound
        {
            public string name;
            public AudioClip clip;
        }
        
        [Serializable]
        public class BGM
        {
            public string name;
            public AudioClip clip;
        }
        

        private void OnEnable()
        {
            BuildSfxDictionary();
            BuildBgmDictionary();
            LoadVolumeSettings();
            PlayBGM("BGM 1");
            IsBgmPlaying = true;
        }

        private void BuildSfxDictionary()
        {
            _sfxDict = new Dictionary<string, AudioClip>();
            foreach (var sound in sfxClips)
            {
                if (!_sfxDict.ContainsKey(sound.name) && sound.clip != null)
                    _sfxDict.Add(sound.name, sound.clip);
            }
        }
        
        private void BuildBgmDictionary()
        {
            _bgmDict = new Dictionary<string, AudioClip>();
            foreach (var bgm in bgmClips)
            {
               if(!_bgmDict.ContainsKey(bgm.name) && bgm.clip != null)
                   _bgmDict.Add(bgm.name, bgm.clip);
            }
        }

        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null || clip == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
        
        public void PlayBGM(string bgmName, bool loop = true)
        {
            if (bgmSource == null || string.IsNullOrEmpty(bgmName)) return;
            if (_bgmDict.TryGetValue(bgmName, out AudioClip clip) && clip != null)
                PlayBGM(clip, loop);
            IsBgmPlaying = true;
        }

        public void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
            IsBgmPlaying = false;
        }

        public void PauseBGM()
        {
            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.Pause();
            IsBgmPlaying = false;
        }

        public void PlaySfx(string sfxName)
        {
            if (sfxSource is null || string.IsNullOrEmpty(sfxName)) return;
            if (_sfxDict.TryGetValue(sfxName, out AudioClip clip) && clip is not null)
                sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (!IsBgmMuted)
                _lastBgmVolume = bgmVolume;
            ApplyBGMVolume();
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            OnBGMVolumeChanged?.Invoke(bgmVolume);
        }

        public void SetSfxVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (!IsSfxMuted)
                _lastSfxVolume = sfxVolume;
            ApplySfxVolume();
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            OnSfxVolumeChanged?.Invoke(sfxVolume);
        }



        public float GetBGMVolume() => bgmVolume;
        public float GetSfxVolume() => sfxVolume;

        public void ToggleBgmMute()
        {
            if (!IsBgmMuted)
            {
                _lastBgmVolume = bgmVolume;
                IsBgmMuted = true;
            }
            else
            {
                IsBgmMuted = false;
                bgmVolume = _lastBgmVolume;
            }
            ApplyBGMVolume();
            PlayerPrefs.SetInt("BgmMuted", IsBgmMuted ? 1 : 0);
        }

        public void ToggleSfxMute()
        {
            if (!IsSfxMuted)
            {
                _lastSfxVolume = sfxVolume;
                IsSfxMuted = true;
            }
            else
            {
                IsSfxMuted = false;
                sfxVolume = _lastSfxVolume;
            }
            ApplySfxVolume();
            PlayerPrefs.SetInt("SfxMuted", IsSfxMuted ? 1 : 0);
        }

        private void LoadVolumeSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            IsBgmMuted = PlayerPrefs.GetInt("BgmMuted", 0) == 1f;
            IsSfxMuted = PlayerPrefs.GetInt("SfxMuted", 0) == 1f;

            ApplyBGMVolume();
            ApplySfxVolume();
        }

        private void ApplyBGMVolume()
        {
            if (bgmSource != null)
                bgmSource.volume = IsBgmMuted ? 0f : bgmVolume;
        }

        private void ApplySfxVolume()
        {
            if (sfxSource != null)
                sfxSource.volume = IsSfxMuted ? 0f : sfxVolume;
        }
    }
}
