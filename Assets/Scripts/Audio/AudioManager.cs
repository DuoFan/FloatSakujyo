using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameExtension;
using System;
using FloatSakujyo.SaveData;

namespace FloatSakujyo.Audio
{
    public class AudioManager : GameExtension.AudioManagerBase, IGameInitializer
    {
        public static AudioManager Instance;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Zqddn_Zhb_right;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Win;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Pz;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Put;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip PoP;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Fly;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Fail;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Done;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Cut;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip CliCk;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Citywin;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Boom;
        [UnityEngine.SerializeField]
        private UnityEngine.AudioClip Bgm;

        public IEnumerator InitializeGame()
        {
            SoundVolume = 1;
            ThemeVolume = 1;

            PlayBgm();

            bool isMute = GameDataManager.Instance.GetPlayerPreference().IsMute;
            ThemeMute = isMute;
            SoundMute = isMute;

            yield return null;
        }

        protected override void Awake()
        {

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);

                handledSources = new HashSet<AudioSource>();

                audioPool = new Pool<AudioSource>(() =>
                {
                    var obj = new GameObject("Sound");
                    obj.transform.SetParent(transform);
                    return obj.AddComponent<AudioSource>();
                });
                ;
                sounds = new List<AudioSource>(initSoundCount);
                for (int i = 0; i < initSoundCount; i++)
                {
                    sounds.Add(audioPool.Get());
                };
                theme = audioPool.Get();
                theme.name = "Theme";
                theme.loop = true;
                ;
            }
        }
        public void PlayZqddn_Zhb_right()
        {

            PlaySound(Zqddn_Zhb_right)?.SetAutoRelease(true); 
        }
        public void PlayWin()
        {
            PlaySound(Win)?.SetAutoRelease(true);
        }
        public void PlayPz()
        {

            PlaySound(Pz)?.SetAutoRelease(true);
        }
        public void PlayPut()
        {

            PlaySound(Put)?.SetAutoRelease(true);
        }
        public void PlayPoP()
        {
            PlaySound(PoP)?.SetAutoRelease(true);
        }
        public void PlayFly()
        {

            PlaySound(Fly)?.SetAutoRelease(true); 
        }
        public void PlayFail()
        {
            PlaySound(Fail)?.SetAutoRelease(true);
        }
        public void PlayDone()
        {
            PlaySound(Done)?.SetAutoRelease(true);
        }
        public void PlayCut()
        {

            PlaySound(Cut)?.SetAutoRelease(true); 
        }
        public void PlayCliCk()
        {

            PlaySound(CliCk)?.SetAutoRelease(true);
        }
        public void PlayCitywin()
        {

            PlaySound(Citywin)?.SetAutoRelease(true);
        }
        public void PlayBoom()
        {

            PlaySound(Boom)?.SetAutoRelease(true);
        }
        public void PlayBgm()
        {
            PlayTheme(Bgm);
        }
    }
}