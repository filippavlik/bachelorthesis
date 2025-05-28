using AdminPartDevelop.Common;
using AdminPartDevelop.DTOs;
using AdminPartDevelop.Models;
using AdminPartDevelop.Services.FileParsers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Text;

namespace AdminPartDevelop.Tests.Services
{
    public class GetDataTest
    {
        private readonly Mock<ILogger<GetData>> _loggerMock;
        private readonly GetData _getDataService;
        private readonly string _testFilesPath;

        public GetDataTest()
        {
            _loggerMock = new Mock<ILogger<GetData>>();
            _getDataService = new GetData(_loggerMock.Object);

            _testFilesPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "TestFiles");
            Directory.CreateDirectory(_testFilesPath);
        }

        private IFormFile CreateTestExcelFile(string fileName)
        {
            byte[] fileContent = Encoding.UTF8.GetBytes("Test file content");

            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream(fileContent);

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);

            return fileMock.Object;
        }

        [Fact]
        public async Task SaveAndValidateFileAsync_EmptyFile_ReturnsFailure()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _getDataService.SaveAndValidateFileAsync(fileMock.Object);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Soubor je prázdný nebo neexistuje.", result.ErrorMessage);
        }

        [Fact]
        public async Task SaveAndValidateFileAsync_NullFile_ReturnsFailure()
        {
            // Act
            var result = await _getDataService.SaveAndValidateFileAsync(null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Soubor je prázdný nebo neexistuje.", result.ErrorMessage);
        }

        [Fact]
        public async Task SaveAndValidateFileAsync_NonExcelFile_ReturnsFailure()
        {
            // Arrange
            var file = CreateTestExcelFile("test.txt");

            // Act
            var result = await _getDataService.SaveAndValidateFileAsync(file);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Prosím vyberte validní excel soubor!", result.ErrorMessage);
        }

        [Theory]
        [InlineData("test.xls")]
        [InlineData("test.xlsx")]
        public async Task SaveAndValidateFileAsync_ValidExcelFile_ReturnsSuccess(string fileName)
        {
            // Arrange
            var file = CreateTestExcelFile(fileName);

            // Act
            var result = await _getDataService.SaveAndValidateFileAsync(file);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Contains(fileName, result.Data);
        }


        [Fact]
        public async Task GetRefereesDataAsync_ValidFile_ReturnsCorrectReferee()
        {
            string testFilePath = Path.Combine(_testFilesPath, "referees.xlsx");

            var result = await _getDataService.GetRefereesDataAsync(testFilePath);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);

            var referee = result.Data.FirstOrDefault(r => r.Key.Value == "Amler" && r.Key.Key == "Matyáš");
            Assert.NotNull(referee);
        }
     
        [Fact]
        public async Task GetPlayedMatchesDataAsync_ValidFile_ReturnsPlayedMatches()
        {
            // Arrange
            string testFilePath = Path.Combine(_testFilesPath, "played_matches.xls");

            // Act
            var result = await _getDataService.GetPlayedMatchesDataAsync(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetInformationsOfReferees_ValidFile_ReturnsRefereeInfo()
        {
            // Arrange
            string testFilePath = Path.Combine(_testFilesPath, "referee_info.xlsx");

            // Act
            var result = await _getDataService.GetInformationsOfReferees(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetInformationsAboutFields_ValidFile_ContainsCorrectCoordinates()
        {
            string testFilePath = Path.Combine(_testFilesPath, "fields_info.xlsx");

            var result = await _getDataService.GetInformationsAboutFields(testFilePath);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);

            // Validate that a field has correct coordinates
            var field = result.Data.FirstOrDefault(f => f.FieldName == "SLIVENEC  T.");
            Assert.NotNull(field);
            Assert.Equal(50.0207748f, field.FieldLatitude);
            Assert.Equal(14.3569422f, field.FieldLongtitude);
        }
      
        [Fact]
        public async Task GetRefereesDataAsync_FileMissingColumns_ReturnsTrue()
        {
            string testFilePath = Path.Combine(_testFilesPath, "missing_columns_referees.xlsx");

            var result = await _getDataService.GetRefereesDataAsync(testFilePath);

            Assert.True(result.IsSuccess);
        }

 
        [Fact]
        public async Task GetMatchesDataAsync_ValidFile_ParsesAllMatchDetails()
        {
            string testFilePath = Path.Combine(_testFilesPath, "matches.xls");

            var result = await _getDataService.GetMatchesDataAsync(testFilePath);

            Assert.True(result.IsSuccess);

            var culture = new CultureInfo("cs-CZ");
            var match = result.Data.FirstOrDefault(m => m.IdHomeRaw == "1050171" && m.IdAwayRaw == "1040011" && DateOnly.FromDateTime(m.DateOfGame) == DateOnly.Parse("27.08.2023", culture));
            Assert.NotNull(match);
            Assert.Equal("2023110C1A1307", match.NumberMatch);
            Assert.Equal("ZLIČÍN  UMT.", match.GameField);
            Assert.InRange(match.DateOfGame.Year, 2020, 2025);
        }
        [Fact]
        public async Task GetRefereesDataAsync_FileNotFound_ReturnsFailure()
        {
            string testFilePath = Path.Combine(_testFilesPath, "non_existent_file.xlsx");

            var result = await _getDataService.GetRefereesDataAsync(testFilePath);

            Assert.False(result.IsSuccess);
            Assert.Contains("Nepodařilo se načíst data ze souboru", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("50.1234N", 50.1234f)]
        [InlineData("50.1234E", 50.1234f)]
        [InlineData("50.1234S", -50.1234f)]
        [InlineData("50.1234W", -50.1234f)]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("invalid", null)]
        public void ConvertToFloat_ReturnsExpectedValue(string input, float? expected)
        {
            var method = typeof(GetData).GetMethod("ConvertToFloat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = method.Invoke(_getDataService, new object[] { input }) as float?;

            Assert.Equal(expected, result);
        }
    }
}
