using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EditorExtension
{
    public static partial class EditorUtils
    {

        [MenuItem("EditorExtension/Tools/OpenPersistentDataPath", false, 200)]
        private static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("EditorExtension/Tools/ClearPersistentDataPath", false, 200)]
        private static void ClearPersistentDataPath()
        {
            if (EditorUtility.DisplayDialog("Clear Persistent Data Path", "是否清空持久化路径?\n操作无法撤回", "Clear", "Cancel"))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);

                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);
            }
        }

        [MenuItem("EditorExtension/Tools/Clear PlayerPrefs", false, 200)]
        private static void ClearPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs", "是否删除PlayerPrefs?\n操作无法撤回", "Clear", "Cancel"))
                PlayerPrefs.DeleteAll();
        }


        [MenuItem("EditorExtension/Tools/Cleanup Missing Scripts")]
        private static void CleanupMissingScript()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                CleanupMissingScriptRecursive(selectedObjects[i]);
            }

            static void CleanupMissingScriptRecursive(GameObject gameObject)
            {
                if(gameObject == null)
                {
                    return;
                }
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);

                foreach (Transform child in gameObject.transform)
                {
                    CleanupMissingScriptRecursive(child.gameObject);
                }
            }
        }


        [MenuItem("EditorExtension/Tools/ConvertToUtf8")]
        private static void ConvertToUtf8()
        {
            // 获取选中的文件路径
            var assets = Selection.GetFiltered(typeof(TextAsset), SelectionMode.Assets);
            for (int i = 0; i < assets.Length; i++)
            {
                var selectedFilePath = AssetDatabase.GetAssetPath(assets[i]);
                if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
                {
                    Debug.LogError("Please select a valid file.");
                    return;
                }

                // 注册编码
                // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // 读取 ANSI 格式的文件内容
                string ansiContent = File.ReadAllText(selectedFilePath, Encoding.GetEncoding("GB2312"));

                // 去掉末尾的换行符
                ansiContent = ansiContent.TrimEnd('\r', '\n');

                // 将 ANSI 格式的文件内容转换为 UTF-8 格式
                byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(ansiContent);

                // 添加 BOM 头部
                byte[] utf8WithBomBytes = AddUtf8Bom(utf8Bytes);
                File.WriteAllBytes(selectedFilePath, utf8WithBomBytes);
                Debug.Log("File converted to UTF-8 and saved as: " + selectedFilePath);
            }
            AssetDatabase.Refresh();
        }

        private static byte[] AddUtf8Bom(byte[] utf8Bytes)
        {
            // 添加 UTF-8 BOM（字节顺序标记）
            byte[] utf8WithBomBytes = new byte[utf8Bytes.Length + 3];
            utf8WithBomBytes[0] = 0xEF;
            utf8WithBomBytes[1] = 0xBB;
            utf8WithBomBytes[2] = 0xBF;
            utf8Bytes.CopyTo(utf8WithBomBytes, 3);
            return utf8WithBomBytes;
        }

        [MenuItem("EditorExtension/Tools/SetAddressableAsItsNameByFiles", false, 0)]
        private static void SetAddressableAsItsNameByFiles()
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                var entry = EditorUtils.CreateEntryToGroup(obj);
                if (entry != null)
                {
                    entry.SetAddress(obj.name);
                }
            }
            AssetDatabase.Refresh();
        }

        public static List<T> LoadAssets<T>(string dirName) where T : UnityEngine.Object
        {
            var path = $"{Application.dataPath}/{dirName}";
            var files = Directory.GetFiles(path)
                .Where(x => !x.EndsWith(".meta")).ToArray();
            List<T> result = new List<T>();
            for (int i = 0; i < files.Length; i++)
            {
                var objPath = files[i].Replace($"{Application.dataPath}", "Assets");
                objPath = objPath.Replace("\\", "/");
                var obj = AssetDatabase.LoadAssetAtPath<T>(objPath);
                if (obj != null)
                {
                    result.Add(obj);
                }
            }
            return result;
        }
        public static string[] FindDirectories(string root, Regex pattern)
        {
            Stack<string> stack = new Stack<string>();
            List<string> result = new List<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var directory = stack.Pop();
                if (pattern.IsMatch(directory))
                {
                    result.Add(directory);
                }
                var directories = Directory.GetDirectories(directory);
                for (int i = 0; i < directories.Length; i++)
                {
                    stack.Push(directories[i]);
                }
            }
            return result.ToArray();
        }
        public static string[] FindFiles(string root, Regex pattern)
        {
            var directories = FindDirectories(root, new Regex(".*"));
            List<string> result = new List<string>();
            for (int i = 0; i < directories.Length; i++)
            {
                var files = Directory.GetFiles(directories[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    files[j] = files[j].Trim();
                    if (pattern.IsMatch(files[j]))
                    {
                        result.Add(files[j]);
                    }
                }
            }
            return result.ToArray();
        }
        public static async Task<string[]> FindDirectoriesAsync(string root, Regex pattern)
        {
            ConcurrentStack<string> stack = new ConcurrentStack<string>();
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            stack.Push(root);
            while (!stack.IsEmpty)
            {
                // 提取出栈中的所有元素
                var directories = new List<string>();
                string directory;
                while (stack.TryPop(out directory))
                {
                    directories.Add(directory);
                }

                // 创建并行任务
                var tasks = directories.Select(directory => Task.Run(() =>
                {
                    if (pattern.IsMatch(directory))
                    {
                        result.Add(directory);
                    }
                    var subDirectories = Directory.GetDirectories(directory);
                    if (subDirectories.Length > 0)
                    {
                        stack.PushRange(subDirectories);
                    }
                })).ToArray();

                // 等待所有任务完成
                await Task.WhenAll(tasks);
            }
            return result.ToArray();
        }
        public static async Task<string[]> FindFilesAsync(string root, Regex pattern)
        {
            var directories = await FindDirectoriesAsync(root, new Regex(".*"));
            ConcurrentBag<string> result = new ConcurrentBag<string>();
            var tasks = directories.Select(directory => Task.Run(() =>
            {
                var files = Directory.GetFiles(directory);
                for (int j = 0; j < files.Length; j++)
                {
                    files[j] = files[j].Trim();
                    if (pattern.IsMatch(files[j]))
                    {
                        result.Add(files[j]);
                    }
                }
            })).ToArray();
            await Task.WhenAll(tasks);
            return result.ToArray();
        }
        public static void SetProperty<T>(object obj, string property, T value)
        {
            var type = obj.GetType();
            var target = type.GetProperty(property);
            target.SetValue(obj, value);
        }
        public static T GetProperty<T>(object obj, string property)
        {
            var type = obj.GetType();
            var target = type.GetProperty(property, System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return (T)target.GetValue(obj);
        }
        public static void SetField<T>(object obj, string field, T value)
        {
            var type = obj.GetType();
            var target = type.GetField(field, System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            target.SetValue(obj, value);
        }
        public static T GetField<T>(object obj, string field)
        {
            var type = obj.GetType();
            var target = type.GetField(field, System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return (T)target.GetValue(obj);
        }
        public static System.Reflection.EventInfo GetEvent(object obj, string field)
        {
            var type = obj.GetType();
            return type.GetEvent(field, System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        }
        public async static void DelayTask(Action action, int millisecond)
        {
            await Task.Delay(millisecond);
            action();
        }

        public static void CheckOrCreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void CheckOrCreateScriptDirectory()
        {
            CheckOrCreateDirectory($"{Application.dataPath}/Scripts");
        }

        public static string GetTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return $"{type.Namespace}.{type.Name}";
            }

            List<int> genericEndPoint = new List<int>();
            StringBuilder sb = new StringBuilder();
            //IBuild<IBuild<string>>
            Stack<Type> types = new Stack<Type>();
            types.Push(type);
            while (types.Count > 0)
            {
                var curType = types.Pop();
                if (!curType.IsGenericType)
                {
                    sb.Append($"{curType.Namespace}.{curType.Name}");
                }
                else
                {
                    sb.Append($"{curType.Namespace}.");
                    sb.Append(curType.Name, 0, curType.Name.Length - 2);
                    sb.Append("<");
                    genericEndPoint.Add(types.Count);
                    var arguments = curType.GetGenericArguments();
                    for (int i = arguments.Length - 1; i >= 0; i--)
                    {
                        types.Push(arguments[i]);
                    }
                }
                if (genericEndPoint.Count > 0 && types.Count == genericEndPoint[genericEndPoint.Count - 1])
                {
                    genericEndPoint.RemoveAt(genericEndPoint.Count - 1);
                    sb.Append(">");
                }
                else if (!curType.IsGenericType)
                {
                    sb.Append($",");
                }
            }
            for (int i = 0; i < genericEndPoint.Count; i++)
            {
                sb.Append(">");
            }
            genericEndPoint.Clear();
            return sb.ToString();
        }

        public static T LoadAddressableAsset<T>(string targetAddress) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(targetAddress))
            {
                return null;
            }

            UnityEngine.Object result = null;

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            string foundGUID = string.Empty;

            foreach (var group in settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.address == targetAddress)
                    {
                        foundGUID = entry.guid;
                        goto TryLoadAssets;
                    }
                }
            }

        TryLoadAssets:
            if (!string.IsNullOrEmpty(foundGUID))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(foundGUID);
                result = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            }

            return (T)result;
        }

        public static string GetAddressableAssetAddress(UnityEngine.Object gameObject)
        {
            string address = string.Empty;
            if (gameObject != null)
            {
                var gameObjectPath = AssetDatabase.GetAssetPath(gameObject);

                var settings = AddressableAssetSettingsDefaultObject.Settings;

                foreach (var group in settings.groups)
                {
                    foreach (var entry in group.entries)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);

                        if (assetPath.Equals(gameObjectPath))
                        {
                            address = entry.address;
                            goto SearchEnd;
                        }
                    }
                }
            }
        SearchEnd:
            return address;
        }

        public static AddressableAssetEntry GetAddressableAssetEntry(UnityEngine.Object _object)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var assetPath = AssetDatabase.GetAssetPath(_object);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            return settings.FindAssetEntry(assetGuid);
        }
        public static void RemoveAddressableAssetEntry(UnityEngine.Object _object)
        {
            var entry = GetAddressableAssetEntry(_object);
            if (entry != null)
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                settings.RemoveAssetEntry(entry.guid);
            }
        }
        public static AddressableAssetEntry CreateEntryToGroupByGroupName(UnityEngine.Object _object, string groupName = null)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = null;
            if (string.IsNullOrEmpty(groupName))
            {
                group = settings.DefaultGroup;
            }
            else
            {
                group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, false, null, settings.DefaultGroup.SchemaTypes.ToArray());
            }
            return CreateEntryToGroup(_object, group);
        }
        public static AddressableAssetEntry CreateEntryToGroup(UnityEngine.Object _object, AddressableAssetGroup defaultGroup = null)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (defaultGroup == null)
            {
                defaultGroup = settings.DefaultGroup;
            }
            var assetPath = AssetDatabase.GetAssetPath(_object);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            return settings.CreateOrMoveEntry(assetGuid, defaultGroup);
        }
        public static AddressableAssetGroup FindGroup(string groupName)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = null;
            if (!string.IsNullOrEmpty(groupName))
            {
                group = settings.FindGroup(groupName) ?? settings.CreateGroup(groupName, false, false, false, null, settings.DefaultGroup.SchemaTypes.ToArray());
            }
            return group;
        }
    }
}
