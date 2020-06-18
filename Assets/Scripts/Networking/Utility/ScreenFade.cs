using System.Collections;
using UnityEngine;

namespace Networking
{
    public class ScreenFade : MonoBehaviour
    {
        public static ScreenFade Instance { get; private set; }
        public int renderQueue = 5000;

#pragma warning disable 0649
        [SerializeField] private Color color = new Color(0, 0, 0, 0);
        [SerializeField] [Range(0f, 1f)] private float alpha = 0;
#pragma warning restore 0649

        private float Alpha
        {
            get => this.alpha;
            set
            {
                this.alpha = value;
                UpdateMaterialAlpha();
            }
        }

        private Material material;

        private const string ShaderName = "Oculus/Unlit Transparent Color";
        private int materialColorID;
        private Coroutine fadeCoroutine;
        private MeshFilter fadeMesh;
        private MeshRenderer fadeRenderer;

        private void Awake()
        {
            // create the fade material
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"Screen fading disabled. Required shader not found in the project: {ShaderName}");
                UnityEngine.Object.Destroy(this);
                return;
            }

            this.material = new Material(shader);
            this.fadeMesh = gameObject.AddComponent<MeshFilter>();
            this.fadeRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            this.fadeMesh.mesh = mesh;
            this.fadeRenderer.material = this.material;

            Vector3[] vertices = new Vector3[4];

            float width = 2f;
            float height = 2f;
            float depth = 1f;

            vertices[0] = new Vector3(-width, -height, depth);
            vertices[1] = new Vector3(width, -height, depth);
            vertices[2] = new Vector3(-width, height, depth);
            vertices[3] = new Vector3(width, height, depth);

            mesh.vertices = vertices;

            int[] tri = new int[6];

            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;

            tri[3] = 2;
            tri[4] = 3;
            tri[5] = 1;

            mesh.triangles = tri;

            Vector3[] normals = new Vector3[4];

            normals[0] = -Vector3.forward;
            normals[1] = -Vector3.forward;
            normals[2] = -Vector3.forward;
            normals[3] = -Vector3.forward;

            mesh.normals = normals;

            Vector2[] uv = new Vector2[4];

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            mesh.uv = uv;

            this.materialColorID = Shader.PropertyToID("_Color");
            this.material.SetColor(materialColorID, this.color);
            this.material.renderQueue = renderQueue;

            //FadeIn(1);
            Instance = this;

            UpdateMaterialAlpha();
        }

        void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private IEnumerator InternalFadeInAsync(float duration, System.Action whenDone = null)
        {
            if (this.Alpha < 0.01)
            {
                this.Alpha = 0f;
                whenDone?.Invoke();
                yield break;
            }

            float startAlpha = this.Alpha;
            float time = 0;
            while (time < duration)
            {
                this.Alpha = startAlpha - (startAlpha * time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            this.Alpha = 0f;

            this.fadeCoroutine = null;
            whenDone?.Invoke();
        }

        private IEnumerator InternalFadeOutAsync(float duration, float fadeLevel = 1f, System.Action whenDone = null)
        {
            if (this.Alpha > fadeLevel - 0.01)
            {
                this.Alpha = fadeLevel;
                whenDone?.Invoke();
                yield break;
            }

            float startAlpha = this.Alpha;
            float time = 0;
            while (time < duration)
            {
                this.Alpha = startAlpha + ((fadeLevel - startAlpha) * time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            this.Alpha = fadeLevel;

            this.fadeCoroutine = null;
            whenDone?.Invoke();
        }

        private void UpdateMaterialAlpha()
        {
            this.color.a = this.Alpha;
            this.material.SetColor(this.materialColorID, this.color);

            if (this.Alpha <= 0 && this.fadeRenderer.enabled)
            {
                this.fadeRenderer.enabled = false;
            }
            else if (this.Alpha >= 0 && !this.fadeRenderer.enabled)
            {
                this.fadeRenderer.enabled = true;
            }
        }

        public static bool IsFading => Instance?.fadeCoroutine != null;

        public static void FadeIn(float duration = 1, System.Action whenDone = null)
        {
            if (Instance == null)
            {
                whenDone?.Invoke();
                return;
            }

            if (Instance.fadeCoroutine != null)
            {
                Instance.StopCoroutine(Instance.fadeCoroutine);
            }
            Instance.fadeCoroutine = Instance.StartCoroutine(Instance.InternalFadeInAsync(Mathf.Max(duration, 0.01f), whenDone));
        }

        public static void FadeOut(float duration = 1, float fadeLevel = 1f, System.Action whenDone = null)
        {
            if (Instance == null)
            {
                whenDone?.Invoke();
                return;
            }

            if (Instance.fadeCoroutine != null)
            {
                Instance.StopCoroutine(Instance.fadeCoroutine);
            }
            Instance.fadeCoroutine = Instance.StartCoroutine(Instance.InternalFadeOutAsync(Mathf.Max(duration, 0.01f), fadeLevel: fadeLevel, whenDone: whenDone));
        }
    }
}
