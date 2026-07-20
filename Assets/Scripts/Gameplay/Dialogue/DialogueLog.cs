using System;

namespace DemonKing.Gameplay.Dialogue
{
    /// <summary>
    /// 画面表示や話者のGameObjectに依存しない、1件の会話です。
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
    /// 現在画面に表示する1件の会話を保持します。
    /// 会話分岐や入力制御は持たず、表示状態と更新通知だけを担当します。
    /// </summary>
    public sealed class DialogueLog
    {
        private DialogueLine? currentLine;

        public event Action Changed;

        public bool HasCurrentLine => currentLine.HasValue;
        public DialogueLine? CurrentLine => currentLine;

        public DialogueLine ShowLine(string speaker, string text)
        {
            var line = new DialogueLine(speaker, text);
            currentLine = line;
            Changed?.Invoke();
            return line;
        }

        public void Clear()
        {
            if (!currentLine.HasValue)
            {
                return;
            }

            currentLine = null;
            Changed?.Invoke();
        }
    }
}
