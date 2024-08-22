using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor.PackageManager.UI;

namespace EditorExtension
{
    public class CodeInjectWindow : EditorWindow
    {
        public static async void CreateEditor()
        {
            var window = EditorWindow.CreateInstance<CodeInjectWindow>();
            var options = await FindScriptDirectory(Application.dataPath);
            window.objectSelector = new ObjectSelector();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            window.objectSelector.SetOptions(new List<string>(options), dict);
            window.Show();
        }

        ObjectSelector objectSelector;
        int classCount;
        int maxMethodCount;
        int density;
        string shortName = EditorUtils.GetTypeName(typeof(short));
        string intName = EditorUtils.GetTypeName(typeof(int));
        string longName = EditorUtils.GetTypeName(typeof(long));
        string floatName = EditorUtils.GetTypeName(typeof(float));
        string doubleName = EditorUtils.GetTypeName(typeof(double));
        string charName = EditorUtils.GetTypeName(typeof(char));
        string stringName = EditorUtils.GetTypeName(typeof(string));

        private void OnGUI()
        {
            objectSelector.Draw();

            classCount = EditorGUILayout.IntSlider("生成类数量", classCount, 1, 100);

            maxMethodCount = EditorGUILayout.IntSlider("最大方法数", maxMethodCount, 5, 20);

            density = EditorGUILayout.IntSlider("注入密度", density, 1, 10);

            if (GUILayout.Button("生成代码"))
            {
                var classBuilders = GenerateCode();
                for (int i = 0; i < classBuilders.Count; i++)
                {
                    CodeGenerator.GenerateClassScript(classBuilders[i], $"{Application.dataPath}/GeneratedCode");
                }
                var randomStringBuilder = new ClassBuilder();
                randomStringBuilder.SetClassName("RandomString");
                randomStringBuilder.AddUsingNameSpace("System");
                randomStringBuilder.AddUsingNameSpace("System.Security.Cryptography");
                randomStringBuilder.AddUsingNameSpace("System.Text");
                randomStringBuilder.SetAccessingModifier(AccessingModifier.Public);
                var getStringBuilder = randomStringBuilder.AddMethod("GetString", AccessingModifier.Public);
                getStringBuilder.isStatic = true;
                getStringBuilder.SetReturnType(typeof(string));
                getStringBuilder.codeBuilder.AddLine("int seed = UnityEngine.Random.Range(0, 1000000)");
                getStringBuilder.codeBuilder.AddLine("var md5 = new MD5CryptoServiceProvider()");
                getStringBuilder.codeBuilder.AddLine("string name = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(seed.ToString())), 4, 8)");
                getStringBuilder.codeBuilder.AddLine("name = name.Replace(\" - \", \"\")");
                getStringBuilder.codeBuilder.AddLine("name = name.Insert(0, \"MM\")");
                getStringBuilder.codeBuilder.AddLine("return name");
                CodeGenerator.GenerateClassScript(randomStringBuilder, $"{Application.dataPath}/GeneratedCode");
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("注入"))
            {
                try
                {
                    BackupScripts();
                }
                catch (Exception e)
                {
                    Debug.LogError($"备份失败:{e.Message}");
                    return;
                }
                var classBuilders = GenerateCode();
                for (int i = 0; i < classBuilders.Count; i++)
                {
                    CodeGenerator.GenerateClassScript(classBuilders[i], $"{Application.dataPath}/GeneratedCode");
                }
                var randomStringBuilder = new ClassBuilder();
                randomStringBuilder.SetClassName("RandomString");
                randomStringBuilder.AddUsingNameSpace("System");
                randomStringBuilder.AddUsingNameSpace("System.Security.Cryptography");
                randomStringBuilder.AddUsingNameSpace("System.Text");
                randomStringBuilder.SetAccessingModifier(AccessingModifier.Public);
                var getStringBuilder = randomStringBuilder.AddMethod("GetString", AccessingModifier.Public);
                getStringBuilder.isStatic = true;
                getStringBuilder.SetReturnType(typeof(string));
                getStringBuilder.codeBuilder.AddLine("int seed = UnityEngine.Random.Range(0, 1000000)");
                getStringBuilder.codeBuilder.AddLine("var md5 = new MD5CryptoServiceProvider()");
                getStringBuilder.codeBuilder.AddLine("string name = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(seed.ToString())), 4, 8)");
                getStringBuilder.codeBuilder.AddLine("name = name.Replace(\" - \", \"\")");
                getStringBuilder.codeBuilder.AddLine("name = name.Insert(0, \"MM\")");
                getStringBuilder.codeBuilder.AddLine("return name");
                CodeGenerator.GenerateClassScript(randomStringBuilder, $"{Application.dataPath}/GeneratedCode");
                InjectCode(classBuilders);
                AssetDatabase.Refresh();
            }
        }
        static async Task<string[]> FindScriptDirectory(string root)
        {
            var directories = await EditorUtils.FindDirectoriesAsync(root, new Regex(".*[^CodeInjectBackup].*[S|s]cript[s]?"));
            var result = new List<string>(directories);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (result[i].Contains("UnityScriptExtension")){
                    result.RemoveAt(i);
                }
            }
            return result.ToArray();
        }
            
