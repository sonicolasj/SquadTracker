#nullable enable

using System;
using System.Linq;
using Blish_HUD.ArcDps;
using Blish_HUD.ArcDps.Models;
using NUnit.Framework;

namespace Torlando.SquadTracker.Tests
{
    [TestFixture]
    public class CustomArcDpsModule_Tests
    {
        #region Default test data

        private const string DEFAULT_ACCOUNT_NAME = "AccountName.0000";
        private const string DEFAULT_CHARACTER_NAME = "CharacterName";
        private const uint DEFAULT_PROFESSION = 5; // Thief
        private const uint DEFAULT_ELITE_SPEC = 7; // Daredevil
        private const uint DEFAULT_SELF = 0;
        private const ushort DEFAULT_TEAM = 1862;

        #endregion

        #region Simple cases: one event against empty list.

        [Test]
        public void RawCombatEvent_TargetChanged_OnEmptyList_DoesNothing()
        {
            // Arrange
            var studEventProvider = new FakeArcDpsEventsProvider();
            var arcDpsModule = new CustomArcDpsModule(studEventProvider);

            // Act
            studEventProvider.SendTargetChangedEvent();

            // Assert
            Assert.That(arcDpsModule.GetPlayers(), Is.Empty);
        }

        [Test]
        public void RawCombatEvent_PlayerJoined_OnEmptyList_AddsPlayerWithCharacter()
        {
            // Arrange
            var studEventProvider = new FakeArcDpsEventsProvider();
            var arcDpsModule = new CustomArcDpsModule(studEventProvider);

            // Act
            studEventProvider.SendPlayerJoinEvent(DEFAULT_ACCOUNT_NAME, DEFAULT_CHARACTER_NAME, DEFAULT_PROFESSION, DEFAULT_ELITE_SPEC, DEFAULT_SELF, DEFAULT_TEAM);

            // Assert
            var players = arcDpsModule.GetPlayers();
            Assert.That(players, Has.Exactly(1).Items);

            var player = players.First();
            Assert.That(player.AccountName, Is.EqualTo(DEFAULT_ACCOUNT_NAME));
            Assert.That(player.IsSelf, Is.EqualTo(DEFAULT_SELF == 1));

            Assert.That(player.KnownCharacters, Has.Exactly(1).Items);

            var character = player.KnownCharacters.First();
            Assert.That(character.Name, Is.EqualTo(DEFAULT_CHARACTER_NAME));
            Assert.That(character.Profession, Is.EqualTo(DEFAULT_PROFESSION));
            Assert.That(character.Elite, Is.EqualTo(DEFAULT_ELITE_SPEC));

            Assert.That(character.Player, Is.SameAs(player));
            Assert.That(player.CurrentCharacter, Is.SameAs(character));
        }

        [Test]
        public void RawCombatEvent_PlayerLeft_OnEmptyList_AddsPlayerWithCharacter()
        {
            // Arrange
            var studEventProvider = new FakeArcDpsEventsProvider();
            var arcDpsModule = new CustomArcDpsModule(studEventProvider);

            // Act
            studEventProvider.SendPlayerLeftEvent(DEFAULT_ACCOUNT_NAME, DEFAULT_CHARACTER_NAME, DEFAULT_SELF);

            // Assert
            var players = arcDpsModule.GetPlayers();
            Assert.That(players, Has.Exactly(1).Items);

            var player = players.First();
            Assert.That(player.AccountName, Is.EqualTo(DEFAULT_ACCOUNT_NAME));
            Assert.That(player.IsSelf, Is.EqualTo(DEFAULT_SELF == 1));

            Assert.That(player.KnownCharacters, Has.Exactly(1).Items);

            var character = player.KnownCharacters.First();
            Assert.That(character.Name, Is.EqualTo(DEFAULT_CHARACTER_NAME));
            Assert.That(character.Profession, Is.Null);
            Assert.That(character.Elite, Is.Null);

            Assert.That(character.Player, Is.SameAs(player));
            Assert.That(player.CurrentCharacter, Is.SameAs(character));
        }

        [Test]
        public void RawCombatEvent_CharacterDoesCombatEvent_OnEmptyList_AddsEmptyPlayerWithCharacter()
        {
            // Arrange
            var studEventProvider = new FakeArcDpsEventsProvider();
            var arcDpsModule = new CustomArcDpsModule(studEventProvider);

            // Act
            studEventProvider.SendCharacterCombatEvent(DEFAULT_CHARACTER_NAME, DEFAULT_PROFESSION, DEFAULT_ELITE_SPEC, DEFAULT_SELF, isCharacterSource: true);

            // Assert
            var players = arcDpsModule.GetPlayers();
            Assert.That(players, Has.Exactly(1).Items);

            var player = players.First();
            Assert.That(player.AccountName, Is.Null);
            Assert.That(player.IsSelf, Is.EqualTo(DEFAULT_SELF == 1));

            Assert.That(player.KnownCharacters, Has.Exactly(1).Items);

            var character = player.KnownCharacters.First();
            Assert.That(character.Name, Is.EqualTo(DEFAULT_CHARACTER_NAME));
            Assert.That(character.Profession, Is.EqualTo(DEFAULT_PROFESSION));
            Assert.That(character.Elite, Is.EqualTo(DEFAULT_ELITE_SPEC));

            Assert.That(character.Player, Is.SameAs(player));
            Assert.That(player.CurrentCharacter, Is.SameAs(character));
        }

