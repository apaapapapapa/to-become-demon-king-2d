namespace DemonKing.Core.Input
{
    /// <summary>
    /// プレイヤー入力をどの用途へ渡すかを表します。
    /// GameplayとUIを同時に有効化せず、Disabledではすべての操作を停止します。
    /// </summary>
    public enum PlayerInputContext
    {
        Gameplay = 0,
        UI = 1,
        Disabled = 2
    }
}
