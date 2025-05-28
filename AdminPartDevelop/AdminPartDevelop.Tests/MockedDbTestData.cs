using AdminPartDevelop.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace AdminPartDevelop.TestData
{
    public static class MockedDbTestData
    {
        // Starting Game Date
        public static List<StartingGameDate> GetStartingGameDates()
        {
            return new List<StartingGameDate>
            {
                new StartingGameDate { GameDateId = 1, GameDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)) }
            };
        }

        // Competitions
        public static List<Competition> GetCompetitions()
        {
            return new List<Competition>
            {
                new Competition { CompetitionId = "2023110A1A", CompetitionName = "11 PRAŽSKÁ TEPLÁRENSKÁ PŘEBOR MUŽŮ&Muži Krajský přebor skupina A", MatchLength = 45, League = 0 },
                new Competition { CompetitionId = "2023110A2B", CompetitionName = "13 M-1.A/B\t&Muži 1. A třída skupina B", MatchLength = 45, League = 0 },
                new Competition { CompetitionId = "2023005L1A", CompetitionName = "1.liga dorostenek&Neurčeno FAČR Ženy skupina A", MatchLength = 45, League = 0 },
                new Competition { CompetitionId = "2023110F1A", CompetitionName = "61 PŘEBOR MLADŠÍCH ŽÁKŮ &Žáci - mladší Krajský přebor skupina A", MatchLength = 30, League = 0 },
                new Competition { CompetitionId = "2023110Č1A", CompetitionName = "27 - 1. LIGA GENTLEMANŮ 40+ PODZIM &Hráči 40+ Krajský přebor skupina A", MatchLength = 30, League = 0 },
                new Competition { CompetitionId = "2023002E3C", CompetitionName = "ČDŽ U 15 C &Neurčeno Řídící komise pro Čechy skupina C", MatchLength = 40, League = 0}
            };
        }

        // Fields
        public static List<Field> GetFields()
        {
            return new List<Field>
            {
                new Field("Aritma Praha / tráva") { FieldId = 1, FieldAddress = "Nad Lávkou 5, 16000 Praha 6", Latitude = 50.098305f, Longitude = 14.340977f },
                new Field("SLIVENEC  T.") { FieldId = 2, FieldAddress = "U Sportoviště 54/5, 154 00 Slivenec", Latitude = 50.020775f, Longitude = 14.356942f },
                new Field("ZLIČÍN  UMT.") { FieldId = 3, FieldAddress = "U Zličínského hřiště 499/3, 155 21 Zličín", Latitude = 50.058243f, Longitude = 14.28661f },
                new Field("ŘEPORYJE  T.") { FieldId = 4, FieldAddress = "Tělovýchovná 642, 155 00 Řeporyje", Latitude = 50.030514f, Longitude = 14.311001f },
                new Field("PODOLÍ  T.") { FieldId = 5, FieldAddress = "Na Hřebenech II 1132/4, 140 70 Praha 4-Podolí", Latitude = 50.049946f, Longitude = 14.421083f },
                new Field("METEOR BEDŘICHOVSKÁ  UMT.") { FieldId = 6, FieldAddress = "U Meteoru 29, 180 00 Praha 8-Libeň", Latitude = 50.127552f, Longitude = 14.478136f },
                new Field("Tempo  UMT") { FieldId = 7, FieldAddress = "Ve Lhotce 1045/3, 142 00 Praha 4", Latitude = 50.019573f, Longitude = 14.43432f }               
            };
        }

        // Teams
        public static List<Team> GetTeams()
        {
            return new List<Team>
            {
                new Team { TeamId = "1060121", Name = "SK Aritma Praha, z.s.\r\n" },
                new Team { TeamId = "1040231", Name = "AFK Slavoj Podolí Praha, z.s. \"B\"" },       
                new Team { TeamId = "1080051", Name = "FK Meteor Praha VIII, z.s. \"B\"" },
                new Team { TeamId = "10A0151", Name = "SK Čechie Uhříněves, z.s. \"B\"" },
                new Team { TeamId = "1090181", Name = "Spartak Kbely z.s." },
                new Team { TeamId = "1040011", Name = "FC TEMPO PRAHA" },
                new Team { TeamId = "5250321", Name = "MFK Trutnov" }
            };
        }

        // Referees
        public static List<Referee> GetReferees()
        {
            var baseDate = DateTime.Now;
            return new List<Referee>
            {
                new Referee
                {
                    RefereeId = 1, UserId = "user001", FacrId = "02081713", Name = "Filip", Surname = "Pavlík",
                    Email = "filippavlikw@gmail.com", League = 2, Age = 21, Ofs = true,
                    Note = "00421944028825 mladý rozhodčí", PragueZone = "Praha 1", ActuallPragueZone = "Praha 1",
                    CarAvailability = true, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 2, UserId = "user002", FacrId = "FACR002", Name = "Petr", Surname = "Svoboda",
                    Email = "petr.svoboda@email.cz", League = 1, Age = 42, Ofs = true,
                    Note = "Vrchní rozhodčí", PragueZone = "Praha 2", ActuallPragueZone = "Praha 2",
                    CarAvailability = true, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 3, UserId = "user003", FacrId = "FACR003", Name = "Karel", Surname = "Dvořák",
                    Email = "karel.dvorak@email.cz", League = 2, Age = 28, Ofs = false,
                    Note = "Nadějný rozhodčí", PragueZone = "Praha 3", ActuallPragueZone = "Praha 3",
                    CarAvailability = false, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 4, UserId = "user004", FacrId = "FACR004", Name = "Tomáš", Surname = "Černý",
                    Email = "tomas.cerny@email.cz", League = 2, Age = 31, Ofs = true,
                    Note = "00420589563247", PragueZone = "Praha 4", ActuallPragueZone = "Praha 4",
                    CarAvailability = true, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 5, UserId = "user005", FacrId = "FACR005", Name = "Martin", Surname = "Procházka",
                    Email = "martin.prochazka@email.cz", League = 3, Age = 26, Ofs = false,
                    Note = "Nový rozhodčí", PragueZone = "Praha 5", ActuallPragueZone = "Praha 5",
                    CarAvailability = false, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 6, UserId = "user006", FacrId = "FACR006", Name = "Jiří", Surname = "Krejčí",
                    Email = "jiri.krejci@email.cz", League = 3, Age = 33, Ofs = true,
                    Note = "Důsledný rozhodčí", PragueZone = "Praha 6", ActuallPragueZone = "Praha 6",
                    CarAvailability = true, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 7, UserId = "user007", FacrId = "FACR007", Name = "Pavel", Surname = "Novotný",
                    Email = "pavel.novotny@email.cz", League = 4, Age = 24, Ofs = false,
                    Note = "004", PragueZone = "Praha 7", ActuallPragueZone = "Praha 7",
                    CarAvailability = false, TimestampChange = baseDate
                },
                new Referee
                {
                    RefereeId = 8, UserId = "user008", FacrId = "FACR008", Name = "Michal", Surname = "Veselý",
                    Email = "michal.vesely@email.cz", League = 4, Age = 29, Ofs = true,
                    Note = "Assistant referee", PragueZone = "Praha 8", ActuallPragueZone = "Praha 8",
                    CarAvailability = true, TimestampChange = baseDate
                }
            };
        }

        // Matches
        public static List<Match> GetMatches()
        {
            var gameDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            var nextDay = gameDate.AddDays(1);
            var baseTime = new TimeOnly(14, 0);
            var baseDateTime = DateTime.Now;

            return new List<Match>
            {
                // Saturday matches
                new Match
                {
                    MatchId = "2023110A1A0102", CompetitionId = "2023110A1A", HomeTeamId = "1040231", AwayTeamId = "10A0151",
                    FieldId = 5, MatchDate = gameDate, MatchTime = baseTime,
                    RefereeId = 1, Ar1Id = 3, Ar2Id = 4,
                    AlreadyPlayed = true, Locked = false, LastChangedBy = "admin", LastChanged = baseDateTime
                },
                new Match
                {
                    MatchId = "2023110F1A0105", CompetitionId = "2023110F1A", HomeTeamId = "1080051", AwayTeamId = "1040231",
                    FieldId = 6, MatchDate = gameDate, MatchTime = baseTime.AddHours(5),
                    RefereeId = 4, Ar1Id = 5, Ar2Id = 6,
                    AlreadyPlayed = false, Locked = false, LastChangedBy = "admin", LastChanged = baseDateTime
                },
                new Match
                {
                    MatchId = "2023110A2B0404", CompetitionId = "2023110A2B", HomeTeamId = "1090181", AwayTeamId = "1060121",
                    FieldId = 1, MatchDate = gameDate, MatchTime = baseTime.AddHours(1),
                    RefereeId = null, Ar1Id = null, Ar2Id = null, // Unassigned match
                    AlreadyPlayed = false, Locked = false, LastChangedBy = "admin", LastChanged = baseDateTime
                },
                
                // Sunday matches
                new Match
                {
                    MatchId = "2023002E3C0503", CompetitionId = "2023002E3C", HomeTeamId = "1040011", AwayTeamId = "5250321",
                    FieldId = 5, MatchDate = nextDay, MatchTime = baseTime.AddHours(-2).AddMinutes(-30),
                    RefereeId = null, Ar1Id = 1, Ar2Id = null,
                    AlreadyPlayed = false, Locked = false, LastChangedBy = "admin", LastChanged = baseDateTime
                },
                new Match
                {
                    MatchId = "2023110A1A0110", CompetitionId = "2023110A1A", HomeTeamId = "10A0151", AwayTeamId = "1040231",
                    FieldId = 5, MatchDate = nextDay, MatchTime = baseTime,
                    RefereeId = 6, Ar1Id = 2, Ar2Id = 1,
                    AlreadyPlayed = true, Locked = true, LastChangedBy = "admin", LastChanged = baseDateTime
                },
            };
        }

        // Transfers
        public static List<Transfer> GetTransfers()
        {
            var gameWeekend = DateTime.Now.AddDays(5);
            return new List<Transfer>
            {
                new Transfer
                {
                    TransferId = 1, RefereeId = 4, PreviousMatchId = "2023110A1A0102", FutureMatchId = "2023110F1A0105", //half an hour transfer by car
                    ExpectedDeparture = gameWeekend.AddHours(34).AddMinutes(30), ExpectedArrival = gameWeekend.AddHours(35),
                    FromHome = false, Car = true
                }
            };
        }

        // Excuses
        public static List<Excuse> GetExcuses()
        {
            var gameDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            var baseTime = new TimeOnly(10, 0);
            var addedTime = DateTime.Now.AddDays(-2);

            return new List<Excuse>
            {
                new Excuse
                {
                    ExcuseId = 1, RefereeId = 3,
                    DateFrom = gameDate, TimeFrom = baseTime,
                    DateTo = gameDate, TimeTo = new TimeOnly(18, 0),
                    DatetimeAdded = addedTime, Note = "jeden zápas za den", Reason = "Rodinná svatba"
                },
                new Excuse
                {
                    ExcuseId = 2, RefereeId = 5,
                    DateFrom = gameDate.AddDays(1), TimeFrom = baseTime,
                    DateTo = gameDate.AddDays(1), TimeTo = new TimeOnly(16, 0),
                    DatetimeAdded = addedTime, Note = "", Reason = ""
                },
                new Excuse
                {
                    ExcuseId = 3, RefereeId = 6,
                    DateFrom = gameDate, TimeFrom = new TimeOnly(16, 0),
                    DateTo = gameDate, TimeTo = new TimeOnly(20, 0),
                    DatetimeAdded = addedTime, Note = "", Reason = ""
                }
            };
        }

        // Vehicle Slots
        public static List<VehicleSlot> GetVehicleSlots()
        {
            var gameDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            var baseTime = new TimeOnly(8, 0);
            var addedTime = DateTime.Now.AddDays(-1);

            return new List<VehicleSlot>
            {
                new VehicleSlot
                {
                    SlotId = 1, RefereeId = 1,
                    DateFrom = gameDate, TimeFrom = baseTime,
                    DateTo = gameDate, TimeTo = new TimeOnly(20, 0),
                    DatetimeAdded = addedTime, HasCarInTheSlot = true
                },
                new VehicleSlot
                {
                    SlotId = 2, RefereeId = 2,
                    DateFrom = gameDate, TimeFrom = baseTime,
                    DateTo = gameDate.AddDays(1), TimeTo = new TimeOnly(18, 0),
                    DatetimeAdded = addedTime, HasCarInTheSlot = true
                },
                new VehicleSlot
                {
                    SlotId = 3, RefereeId = 4,
                    DateFrom = gameDate.AddDays(1), TimeFrom = new TimeOnly(12, 0),
                    DateTo = gameDate.AddDays(1), TimeTo = new TimeOnly(19, 0),
                    DatetimeAdded = addedTime, HasCarInTheSlot = true
                },
                new VehicleSlot
                {
                    SlotId = 4, RefereeId = 8,
                    DateFrom = gameDate.AddDays(1), TimeFrom = baseTime,
                    DateTo = gameDate.AddDays(1), TimeTo = new TimeOnly(17, 0),
                    DatetimeAdded = addedTime, HasCarInTheSlot = true
                }
            };
        }

        // Vetoes
        public static List<Veto> GetVetoes()
        {
            return new List<Veto>
            {
                new Veto { VetoId = 1, CompetitionId = "2023110A2B", TeamId = "1090181", RefereeId = 1, Note = "Previous conflict" },
                new Veto { VetoId = 2, CompetitionId = "all", TeamId = "5250321", RefereeId = 7, Note = "Bias concern" }
            };
        }

        // Files Previous Delegation
        public static List<FilesPreviousDelegation> GetFilesPreviousDelegation()
        {
            var uploadDate = DateTime.Now.AddDays(-7);
            var delegationStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-14));

            return new List<FilesPreviousDelegation>
            {
                new FilesPreviousDelegation
                {
                    FileId = 1, AmountOfMatches = 15,
                    DelegationsFrom = delegationStart, DelegationsTo = delegationStart.AddDays(2),
                    FileUploadedDatetime = uploadDate, FileName = "delegations_week1.xlsx",
                    FileUploadedBy = 1
                },
                new FilesPreviousDelegation
                {
                    FileId = 2, AmountOfMatches = 12,
                    DelegationsFrom = delegationStart.AddDays(7), DelegationsTo = delegationStart.AddDays(9),
                    FileUploadedDatetime = uploadDate.AddDays(1), FileName = "delegations_week2.xlsx",
                    FileUploadedBy = 1
                }
            };
        }
    }
}
