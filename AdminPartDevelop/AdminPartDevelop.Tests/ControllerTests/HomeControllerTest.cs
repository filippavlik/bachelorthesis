using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AdminPartDevelop.Controllers;
using AdminPartDevelop.TestData;
using AdminPartDevelop.Data;
using AdminPartDevelop.Services.RefereeServices;
using AdminPartDevelop.Services.AdminServices;
using AdminPartDevelop.Views.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AdminPartDevelop.Models;

namespace AdminPartDevelop.Tests
{
    public class HomeControllerFunctionalTests : IDisposable
    {
        private readonly AdminPartDevelop.Models.AdminDbContext _adminContext;
        private readonly AdminPartDevelop.Models.RefereeDbContext _refereeContext;
        private readonly HomeController _controller;

        public HomeControllerFunctionalTests()
        {
            var optionsAdmin = new DbContextOptionsBuilder<Models.AdminDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var optionsReferee = new DbContextOptionsBuilder<Models.RefereeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _adminContext = new AdminDbContext(optionsAdmin);
            _refereeContext = new RefereeDbContext(optionsReferee);

            // Seed database with mock data
            SeedDatabase();

            // Create controller with real services and seeded database
            var mockHomeControllerLogger = new Mock<ILogger<HomeController>>();
            var mockAdminRepoLogger = new Mock<ILogger<AdminRepo>>();
            var mockRefereeRepoLogger = new Mock<ILogger<RefereeRepo>>();
            var mockRefereeServiceLogger = new Mock<ILogger<RefereeService>>();
            var mockAdminServiceLogger = new Mock<ILogger<AdminService>>();
            var mockExcelParser = new Mock<AdminPartDevelop.Services.FileParsers.IExcelParser>();

            var adminRepo = new AdminRepo(mockAdminRepoLogger.Object, _adminContext);
            var refereeRepo = new RefereeRepo(mockRefereeRepoLogger.Object, _refereeContext);
            var refereeService = new RefereeService(mockRefereeServiceLogger.Object,refereeRepo);
            var adminService = new AdminService(adminRepo, mockAdminServiceLogger.Object);

            _controller = new HomeController(
                refereeRepo,
                adminRepo,
                mockExcelParser.Object,
                refereeService,
                adminService,
                mockHomeControllerLogger.Object
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
        public async Task Index_ReturnsViewWithCorrectData()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MainViewModel>(viewResult.Model);

            // Check ViewBag values
            Assert.Equal("Test admin", _controller.ViewBag.Username);
            Assert.Equal("Admin", _controller.ViewBag.UserRole);

            // Check model data
            Assert.NotNull(model.Referees);
            Assert.NotNull(model.Matches);

            // Verify we have the expected number of referees (8 from mock data)
            var totalReferees = model.Referees.Sum(tab => tab.Referees.Count);
            Assert.Equal(8, totalReferees);

            // Verify we have the expected number of matches (4 from mock data)
            Assert.Equal(5, model.Matches.Count);

            // Check specific referee data

            var allReferees = model.Referees.SelectMany(tab => tab.Referees).ToList();
            Assert.Contains(allReferees, r => r.Referee.Name == "Filip" && r.Referee.Surname == "Pavl�k");
            Assert.Contains(allReferees, r => r.Referee.Name == "Petr" && r.Referee.Surname == "Svoboda");
            Assert.Contains(allReferees, r => r.Referee.Name == "Karel" && r.Referee.Surname == "Dvo��k");
            Assert.Contains(allReferees, r => r.Referee.Name == "Tom�" && r.Referee.Surname == "�ern�");
            Assert.Contains(allReferees, r => r.Referee.Name == "Martin" && r.Referee.Surname == "Proch�zka");
            Assert.Contains(allReferees, r => r.Referee.Name == "Ji��" && r.Referee.Surname == "Krej��");
            Assert.Contains(allReferees, r => r.Referee.Name == "Pavel" && r.Referee.Surname == "Novotn�");
            Assert.Contains(allReferees, r => r.Referee.Name == "Michal" && r.Referee.Surname == "Vesel�");


            // Check matches contain expected data
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110F1A0105");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A2B0404");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023002E3C0503");
            //Sort test
            Assert.Equal("2023110A1A0102", model.Matches[0].Match.MatchId);
            Assert.Equal("2023110A2B0404", model.Matches[1].Match.MatchId);
            Assert.Equal("2023110F1A0105", model.Matches[2].Match.MatchId);
            Assert.Equal("2023002E3C0503", model.Matches[3].Match.MatchId);


            //Check if the referees are delegated
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102" && m.Match.RefereeId == 1);
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102" && m.Match.Ar1Id == 3);
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102" && m.Match.Ar2Id == 4);

            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102" && m.RefereeName == "F. Pavl�k");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102" && m.Ar1Name == "K. Dvo��k");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A1A0102" && m.Ar2Name == "T. �ern�");

            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110F1A0105" && m.RefereeName == "T. �ern�");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110F1A0105" && m.Ar1Name == "M. Proch�zka");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110F1A0105" && m.Ar2Name == "J. Krej��");

            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A2B0404" && m.RefereeName == null);
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A2B0404" && m.Ar1Name == null);
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023110A2B0404" && m.Ar2Name == null);

            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023002E3C0503" && m.RefereeName == null);
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023002E3C0503" && m.Ar1Name == "F. Pavl�k");
            Assert.Contains(model.Matches, m => m.Match.MatchId == "2023002E3C0503" && m.Ar2Name == null);

