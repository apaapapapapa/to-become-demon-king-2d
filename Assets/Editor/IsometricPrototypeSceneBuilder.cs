using System.IO;
using DemonKing.Field;
using DemonKing.Presentation.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace DemonKing.EditorTools
{
    /// <summary>
    /// Prototypeシーンの基礎構造をEditor上で再生成する補助ツールです。
    /// 既存のPrototype.unityを作り直すため、手作業でシーンを編集した後に実行する場合は保存差分を確認してください。
    /// </summary>
    public static class IsometricPrototypeSceneBuilder
    {
        private const string SceneDirectory = "Assets/Scenes/Prototype";
        private const string ScenePath = SceneDirectory + "/Prototype.unity";

        [MenuItem("Demon King/Prototype/Create Isometric Scene")]
        public static void CreateIsometricScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureDirectory(SceneDirectory);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Prototype";

            CreateCamera();
            CreateDirectionalLight();
            CreateIsometricGrid();
            CreateBootstrap();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log($"アイソメトリック試作シーンを作成しました: {ScenePath}");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.backgroundColor = new Color(0.11f, 0.24f, 0.20f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateDirectionalLight()
        {
            // 最終的な2D Lighting構成は未確定のため、シーン生成時は簡易確認用のライトだけを配置します。
            GameObject lightObject = new("Global Light Placeholder");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.8f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateIsometricGrid()
        {
            GameObject gridObject = new("Grid");
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, 0.5f, 1f);

            CreateTilemap("Ground", gridObject.transform, SortingLayerNames.Ground, 0, true, TilemapRenderer.Mode.Chunk);
            CreateTilemap("Collision", gridObject.transform, SortingLayerNames.Ground, 0, false, TilemapRenderer.Mode.Chunk);
            CreateTilemap("Props", gridObject.transform, SortingLayerNames.World, 0, true, TilemapRenderer.Mode.Individual);
            CreateTilemap("Foreground", gridObject.transform, SortingLayerNames.Foreground, 0, true, TilemapRenderer.Mode.Chunk);
        }

        private static void CreateTilemap(
            string name,
            Transform parent,
            string sortingLayerName,
            int sortingOrder,
            bool visible,
            TilemapRenderer.Mode mode)
        {
            GameObject tilemapObject = new(name);
            tilemapObject.transform.SetParent(parent, false);
            tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
            renderer.mode = mode;
            renderer.enabled = visible;

            if (name == "Collision")
            {
                tilemapObject.AddComponent<TilemapCollider2D>();
            }
        }

        private static void CreateBootstrap()
        {
            GameObject bootstrapObject = new("Runtime Prototype Bootstrap");
            bootstrapObject.AddComponent<FieldBootstrap>();
        }

        private static void EnsureDirectory(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string current = "Assets";
            foreach (string part in assetPath.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, part);
                }

                current = next;
            }
        }
    }
}
