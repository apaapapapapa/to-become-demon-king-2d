namespace DemonKing.Gameplay.Combat
{
    /// <summary>
    /// ダメージを受け取れる対象の共通契約です。
    /// 攻撃側は敵種別やHP実装を知らず、このインターフェースだけに依存します。
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        DamageResult ApplyDamage(DamageRequest request);
    }
}
