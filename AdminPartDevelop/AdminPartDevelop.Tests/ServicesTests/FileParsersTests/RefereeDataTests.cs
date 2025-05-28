using AdminPartDevelop.DTOs;
using AdminPartDevelop.Models;
using AdminPartDevelop.Services.FileParsers;
using AdminPartDevelop.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AdminPartDevelop.Tests.Services
{
    public class RefereesDataTests
    {
        private readonly Mock<ILogger<GetData>> _loggerMock;
        private readonly GetData _getDataService;
        private readonly string _testFilesPath;

        public RefereesDataTests()
        {
            _loggerMock = new Mock<ILogger<GetData>>();
            _getDataService = new GetData(_loggerMock.Object);

            _testFilesPath = Path.Combine(Path.GetTempPath(), "AdminPartTests", "TestFiles");
            Directory.CreateDirectory(_testFilesPath); 
        }

        [Fact]
        public async Task GetRefereesDataAsync_ValidFile_ReturnsCorrectData()
        {
            // Arrange
            string testFilePath = await TestExcelGenerator.CreateRefereesExcelFile(_testFilesPath);

            // Act
            var result = await _getDataService.GetRefereesDataAsync(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);

            // Check specific referee entries
            var novakJan = result.Data[new KeyValuePair<string, string>("Jan", "Novák")];
            if (novakJan is RefereeLevelDto levelDtoNovak)
            {
                Assert.Equal(0, levelDtoNovak.League);
                Assert.Equal(35, levelDtoNovak.Age);
            }
            Assert.NotNull(novakJan);
            Assert.Equal("Jan", novakJan.Name);
            Assert.Equal("Novák", novakJan.Surname);


            var dvorakMartin = result.Data[new KeyValuePair<string, string>("Martin", "Dvořák")];
            if (dvorakMartin is RefereeLevelDto levelDtoDvorak)
            {
                Assert.Equal(1, levelDtoDvorak.League);
                Assert.Equal(28, levelDtoDvorak.Age);
            }
            Assert.NotNull(dvorakMartin);
            Assert.Equal("Martin", dvorakMartin.Name);
            Assert.Equal("Dvořák", dvorakMartin.Surname);          
        }

        [Fact]
        public async Task GetInformationsOfReferees_ValidFile_ReturnsCorrectData()
        {
            // Arrange
            string testFilePath = await TestExcelGenerator.CreateRefereeInfoExcelFile(_testFilesPath);

            // Act
            var result = await _getDataService.GetInformationsOfReferees(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);

            // Check specific referee entries
            var novakJan = result.Data[new KeyValuePair<string, string>("Jan", "Novák")];
            Assert.NotNull(novakJan);
            Assert.Equal("Jan", novakJan.Name);
            Assert.Equal("Novák", novakJan.Surname);
            if (novakJan is FilledRefereeDto filledDtoNovak)
            {
                Assert.Equal("jan.novak@example.com", filledDtoNovak.Email);
                Assert.Equal("1001", filledDtoNovak.FacrId);
                Assert.Equal("123456789", filledDtoNovak.PhoneNumber);
            }
            

            var svobodaPetr = result.Data[new KeyValuePair<string, string>("Petr", "Svoboda")];
            Assert.NotNull(svobodaPetr);
            Assert.Equal("Petr", svobodaPetr.Name);
            Assert.Equal("Svoboda", svobodaPetr.Surname);
            if (svobodaPetr is FilledRefereeDto filledDtoSvoboda)
            {
                Assert.Equal("petr.svoboda@example.com", filledDtoSvoboda.Email);
                Assert.Equal("1002", filledDtoSvoboda.FacrId);
                Assert.Equal("987654321", filledDtoSvoboda.PhoneNumber);
            }
        }

        [Fact]
        public async Task GetInformationsOfReferees_EmptyFile_ReturnsFailure()
        {
            // Arrange
            // Create an empty workbook
            var workbook = new Aspose.Cells.Workbook();
            string emptyFilePath = Path.Combine(_testFilesPath, "empty_referee_info.xlsx");
            workbook.Save(emptyFilePath);

            // Act
            var result = await _getDataService.GetInformationsOfReferees(emptyFilePath);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Tenhle soubor neobsahuje žádné informace!", result.ErrorMessage);
        }

    }
}