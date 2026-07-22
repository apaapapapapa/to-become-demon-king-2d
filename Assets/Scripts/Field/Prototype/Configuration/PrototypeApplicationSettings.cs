using System;
using UnityEngine;

namespace DemonKing.Field.Prototype.Configuration
{
    /// <summary>
    /// プロトタイプ起動時に必要なアプリケーション全体設定を管理します。
    /// FieldBootstrapへ直接シリアライズ値を持たせず、起動構成からデータを分離します。
    /// </summary>
    [CreateAssetMenu(fileName = "PrototypeApplicationSettings", menuName = "Demon King/Prototype Application Settings")]
    public sealed class PrototypeApplicationSettings : ScriptableObject
    {
        [Header("Initial Field")]
        [SerializeField] private string initialFieldId = PrototypeFieldDefinition.DefaultFieldId;
        [SerializeField] private string initialSceneName = PrototypeFieldDefinition.DefaultSceneName;
        [SerializeField] private string initialFieldDisplayName = PrototypeFieldDefinition.DefaultDisplayName;
        [SerializeField] private string defaultEntryPointId = PrototypeFieldDefinition.DefaultEntryPointId;

        [Header("Player Entry Point")]
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, -1.35f, -1f);

        [Header("Field Runtime Shape")]
        [SerializeField, Min(4)] private int playableTileRadius = 15;

        [Header("Application")]
        [SerializeField, Range(0f, 1f)] private float pausedTimeScale = 0f;

        public string InitialFieldId => string.IsNullOrWhiteSpace(initialFieldId)
            ? PrototypeFieldDefinition.DefaultFieldId
            : initialFieldId;
        public string InitialSceneName => string.IsNullOrWhiteSpace(initialSceneName)
            ? PrototypeFieldDefinition.DefaultSceneName
            : initialSceneName;
        public string InitialFieldDisplayName => string.IsNullOrWhiteSpace(initialFieldDisplayName)
            ? PrototypeFieldDefinition.DefaultDisplayName
            : initialFieldDisplayName;
        public string DefaultEntryPointId => string.IsNullOrWhiteSpace(defaultEntryPointId)
            ? PrototypeFieldDefinition.DefaultEntryPointId
            : defaultEntryPointId;
        public Vector3 PlayerSpawnPosition => playerSpawnPosition;
        public int PlayableTileRadius => Mathf.Max(4, playableTileRadius);
        public float PausedTimeScale => Mathf.Clamp(pausedTimeScale, 0f, 1f);

        public bool HasValidInitialFieldDefinition =>
            !string.IsNullOrWhiteSpace(InitialFieldId) &&
            !string.IsNullOrWhiteSpace(InitialSceneName) &&
            !string.IsNullOrWhiteSpace(InitialFieldDisplayName) &&
            !string.IsNullOrWhiteSpace(DefaultEntryPointId);
    }
}
