using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// プレイヤーの論理Ability Slot入力をRuntime LoadoutでAbility IDへ解決し、AbilityControllerへ委譲します。
    /// 効果処理、クールダウン判定、個別Ability IDは保持しません。
    /// AbilityLoadoutControllerはPlayer Runtime Compositionで注入されるため、利用時に依存を再解決します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(AbilityController))]
    public sealed class PlayerAbilityInput : MonoBehaviour
    {
        private PlayerInputReader inputReader;
        private AbilityController abilityController;
        private AbilityLoadoutController loadoutController;
        private Vector2 facingDirection = Vector2.down;

        private void Awake()
        {
            ResolveDependencies();
        }

        private void OnEnable()
        {
            ResolveDependencies();
            if (inputReader != null)
            {
                inputReader.AbilitySlotPressed += HandleAbilitySlotPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.AbilitySlotPressed -= HandleAbilitySlotPressed;
            }
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            Vector2 move = inputReader.Move;
            if (move.sqrMagnitude > 0.0001f)
            {
                facingDirection = move.normalized;
            }
        }

        public bool TryUseSlot(AbilitySlot slot)
        {
            ResolveDependencies();
            if (abilityController == null ||
                loadoutController == null ||
                !loadoutController.TryResolve(slot, out string abilityId))
            {
                return false;
            }

            abilityController.TryUse(
                abilityId,
                gameObject,
                new AbilityExecutionInput(facingDirection));
            return true;
        }

        private void HandleAbilitySlotPressed(AbilitySlot slot)
        {
            TryUseSlot(slot);
        }

        private void ResolveDependencies()
        {
            inputReader ??= GetComponent<PlayerInputReader>();
            abilityController ??= GetComponent<AbilityController>();
            loadoutController ??= GetComponent<AbilityLoadoutController>();
        }
    }
}
