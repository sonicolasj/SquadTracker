﻿using System.Collections.Generic;

namespace Torlando.SquadTracker
{
    public static class Specialization
    {
        public static string GetEliteName(uint elite, uint core)
        {
            return elite switch
            {
                0 => GetCoreName(core),
                18 => "Berserker",
                61 => "Spellbreaker",
                68 => "Bladesworn",
                27 => "Dragonhunter",
                62 => "Firebrand",
                65 => "Willbender",
                52 => "Herald",
                63 => "Renegade",
                69 => "Vindicator",
                5 => "Druid",
                55 => "Soulbeast",
                72 => "Untamed",
                7 => "Daredevil",
                58 => "Deadeye",
                71 => "Specter",
                43 => "Scrapper",
                57 => "Holosmith",
                70 => "Mechanist",
                34 => "Reaper",
                60 => "Scourge",
                64 => "Harbinger",
                48 => "Tempest",
                56 => "Weaver",
                67 => "Catalyst",
                40 => "Chronomancer",
                59 => "Mirage",
                66 => "Virtuoso",
                _ => "Unknown",
            };
        }

        public static string GetCoreName(uint core)
        {
            return core switch
            {
                1 => "Guardian",
                2 => "Warrior",
                3 => "Engineer",
                4 => "Ranger",
                5 => "Thief",
                6 => "Elementalist",
                7 => "Mesmer",
                8 => "Necromancer",
                9 => "Revenant",
                _ => "Unknown"
            };
        }

        public static readonly IReadOnlyCollection<int> EliteCodes = new[]
        {
            18, // Berserker
            61, // Spellbreaker
            //68, // Bladesworn

            27, // Dragonhunter
            62, // Firebrand
            //65, // Willbender

            52, // Herald
            63, // Renegade
            //69, // Vindicator

             5, // Druid
            55, // Soulbeast
            //72, // Untamed

             7, // Daredevil
            58, // Deadeye
            //71, // Specter

            43, // Scrapper
            57, // Holosmith
            //70, // Mechanist

            34, // Reaper
            60, // Scourge
            //64, // Harbinger

            48, // Tempest
            56, // Weaver
            //67, // Catalyst

            40, // Chronomancer
            59, // Mirage
            //66, // Virtuoso
        };
    }
}
