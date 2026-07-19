using System;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Characters
{
    /// <summary>
    /// 不変なキャラクター定義と、プレイ中に変化する成長状態を組み合わせます。
    /// </summary>
    public sealed class CharacterRuntimeContext
    {
        public CharacterRuntimeContext(
            CharacterDefinition definition,
            CharacterProgressionState progressionState)
        {
            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
            ProgressionState = progressionState != null
                ? progressionState
                : throw new ArgumentNullException(nameof(progressionState));

            if (!string.Equals(
                    Definition.CharacterId,
                    ProgressionState.CharacterDefinitionId,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "CharacterDefinitionと成長状態のIDが一致していません。",
                    nameof(progressionState));
            }
        }

        public CharacterDefinition Definition { get; }
        public CharacterProgressionState ProgressionState { get; }
    }

    /// <summary>
    /// Unity上のキャラクターから実行時コンテキストを参照するための薄い保持コンポーネントです。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterRuntimeContextHost : MonoBehaviour
    {
        public CharacterRuntimeContext Context { get; private set; }
        public bool IsInitialized => Context != null;

        public void Initialize(CharacterRuntimeContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
