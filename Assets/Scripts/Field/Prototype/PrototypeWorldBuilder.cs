using System;
using DemonKing.Domain.Progression;
using DemonKing.Field.Composition;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    internal sealed class PrototypeWorldBuilder
    {
        private readonly PrototypeFieldDefinition fieldDefinition;
        private readonly FieldEntryPoint entryPoint;
        private readonly DialogueLog dialogueLog;
        private readonly CharacterProgressionState progressionState;
        private readonly ProgressionGrantConsumptionState grantConsumptionState;
        private readonly QuestProgressionService sharedQuestProgressionService;
        private readonly GameplayEventHub sharedGameplayEventHub;
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
                sharedGameplayEventHub: null,
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
            : this(
                fieldDefinition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService,
                sharedGameplayEventHub: null,
                transitionRequester)
        {
        }

        public PrototypeWorldBuilder(
            PrototypeFieldDefinition fieldDefinition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            QuestProgressionService sharedQuestProgressionService,
            GameplayEventHub sharedGameplayEventHub,
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
            this.sharedGameplayEventHub = sharedGameplayEventHub;
            this.transitionRequester = transitionRequester;
        }

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
            sharedGameplayEventHub = null;
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
                sharedGameplayEventHub,
                transitionRequester);
        }
    }
}
