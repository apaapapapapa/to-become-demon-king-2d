using System;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Dialogue.Configuration;
using DemonKing.Gameplay.Interaction;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Interaction機能を確認するための試作NPCです。
    /// 会話コンテンツはDialogueDefinition、進行位置はLinearDialogueSequenceへ委譲し、
    /// このComponentはInteractionと画面表示への橋渡しだけを担当します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(GroupYSorter))]
    public sealed class PrototypeNpcInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueDefinition dialogueDefinition;

        private DialogueLog dialogueLog;
        private LinearDialogueSequence dialogueSequence;

        public event Action Interacted;
        public event Action<GameObject> DialogueCompleted;

        public string DialogueId => dialogueDefinition == null ? string.Empty : dialogueDefinition.DialogueId;

        private void Awake()
        {
            RebuildDialogueSequence();

            SphereCollider interactionCollider = GetComponent<SphereCollider>();
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 0.55f;
            interactionCollider.center = new Vector3(0f, 0.35f, 0.55f);

            if (GetComponentInChildren<SpriteRenderer>(includeInactive: true) == null)
            {
                CreateVisuals();
            }
        }

        private void Start()
        {
            GetComponent<GroupYSorter>()?.RefreshRenderers();
        }

        public bool CanInteract(GameObject interactor)
        {
            return enabled && gameObject.activeInHierarchy && interactor != null;
        }

        public void ConfigureDialogueLog(DialogueLog log)
        {
            dialogueLog = log;
        }

        public void ConfigureDialogue(DialogueDefinition definition)
        {
            if (definition == null || !definition.IsConfigured)
            {
                throw new ArgumentException("NPCの会話定義が正しく設定されていません。", nameof(definition));
            }

            dialogueDefinition = definition;
            RebuildDialogueSequence();
        }

        public void Interact(GameObject interactor)
        {
            Interacted?.Invoke();

            if (dialogueLog == null)
            {
                Debug.LogWarning("会話ログが設定されていないため、NPCの発言を画面へ表示できません。", this);
                return;
            }

            if (dialogueDefinition == null || !dialogueDefinition.IsConfigured)
            {
                Debug.LogWarning("会話定義が設定されていないため、NPCの発言を進行できません。", this);
                return;
            }

            dialogueSequence ??= new LinearDialogueSequence(dialogueDefinition.Lines);
            if (!dialogueSequence.TryAdvance(out string dialogueText))
            {
                dialogueLog.Clear();
                dialogueSequence.Reset();
                DialogueCompleted?.Invoke(interactor);
                Debug.Log($"{dialogueDefinition.Speaker}との会話を終了しました。", this);
                return;
            }

            dialogueLog.ShowLine(dialogueDefinition.Speaker, dialogueText);
            Debug.Log($"{dialogueDefinition.Speaker}：『{dialogueText}』", this);
        }

        private void RebuildDialogueSequence()
        {
            dialogueSequence = dialogueDefinition != null && dialogueDefinition.IsConfigured
                ? new LinearDialogueSequence(dialogueDefinition.Lines)
                : null;
        }

        private void CreateVisuals()
        {
            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse("NPCの影", new Vector2(0f, -0.34f), new Vector2(0.88f, 0.24f),
                new Color(0.05f, 0.12f, 0.13f, 0.6f), -2, transform);
            shapes.CreatePatch("ローブ", new Vector2(0f, 0.20f), new Vector2(0.58f, 0.92f),
                new Color(0.30f, 0.34f, 0.68f), 0, transform);
            shapes.CreateEllipse("顔", new Vector2(0f, 0.76f), new Vector2(0.46f, 0.46f),
                new Color(0.95f, 0.76f, 0.58f), 1, transform);
            shapes.CreateDiamond("帽子", new Vector2(0f, 1.08f), new Vector2(0.72f, 0.42f),
                new Color(0.19f, 0.17f, 0.42f), 2, transform);
        }
    }
}
