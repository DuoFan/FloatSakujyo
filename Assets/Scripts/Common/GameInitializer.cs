using GameExtension;
using Newtonsoft.Json;
using SDKExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FloatSakujyo
{
    public class GameInitializer : GameInitializerBase
    {
        public override IEnumerator InitializeGame()
        {
            CoroutineManager.Instance.StartCoroutine(InitServices());

            yield return InstantiateGameObjects();

            yield return LoadAllSheetData(1f / loadSheetDataTasks.Length);

            yield return LoadAllSheetData(1f / loadSheetDataTasks.Length);

            yield return ExecuteAllInitializer();

            SceneManager.LoadScene("Game");

            yield break;
        }

        IEnumerator InitServices()
        {
#if WEI_XIN
            IOAdapter.Init(new WeiXinIOAdapter());
#else
            SRDebug.Init();
            IOAdapter.Init(new SystemIOAdapter());
#endif

#if WEI_XIN
            RemoteGameConfig.LoadConfig("https://res.cjs001.com/Screw_Train/Switch/WeiXin/WeiXin_V0.json", null, null);
            float waitTime = 5;
            while (waitTime > 0)
            {
                yield return null;
                waitTime -= Time.deltaTime;
                if (RemoteGameConfig.Instance != null)
                {
                    break;
                }
            }

            if (RemoteGameConfig.Instance != null && RemoteGameConfig.Instance.isOpenDebug)
            {
                SDKListener.Init(new StubAD(), null);
                SRDebug.Init();
            }
            else
            {
                SDKListener.Init(new WeiXinAD(), null);
            }
#else
            SDKListener.Init(new StubAD(), null);
#endif
            /*SDKListener.Instance.RewardEvent.onShow += (s, e) =>
            {
                AudioManager.Instance.SoundMute = true;
                AudioManager.Instance.ThemeMute = true;
            };

            SDKListener.Instance.RewardEvent.onClose += (s, e) =>
            {
                var isMute = GameDataManager.Instance.GetPlayerPreference().IsMute;
                AudioManager.Instance.SoundMute = isMute;
                AudioManager.Instance.ThemeMute = isMute;
            };*/

            yield break;
        }
    }
}
