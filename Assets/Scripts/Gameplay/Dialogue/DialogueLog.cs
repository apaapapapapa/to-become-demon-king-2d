using System;
using System.Collections.Generic;

namespace DemonKing.Gameplay.Dialogue
{
    /// <summary>
    /// 画面表示や話者のGameObjectに依存しない、1件の会話ログです。
    /// </summary>
    public readonly struct DialogueLine
    {
        public DialogueLine(string speaker, string text)
        {
            Speaker = string.IsNullOrWhiteSpace(speaker) ? "？？？" : speaker.Trim();
            Text = string.IsNullOrWhiteSpace(text)
                ? throw new ArgumentException("会話本文は空にできません。", nameof(text))
                : text.Trim();
        }

        public string Speaker { get; }
        public string Text { get; }
    }

    /// <summary>
    /// 現在のプレイセッションで表示する直近の会話を保持します。
    /// 会話分岐や入力制御は持たず、発言履歴と表示更新通知だけを担当します。
    /// </summary>
    public sealed class DialogueLog
    {
        private readonly List<DialogueLine> lines = new();

        public DialogueLog(int capacity = 4)
        {
            Capacity = Math.Max(1, capacity);
        }

        public event Action<DialogueLine> LineAdded;

        public int Capacity { get; }
        public IReadOnlyList<DialogueLine> Lines => lines;

        public DialogueLine AddLine(string speaker, string text)
        {
            var line = new DialogueLine(speaker, text);
            if (lines.Count >= Capacity)
            {
                lines.RemoveAt(0);
            }

            lines.Add(line);
            LineAdded?.Invoke(line);
            return line;
        }
    }
}
