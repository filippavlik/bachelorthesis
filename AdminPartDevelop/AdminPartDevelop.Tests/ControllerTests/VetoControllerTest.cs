using AdminPartDevelop.Controllers;
using AdminPartDevelop.Data;
using AdminPartDevelop.Models;
using AdminPartDevelop.TestData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AdminPartDevelop.Tests.Controllers
{
    public class VetoControllerTest : IDisposable
    {
        private readonly AdminDbContext _adminContext;
        private readonly VetoController _controller;
        private readonly AdminRepo _adminRepo;

        public VetoControllerTest()
        {
            var optionsAdmin = new DbContextOptionsBuilder<AdminDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _adminContext = new AdminDbContext(optionsAdmin);

            // Seed database with mock data
            SeedDatabase();

            // Create controller with real services and seeded database
            var mockVetoControllerLogger = new Mock<ILogger<VetoController>>();
            var mockAdminRepoLogger = new Mock<ILogger<AdminRepo>>();

            _adminRepo = new AdminRepo(mockAdminRepoLogger.Object, _adminContext);
            _controller = new VetoController(_adminRepo, mockVetoControllerLogger.Object);
        }

        private void SeedDatabase()
        {
            // Add your existing mock data
            _adminContext.StartingGameDates.AddRange(MockedDbTestData.GetStartingGameDates());
            _adminContext.Competitions.AddRange(MockedDbTestData.GetCompetitions());
            _adminContext.Fields.AddRange(MockedDbTestData.GetFields());
            _adminContext.Teams.AddRange(MockedDbTestData.GetTeams());
            _adminContext.Matches.AddRange(MockedDbTestData.GetMatches());
            _adminContext.Transfers.AddRange(MockedDbTestData.GetTransfers());
            _adminContext.Vetoes.AddRange(MockedDbTestData.GetVetoes());
            _adminContext.FilesPreviousDelegations.AddRange(MockedDbTestData.GetFilesPreviousDelegation());
            _adminContext.SaveChanges();
        }

        [Fact]
        public async Task AddVeto_WithValidData_ReturnsOkResult()
        {
            // Arrange
            int idOfReferee = 4;
            string idOfTeam = "1090181";
            string idOfCompetition = "all";
            string note = "Test veto note";

            // Act
            var result = await _controller.AddVeto(idOfReferee, idOfTeam, idOfCompetition, note);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify the veto was actually added to the database
            var addedVeto = await _adminContext.Vetoes
                .FirstOrDefaultAsync(v => v.RefereeId == idOfReferee &&
                                         v.TeamId == idOfTeam &&
                                         v.CompetitionId == idOfCompetition);
            Assert.NotNull(addedVeto);
            Assert.Equal(note, addedVeto.Note);
        }
        [Fact]
        public async Task AddVeto_WithDuplicateValues_ReturnsOkResult()
        {
            // Arrange
            int idOfReferee = 1;
            string idOfTeam = "1090181";
            string idOfCompetition = "2023110A2B";
            string note = "Test veto note";

            // Act
            var result = await _controller.AddVeto(idOfReferee, idOfTeam, idOfCompetition, note);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }      

        [Fact]
        public void UpdateVeto_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var existingVeto = _adminContext.Vetoes.First();
            int vetoId = existingVeto.VetoId;
            string newNote = "Updated note";

            // Act
            var result = _controller.UpdateVeto(vetoId, newNote);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify the veto was actually updated in the database
            var updatedVeto = _adminContext.Vetoes.Where(v => v.VetoId==vetoId).FirstOrDefault();
            Assert.NotNull(updatedVeto);
            Assert.Equal(newNote, updatedVeto.Note);
        }

        [Fact]
        public void UpdateVeto_WithInvalidId_ReturnsServerError()
        {
            // Arrange
            int invalidVetoId = 9999; // Non-existent ID
            string newNote = "Updated note";

            // Act
            var result = _controller.UpdateVeto(invalidVetoId, newNote);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public void DeleteVeto_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var existingVeto = _adminContext.Vetoes.First();
            int vetoId = existingVeto.VetoId;

            // Act
            var result = _controller.DeleteVeto(vetoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify the veto was actually deleted from the database
            var deletedVeto = _adminContext.Vetoes.Where(v => v.VetoId == vetoId).FirstOrDefault();
            Assert.Null(deletedVeto);
        }

        [Fact]
        public void DeleteVeto_WithInvalidId_ReturnsServerError()
        {
            // Arrange
            int invalidVetoId = 9999; // Non-existent ID

            // Act
            var result = _controller.DeleteVeto(invalidVetoId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AddVeto_DatabaseExceptionHandling_ReturnsServerError()
        {
            _adminContext.Dispose();

            // Act
            var result = await _controller.AddVeto(1, "team1", "comp1", "note");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Došlo k chybě při ukládání veta.", statusCodeResult.Value.ToString());
        }

        [Fact]
        public void UpdateVeto_DatabaseExceptionHandling_ReturnsServerError()
        {
            // Arrange - Get a valid ID first, then dispose context
            var existingVeto = _adminContext.Vetoes.First();
            int vetoId = existingVeto.VetoId;
            _adminContext.Dispose();

            // Act
            var result = _controller.UpdateVeto(vetoId, "new note");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Chyba při procesu aktualizace veta!", statusCodeResult.Value.ToString());
        }

        [Fact]
        public void DeleteVeto_DatabaseExceptionHandling_ReturnsServerError()
        {
            // Arrange - Get a valid ID first, then dispose context
            var existingVeto = _adminContext.Vetoes.First();
            int vetoId = existingVeto.VetoId;
            _adminContext.Dispose();

            // Act
            var result = _controller.DeleteVeto(vetoId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Nepodařilo se vymazat veto z databáze!", statusCodeResult.Value.ToString());
        }

        public void Dispose()
        {
            _adminContext?.Dispose();
        }
    }
}
