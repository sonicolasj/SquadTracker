#nullable enable

using System;
using Blish_HUD;
using Blish_HUD.ArcDps;

namespace Torlando.SquadTracker
{
    internal interface IArcDpsEventsProvider
    {
        event EventHandler<RawCombatEventArgs> RawCombatEvent;
    }

    internal class ArcDpsEventsProvider : IArcDpsEventsProvider, IDisposable
    {
        public event EventHandler<RawCombatEventArgs>? RawCombatEvent;

        public ArcDpsEventsProvider() => GameService.ArcDps.RawCombatEvent += ArcDps_RawCombatEvent;
        public void Dispose() => GameService.ArcDps.RawCombatEvent -= ArcDps_RawCombatEvent;

        private void ArcDps_RawCombatEvent(object sender, RawCombatEventArgs e)
            => this.RawCombatEvent?.Invoke(sender, e);
    }
}

#nullable restore