#if UNITY_EDITOR
using System.IO;
using MLAgent2026.Demo;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MLAgent2026.Demo.Editor
{
    public static class MLAgentsDemoBootstrap
    {
        private const string ScenePath = "Assets/MLAgentsDemo/RollerTraining.unity";

        [InitializeOnLoadMethod]
        private static void CreateSceneAfterImport()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isCompiling && !EditorApplication.isUpdating && !File.Exists(ScenePath))
                {
                    CreateRollerTrainingScene();
                }
            };
        }

        [MenuItem("MLAgent2026/Create Roller Training Scene")]
        public static void CreateRollerTrainingScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject area = new GameObject("Training Area");
            area.transform.position = Vector3.zero;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(area.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(10f, 0.2f, 10f);
            floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Floor_Mat", new Color(0.18f, 0.22f, 0.25f));

            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Target";
            target.transform.SetParent(area.transform);
            target.transform.localPosition = new Vector3(3f, 0.6f, 3f);
            target.transform.localScale = Vector3.one;
            target.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Target_Mat", new Color(0.95f, 0.65f, 0.18f));

            GameObject agent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            agent.name = "RollerAgent";
            agent.transform.SetParent(area.transform);
            agent.transform.localPosition = new Vector3(-3f, 0.6f, -3f);
            agent.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Agent_Mat", new Color(0.1f, 0.55f, 0.95f));

            Rigidbody rigidbody = agent.AddComponent<Rigidbody>();
            rigidbody.mass = 1f;

            RollerAgent rollerAgent = agent.AddComponent<RollerAgent>();
            SerializedObject agentObject = new SerializedObject(rollerAgent);
            agentObject.FindProperty("target").objectReferenceValue = target.transform;
            agentObject.FindProperty("area").objectReferenceValue = area.transform;
            agentObject.ApplyModifiedPropertiesWithoutUndo();

            BehaviorParameters behavior = agent.AddComponent<BehaviorParameters>();
            behavior.BehaviorName = "RollerAgent";
            behavior.BehaviorType = BehaviorType.Default;
            behavior.BrainParameters.VectorObservationSize = 6;
            behavior.BrainParameters.NumStackedVectorObservations = 1;
            behavior.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(2);

            Unity.MLAgents.DecisionRequester requester = agent.AddComponent<Unity.MLAgents.DecisionRequester>();
            requester.DecisionPeriod = 5;
            requester.TakeActionsBetweenDecisions = true;

            GameObject light = new GameObject("Directional Light");
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject camera = new GameObject("Main Camera");
            Camera cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 8f, -8f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"MLAgent2026 demo scene created: {ScenePath}");
        }

        private static Material CreateMaterial(string name, Color color)
        {
            const string folder = "Assets/MLAgentsDemo/Materials";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/MLAgentsDemo", "Materials");
            }

            string path = $"{folder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif
