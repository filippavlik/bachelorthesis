using AdminPartDevelop.Services.FileParsers;
using AdminPartDevelop.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AdminPartDevelop.Tests.Services
{
    public class MatchesDataTests
    {
        private readonly Mock<ILogger<GetData>> _loggerMock;
        private readonly GetData _getDataService;
        private readonly string _testFilesPath;

        public MatchesDataTests()
        {
            _loggerMock = new Mock<ILogger<GetData>>();
            _getDataService = new GetData(_loggerMock.Object);

            // Setup path to test files
            _testFilesPath = Path.Combine(Path.GetTempPath(), "AdminPartTests", "TestFiles");
            Directory.CreateDirectory(_testFilesPath); // Ensure directory exists
        }

        [Fact]
        public async Task GetMatchesDataAsync_ValidFile_ReturnsCorrectData()
        {
            // Arrange
            string testFilePath = await TestExcelGenerator.CreateMatchesExcelFile(_testFilesPath);

            // Act
            var result = await _getDataService.GetMatchesDataAsync(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.Data.Count);

            // Check first match
            var match1 = result.Data[0];
            Assert.Equal("2023001", match1.NumberMatch);
            Assert.Equal("1001", match1.IdHomeRaw);
            Assert.Equal("SK Slavia", match1.NameHome);
            Assert.Equal("1002", match1.IdAwayRaw);
            Assert.Equal("AC Sparta", match1.NameAway);
            Assert.Equal("Eden Arena", match1.GameField);

            // Check date with Czech culture format
            var czechCulture = new System.Globalization.CultureInfo("cs-CZ");
            var expectedDate = DateTime.Parse("15.08.2023 18:00", czechCulture);
            Assert.Equal(expectedDate, match1.DateOfGame);

            // Check second match
            var match2 = result.Data[1];
            Assert.Equal("2023002", match2.NumberMatch);
            Assert.Equal("1003", match2.IdHomeRaw);
            Assert.Equal("FK Viktoria", match2.NameHome);
            Assert.Equal("1004", match2.IdAwayRaw);
            Assert.Equal("FC Baník", match2.NameAway);
            Assert.Equal("Doosan Arena", match2.GameField);
        }

        [Fact]
        public async Task GetPlayedMatchesDataAsync_ValidFile_ReturnsCorrectData()
        {
            // Arrange
            string testFilePath = await TestExcelGenerator.CreatePlayedMatchesExcelFile(_testFilesPath);

            // Act
            var result = await _getDataService.GetPlayedMatchesDataAsync(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.Data.Count);

            // Check first match
            var match1 = result.Data[0];
            Assert.Equal("2023001", match1.NumberMatch);
            Assert.Equal("1001", match1.IdOfReferee);
            Assert.Equal("Jan", match1.NameOfReferee);
            Assert.Equal("Novák", match1.SurnameOfReferee);
            Assert.Equal("1002", match1.IdOfAr1);
            Assert.Equal("Petr", match1.NameOfAr1);
            Assert.Equal("Svoboda", match1.SurnameOfAr1);
            Assert.Equal("1003", match1.IdOfAr2);
            Assert.Equal("Martin", match1.NameOfAr2);
            Assert.Equal("Dvořák", match1.SurnameOfAr2);

            // Check second match
            var match2 = result.Data[1];
            Assert.Equal("2023002", match2.NumberMatch);
            Assert.Equal("1004", match2.IdOfReferee);
            Assert.Equal("Tomáš", match2.NameOfReferee);
            Assert.Equal("Procházka", match2.SurnameOfReferee);
            Assert.Equal("1001", match2.IdOfAr1);
            Assert.Equal("Jan", match2.NameOfAr1);
            Assert.Equal("Novák", match2.SurnameOfAr1);
        }

        [Fact]
        public async Task GetMatchesDataAsync_EmptyFile_ReturnsFailure()
        {
            // Arrange
            // Create an empty workbook
            var workbook = new Aspose.Cells.Workbook();
            string emptyFilePath = Path.Combine(_testFilesPath, "empty_matches.xlsx");
            workbook.Save(emptyFilePath);

            // Act
            var result = await _getDataService.GetMatchesDataAsync(emptyFilePath);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Tenhle soubor neobsahuje žádné zápasy!", result.ErrorMessage);
        }

    }
}