using System;
using System.Collections.Generic;
using System.Linq;
using DemonKing.Domain;
using UnityEngine;

namespace DemonKing.Gameplay.Dialogue.Configuration
{
    /// <summary>
    /// NPCやイベントから再利用できる不変の会話コンテンツを定義します。
    /// 表示対象のGameObjectや会話進行位置は保持せず、安定ID・話者・発言列だけを管理します。
    /// </summary>
    [CreateAssetMenu(
        fileName = "DialogueDefinition",
        menuName = "Demon King/Gameplay/Dialogue Definition")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        [SerializeField] private string dialogueId = string.Empty;
        [SerializeField] private string speaker = "？？？";
        [SerializeField, TextArea(2, 4)] private string[] lines = Array.Empty<string>();

        public string DialogueId => dialogueId;
        public string Speaker => string.IsNullOrWhiteSpace(speaker) ? "？？？" : speaker.Trim();
        public IReadOnlyList<string> Lines => lines ?? Array.Empty<string>();
        public bool IsConfigured =>
            StableContentId.IsValid(dialogueId) &&
            lines != null &&
            lines.Any(line => !string.IsNullOrWhiteSpace(line));

        /// <summary>
        /// TestやRuntime生成コンテンツ向けのDefinitionを作成します。
        /// 正式コンテンツはProject Assetとして作成し、このFactoryに依存しません。
        /// </summary>
        public static DialogueDefinition CreateRuntime(
            string id,
            string speakerName,
            params string[] dialogueLines)
        {
            DialogueDefinition definition = CreateInstance<DialogueDefinition>();
            definition.dialogueId = StableContentId.Require(id, nameof(id));
            definition.speaker = string.IsNullOrWhiteSpace(speakerName) ? "？？？" : speakerName.Trim();
            definition.lines = dialogueLines == null
                ? Array.Empty<string>()
                : dialogueLines.ToArray();
            return definition;
        }

        private void OnValidate()
        {
            dialogueId = StableContentId.Normalize(dialogueId);
            speaker = string.IsNullOrWhiteSpace(speaker) ? "？？？" : speaker.Trim();
            lines ??= Array.Empty<string>();
        }
    }
}