        List<ClassBuilder> GenerateCode()
        {
            HashSet<ClassBuilder> classBuilders = new HashSet<ClassBuilder>();

            List<string> types = new List<string>();

            types.Add(shortName);
            types.Add(intName);
            types.Add(longName);
            types.Add(floatName);
            types.Add(doubleName);
            types.Add(charName);
            types.Add(stringName);

            int GetRandomCodeType()
            {
                return UnityEngine.Random.Range((int)CodeType.Declare, (int)CodeType.InvokeMethod + 1);
            }

            CodeBuilder GenerateDeclareCode(Dictionary<string, List<ParameterBuilder>> _parameterDic)
            {
                var codeBuilder = new LineCodeBuilder();
                ParameterBuilder parameter = GetNewParameter(types, _parameterDic);
                codeBuilder.SetLine($"{parameter.parameterType} {parameter.parameterName} = {GetValueByType(parameter.parameterType)}");
                return codeBuilder;
            }

            CodeBuilder GenerateSetValueCode(Dictionary<string, List<ParameterBuilder>> _parameterDic)
            {
                var codeBuilder = new LineCodeBuilder();
                ParameterBuilder p1 = GetAnyParameter(_parameterDic);
                bool isNewParameter = p1 == null;
                if (isNewParameter)
                {
                    p1 = GetNewParameter(types, _parameterDic);
                }
                ParameterBuilder p2 = GetAnyParameterByType(_parameterDic, p1.parameterType, p1.parameterName);
                if(p2 == null)
                {
                    if (isNewParameter)
                    {
                        codeBuilder.SetLine($"{p1.parameterType} {p1.parameterName} = {GetValueByType(p1.parameterType)}");
                    }
                    else
                    {
                        codeBuilder.SetLine($"{p1.parameterName} = {GetValueByType(p1.parameterType)}");
                    }
                }
                else
                {
                    if (isNewParameter)
                    {
                        codeBuilder.SetLine($"{p1.parameterType} {p1.parameterName} = {p2.parameterName}");
                    }
                    else
                    {
                        codeBuilder.SetLine($"{p1.parameterName} = {p2.parameterName}");
                    }
                }
                return codeBuilder;
            }
            CodeBuilder InGenerateInvokeMethodCode(HashSet<ClassBuilder> _classDic, Dictionary<string, List<ParameterBuilder>> _parameterDic)
            {
                var parameter = GetAnyParameter(_parameterDic);
                List<string> excludeParameter = new List<string>();
                while (parameter != null && IsBaseType(parameter.parameterType))
                {
                    excludeParameter.Add(parameter.parameterName);
                    parameter = GetAnyParameter(_parameterDic, excludeParameter.ToArray());
                }
                if (parameter == null)
                {
                    return GenerateDeclareCode(_parameterDic);
                }
                else
                {
                    return GenerateInvokeMethodCode(parameter, _classDic, _parameterDic);
                }
            }

            CodeBuilder GenerateCodeFragment(HashSet<ClassBuilder> classDic, Dictionary<string, List<FieldBuilder>> _fieldDic,
                Dictionary<string, List<ParameterBuilder>> _parameterDic)
            {
                var codeBuilder = new CodeBuilder();
                int lineCount = UnityEngine.Random.Range(3,5);
                while (lineCount > 0)
                {
                    var codeType = GetRandomCodeType();
                    CodeBuilder childCode = null;
                    if (codeType == (int)CodeType.Declare)
                    {
                        childCode = GenerateDeclareCode(_parameterDic);
                    }
                    else if (codeType == (int)CodeType.SetValue)
                    {
                        childCode = GenerateSetValueCode(_parameterDic);
                    }
                    else if (codeType <= (int)CodeType.InvokeMethod)
                    {
                        childCode = InGenerateInvokeMethodCode(classBuilders, _parameterDic);
                    }
                    if (childCode != null)
                    {
                        codeBuilder.AddChildren(childCode);
                    }
                    lineCount--;
                }
                return codeBuilder;
            }

            Dictionary<string, List<FieldBuilder>> fieldDic = new Dictionary<string, List<FieldBuilder>>();
            List<MethodBuilder> methodBuilders = new List<MethodBuilder>();
            Dictionary<string, List<ParameterBuilder>> parameterDic = new Dictionary<string, List<ParameterBuilder>>();

            for (int i = 0; i < classCount; i++)
            {
                var classBuilder = new ClassBuilder();
                classBuilder.SetAccessingModifier(AccessingModifier.Public);
                string className = null;
                while (className == null)
                {
                    className = GetRandomName();
                    foreach (var _classBuilder in classBuilders)
                    {
                        if (_classBuilder.ClassName == className)
                        {
                            className = null;
                            break;
                        }
                    }
                }
                classBuilder.SetClassName(className);
                int methodCount = UnityEngine.Random.Range(maxMethodCount / 2, maxMethodCount);
                int fieldCount = UnityEngine.Random.Range(methodCount / 2, methodCount);
                for (int j = 0; j < fieldCount; j++)
                {
                    var fieldType = GetRandomType(types);
                    string fieldName = null;
                    if (!fieldDic.ContainsKey(fieldType))
                    {
                        fieldDic.Add(fieldType, new List<FieldBuilder>());
                    }
                    while (fieldName == null)
                    {
                        fieldName = GetRandomName();
                        for (int k = 0; k < fieldDic[fieldType].Count; k++)
                        {
                            if (fieldDic[fieldType][k].filedName == fieldName)
                            {
                                fieldName = null;
                                break;
                            }
                        }
                    }
                    var filedBuilder = classBuilder.AddField(fieldType, fieldName, AccessingModifier.Public);
                    fieldDic[fieldType].Add(filedBuilder);
                }

                for (int j = 0; j < methodCount; j++)
                {
                    string methodName = null;
                    while (methodName == null)
                    {
                        methodName = GetRandomName();
                        for (int k = 0; k < methodBuilders.Count; k++)
                        {
                            if (methodBuilders[k].methodName == methodName)
                            {
                                methodName = null;
                                break;
                            }
                        }
                    }
                    var methodBuilder = classBuilder.AddMethod(methodName, AccessingModifier.Public);
                    methodBuilders.Add(methodBuilder);
                    bool hasReturnType = UnityEngine.Random.Range(0, 2) == 1;
                    if (hasReturnType && false)
                    {
                        methodBuilder.returnType = GetRandomType(types);
                    }

                    int parameterPossible = UnityEngine.Random.Range(0, 100);
                    bool hasParameter = UnityEngine.Random.Range(0, 100) < parameterPossible;
                    while (hasParameter)
                    {
                        string parameterName = GetCheckedParameterName(parameterDic);
                        var parameterBuilder = methodBuilder.AddParameter(GetRandomType(types), parameterName, ParameterModifier.None);
                        if (!parameterDic.ContainsKey(parameterBuilder.parameterType))
                        {
                            parameterDic.Add(parameterBuilder.parameterType, new List<ParameterBuilder>());
                        }
                        parameterDic[parameterBuilder.parameterType].Add(parameterBuilder);
                        parameterPossible -= parameterPossible / 10;
                        hasParameter = UnityEngine.Random.Range(0, 100) < parameterPossible;
                    }
                    int lineCount = UnityEngine.Random.Range(5,10);
                    var codeBuilder = methodBuilder.codeBuilder;
                    while (lineCount > 0)
                    {
                        var codeType = GetRandomCodeType();
                        if (codeType == (int)CodeType.Declare)
                        {
                            var lineCode = GenerateDeclareCode(parameterDic);
                            codeBuilder.AddChildren(lineCode);
                        }
                        else if (codeType == (int)CodeType.SetValue)
                        {
                            var lineCode = GenerateSetValueCode(parameterDic);
                            codeBuilder.AddChildren(lineCode);
                        }
                        else if (codeType == (int)CodeType.IF)
                        {
                            int ifType = UnityEngine.Random.Range(0, 3);
                            //判断两个对象是否相等
                            if (ifType == 0)
                            {
                                var p1 = GetAnyParameter(parameterDic);
                                if (p1 == null)
                                {
                                    p1 = GetNewParameter(types, parameterDic);
                                    codeBuilder.AddLine($"{p1.parameterType} {p1.parameterName} = {GetValueByType(p1.parameterType)}");
                                }
                                var p2 = GetAnyParameterByType(parameterDic, p1.parameterType, p1.parameterName);
                                if (p2 == null)
                                {
                                    p2 = GetNewParameter(types, parameterDic, p1.parameterType);
                                    codeBuilder.AddLine($"{p2.parameterType} {p2.parameterName} = {GetValueByType(p2.parameterType)}");
                                }
                                codeBuilder = codeBuilder.AddIF($"{p1.parameterName}.Equals({p2.parameterName})", "");
                                codeBuilder.AddChildren(GenerateCodeFragment(classBuilders, fieldDic, parameterDic));
                            }
                            //判断一个对象的某个字段是否不为空
                            else if (ifType == 1)
                            {
                                var p1 = GetAnyParameter(parameterDic);
                                if (p1 == null)
                                {
                                    p1 = GetNewParameter(types, parameterDic);
                                    codeBuilder.AddLine($"{p1.parameterType} {p1.parameterName} = {GetValueByType(p1.parameterType)}");
                                }
                                var p1Class = FindClassByName(p1.parameterType, classBuilders);
                                if (p1Class != null)
                                {
                                    var field = p1Class.fieldBuilders[UnityEngine.Random.Range(0, p1Class.fieldBuilders.Count - 1)];

                                    codeBuilder = codeBuilder.AddIF($"{p1.parameterName}.{field.filedName} != (default({field.fieldType}))", "");
                                    codeBuilder.AddChildren(GenerateCodeFragment(classBuilders, fieldDic, parameterDic));
                                }
                            }
                            else if (ifType == 2)
                            {

                            }
                        }
                        else if (codeType <= (int)CodeType.InvokeMethod)
                        {
                            var lineCode = InGenerateInvokeMethodCode(classBuilders, parameterDic);
                            codeBuilder.AddChildren(lineCode);
                        }
                        lineCount--;
                    }
                    parameterDic.Clear();
                }
                fieldDic.Clear();
                methodBuilders.Clear();
                classBuilders.Add(classBuilder);
                types.Add(classBuilder.ClassName);
            }
            return classBuilders.ToList();
        }
        async void BackupScripts()
        {
            if (!Directory.Exists($"{Application.dataPath}/CodeInjectBackup"))
            {
                Directory.CreateDirectory($"{Application.dataPath}/CodeInjectBackup");
            }

            var backupDirectory = objectSelector.GetOption();
            backupDirectory = backupDirectory.Replace(Application.dataPath, "Assets");
            backupDirectory += $"-{DateTime.Now.ToString().Replace(":", "_")}";
            backupDirectory = backupDirectory.Replace("\\", "-");
            backupDirectory = backupDirectory.Replace("/", "-");
            backupDirectory = $"{Application.dataPath}/CodeInjectBackup/{backupDirectory}";
            Directory.CreateDirectory(backupDirectory);

            var scriptDirectory = objectSelector.GetOption();
            var directories = await FindScriptDirectory(scriptDirectory);
            for (int i = 0; i < directories.Length; i++)
            {
                var backupChildDirectory = directories[i].Replace(scriptDirectory, backupDirectory);
                try
                {
                    Directory.CreateDirectory(backupChildDirectory);
                }
                catch (Exception)
                {
                    throw new Exception($"创建备份文件夹:{backupChildDirectory}失败");
                }

                var scripts = Directory.GetFiles(directories[i]).Where(x => x.EndsWith(".cs")).ToArray();
                for (int j = 0; j < scripts.Length; j++)
                {
                    var backupFile = scripts[j].Replace(directories[i], backupChildDirectory);
                    backupFile = backupFile.Remove(backupFile.Length - 3, 3);
                    try
                    {
                        File.Copy(scripts[j], backupFile);
                    }
                    catch (Exception)
                    {
                        throw new Exception($"创建备份代码:{backupFile}失败"); ;
                    }
                }
            }

            AssetDatabase.Refresh();
        }
        async void InjectCode(List<ClassBuilder> classBuilders)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var scriptDirectory = objectSelector.GetOption();
            var directories = await FindScriptDirectory(scriptDirectory);
            Dictionary<string, List<ParameterBuilder>> parameterDic = new Dictionary<string, List<ParameterBuilder>>();
            List<string> types = new List<string>();
            for (int i = 0; i < classBuilders.Count; i++)
            {
                types.Add(classBuilders[i].ClassName);
            }
            for (int i = 0; i < directories.Length; i++)
            {
                var scripts = Directory.GetFiles(directories[i]).Where(x => x.EndsWith(".cs")).ToArray();
                for (int j = 0; j < scripts.Length; j++)
                {
                    var script = File.ReadAllText(scripts[j]);
                    var injectedScript = script;
                    var methodsMatch = Regex.Match(script, "([ ]*(public|private|protected|internal)[ ]+)[^;{]+[(][^)]*[)][^{;]*{");
                    while (methodsMatch.Success)
                    {
                        int objCount = 0;
                        if (!Regex.Match(methodsMatch.Value, "[ ]*//.*").Success && !Regex.Match(methodsMatch.Value, ".*Update([(]{1}.*[)]{1}).*").Success)
                        {
                            int count = 0;
                            int index = methodsMatch.Index;
                            while (count == 0)
                            {
                                if (script[index] == '{')
                                {
                                    count++;
                                }
                                index++;
                            }

                            while (count > 0)
                            {
                                if (script[index] == '{')
                                {
                                    count++;
                                }
                                else if (script[index] == '}')
                                {
                                    count--;
                                }
                                index++;
                            }
                            var codeFragment = script.Substring(methodsMatch.Index, index - methodsMatch.Index);
                            var codeLines = codeFragment.Split('\n', '\r');
                            stringBuilder.Clear();
                            int k = 0;
                            for (; k < codeLines.Length; k++)
                            {
                                //Original Code...
                                //Inject Code...
                                if (!codeLines[k].Contains("return"))
                                {
                                    stringBuilder.Append($"{codeLines[k]}\n");
                                }

                                if (k % density == 0 && (codeLines[k].EndsWith(";\r") || codeLines[k].EndsWith(";\n") || codeLines[k].EndsWith(";")))
                                {
                                    //Skip Case if() || while() || switch() || do()
                                    /* if (!Regex.Match(codeLines[k], ".*[if|while|switch|do][ ]*[(].*[)]").Success)
                                     {

                                     }*/
                                    bool newObj = UnityEngine.Random.Range(0, objCount) < 10;
                                    ParameterBuilder obj = GetAnyParameter(parameterDic);
                                    LineCodeBuilder codeBuilder = new LineCodeBuilder();
                                    if (newObj || obj == null)
                                    {
                                        obj = GetNewParameter(types, parameterDic);
                                        codeBuilder.SetLine($"{obj.parameterType} {obj.parameterName} = {GetValueByType(obj.parameterType)}");
                                    }
                                    codeBuilder.AddChildren(GenerateInvokeMethodCode(obj, classBuilders, parameterDic));

                                    stringBuilder.Append($"{codeBuilder.Build()}\n");
                                }

                                //Inject Code...
                                //Return Code...
                                if (codeLines[k].Contains("return"))
                                {
                                    stringBuilder.Append($"{codeLines[k]}\n");
                                }
                            }
                            while (k < codeLines.Length)
                            {
                                stringBuilder.Append($"{codeLines[k]}\n");
                                k++;
                            }
                            var injectedFragment = stringBuilder.ToString();
                            injectedScript = injectedScript.Replace(codeFragment, injectedFragment);
                        }
                        methodsMatch = methodsMatch.NextMatch();
                    }
                    script = injectedScript;
                    File.WriteAllText(scripts[j], script);
                }
            }
            AssetDatabase.Refresh();
        }

        enum CodeType
        {
            Declare, IF, InvokeMethod = 10, SetValue = 100
        }


        string GetCheckedParameterName(Dictionary<string, List<ParameterBuilder>> parameters)
        {
        Loop:
            string name = GetRandomName();
            foreach (var item in parameters)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (item.Value[i].parameterName == name)
                    {
                        name = null;
                        goto Loop;
                    }
                }
            }
            return name;
        }

        string GetRandomName()
        {
            int seed = UnityEngine.Random.Range(0, 1000000);
            var md5 = new MD5CryptoServiceProvider();
            string name = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(seed.ToString())), 4, 8);
            name = name.Replace("-", "");
            name = name.Insert(0, "MM");
            return name;
        }
        ClassBuilder FindClassByName(string className, IEnumerable<ClassBuilder> classBuilders)
        {
            ClassBuilder result = null;
            foreach (var item in classBuilders)
            {
                if (item.ClassName == className)
                {
                    result = item;
                    return result;
                }
            }
            return null;
        }

        ParameterBuilder GetAnyParameter(Dictionary<string, List<ParameterBuilder>> _parameters, params string[] excludeName)
        {
            if (excludeName == null)
            {
                excludeName = new string[0];
            }
            return GetAnyParameterByType(_parameters, "Any", excludeName);
        }

        ParameterBuilder GetAnyParameterByType(Dictionary<string, List<ParameterBuilder>> _parameters, string type, params string[] excludeName)
        {
            ParameterBuilder result = null;
            if (type == "Any")
            {
                int index = UnityEngine.Random.Range(0, _parameters.Count());
                foreach (var item in _parameters)
                {
                    if (IsBaseType(item.Key))
                    {
                        continue;
                    }
                    if (index <= 0)
                    {
                        var parameterList = item.Value;
                        List<int> selectableParameter = new List<int>();
                        for (int i = 0; i < parameterList.Count; i++)
                        {
                            selectableParameter.Add(i);
                        }
                        while (selectableParameter.Count > 0)
                        {
                            int selectedIndex = UnityEngine.Random.Range(0, selectableParameter.Count);
                            if (!excludeName.Contains(parameterList[selectedIndex].parameterName))
                            {
                                result = parameterList[selectedIndex];
                            }
                            selectableParameter.RemoveAt(selectedIndex);
                        }
                    }
                    index--;
                }
            }
            else if (_parameters.ContainsKey(type))
            {
                var parameterList = _parameters[type];
                List<int> selectableParameter = new List<int>();
                for (int i = 0; i < selectableParameter.Count; i++)
                {
                    selectableParameter.Add(i);
                }
                while (selectableParameter.Count > 0)
                {
                    int selectedIndex = UnityEngine.Random.Range(0, selectableParameter.Count);
                    if (!excludeName.Contains(parameterList[selectedIndex].parameterName))
                    {
                        result = parameterList[selectedIndex];
                    }
                    selectableParameter.RemoveAt(selectedIndex);
                }
            }
            return result;
        }

        ParameterBuilder GetNewParameter(List<string> types, Dictionary<string, List<ParameterBuilder>> _parameterBuilders,string type = null)
        {
            string declareName = GetCheckedParameterName(_parameterBuilders);
            string declareType = type ?? GetRandomType(types);
            ParameterBuilder parameterBuilder = new ParameterBuilder();
            parameterBuilder.parameterName = declareName;
            parameterBuilder.parameterType = declareType;
            if (!_parameterBuilders.ContainsKey(declareType))
            {
                _parameterBuilders.Add(declareType, new List<ParameterBuilder>());
            }
            _parameterBuilders[declareType].Add(parameterBuilder);
            return parameterBuilder;
        }
        string GetRandomType(List<string> types)
        {
            return types[UnityEngine.Random.Range(0, types.Count)];
        }
        bool IsBaseType(string type)
        {
            return type == shortName || type == intName || type == longName
                || type == floatName || type == doubleName || type == charName
                || type == stringName;
        }

        CodeBuilder GenerateInvokeMethodCode(ParameterBuilder obj, IEnumerable<ClassBuilder> _classDic, Dictionary<string, List<ParameterBuilder>> _parameterDic)
        {
            var codeBuilder = new LineCodeBuilder();
            ClassBuilder classBuilder = FindClassByName(obj.parameterType, _classDic);
            int index = UnityEngine.Random.Range(0, classBuilder.methodBuilders.Count);
            var method = classBuilder.methodBuilders[index];
            string code = $"{obj.parameterName}.{method.methodName}(";
            if (method.parameterBuilders != null)
            {
                for (int i = 0; i < method.parameterBuilders.Count; i++)
                {
                    var methodParameter = GetAnyParameterByType(_parameterDic, method.parameterBuilders[i].parameterType,
                        obj.parameterName);
                    if (methodParameter == null)
                    {
                        code += GetValueByType(method.parameterBuilders[i].parameterType);
                    }
                    else
                    {
                        code += methodParameter.parameterName;
                    }
                    if (i < method.parameterBuilders.Count - 1)
                    {
                        code += ",";
                    }
                }
            }
            code += ")";
            codeBuilder.SetLine(code);
            return codeBuilder;
        }
        string GetValueByType(string type)
        {
            if (type == shortName)
            {
                return $"(short)UnityEngine.Random.Range(0, short.MaxValue)";
            }
            else if (type == intName)
            {
                return "(int)UnityEngine.Random.Range(0, int.MaxValue)";
            }
            else if (type == longName)
            {
                return "(long)UnityEngine.Random.Range(0, long.MaxValue)";
            }
            else if (type == floatName)
            {
                return "(float)UnityEngine.Random.Range(0, float.MaxValue)";
            }
            else if (type == doubleName)
            {
                return "(double)UnityEngine.Random.Range(0, float.MaxValue)";
            }
            else if (type == charName)
            {
                return "(char)UnityEngine.Random.Range(0f, 255)";
            }
            else if (type == stringName)
            {
                return "RandomString.GetString()";
            }
            else
            {
                return $"new {type}()";
            }
        }
    }
}
