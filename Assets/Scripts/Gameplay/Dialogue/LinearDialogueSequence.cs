using System;
using System.Collections.Generic;

namespace DemonKing.Gameplay.Dialogue
{
    /// <summary>
    /// 直線的な会話列の進行位置だけを管理する、Unity非依存のRuntime Stateです。
    /// 表示先やInteraction入力を知らず、空行を読み飛ばして次の有効な発言を返します。
    /// </summary>
    public sealed class LinearDialogueSequence
    {
        private readonly IReadOnlyList<string> dialogueTexts;
        private int nextDialogueIndex;

        public LinearDialogueSequence(IReadOnlyList<string> dialogueTexts)
        {
            this.dialogueTexts = dialogueTexts ?? Array.Empty<string>();
        }

        public int NextDialogueIndex => nextDialogueIndex;

        public bool TryAdvance(out string dialogueText)
        {
            while (nextDialogueIndex < dialogueTexts.Count)
            {
                string candidate = dialogueTexts[nextDialogueIndex];
                nextDialogueIndex++;
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    dialogueText = candidate.Trim();
                    return true;
                }
            }

            dialogueText = null;
            return false;
        }

        public void Reset()
        {
            nextDialogueIndex = 0;
        }
    }
}
