using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// プレイヤーの論理入力をAbility実行要求へ変換します。効果処理やクールダウン判定は持ちません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(AbilityController))]
    public sealed class PlayerAbilityInput : MonoBehaviour
    {
        private const string DefaultBasicAttackAbilityId = "ability.basic_melee";
        private const string DefaultArtAbilityId = "ability.magic.fire_bolt";

        [SerializeField] private string basicAttackAbilityId = DefaultBasicAttackAbilityId;
        [SerializeField] private string artAbilityId = DefaultArtAbilityId;

        private PlayerInputReader inputReader;
        private AbilityController abilityController;
        private Vector2 facingDirection = Vector2.down;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            abilityController = GetComponent<AbilityController>();
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            inputReader.AttackPressed += HandleAttackPressed;
            inputReader.ArtPressed += HandleArtPressed;
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.AttackPressed -= HandleAttackPressed;
                inputReader.ArtPressed -= HandleArtPressed;
            }
        }

        private void Update()
        {
            Vector2 move = inputReader.Move;
            if (move.sqrMagnitude > 0.0001f)
            {
                facingDirection = move.normalized;
            }
        }

        private void HandleAttackPressed()
        {
            abilityController.TryUse(
                basicAttackAbilityId,
                gameObject,
                new AbilityExecutionInput(facingDirection));
        }

        private void HandleArtPressed()
        {
            abilityController.TryUse(
                artAbilityId,
                gameObject,
                new AbilityExecutionInput(facingDirection));
        }
    }
}
