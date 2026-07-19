namespace DemonKing.Presentation.Rendering
{
    /// <summary>
    /// プロジェクト全体で使用するSorting Layer名を一か所に集約します。
    /// 文字列の直書きを避け、アイソメトリック描画順のルールを明確にします。
    /// </summary>
    public static class SortingLayerNames
    {
        public const string Ground = "Ground";
        public const string World = "World";
        public const string Foreground = "Foreground";
        public const string UI = "UI";
    }
}
