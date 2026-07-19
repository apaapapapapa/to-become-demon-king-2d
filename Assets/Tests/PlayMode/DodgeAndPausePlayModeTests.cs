using System.Collections;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Characters.Configuration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DemonKing.Tests.PlayMode
{
    /// <summary>
    /// P2で追加したDodge実挙動とPause状態管理の最小回帰テストです。
    /// </summary>
    public sealed class DodgeAndPausePlayModeTests
    {
        [UnityTest]
        public IEnumerator CharacterDodge2D_右方向へ回避するとRigidbody2Dが移動する()
        {
            Time.timeScale = 1f;

            GameObject player = new("Dodge Test Player");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            player.AddComponent<PlayerInputReader>();
            player.AddComponent<MoveInputReader>();
            player.AddComponent<CharacterMotor2D>();
            CharacterDodge2D dodge = player.AddComponent<CharacterDodge2D>();

            DodgeDefinition definition = ScriptableObject.CreateInstance<DodgeDefinition>();
            dodge.Configure(definition);

            float beforeX = body.position.x;
            bool started = dodge.TryDodge(Vector2.right);
            yield return new WaitForFixedUpdate();

            Assert.That(started, Is.True);
            Assert.That(body.position.x, Is.GreaterThan(beforeX));
            Assert.That(dodge.IsDodging, Is.True);

            Object.Destroy(player);
            Object.Destroy(definition);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GamePauseController_PauseとResumeでTimeScaleとInputContextを切り替える()
        {
            Time.timeScale = 1f;

            GameObject root = new("Pause Test Root");
            PlayerInputReader input = root.AddComponent<PlayerInputReader>();
            GamePauseController pause = root.AddComponent<GamePauseController>();
            pause.Initialize(input, 0f);

            pause.PauseGame();

            Assert.That(pause.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(input.CurrentContext, Is.EqualTo(PlayerInputContext.UI));

            pause.ResumeGame();

            Assert.That(pause.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(input.CurrentContext, Is.EqualTo(PlayerInputContext.Gameplay));

            Object.Destroy(root);
            yield return null;
            Time.timeScale = 1f;
        }
    }
}
