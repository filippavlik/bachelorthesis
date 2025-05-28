using AdminPartDevelop.DTOs;
using AdminPartDevelop.Models;
using AdminPartDevelop.Services.FileParsers;
using AdminPartDevelop.Views.ViewModels;
using Aspose.Cells;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPartDevelop.Tests.ServicesTests.FileParsersTests
{
    public class ExportDataTest
    {
        private readonly Mock<ILogger<ExportData>> _loggerMock;
        private readonly ExportData _exporter;

        public ExportDataTest()
        {
            _loggerMock = new Mock<ILogger<ExportData>>();
            _exporter = new ExportData(_loggerMock.Object);
        }

        [Fact]
        public void GenerateMatchExcel_WhenValidData_ReturnsExcelBytes()
        {
            // Arrange
            var matches = CreateTestMatches();
            var refereeInfo = CreateRefereeInfoDictionary();

            // Act
            var result = _exporter.GenerateMatchExcel(matches, refereeInfo);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0);

            // Verify the Excel content
            using var stream = new MemoryStream(result.Data);
            var workbook = new Workbook(stream);
            var worksheet = workbook.Worksheets[0];

            // Verify headers
            var expectedHeaders = new[]
            {
                "Číslo zápasu", "Domací", "Hosté", "Datum zápasu", "HR",
                "AR1", "AR2", "4R", "DS", "DT",
                "VAR", "AVAR", "Hřiště", "competition", "already_played", "locked", "last_changed_by", "last_changed"
            };

            for (int i = 0; i < expectedHeaders.Length; i++)
            {
                Assert.Equal(expectedHeaders[i], worksheet.Cells[0, i].StringValue);
            }

            // Verify data rows (check just the first row)
            Assert.Equal("2023110A1A0104", worksheet.Cells[1, 0].StringValue); // Match ID
            Assert.Equal("10A0021 - ČAFC Praha z.s.", worksheet.Cells[1, 1].StringValue); // Home team
            Assert.Equal("1030031 - FK VIKTORIA ŽIŽKOV a.s.", worksheet.Cells[1, 2].StringValue); // Away team
            Assert.Contains("04.08.2023 18:00", worksheet.Cells[1, 3].StringValue); // Match date and time
            Assert.Equal("John Doe (01081878)", worksheet.Cells[1, 4].StringValue); // Referee name
            Assert.Equal("ČAFC  T1. - Záběhlice", worksheet.Cells[1, 12].StringValue); // field
            Assert.Equal("League A", worksheet.Cells[1, 13].StringValue); // competition
            Assert.Equal("FALSE", worksheet.Cells[1, 14].StringValue); // already_played
            Assert.Equal("FALSE", worksheet.Cells[1, 15].StringValue); // locked
            Assert.Equal("Admin", worksheet.Cells[1, 16].StringValue); // last_changed_by
            Assert.Equal("01.01.2025 10:00", worksheet.Cells[1, 17].StringValue); // last_changed

            Assert.Equal("2023110A3A0108", worksheet.Cells[2, 0].StringValue); // Match ID
            Assert.Equal("1050081 - SC Olympia Radotín z.s.", worksheet.Cells[2, 1].StringValue); // Home team
            Assert.Equal("1050171 - FOTBALOVÝ KLUB FC ZLIČÍN", worksheet.Cells[2, 2].StringValue); // Away team
            Assert.Contains("04.08.2023 19:00", worksheet.Cells[2, 3].StringValue); // Match date and time
            Assert.Equal("Bob Brown (01081880)", worksheet.Cells[2, 4].StringValue); // Referee name
            Assert.Equal("SC RADOTÍN  UMT.", worksheet.Cells[2, 12].StringValue); // field
            Assert.Equal("League B", worksheet.Cells[2, 13].StringValue); // competition
            Assert.Equal("TRUE", worksheet.Cells[2, 14].StringValue); // already_played
            Assert.Equal("TRUE", worksheet.Cells[2, 15].StringValue); // locked
            Assert.Equal("User", worksheet.Cells[2, 16].StringValue); // last_changed_by
            Assert.Equal("01.01.2025 12:00", worksheet.Cells[2, 17].StringValue); // last_changed


            // Logger shouldn't have logged any errors
            VerifyNoErrorsLogged();
        }

        [Fact]
        public void GenerateMatchExcel_WithEmptyRefereeId_DisplaysEmptyString()
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
                        MatchDate = new DateOnly(2025, 1, 1),
                        MatchTime = new TimeOnly(12, 0),
                        RefereeId = null,
                        Ar1Id = null,
                        Ar2Id = null,
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0)
                    },
                    HomeTeamName = "TJ Sokol Královice, z.s.",
                    AwayTeamName = "Sportovní klub Dolní Chabry, z.s.",
                    FieldName = "KRÁLOVICE  T.",
                    CompetitionName = "League A"
                }
            };
            var refereeInfo = CreateRefereeInfoDictionary();

            // Act
            var result = _exporter.GenerateMatchExcel(matches, refereeInfo);

            // Assert
            Assert.True(result.IsSuccess);

            using var stream = new MemoryStream(result.Data);
            var workbook = new Workbook(stream);
            var worksheet = workbook.Worksheets[0];

            Assert.Equal("", worksheet.Cells[1, 4].StringValue); // HR should be empty
            Assert.Equal("", worksheet.Cells[1, 5].StringValue); // AR1 should be empty
            Assert.Equal("", worksheet.Cells[1, 6].StringValue); // AR2 should be empty
        }

        [Fact]
        public void GenerateMatchExcel_WithRefereeWithoutNickname_FormatsCorrectly()
        {
            // Arrange
            var matches = CreateTestMatches();
            var refereeInfo = new Dictionary<int, Tuple<string, string>>
            {
                { 1, new Tuple<string, string>("John Doe", "") }, // Empty nickname
                { 2, new Tuple<string, string>("Jane Smith", null) }, // Null nickname
                { 3, new Tuple<string, string>("Bob Brown", "0057871254") }
            };

            // Act
            var result = _exporter.GenerateMatchExcel(matches, refereeInfo);

            // Assert
            Assert.True(result.IsSuccess);

            // Verify referee formatting
            using var stream = new MemoryStream(result.Data);
            var workbook = new Workbook(stream);
            var worksheet = workbook.Worksheets[0];

            Assert.Equal("John Doe ()", worksheet.Cells[1, 4].StringValue);
            Assert.Equal("Jane Smith ()", worksheet.Cells[1, 5].StringValue);
            Assert.Equal("Bob Brown (0057871254)", worksheet.Cells[1, 6].StringValue);
            Assert.Equal("Bob Brown (0057871254)", worksheet.Cells[2, 4].StringValue);
            Assert.Equal("John Doe ()", worksheet.Cells[2, 5].StringValue);
            Assert.Equal("Jane Smith ()", worksheet.Cells[2, 6].StringValue);

        }

        [Fact]
        public void GenerateMatchExcel_WhenExceptionOccurs_ReturnsFailureResult()
        {
            List<MatchViewModel> matches = null;
            var refereeInfo = CreateRefereeInfoDictionary();

            var result = _exporter.GenerateMatchExcel(matches, refereeInfo);

            Assert.False(result.IsSuccess);
            Assert.Equal("Nepodařilo se vytvořit soubor", result.ErrorMessage);

            // Verify that error was logged
            VerifyErrorWasLogged();
        }

        private List<MatchViewModel> CreateTestMatches()
        {
            return new List<MatchViewModel>
            {
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A1A0104",
                        HomeTeamId = "10A0021",
                        AwayTeamId = "1030031",
                        MatchDate = new DateOnly(2023, 8, 4),
                        MatchTime = new TimeOnly(18, 0),
                        RefereeId = 1,
                        Ar1Id = 2,
                        Ar2Id = 3,
                        AlreadyPlayed = false,
                        Locked = false,
                        LastChangedBy = "Admin",
                        LastChanged = new DateTime(2025, 1, 1, 10, 0, 0)
                    },
                    HomeTeamName = "ČAFC Praha z.s.",
                    AwayTeamName = "FK VIKTORIA ŽIŽKOV a.s.",
                    FieldName = "ČAFC  T1. - Záběhlice",
                    CompetitionName = "League A"
                },
                new MatchViewModel
                {
                    Match = new AdminPartDevelop.Models.Match
                    {
                        MatchId = "2023110A3A0108",
                        HomeTeamId = "1050081",
                        AwayTeamId = "1050171",
                        MatchDate = new DateOnly(2023, 8, 4),
                        MatchTime = new TimeOnly(19, 00),
                        RefereeId = 3,
                        Ar1Id = 1,
                        Ar2Id = 2,
                        AlreadyPlayed = true,
                        Locked = true,
                        LastChangedBy = "User",
                        LastChanged = new DateTime(2025, 1, 1, 12, 0, 0)
                    },
                    HomeTeamName = "SC Olympia Radotín z.s.",
                    AwayTeamName = " FOTBALOVÝ KLUB FC ZLIČÍN",
                    FieldName = "SC RADOTÍN  UMT.",
                    CompetitionName = "League B"
                }
            };
        }

        private Dictionary<int, Tuple<string, string>> CreateRefereeInfoDictionary()
        {
            return new Dictionary<int, Tuple<string, string>>
            {
                { 1, new Tuple<string, string>("John Doe", "01081878") },
                { 2, new Tuple<string, string>("Jane Smith", "01081879") },
                { 3, new Tuple<string, string>("Bob Brown", "01081880") }
            };
        }

        private void VerifyNoErrorsLogged()
        {
            _loggerMock.Verify(
                x => x.Log(
                    Microsoft.Extensions.Logging.LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        private void VerifyErrorWasLogged()
        {
            _loggerMock.Verify(
                x => x.Log(
                    Microsoft.Extensions.Logging.LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

    }
}
