using System.Linq;
using DemonKing.Core.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DemonKing.Tests.EditMode
{
    public sealed class PlayerInputReaderTests
    {
        [Test]
        public void PlayerControls_AbilityActionsUseCanonicalSlotNames()
        {
            InputActionAsset asset = Resources.Load<InputActionAsset>("Input/PlayerControls");
            Assert.That(asset, Is.Not.Null);

            InputActionMap gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
            Assert.That(gameplay.FindAction(nameof(AbilitySlot.Primary), false), Is.Not.Null);
            Assert.That(gameplay.FindAction(nameof(AbilitySlot.Action1), false), Is.Not.Null);
            Assert.That(gameplay.FindAction(nameof(AbilitySlot.Action2), false), Is.Not.Null);
            Assert.That(gameplay.FindAction(nameof(AbilitySlot.Action3), false), Is.Not.Null);
            Assert.That(gameplay.FindAction(nameof(AbilitySlot.Action4), false), Is.Not.Null);
            Assert.That(gameplay.FindAction("Attack", false), Is.Null);
            Assert.That(gameplay.FindAction("Art", false), Is.Null);
        }

        [Test]
        public void PlayerControls_PrimaryAndAction1PreserveKeyboardAndGamepadBindings()
        {
            InputActionAsset asset = Resources.Load<InputActionAsset>("Input/PlayerControls");
            InputActionMap gameplay = asset.FindActionMap("Gameplay", throwIfNotFound: true);
            InputAction primary = gameplay.FindAction(nameof(AbilitySlot.Primary), true);
            InputAction action1 = gameplay.FindAction(nameof(AbilitySlot.Action1), true);

            Assert.That(primary.bindings.Any(binding => binding.path == "<Keyboard>/j"), Is.True);
            Assert.That(primary.bindings.Any(binding => binding.path == "<Gamepad>/buttonWest"), Is.True);
            Assert.That(action1.bindings.Any(binding => binding.path == "<Keyboard>/k"), Is.True);
            Assert.That(action1.bindings.Any(binding => binding.path == "<Keyboard>/1"), Is.True);
            Assert.That(action1.bindings.Any(binding => binding.path == "<Gamepad>/buttonNorth"), Is.True);
            Assert.That(action1.bindings.Any(binding => binding.path == "<Gamepad>/dpad/up"), Is.True);
        }
    }
}
