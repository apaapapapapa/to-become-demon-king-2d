using System;
using DemonKing.Domain.Progression;
using DemonKing.Field.Composition;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Quests;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 既存呼び出し元とField Composition境界を接続する薄い互換アダプタです。
    /// Terrain、Collision、Architecture、Nature、Atmosphere、Gameplay、Pickup、Cameraの詳細構築は
    /// <see cref="PrototypeFieldComposer"/> とField Installer群が担当します。
    /// </summary>
    internal sealed class PrototypeWorldBuilder
    {
        private readonly PrototypeFieldDefinition fieldDefinition;
        private readonly FieldEntryPoint entryPoint;
        private readonly DialogueLog dialogueLog;
        private readonly CharacterProgressionState progressionState;
        private readonly ProgressionGrantConsumptionState grantConsumptionState;
        private readonly QuestProgressionService sharedQuestProgressionService;
        private readonly IPrototypeFieldTransitionRequester transitionRequester;

        public PrototypeWorldBuilder(
            PrototypeFieldDefinition fieldDefinition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState = null,
            ProgressionGrantConsumptionState grantConsumptionState = null)
            : this(
                fieldDefinition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService: null,
                transitionRequester: null)
        {
        }

        public PrototypeWorldBuilder(
            PrototypeFieldDefinition fieldDefinition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            QuestProgressionService sharedQuestProgressionService,
            IPrototypeFieldTransitionRequester transitionRequester)
        {
            this.fieldDefinition = fieldDefinition ??
                throw new ArgumentNullException(nameof(fieldDefinition));
            this.entryPoint = entryPoint;
            this.dialogueLog = dialogueLog ?? throw new ArgumentNullException(nameof(dialogueLog));
            this.progressionState = progressionState;
            this.grantConsumptionState = grantConsumptionState ??
                ProgressionGrantConsumptionState.CreateInitial();
            this.sharedQuestProgressionService = sharedQuestProgressionService;
            this.transitionRequester = transitionRequester;
        }

        /// <summary>
        /// 既存テスト・呼び出し元向けの互換Constructorです。
        /// 新しいFieldは<see cref="PrototypeFieldDefinition"/>をCatalogへ登録して構築します。
        /// </summary>
        public PrototypeWorldBuilder(
            Vector3 playerSpawnPosition,
            int playableTileRadius,
            PrototypeProjectAssets projectAssets,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState = null,
            ProgressionGrantConsumptionState grantConsumptionState = null)
        {
            fieldDefinition = PrototypeFieldDefinition.CreateLegacy(
                playerSpawnPosition,
                playableTileRadius,
                projectAssets);
            entryPoint = fieldDefinition.ResolveEntryPoint(fieldDefinition.ConfiguredDefaultEntryPointId);
            this.dialogueLog = dialogueLog ?? throw new ArgumentNullException(nameof(dialogueLog));
            this.progressionState = progressionState;
            this.grantConsumptionState = grantConsumptionState ??
                ProgressionGrantConsumptionState.CreateInitial();
            sharedQuestProgressionService = null;
            transitionRequester = null;
        }

        public PrototypeWorldBuildResult Build()
        {
            return new PrototypeFieldComposer().Compose(
                fieldDefinition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService,
                transitionRequester);
        }
    }
}
