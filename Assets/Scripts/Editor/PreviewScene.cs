using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;
using System.Linq;
using System.IO;

public class PreviewScene : IDisposable
{
    public Scene Scene { get; private set; }
    public Camera Camera { get; private set; }
    public RenderTexture RenderTexture { get; private set; }
    public Vector2Int RenderTextureSize { get; set; } = new Vector2Int(1024, 1024);

    private SavedRenderSettings _savedRenderSettings;
    //プレビューシーンにあるゲームオブジェクトのリスト
    private List<GameObject> _gameObjects = new List<GameObject>();
    //初期化
    private bool _isDidlnitialize = false;

    /// <summary>
    /// 例外処理
    /// </summary>
    /// <param name="environmentSceneAssetPath"></param>
    public PreviewScene(string environmentSceneAssetPath = null)
    {
        try
        {
            Scene = EditorSceneManager.NewPreviewScene();

            if (environmentSceneAssetPath != null)
            {
                _savedRenderSettings = SavedRenderSettings.Create(environmentSceneAssetPath);
                CopyRootGameObjects(environmentSceneAssetPath);
            }

            // 未使用のカメラを無効にする
            IEnumerable<Camera> oldCameras = Scene
                .GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<Camera>());
            foreach (Camera oldCamera in oldCameras)
            {
                oldCamera.enabled = false;
            }

            GameObject cameraGO = new GameObject("Preview Scene Camera", typeof(Camera));
            cameraGO.transform.position = new Vector3(0, 0, -10);
            AddGameObject(cameraGO);
            Camera = cameraGO.GetComponent<Camera>();
            Camera.cameraType = CameraType.Preview;
            Camera.forceIntoRenderTexture = true;
            Camera.scene = Scene;
            Camera.enabled = false; // GameView に影響を与えないように非アクティブ化

            bool hasDirectionalLight = Scene
                .GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<Light>())
                .Any(x => x.type == LightType.Directional);
            if (!hasDirectionalLight)
            {
                GameObject lightGO = new GameObject("Directional Light", typeof(Light));
                AddGameObject(lightGO);
                lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
                Light light = lightGO.GetComponent<Light>();
                light.type = LightType.Directional;
            }

