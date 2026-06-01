#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MLAgent2026.Demo.Editor
{
    public static class MLAgentsDemoValidator
    {
        private const string ScenePath = "Assets/MLAgentsDemo/RollerTraining.unity";
        private const string MarkerPath = "Logs/mlagent2026-validation.txt";

        public static void RefreshOpenSceneAndQuit()
        {
            Directory.CreateDirectory("Logs");
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                if (!File.Exists(ScenePath))
                {
                    throw new FileNotFoundException($"Scene file missing: {ScenePath}");
                }

                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ScenePath);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    throw new InvalidOperationException($"Scene did not load: {ScenePath}");
                }

                GameObject agent = GameObject.Find("RollerAgent");
                if (agent == null)
                {
                    throw new InvalidOperationException("RollerAgent object not found.");
                }

                if (!agent.TryGetComponent<RollerAgent>(out _))
                {
                    throw new InvalidOperationException("RollerAgent component missing.");
                }

                if (!agent.TryGetComponent<Unity.MLAgents.Policies.BehaviorParameters>(out var behavior))
                {
                    throw new InvalidOperationException("BehaviorParameters component missing.");
                }

                if (behavior.BehaviorName != "RollerAgent")
                {
                    throw new InvalidOperationException($"Unexpected behavior name: {behavior.BehaviorName}");
                }

                if (behavior.BrainParameters.VectorObservationSize != 6)
                {
                    throw new InvalidOperationException($"Unexpected observation size: {behavior.BrainParameters.VectorObservationSize}");
                }

                if (behavior.BrainParameters.ActionSpec.NumContinuousActions != 2)
                {
                    throw new InvalidOperationException($"Unexpected continuous action count: {behavior.BrainParameters.ActionSpec.NumContinuousActions}");
                }

                if (!agent.TryGetComponent<Unity.MLAgents.DecisionRequester>(out var requester))
                {
                    throw new InvalidOperationException("DecisionRequester component missing.");
                }

                if (requester.DecisionPeriod != 5)
                {
                    throw new InvalidOperationException($"Unexpected decision period: {requester.DecisionPeriod}");
                }

                string message = $"OK {DateTime.UtcNow:O} scene={ScenePath} behavior={behavior.BehaviorName}";
                File.WriteAllText(MarkerPath, message);
                Debug.Log($"MLAgent2026 validation succeeded: {message}");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                File.WriteAllText(MarkerPath, $"FAIL {DateTime.UtcNow:O} {exception}");
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
#endif
