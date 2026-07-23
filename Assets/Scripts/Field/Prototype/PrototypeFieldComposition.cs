using System;
using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Domain.Story;
using DemonKing.Field.Composition;
using DemonKing.Gameplay.Content;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Quests;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    internal sealed class PrototypeFieldCompositionContext
    {
        public PrototypeFieldCompositionContext(
            PrototypeFieldDefinition definition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            QuestProgressionService sharedQuestProgressionService = null,
            GameplayEventHub sharedGameplayEventHub = null,
            StoryProgressionService sharedStoryProgressionService = null,
            IPrototypeFieldTransitionRequester transitionRequester = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            EntryPoint = entryPoint;
            DialogueLog = dialogueLog ?? throw new ArgumentNullException(nameof(dialogueLog));
            ProgressionState = progressionState;
            GrantConsumptionState = grantConsumptionState ??
                ProgressionGrantConsumptionState.CreateInitial();
            SharedQuestProgressionService = sharedQuestProgressionService;
            SharedGameplayEventHub = sharedGameplayEventHub;
            SharedStoryProgressionService = sharedStoryProgressionService;
            TransitionRequester = transitionRequester;
            WorldRoot = new GameObject(definition.DisplayName).transform;
            AmbientEffects = WorldRoot.gameObject.AddComponent<AmbientEffectController>();
        }

        public PrototypeFieldDefinition Definition { get; }
        public FieldEntryPoint EntryPoint { get; }
        public FieldLocation Location => new(Definition.FieldId, EntryPoint.EntryPointId);
        public DialogueLog DialogueLog { get; }
        public CharacterProgressionState ProgressionState { get; }
        public ProgressionGrantConsumptionState GrantConsumptionState { get; }
        public QuestProgressionService SharedQuestProgressionService { get; }
        public GameplayEventHub SharedGameplayEventHub { get; }
        public StoryProgressionService SharedStoryProgressionService { get; }
        public IPrototypeFieldTransitionRequester TransitionRequester { get; }
        public Transform WorldRoot { get; }
        public AmbientEffectController AmbientEffects { get; }

        public RuntimeShapeFactory Shapes { get; private set; }
        public PrototypeTilemapContext Tilemaps { get; private set; }
        public PrototypeRuntimeTileFactory Tiles { get; private set; }
        public PrototypeWorldPrefabFactory Prefabs { get; private set; }
        public TerrainBuilder Terrain { get; private set; }
        public ArchitectureBuilder Architecture { get; private set; }
        public GameObject Player { get; private set; }
        public PrototypeGameplayServices GameplayServices { get; private set; }

        public void SetWorldInfrastructure(
            RuntimeShapeFactory shapes,
            PrototypeTilemapContext tilemaps,
            PrototypeRuntimeTileFactory tiles,
            PrototypeWorldPrefabFactory prefabs,
            TerrainBuilder terrain,
            ArchitectureBuilder architecture)
        {
            Shapes = shapes ?? throw new ArgumentNullException(nameof(shapes));
            Tilemaps = tilemaps ?? throw new ArgumentNullException(nameof(tilemaps));
            Tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
            Prefabs = prefabs ?? throw new ArgumentNullException(nameof(prefabs));
            Terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
        }

        public void SetPlayer(GameObject player) => Player = player;
        public void SetGameplayServices(PrototypeGameplayServices gameplayServices) => GameplayServices = gameplayServices;

        public PrototypeWorldBuildResult CreateResult()
        {
            return new PrototypeWorldBuildResult(
                WorldRoot,
                Player,
                GameplayServices?.RewardService,
                GameplayServices?.GameContentCatalog,
                GameplayServices?.QuestProgressionService ?? SharedQuestProgressionService,
                Location);
        }
    }

    internal sealed class PrototypeFieldComposer
    {
        private readonly FieldCompositionPipeline<PrototypeFieldCompositionContext> pipeline;

        public PrototypeFieldComposer()
            : this(CreateDefaultInstallers())
        {
        }

        internal PrototypeFieldComposer(
            IEnumerable<IFieldInstaller<PrototypeFieldCompositionContext>> installers)
        {
            pipeline = new FieldCompositionPipeline<PrototypeFieldCompositionContext>(installers);
        }

        public PrototypeWorldBuildResult Compose(
            PrototypeFieldDefinition definition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState)
        {
            return Compose(
                definition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService: null,
                sharedGameplayEventHub: null,
                sharedStoryProgressionService: null,
                transitionRequester: null);
        }

        public PrototypeWorldBuildResult Compose(
            PrototypeFieldDefinition definition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            QuestProgressionService sharedQuestProgressionService,
            IPrototypeFieldTransitionRequester transitionRequester)
        {
            return Compose(
                definition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService,
                sharedGameplayEventHub: null,
                sharedStoryProgressionService: null,
                transitionRequester);
        }

        public PrototypeWorldBuildResult Compose(
            PrototypeFieldDefinition definition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            QuestProgressionService sharedQuestProgressionService,
            GameplayEventHub sharedGameplayEventHub,
            IPrototypeFieldTransitionRequester transitionRequester)
        {
            return Compose(
                definition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService,
                sharedGameplayEventHub,
                sharedStoryProgressionService: null,
                transitionRequester);
        }

        public PrototypeWorldBuildResult Compose(
            PrototypeFieldDefinition definition,
            FieldEntryPoint entryPoint,
            DialogueLog dialogueLog,
            CharacterProgressionState progressionState,
            ProgressionGrantConsumptionState grantConsumptionState,
            QuestProgressionService sharedQuestProgressionService,
            GameplayEventHub sharedGameplayEventHub,
            StoryProgressionService sharedStoryProgressionService,
            IPrototypeFieldTransitionRequester transitionRequester)
        {
            var context = new PrototypeFieldCompositionContext(
                definition,
                entryPoint,
                dialogueLog,
                progressionState,
                grantConsumptionState,
                sharedQuestProgressionService,
                sharedGameplayEventHub,
                sharedStoryProgressionService,
                transitionRequester);
            pipeline.Install(context);
            return context.CreateResult();
        }

        internal static IReadOnlyList<IFieldInstaller<PrototypeFieldCompositionContext>> CreateDefaultInstallers()
        {
            return new IFieldInstaller<PrototypeFieldCompositionContext>[]
            {
                new PrototypeWorldInfrastructureInstaller(),
                new PrototypeTerrainBaseInstaller(),
                new PrototypeCollisionInstaller(),
                new PrototypeArchitectureStructuresInstaller(),
                new PrototypeNatureInstaller(),
                new PrototypeArchitectureLandmarksInstaller(),
                new PrototypeAtmosphereInstaller(),
                new PrototypePlayerFieldInstaller(),
                new PrototypeGameplayScenarioInstaller(),
                new PrototypeProgressionPickupFieldInstaller(),
                new PrototypeFieldTransitionInstaller(),
                new PrototypeTerrainForegroundInstaller(),
                new PrototypeCameraFieldInstaller()
            };
        }
    }

    internal sealed class PrototypeWorldInfrastructureInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            PrototypeProjectAssets assets = context.Definition.ProjectAssets;
            var shapes = new RuntimeShapeFactory();
            PrototypeTilemapContext tilemaps = PrototypeTilemapContext.Resolve();
            var tiles = new PrototypeRuntimeTileFactory(assets.GrassTileSprite, assets.PathTileSprite);
            var prefabs = new PrototypeWorldPrefabFactory(assets);
            var terrain = new TerrainBuilder(shapes, tilemaps, tiles, context.Definition.PlayableTileRadius);
            var architecture = new ArchitectureBuilder(shapes, prefabs);
            context.SetWorldInfrastructure(shapes, tilemaps, tiles, prefabs, terrain, architecture);
        }
    }

    internal sealed class PrototypeTerrainBaseInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context) => context.Terrain.BuildBase(context.WorldRoot);
    }

    internal sealed class PrototypeCollisionInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            new CollisionMapBuilder(context.Tilemaps, context.Tiles, context.Definition.PlayableTileRadius).Build();
        }
    }

    internal sealed class PrototypeArchitectureStructuresInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context) => context.Architecture.BuildStructures(context.WorldRoot);
    }

    internal sealed class PrototypeNatureInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            new NatureBuilder(context.Shapes, context.AmbientEffects, context.Prefabs).Build(context.WorldRoot);
        }
    }

    internal sealed class PrototypeArchitectureLandmarksInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context) => context.Architecture.BuildLandmarksAndLighting(context.WorldRoot);
    }

    internal sealed class PrototypeAtmosphereInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            new AtmosphereBuilder(context.Shapes, context.AmbientEffects).Build(context.WorldRoot);
        }
    }

    internal sealed class PrototypePlayerFieldInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            GameObject player = new PrototypePlayerSpawner(
                    context.EntryPoint.Position,
                    context.Definition.PlayerCharacter,
                    context.ProgressionState)
                .Spawn(context.WorldRoot);
            context.SetPlayer(player);
        }
    }

    internal sealed class PrototypeGameplayScenarioInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            GameContentCatalog gameContentCatalog = context.Definition.ProjectAssets.CreateGameContentCatalog();
            if (!PrototypeGameplayServicesFactory.TryCreate(
                    context.Player,
                    context.Definition.ProjectAssets.QuestDefinitions,
                    gameContentCatalog,
                    context.SharedQuestProgressionService,
                    context.SharedGameplayEventHub,
                    context.SharedStoryProgressionService,
                    out PrototypeGameplayServices gameplayServices))
            {
                return;
            }

            context.SetGameplayServices(gameplayServices);
            if (context.Definition.TrainingScenario == null)
            {
                return;
            }

            new PrototypeGameplayFeatureInstaller().Install(
                context.WorldRoot,
                context.Player,
                gameplayServices,
                context.Definition.TrainingScenario,
                context.DialogueLog);
        }
    }

    internal sealed class PrototypeProgressionPickupFieldInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            if (context.GameplayServices == null)
            {
                return;
            }

            new PrototypeProgressionPickupInstaller().Install(
                context.WorldRoot,
                context.GameplayServices.ProgressionAcquisitionService,
                context.GrantConsumptionState,
                context.Definition.ProgressionPickups);
        }
    }

    internal sealed class PrototypeFieldTransitionInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            if (context.TransitionRequester == null || context.Definition.Transitions.Count == 0)
            {
                return;
            }

            foreach (PrototypeFieldTransitionDefinition transition in context.Definition.Transitions)
            {
                GameObject exit = context.Shapes.CreateDiamond(
                    transition.DisplayName,
                    new Vector2(transition.Position.x, transition.Position.y),
                    new Vector2(0.85f, 0.85f),
                    new Color(0.45f, 0.88f, 1f, 0.9f),
                    PrototypeWorldMath.SortOrder(transition.Position.y),
                    context.WorldRoot);
                exit.transform.localPosition = transition.Position;
                PrototypeFieldTransitionInteractable interactable = exit.AddComponent<PrototypeFieldTransitionInteractable>();
                interactable.Initialize(transition.DisplayName, transition.Destination, context.TransitionRequester);
            }
        }
    }

    internal sealed class PrototypeTerrainForegroundInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context) => context.Terrain.BuildForeground(context.WorldRoot);
    }

    internal sealed class PrototypeCameraFieldInstaller : IFieldInstaller<PrototypeFieldCompositionContext>
    {
        public void Install(PrototypeFieldCompositionContext context)
        {
            PrototypeCameraInstaller.Configure(
                PrototypeFieldSceneRuntime.ResolveOrCreateCamera(),
                context.Player == null ? null : context.Player.transform);
        }
    }
}
