using System;
using DemonKing.Domain.Events;
using DemonKing.Domain.Story;
using DemonKing.Gameplay.AI;
using DemonKing.Gameplay.AI.Configuration;
using DemonKing.Gameplay.Combat;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Events;
using DemonKing.Gameplay.Interaction;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prologue Part 1専用のField Compositionです。
    /// 訓練QuestのCoordinatorへ本編Storyを混在させず、既存のDialogue / Combat / Gameplay Event境界だけを再利用します。
    /// </summary>
    internal sealed class PrototypePrologueScenarioInstaller
    {
        public void Install(
            Transform parent,
            GameObject player,
            PrototypeGameplayServices gameplayServices,
            EnemyAiDefinition forestCreatureAi,
            DialogueLog dialogueLog)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (gameplayServices?.GameplayEventHub == null ||
                gameplayServices.StoryProgressionService == null)
            {
                throw new ArgumentException(
                    "Prologue進行に必要なGameplay Event Hub / Story Runtimeが初期化されていません。",
                    nameof(gameplayServices));
            }

            if (dialogueLog == null)
            {
                throw new ArgumentNullException(nameof(dialogueLog));
            }

            PrototypeNpcInteractable guardian = CreateGuardian(parent, dialogueLog);

            GameObject controllerObject = new("プロローグ進行制御");
            controllerObject.transform.SetParent(parent, false);
            PrototypePrologueFlowController controller =
                controllerObject.AddComponent<PrototypePrologueFlowController>();
            controller.Initialize(
                parent,
                player,
                guardian,
                dialogueLog,
                gameplayServices.GameplayEventHub,
                gameplayServices.StoryProgressionService,
                forestCreatureAi);
        }

        private static PrototypeNpcInteractable CreateGuardian(
            Transform parent,
            DialogueLog dialogueLog)
        {
            GameObject guardianObject = new("育ての親");
            guardianObject.transform.SetParent(parent, false);
            guardianObject.transform.localPosition = new Vector3(-1.55f, 0.35f, 0f);

            PrototypeNpcInteractable guardian =
                guardianObject.AddComponent<PrototypeNpcInteractable>();
            guardian.ConfigureDialogueLog(dialogueLog);
            guardian.ConfigureDialogue(PrototypePrologueContent.GuardianIntroDialogue);
            ConfigureGuardianVisuals(guardianObject);
            return guardian;
        }

        private static void ConfigureGuardianVisuals(GameObject guardianObject)
        {
            foreach (SpriteRenderer renderer in guardianObject.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.enabled = false;
            }

            var shapes = new RuntimeShapeFactory();
            Transform parent = guardianObject.transform;
            shapes.CreateEllipse(
                "育ての親の影",
                new Vector2(0f, -0.36f),
                new Vector2(1.28f, 0.30f),
                new Color(0.04f, 0.09f, 0.08f, 0.62f),
                -2,
                parent);
            shapes.CreateEllipse(
                "育ての親のからだ",
                new Vector2(0f, 0.18f),
                new Vector2(1.08f, 0.86f),
                new Color(0.20f, 0.47f, 0.40f),
                0,
                parent);
            shapes.CreateEllipse(
                "育ての親の額",
                new Vector2(0f, 0.49f),
                new Vector2(0.74f, 0.42f),
                new Color(0.29f, 0.60f, 0.48f),
                1,
                parent);
            shapes.CreateDiamond(
                "育ての親の左角",
                new Vector2(-0.31f, 0.75f),
                new Vector2(0.18f, 0.32f),
                new Color(0.58f, 0.55f, 0.40f),
                2,
                parent);
            shapes.CreateDiamond(
                "育ての親の右角",
                new Vector2(0.31f, 0.75f),
                new Vector2(0.18f, 0.32f),
                new Color(0.58f, 0.55f, 0.40f),
                2,
                parent);
            shapes.CreateEllipse(
                "育ての親の左目",
                new Vector2(-0.18f, 0.42f),
                new Vector2(0.08f, 0.11f),
                new Color(0.94f, 0.84f, 0.52f),
                3,
                parent);
            shapes.CreateEllipse(
                "育ての親の右目",
                new Vector2(0.18f, 0.42f),
                new Vector2(0.08f, 0.11f),
                new Color(0.94f, 0.84f, 0.52f),
                3,
                parent);

            guardianObject.GetComponent<GroupYSorter>()?.RefreshRenderers();
        }
    }

    internal sealed class PrototypePrologueFlowController : MonoBehaviour
    {
        private Transform worldRoot;
        private GameObject player;
        private PrototypeNpcInteractable guardian;
        private DialogueLog dialogueLog;
        private GameplayEventHub gameplayEventHub;
        private StoryProgressionService storyProgressionService;
        private EnemyAiDefinition forestCreatureAi;
        private PrototypePrologueForageInteractable forage;
        private PrototypePrologueCreature creature;
        private bool initialized;

        public void Initialize(
            Transform worldRoot,
            GameObject player,
            PrototypeNpcInteractable guardian,
            DialogueLog dialogueLog,
            GameplayEventHub gameplayEventHub,
            StoryProgressionService storyProgressionService,
            EnemyAiDefinition forestCreatureAi)
        {
            if (initialized)
            {
                Debug.LogWarning("PrototypePrologueFlowControllerは既に初期化されています。", this);
                return;
            }

            this.worldRoot = worldRoot != null
                ? worldRoot
                : throw new ArgumentNullException(nameof(worldRoot));
            this.player = player != null
                ? player
                : throw new ArgumentNullException(nameof(player));
            this.guardian = guardian != null
                ? guardian
                : throw new ArgumentNullException(nameof(guardian));
            this.dialogueLog = dialogueLog ??
                throw new ArgumentNullException(nameof(dialogueLog));
            this.gameplayEventHub = gameplayEventHub ??
                throw new ArgumentNullException(nameof(gameplayEventHub));
            this.storyProgressionService = storyProgressionService ??
                throw new ArgumentNullException(nameof(storyProgressionService));
            this.forestCreatureAi = forestCreatureAi;

            guardian.Interacted += HandleGuardianInteracted;
            guardian.ConversationStarted += HandleGuardianConversationStarted;
            guardian.DialogueCompleted += HandleGuardianDialogueCompleted;

            RefreshGuardianDialogue();
            EnsureObjectives();
            initialized = true;
        }

        private void OnDestroy()
        {
            if (guardian != null)
            {
                guardian.Interacted -= HandleGuardianInteracted;
                guardian.ConversationStarted -= HandleGuardianConversationStarted;
                guardian.DialogueCompleted -= HandleGuardianDialogueCompleted;
            }

            if (forage != null)
            {
                forage.Completed -= HandleForageCompleted;
            }

            if (creature != null)
            {
                creature.Defeated -= HandleCreatureDefeated;
            }
        }

        private void HandleGuardianInteracted()
        {
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                guardian.DialogueId));
        }

        private void HandleGuardianConversationStarted()
        {
            RefreshGuardianDialogue();
        }

        private void HandleGuardianDialogueCompleted(GameObject interactor)
        {
            string completedDialogueId = guardian.DialogueId;
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.DialogueCompleted,
                completedDialogueId));

            EnsureObjectives();
            RefreshGuardianDialogue();

            StoryProgressState state = storyProgressionService.State;
            if (string.Equals(
                    completedDialogueId,
                    PrototypePrologueContent.GuardianIntroDialogueId,
                    StringComparison.Ordinal) &&
                state.HasFlag(PrototypeStoryDefinitions.MetGuardianFlagId))
            {
                dialogueLog.ShowLine(
                    "育ての親",
                    "まず木の実を一つ探し、小さな魔物を一体倒して戻ってこい。危なくなったら逃げろ。");
            }
        }

        private void HandleForageCompleted(GameObject interactor)
        {
            if (forage != null)
            {
                forage.Completed -= HandleForageCompleted;
            }

            forage = null;
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.InteractionCompleted,
                PrototypePrologueContent.ForageInteractionId));
            dialogueLog.ShowLine(
                "育ての親",
                "その実なら食べられる。匂いと色を覚えておけ。");
            RefreshGuardianDialogue();
        }

        private void HandleCreatureDefeated(DefeatContext context)
        {
            if (creature != null)
            {
                creature.Defeated -= HandleCreatureDefeated;
            }

            creature = null;
            gameplayEventHub.Publish(new GameplayEvent(
                GameplayEventIds.EnemyDefeated,
                PrototypePrologueContent.ForestCreatureActorId));
            dialogueLog.ShowLine(
                "育ての親",
                "倒せたな。勝つことより、無事に戻ってこられたことを覚えておけ。");
            RefreshGuardianDialogue();
        }

        private void RefreshGuardianDialogue()
        {
            guardian.ConfigureDialogue(
                PrototypePrologueContent.SelectGuardianDialogue(
                    storyProgressionService.State));
        }

        private void EnsureObjectives()
        {
            StoryProgressState state = storyProgressionService.State;
            if (!state.HasFlag(PrototypeStoryDefinitions.MetGuardianFlagId) ||
                state.HasFlag(PrototypeStoryDefinitions.ProloguePart1CompletedFlagId))
            {
                return;
            }

            if (!state.HasFlag(PrototypeStoryDefinitions.FoundFoodFlagId) && forage == null)
            {
                forage = CreateForage();
                forage.Completed += HandleForageCompleted;
            }

            if (!state.HasFlag(PrototypeStoryDefinitions.FirstHuntFlagId) && creature == null)
            {
                creature = CreateCreature();
                creature.Defeated += HandleCreatureDefeated;
            }
        }

        private PrototypePrologueForageInteractable CreateForage()
        {
            GameObject forageObject = new("赤い木の実");
            forageObject.transform.SetParent(worldRoot, false);
            forageObject.transform.localPosition = new Vector3(2.0f, 1.25f, 0f);
            return forageObject.AddComponent<PrototypePrologueForageInteractable>();
        }

        private PrototypePrologueCreature CreateCreature()
        {
            GameObject creatureObject = new("森の幼獣");
            creatureObject.transform.SetParent(worldRoot, false);
            creatureObject.transform.localPosition = new Vector3(2.25f, -1.35f, 0f);
            PrototypePrologueCreature prologueCreature =
                creatureObject.AddComponent<PrototypePrologueCreature>();

            if (forestCreatureAi != null && forestCreatureAi.IsConfigured)
            {
                EnemyAiController aiController =
                    creatureObject.AddComponent<EnemyAiController>();
                aiController.Configure(forestCreatureAi, player);
            }

            creatureObject.AddComponent<PrototypeMonsterDefeatEffect>();
            return prologueCreature;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(GroupYSorter))]
    internal sealed class PrototypePrologueForageInteractable : MonoBehaviour, IInteractable
    {
        private bool consumed;

        public event Action<GameObject> Completed;

        private void Awake()
        {
            SphereCollider interactionCollider = GetComponent<SphereCollider>();
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 0.55f;
            interactionCollider.center = new Vector3(0f, 0.18f, 0.45f);

            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse(
                "木の実の影",
                new Vector2(0f, -0.18f),
                new Vector2(0.72f, 0.20f),
                new Color(0.04f, 0.10f, 0.07f, 0.52f),
                -2,
                transform);
            shapes.CreatePatch(
                "葉",
                new Vector2(0f, 0.05f),
                new Vector2(0.70f, 0.44f),
                new Color(0.18f, 0.52f, 0.26f),
                0,
                transform);
            shapes.CreateEllipse(
                "赤い木の実",
                new Vector2(0.04f, 0.25f),
                new Vector2(0.28f, 0.28f),
                new Color(0.78f, 0.20f, 0.24f),
                1,
                transform);
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        public bool CanInteract(GameObject interactor)
        {
            return !consumed && enabled && gameObject.activeInHierarchy && interactor != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            consumed = true;
            Completed?.Invoke(interactor);
            gameObject.SetActive(false);
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(GroupYSorter))]
    [RequireComponent(typeof(Health))]
    internal sealed class PrototypePrologueCreature : MonoBehaviour
    {
        private Health health;

        public event Action<DefeatContext> Defeated;

        public bool IsAlive => health != null && health.IsAlive;

        private void Awake()
        {
            health = GetComponent<Health>();
            health.ConfigureMaxHealth(2);
            health.ConfigureCombatIdentity(PrototypePrologueContent.ForestCreatureActorId);

            SphereCollider hitCollider = GetComponent<SphereCollider>();
            hitCollider.isTrigger = true;
            hitCollider.radius = 0.42f;
            hitCollider.center = new Vector3(0f, 0.20f, 0.44f);

            CreateVisuals();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        private void HandleDied(DefeatContext context)
        {
            Defeated?.Invoke(context);
            Destroy(gameObject);
        }

        private void CreateVisuals()
        {
            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse(
                "幼獣の影",
                new Vector2(0f, -0.27f),
                new Vector2(0.86f, 0.24f),
                new Color(0.05f, 0.09f, 0.08f, 0.58f),
                -2,
                transform);
            shapes.CreateEllipse(
                "幼獣のからだ",
                new Vector2(0f, 0.04f),
                new Vector2(0.74f, 0.58f),
                new Color(0.47f, 0.34f, 0.24f),
                0,
                transform);
            shapes.CreateDiamond(
                "幼獣の左耳",
                new Vector2(-0.24f, 0.36f),
                new Vector2(0.18f, 0.24f),
                new Color(0.37f, 0.26f, 0.18f),
                1,
                transform);
            shapes.CreateDiamond(
                "幼獣の右耳",
                new Vector2(0.24f, 0.36f),
                new Vector2(0.18f, 0.24f),
                new Color(0.37f, 0.26f, 0.18f),
                1,
                transform);
            shapes.CreateEllipse(
                "幼獣の左目",
                new Vector2(-0.13f, 0.12f),
                new Vector2(0.07f, 0.09f),
                new Color(0.08f, 0.05f, 0.03f),
                2,
                transform);
            shapes.CreateEllipse(
                "幼獣の右目",
                new Vector2(0.13f, 0.12f),
                new Vector2(0.07f, 0.09f),
                new Color(0.08f, 0.05f, 0.03f),
                2,
                transform);
        }
    }
}
