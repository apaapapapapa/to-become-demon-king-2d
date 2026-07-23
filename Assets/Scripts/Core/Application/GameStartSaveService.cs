using System;
using DemonKing.Domain.Save;

namespace DemonKing.Core.Application
{
    /// <summary>
    /// New Game開始時だけ既存Saveの読込を抑止し、最初の保存以降は選択Slotへ通常どおり書き込みます。
    /// Slotが空であることはUI側でも検証しますが、開始直前にファイル状態が変わっても既存Runtime Stateを復元しないための境界です。
    /// </summary>
    public sealed class FreshGameSaveService : ISaveService
    {
        private readonly ISaveService innerSaveService;

        public FreshGameSaveService(ISaveService innerSaveService)
        {
            this.innerSaveService = innerSaveService ??
                throw new ArgumentNullException(nameof(innerSaveService));
        }

        public bool TryLoad(out GameSaveData saveData)
        {
            saveData = null;
            return false;
        }

        public void Save(GameSaveData saveData)
        {
            innerSaveService.Save(saveData);
        }
    }
}