            Assert.Contains(allReferees, r => r.Referee.Name == "Filip" && r.Referee.Surname == "Pavl�k" && !r.isFreeSaturdayAfternoon);
            Assert.Contains(allReferees, r => r.Referee.Name == "Filip" && r.Referee.Surname == "Pavl�k" && !r.isFreeSundayMorning);
            Assert.Contains(allReferees, r => r.Referee.Name == "Filip" && r.Referee.Surname == "Pavl�k" && r.isFreeSaturdayMorning);
            Assert.Contains(allReferees, r => r.Referee.Name == "Filip" && r.Referee.Surname == "Pavl�k" && !r.isFreeSundayAfternoon);

            Assert.Contains(allReferees, r => r.Referee.Name == "Tom�" && r.Referee.Surname == "�ern�" && !r.isFreeSaturdayAfternoon);
            Assert.Contains(allReferees, r => r.Referee.Name == "Tom�" && r.Referee.Surname == "�ern�" && r.isFreeSundayAfternoon);
            Assert.Contains(allReferees, r => r.Referee.Name == "Tom�" && r.Referee.Surname == "�ern�" && r.isFreeSundayMorning);
            Assert.Contains(allReferees, r => r.Referee.Name == "Tom�" && r.Referee.Surname == "�ern�" && r.isFreeSaturdayMorning);

            Assert.Contains(allReferees, r => r.Referee.Name == "Michal" && r.Referee.Surname == "Vesel�" && r.isFreeSaturdayAfternoon);
            Assert.Contains(allReferees, r => r.Referee.Name == "Michal" && r.Referee.Surname == "Vesel�" && r.isFreeSundayAfternoon);
            Assert.Contains(allReferees, r => r.Referee.Name == "Michal" && r.Referee.Surname == "Vesel�" && r.isFreeSundayMorning);
            Assert.Contains(allReferees, r => r.Referee.Name == "Michal" && r.Referee.Surname == "Vesel�" && r.isFreeSaturdayMorning);

        }

        [Fact]
        public async Task Index_ChecksDelegationPercentage()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);

            var percentage = _controller.ViewBag.percentageDelegated;
            Assert.NotNull(percentage);
            Assert.Equal(80.0, (double)percentage);
        }

        [Fact]
        public async Task Index_ChecksGameDates()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);

           DateOnly gamingDay = DateOnly.FromDateTime(DateTime.Now.AddDays(5));


            var firstGameDay = (DateOnly)_controller.ViewBag.FirstGameDay;
            var secondGameDay = (DateOnly)_controller.ViewBag.SecondGameDay;

            Assert.Equal(gamingDay, firstGameDay);
            Assert.Equal(gamingDay.AddDays(1), secondGameDay);

        }

        public void Dispose()
        {
            _adminContext.Dispose();
            _refereeContext.Dispose();

        }
    }
}