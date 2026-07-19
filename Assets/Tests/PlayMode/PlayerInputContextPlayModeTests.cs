using System.Collections;
using DemonKing.Core.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DemonKing.Tests.PlayMode
{
    /// <summary>
    /// Gameplay / UI / Disabledの入力コンテキスト切り替えを検証します。
    /// </summary>
    public sealed class PlayerInputContextPlayModeTests
    {
        [UnityTest]
        public IEnumerator Context切り替えで有効なActionMapが1つに限定される()
        {
            GameObject player = new("Input Context Test Player");
            PlayerInputReader inputReader = player.AddComponent<PlayerInputReader>();

            yield return null;

            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Gameplay));
            Assert.That(inputReader.IsGameplayInputEnabled, Is.True);
            Assert.That(inputReader.IsUiInputEnabled, Is.False);

            inputReader.EnableUiInput();
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.UI));
            Assert.That(inputReader.IsGameplayInputEnabled, Is.False);
            Assert.That(inputReader.IsUiInputEnabled, Is.True);
            Assert.That(inputReader.Move, Is.EqualTo(Vector2.zero));

            inputReader.DisableInput();
            Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Disabled));
            Assert.That(inputReader.IsGameplayInputEnabled, Is.False);
            Assert.That(inputReader.IsUiInputEnabled, Is.False);
            Assert.That(inputReader.Move, Is.EqualTo(Vector2.zero));
            Assert.That(inputReader.Navigate, Is.EqualTo(Vector2.zero));

            Object.Destroy(player);
        }
    }
}
