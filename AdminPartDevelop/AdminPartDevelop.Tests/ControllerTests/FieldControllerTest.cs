using AdminPartDevelop.Controllers;
using AdminPartDevelop.Data;
using AdminPartDevelop.Models;
using AdminPartDevelop.DTOs;
using AdminPartDevelop.Services.FileParsers;
using AdminPartDevelop.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AdminPartDevelop.TestData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AdminPartDevelop.Tests.Controllers
{
    public class FieldControllerFunctionalTests : IDisposable
    {
        private readonly AdminDbContext _adminContext;
        private readonly RefereeDbContext _refereeContext;
        private readonly FieldController _controller;
        private readonly AdminRepo _adminRepo;
        private readonly RefereeRepo _refereeRepo;
        private readonly Mock<IExcelParser> _mockExcelParser;

        public FieldControllerFunctionalTests()
        {
            var optionsAdmin = new DbContextOptionsBuilder<AdminDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var optionsReferee = new DbContextOptionsBuilder<RefereeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _adminContext = new AdminDbContext(optionsAdmin);
            _refereeContext = new RefereeDbContext(optionsReferee);

            // Seed database with mock data
            SeedDatabase();

            // Create mocks
            var mockFieldControllerLogger = new Mock<ILogger<FieldController>>();
            var mockAdminRepoLogger = new Mock<ILogger<AdminRepo>>();
            var mockRefereeRepoLogger = new Mock<ILogger<RefereeRepo>>();
            _mockExcelParser = new Mock<IExcelParser>();

            // Create real repositories
            _adminRepo = new AdminRepo(mockAdminRepoLogger.Object, _adminContext);
            _refereeRepo = new RefereeRepo(mockRefereeRepoLogger.Object, _refereeContext);

            // Create controller with dependencies
            _controller = new FieldController(
                _refereeRepo,
                _adminRepo,
                _mockExcelParser.Object,
                mockFieldControllerLogger.Object
            );
        }

        private void SeedDatabase()
        {
            _adminContext.StartingGameDates.AddRange(MockedDbTestData.GetStartingGameDates());
            _adminContext.Competitions.AddRange(MockedDbTestData.GetCompetitions());
            _adminContext.Fields.AddRange(MockedDbTestData.GetFields());
            _adminContext.Teams.AddRange(MockedDbTestData.GetTeams());
            _refereeContext.Referees.AddRange(MockedDbTestData.GetReferees());
            _adminContext.Matches.AddRange(MockedDbTestData.GetMatches());
            _adminContext.Transfers.AddRange(MockedDbTestData.GetTransfers());
            _refereeContext.Excuses.AddRange(MockedDbTestData.GetExcuses());
            _refereeContext.VehicleSlots.AddRange(MockedDbTestData.GetVehicleSlots());
            _adminContext.Vetoes.AddRange(MockedDbTestData.GetVetoes());
            _adminContext.FilesPreviousDelegations.AddRange(MockedDbTestData.GetFilesPreviousDelegation());
            _adminContext.SaveChanges();
            _refereeContext.SaveChanges();
        }

        [Fact]
        public async Task GetPreviewOfFields_ReturnsPartialViewWithFields()
        {
            // Act
            var result = await _controller.GetPreviewOfFields();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("~/Views/PartialViews/_FieldsTable.cshtml", partialViewResult.ViewName);

            var model = Assert.IsAssignableFrom<IEnumerable<Field>>(partialViewResult.Model);
            Assert.NotEmpty(model);
        }

        [Fact]
        public async Task UploadFieldsInformationsFromFileAsync_WithValidFile_ReturnsOkResult()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var mockFilePath = "/temp/test.xlsx";
            var filledMockFields = new List<FilledFieldDto>
            { 
                new FilledFieldDto("Test Field name 2","Test Field address 2",50.01f,14.01f)
            };

            var mockResponse = ServiceResult<string>.Success(mockFilePath);
            var mockFieldsResponse = ServiceResult<List<FilledFieldDto>>.Success(filledMockFields);
            var mockUpdateResponse = new RepositoryResponse { Success = true, Message = "Fields updated successfully" }
            ;

            _mockExcelParser.Setup(x => x.SaveAndValidateFileAsync(It.IsAny<IFormFile>()))
                           .ReturnsAsync(mockResponse);
            _mockExcelParser.Setup(x => x.GetInformationsAboutFields(mockFilePath))
                           .ReturnsAsync(mockFieldsResponse);

            // Act
            var result = await _controller.UploadFieldsInformationsFromFileAsync(mockFile);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify the mocks were called
            _mockExcelParser.Verify(x => x.SaveAndValidateFileAsync(mockFile), Times.Once);
            _mockExcelParser.Verify(x => x.GetInformationsAboutFields(mockFilePath), Times.Once);
        }

        [Fact]
        public async Task UploadFieldsInformationsFromFileAsync_WithInvalidFile_ReturnsServerError()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.txt", "text/plain");
            var mockResponse = ServiceResult<string>.Failure("Invalid file format");

            _mockExcelParser.Setup(x => x.SaveAndValidateFileAsync(It.IsAny<IFormFile>()))
                           .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.UploadFieldsInformationsFromFileAsync(mockFile);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UploadFieldsInformationsFromFileAsync_ExceptionThrown_ReturnsServerError()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            _mockExcelParser.Setup(x => x.SaveAndValidateFileAsync(It.IsAny<IFormFile>()))
                           .ThrowsAsync(new Exception("File processing error"));

            // Act
            var result = await _controller.UploadFieldsInformationsFromFileAsync(mockFile);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Nastala chyba při nahrávání informací o hřištích na server.", statusCodeResult.Value.ToString());
        }

        [Fact]
        public void UpdateSingleField_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var existingField = _adminContext.Fields.First();
            int fieldId = existingField.FieldId;
            string fieldName = "Aritma Praha / tráva";
            string fieldAddress = "Nad Lávkou 5, 16000 Praha 6";
            float latitude = 50.123f;
            float longitude = 14.456f;

            // Act
            var result = _controller.UpdateSingleField(fieldId, fieldName, fieldAddress, latitude, longitude);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify the field was actually updated in the database
            var updatedField = _adminContext.Fields.Find(fieldId);
            Assert.NotNull(updatedField);
            Assert.Equal(fieldName, updatedField.FieldName);
            Assert.Equal(fieldAddress, updatedField.FieldAddress);
            Assert.Equal(latitude, updatedField.Latitude);
            Assert.Equal(longitude, updatedField.Longitude);
        }

        [Fact]
        public void UpdateSingleField_WithInvalidFieldId_ReturnsServerError()
        {
            // Arrange
            int invalidFieldId = 9999;
            string fieldName = "Test Field";
            string fieldAddress = "Test Address";
            float latitude = 50.0f;
            float longitude = 14.0f;

            // Act
            var result = _controller.UpdateSingleField(invalidFieldId, fieldName, fieldAddress, latitude, longitude);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public void UpdateSingleField_DatabaseExceptionHandling_ReturnsServerError()
        {
            // Arrange
            var existingField = _adminContext.Fields.First();
            int fieldId = existingField.FieldId;
            _adminContext.Dispose(); // Simulate database connection issue

            // Act
            var result = _controller.UpdateSingleField(fieldId, "Test", "Test Address", 50.0f, 14.0f);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Chyba při procesu aktualizace hřišť!", statusCodeResult.Value.ToString());
        }

        [Fact]
        public async Task UploadFieldsInformationsFromFileAsync_InvalidOperationException_ReturnsServerError()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            _mockExcelParser.Setup(x => x.SaveAndValidateFileAsync(It.IsAny<IFormFile>()))
                           .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.UploadFieldsInformationsFromFileAsync(mockFile);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Invalid operation", statusCodeResult.Value.ToString());
        }

        [Fact]
        public async Task GetPreviewOfFields_RepositoryFailure_ThrowsException()
        {
            // Arrange - Mock the repository to return a failure
            var mockAdminRepo = new Mock<IAdminRepo>();
            mockAdminRepo.Setup(x => x.GetFields())
                        .ReturnsAsync(RepositoryResult<List<Field>>.Failure("Database error"));

            var controller = new FieldController(
                _refereeRepo,
                mockAdminRepo.Object,
                _mockExcelParser.Object,
                Mock.Of<ILogger<FieldController>>()
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetPreviewOfFields());
        }

        [Fact]
        public async Task UploadFieldsInformationsFromFileAsync_RepositoryUpdateFailure_ReturnsServerError()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var mockFields = new List<Field>
            {
                new Field("Test Field") { FieldId = 100, FieldAddress = "Test Address", Latitude = 50.0f, Longitude = 14.0f }
            };
            var filledMockFields = new List<FilledFieldDto>
            {
                new FilledFieldDto("Test Field name 2","Test Field address 2",50.01f,14.01f)
            };

            var mockResponse = ServiceResult<string>.Success("/temp/test.xlsx");
            var mockFieldsResponse = ServiceResult<List<FilledFieldDto>>.Success(filledMockFields);

            _mockExcelParser.Setup(x => x.SaveAndValidateFileAsync(It.IsAny<IFormFile>()))
                           .ReturnsAsync(mockResponse);
            _mockExcelParser.Setup(x => x.GetInformationsAboutFields("/temp/test.xlsx"))
                           .ReturnsAsync(mockFieldsResponse);
            // Mock the repository to return failure for update
            var mockAdminRepo = new Mock<IAdminRepo>();
            mockAdminRepo.Setup(x => x.UpdateFieldsAsync(It.IsAny<List<FilledFieldDto >>()))
                        .ReturnsAsync(new RepositoryResponse { Success = false, Message = "Update failed" });

            var controller = new FieldController(
                _refereeRepo,
                mockAdminRepo.Object,
                _mockExcelParser.Object,
                Mock.Of<ILogger<FieldController>>()
            );

            // Act
            var result = await controller.UploadFieldsInformationsFromFileAsync(mockFile);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Update failed", statusCodeResult.Value);
        }

        private IFormFile CreateMockFormFile(string fileName, string contentType)
        {
            var content = "Mock file content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
            return formFile;
        }

        public void Dispose()
        {
            _adminContext?.Dispose();
            _refereeContext?.Dispose();
        }
    }
}