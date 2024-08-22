using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEditor;
using UnityEngine;
using System;
using EditorExtension;
using UnityEngine.Tilemaps;
using System.Reflection;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine.UIElements;

namespace EditorExtension
{
    public class SceneGUIManager
    {
        public static SceneGUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneGUIManager();
                    SceneView.duringSceneGui += instance.OnSceneGUI;
                }
                return instance;
            }
        }
        static SceneGUIManager instance;

        public static void Release()
        {
            if (instance != null)
            {
                SceneView.duringSceneGui -= instance.OnSceneGUI;
                instance = null;
            }
        }

        ~SceneGUIManager()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private bool canDraw = false;
        private bool canSelect = false;
        Dictionary<object, SceneGUIElement> sceneGUIElements;

        private bool isSelecting = false;
        private Vector2 selectionStart;
        private Vector2 selectionEnd;
        private Rect selectionRect;
        public event Action<Rect, SelectMode> OnDragSelectRect;
        public event Action<SceneGUIElement[], SelectMode> OnSelectedElement;

        public void AddSceneGUIElement(object key, SceneGUIElement element)
        {
            if (sceneGUIElements == null)
            {
                sceneGUIElements = new Dictionary<object, SceneGUIElement>();
            }
            sceneGUIElements[key] = element;
        }
        public void RemoveSceneGUIElement(object key)
        {
            if (sceneGUIElements == null)
            {
                return;
            }
            if (sceneGUIElements.ContainsKey(key))
            {
                sceneGUIElements[key].OnRemove();
                sceneGUIElements.Remove(key);
            }
        }
        public void ClearSceneGUIElement()
        {
            sceneGUIElements = null;
            OnSelectedElement = null;
            OnDragSelectRect = null;    
        }
        public void StartSelect()
        {
            canSelect = true;
        }
        public void StopSelect()
        {
            canSelect = false;
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            Handles.BeginGUI();

            DrawTools(sceneView);

            if (canDraw)
            {
                DrawElements(sceneView);
            }
            if(canSelect)
            {
                DrawSelectionRect(sceneView);
            }

            Handles.EndGUI();
        }
        private void DrawTools(SceneView sceneView)
        {
            canDraw = GUILayout.Toggle(canDraw, "绘制GUI");
            canSelect = GUILayout.Toggle(canSelect, "绘制选框");
        }
        private void DrawElements(SceneView sceneView)
        {
            if (sceneGUIElements != null)
            {
                var elements = sceneGUIElements.Values;
                foreach (var element in elements)
                {
                    element.Draw(sceneView);
                }
            }
        }
        void DrawSelectionRect(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1))
            {
                isSelecting = true;
                selectionStart = e.mousePosition;
                selectionRect = new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0);
                e.Use();
            }

            if (e.type == EventType.MouseDrag && (e.button == 0 || e.button == 1) && isSelecting)
            {
                selectionEnd = e.mousePosition;
                selectionRect = GetScreenRect(selectionStart, selectionEnd);
                OnDragSelectRect?.Invoke(selectionRect, e.button == 0 ? SelectMode.Select : SelectMode.Deselect);
                e.Use();
            }

            if (e.type == EventType.MouseUp && (e.button == 0 || e.button == 1) && isSelecting)
            {
                isSelecting = false;
                e.Use();
                if (OnSelectedElement != null)
                {
                    List<SceneGUIElement> selectedElements = new List<SceneGUIElement>();
                    foreach (var element in sceneGUIElements.Values)
                    {
                        var screenPos = HandleUtility.WorldToGUIPoint(element.sceneGUIContext.worldPos);
                        if (selectionRect.Contains(screenPos))
                        {
                            selectedElements.Add(element);
                        }
                    }
                    OnSelectedElement.Invoke(selectedElements.ToArray(), e.button == 0 ? SelectMode.Select : SelectMode.Deselect);
                }
            }

            if (isSelecting)
            {
                // Draw a rectangle on the Scene view to represent the selection area
                Handles.BeginGUI();
                GUI.color = e.button == 0 ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f);
                GUI.DrawTexture(selectionRect, EditorGUIUtility.whiteTexture);
                Handles.EndGUI();
            }
        }
        private Rect GetScreenRect(Vector2 start, Vector2 end)
        {
            return Rect.MinMaxRect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
        }
    }

    public enum SelectMode
    {
        Select,Deselect
    }

    public abstract class SceneGUIElement
    {
        public SceneGUIContext sceneGUIContext;
        Vector2 scrollPos;
        protected Rect area;
        public virtual void Draw(SceneView sceneView)
        {
            Vector3 screenPos = HandleUtility.WorldToGUIPoint(sceneGUIContext.worldPos);

            // 获取 Scene 视图的屏幕范围
            Rect sceneViewRect = sceneView.camera.pixelRect;

            // 检查按钮是否在可见范围内
            if (!sceneViewRect.Contains(screenPos))
            {
                return;
            }

            Color originalColor = GUI.color;
            GUI.color = sceneGUIContext.color != default ? sceneGUIContext.color : originalColor;

            area = new Rect(screenPos.x, screenPos.y, sceneGUIContext.size.x * 2, sceneGUIContext.size.y * 2);
            GUILayout.BeginArea(area);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(sceneGUIContext.size.x * 2), GUILayout.Height(sceneGUIContext.size.y * 2));

            InternalDraw(sceneView);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
            GUI.color = originalColor;
        }

        public abstract void InternalDraw(SceneView sceneView);
        public override int GetHashCode()
        {
            return sceneGUIContext.worldPos.GetHashCode();
        }

        public virtual void OnRemove()
        {

        }
    }
    public class SceneGUIButton : SceneGUIElement
    {
        public event Action OnClick;
        public override void InternalDraw(SceneView sceneView)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = sceneGUIContext.fontSize >= 0 ? sceneGUIContext.fontSize : 14;

            if (GUILayout.Button(sceneGUIContext.label))
            {
                OnClick?.Invoke();
            }
        }
    }
    public abstract class SceneGUIDynamicElement: SceneGUIElement
    {
        public event Action<object> OnValueChanged
        {
            add => onValueChanged += value;
            remove => onValueChanged -= value;
        }
        protected Action<object> onValueChanged;
    }
    public abstract class SceneGUIDynamicElement<T> : SceneGUIDynamicElement
    {
        public T Value { get; protected set; }
        protected bool isDirty;
        protected bool isFoldout;
        public SceneGUIDynamicElement(T value)
        {
            this.Value = value;
        }
        public override void InternalDraw(SceneView sceneView)
        {
            var color = GUI.color;
            GUI.color = Color.black;
            isFoldout = EditorGUILayout.Foldout(isFoldout, sceneGUIContext.label);
            GUI.color = color;
            if (isFoldout)
            {
                var newValue = UpdateValue();
                if ((newValue != null && !newValue.Equals(Value)))
                {
                    Value = newValue;
                    onValueChanged?.Invoke(newValue);
                    isDirty = false;
                }
                else if (isDirty)
                {
                    onValueChanged?.Invoke(Value);
                    isDirty = false;
                }
            }
        }
        protected abstract T UpdateValue();
    }
    public class SceneGUIArrayField<T, I> : SceneGUIDynamicElement<T[]> where I : SceneGUIDynamicElement<T>
    {
        List<I> elements;
        public SceneGUIArrayField(T[] value) : base(value)
        {
            elements = new List<I>();
            var type = typeof(T);
            var ctor = typeof(I).GetConstructor(new Type[] { type });
            for (int i = 0; i < value.Length; i++)
            {
                var element = ctor.Invoke(new object[] { value[i] }) as I;
                element.OnValueChanged += OnElementValueChanged;
                elements.Add(element);
            }
        }

        void OnElementValueChanged(object value)
        {
            isDirty = true;
        }

        public override void InternalDraw(SceneView sceneView)
        {
            var color = GUI.color;
            GUI.color = Color.black;
            isFoldout = EditorGUILayout.Foldout(isFoldout, sceneGUIContext.label);
            GUI.color = color;

            if (isFoldout)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("添加"))
                {
                    I element;
                    element = Activator.CreateInstance(typeof(I), default(T)) as I;
                    element.OnValueChanged += OnElementValueChanged;
                    elements.Add(element);
                    isDirty = true;
                }
                else if (GUILayout.Button("清空"))
                {
                    elements.Clear();
                    isDirty = true;
                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < elements.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    elements[i].sceneGUIContext.worldPos = sceneGUIContext.worldPos;
                    elements[i].InternalDraw(sceneView);
                    if (GUILayout.Button("删除"))
                    {
                        elements[i].OnValueChanged -= OnElementValueChanged;
                        elements.RemoveAt(i);
                        isDirty = true;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                if (isDirty)
                {
                    Value = new T[elements.Count];
                    for (int i = 0; i < elements.Count; i++)
                    {
                        Value[i] = elements[i].Value;
                    }
                    onValueChanged?.Invoke(Value);
                    isDirty = false;
                }
            }
        }

        protected override T[] UpdateValue()
        {
            return null;
        }
    }
    public class SceneGUITextField : SceneGUIDynamicElement<string>
    {
        public SceneGUITextField(string value) : base(value)
        {
        }

        protected override string UpdateValue()
        {
            return EditorGUILayout.TextField(Value);
        }
    }
    public class SceneGUIIntField : SceneGUIDynamicElement<int>
    {
        public SceneGUIIntField(int value) : base(value)
        {
        }

        protected override int UpdateValue()
        {
            return EditorGUILayout.IntField(Value);
        }
    }
    public class SceneGUIFloatField : SceneGUIDynamicElement<float>
    {
        public SceneGUIFloatField(float value) : base(value)
        {
        }

        protected override float UpdateValue()
        {
            return EditorGUILayout.FloatField(Value);
        }
    }
    public class SceneGUIToggle : SceneGUIDynamicElement<bool>
    {
        public SceneGUIToggle(bool value) : base(value)
        {

        }

        protected override bool UpdateValue()
        {
            return EditorGUILayout.Toggle(Value);
        }
    }
    public class SceneGUIEnumPopup : SceneGUIDynamicElement<Enum>
    {
        Type enumType;
        int index;
        public SceneGUIEnumPopup(Enum e) : base(e)
        {
            enumType = e.GetType();
            var array = Enum.GetValues(enumType);
            index = Array.IndexOf(array, e);
            // isDirty = true;
        }

        protected override Enum UpdateValue()
        {
            index = EditorGUILayout.Popup(index, Enum.GetNames(enumType));
            return enumType.GetEnumValues().GetValue(index) as Enum;
        }
    }
    public class SceneGUIEnumArrayField : SceneGUIDynamicElement<Enum[]>
    {
        Type enumType;
        List<SceneGUIEnumPopup> elements;
        public SceneGUIEnumArrayField(Type _enumType, Enum[] value) : base(value)
        {
            enumType = _enumType;
            elements = new List<SceneGUIEnumPopup>();
            for (int i = 0; i < value.Length; i++)
            {
                var element = new SceneGUIEnumPopup(value[i]);
                element.OnValueChanged += OnElementValueChanged;
                elements.Add(element);
            }
        }

        void OnElementValueChanged(object value)
        {
            isDirty = true;
        }

        public override void InternalDraw(SceneView sceneView)
        {
            var color = GUI.color;
            GUI.color = Color.black;
            isFoldout = EditorGUILayout.Foldout(isFoldout, sceneGUIContext.label);
            GUI.color = color;

            if (isFoldout)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("添加"))
                {
                    var element = new SceneGUIEnumPopup((Enum)enumType.GetEnumValues().GetValue(0));
                    element.OnValueChanged += OnElementValueChanged;
                    elements.Add(element);
                    isDirty = true;
                }
                else if (GUILayout.Button("清空"))
                {
                    elements.Clear();
                    isDirty = true;
                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < elements.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    elements[i].sceneGUIContext.worldPos = sceneGUIContext.worldPos;
                    elements[i].InternalDraw(sceneView);
                    if (GUILayout.Button("删除"))
                    {
                        elements[i].OnValueChanged -= OnElementValueChanged;
                        elements.RemoveAt(i);
                        isDirty = true;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                if (isDirty)
                {
                    Value = new Enum[elements.Count];
                    for (int i = 0; i < elements.Count; i++)
                    {
                        Value[i] = elements[i].Value;
                    }
                    onValueChanged?.Invoke(Value);
                    isDirty = false;
                }
            }
        }

        protected override Enum[] UpdateValue()
        {
            return null;
        }
    }
    public class SceneGUIVector2Field : SceneGUIDynamicElement<Vector2>
    {
        public SceneGUIVector2Field(Vector2 value) : base(value)
        {
        }

        protected override Vector2 UpdateValue()
        {
            return EditorGUILayout.Vector2Field(string.Empty, Value);
        }
    }
    public class SceneGUIVector2IntField : SceneGUIDynamicElement<Vector2Int>
    {
        public SceneGUIVector2IntField(Vector2Int value) : base(value)
        {
        }

        protected override Vector2Int UpdateValue()
        {
            return EditorGUILayout.Vector2IntField(string.Empty, Value);
        }
    }
    public class SceneGUIVector3Field : SceneGUIDynamicElement<Vector3>
    {
        public SceneGUIVector3Field(Vector3 value) : base(value)
        {
        }

        protected override Vector3 UpdateValue()
        {
            return EditorGUILayout.Vector3Field(string.Empty, Value);
        }
    }
    public class SceneGUIVector3IntField : SceneGUIDynamicElement<Vector3Int>
    {
        public SceneGUIVector3IntField(Vector3Int value) : base(value)
        {
        }

        protected override Vector3Int UpdateValue()
        {
            return EditorGUILayout.Vector3IntField(string.Empty, Value);
        }
    }
    public class SceneGUIObjectField : SceneGUIDynamicElement<UnityEngine.Object>
    {
        Type objType;
        public SceneGUIObjectField(Type _objType, UnityEngine.Object value) : base(value)
        {
            objType = _objType;
        }

        protected override UnityEngine.Object UpdateValue()
        {
            return EditorGUILayout.ObjectField(Value, objType, true);
        }
    }
    public class SceneGUIPolygonDrawer : SceneGUIDynamicElement<Vector2[]>
    {
        List<Vector2> points;
        public SceneGUIPolygonDrawer(Vector2[] value) : base(value)
        {
            points = new List<Vector2>(value);
        }

        protected override Vector2[] UpdateValue()
        {
            Handles.EndGUI();

            for (int i = 0; i < points.Count; i++)
            {
                var oldPoint = points[i];
                points[i] = Handles.PositionHandle(points[i], Quaternion.identity);
                var nextPoint = points[(i + 1) % points.Count];
                // 在相邻顶点间画线
                Handles.DrawLine(points[i], nextPoint);
                if (oldPoint != points[i])
                {
                    isDirty = true;
                }
            }

            Handles.BeginGUI();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("添加"))
            {
                var newPoint = (Vector2)sceneGUIContext.worldPos + new Vector2(2, -2);
                points.Add(newPoint);
                isDirty = true;
            }
            else if (GUILayout.Button("清空"))
            {
                points.Clear();
                isDirty = true;
            }

            GUILayout.EndHorizontal();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                point = EditorGUILayout.Vector2Field($"顶点{i})", point);
                if (point != points[i])
                {
                    points[i] = point;
                    isDirty = true;
                }
                if (GUILayout.Button("删除"))
                {
                    points.RemoveAt(i);
                    i--;
                    isDirty = true;
                }
            }

            GUILayout.EndVertical();

            if (isDirty)
            {
                Value = points.ToArray();
            }

            return Value;
        }
    }
    public class SceneGUIRectDrawer : SceneGUIDynamicElement<Vector2>
    {
        public SceneGUIRectDrawer(Vector2 value) : base(value)
        {
        }

        protected override Vector2 UpdateValue()
        {
            var newValue = EditorGUILayout.Vector2Field(string.Empty, Value);
            Handles.EndGUI();
            Handles.DrawWireCube(sceneGUIContext.worldPos, new Vector3(newValue.x, newValue.y, 0));
            Handles.BeginGUI();
            return newValue;
        }
    }
    public struct SceneGUIContext
    {
        public Vector3 worldPos;
        public Vector2 size;
        public string label;
        public Color color;
        public int fontSize;
    }
}
