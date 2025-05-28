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
    public class FieldsDataTests
    {
        private readonly Mock<ILogger<GetData>> _loggerMock;
        private readonly GetData _getDataService;
        private readonly string _testFilesPath;

        public FieldsDataTests()
        {
            _loggerMock = new Mock<ILogger<GetData>>();
            _getDataService = new GetData(_loggerMock.Object);

            // Setup path to test files
            _testFilesPath = Path.Combine(Path.GetTempPath(), "TestFiles");
            Directory.CreateDirectory(_testFilesPath); // Ensure directory exists
        }

        [Fact]
        public async Task GetInformationsAboutFields_ValidFile_ReturnsCorrectData()
        {
            // Arrange
            string testFilePath = await TestExcelGenerator.CreateFieldsInfoExcelFile(_testFilesPath);

            // Act
            var result = await _getDataService.GetInformationsAboutFields(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.Data.Count);

            // Check first field
            var field1 = result.Data[0];
            Assert.Equal("Eden Arena", field1.FieldName);
            Assert.Equal("U Slavie 1540/2a, 100 00 Praha 10", field1.FieldAddress);
            Assert.Equal(50.0678f, field1.FieldLatitude);
            Assert.Equal(14.4724f, field1.FieldLongtitude);

            // Check second field
            var field2 = result.Data[1];
            Assert.Equal("Doosan Arena", field2.FieldName);
            Assert.Equal("Štruncovy sady 2741/3, 301 00 Plzeň", field2.FieldAddress);
            Assert.Equal(49.7472f, field2.FieldLatitude);
            Assert.Equal(13.3811f, field2.FieldLongtitude);
        }

        [Fact]
        public async Task GetInformationsAboutFields_EmptyFile_ReturnsFailure()
        {
            // Arrange
            // Create an empty workbook
            var workbook = new Aspose.Cells.Workbook();
            string emptyFilePath = Path.Combine(_testFilesPath, "empty_fields.xlsx");
            workbook.Save(emptyFilePath);

            // Act
            var result = await _getDataService.GetInformationsAboutFields(emptyFilePath);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Tenhle soubor neobsahuje žádné hřište!", result.ErrorMessage);
        }

        [Fact]
        public async Task GetInformationsAboutFields_CoordinatesHandledCorrectly()
        {
            var workbook = new Aspose.Cells.Workbook();
            var worksheet = workbook.Worksheets[0];

            // Add headers
            worksheet.Cells["A1"].PutValue("Název hřiště");
            worksheet.Cells["B1"].PutValue("Adresa");
            worksheet.Cells["C1"].PutValue("Zeměpisná šířka");
            worksheet.Cells["D1"].PutValue("Zeměpisná délka");

            worksheet.Cells["A2"].PutValue("Field North");
            worksheet.Cells["B2"].PutValue("North Address");
            worksheet.Cells["C2"].PutValue("50.1234N");
            worksheet.Cells["D2"].PutValue("14.5678E");

            worksheet.Cells["A3"].PutValue("Field South");
            worksheet.Cells["B3"].PutValue("South Address");
            worksheet.Cells["C3"].PutValue("40.1234S");
            worksheet.Cells["D3"].PutValue("15.5678W");

            worksheet.Cells["A4"].PutValue("Field Empty");
            worksheet.Cells["B4"].PutValue("Empty Coordinates");
            worksheet.Cells["C4"].PutValue("");
            worksheet.Cells["D4"].PutValue(null);

            worksheet.Cells["A5"].PutValue("Field Invalid");
            worksheet.Cells["B5"].PutValue("Invalid Coordinates");
            worksheet.Cells["C5"].PutValue("NotACoordinate");
            worksheet.Cells["D5"].PutValue("12345");

            string customFilePath = Path.Combine(_testFilesPath, "custom_fields.xlsx");
            workbook.Save(customFilePath);

            // Act
            var result = await _getDataService.GetInformationsAboutFields(customFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(4, result.Data.Count);

            // Check North coordinates (positive)
            Assert.Equal(50.1234f, result.Data[0].FieldLatitude);
            Assert.Equal(14.5678f, result.Data[0].FieldLongtitude);

            // Check South/West coordinates (negative)
            Assert.Equal(-40.1234f, result.Data[1].FieldLatitude);
            Assert.Equal(-15.5678f, result.Data[1].FieldLongtitude);

            // Check empty coordinates (null)
            Assert.Null(result.Data[2].FieldLatitude);
            Assert.Null(result.Data[2].FieldLongtitude);

            // Check invalid coordinates (null)
            Assert.Null(result.Data[3].FieldLatitude);
            Assert.Null(result.Data[3].FieldLongtitude);
        }

        [Fact]
        public async Task ConvertToFloat_TestPrivateMethod()
        {
            // We need to access the private method via reflection
            var method = typeof(GetData).GetMethod("ConvertToFloat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test with North direction (positive)
            var resultN = method.Invoke(_getDataService, new object[] { "50.1234N" }) as float?;
            Assert.Equal(50.1234f, resultN);

            // Test with East direction (positive)
            var resultE = method.Invoke(_getDataService, new object[] { "14.5678E" }) as float?;
            Assert.Equal(14.5678f, resultE);

            // Test with South direction (negative)
            var resultS = method.Invoke(_getDataService, new object[] { "40.1234S" }) as float?;
            Assert.Equal(-40.1234f, resultS);

            // Test with West direction (negative)
            var resultW = method.Invoke(_getDataService, new object[] { "15.5678W" }) as float?;
            Assert.Equal(-15.5678f, resultW);

            // Test with null input
            var resultNull = method.Invoke(_getDataService, new object[] { null }) as float?;
            Assert.Null(resultNull);

            // Test with empty string
            var resultEmpty = method.Invoke(_getDataService, new object[] { "" }) as float?;
            Assert.Null(resultEmpty);

            // Test with invalid format
            var resultInvalid = method.Invoke(_getDataService, new object[] { "InvalidCoordinate" }) as float?;
            Assert.Null(resultInvalid);

            // Test with numeric value but no direction
            var resultNoDirection = method.Invoke(_getDataService, new object[] { "50.1234" }) as float?;
            Assert.Null(resultNoDirection);
        }
    }
}