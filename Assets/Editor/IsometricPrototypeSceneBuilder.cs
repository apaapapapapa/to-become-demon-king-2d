using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace DemonKing.EditorTools
{
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
            Debug.Log($"Created isometric prototype scene: {ScenePath}");
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

            CreateTilemap("Ground", gridObject.transform, -100);
            CreateTilemap("Collision", gridObject.transform, -50, false);
            CreateTilemap("Props", gridObject.transform, 0);
            CreateTilemap("Foreground", gridObject.transform, 100);
        }

        private static void CreateTilemap(string name, Transform parent, int sortingOrder, bool visible = true)
        {
            GameObject tilemapObject = new(name);
            tilemapObject.transform.SetParent(parent, false);
            tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = visible;

            if (name == "Collision")
            {
                tilemapObject.AddComponent<TilemapCollider2D>();
            }
        }

        private static void CreateBootstrap()
        {
            GameObject bootstrapObject = new("Runtime Prototype Bootstrap");
            System.Type bootstrapType = System.Type.GetType("DemonKing.Field.FieldBootstrap, Assembly-CSharp");
            if (bootstrapType != null)
            {
                bootstrapObject.AddComponent(bootstrapType);
            }
            else
            {
                Debug.LogWarning("FieldBootstrap type was not found. Add it manually after scripts finish compiling.");
            }
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
