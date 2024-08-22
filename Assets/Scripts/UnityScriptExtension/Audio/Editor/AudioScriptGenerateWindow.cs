using EditorExtension;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Reflection;
using UnityEditor.PackageManager.UI;

namespace GameExtension.Editor
{
    public class AudioScriptGenerateWindow : EditorWindow
    {
        [MenuItem("GameExtension/Windows/AudioScriptGenerateWindow")]
        public static async void OpenWindow()
        {
            var window = EditorWindow.CreateInstance<AudioScriptGenerateWindow>();
            var options = await EditorExtension.EditorUtils.FindDirectoriesAsync(Application.dataPath, new Regex(".*(A|a)udio[s]?$"));
            window.objectSelector = new ObjectSelector();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0; i < options.Length; i++)
            {
                dict.Add(options[i], options[i]);
            }
            window.objectSelector.SetOptions(new List<string>(options), dict);
            window.objectSelector.onOptionChange += window.SetScriptName;
            window.Show();

        }

        string scriptName;
        ObjectSelector objectSelector;
        bool hasStaticInstance;
        int initSoundCount;

        private void OnGUI()
        {
            objectSelector.Draw();
            scriptName = EditorGUILayout.TextField("脚本名", scriptName);
            hasStaticInstance = EditorGUILayout.Toggle("拥有静态单例", hasStaticInstance);
            initSoundCount = EditorGUILayout.IntSlider("初始化音频数量", initSoundCount, 1, 10);
            if (GUILayout.Button("生成脚本"))
            {
                GenerateAudioManagerScript();
            }
            else if (GUILayout.Button("添加脚本实例到场景"))
            {
                AddAudioManagerToScene();
            }
        }

        void SetScriptName(string _scriptName)
        {
            _scriptName = _scriptName.Replace('\\', '/');
            var splits = _scriptName.Split('/');
            _scriptName = splits[splits.Length - 1];
            scriptName = Regex.Replace(_scriptName, "[\\s]", "_") + "Manager";
        }

        public static async void GenerateAudioEnum()
        {
            var audios = await EditorExtension.EditorUtils.FindFilesAsync(Application.dataPath, new Regex(".*(mp3|wav|ogg){1}$"));
            List<string> paths = new List<string>();
            for (int i = 0; i < audios.Length; i++)
            {
                var audio = audios[i].Replace('\\', '/');
                var splited = Regex.Split(audio, "/");
                audio = splited[splited.Length - 1];
                audio = audio.Split('.')[0];
                paths.Add(audio);
            }
            CodeGenerator.GenerateEnumScript("AudioEnum", paths.ToArray());
        }

        public async void GenerateAudioManagerScript()
        {
            var classBuilder = new ClassBuilder();

            classBuilder.SetClassName(scriptName);

            classBuilder.AddUsingNameSpace("System.Collections");
            classBuilder.AddUsingNameSpace("System.Collections.Generic");
            classBuilder.AddUsingNameSpace("UnityEngine");
            classBuilder.AddUsingNameSpace("GameExtension");

            classBuilder.SetAccessingModifier(AccessingModifier.Public);

            classBuilder.AddInterface(typeof(AudioManagerBase));

            FieldBuilder fieldBuilder;
            if (hasStaticInstance)
            {
                fieldBuilder = classBuilder.AddField(scriptName, "Instance", AccessingModifier.Public);
                fieldBuilder.SetStatic(true);
            }

            var methodBuilder = classBuilder.AddMethod($"Awake", AccessingModifier.Protected);

            CodeBuilder codeBuilder = methodBuilder.codeBuilder;
            if (hasStaticInstance)
            {
                codeBuilder = methodBuilder.codeBuilder.AddIF("Instance == null", "Instance = this;\nDontDestroyOnLoad(this);\n");
            }

            codeBuilder.AddLine("audioPool = new Pool<AudioSource>(() =>\n" +
                "\n{" +
                "var obj = new GameObject(\"Sound\");\n" +
                "obj.transform.SetParent(transform);\n" +
                "return obj.AddComponent<AudioSource>();\n" +
                "});\n");
            codeBuilder.AddLine("sounds = new List<AudioSource>(initSoundCount);\n" +
                "for (int i = 0; i < initSoundCount; i++)\n" +
                "\n{" +
                "sounds.Add(audioPool.Get());\n" +
                "\n}");
            codeBuilder.AddLine("theme = audioPool.Get();\n" +
                "theme.name = \"Theme\";\n" +
                "theme.loop = true;\n");

            var audios = await EditorExtension.EditorUtils.FindFilesAsync(objectSelector.GetOption(), new Regex(".*(mp3|wav|ogg){1}$"));
            for (int i = 0; i < audios.Length; i++)
            {
                var audio = audios[i].Replace('\\', '/');
                var splited = Regex.Split(audio, "/");
                audio = splited[splited.Length - 1];
                audio = audio.Split('.')[0];
                audio = audio.Replace(audio[0], char.ToUpper(audio[0]));
                audio = audio.Replace(' ', '_');

                fieldBuilder = classBuilder.AddField(typeof(AudioClip), audio, AccessingModifier.Private);
                fieldBuilder.AddAttributer(typeof(SerializeField));

                methodBuilder = classBuilder.AddMethod($"Play{audio}", AccessingModifier.Public);
                if (audio.StartsWith("Theme"))
                {
                    methodBuilder.codeBuilder.AddLine($"PlayTheme({audio})");
                }
                else
                {
                    methodBuilder.codeBuilder.AddLine($"PlaySound({audio});");
                }
            }
            CodeGenerator.GenerateClassScript(classBuilder);
        }

        public async void AddAudioManagerToScene()
        {
            var assemblyPath = Application.dataPath.Replace("Assets", "Library/ScriptAssemblies/Assembly-CSharp.dll");
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(scriptName);
            var obj = new GameObject(scriptName);
            var audioManager = obj.AddComponent(type);
            var audios = await EditorExtension.EditorUtils.FindFilesAsync(objectSelector.GetOption(), new Regex(".*(mp3|wav|ogg){1}$"));
            for (int i = 0; i < audios.Length; i++)
            {
                var audio = audios[i].Replace('\\', '/');
                audio = audio.Replace(Application.dataPath, "Assets");
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audio);
                var splited = Regex.Split(audio, "/");
                audio = splited[splited.Length - 1];
                audio = audio.Split('.')[0];
                audio = audio.Replace(audio[0], char.ToUpper(audio[0]));
                audio = audio.Replace(' ', '_');
                EditorExtension.EditorUtils.SetField(audioManager, audio, clip);
            }
            EditorExtension.EditorUtils.SetField(audioManager, "initSoundCount", initSoundCount);
        }
    }
}