            _isDidlnitialize = true;
        }
        catch (Exception e)
        {
            Dispose();
            _isDidlnitialize = false;
            Debug.Log("例外");
            throw e;
        }
    }
    /// <summary>
    /// レンダー
    /// </summary>
    /// <param name="useScriptableRenderPipeline"></param>
    public void Render(bool useScriptableRenderPipeline = false)
    {
        if (!_isDidlnitialize)
        {
            return;
        }
        // レンダリング設定を変更する
        if (_savedRenderSettings != null && Unsupported.SetOverrideLightingSettings(Scene))
        {
            _savedRenderSettings.Apply();
        }

        // 必要に応じて RenderTexture を作成する
        if (!RenderTexture || RenderTexture.width != RenderTextureSize.x || RenderTexture.height != RenderTextureSize.y)
        {
            if (RenderTexture)
            {
                Object.DestroyImmediate(RenderTexture);
                RenderTexture = null;
            }

            GraphicsFormat format = Camera.allowHDR ? GraphicsFormat.R16G16B16A16_SFloat : GraphicsFormat.R8G8B8A8_UNorm;
            RenderTexture = new RenderTexture(RenderTextureSize.x, RenderTextureSize.y, 32, format);
        }
        Camera.targetTexture = RenderTexture;

        // Render
        bool oldAllowPipes = Unsupported.useScriptableRenderPipeline;
        Unsupported.useScriptableRenderPipeline = useScriptableRenderPipeline;
        Camera.Render();
        Unsupported.useScriptableRenderPipeline = oldAllowPipes;

        Camera.targetTexture = null;
        // Restore RenderSettings
        if (_savedRenderSettings != null)
        {
            Unsupported.RestoreOverrideLightingSettings();
        }
    }
    /// <summary>
    /// プレビューシーンを廃棄
    /// </summary>
    public void Dispose()
    {
        Camera.targetTexture = null;

        if (RenderTexture != null)
        {
            Object.DestroyImmediate(RenderTexture);
            RenderTexture = null;
        }

        foreach (GameObject go in _gameObjects)
        {
            Object.DestroyImmediate(go);
        }
        _gameObjects.Clear();

        EditorSceneManager.ClosePreviewScene(Scene);
    }
    /// <summary>
    /// プレビューシーンにゲームオブジェクトを追加
    /// </summary>
    public void AddGameObject(GameObject go)
    {
        if (_gameObjects.Contains(go))
        {
            return;
        }
        SceneManager.MoveGameObjectToScene(go, Scene);
        _gameObjects.Add(go);
    }

    /// <summary>
    /// プレハブインスタンスをプレビューシーンに追加する
    /// </summary>
    public GameObject InstantiatePrefab(GameObject prefab)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Scene);
        _gameObjects.Add(instance);
        return instance;
    }

    /// <summary>
    /// ソース シーンのすべてのルートゲームオブジェクトをプレビューシーンにコピーする
    /// </summary>
    private void CopyRootGameObjects(string sourceSceneAssetPath)
    {
        GameObject[] rootGameObjects = null;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            if (scene.path == sourceSceneAssetPath)
            {
                rootGameObjects = scene.GetRootGameObjects();
                break;
            }
        }
        if (rootGameObjects == null)
        {
            Scene scene = EditorSceneManager.OpenScene(sourceSceneAssetPath, OpenSceneMode.Additive);
            rootGameObjects = scene.GetRootGameObjects();
            EditorSceneManager.CloseScene(scene, true);
        }
        if (rootGameObjects != null)
        {
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                AddGameObject(GameObject.Instantiate(rootGameObject));
            }
        }
    }

    public class SavedRenderSettings
    {
        private Material _skybox;
        private Light _sun;
        private AmbientMode _ambientMode;
        private SphericalHarmonicsL2 _ambientProbe;
        private Color _ambientSkyColor;
        private Color _ambientEquatorColor;
        private Color _ambientGroundColor;
        private Color _ambientLight;
        private float _ambientIntensity;
        private DefaultReflectionMode _defaultReflectionMode;
        private int _defaultReflectionResolution;
        private Cubemap _customReflection;
        private float _reflectionIntensity;
        private int _reflectionBounces;
        private Color _substractiveShadowColor;
        private bool _fog;
        private FogMode _fogMode;
        private Color _fogColor;
        private float _fogDensity;
        private float _fogStartDistance;
        private float _fogEndDistance;

        public static SavedRenderSettings Create(string scenePath)
        {
            SavedRenderSettings result = new SavedRenderSettings();
            Scene? oldActiveScene = null;
            if (scenePath != EditorSceneManager.GetActiveScene().path)
            {
                oldActiveScene = EditorSceneManager.GetActiveScene();
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                EditorSceneManager.SetActiveScene(scene);
            }

            result._skybox = RenderSettings.skybox;
            result._sun = RenderSettings.sun;
            result._ambientMode = RenderSettings.ambientMode;
            result._ambientProbe = RenderSettings.ambientProbe;
            result._ambientSkyColor = RenderSettings.ambientSkyColor;
            result._ambientEquatorColor = RenderSettings.ambientEquatorColor;
            result._ambientGroundColor = RenderSettings.ambientGroundColor;
            result._ambientLight = RenderSettings.ambientLight;
            result._ambientIntensity = RenderSettings.ambientIntensity;
            result._defaultReflectionMode = RenderSettings.defaultReflectionMode;
            result._defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
            result._customReflection = RenderSettings.customReflection;
            // If defaultReflectionMode is Skybox, search and set the created cube map
            if (result._defaultReflectionMode == DefaultReflectionMode.Skybox && Lightmapping.lightingDataAsset != null)
            {
                string lightingDataAssetPath = AssetDatabase.GetAssetPath(Lightmapping.lightingDataAsset);
                string lightingDataAssetDirectoryName = Path.GetDirectoryName(lightingDataAssetPath);
                string environmentProbeAssetPath = Directory
                    .GetFiles(lightingDataAssetDirectoryName)
                    .FirstOrDefault(x => x.EndsWith(".exr"));
                if (!string.IsNullOrEmpty(environmentProbeAssetPath))
                {
                    result._defaultReflectionMode = DefaultReflectionMode.Custom;
                    result._customReflection = AssetDatabase.LoadAssetAtPath<Cubemap>(environmentProbeAssetPath.Replace("\\", "/"));
                }
            }
            result._reflectionIntensity = RenderSettings.reflectionIntensity;
            result._reflectionBounces = RenderSettings.reflectionBounces;
            result._substractiveShadowColor = RenderSettings.subtractiveShadowColor;
            result._fog = RenderSettings.fog;
            result._fogMode = RenderSettings.fogMode;
            result._fogColor = RenderSettings.fogColor;
            result._fogDensity = RenderSettings.fogDensity;
            result._fogStartDistance = RenderSettings.fogStartDistance;
            result._fogEndDistance = RenderSettings.fogEndDistance;

            if (oldActiveScene.HasValue)
            {
                Scene scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.SetActiveScene(oldActiveScene.Value);
                EditorSceneManager.CloseScene(scene, true);
            }
            return result;
        }

        public void Apply()
        {
            RenderSettings.skybox = _skybox;
            RenderSettings.sun = _sun;
            RenderSettings.ambientMode = _ambientMode;
            RenderSettings.ambientProbe = _ambientProbe;
            RenderSettings.ambientSkyColor = _ambientSkyColor;
            RenderSettings.ambientEquatorColor = _ambientEquatorColor;
            RenderSettings.ambientGroundColor = _ambientGroundColor;
            RenderSettings.ambientLight = _ambientLight;
            RenderSettings.ambientIntensity = _ambientIntensity;
            RenderSettings.defaultReflectionMode = _defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = _defaultReflectionResolution;
            RenderSettings.customReflection = _customReflection;
            RenderSettings.reflectionIntensity = _reflectionIntensity;
            RenderSettings.reflectionBounces = _reflectionBounces;
            RenderSettings.subtractiveShadowColor = _substractiveShadowColor;
            RenderSettings.fog = _fog;
            RenderSettings.fogMode = _fogMode;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogDensity = _fogDensity;
            RenderSettings.fogStartDistance = _fogStartDistance;
            RenderSettings.fogEndDistance = _fogEndDistance;
        }
    }
}
