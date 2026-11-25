using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    public enum RuntimeInitializeLoadType { SubsystemRegistration, AfterAssembliesLoaded, BeforeSceneLoad, AfterSceneLoad }
    public enum Space { World, Self }
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }

    [AttributeUsage(AttributeTargets.Field)] public class HeaderAttribute : Attribute { public HeaderAttribute(string s) {} }
    [AttributeUsage(AttributeTargets.Field)] public class TooltipAttribute : Attribute { public TooltipAttribute(string s) {} }
    [AttributeUsage(AttributeTargets.Field)] public class HideInInspector : Attribute {}
    [AttributeUsage(AttributeTargets.Class)] public class RequireComponent : Attribute { public RequireComponent(Type t) {} }
    [AttributeUsage(AttributeTargets.Method)] public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) {} }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator *(Vector2 a, float k) { return new Vector2(a.x * k, a.y * k); }
        public override string ToString() { return "(" + x + ", " + y + ")"; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public float magnitude { get { return (float)Math.Sqrt(x*x + y*y + z*z); } }
        public static Vector3 zero { get { return new Vector3(0,0,0); } }
        public static Vector3 one { get { return new Vector3(1,1,1); } }
        public static Vector3 up { get { return new Vector3(0,1,0); } }
        public static Vector3 forward { get { return new Vector3(0,0,1); } }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x+b.x, a.y+b.y, a.z+b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x-b.x, a.y-b.y, a.z-b.z); }
        public static Vector3 operator *(Vector3 a, float k) { return new Vector3(a.x*k, a.y*k, a.z*k); }
        public static Vector3 operator *(float k, Vector3 a) { return a * k; }
        public static bool operator ==(Vector3 a, Vector3 b) { return a.x==b.x && a.y==b.y && a.z==b.z; }
        public static bool operator !=(Vector3 a, Vector3 b) { return !(a==b); }
        public override bool Equals(object o) { return o is Vector3 && this == (Vector3)o; }
        public override int GetHashCode() { return (int)(x*100) ^ (int)(y*100) ^ (int)(z*100); }
        public override string ToString() { return "(" + x + ", " + y + ", " + z + ")"; }
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x=x; this.y=y; this.z=z; this.w=w; }
        public static Quaternion identity { get { return new Quaternion(0,0,0,1); } }
        public static Quaternion Euler(float x, float y, float z) { return new Quaternion(x,y,z,1); }
        public override string ToString() { return "Q(" + x + "," + y + "," + z + "," + w + ")"; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a=1f) { this.r=r; this.g=g; this.b=b; this.a=a; }
        public static Color green { get { return new Color(0,1,0); } }
        public static Color red { get { return new Color(1,0,0); } }
        public static Color white { get { return new Color(1,1,1); } }
        public static bool operator ==(Color a, Color b) { return a.r==b.r && a.g==b.g && a.b==b.b && a.a==b.a; }
        public static bool operator !=(Color a, Color b) { return !(a==b); }
        public override bool Equals(object o) { return o is Color && this == (Color)o; }
        public override int GetHashCode() { return (int)(r*255) ^ (int)(g*255) ^ (int)(b*255); }
        public override string ToString() { return "C(" + r + "," + g + "," + b + ")"; }
    }

    public struct LayerMask
    {
        public int value;
        public LayerMask(int v) { value = v; }
        public static implicit operator LayerMask(int v) { return new LayerMask(v); }
    }

    public struct RaycastHit
    {
        public Vector3 point;
        public Collider collider;
        public float distance;
    }

    public static class Mathf
    {
        public static float Abs(float v) { return Math.Abs(v); }
        public static int Abs(int v) { return Math.Abs(v); }
        public static float Max(float a, float b) { return Math.Max(a,b); }
        public static int Max(int a, int b) { return Math.Max(a,b); }
        public static float Min(float a, float b) { return Math.Min(a,b); }
        public static float Pow(float f, float p) { return (float)Math.Pow(f,p); }
        public static float Sqrt(float f) { return (float)Math.Sqrt(f); }
        public static float Log(float f, float b) { return (float)(Math.Log(f)/Math.Log(b)); }
        public static float Clamp01(float v) { return v < 0 ? 0 : (v > 1 ? 1 : v); }
    }

    public static class Debug
    {
        public static readonly List<string> Messages = new List<string>();
        public static void Log(object m) { Messages.Add("LOG: " + m); }
        public static void LogWarning(object m) { Messages.Add("WARN: " + m); }
        public static void LogError(object m) { Messages.Add("ERROR: " + m); }
        public static void DrawLine(Vector3 a, Vector3 b, Color c) { }
        public static void DrawRay(Vector3 a, Vector3 d, Color c) { }
        public static void Clear() { Messages.Clear(); }
    }

    public static class Time
    {
        public static int frameCount = 100;
        public static float unscaledTime = 0f;
        public static float deltaTime = 1f / 60f;
    }

    public class Material
    {
        public string name;
        private readonly HashSet<string> props = new HashSet<string>();
        private readonly Dictionary<string, Color> colors = new Dictionary<string, Color>();
        public Material(string n="mat") { name = n; props.Add("_BaseColor"); props.Add("_Color"); }
        public Material(Shader s) : this(s != null ? s.name : "shader") {}
        public bool HasProperty(string p) { return props.Contains(p); }
        public void SetColor(string p, Color c) { colors[p] = c; }
        public Color GetColor(string p) { return colors.ContainsKey(p) ? colors[p] : new Color(0,0,0,0); }
    }

    public class Shader
    {
        public string name;
        public static Shader Find(string n) { return null; }
    }

    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 localScale = Vector3.one;
        public Transform parent;
        public Vector3 forward = Vector3.forward;
        public void Rotate(Vector3 axis, float degrees, Space space)
        {
            float rad = degrees * (float)Math.PI / 180f;
            if (space == Space.World && axis == Vector3.up)
                rotation = new Quaternion(rotation.x, rotation.y + rad, rotation.z, rotation.w);
            else
                rotation = new Quaternion(rotation.x + axis.x * rad, rotation.y + axis.y * rad, rotation.z + axis.z * rad, rotation.w);
        }
    }

    public class Component : Object
    {
        public GameObject gameObject { get; internal set; }
        public string name { get { return gameObject != null ? gameObject.name : ""; } set { if (gameObject != null) gameObject.name = value; } }
        public Transform transform { get { return gameObject != null ? gameObject.transform : null; } }
        public T GetComponent<T>() where T : Component { return gameObject == null ? null : gameObject.GetComponent<T>(); }
        public T GetComponentInParent<T>() where T : Component { return gameObject == null ? null : gameObject.GetComponentInParent<T>(); }
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component { return gameObject == null ? new T[0] : gameObject.GetComponentsInChildren<T>(includeInactive); }
    }

    public class Behaviour : Component
    {
        public bool enabled = true;
    }

    public class Coroutine { public string name; }

    public class MonoBehaviour : Behaviour
    {
        public int CoroutinesStarted;
        public int CoroutinesStopped;
        public bool isActiveAndEnabled { get { return gameObject != null && gameObject.activeSelf && enabled; } }
        public Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            CoroutinesStarted++;
            return new Coroutine { name = "stub" };
        }
        public void StopCoroutine(Coroutine c) { CoroutinesStopped++; }
    }

    public class Collider : Component { public bool enabled = true; }
    public class Renderer : Component
    {
        private Material[] _shared = new Material[0];
        public Material[] sharedMaterials { get { return _shared; } set { _shared = value ?? new Material[0]; } }
        public Material material { get { if (_shared.Length == 0) _shared = new Material[] { new Material("auto") }; return _shared[0]; } set { _shared = new Material[] { value }; } }
    }

    public class LineRenderer : Renderer
    {
        public int positionCount;
        public bool useWorldSpace;
        public float startWidth, endWidth;
        public Color startColor, endColor;
        public bool enabled;
        public Vector3[] positions = new Vector3[2];
        public void SetPosition(int i, Vector3 p) { if (i >= positions.Length) Array.Resize(ref positions, i+1); positions[i] = p; }
    }

    public static class Physics
    {
        public static Func<Vector3, Vector3, float, LayerMask, QueryTriggerInteraction, RaycastHit?> Handler;
        public static int Calls;
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, LayerMask mask, QueryTriggerInteraction qti)
        {
            Calls++;
            if (Handler != null)
            {
                var r = Handler(origin, direction, maxDistance, mask, qti);
                if (r.HasValue) { hit = r.Value; return true; }
            }
            hit = new RaycastHit();
            return false;
        }
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, LayerMask mask)
        { return Raycast(origin, direction, out hit, maxDistance, mask, QueryTriggerInteraction.UseGlobal); }
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance)
        { return Raycast(origin, direction, out hit, maxDistance, (LayerMask)0); }
    }

    public class GameObject : Object
    {
        public string name;
        public bool activeSelf = true;
        public bool Destroyed;
        public Transform transform;
        private readonly List<Component> components = new List<Component>();
        public static readonly List<GameObject> All = new List<GameObject>();

        public GameObject(string n = "go")
        {
            name = n;
            transform = new Transform { gameObject = this };
            components.Add(transform);
            All.Add(this);
        }
        public bool activeInHierarchy { get { return activeSelf && !Destroyed; } }
        public void SetActive(bool v) { activeSelf = v; }
        public T AddComponent<T>() where T : Component
        {
            T c = (T)Activator.CreateInstance(typeof(T));
            c.gameObject = this;
            components.Add(c);
            if (c.GetType().Name == "PodsvetkaNasazhdeniya") InvokeAwake(c);
            return c;
        }
        internal Component AddComponent(Type t)
        {
            Component c = (Component)Activator.CreateInstance(t);
            c.gameObject = this;
            components.Add(c);
            if (c.GetType().Name == "PodsvetkaNasazhdeniya") InvokeAwake(c);
            return c;
        }
        public T GetComponent<T>() where T : Component
        {
            foreach (var c in components) if (c is T) return (T)c;
            return null;
        }
        public T GetComponentInParent<T>() where T : Component
        {
            Transform t = transform;
            while (t != null)
            {
                var c = t.gameObject.GetComponent<T>();
                if (c != null) return c;
                t = t.parent;
            }
            return null;
        }
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component
        {
            var list = new List<T>();
            foreach (var c in components) if (c is T) list.Add((T)c);
            return list.ToArray();
        }
        internal void InvokeAwake(Component c)
        {
            var m = c.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m != null) m.Invoke(c, null);
        }
        internal Component[] Components() { return components.ToArray(); }
    }

    public class Object
    {
        public static GameObject Instantiate(GameObject original)
        {
            return Instantiate(original, original.transform.position, original.transform.rotation, original.transform.parent);
        }
        public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (original == null) return null;
            var go = new GameObject(original.name + "(Clone)");
            go.activeSelf = original.activeSelf;
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.transform.localScale = original.transform.localScale;
            go.transform.parent = parent;
            foreach (var src in original.Components())
            {
                if (src is Transform) continue;
                var dst = go.AddComponent(src.GetType());
                foreach (var f in src.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (f.DeclaringType == typeof(Component) || f.DeclaringType == typeof(Behaviour) || f.DeclaringType == typeof(MonoBehaviour)) continue;
                    try { f.SetValue(dst, f.GetValue(src)); } catch { }
                }
            }
            return go;
        }
        public static void Destroy(GameObject go) { if (go != null) go.Destroyed = true; }
        public static void Destroy(Object o) { var go = o as GameObject; if (go != null) Destroy(go); }
    }
}

