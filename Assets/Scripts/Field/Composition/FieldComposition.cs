using System;
using System.Collections.Generic;
using UnityEngine;

namespace DemonKing.Field.Composition
{
    /// <summary>
    /// Field内の進入地点をStable IDと座標で表現します。
    /// Scene上のTransformやGameObjectをGame Sessionへ持ち込まないための静的定義です。
    /// </summary>
    public readonly struct FieldEntryPoint : IEquatable<FieldEntryPoint>
    {
        public FieldEntryPoint(string entryPointId, Vector3 position)
        {
            if (string.IsNullOrWhiteSpace(entryPointId))
            {
                throw new ArgumentException("Entry Point IDは必須です。", nameof(entryPointId));
            }

            EntryPointId = entryPointId;
            Position = position;
        }

        public string EntryPointId { get; }
        public Vector3 Position { get; }

        public bool Equals(FieldEntryPoint other)
        {
            return string.Equals(EntryPointId, other.EntryPointId, StringComparison.Ordinal) &&
                   Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is FieldEntryPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EntryPointId == null ? 0 : EntryPointId.GetHashCode()) * 397) ^
                       Position.GetHashCode();
            }
        }
    }

    /// <summary>
    /// SaveやField遷移要求で利用するField位置です。
    /// Unity SceneのBuild Indexではなく変更されないField / Entry Point IDを保持します。
    /// </summary>
    public readonly struct FieldLocation : IEquatable<FieldLocation>
    {
        public FieldLocation(string fieldId, string entryPointId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
            {
                throw new ArgumentException("Field IDは必須です。", nameof(fieldId));
            }

            if (string.IsNullOrWhiteSpace(entryPointId))
            {
                throw new ArgumentException("Entry Point IDは必須です。", nameof(entryPointId));
            }

            FieldId = fieldId;
            EntryPointId = entryPointId;
        }

        public string FieldId { get; }
        public string EntryPointId { get; }
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(FieldId) &&
            !string.IsNullOrWhiteSpace(EntryPointId);

        public bool Equals(FieldLocation other)
        {
            return string.Equals(FieldId, other.FieldId, StringComparison.Ordinal) &&
                   string.Equals(EntryPointId, other.EntryPointId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is FieldLocation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FieldId == null ? 0 : FieldId.GetHashCode()) * 397) ^
                       (EntryPointId == null ? 0 : EntryPointId.GetHashCode());
            }
        }

        public override string ToString()
        {
            return $"{FieldId}:{EntryPointId}";
        }
    }

    /// <summary>
    /// 一つのField Composition単位を構築する契約です。
    /// Installer間の実行順序はField Definition側のPipelineで決定します。
    /// </summary>
    public interface IFieldInstaller<in TContext>
    {
        void Install(TContext context);
    }

    /// <summary>
    /// Field固有Installerを定義順に実行します。
    /// Scene BootstrapやGame Sessionは各Installerの詳細を知りません。
    /// </summary>
    public sealed class FieldCompositionPipeline<TContext>
    {
        private readonly IReadOnlyList<IFieldInstaller<TContext>> installers;

        public FieldCompositionPipeline(IEnumerable<IFieldInstaller<TContext>> installers)
        {
            if (installers == null)
            {
                throw new ArgumentNullException(nameof(installers));
            }

            var installerList = new List<IFieldInstaller<TContext>>();
            foreach (IFieldInstaller<TContext> installer in installers)
            {
                installerList.Add(installer ??
                    throw new ArgumentException("Field Installerにnullを含めることはできません。", nameof(installers)));
            }

            this.installers = installerList;
        }

        public IReadOnlyList<IFieldInstaller<TContext>> Installers => installers;

        public void Install(TContext context)
        {
            if (ReferenceEquals(context, null))
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (IFieldInstaller<TContext> installer in installers)
            {
                installer.Install(context);
            }
        }
    }
}
