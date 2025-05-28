using AdminPartDevelop;
using AdminPartDevelop.Common;
using AdminPartDevelop.Data;
using AdminPartDevelop.DTOs;
using AdminPartDevelop.Models;
using AdminPartDevelop.Services.AdminServices;
using AdminPartDevelop.Views.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AdminPartDevelop.Tests.Services
{
    public class AdminServiceTest
    {
        private readonly Mock<IAdminRepo> _mockAdminRepo;
        private readonly Mock<ILogger<AdminService>> _mockLogger;
        private readonly AdminService _adminService;

        public AdminServiceTest()
        {
            _mockAdminRepo = new Mock<IAdminRepo>();
            _mockLogger = new Mock<ILogger<AdminService>>();
            _adminService = new AdminService(_mockAdminRepo.Object, _mockLogger.Object);
        }

        #region ProccessDtosToMatches Tests

        [Fact]
        public void ProccessDtosToMatches_ValidInput_ReturnsSuccessWithMatches()
        {
            // Arrange
            var competition = new Competition { CompetitionId = "2023110A1A", MatchLength = 45, CompetitionName = "Muži Krajský přebor skupina A" };
            var field = new Field("Aritma Praha / tráva");
            var homeTeam = new Team { TeamId = "10A0141", Name = "TJ Sokol Královice, z.s." };
            var awayTeam = new Team { TeamId = "1080021", Name = "Sportovní klub Dolní Chabry, z.s." };
            var matchDtos = new List<UnfilledMatchDto>
            {
                 new UnfilledMatchDto
                (
                    numberMatch : "2023110A1A0106",
                    gameField : "Aritma Praha / tráva",
                    idHomeRaw : "10A0141",
                    nameHome : "TJ Sokol Královice, z.s.",
                    idAwayRaw : "1080021",
                    nameAway : "Sportovní klub Dolní Chabry, z.s.",
                    dateOfGame : DateTime.Now.AddDays(1)
                )
            };

            _mockAdminRepo.Setup(x => x.DoesCompetitionExist("2023110A1A"))
                .Returns(RepositoryResult<Competition>.Success(competition));
            _mockAdminRepo.Setup(x => x.GetOrSaveTheField("Aritma Praha / tráva"))
                .Returns(RepositoryResult<Field>.Success(field));
            _mockAdminRepo.Setup(x => x.GetOrSaveTheTeam("10A0141", "TJ Sokol Královice, z.s."))
                .Returns(RepositoryResult<Team>.Success(homeTeam));
            _mockAdminRepo.Setup(x => x.GetOrSaveTheTeam("1080021", "Sportovní klub Dolní Chabry, z.s."))
                .Returns(RepositoryResult<Team>.Success(awayTeam));

            // Act
            var result = _adminService.ProccessDtosToMatches(matchDtos, "Test admin");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.GetDataOrThrow());
            var match = result.GetDataOrThrow().First();
            Assert.Equal("2023110A1A0106", match.MatchId);
            Assert.Equal(competition.CompetitionId, match.CompetitionId);
            Assert.Equal("10A0141", match.HomeTeamId);
            Assert.Equal("1080021", match.AwayTeamId);
            Assert.Equal("Test admin", match.LastChangedBy);
            Assert.False(match.AlreadyPlayed);
            Assert.False(match.Locked);
            Assert.Equal(competition.CompetitionName, match.Competition.CompetitionName);
        }
        [Fact]
        public void ProccessDtosToMatches_EmptyNumberMatch_UsesEmptyString()
        {
            // Arrange
            var competition = new Competition { CompetitionId = "2023110A1A", MatchLength = 45, CompetitionName = "Muži Krajský přebor skupina A" };
            var field = new Field("Aritma Praha / tráva");
            var homeTeam = new Team { TeamId = "10A0141", Name = "TJ Sokol Královice, z.s." };
            var awayTeam = new Team { TeamId = "1080021", Name = "Sportovní klub Dolní Chabry, z.s." };

            var matchDtos = new List<UnfilledMatchDto>
            {
                 new UnfilledMatchDto
                (
                    numberMatch : "",
                    gameField : "Aritma Praha / tráva",
                    idHomeRaw : "10A0141",
                    nameHome : "TJ Sokol Královice, z.s.",
                    idAwayRaw : "1080021",
                    nameAway : "Sportovní klub Dolní Chabry, z.s.",
                    dateOfGame : DateTime.Now.AddDays(1)
                )
            };

            _mockAdminRepo.Setup(x => x.DoesCompetitionExist("2023110A1A"))
                 .Returns(RepositoryResult<Competition>.Success(competition));
            _mockAdminRepo.Setup(x => x.GetOrSaveTheField("Aritma Praha / tráva"))
                .Returns(RepositoryResult<Field>.Success(field));
            _mockAdminRepo.Setup(x => x.GetOrSaveTheTeam("10A0141", "TJ Sokol Královice, z.s."))
                .Returns(RepositoryResult<Team>.Success(homeTeam));
            _mockAdminRepo.Setup(x => x.GetOrSaveTheTeam("1080021", "Sportovní klub Dolní Chabry, z.s."))
                .Returns(RepositoryResult<Team>.Success(awayTeam));

            // Act
            var result = _adminService.ProccessDtosToMatches(matchDtos, "Test admin");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.GetDataOrThrow());
        }

        [Fact]
        public void ProccessDtosToMatches_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var matchDtos = new List<UnfilledMatchDto>
            {
                 new UnfilledMatchDto
                (
                    numberMatch : "2023110A1A0106",
                    gameField : "Aritma Praha / tráva",
                    idHomeRaw : "10A0141",
                    nameHome : "TJ Sokol Královice, z.s.",
                    idAwayRaw : "1080021",
                    nameAway : "Sportovní klub Dolní Chabry, z.s.",
                    dateOfGame : DateTime.Now.AddDays(1)
                )
            };

            _mockAdminRepo.Setup(x => x.DoesCompetitionExist(It.IsAny<string>()))
                .Returns(RepositoryResult<Models.Competition>.Failure("chyba"));

            var exception = Assert.Throws<InvalidOperationException>(() =>
               _adminService.ProccessDtosToMatches(matchDtos, "user").GetDataOrThrow());

            Assert.Equal("Nepodařilo se spracovat zápasy!", exception.Message);
        }

        #endregion

        #region GetRefereeMatchStatsAsync Tests

        [Fact]
        public async void GetRefereeMatchStatsAsync_WithMainAndAssistantVetoReferee_ReturnsCorrectStats()
        {
            // Arrange
            var request = new RefereesTeamsMatchesRequestDto
            {
                TeamId = "10A0021",
                CompetitionId = "2023110A1A",
                RefereeIds = new List<int> { 9,10,11, 12 },
                IsReferee = true
            };
            var requestAr = new RefereesTeamsMatchesRequestDto
            {
                TeamId = "10A0021",
                CompetitionId = "2023110A1A",
                RefereeIds = new List<int> { 9,10,11, 12 },
                IsReferee = false
            };

            var matches = new List<Models.Match>
            {
                new Models.Match
                {
                    MatchId = "2023110A1A0104",
                    HomeTeamId = "10A0021",
                    AwayTeamId = "1030031",
                    RefereeId = 9,
                    Ar1Id = 15,
                    Ar2Id = 11,
                    MatchDate = new DateOnly(2025, 5, 17),
                    MatchTime = new TimeOnly(15, 0),
                    Competition = new Competition {CompetitionId = "2023110A1A", MatchLength = 45 }
                },
               new Models.Match
                {
                    MatchId = "2023110A1A0104",
                    HomeTeamId = "10A0021",
                    AwayTeamId = "1030031",
                    RefereeId = 10,
                    MatchDate = new DateOnly(2025, 5, 17),
                    MatchTime = new TimeOnly(15, 0),
                    Competition = new Competition {CompetitionId = "2023110A1A", MatchLength = 45 }
                },
               new Models.Match
                {
                    MatchId = "2023110A1A0104",
                    HomeTeamId = "1030031",
                    AwayTeamId = "10A0021",
                    RefereeId = 11,
                    Ar1Id = 15,
                    Ar2Id = 9,
                    MatchDate = new DateOnly(2025, 5, 17),
                    MatchTime = new TimeOnly(15, 0),
                    Competition = new Competition { CompetitionId = "2023110A1A",MatchLength = 45 }
                }
            };

            _mockAdminRepo.Setup(x => x.GetTeamsPureMatchesAsync("10A0021", "2023110A1A"))
                .ReturnsAsync(RepositoryResult<List<Models.Match>>.Success(matches));
            _mockAdminRepo.Setup(x => x.DoesVetoExistForTeam("10A0021", "2023110A1A", 12))
                .Returns(RepositoryResult<bool>.Success(true));
            _mockAdminRepo.Setup(x => x.DoesVetoExistForTeam("10A0021", "2023110A1A", 9))
                .Returns(RepositoryResult<bool>.Success(false));
            _mockAdminRepo.Setup(x => x.DoesVetoExistForTeam("10A0021", "2023110A1A", 10))
                .Returns(RepositoryResult<bool>.Success(false));
            _mockAdminRepo.Setup(x => x.DoesVetoExistForTeam("10A0021", "2023110A1A", 11))
                .Returns(RepositoryResult<bool>.Success(false));

            // Act
            var result = await _adminService.GetRefereeMatchStatsAsync(request);
            var resultAr = await _adminService.GetRefereeMatchStatsAsync(requestAr);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.GetDataOrThrow().Count);
            Assert.True(resultAr.IsSuccess);
            Assert.Equal(4, resultAr.GetDataOrThrow().Count);

            var referee9Stats = result.GetDataOrThrow().First(x => x.RefereeId == 9);
            Assert.Equal(1, referee9Stats.HomeCount);
            Assert.Equal(0, referee9Stats.AwayCount);

            var referee9StatsAr = resultAr.GetDataOrThrow().First(x => x.RefereeId == 9);
            Assert.Equal(0, referee9StatsAr.HomeCount);
            Assert.Equal(1, referee9StatsAr.AwayCount);

            var referee10Stats = result.GetDataOrThrow().First(x => x.RefereeId == 10);
            Assert.Equal(1, referee10Stats.HomeCount);
            Assert.Equal(0, referee10Stats.AwayCount);

            var referee10StatsAr = resultAr.GetDataOrThrow().First(x => x.RefereeId == 10);
            Assert.Equal(0, referee10StatsAr.HomeCount);
            Assert.Equal(0, referee10StatsAr.AwayCount);

            var referee11Stats = result.GetDataOrThrow().First(x => x.RefereeId == 11);
            Assert.Equal(0, referee11Stats.HomeCount);
            Assert.Equal(1, referee11Stats.AwayCount);

            var referee11StatsAr = resultAr.GetDataOrThrow().First(x => x.RefereeId == 11);
            Assert.Equal(1, referee11StatsAr.HomeCount);
            Assert.Equal(0, referee11StatsAr.AwayCount);

            var referee12Stats = result.GetDataOrThrow().First(x => x.RefereeId == 12);
            Assert.Equal(6, referee12Stats.HomeCount);
            Assert.Equal(6, referee12Stats.AwayCount);

            var referee12StatsAr = resultAr.GetDataOrThrow().First(x => x.RefereeId == 12);
            Assert.Equal(6, referee12StatsAr.HomeCount);
            Assert.Equal(6, referee12StatsAr.AwayCount);
        }

        [Fact]
        public async void GetRefereeMatchStatsAsync_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var request = new RefereesTeamsMatchesRequestDto();

            _mockAdminRepo.Setup(x => x.GetTeamsPureMatchesAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _adminService.GetRefereeMatchStatsAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Nepodařilo se získat data rozhodčích o delegovaných zápasů týmů!", result.ErrorMessage);
        }

        #endregion

        #region MakeConnectionsOfMatches Tests

        [Fact]
        public void MakeConnectionsOfMatches_ConsecutiveMatchesSameField_CreatesConnections()
        {
            // Arrange
            var matchViewModels = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(16, 30),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League B"
                }
            };


            // Act
            var result = _adminService.MakeConnectionsOfMatches(matchViewModels);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("2023110A1A0105", result.GetDataOrThrow()[0].Match.PostMatch);
            Assert.Equal("2023110A1A0106", result.GetDataOrThrow()[1].Match.PreMatch);
        }

        [Fact]
        public void MakeConnectionsOfMatches_DifferentFields_NoConnections()

        {
            // Arrange
            var matchViewModels = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(20, 00),
                        RefereeId = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                }
            };


            // Act
            var result = _adminService.MakeConnectionsOfMatches(matchViewModels);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Data[2].Match.PostMatch);
            Assert.Null(result.Data[2].Match.PreMatch);
        }

        [Fact]
        public void MakeConnectionsOfMatches_TooLongGap_NoConnections()
        {
            // Arrange
            var matchViewModels = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 45),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2025, 5, 18),
                        MatchTime = new TimeOnly(16, 00),
                        RefereeId = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                }
            };


            // Act
            var result = _adminService.MakeConnectionsOfMatches(matchViewModels);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Data[0].Match.PostMatch);
            Assert.Null(result.Data[1].Match.PreMatch);
        }

        [Fact]
        public void MakeConnectionsOfMatches_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                     Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                }
            };

            // Act
            // Act
            var result = _adminService.MakeConnectionsOfMatches(matches);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Data[0].Match.PostMatch);
            Assert.Null(result.Data[0].Match.PreMatch);
        }

        #endregion

        #region GetPercentageOfDelegatedMatches Tests

        [Fact]
        public void GetPercentageOfDelegatedMatches_AllDelegated_Returns100()
        {
            // Arrange
            var matchViewModels = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        RefereeId = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                         RefereeId = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2025, 5, 18),
                        MatchTime = new TimeOnly(16, 00),
                        RefereeId = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                }
            };


            // Act
            var result = _adminService.GetPercentageOfDelegatedMatches(matchViewModels);

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void GetPercentageOfDelegatedMatches_NoneDelegated_Returns0()
        {
            // Arrange
            var matchViewModels = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2025, 5, 18),
                        MatchTime = new TimeOnly(16, 00),
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                }
            };


            // Act
            var result = _adminService.GetPercentageOfDelegatedMatches(matchViewModels);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetPercentageOfDelegatedMatches_PartiallyDelegated_ReturnsCorrectPercentage()
        {
            // Arrange
            var matchViewModels = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                   Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                         RefereeId = 1,
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2025, 5, 18),
                        MatchTime = new TimeOnly(16, 00),
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2025, 5, 18),
                        MatchTime = new TimeOnly(16, 00),
                        RefereeId = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0),
                        Competition = new Competition
                        {
                            MatchLength = 45
                        }
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                }
            };


            // Act
            var result = _adminService.GetPercentageOfDelegatedMatches(matchViewModels);

            // Assert
            Assert.Equal(50, result);
        }

        [Fact]
        public void GetPercentageOfDelegatedMatches_EmptyList_Returns0()
        {
            // Arrange
            var matches = new List<MatchViewModel>();

            // Act
            var result = _adminService.GetPercentageOfDelegatedMatches(matches);

            // Assert
            Assert.Equal(0, result);
        }

        #endregion

        #region SortMatches Tests

        [Fact]
        public void SortMatches_ValidSortKey_ReturnsSuccess()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 18),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Another Team",
                    AwayTeamName = "Another Away Team",
                    FieldName = "Another Field",
                    CompetitionName = "League B"
                }
            };

            // Act
            var result = _adminService.SortMatches("sortByGameTimeAsc", matches);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public void SortMatches_InvalidSortKey_ReturnsDefaultSort()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                }
            };

            // Act
            var result = _adminService.SortMatches("invalidSortKey", matches);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Data);
        }

        [Fact]
        public void SortMatches_ExceptionThrown_ReturnsFailure()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = null,
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                }
            };

            // Act
            var result = _adminService.SortMatches("sortByGameTimeAsc", matches);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Chyba při třídění zápasů", result.ErrorMessage);
        }

        #endregion

        #region Sorting Methods Tests

        [Fact]
        public void SortByField_Ascending_SortsCorrectly()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "ZZZZ Field",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Another Team",
                    AwayTeamName = "Another Away Team",
                    FieldName = "AAAA Field",
                    CompetitionName = "League B"
                }
            };

            // Act
            var result = _adminService.SortByField(true, matches);

            // Assert
            Assert.Equal("AAAA Field", result[0].FieldName);
            Assert.Equal("ZZZZ Field", result[1].FieldName);
        }

        [Fact]
        public void SortByGameTime_Ascending_SortsCorrectly()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 18), // Later date
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17), // Earlier date
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Another Team",
                    AwayTeamName = "Another Away Team",
                    FieldName = "Another Field",
                    CompetitionName = "League B"
                }
            };

            // Act
            var result = _adminService.SortByGameTime(true, matches);

            // Assert
            Assert.Equal(new DateOnly(2025, 5, 17), result[0].Match.MatchDate);
            Assert.Equal(new DateOnly(2025, 5, 18), result[1].Match.MatchDate);
        }

        [Fact]
        public void SortByHomeTeam_Ascending_SortsCorrectly()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "ZZZ Team",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "AAA Team",
                    AwayTeamName = "Another Away Team",
                    FieldName = "Another Field",
                    CompetitionName = "League B"
                }
            };

            // Act
            var result = _adminService.SortByHomeTeam(true, matches);

            // Assert
            Assert.Equal("AAA Team", result[0].HomeTeamName);
            Assert.Equal("ZZZ Team", result[1].HomeTeamName);
        }

        [Fact]
        public void SortByAwayTeam_Ascending_SortsCorrectly()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "ZZZ Away Team",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Another Team",
                    AwayTeamName = "AAA Away Team",
                    FieldName = "Another Field",
                    CompetitionName = "League B"
                }
            };

            // Act
            var result = _adminService.SortByAwayTeam(true, matches);

            // Assert
            Assert.Equal("AAA Away Team", result[0].AwayTeamName);
            Assert.Equal("ZZZ Away Team", result[1].AwayTeamName);
        }

        [Fact]
        public void SortByCategory_WithAmpersand_ExtractsCorrectPart()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League&Category B"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Another Team",
                    AwayTeamName = "Another Away Team",
                    FieldName = "Another Field",
                    CompetitionName = "League&Category A"
                }
            };

            // Act
            var result = _adminService.SortByCategory(true, matches);

            // Assert
            Assert.Equal("League&Category A", result[0].CompetitionName);
            Assert.Equal("League&Category B", result[1].CompetitionName);
        }

        [Fact]
        public void SortByUndelegatedMatches_SortsByRefereeCount()
        {
            // Arrange
            var matches = new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0106",
                        HomeTeamId = "10A0141",
                        AwayTeamId = "1080021",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(14, 0),
                        RefereeId = 1,
                        Ar1Id = 2,
                        Ar2Id = 3, // 3 referees
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0105",
                        HomeTeamId = "10A0142",
                        AwayTeamId = "1080022",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(17, 0),
                        RefereeId = 1, // 1 referee
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Another Team",
                    AwayTeamName = "Another Away Team",
                    FieldName = "Another Field",
                    CompetitionName = "League B"
                },
                new MatchViewModel
                {
                    Match = new Models.Match
                    {
                        MatchId = "2023110A1A0108",
                        HomeTeamId = "10A0143",
                        AwayTeamId = "1080023",
                        MatchDate = new DateOnly(2025, 5, 17),
                        MatchTime = new TimeOnly(19, 0),
                        // No referees assigned
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0),
                        Competition = new Competition { MatchLength = 45 }
                    },
                    HomeTeamName = "Third Team",
                    AwayTeamName = "Third Away Team",
                    FieldName = "Third Field",
                    CompetitionName = "League C"
                }
            };

            // Act
            var result = _adminService.SortByUndelegatedMatches(matches);

            // Assert
            Assert.Null(result[0].Match.RefereeId); // No referees first
            Assert.NotNull(result[1].Match.RefereeId); // 1 referee second
            Assert.Equal(3, new[] { result[2].Match.RefereeId, result[2].Match.Ar1Id, result[2].Match.Ar2Id }.Count(id => id != null)); // 3 referees last
        }
        #endregion

    }
}