namespace UnityEngine.UI
{
    public class UnityEvent
    {
        private readonly List<Action> listeners = new List<Action>();
        public int ListenerCount { get { return listeners.Count; } }
        public void AddListener(Action a) { listeners.Add(a); }
        public void RemoveListener(Action a) { listeners.Remove(a); }
        public void Invoke() { foreach (var a in listeners.ToArray()) a(); }
    }

    public class UnityEvent<T>
    {
        private readonly List<Action<T>> listeners = new List<Action<T>>();
        public int ListenerCount { get { return listeners.Count; } }
        public void AddListener(Action<T> a) { listeners.Add(a); }
        public void RemoveListener(Action<T> a) { listeners.Remove(a); }
        public void Invoke(T arg) { foreach (var a in listeners.ToArray()) a(arg); }
    }

    public class Selectable : UnityEngine.Component
    {
        public bool interactable = true;
    }

    public class Button : Selectable
    {
        public readonly UnityEvent onClick = new UnityEvent();
    }

    public class Slider : Selectable
    {
        public float minValue = 0f;
        public float maxValue = 1f;
        public bool wholeNumbers;
        private float _value;
        public readonly UnityEvent<float> onValueChanged = new UnityEvent<float>();
        public float value
        {
            get { return _value; }
            set { _value = value; onValueChanged.Invoke(_value); }
        }
        public void SetValueWithoutNotify(float v) { _value = v; }
    }

