using EditorExtension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using Object = UnityEngine.Object;
using Unity.VisualScripting;
using GameExtension;

namespace EditorExtension
{
    public partial class AssetUtil
    {

        [MenuItem("Assets/CodeR/InitSpriteSettings")]
        public static async void InitSpriteSettings()
        {
            //获取选中的文件

            // 获取选中的文件夹
            var selectedFolders = Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets);
            foreach (var folder in selectedFolders)
            {
                // 获取文件夹路径
                string folderPath = AssetDatabase.GetAssetPath(folder);
                var texturePathes = await EditorUtils.FindFilesAsync(folderPath,
                    new System.Text.RegularExpressions.Regex("\\.png$|\\.jpg$|\\.jpeg$"));
                SetTextureWebGLSettings(texturePathes);
                SetTextureToSprite(texturePathes);
            }
        }

        [MenuItem("Assets/CodeR/InitSpriteSettingsByFiles")]
        public static async void InitSpriteSettingsByFiles()
        {
            //获取选中的文件

            // 获取选中的文件夹
            var selectedFolders = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
            foreach (var folder in selectedFolders)
            {
                // 获取文件夹路径
                string folderPath = AssetDatabase.GetAssetPath(folder);
                // var texturePathes = await EditorUtil.FindFilesAsync(folderPath,
                //     new System.Text.RegularExpressions.Regex("\\.png$|\\.jpg$|\\.jpeg$"));
                SetTextureWebGLSettings(new[] { folderPath });
                SetTextureToSprite(new[] { folderPath });
            }
        }

        static void SetTextureWebGLSettings(string[] texturePathes)
        {
            AssetDatabase.StartAssetEditing();
            string texturePath = null;
            try
            {
                for (int i = 0; i < texturePathes.Length; i++)
                {
                    texturePath = texturePathes[i].Replace(Application.dataPath, "Assets");
                    EditorUtility.DisplayProgressBar("初始化WebGL纹理设置", texturePath, (float)i / texturePathes.Length);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    if (textureImporter == null)
                    {
                        continue;
                    }
                    var webGLSettings = textureImporter.GetPlatformTextureSettings("WebGL");
                    if (webGLSettings == null || webGLSettings.overridden)
                    {
                        continue;
                    }
                    webGLSettings.overridden = true;
                    webGLSettings.format = TextureImporterFormat.ASTC_6x6;
                    webGLSettings.compressionQuality = 100;
                    textureImporter.SetPlatformTextureSettings(webGLSettings);
                    AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"texturePath:{texturePath}:{e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }
        }

        static void SetTextureToSprite(string[] texturePathes)
        {
            AssetDatabase.StartAssetEditing();
            string texturePath = null;
            try
            {
                for (int i = 0; i < texturePathes.Length; i++)
                {
                    texturePath = texturePathes[i].Replace(Application.dataPath, "Assets");
                    EditorUtility.DisplayProgressBar("设置Texture为Sprite", texturePath, (float)i / texturePathes.Length);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    if (textureImporter == null)
                    {
                        continue;
                    }
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.alphaIsTransparency = true;
                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                    var textureName = Path.GetFileNameWithoutExtension(texturePath);
                    if (textureName.StartsWith("Ground_"))
                    {
                        textureImporter.spritePixelsPerUnit = 120;
                    }
                    textureImporter.SaveAndReimport();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"texturePath:{texturePath}:{e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }
        }

        public static NativeAddressableConfig LoadNativeAddressableConfig()
        {
            var configPath = "Assets/NativeAddressableConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<NativeAddressableConfig>(configPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<NativeAddressableConfig>();
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
            }
            return config;
        }
    }
}