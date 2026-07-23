using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Runtime生成型Prototype FieldのScene寿命を扱う境界です。
    /// Fieldの識別はStable Field ID、SceneはField DefinitionのSceneNameから生成します。
    /// Build IndexやScene内Object参照をGame Session Stateへ保持しません。
    /// </summary>
    internal static class PrototypeFieldSceneRuntime
    {
        public static Scene Activate(string sceneName, out Scene previousScene)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("Field Scene名は必須です。", nameof(sceneName));
            }

            previousScene = SceneManager.GetActiveScene();
            if (previousScene.IsValid() &&
                previousScene.isLoaded &&
                string.Equals(previousScene.name, sceneName, StringComparison.Ordinal))
            {
                ResolveOrCreateCamera();
                return previousScene;
            }

            Scene targetScene = SceneManager.GetSceneByName(sceneName);
            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                targetScene = SceneManager.CreateScene(sceneName);
            }

            if (!SceneManager.SetActiveScene(targetScene))
            {
                throw new InvalidOperationException($"Field SceneをActiveにできませんでした: {sceneName}");
            }

            ResolveOrCreateCamera();
            return targetScene;
        }

        public static AsyncOperation UnloadPrevious(Scene previousScene, Scene activeScene)
        {
            if (!previousScene.IsValid() ||
                !previousScene.isLoaded ||
                !activeScene.IsValid() ||
                previousScene.handle == activeScene.handle)
            {
                return null;
            }

            return SceneManager.UnloadSceneAsync(previousScene);
        }

        public static Camera ResolveOrCreateCamera()
        {
            Camera camera = FindInActiveScene<Camera>();
            if (camera != null)
            {
                EnsureMainCameraTag(camera.gameObject);
                PrototypeSceneConfigurator.Configure(camera);
                return camera;
            }

            GameObject cameraObject = new("Main Camera");
            EnsureMainCameraTag(cameraObject);
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            PrototypeSceneConfigurator.Configure(camera);
            return camera;
        }

        public static T FindInActiveScene<T>()
            where T : Component
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                return null;
            }

            foreach (GameObject root in activeScene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(includeInactive: true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void EnsureMainCameraTag(GameObject cameraObject)
        {
            if (cameraObject != null && !cameraObject.CompareTag("MainCamera"))
            {
                cameraObject.tag = "MainCamera";
            }
        }
    }
}