    public class Text : UnityEngine.Component
    {
        public string text = "";
    }
}

namespace UnityEngine.EventSystems
{
    public class EventSystem : UnityEngine.Component
    {
        public static EventSystem current;
        public UnityEngine.GameObject currentSelectedGameObject;
        public int SelectCalls;
        public void SetSelectedGameObject(UnityEngine.GameObject go) { currentSelectedGameObject = go; SelectCalls++; }
    }
}

namespace UnityEngine.InputSystem
{
    public class InputAction
    {
        public struct CallbackContext
        {
            public UnityEngine.Vector2 value;
            public T ReadValue<T>() { return (T)(object)value; }
        }
        public event Action<CallbackContext> performed;
        public event Action<CallbackContext> canceled;
        public UnityEngine.Vector2 StoredValue;
        public bool Enabled;
        public void Enable() { Enabled = true; }
        public void Disable() { Enabled = false; }
        public T ReadValue<T>() { return (T)(object)StoredValue; }
        public void Fire(UnityEngine.Vector2 v) { if (performed != null) performed(new CallbackContext { value = v }); }
        public void Cancel(UnityEngine.Vector2 v) { if (canceled != null) canceled(new CallbackContext { value = v }); }
        public int PerformedListeners { get { return performed == null ? 0 : performed.GetInvocationList().Length; } }
    }

    public class InputActionMap
    {
        public string name;
        public readonly Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();
        public InputActionMap(string n) { name = n; }
        public InputAction FindAction(string n) { return actions.ContainsKey(n) ? actions[n] : null; }
        public InputAction Add(string n) { var a = new InputAction(); actions[n] = a; return a; }
    }

    public class InputActionAsset
    {
        public readonly Dictionary<string, InputActionMap> maps = new Dictionary<string, InputActionMap>();
        public InputActionMap FindActionMap(string n) { return maps.ContainsKey(n) ? maps[n] : null; }
        public InputActionMap Add(string n) { var m = new InputActionMap(n); maps[n] = m; return m; }
    }
}
