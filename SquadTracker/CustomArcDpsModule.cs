#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Blish_HUD.ArcDps;
using Blish_HUD.ArcDps.Models;

namespace Torlando.SquadTracker
{
    interface ICustomArcDpsModule
    {
        ICollection<CustomArcDpsModule.Player> GetPlayers();

        event EventHandler PlayerJoined;
        event EventHandler PlayerLeft;
        event EventHandler PlayerChangedCharacter;
        event EventHandler CharacterChangedProfession;
    }

    class CustomArcDpsModule : ICustomArcDpsModule, IDisposable
    {
        private readonly IArcDpsEventsProvider EventsProvider;

        private readonly List<Player> Players = new List<Player>();
        private readonly List<Character> Characters = new List<Character>();

        public event EventHandler? PlayerJoined;
        public event EventHandler? PlayerLeft;
        public event EventHandler? PlayerChangedCharacter;
        public event EventHandler? CharacterChangedProfession;

        public CustomArcDpsModule(IArcDpsEventsProvider eventsProvider)
        {
            this.EventsProvider = eventsProvider;
            this.EventsProvider.RawCombatEvent += EventsProvider_RawCombatEvent;
        }

        public void Dispose() => this.EventsProvider.RawCombatEvent -= EventsProvider_RawCombatEvent;

        private void EventsProvider_RawCombatEvent(object sender, RawCombatEventArgs e)
        {
            var characterEvent = ParseEvent(e.CombatEvent);
            if (characterEvent == null) return; // Sorry kid, we can't do shit…

            var character = this.Characters.FirstOrDefault(c => c.Name == characterEvent.CharacterName);

            var player = character?.Player ?? this.Players.FirstOrDefault(p => p.AccountName != null && p.AccountName == characterEvent.AccountName);

            if (player == null)
            {
                player = characterEvent.CreatePlayer();
                this.Players.Add(player);
            }
            else
            {
                characterEvent.ApplyToPlayer(player);
            }

            if (character == null)
            {
                character = characterEvent.CreateCharacter(player);
                this.Characters.Add(character);
            }
            else
            {
                characterEvent.ApplyToCharacter(character);
            }

            if (characterEvent.IsCurrentCharacter == true)
            {
                if (player.CurrentCharacter != character) player.CurrentCharacter = character;
            }
            else
            {
                player.CurrentCharacter = null;
            }
        }

        /// <summary>
        /// Grab the most data we can for each type of event!
        /// </summary>
        private static IArcDPSEvent? ParseEvent(CombatEvent ce)
        {
            var ev = ce.Ev;
            var src = ce.Src;
            var dst = ce.Dst;

            //ev is null. dst will only be valid on tracking add. skillname will also be null
            if (ev == null)
            {
                //notify tracking change
                if (src.Elite == 0)
                {
                    //add
                    if (src.Profession != 0)
                    {
                        return new CharacterJoinedEvent(
                            characterName: src.Name,
                            accountName: dst.Name,
                            profession: dst.Profession,
                            elite: dst.Elite,
                            self: dst.Self
                        );
                    }
                    //remove
                    else
                    {
                        return new CharacterLeftEvent(
                            characterName: src.Name,
                            accountName: dst.Name
                        );
                    }
                }
                // target change
                else
                {
                    return null;
                }
            }
            //combat event. skillname may be null. non-null skillname will remain static until client exit.refer to evtc notes for complete detail 
            else
            {
                return new CharacterCombatEvent(
                    characterName: src.Name,
                    profession: src.Profession,
                    elite: src.Elite,
                    self: src.Self
                );
            }
        }

        public ICollection<Player> GetPlayers()
        {
            return this.Players;
        }

        private interface IArcDPSEvent
        {
            public string CharacterName { get; }
            public string? AccountName { get; }
            public bool IsCurrentCharacter { get; }

            /// <summary>
            /// Create a new player from the event, to use when the player is not known.
            /// </summary>
            /// <returns>A fresh new player.</returns>
            public Player CreatePlayer();

            /// <summary>
            /// Create a new character from the event, to use when the character is not known.
            /// </summary>
            /// <param name="player">The player attached to the character.</param>
            /// <returns>A fresh new Character.</returns>
            public Character CreateCharacter(Player player);

            /// <summary>
            /// Apply the event data on a Player.
            /// </summary>
            /// <param name="player">The player whose data will be completed.</param>
            public void ApplyToPlayer(Player player);

            /// <summary>
            /// Apply the event data on a Character.
            /// </summary>
            /// <param name="character">The character whose data will be completed.</param>
            public void ApplyToCharacter(Character character);
        }

