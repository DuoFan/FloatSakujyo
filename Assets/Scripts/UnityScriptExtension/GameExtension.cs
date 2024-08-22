using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace GameExtension
{
    #region StaticExtension

    public static class ScriptExtension
    {
        #region Number

        public static int Abs(this int value) => Mathf.Abs(value);
        public static float Abs(this float value) => Mathf.Abs(value);
        public static int Digit(this int value) => (int)Mathf.Log10(value) + 1;
        public static float Pow(this int value, int pow) => Mathf.Pow(value, pow);
        public static float Pow(this float value, int pow) => Mathf.Pow(value, pow);
        public static float Sqrt(this int value) => Mathf.Sqrt(value);
        public static float Sqrt(this float value) => Mathf.Sqrt(value);

        public static float Round(this float value,int i)
        {
            double p = value;
            p = Math.Round((double)p, i);
            return (float)p;
        }

        public static List<int> GetIntList(int start, int end)
        {
            var list = new List<int>(end - start);

            for (int i = start; i < end; i++)
                list.Add(i);

            return list;
        }

        #endregion

        #region String

        public static int ToInt(this string str) => int.Parse(str);
        public static float ToFloat(this string str) => float.Parse(str);
        public static bool ToBool(this string str) => bool.Parse(str);

        #endregion

        #region Vector

        public static Vector2 Round(this Vector2 value, int i) 
        {
            return new Vector2(value.x.Round(i), value.y.Round(i));
        }  

        public static Vector3 Abs(this Vector3 value) => new Vector3(value.x.Abs(), value.y.Abs(), value.z.Abs());

        public static Vector3 Round(this Vector3 value,int i)
        {
            return new Vector3(value.x.Round(i), value.y.Round(i), value.z.Round(i));
        }

        #endregion

        #region Quaternion

        public static Quaternion Round(this Quaternion value, int i)
        {
            return new Quaternion(value.x.Round(i), value.y.Round(i), value.z.Round(i), value.w.Round(i));
        }

        #endregion

        #region Camera

        /// <summary>
        /// </summary>
        /// <param name="refValue"></param>
        /// 参考的高度或宽度
        /// <param name="refRes1"></param>
        /// 参考的宽高比或高宽比（小值）
        /// <param name="refRes2"></param>
        /// 参考的宽高比或高宽比（大值）
        /// <param name="refView1"></param>
        /// 参考的尺寸（小值）
        /// <param name="refView2"></param>
        /// 参考的尺寸（大值）
        /// <param name="isWidth"></param>
        /// 是否匹配宽度
        /// <returns></returns>
        public static Camera AdjustSize(this Camera camera, float refValue, float refRes1, float refRes2,
            float refSize1, float refSize2, bool isWidth)
        {
            float res;

            if (isWidth)
                res = (float)Screen.width / Screen.height;
            else
                res = (float)Screen.height / Screen.width;

            var dif = res - refRes1;

            var per = (refSize2 - refSize1) / (refRes2 - refRes1);

            camera.fieldOfView = refSize1 + dif * per;

            return camera;
        }

        /// <summary>
        /// </summary>
        /// <param name="refValue"></param>
        /// 参考的高度或宽度
        /// <param name="refRes1"></param>
        /// 参考的宽高比或高宽比（小值）
        /// <param name="refRes2"></param>
        /// 参考的宽高比或高宽比（大值）
        /// <param name="refView1"></param>
        /// 参考的视野（小值）
        /// <param name="refView2"></param>
        /// 参考的视野（大值）
        /// <param name="isWidth"></param>
        /// 是否匹配宽度
        /// <returns></returns>
        public static Camera AdjustView(this Camera camera, float refValue, float refRes1, float refRes2,
            float refView1, float refView2, bool isWidth)
        {
            float res;

            if (isWidth)
                res = (float)Screen.width / Screen.height;
            else
                res = (float)Screen.height / Screen.width;

            var dif = res - refRes1;

            var per = (refView2 - refView1) / (refRes2 - refRes1);

            camera.fieldOfView = refView1 + dif * per;

            return camera;
        }

        #endregion

        #region Color

        public static Color GetRandomColor(int min = 0, int max = 255)
        {
            if (min < 0)
                min = 0;

            if (max > 255)
                max = 255;

            int r = UnityEngine.Random.Range(min, max);

            int g = UnityEngine.Random.Range(min, max);

            int b = UnityEngine.Random.Range(min, max);

            Color32 color = new Color32((byte)r, (byte)g, (byte)b, 255);

            return color;
        }

        public static Color Inverse(this Color color)
        {
            var ivColor = Color.white - color;

            ivColor.a = 1;

            return ivColor;
        }

        #endregion

        #region Transform

        public static RectTransform ToRect(this Transform transform) => transform as RectTransform;

        #endregion

        #region RectTransform

        public enum AnchorPresets
        {
            BottomLeft,
            MiddleLeft,
            TopLeft,

            BottomCenter,
            MiddleCenter,
            TopCenter,

            BottomRight,
            MiddleRight,
            TopRight,

            BottomStretch,
            VertStretchLeft,
            VertStretchRight,
            VertStretchCenter,

            HorStretchTop,
            HorStretchMiddle,
            HorStretchBottom,

            StretchAll
        }

        public enum PivotPresets
        {
            TopLeft,
            TopCenter,
            TopRight,

            MiddleLeft,
            MiddleCenter,
            MiddleRight,

            BottomLeft,
            BottomCenter,
            BottomRight,
        }

        public static Dictionary<AnchorPresets, Vector2[]> anchordDic = new Dictionary<AnchorPresets, Vector2[]>()
        {
            { AnchorPresets.TopLeft, new Vector2[] { new Vector2(0, 1), new Vector2(0, 1) } },
            { AnchorPresets.TopCenter, new Vector2[] { new Vector2(0.5f, 1), new Vector2(0.5f, 1) } },
            { AnchorPresets.TopRight, new Vector2[] { new Vector2(1, 1), new Vector2(1, 1) } },
            { AnchorPresets.MiddleLeft, new Vector2[] { new Vector2(0, 0.5f), new Vector2(0, 0.5f) } },
            { AnchorPresets.MiddleCenter, new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f) } },
            { AnchorPresets.MiddleRight, new Vector2[] { new Vector2(1, 0.5f), new Vector2(1, 0.5f) } },
            { AnchorPresets.BottomLeft, new Vector2[] { new Vector2(0, 0), new Vector2(0, 0) } },
            { AnchorPresets.BottomCenter, new Vector2[] { new Vector2(0.5f, 0), new Vector2(0.5f, 0) } },
            { AnchorPresets.BottomRight, new Vector2[] { new Vector2(1, 0), new Vector2(1, 0) } },
            { AnchorPresets.HorStretchTop, new Vector2[] { new Vector2(0, 1), new Vector2(1, 1) } },
            { AnchorPresets.HorStretchMiddle, new Vector2[] { new Vector2(0, 0.5f), new Vector2(1, 0.5f) } },
            { AnchorPresets.HorStretchBottom, new Vector2[] { new Vector2(0, 0), new Vector2(1, 0) } },
            { AnchorPresets.VertStretchLeft, new Vector2[] { new Vector2(0, 0), new Vector2(0, 1) } },
            { AnchorPresets.VertStretchCenter, new Vector2[] { new Vector2(0.5f, 0), new Vector2(0.5f, 1) } },
            { AnchorPresets.VertStretchRight, new Vector2[] { new Vector2(1, 0), new Vector2(1, 1) } },
            { AnchorPresets.StretchAll, new Vector2[] { new Vector2(0, 0), new Vector2(1, 1) } }
        };

        public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
        {
            source.anchoredPosition = new Vector3(offsetX, offsetY, 0);
            var preset = anchordDic[allign];
            source.anchorMin = preset[0];
            source.anchorMax = preset[1];
        }

        public static AnchorPresets? GetAnchor(this RectTransform source)
        {
            foreach (var item in anchordDic)
            {
                if (source.anchorMin == item.Value[0])
                {
                    if (source.anchorMax == item.Value[1])
                        return item.Key;
                }
            }

            return null;
        }

        public static void SetPivot(this RectTransform source, PivotPresets preset)
        {
            switch (preset)
            {
                case (PivotPresets.TopLeft):
                    {
                        source.pivot = new Vector2(0, 1);
                        break;
                    }
                case (PivotPresets.TopCenter):
                    {
                        source.pivot = new Vector2(0.5f, 1);
                        break;
                    }
                case (PivotPresets.TopRight):
                    {
                        source.pivot = new Vector2(1, 1);
                        break;
                    }

                case (PivotPresets.MiddleLeft):
                    {
                        source.pivot = new Vector2(0, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleRight):
                    {
                        source.pivot = new Vector2(1, 0.5f);
                        break;
                    }

                case (PivotPresets.BottomLeft):
                    {
                        source.pivot = new Vector2(0, 0);
                        break;
                    }
                case (PivotPresets.BottomCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0);
                        break;
                    }
                case (PivotPresets.BottomRight):
                    {
                        source.pivot = new Vector2(1, 0);
                        break;
                    }
            }
        }

        #endregion

        #region ParticleSystem

        public static ParticleSystem ColorTo(this ParticleSystem sys, Color color)
        {
            var main = sys.main;

            main.startColor = color;

            return sys;
        }

        public static ParticleSystem ColorOverTimeTo(this ParticleSystem sys, (Color, float) start, (Color, float) end,
            (float, float) alpha1, (float, float) alpha2)
        {
            var col = sys.colorOverLifetime;
            col.enabled = true;

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(start.Item1, start.Item2), new GradientColorKey(end.Item1, end.Item2)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(alpha1.Item1, alpha1.Item2),
                    new GradientAlphaKey(alpha2.Item1, alpha2.Item2)
                }
            );

            col.color = grad;

            return sys;
        }

        public static ParticleSystem ColorOverTimeTo(this ParticleSystem sys, GradientColorKey[] colorKeys,
            GradientAlphaKey[] alphaKeys)
        {
            var col = sys.colorOverLifetime;
            if (!col.enabled)
                col.enabled = true;

            Gradient grad = new Gradient();

            grad.SetKeys(
                colorKeys,
                alphaKeys
            );

            col.color = grad;

            return sys;
        }

        #endregion

        #region IList

        public static T RandomSelect<T>(this IList<T> list, bool isRemove)
        {
            if (list.Count == 0)
                throw new Exception("list's Count is 0");
            int index = UnityEngine.Random.Range(0, list.Count);
            var result = list[index];
            if (isRemove)
                list.RemoveAt(index);
            return result;
        }

        public static IList<T> RemoveRange<T>(this IList<T> list, List<int> removeRange, bool isSorted)
        {
            if (removeRange.Count > list.Count)
            {
                Debug.Log($"List包含{list.Count}个元素,但需删除" +
                          $"{removeRange.Count}个元素");
                return list;
            }

            if (!isSorted)
            {
                removeRange.Sort();
            }

            if (removeRange.Count > 0 && removeRange[removeRange.Count - 1] >= list.Count)
            {
                Debug.Log($"List包含{list.Count}个元素,但需删除第" +
                          $"{removeRange[removeRange.Count - 1]}个元素");
                return list;
            }
            else if (removeRange.Count == list.Count)
            {
                list.Clear();
                return list;
            }

            int slow, fast, curIndex;
            slow = fast = curIndex = 0;
            for (int i = 0; i < list.Count; i++)
            {
                list[slow] = list[fast];
                if (curIndex < removeRange.Count && i == removeRange[curIndex])
                {
                    curIndex++;
                }
                else
                {
                    slow++;
                }

                fast++;
            }

            curIndex = list.Count - 1;
            for (int i = 0; i < removeRange.Count; i++)
            {
                list.RemoveAt(curIndex);
                curIndex--;
            }

            return list;
        }

        public static T[] Disorder<T>(this IList<T> list)
        {
            var result = new T[list.Count];
            for (int i = 0; i < list.Count - 1; i++)
            {
                var j = UnityEngine.Random.Range(i + 1, list.Count);
                result[i] = result[j];
                result[j] = list[i];
            }

            return result;
        }

        #endregion

        #region RigidBody

        public static void FreezePosition(this Rigidbody rigidBody, bool x, bool y, bool z)
        {
            var constraints = rigidBody.constraints;
            if (x)
            {
                constraints = constraints | RigidbodyConstraints.FreezePositionX;
            }
            else if ((constraints & RigidbodyConstraints.FreezePositionX) == RigidbodyConstraints.FreezePositionX)
            {
                constraints = constraints ^ RigidbodyConstraints.FreezePositionX;
            }

            if (y)
            {
                constraints = constraints | RigidbodyConstraints.FreezePositionY;
            }
            else if ((constraints & RigidbodyConstraints.FreezePositionY) == RigidbodyConstraints.FreezePositionY)
            {
                constraints = constraints ^ RigidbodyConstraints.FreezePositionY;
            }

            if (z)
            {
                constraints = constraints | RigidbodyConstraints.FreezePositionZ;
            }
            else if ((constraints & RigidbodyConstraints.FreezePositionZ) == RigidbodyConstraints.FreezePositionZ)
            {
                constraints = constraints ^ RigidbodyConstraints.FreezePositionZ;
            }

            rigidBody.constraints = constraints;
        }

        public static void FreezeRotation(this Rigidbody rigidBody, bool x, bool y, bool z)
        {
            var constraints = rigidBody.constraints;
            if (x)
            {
                constraints = constraints | RigidbodyConstraints.FreezeRotationX;
            }
            else if ((constraints & RigidbodyConstraints.FreezeRotationX) == RigidbodyConstraints.FreezeRotationX)
            {
                constraints = constraints ^ RigidbodyConstraints.FreezeRotationX;
            }

            if (y)
            {
                constraints = constraints | RigidbodyConstraints.FreezeRotationY;
            }
            else if ((constraints & RigidbodyConstraints.FreezeRotationY) == RigidbodyConstraints.FreezeRotationY)
            {
                constraints = constraints ^ RigidbodyConstraints.FreezeRotationY;
            }

            if (z)
            {
                constraints = constraints | RigidbodyConstraints.FreezeRotationZ;
            }
            else if ((constraints & RigidbodyConstraints.FreezeRotationZ) == RigidbodyConstraints.FreezeRotationZ)
            {
                constraints = constraints ^ RigidbodyConstraints.FreezeRotationZ;
            }

            rigidBody.constraints = constraints;
        }

        #endregion

        #region Text
        public static IEnumerator TransitionToNumber(this TMP_Text text, float updateSeconds, float curNumber, float targetNumber,
            Color textColor, Vector3 scale, Color? endColor = null, Regex regex = null, string format = null)
        {
            if (regex == null)
            {
                regex = new Regex(@"\d+");
            }

            float delta = (targetNumber - curNumber) / updateSeconds;
            text.color = textColor;
            var oldScale = text.transform.localScale;
            text.transform.localScale = scale;
            var oldText = text.text;
            while (updateSeconds > 0)
            {
                yield return null;
                curNumber += delta * Time.deltaTime;
                text.text = format != null ? curNumber.ToString(format) : curNumber.ToString();
                updateSeconds -= Time.deltaTime;
                oldText = text.text;
            }

            text.text = format != null ? targetNumber.ToString(format) : targetNumber.ToString();
            text.transform.localScale = oldScale;

            if (endColor == null)
                endColor = Color.white;

            float t = 0;
            while (t < 0.2f)
            {
                yield return null;
                t += Time.deltaTime;
                text.color = Color.Lerp(textColor, endColor.Value, t / 0.2f);
            }
            text.color = endColor.Value;

        }

        public class TransitionToNumberContext
        {
            public float targetNumber;
        }
        #endregion

        #region Image


        #endregion

        #region Array

        public static T[] RemoveElementFromArray<T>(this T[] array, T element)
        {
            var index = Array.IndexOf(array, element);
            if (index < 0)
            {
                return array.Clone() as T[];
            }

            T[] newArray = new T[array.Length - 1];

            Array.Copy(array, 0, newArray, 0, index);
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
            return newArray;
        }

        public static T[] AddElementToArray<T>(this T[] array, T element, bool isCheck)
        {
            if (isCheck && Array.IndexOf(array, element) >= 0)
            {
                return array.Clone() as T[];
            }
            T[] newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = element;
            return newArray;
        }

        #endregion

        #region Object

        public static T Cast<T>(this object data)
        {
            if (data == null)
            {
                var error = $"待转换类型数据为空";
                GameExtension.Logger.Error(error);
                throw new Exception(error);
            }
            var result = (T)data;
            if (result == null)
            {
                var type1 = data.GetType();
                var type2 = typeof(T);
                var error = $"类型转换失败，{type1}无法转换为{type2}";
                GameExtension.Logger.Error(error);
                throw new Exception(error);
            }
            return result;
        }

        public static bool TryCast<T>(this object data, out T result)
        {
            result = (T)data;
            return result != null && !result.Equals(default(T));
        }

        #endregion

        #region GameObject

        public static void CheckActiveSelf(this GameObject go, bool active)
        {
            if (go.gameObject.activeSelf != active)
            {
                go.gameObject.SetActive(active);
            }
        }

        #endregion
    }
    #endregion

    #region FileWindow

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    public static class WindowDll
    {
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        public static bool GetOpenFileName1([In, Out] OpenFileName ofn)
        {
            return GetOpenFileName(ofn);
        }

        public static string OpenFileWindow(string firstPath, params string[] filter)
        {
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            if (filter == null)
                ofn.filter = "All Files\0*.*\0\0";
            else
            {
                //ofn.filter = "(*.jpg*.png)\0*.png;.jpg\0\0";

                StringBuilder before = new StringBuilder();

                StringBuilder after = new StringBuilder();

                before.Append("(");

                for (int i = 0; i < filter.Length; i++)
                {
                    before.Append($"*.{filter[i]}");

                    after.Append($"*.{filter[i]}");

                    if (i != filter.Length - 1)
                        after.Append($";");
                }

                before.Append(")\0");

                after.Append("\0\0");

                before.Append(after);

                ofn.filter = before
                    .Append(after)
                    .ToString();
            }

            ofn.file = new string(new char[256]);

            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new string(new char[64]);

            ofn.maxFileTitle = ofn.fileTitle.Length;
            string path = firstPath;
            path = path.Replace('/', '\\');

            ofn.initialDir = path;

            ofn.title = "Open File";

            ofn.defExt = "JPG"; //显示文件的类型
            //注意 一下项目不一定要全选 但是0x00000008项不要缺少
            ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 |
                        0x00000008; //OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR

            if (WindowDll.GetOpenFileName(ofn))
            {
                Debug.Log("Selected file with full path: {0}" + ofn.file);

                return ofn.file;
            }

            return null;
        }
    }

    #endregion

    #region SelectRect

    public class SelectRectUI : MonoBehaviour
    {
        bool isTrigger;
        public bool IsTrigger => isTrigger;
        public event Func<bool> trigger;
        public event Func<bool> endTrigger;
        public event Func<bool> releaseTrigger;
        public event Action onClear;
        Vector2 startPos;

        Func<ISelect[]> getItems;
        ISelect[] items;
        HashSet<ISelect> selectItems = new HashSet<ISelect>();
        public HashSet<ISelect> SelectItems => selectItems;
        List<ISelect> waitRemoveItems = new List<ISelect>();

        Material drawMat;

        public SelectRectUI Set(Material _drawMat, Func<bool> _trigger, Func<bool> _endTrigger,
            Func<bool> _releaseTrigger, Func<ISelect[]> _getItems)
        {
            trigger = _trigger;

            endTrigger = _endTrigger;

            releaseTrigger = _releaseTrigger;

            getItems = _getItems;

            drawMat = _drawMat;

            return this;
        }

        public SelectRectUI Set(ISelect[] _items)
        {
            items = _items;

            return this;
        }

        void Update()
        {
            if (trigger() && !isTrigger)
            {
                isTrigger = true;

                ClearItems();

                if (getItems != null)
                    Set(getItems());

                startPos = Input.mousePosition;
            }
            else if (endTrigger() && isTrigger)
                isTrigger = false;
            else if (releaseTrigger() && selectItems.Count > 0)
                ClearItems();

            foreach (var item in selectItems)
            {
                if (item.Go == null)
                {
                    waitRemoveItems.Add(item);
                }
            }

            for (int i = 0; i < waitRemoveItems.Count; i++)
            {
                selectItems.Remove(waitRemoveItems[i]);
            }

            waitRemoveItems.Clear();
        }

        SelectRectUI Judge()
        {
            var cam = Camera.main;

            var mousePos = Input.mousePosition;

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];

                //var pos = cam.transform.InverseTransformPoint(item.Go.transform.position);

                //pos = RectTransformUtility.WorldToScreenPoint(cam, pos);
                var pos = item.Go.transform.position;
                pos = cam.WorldToScreenPoint(pos);

                if ((pos.x > startPos.x && pos.x < mousePos.x && pos.y > startPos.y && pos.y < mousePos.y) ||
                    (pos.x > startPos.x && pos.x < mousePos.x && pos.y < startPos.y && pos.y > mousePos.y) ||
                    (pos.x < startPos.x && pos.x > mousePos.x && pos.y > startPos.y && pos.y < mousePos.y) ||
                    (pos.x < startPos.x && pos.x > mousePos.x && pos.y < startPos.y && pos.y > mousePos.y))
                {
                    selectItems.Add(item.Select());
                }
                else
                {
                    selectItems.Remove(item.UnSelect());
                }
            }

            return this;
        }

        public SelectRectUI ClearItems()
        {
            foreach (var item in selectItems)
            {
                item.UnSelect();
            }

            selectItems.Clear();

            onClear?.Invoke();

            return this;
        }

        public bool IsSelected(ISelect item) => selectItems.Contains(item);

        public HashSet<T> ConvertItems<T>() where T : ISelect
        {
            HashSet<T> result = new HashSet<T>();
            foreach (var item in selectItems)
            {
                result.Add((T)item);
            }

            return result;
        }

        public void AddItem(ISelect item)
        {
            selectItems.Add(item);
        }

        public void AddItems(HashSet<ISelect> items)
        {
            selectItems.UnionWith(items);
        }

        private void OnGUI()
        {
            if (isTrigger)
                Draw();
        }

        private void Draw()
        {
            drawMat.SetPass(0);

            GL.PushMatrix(); //保存摄像机变换矩阵  
            GL.LoadPixelMatrix(); //设置用屏幕坐标绘图  
            //透明框  

            var mousePosition = Input.mousePosition;

            var lineColor = Color.white;
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex3(startPos.x, startPos.y, 0);
            GL.Vertex3(mousePosition.x, startPos.y, 0);
            GL.End();

            //下  
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex3(startPos.x, mousePosition.y, 0);
            GL.Vertex3(mousePosition.x, mousePosition.y, 0);
            GL.End();

            //左  
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex3(startPos.x, startPos.y, 0);
            GL.Vertex3(startPos.x, mousePosition.y, 0);
            GL.End();

            //右  
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex3(mousePosition.x, startPos.y, 0);
            GL.Vertex3(mousePosition.x, mousePosition.y, 0);
            GL.End();

            GL.PopMatrix(); //还原 

            Judge();
        }
    }

    public interface ISelect
    {
        GameObject Go { set; get; }
        ISelect Select();
        ISelect UnSelect();
    }

    #endregion

    #region Shape

    public class Circle
    {
        Vector2 center;
        public Vector2 Center => center;
        float radius;
        public float R => radius;
        float area;
        public float A => area;
        float circum;
        public float C => circum;

        public Circle(float _radius, Vector2 _center)
        {
            SetRadius(_radius);
            SetCenter(_center);
        }

        public void SetCenter(Vector2 _center)
        {
            center = _center;
        }

        public void SetRadius(float _radius)
        {
            radius = _radius;

            area = Mathf.PI * radius.Pow(2);

            circum = 2 * Mathf.PI * radius;
        }

        public Vector2 GetPosByX(float x, bool isUp)
        {
            if (x.Abs() > radius)
                throw new InvalidDataException("x should be less than radius");

            var y = (R.Pow(2) - x.Pow(2)).Sqrt();

            if (isUp)
                return new Vector2(center.x + x, center.y + y);
            else
                return new Vector2(center.x + x, center.y - y);
        }

        public Vector2 GetPosByY(float y, bool isRight)
        {
            if (y.Abs() > radius)
                throw new InvalidDataException("y should be less than radius");

            var x = (R.Pow(2) - y.Pow(2)).Sqrt();

            if (isRight)
                return new Vector2(center.x + x, center.y + y);
            else
                return new Vector2(center.x - x, center.y + y);
        }

        public Vector2 GetPosByDeg(float degree)
        {
            degree %= 360;

            degree *= Mathf.Deg2Rad;

            var x = center.x + R * Mathf.Cos(degree);

            var y = center.y + R * Mathf.Sin(degree);

            return new Vector2(x, y);
        }

        public float GetDegByPos(Vector2 pos)
        {
            if (pos.y > center.y)
                return Vector3.Angle(Vector3.right, pos);
            else
                return -Vector3.Angle(Vector3.right, pos);
        }
    }

    public class LogarithmicSpiral
    {
        Vector2 center;
        public Vector2 Center => center;
        float radius;
        public float R => radius;
        float deltaRad;
        float delta;
        public float Delta => delta;

        public LogarithmicSpiral(float _radius, Vector2 _center, float _delta)
        {
            radius = _radius;

            center = _center;

            delta = _delta;

            deltaRad = (90 + delta) * Mathf.Deg2Rad;
        }

        public Vector2 GetPos(float angle)
        {
            var rad = angle * Mathf.Deg2Rad;

            var p = 2 * Mathf.Exp(rad / Mathf.Tan(deltaRad));

            var x = p * Mathf.Cos(rad);

            var y = p * Mathf.Sin(rad);

            var pos = new Vector2(x, y);

            return pos;
        }
    }

    public static class Polygon
    {
        /// <summary>
        /// 得到多边形顶点数
        /// </summary>
        /// <param name="e">多边形边数</param>
        /// <param name="f">多边形面数</param>
        /// <returns></returns>
        public static int GetVertexNum(int e, int f) => e - f + 2;

        /// <summary>
        /// 得到多边形面数
        /// </summary>
        /// <param name="e">多边形边数</param>
        /// <param name="v">多边形顶点数</param>
        /// <returns></returns>
        public static int GetFaceNum(int e, int v) => e - v + 2;

        /// <summary>
        /// 得到多边形边数
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int GetEdgeNum(int v, int f) => v + f + 2;
    }

    #endregion

    #region PlayerPrefs

    public abstract class PrefData<T>
    {
        protected string key;
        protected T value;
        public PrefData(string _key) => key = _key;
        public abstract PrefData<T> SetValue(T _value);

        public T GetValue()
        {
            if (HasValue())
                return LoadValue();

            throw new Exception($"无该键值对:{key}");
        }

        public T GetValue(T defaultValue)
        {
            if (HasValue())
            {
                return LoadValue();
            }
            else
            {
                return defaultValue;
            }
        }

        protected bool isLoad;
        protected abstract T LoadValue();
        public bool HasValue() => PlayerPrefs.HasKey(key);
    }

    public class PrefInt : PrefData<int>
    {
        public PrefInt(string _key) : base(_key)
        {
        }

        public override PrefData<int> SetValue(int _value)
        {
            PlayerPrefs.SetInt(key, _value);

            value = _value;

            return this;
        }

        protected override int LoadValue()
        {
            if (!isLoad)
            {
                value = PlayerPrefs.GetInt(key);

                isLoad = true;
            }

            return value;
        }
    }

    public class PrefFloat : PrefData<float>
    {
        public PrefFloat(string _key) : base(_key)
        {
        }

        public override PrefData<float> SetValue(float _value)
        {
            PlayerPrefs.SetFloat(key, _value);

            value = _value;

            return this;
        }

        protected override float LoadValue()
        {
            if (!isLoad)
            {
                value = PlayerPrefs.GetFloat(key);

                isLoad = true;
            }

            return value;
        }
    }

    public class PrefString : PrefData<string>
    {
        public PrefString(string _key) : base(_key)
        {
        }

        public override PrefData<string> SetValue(string _value)
        {
            PlayerPrefs.SetString(key, _value);

            value = _value;

            return this;
        }

        protected override string LoadValue()
        {
            if (!isLoad)
            {
                value = PlayerPrefs.GetString(key);
                isLoad = true;
            }

            return value;
        }
    }

    public class PrefBool : PrefData<bool>
    {
        public PrefBool(string _key) : base(_key)
        {
        }

        public override PrefData<bool> SetValue(bool _value)
        {
            PlayerPrefs.SetInt(key, _value ? 1 : 0);
            value = _value;
            return this;
        }

        protected override bool LoadValue()
        {
            if (!isLoad)
            {
                value = PlayerPrefs.GetInt(key) == 1;

                isLoad = true;
            }

            return value;
        }
    }

    public class PrefJsonList<T, I> : PrefData<T> where T : IList<I>, new()
    {
        public PrefJsonList(string _key) : base(_key)
        {
        }

        public override PrefData<T> SetValue(T list)
        {
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(list));
            value = list;
            return this;
        }

        protected override T LoadValue()
        {
            if (!isLoad)
            {
                var str = PlayerPrefs.GetString(key, string.Empty);
                value = string.IsNullOrEmpty(str) ? new T() : JsonConvert.DeserializeObject<T>(str);
                isLoad = true;
            }

            return value;
        }

        public void Add(I element)
        {
            GetValue().Add(element);
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(value));
        }

        public void Remove(I element)
        {
            GetValue().Remove(element);
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(value));
        }

        public bool IsContains(I item)
        {
            return GetValue().Contains(item);
        }
    }

    public class PrefJson<T> : PrefData<T>
    {
        public PrefJson(string _key) : base(_key)
        {
        }

        public override PrefData<T> SetValue(T _value)
        {
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(_value));
            value = _value;
            return this;
        }

        protected override T LoadValue()
        {
            if (!isLoad)
            {
                var str = PlayerPrefs.GetString(key, string.Empty);
                value = string.IsNullOrEmpty(str) ? default : JsonConvert.DeserializeObject<T>(str);
                isLoad = true;
            }

            return value;
        }
    }

    #endregion

    #region SignChecker

    public static class SignChecker
    {
        //上一次签到日期
        static PrefInt DAY = new PrefInt("DAY");

        //总签到天数
        static PrefInt SIGNED_DAY = new PrefInt("SIGNED_DAY");

        //一周内签到天数
        static PrefInt SIGNED_DAY_IN_WEEK = new PrefInt("SIGNED_DAY_IN_WEEK");

        //一个月内签到天数
        static PrefInt SIGNED_DAY_IN_MONTH = new PrefInt("SIGNED_DAY_IN_MONTH");

        //一年内签到天数
        static PrefInt SIGNED_DAY_IN_YEAR = new PrefInt("SIGNED_DAY_IN_YEAR");

        //上一次签到的年份
        static PrefInt LAST_YEAR = new PrefInt("LAST_YEAR");

        //上一次签到的月份
        static PrefInt LAST_MONTH = new PrefInt("LAST_MONTH");

        //上一次签到的周数
        static PrefInt LAST_WEEK = new PrefInt("LAST_WEEK");
        static int Today => (int)(DateTime.Now - new DateTime(DateTime.Now.Year, 1, 1)).TotalDays + 1;

        public static int SignedDay => SIGNED_DAY.GetValue(0);
        public static int SignedWeek => SIGNED_DAY_IN_WEEK.GetValue(0);
        public static int SignedMonth => SIGNED_DAY_IN_MONTH.GetValue(0);
        public static int SignedYear => SIGNED_DAY_IN_YEAR.GetValue(0);

        public static bool IsNewDay
        {
            get
            {
                int lastDay = DAY.GetValue(0);
                return lastDay != Today;
            }
        }

        public static bool IsNewWeek
        {
            get
            {
                int lastWeek = LAST_WEEK.GetValue(0);
                int currentWeek =
                    CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday);
                return lastWeek != currentWeek;
            }
        }

        public static bool IsNewYear
        {
            get
            {
                int lastYear = LAST_YEAR.GetValue(0);
                return lastYear != DateTime.Now.Year;
            }
        }

        public static bool IsSigned
        {
            get
            {
                if (!SIGNED_DAY.HasValue())
                {
                    SIGNED_DAY.SetValue(0);
                    SIGNED_DAY_IN_WEEK.SetValue(0);
                    SIGNED_DAY_IN_MONTH.SetValue(0);
                    SIGNED_DAY_IN_YEAR.SetValue(0);
                    DAY.SetValue(0);
                    LAST_YEAR.SetValue(DateTime.Now.Year);
                    LAST_MONTH.SetValue(DateTime.Now.Month);
                    LAST_WEEK.SetValue(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now,
                        CalendarWeekRule.FirstDay, DayOfWeek.Monday));
                }

                return DAY.GetValue() == Today;
            }
        }

        public static void Sign()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            int currentWeek =
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
                    DayOfWeek.Monday);

            if (LAST_YEAR.GetValue(0) != currentYear)
            {
                SIGNED_DAY_IN_YEAR.SetValue(0);
            }

            if (LAST_MONTH.GetValue(0) != currentMonth)
            {
                SIGNED_DAY_IN_MONTH.SetValue(0);
            }

            if (LAST_WEEK.GetValue(0) != currentWeek)
            {
                SIGNED_DAY_IN_WEEK.SetValue(0);
            }

            SIGNED_DAY.SetValue(SignedDay + 1);
            SIGNED_DAY_IN_WEEK.SetValue(SignedWeek + 1);
            SIGNED_DAY_IN_MONTH.SetValue(SignedMonth + 1);
            SIGNED_DAY_IN_YEAR.SetValue(SignedYear + 1);
            DAY.SetValue(Today);
            LAST_YEAR.SetValue(currentYear);
            LAST_MONTH.SetValue(currentMonth);
            LAST_WEEK.SetValue(currentWeek);
        }
    }

    public struct TimeRecord
    {
        public int Years { get; set; }
        public int Months { get; set; }
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }

        [JsonConstructor]
        public TimeRecord(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            Years = years;
            Months = months;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
        }

        public TimeRecord(DateTime dateTime)
        {
            Years = dateTime.Year;
            Months = dateTime.Month;
            Days = dateTime.Day;
            Hours = dateTime.Hour;
            Minutes = dateTime.Minute;
            Seconds = dateTime.Second;
            Milliseconds = dateTime.Millisecond;
        }

        public void Set(DateTime dateTime)
        {
            Years = dateTime.Year;
            Months = dateTime.Month;
            Days = dateTime.Day;
            Hours = dateTime.Hour;
            Minutes = dateTime.Minute;
            Seconds = dateTime.Second;
            Milliseconds = dateTime.Millisecond;
        }

        public TimeRecord(TimeSpan timeSpan)
        {
            Seconds = (int)timeSpan.TotalSeconds;
            Milliseconds = 0;
            Minutes = 0;
            Hours = 0;
            Days = 0;
            Months = 0;
            Years = 0;
        }

        public DateTime ToDateTime()
        {
            return new DateTime(Years, Months, Days, Hours, Minutes, Seconds, Milliseconds);
        }

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(Days, Hours, Minutes, Seconds, Milliseconds);
        }
    }

    #endregion

    #region Timer

    public class Timer
    {
        public WaitForSecondsRealtime WaitForSecondsRealtime { get; protected set; }
        public float Timing { get; protected set; }
        bool isPause;
        MonoBehaviour owner;
        Action<float> onUpdate;
        Coroutine timingCo;

        public Timer Set(MonoBehaviour _owner, WaitForSecondsRealtime waitForSecondsRealtime, Action<float> _onUpdate)
        {
            owner = _owner;
            WaitForSecondsRealtime = waitForSecondsRealtime;
            onUpdate = _onUpdate;
            return this;
        }

        public void Start()
        {
            StartSince(0);
        }

        public void Resume()
        {
            StartSince(Timing);
        }

        public void Stop()
        {
            if (timingCo == null)
            {
                GameExtension.Logger.Log($"未开始记时却尝试停止记时:{owner.name}");
                return;
            }

            owner.StopCoroutine(timingCo);
            isPause = false;
        }

        public void StartSince(float sinceTime)
        {
            if (timingCo != null)
            {
                GameExtension.Logger.Log($"已在记时中却尝试重复记时:{owner.name}");
                return;
            }

            isPause = false;
            Timing = sinceTime;
            timingCo = owner.StartCoroutine(StartTimer());
        }

        public void Pause()
        {
            if (timingCo == null)
            {
                GameExtension.Logger.Log($"未开始记时却尝试暂停记时:{owner.name}");
                return;
            }

            if (isPause)
            {
                GameExtension.Logger.Log($"已记时却尝试重复暂停记时:{owner.name}");
                return;
            }

            isPause = true;
            owner.StopCoroutine(timingCo);
            timingCo = null;
        }

        IEnumerator StartTimer()
        {
            isPause = false;
            while (!isPause)
            {
                yield return WaitForSecondsRealtime;
                Timing += WaitForSecondsRealtime.waitTime;
                if (onUpdate != null) onUpdate(Timing);
            }

            timingCo = null;
        }
    }

    #endregion

    #region SuDoKu

    public static class SuDoKu
    {
        public enum Mode
        {
            Classical,
            Killer,
            Sawtooth,
            Stronghold,
            Sequence,
            Diagonal,
            Pyramid,
            Geometry
        }

        public enum Difficult
        {
            Primary,
            Skilled,
            Expert,
            Master
        }

        static bool[] constrains;
        static Map<int> finalAnswer;
        static Map<int> answer;
        static Mode? mode;

        static Difficult? difficult;

        //对角线数独
        public static Map<int>[] CreateDiagonal(Difficult difficult, out bool _isSuccess)
        {
            mode = Mode.Diagonal;
            var result = Create(difficult);
            _isSuccess = isSuccess;
            DisposeMemory();
            return result;
        }

        //堡垒数独图案模板
        static List<int[]> strongholdStyles = new List<int[]>()
        {
            new int[]
            {
                0, 1, 7, 8, 9, 17, 20, 21, 23, 24, 29, 30, 32, 33, 47, 48, 50, 51, 56, 57, 59, 60, 63, 71, 72, 73,
                79, 80
            },
            new int[] { 1, 6, 8, 9, 11, 16, 19, 24, 26, 31, 39, 41, 49, 54, 56, 61, 64, 69, 71, 72, 74, 79 },
            new int[] { 4, 12, 14, 20, 24, 28, 34, 36, 44, 46, 52, 56, 60, 66, 68, 76 },
            new int[] { 13, 22, 31, 37, 38, 39, 40, 41, 42, 43, 49, 58, 67 },
            new int[] { 0, 1, 9, 13, 14, 15, 24, 33, 47, 56, 65, 66, 67, 71, 79, 80 },
            new int[] { 10, 11, 15, 16, 19, 22, 25, 30, 32, 38, 42, 48, 50, 55, 58, 61, 64, 65, 69, 70 },
            new int[] { 0, 8, 11, 13, 15, 19, 22, 25, 31, 37, 38, 39, 41, 42, 43, 49, 55, 58, 61, 65, 67, 69, 72, 80 },
        };

        //所采用的图案
        static List<int> strongholdStyle;

        //是否成功生成
        static bool isSuccess;

        const int MAX_ERROR = 5;

        //堡垒数独
        public static Map<int>[] CreateStronghold(Difficult difficult, out List<int> _strongholdStyle,
            out bool _isSuccess)
        {
            mode = Mode.Stronghold;
            var result = Create(difficult);
            _strongholdStyle = strongholdStyle;
            _isSuccess = isSuccess;
            DisposeMemory();
            return result;
        }

        static List<int>[] pyramids = new List<int>[]
        {
            new List<int>()
            {
                3,
                4,
                5,
                6,
                7,
                13,
                14,
                15,
                23
            },
            new List<int>()
            {
                9,
                18,
                19,
                27,
                28,
                29,
                36,
                37,
                45
            },
            new List<int>()
            {
                35,
                43,
                44,
                51,
                52,
                53,
                61,
                62,
                71
            },
            new List<int>()
            {
                57,
                65,
                66,
                67,
                73,
                74,
                75,
                76,
                77
            }
        };

        //金字塔数独
        public static Map<int>[] CreatePyramid(Difficult difficult, out bool _isSuccess)
        {
            mode = Mode.Pyramid;
            var result = Create(difficult);
            _isSuccess = isSuccess;
            DisposeMemory();
            return result;
        }

        //普通数独
        public static Map<int>[] CreateClassical(Difficult difficult)
        {
            var result = Create(difficult);
            DisposeMemory();
            return result;
        }

        static Map<int>[] Create(Difficult _difficult)
        {
            if (mode == null) mode = Mode.Classical;
            difficult = _difficult;
            Init();
            isSuccess = true;
            BuildFinalAnswer();
            if (!isSuccess) return null;
            BuildAnswer();
            var result = new Map<int>[] { finalAnswer, answer };
            return result;
        }

        static void BuildFinalAnswer()
        {
            Node head = new Node();
            Node cur = head;
            for (int i = 0; i < 81; i++)
            {
                Node cell = new Node();
                cell.index = i;
                cur.SetNext(cell);
                cur = cur.next;
            }

            Slove(head);
        }

        private static void Slove(Node head)
        {
            var cur = head.next;
            int count = 0;
            int maxCount = 10000;
            while (cur != head && cur != null)
            {
                if (cur.selectableNum.Count == 0)
                {
                    InitCellSelectNum(cur);
                }

                if (cur.answer > 0)
                {
                    SetCellAnswer(cur, 0, true);
                }

                if (cur.selectableNum.Count == 0)
                {
                    cur = cur.last;
                }
                else
                {
                    SelectNumForCell(cur, true);
                    cur = cur.next;
                }

                count++;
                if (count >= maxCount)
                {
                    isSuccess = false;
                    return;
                }
            }
        }

        static void BuildAnswer()
        {
            answer = finalAnswer.Clone() as Map<int>;
            List<int> selectableIndex = new List<int>();
            for (int i = 0; i < 81; i++)
            {
                selectableIndex.Add(i);
            }

            int hintCount = GetHintCount();
            Debug.Log(hintCount);
            int cellCount = 81;
            Stack<string> answerHistory = new Stack<string>();
            bool isFirstRemove = true;
            int errorCount = 0;
            while (cellCount > hintCount && selectableIndex.Count > 0 && errorCount < MAX_ERROR)
            {
                var index = selectableIndex.RandomSelect(true);
                if (isFirstRemove)
                {
                    SetIndexOfConstrain(index, answer[GetRow(index), GetCol(index)], false);
                    answer[GetRow(index), GetCol(index)] = 0;
                    isFirstRemove = false;
                    cellCount--;
                }
                else if (TryRemoveAnswer(index))
                {
                    cellCount--;
                }
                else
                {
                    errorCount++;
                }
            }

            Debug.Log(cellCount);
        }

        private static bool TryRemoveAnswer(int index)
        {
            int row = GetRow(index);
            int col = GetCol(index);
            int _answer = answer[row, col];
            SetIndexOfConstrain(index, _answer, false);
            answer[row, col] = 0;
            for (int i = 1; i < 10; i++)
            {
                if (i == _answer) continue;
                if (!IsConstrainCanSet(index, i)) continue;
                SetIndexOfConstrain(index, i, true);
                if (TrySlove()) //能解题成功，说明当答案为i时存在多解，不可以挖去此洞
                {
                    SetIndexOfConstrain(index, i, false);
                    SetIndexOfConstrain(index, _answer, true);
                    answer[row, col] = _answer;
                    return false;
                }
                else //不能解题成功，则当答案为i时不存在多解,还原状态
                {
                    SetIndexOfConstrain(index, i, false);
                    answer[row, col] = 0;
                }
            }

            return true;
        }

        private static bool TrySlove()
        {
            Node head = new Node();
            Node cur = head;
            for (int i = 0; i < 81; i++)
            {
                if (!constrains[i])
                {
                    var node = new Node();
                    node.index = i;
                    cur.SetNext(node);
                    cur = cur.next;
                }
            }

            return TrySlove(head);
        }

        private static bool TrySlove(Node head)
        {
            var cur = head.next;
            while (cur != head && cur != null)
            {
                if (cur.selectableNum.Count == 0)
                {
                    InitCellSelectNum(cur);
                }

                if (cur.answer > 0)
                {
                    SetCellAnswer(cur, 0, false);
                }

                if (cur.selectableNum.Count == 0)
                {
                    cur = cur.last;
                }
                else
                {
                    SelectNumForCell(cur, false);
                    cur = cur.next;
                }
            }

            if (cur == null)
            {
                cur = head.next;
                while (cur != null)
                {
                    SetCellAnswer(cur, 0, false);
                    cur = cur.next;
                }

                return true;
            }
            else return false;
        }

        static void Init()
        {
            finalAnswer = new Map<int>().Set(9, 9);
            answer = new Map<int>().Set(9, 9);
            if (mode == Mode.Diagonal)
                InitDiagonalConstrains();
            else if (mode == Mode.Stronghold)
                InitStrongholdConstrains();
            else if (mode == Mode.Pyramid)
                InitPyramidConstrains();
            else
                InitClassicalConstrains();
        }

        static void InitClassicalConstrains() => constrains = new bool[324];

        //加上正对角线和反对角线的18个约束
        static void InitDiagonalConstrains() => constrains = new bool[324 + 18];

        //加上四个金字塔的36个约束
        static void InitPyramidConstrains() => constrains = new bool[324 + 36];

        static void InitStrongholdConstrains()
        {
            InitClassicalConstrains();
            var style = strongholdStyles.RandomSelect(false);
            strongholdStyle = style.ToList();
        }

        static int GetRow(int index) => index / 9;
        static int GetCol(int index) => index % 9;

        static void SetIndexOfConstrain(int index, int answer, bool flag)
        {
            int row = index / 9;
            int col = index % 9;
            int constrainRow = 80 + row * 9;
            int constrainCol = 161 + col * 9;
            int constrainDivide = 242 + (row / 3 * 3 + col / 3) * 9;
            constrains[constrainRow + answer] = flag;
            constrains[constrainCol + answer] = flag;
            constrains[constrainDivide + answer] = flag;
            constrains[index] = flag;
            if (mode == Mode.Diagonal)
                SetDiagonalConstrain(index, answer, flag);
            else if (mode == Mode.Pyramid)
                SetPyramidConstrain(index, answer, flag);
        }

        static void SetDiagonalConstrain(int index, int answer, bool flag)
        {
            int row = GetRow(index);
            int col = GetCol(index);
            if (row == col)
            {
                var diagonalStart = 323;
                constrains[diagonalStart + answer] = flag;
            }

            if (row + col == finalAnswer.Row - 1)
            {
                var ivDiagonalStart = 332;
                constrains[ivDiagonalStart + answer] = flag;
            }
        }

        static void SetPyramidConstrain(int index, int answer, bool flag)
        {
            int constrain = GetPyramidConstrainIndex(index, answer);
            if (constrain < 0) return;
            else constrains[constrain] = flag;
        }

        static bool IsConstrainCanSet(int index, int answer)
        {
            int row = index / 9;
            int col = index % 9;
            int constrainRow = 80 + row * 9;
            int constrainCol = 161 + col * 9;
            int constrainDivide = 242 + (row / 3 * 3 + col / 3) * 9;
            int constrainNo = row * 9 + col;
            bool canSet = !constrains[constrainRow + answer]
                          && !constrains[constrainCol + answer]
                          && !constrains[constrainDivide + answer]
                          && !constrains[constrainNo];
            if (canSet)
            {
                if (mode == Mode.Diagonal)
                    canSet &= CheckDiagonalConstrain(index, answer);
                else if (mode == Mode.Stronghold)
                    canSet &= CheckStrongholdConstrain(index, answer);
                else if (mode == Mode.Pyramid)
                    canSet &= CheckPyramidConstrain(index, answer);
            }

            return canSet;
        }

        static bool CheckDiagonalConstrain(int index, int answer)
        {
            int row = index / 9;
            int col = index % 9;
            bool canSet = true;
            //处于正对角线上
            if (row == col)
            {
                var diagonalStart = 323;
                canSet = canSet && !constrains[diagonalStart + answer];
            }

            //处于负对角线上
            if (row + col == finalAnswer.Row - 1)
            {
                var ivDiagonalStart = 332;
                canSet = canSet && !constrains[ivDiagonalStart + answer];
            }

            return canSet;
        }

        static bool CheckStrongholdConstrain(int index, int answer)
        {
            int row = index / 9;
            int col = index % 9;
            List<int> neibors = new List<int>();

            void TryAddNeibor(int _row, int _col, bool isStronghold)
            {
                if (_row < 0 || _row >= finalAnswer.Row
                             || _col < 0 || _col >= finalAnswer.Col)
                    return;
                int _index = _row * 9 + _col;
                //判断该邻居是不是堡垒,才考虑其数值
                if (strongholdStyle.Contains(_index) == isStronghold)
                    neibors.Add(finalAnswer[_row, _col]);
            }

            //该格是是堡垒的情况下,检查约束
            if (strongholdStyle.Contains(index))
            {
                //添加不是堡垒的邻居
                TryAddNeibor(row - 1, col, false);
                TryAddNeibor(row, col - 1, false);
                for (int i = 0; i < neibors.Count; i++)
                {
                    if (neibors[i] > answer)
                        return false;
                }
            }
            //该格不是堡垒的情况下,检查周围堡垒的约束
            else
            {
                //添加是堡垒的邻居
                TryAddNeibor(row - 1, col, true);
                TryAddNeibor(row, col - 1, true);
                for (int i = 0; i < neibors.Count; i++)
                {
                    if (neibors[i] < answer)
                        return false;
                }
            }

            return true;
        }

        static bool CheckPyramidConstrain(int index, int answer)
        {
            int constrain = GetPyramidConstrainIndex(index, answer);
            if (constrain < 0) return true;
            else return !constrains[constrain];
        }

        static int GetPyramidConstrainIndex(int index, int answer)
        {
            int pyramidStart = 323;
            for (int i = 0; i < pyramids.Length; i++)
            {
                foreach (var j in pyramids[i])
                {
                    if (j == index)
                    {
                        return pyramidStart + i * 9 + answer;
                    }
                }
            }

            return -1;
        }

        static int SelectNumForCell(Node cell, bool isSetFinal)
        {
            int number = cell.selectableNum[UnityEngine.Random.Range(0, cell.selectableNum.Count)];
            cell.selectableNum.Remove(number);
            SetCellAnswer(cell, number, isSetFinal);
            return number;
        }

        static void InitCellSelectNum(Node cell)
        {
            for (int i = 1; i < 10; i++)
            {
                if (IsConstrainCanSet(cell.index, i))
                {
                    cell.selectableNum.Add(i);
                }
            }
        }

        static void SetCellAnswer(Node cell, int answer, bool isSetFinal)
        {
            if (cell.answer > 0)
            {
                SetIndexOfConstrain(cell.index, cell.answer, false);
            }

            if (answer > 0)
            {
                SetIndexOfConstrain(cell.index, answer, true);
            }

            cell.answer = answer;
            if (isSetFinal)
                finalAnswer[GetRow(cell.index), GetCol(cell.index)] = answer;
        }

        static int GetHintCount()
        {
            switch (difficult)
            {
                case Difficult.Primary:
                    return UnityEngine.Random.Range(30, 45);
                case Difficult.Skilled:
                    return UnityEngine.Random.Range(30, 40);
                case Difficult.Expert:
                    return UnityEngine.Random.Range(25, 30);
                case Difficult.Master:
                    return UnityEngine.Random.Range(17, 25);
            }

            return 0;
        }

        static void DisposeMemory()
        {
            constrains = null;
            finalAnswer = null;
            answer = null;
            mode = null;
            difficult = null;
            strongholdStyle = null;
        }

        class Node
        {
            public int index;
            public int answer;
            public Node next;
            public Node last;
            public List<int> selectableNum;

            public Node()
            {
                selectableNum = new List<int>();
            }

            public void SetNext(Node node)
            {
                next = node;
                node.last = this;
            }
        }
    }

    #endregion

    #region Bezier

    public class BezierUtils
    {
        /// <summary>
        /// 线性贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t"></param>T值
        /// <param name="p0"></param>起始点
        /// <param name="p1"></param>控制点
        /// <returns></returns>根据T值计算出来的贝赛尔曲线点
        private static Vector3 CalculateLineBezierPoint(float t, Vector3 p0, Vector3 p1)
        {
            float u = 1 - t;

            Vector3 p = u * p0;
            p += t * p1;

            return p;
        }

        /// <summary>
        /// 二次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t"></param>T值
        /// <param name="p0"></param>起始点
        /// <param name="p1"></param>控制点
        /// <param name="p2"></param>目标点
        /// <returns></returns>根据T值计算出来的贝赛尔曲线点
        private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;

            return p;
        }

        /// <summary>
        /// 三次贝塞尔曲线，根据T值，计算贝塞尔曲线上面相对应的点
        /// </summary>
        /// <param name="t">插量值</param>
        /// <param name="p0">起点</param>
        /// <param name="p1">控制点1</param>
        /// <param name="p2">控制点2</param>
        /// <param name="p3">尾点</param>
        /// <returns></returns>
        private static Vector3 CalculateThreePowerBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ttt = tt * t;
            float uuu = uu * u;

            Vector3 p = uuu * p0;
            p += 3 * t * uu * p1;
            p += 3 * tt * u * p2;
            p += ttt * p3;

            return p;
        }

        /// <summary>
        /// 获取存储贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint"></param>控制点
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum"></param>采样点的数量
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetLineBeizerList(Vector3 startPoint, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 1; i <= segmentNum; i++)
            {
                float t = i / (float)segmentNum;
                Vector3 pixel = CalculateLineBezierPoint(t, startPoint, endPoint);
                path[i - 1] = pixel;
                Debug.Log(path[i - 1]);
            }

            return path;
        }

        /// <summary>
        /// 获取存储的二次贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint"></param>控制点
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum"></param>采样点的数量
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetQuadraticBezierList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint,
            int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 1; i <= segmentNum; i++)
            {
                float t = i / (float)segmentNum;
                Vector3 pixel = CalculateCubicBezierPoint(t, startPoint,
                    controlPoint, endPoint);
                path[i - 1] = pixel;
            }

            return path;
        }

        /// <summary>
        /// 获取存储的三次贝塞尔曲线点的数组
        /// </summary>
        /// <param name="startPoint"></param>起始点
        /// <param name="controlPoint1"></param>控制点1
        /// <param name="controlPoint2"></param>控制点2
        /// <param name="endPoint"></param>目标点
        /// <param name="segmentNum"></param>采样点的数量
        /// <returns></returns>存储贝塞尔曲线点的数组
        public static Vector3[] GetThreePowerBezierList(Vector3 startPoint, Vector3 controlPoint1,
            Vector3 controlPoint2, Vector3 endPoint, int segmentNum)
        {
            Vector3[] path = new Vector3[segmentNum];
            for (int i = 1; i <= segmentNum; i++)
            {
                float t = i / (float)segmentNum;
                Vector3 pixel = CalculateThreePowerBezierPoint(t, startPoint,
                    controlPoint1, controlPoint2, endPoint);
                path[i - 1] = pixel;
            }

            return path;
        }
    }

    #endregion

    #region ClassSet

    public static class ClassSet
    {
        public static (string[], System.Type[]) GetSet<T>()
        {
            return GetSet(typeof(T));
        }

        public static (string[], System.Type[]) GetSet(Type ieffect)
        {
            var effectList = new List<string>();
            var effectTypeList = new List<System.Type>();
            var assembly = ieffect.Assembly;
            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (ieffect.IsAssignableFrom(types[i]) && !types[i].IsAbstract)
                {
                    if (types[i] != ieffect)
                    {
                        effectTypeList.Add(types[i]);
                        effectList.Add(types[i].Name);
                    }
                }
            }

            var result = (effectList.ToArray(), effectTypeList.ToArray());
            return result;
        }
    }

    #endregion

    #region ReflectionUtil

    public static class ReflectionUtil
    {
        public static void SetProperty<T>(object obj, string property, T value)
        {
            var type = obj.GetType();
            var target = type.GetProperty(property);
            target.SetValue(obj, value);
        }

        public static void SetField<T>(object obj, string field, T value)
        {
            var type = obj.GetType();
            var target = type.GetField(field, BindingFlags.NonPublic
                                              | BindingFlags.Public | BindingFlags.Instance);
            target.SetValue(obj, value);
        }
    }

    #endregion

    #region Serialize

    [System.Serializable]
    public struct SerializeVector3
    {
        public float x;
        public float y;
        public float z;

        [Newtonsoft.Json.JsonConstructor]
        public SerializeVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializeVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public override string ToString()
        {
            return $"({x},{y},{z})";
        }

        public static implicit operator Vector3(SerializeVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
    }

    [System.Serializable]
    public struct SerializeVector3Int
    {
        public int x;
        public int y;
        public int z;

        [Newtonsoft.Json.JsonConstructor]
        public SerializeVector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x},{y},{z})";
        }

        public static implicit operator Vector3Int(SerializeVector3Int vector)
        {
            return new Vector3Int(vector.x, vector.y, vector.z);
        }
    }

    [System.Serializable]
    public struct SerializeVector2
    {
        [JsonIgnore]
        public static readonly SerializeVector2 Zero = new SerializeVector2(0, 0);

        public float x;
        public float y;

        [Newtonsoft.Json.JsonConstructor]
        public SerializeVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public SerializeVector2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }

        public static implicit operator Vector2(SerializeVector2 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
    }

    [System.Serializable]
    public struct SerializeVector2Int
    {
        public int x;
        public int y;

        [Newtonsoft.Json.JsonConstructor]
        public SerializeVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }

        public static implicit operator Vector2Int(SerializeVector2Int vector)
        {
            return new Vector2Int(vector.x, vector.y);
        }
    }

    [System.Serializable]
    public struct SerializeQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        [Newtonsoft.Json.JsonConstructor]
        public SerializeQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public SerializeQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public override string ToString()
        {
            return $"({x},{y},{z},{w})";
        }

        public static implicit operator Quaternion(SerializeQuaternion quaternion)
        {
            return new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
    }

    #endregion

    #region TimeoutCleaner

    public class TimeoutCleaner : MonoBehaviour
    {
        static List<IClearableInTimeout> elements;

        static Coroutine cleaningCo;

        /// <summary>
        /// 每次检查清除的间隔时间
        /// </summary>
        public static float CleaningInterval { get; set; }
        /// <summary>
        /// 一帧中最多的清除对象
        /// </summary>
        public static float MaxCleanCountPerFrame { get; set; }

        static TimeoutCleaner instance;

        public static void StartCleaning(float cleaningInterval, float maxCleanCountPerFrame)
        {
            if (cleaningInterval < 0)
            {
                cleaningInterval = 10;
            }

            if (maxCleanCountPerFrame < 0)
            {
                maxCleanCountPerFrame = 10;
            }

            CleaningInterval = cleaningInterval;
            MaxCleanCountPerFrame = maxCleanCountPerFrame;
            if (cleaningCo != null)
            {
                GameExtension.Logger.Warning("清理协程已开始");
            }
            else
            {
                if (elements == null)
                {
                    elements = new List<IClearableInTimeout>();
                }
                if (instance == null)
                {
                    instance = new GameObject("TimeoutCleaner").AddComponent<TimeoutCleaner>();
                    GameObject.DontDestroyOnLoad(instance.gameObject);
                }
                cleaningCo = instance.StartCoroutine(ClearingElements());
            }
        }

        public static void StopCleaning()
        {
            if (cleaningCo == null)
            {
                GameExtension.Logger.Warning("清理协程未开始,却试图停止");
            }
            else
            {
                instance.StopCoroutine(cleaningCo);
                cleaningCo = null;
            }
        }

        static IEnumerator ClearingElements()
        {
            var waitForCleaning = new WaitForSecondsRealtime(CleaningInterval);
            float curTime;
            int cleanCount = 0;
            while (true)
            {
                if (waitForCleaning.waitTime != CleaningInterval)
                {
                    waitForCleaning.waitTime = CleaningInterval;
                }
                yield return waitForCleaning;
                curTime = Time.realtimeSinceStartup;
                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    if (elements[i].CleanInterval < 0)
                    {
                        continue;
                    }

                    if (elements[i].IsEmpty())
                    {
                        continue;
                    }

                    if (IClearableInTimeout.IsTimeout(elements[i], curTime))
                    {
                        cleanCount += elements[i].Clear();
                    }

                    if (cleanCount >= MaxCleanCountPerFrame)
                    {
                        yield return null;
                        cleanCount = 0;
                    }
                }

                cleanCount = 0;
            }
        }

        public static void AddElementToClean(IClearableInTimeout e)
        {
            if (elements != null)
            {
                elements.Add(e);
            }
            else
            {
                var error = "清理列表未初始化";
                GameExtension.Logger.Warning(error);
            }
        }

        public static void RemoveElementToClean(IClearableInTimeout e)
        {
            if (elements != null)
            {
                elements.Remove(e);
            }
            else
            {
                var error = "清理列表未初始化";
                GameExtension.Logger.Error(error);
                throw new Exception(error);
            }
        }
    }

    public interface IClearableInTimeout
    {
        float LastUsedTime { get; }
        float CleanInterval { get; }
        bool IsEmpty();
        /// <summary>
        /// 返回清除的对象数量
        /// </summary>
        /// <returns></returns>
        int Clear();

        static bool IsTimeout(IClearableInTimeout e, float curTime)
        {
            return curTime - e.LastUsedTime > e.CleanInterval;
        }
    }

    #endregion
}