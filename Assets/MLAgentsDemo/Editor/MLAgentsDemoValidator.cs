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
        private const string PlayMarkerPath = "Logs/mlagent2026-playmode-smoke.txt";
        private const int SmokeFrameCount = 60;

        private static int playFrames;
        private static bool hadErrorLog;
        private static bool previousEnterPlayModeOptionsEnabled;
        private static EnterPlayModeOptions previousEnterPlayModeOptions;

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

        public static void RunPlayModeSmokeAndQuit()
        {
            Directory.CreateDirectory("Logs");
            try
            {
                ValidateScene();
                previousEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
                previousEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
                EditorSettings.enterPlayModeOptionsEnabled = true;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

                playFrames = 0;
                hadErrorLog = false;
                Application.logMessageReceived += HandleLogMessage;
                EditorApplication.update += PlayModeSmokeUpdate;
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                File.WriteAllText(PlayMarkerPath, $"FAIL {DateTime.UtcNow:O} {exception}");
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void PlayModeSmokeUpdate()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            playFrames++;
            if (playFrames < SmokeFrameCount)
            {
                return;
            }

            try
            {
                if (hadErrorLog)
                {
                    throw new InvalidOperationException("Error log was emitted during Play Mode smoke test.");
                }

                ValidateLoadedSceneObjects();
                string message = $"OK {DateTime.UtcNow:O} frames={playFrames} scene={ScenePath}";
                File.WriteAllText(PlayMarkerPath, message);
                Debug.Log($"MLAgent2026 Play Mode smoke succeeded: {message}");
                RestorePlayModeOptions();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                File.WriteAllText(PlayMarkerPath, $"FAIL {DateTime.UtcNow:O} {exception}");
                Debug.LogException(exception);
                RestorePlayModeOptions();
                EditorApplication.Exit(1);
            }
            finally
            {
                Application.logMessageReceived -= HandleLogMessage;
                EditorApplication.update -= PlayModeSmokeUpdate;
            }
        }

        private static void HandleLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                hadErrorLog = true;
            }
        }

        private static void RestorePlayModeOptions()
        {
            EditorSettings.enterPlayModeOptionsEnabled = previousEnterPlayModeOptionsEnabled;
            EditorSettings.enterPlayModeOptions = previousEnterPlayModeOptions;
        }

        private static void ValidateScene()
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
        }

        private static void ValidateLoadedSceneObjects()
        {
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
        }
    }
}
#endif