        private class CharacterJoinedEvent : IArcDPSEvent
        {
            public string CharacterName { get; }
            public string AccountName { get; }
            public uint Profession { get; }
            public uint Elite { get; }
            public uint Self { get; }
            //public uint Team { get; } // what is this?
            //public uint Subgroup { get; } // uncomment when it's useful
            public bool IsCurrentCharacter { get => true; }

            public CharacterJoinedEvent(string characterName, string accountName, uint profession, uint elite, uint self)
            {
                CharacterName = characterName;
                AccountName = accountName;
                Profession = profession;
                Elite = elite;
                Self = self;
            }

            public Player CreatePlayer()
            {
                return new Player
                {
                    AccountName = this.AccountName,
                    IsSelf = this.Self == 1,
                };
            }

            public Character CreateCharacter(Player player)
            {
                return new Character(name: this.CharacterName, player: player)
                {
                    Profession = this.Profession,
                    Elite = this.Elite,
                };
            }

            public void ApplyToPlayer(Player player)
            {
                if (player.AccountName == null) player.AccountName = this.AccountName;
                if (player.IsSelf == false && this.Self == 1) player.IsSelf = true;
            }

            public void ApplyToCharacter(Character character)
            {
                if (character.Profession == null) character.Profession = this.Profession;
                if (character.Elite != this.Elite) character.Elite = this.Elite;
            }
        }

        private class CharacterLeftEvent : IArcDPSEvent
        {
            public string CharacterName { get; }
            public string AccountName { get; }
            public bool IsCurrentCharacter { get => false; }

            public CharacterLeftEvent(string characterName, string accountName)
            {
                CharacterName = characterName;
                AccountName = accountName;
            }

            public Player CreatePlayer()
            {
                return new Player
                {
                    AccountName = this.AccountName,
                };
            }

            public Character CreateCharacter(Player player)
            {
                var character = new Character(name: this.CharacterName, player: player);
                return character;
            }

            public void ApplyToPlayer(Player player)
            {
                if (player.AccountName == null) player.AccountName = this.AccountName;
            }

            public void ApplyToCharacter(Character character) { /* NOOP */}
        }

        private class CharacterCombatEvent : IArcDPSEvent
        {
            public string CharacterName { get; }
            public string? AccountName { get => null; }
            public uint Profession { get; }
            public uint Elite { get; }
            public uint Self { get; }
            //public uint Team { get; } // what is this?
            public bool IsCurrentCharacter { get => true; }

            public CharacterCombatEvent(string characterName, uint profession, uint elite, uint self)
            {
                CharacterName = characterName;
                Profession = profession;
                Elite = elite;
                Self = self;
            }

            public Player CreatePlayer()
            {
                return new Player
                {
                    IsSelf = this.Self == 1,
                };
            }

            public Character CreateCharacter(Player player)
            {
                return new Character(name: this.CharacterName, player: player)
                {
                    Profession = this.Profession,
                    Elite = this.Elite,
                };
            }

            public void ApplyToPlayer(Player player)
            {
                if (player.IsSelf == false && this.Self == 1) player.IsSelf = true;
            }

            public void ApplyToCharacter(Character character)
            {
                if (character.Profession == null) character.Profession = this.Profession;
                if (character.Elite != this.Elite) character.Elite = this.Elite;
            }
        }

        // TODO: Replace all usage of player with these structures
        internal class Player : INotifyPropertyChanged
        {
            public string? AccountName { get => accountName; internal set => SetField(ref accountName, value); }
            private string? accountName;

            public bool IsSelf { get => IsSelf; internal set => SetField(ref isSelf, value); }
            private bool isSelf = false;

            public Character? CurrentCharacter { get => currentCharacter; internal set => SetField(ref currentCharacter, value); }
            private Character? currentCharacter;

            public ICollection<Character> KnownCharacters { get; } = new List<Character>();

            #region INotifyPropertyChanged

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            #endregion INotifyPropertyChanged
        }

        internal class Character : INotifyPropertyChanged
        {
            public Character(string name, Player player)
            {
                Name = name;
                Player = player;

                player.KnownCharacters.Add(this);
            }

            public string Name { get; }
            public Player Player { get; }

            public uint? Profession { get => profession; set => SetField(ref profession, value); }
            private uint? profession;

            public uint? Elite { get => elite; set => SetField(ref elite, value); }
            private uint? elite;

            #region INotifyPropertyChanged

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            #endregion INotifyPropertyChanged
        }
    }
}

#nullable restore