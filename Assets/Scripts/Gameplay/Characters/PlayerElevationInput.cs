using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// PlayerInputReaderの論理入力をCharacterElevationMotorへ接続します。
    /// Jumpボタンは地上ではJump、飛行中は上昇入力として使用します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(CharacterElevationMotor))]
    public sealed class PlayerElevationInput : MonoBehaviour
    {
        private PlayerInputReader inputReader;
        private CharacterElevationMotor elevationMotor;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            elevationMotor = GetComponent<CharacterElevationMotor>();
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (elevationMotor == null)
            {
                elevationMotor = GetComponent<CharacterElevationMotor>();
            }

            inputReader.JumpPressed += HandleJumpPressed;
            inputReader.FlightTogglePressed += HandleFlightTogglePressed;
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.JumpPressed -= HandleJumpPressed;
                inputReader.FlightTogglePressed -= HandleFlightTogglePressed;
            }

            elevationMotor?.SetFlightVerticalInput(0f);
        }

        private void Update()
        {
            if (elevationMotor != null && inputReader != null)
            {
                elevationMotor.SetFlightVerticalInput(inputReader.FlightElevationInput);
            }
        }

        private void HandleJumpPressed()
        {
            elevationMotor?.TryJump();
        }

        private void HandleFlightTogglePressed()
        {
            elevationMotor?.ToggleFlight();
        }
    }
}
