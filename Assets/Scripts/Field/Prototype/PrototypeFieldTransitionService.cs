using System;
using System.Collections;
using DemonKing.Core.Input;
using DemonKing.Field.Composition;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Stable FieldLocationによる遷移要求を、Scene切替・Field Runtime再構築・Application再バインドへ変換します。
    /// Session StateはPrototypeGameSessionが保持し、このService自身はGameplay進捗を所有しません。
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class PrototypeFieldTransitionService : MonoBehaviour, IPrototypeFieldTransitionRequester
    {
        private PrototypeGameSession gameSession;
        private PrototypeApplicationFieldBinder fieldBinder;
        private bool initialized;

        public bool IsTransitioning { get; private set; }

        public void Initialize(
            PrototypeGameSession session,
            PrototypeApplicationFieldBinder binder)
        {
            gameSession = session ?? throw new ArgumentNullException(nameof(session));
            fieldBinder = binder != null
                ? binder
                : throw new ArgumentNullException(nameof(binder));
            initialized = true;
        }

        public bool TryTransition(FieldLocation destination)
        {
            if (!initialized ||
                IsTransitioning ||
                !destination.IsValid ||
                fieldBinder.HasOpenModal ||
                (gameSession.IsStarted && destination.Equals(gameSession.CurrentFieldLocation)))
            {
                return false;
            }

            if (!gameSession.TryResolveField(
                    destination,
                    out PrototypeFieldDefinition targetDefinition,
                    out _))
            {
                Debug.LogWarning($"遷移先Field Locationを解決できません: {destination}", this);
                return false;
            }

            StartCoroutine(TransitionRoutine(destination, targetDefinition));
            return true;
        }

        private IEnumerator TransitionRoutine(
            FieldLocation destination,
            PrototypeFieldDefinition targetDefinition)
        {
            IsTransitioning = true;
            PlayerInputReader previousInputReader = fieldBinder.CurrentInputReader;

            try
            {
                previousInputReader?.DisableInput();
                gameSession.PrepareForFieldTransition();

                Scene targetScene = PrototypeFieldSceneRuntime.Activate(
                    targetDefinition.SceneName,
                    out Scene previousScene);

                // CreateSceneは同期ですが、Active Scene切替後のLifecycle境界を1 frame明示します。
                yield return null;

                PrototypeGameSessionResult sessionResult = gameSession.EnterField(destination);
                fieldBinder.Bind(sessionResult);

                AsyncOperation unloadOperation =
                    PrototypeFieldSceneRuntime.UnloadPrevious(previousScene, targetScene);
                if (unloadOperation != null)
                {
                    yield return unloadOperation;
                }
            }
            finally
            {
                IsTransitioning = false;
                if (fieldBinder.CurrentInputReader != null)
                {
                    fieldBinder.CurrentInputReader.EnableGameplayInput();
                }
                else
                {
                    previousInputReader?.EnableGameplayInput();
                }
            }
        }
    }
}