        [Test]
        public void RawCombatEvent_CharacterIsTargetedByCombatEvent_OnEmptyList_AddsEmptyPlayerWithCharacter()
        {
            // Arrange
            var studEventProvider = new FakeArcDpsEventsProvider();
            var arcDpsModule = new CustomArcDpsModule(studEventProvider);

            // Act
            studEventProvider.SendCharacterCombatEvent(DEFAULT_CHARACTER_NAME, DEFAULT_PROFESSION, DEFAULT_ELITE_SPEC, DEFAULT_SELF, isCharacterSource: false);

            // Assert
            var players = arcDpsModule.GetPlayers();
            Assert.That(players, Has.Exactly(1).Items);

            var player = players.First();
            Assert.That(player.AccountName, Is.Null);
            Assert.That(player.IsSelf, Is.EqualTo(DEFAULT_SELF == 1));

            Assert.That(player.KnownCharacters, Has.Exactly(1).Items);

            var character = player.KnownCharacters.First();
            Assert.That(character.Name, Is.EqualTo(DEFAULT_CHARACTER_NAME));
            Assert.That(character.Profession, Is.EqualTo(DEFAULT_PROFESSION));
            Assert.That(character.Elite, Is.EqualTo(DEFAULT_ELITE_SPEC));

            Assert.That(character.Player, Is.SameAs(player));
            Assert.That(player.CurrentCharacter, Is.SameAs(character));
        }

        #endregion
    }

    internal class FakeArcDpsEventsProvider : IArcDpsEventsProvider
    {
        public event EventHandler<RawCombatEventArgs>? RawCombatEvent;

        internal void SendPlayerJoinEvent(string accountName, string characterName, uint profession, uint elite, uint self, ushort team)
        {
            var src = new Ag(
                name: characterName,
                id: default, profession: 1, elite: 0, self: 0, team: default // Stuff I don't care about.
            );

            var dst = new Ag(
                name: accountName,
                profession: profession,
                elite: elite,
                self: self,
                team: team,
                id: default // Stuff I don't care about.
            );

            var ce = new CombatEvent(
                ev: null, src: src, dst: dst,
                skillName: null, id: 0, revision: 1 // Stuff I don't care about.
            );

            this.RawCombatEvent?.Invoke(this, new RawCombatEventArgs(ce, RawCombatEventArgs.CombatEventType.Area));
        }

        internal void SendPlayerLeftEvent(string accountName, string characterName, uint self)
        {
            var src = new Ag(
                name: characterName,
                id: default, profession: 1, elite: 0, self: 0, team: default // Stuff I don't care about.
            );

            var dst = new Ag(
                name: accountName,
                profession: 0,
                elite: 0,
                self: self,
                id: default, team: default // Stuff I don't care about.
            );

            var ce = new CombatEvent(
                ev: null, src: src, dst: dst,
                skillName: null, id: 0, revision: 1 // Stuff I don't care about.
            );

            this.RawCombatEvent?.Invoke(this, new RawCombatEventArgs(ce, RawCombatEventArgs.CombatEventType.Area));
        }

        internal void SendTargetChangedEvent()
        {
            var src = new Ag(
                name: "",
                id: default, profession: 1, elite: 0, self: 0, team: default // Stuff I don't care about.
            );

            Ag? dst = null;

            var ce = new CombatEvent(
                ev: null, src: src, dst: dst,
                skillName: null, id: 0, revision: 1 // Stuff I don't care about.
            );

            this.RawCombatEvent?.Invoke(this, new RawCombatEventArgs(ce, RawCombatEventArgs.CombatEventType.Area));
        }

        /// <param name="isCharacterSource">true if the character made the action, false if it was the action's target.</param>
        internal void SendCharacterCombatEvent(string characterName, uint profession, uint elite, uint self, bool isCharacterSource)
        {
            var character = new Ag(
                name: characterName,
                profession: profession,
                elite: elite,
                self: self,
                id: default, team: default // Stuff I don't care about.
            );

            var target = new Ag(
                name: "TARGET",
                profession: uint.MaxValue, // TODO: explore this
                elite: uint.MaxValue, // TODO: explore this
                self: 0,
                team: ushort.MaxValue, // TODO: explore this
                id: default // Stuff I don't care about.
            );

            if (!isCharacterSource) (character, target) = (target, character);

            // Lol
            var ev = new Ev(default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default); // Stuff I don't care about.

            var ce = new CombatEvent(
                ev: ev, src: character, dst: target,
                skillName: "SKILL", id: uint.MaxValue, revision: 1 // Stuff I don't care about.
            );

            this.RawCombatEvent?.Invoke(this, new RawCombatEventArgs(ce, RawCombatEventArgs.CombatEventType.Area));
        }
    }
}

#nullable restore