using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace GameExtension
{
    public class AudioManagerBase : MonoBehaviour
    {
        public event Action<float> OnThemeVolumeChange;
        public event Action<float> OnSoundVolumeChange;
        protected AudioSource theme;
        protected Pool<AudioSource> audioPool;
        protected List<AudioSource> sounds;
        [SerializeField]
        protected int initSoundCount;
        int lastPlayIndex = 0;
        protected HashSet<AudioSource> handledSources;
        public virtual float ThemeVolume
        {
            get
            {
                return themeVolume;
            }
            set
            {
                themeVolume = value;
                theme.volume = themeVolume;
                OnThemeVolumeChange?.Invoke(themeVolume);
            }
        }
        protected float themeVolume;
        public virtual float SoundVolume
        {
            get
            {
                return soundVolume;
            }
            set
            {
                soundVolume = value;
                for (int i = 0; i < sounds.Count; i++)
                {
                    sounds[i].volume = soundVolume;
                }
                OnSoundVolumeChange?.Invoke(soundVolume);
            }
        }
        protected float soundVolume;
        public bool ThemeMute
        {
            get
            {
                return themeMute;
            }
            set
            {
                themeMute = value;
                theme.mute = themeMute;
            }
        }
        bool themeMute;
        public bool SoundMute
        {
            get
            {
                return soundMute;
            }
            set
            {
                soundMute = value;
                for (int i = 0; i < sounds.Count; i++)
                {
                    sounds[i].mute = soundMute;
                }
            }
        }
        protected bool soundMute;

        HashSet<AudioHandle> autoReleaseHandles;
        Coroutine autoReleaseHandleCo;

        protected virtual void Awake()
        {
            sounds = new List<AudioSource>();
            handledSources = new HashSet<AudioSource>();
            audioPool = new Pool<AudioSource>(() =>
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.volume = 1;
                return audioSource;
            }, -1);
            theme = audioPool.Get();
        }

        protected void InvokeOnThemeVolumeChange(float volume)
        {
            OnThemeVolumeChange?.Invoke(volume);
        }

        protected void InvokeOnSoundVolumeChange(float volume)
        {
            OnSoundVolumeChange?.Invoke(volume);
        }

        protected AudioSource InternalPlaySound(AudioClip audioClip)
        {
            if (SoundMute)
            {
                return null;
            }

            var sound = GetAvailableSource();

            sound.Stop();
            sound.clip = audioClip;
            sound.Play();
            return sound;
        }
        protected AudioHandle PlaySound(AudioClip audioClip)
        {
            var audioSource = InternalPlaySound(audioClip);
            if (audioSource == null)
            {
                return null;
            }

            handledSources.Add(audioSource);
            return CreateAudioHandle(audioSource);
        }
        protected AudioSource InternalPlayTheme(AudioClip audioClip)
        {
            if (ThemeMute)
            {
                return null;
            }
            if (handledSources.Contains(theme))
            {
                return null;
            }
            if (theme.clip == audioClip && theme.isPlaying)
            {
                return theme;
            }
            theme.clip = audioClip;
            theme.Play();
            return theme;
        }
        protected AudioHandle PlayTheme(AudioClip audioClip)
        {
            var audioSource = InternalPlayTheme(audioClip);
            if (audioSource == null)
            {
                return null;
            }
            handledSources.Add(audioSource);
            return CreateAudioHandle(audioSource);
        }

        AudioHandle CreateAudioHandle(AudioSource audioSource)
        {
            var audioHandle = new AudioHandle(audioSource);
            audioHandle.OnSetAutoRelease += CheckAudioHandleAutoReleaseState;
            return audioHandle;
        }

        void CheckAudioHandleAutoReleaseState(AudioHandle audioHandle)
        {
            if (audioHandle.IsAutoRelease)
            {
                if (autoReleaseHandles == null)
                {
                    autoReleaseHandles = new HashSet<AudioHandle>();
                }
                autoReleaseHandles.Add(audioHandle);
                if (autoReleaseHandleCo == null)
                {
                    autoReleaseHandleCo = StartCoroutine(AutoReleaseAudioHandles());
                }
            }
            else if (autoReleaseHandles != null)
            {
                autoReleaseHandles.Remove(audioHandle);
            }
        }

        protected AudioSource GetAvailableSource()
        {
            int playIndex = -1;
            AudioSource sound;
            for (int i = lastPlayIndex; i < sounds.Count + lastPlayIndex; i++)
            {
                int index = i % sounds.Count;
                sound = sounds[index];
                if (!sounds[index].isPlaying && !handledSources.Contains(sound))
                {
                    playIndex = index;
                    break;
                }
            }


            if (playIndex >= 0)
            {
                sound = sounds[playIndex];
                lastPlayIndex = playIndex;
            }
            else
            {
                sound = audioPool.Get();
                sounds.Add(sound);
            }

            sound.mute = SoundMute;
            sound.volume = SoundVolume;

            return sound;
        }
        public virtual void StopTheme()
        {
            theme.Stop();
        }
        public void PauseTheme()
        {
            theme.Pause();
        }
        public void GetThemeSpectrumData(float[] sample, int channel, FFTWindow fftWindow)
        {
            theme.GetSpectrumData(sample, channel, fftWindow);
        }
        public void ReleaseAudioHandle(AudioHandle audioHandle)
        {
            if (audioHandle == null)
            {
                return;
            }
            var audioSource = audioHandle.AudioSource;
            if (handledSources.Remove(audioSource))
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            if(autoReleaseHandles != null)
            {
                autoReleaseHandles.Remove(audioHandle);
            }

            audioHandle.OnSetAutoRelease -= CheckAudioHandleAutoReleaseState;
        }
        IEnumerator AutoReleaseAudioHandles()
        {
            Queue<AudioHandle> waitToReleaseHandles = new Queue<AudioHandle>();

            while(autoReleaseHandles.Count > 0)
            {
                foreach (var handle in autoReleaseHandles)
                {
                    if(!handle.AudioSource.isPlaying)
                    {
                        waitToReleaseHandles.Enqueue(handle);
                    }
                }

                while(waitToReleaseHandles.Count > 0)
                {
                    ReleaseAudioHandle(waitToReleaseHandles.Dequeue());
                }

                yield return null;
            }

            autoReleaseHandleCo = null;
        }
    }

    public class AudioHandle
    {
        public AudioSource AudioSource { get; private set; }
        public bool IsAutoRelease { get; private set; }
        public event Action<AudioHandle> OnSetAutoRelease;
        public AudioHandle(AudioSource audioSource)
        {
            AudioSource = audioSource;
        }

        public void SetAutoRelease(bool autoRelease)
        {
            IsAutoRelease = autoRelease;
            OnSetAutoRelease?.Invoke(this);
        }
    }
}
