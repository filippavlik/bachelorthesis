using AdminPart.Hubs;
using AdminPart.Models;
using AdminPart.DTOs;
using AdminPart.Services.FileParsers;
using AdminPart.Views.ViewModels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.AspNet.SignalR;
using AdminPart.Services.RouteServices;

namespace AdminPart.Controllers
{
    [Route("Admin/Referee")]
    public class RefereeController : Controller
    {
        private readonly ILogger<RefereeController> _logger;
        private readonly Services.FileParsers.IExcelParser _excelParser;
        private readonly Services.EmailsSender.EmailsToLoginDbSender _emailSender;
        private readonly Services.RefereeServices.IRefereeService _refereeService;
        private readonly Services.AdminServices.IAdminService _adminService;
        private readonly Services.RouteServices.RouteByBusPlanner _routeBusPlanner;
        private readonly Services.RouteServices.RouteByCarPlanner _routeCarPlanner;


        private readonly Microsoft.AspNetCore.SignalR.IHubContext<HubForReendering> _hubContext;
        private const string MatchesCacheKey = "AppMatches";
        private readonly IMemoryCache _memoryCache;


        private readonly Data.IRefereeRepo _refereeRepo;
        private readonly Data.IAdminRepo _adminRepo;
        public RefereeController(Data.IRefereeRepo refereeRepo, Data.IAdminRepo adminRepo, Services.RouteServices.RouteByCarPlanner routeCarPlanner, Services.RouteServices.RouteByBusPlanner routeBusPlanner,
            Services.FileParsers.IExcelParser excelParser,Services.EmailsSender.EmailsToLoginDbSender emailSender,
            Services.RefereeServices.IRefereeService refereeService, Services.AdminServices.IAdminService adminService,
            Microsoft.AspNetCore.SignalR.IHubContext<HubForReendering> hubContext, IMemoryCache memoryCache, ILogger<RefereeController> logger)
        {
            _logger = logger;
            _excelParser = excelParser;
            _emailSender = emailSender;
            _refereeService = refereeService;
            _adminService = adminService;
            _routeCarPlanner = routeCarPlanner;
            _routeBusPlanner = routeBusPlanner;
            _refereeRepo = refereeRepo;
            _adminRepo = adminRepo;
            _hubContext = hubContext;
            _memoryCache = memoryCache;
        }

        [HttpPost("AddNewRefereeAsync")]
        public async Task<IActionResult> AddNewRefereeAsync([FromBody] DTOs.RefereeAddRequest request)
        {
            // Check if model is valid based on annotations
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var referee = new Referee
                {
                    FacrId = request.FacrId,
                    Name = request.Name,
                    Surname = request.Surname,
                    Email = request.Email,
                    League = request.League,
                    Age = request.Age,
                    Ofs = request.Ofs,
                    Note = request.Note,
                    CarAvailability = request.CarAvailability,
                    PragueZone = request.Place==null ? "0" : request.Place,
                    TimestampChange = TimeZoneInfo.ConvertTimeFromUtc
                                (DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")) //we want to have timestamp for Prague time
                };
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                        var emails = new List<string> { request.Email };
                        var sendResult = (await _emailSender.AddEmailsToAllowedListAsync(emails)).GetDataOrThrow();
                        if (!sendResult)
                        {
                                _logger.LogWarning("Some or all emails could not be added to the login DB.");
                                return StatusCode(500, "E-mail nebylo možné přidat do přihlašovací databáze.");
                        }
                }

                var resultOfTransaction = await _refereeRepo.AddRefereeAsync(referee);
                if (resultOfTransaction.Success)
                {
                    return Ok(resultOfTransaction.Message);
                }
                else
                {
                    return StatusCode(500, resultOfTransaction.Message);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new referee");
                return StatusCode(500, new { message = "Došlo k chybě při ukládání rozhodčího." });
            }
        }
        [HttpPost("AddRefereeToTheMatch")]
        public async Task<IActionResult> AddRefereeToTheMatch(int refereeId, string matchId, int role, bool force,string user)
        {
            try
            {
                var listOfMatches = GetMatchesFromCache().Result;
                DateOnly firstGameDay = _adminRepo.GetStartGameDate().GetDataOrThrow();
                Referee refereeFromId = (await _refereeRepo.GetRefereeByIdAsync(refereeId)).GetDataOrThrow();
                var listOfTransfers = (await _adminRepo.GetRefereesTransfersAsync(refereeId)).GetDataOrThrow();

                //gather informations from all sources and fill the referee profile to check time availability
                RefereeWithTimeOptions referee = (await _refereeService.AddRefereeTimeOptionsAsync(refereeFromId, listOfMatches, listOfTransfers, firstGameDay)).GetDataOrThrow();

                Match matchToCheck = (await _adminRepo.GetMatchByIdAsync(matchId)).GetDataOrThrow();
                //check time availability and vetoes of the referee
                if (!force)
                {
                    bool hasVeto = (await _adminRepo.DoesVetoExist(matchId, refereeId)).GetDataOrThrow();

                    if (hasVeto)
                    {
                        return StatusCode(400, "Daný rozhodčí má veto na jeden z tímu (zkontrolujte v okně rozhodčího)!");
                    }
                    bool isFree = _refereeService.CheckTimeAvailabilityOfReferee(referee, matchToCheck).GetDataOrThrow();

                    if (!isFree)
                    {
                        return StatusCode(400, "Daný rozhodčí je v daný čas zápasu nedostupný (zkontrolujte v okně rozhodčího)!");
                    }
                }

                //find out if there is longtitute and latitude of play field
                Tuple<bool, double, Transfer> isManageableWTransferPreMatch = null!;
                Tuple<bool, double, Transfer> isManageableWTransferPostMatch = null!;
                Tuple<DateTime, string?> startOfNextMatch = null!;
                Tuple<DateTime, string?> endOfPreviousMatch = null!;
                Transfer transferPre = null!;
                Transfer transferPost = null!;
                bool succesfullyCalculatedRoutePre = true;
                bool succesfullyCalculatedRoutePost = true;



                //find if the location is not set
                float longtitude = matchToCheck.Field.Longitude;
                float latitude = matchToCheck.Field.Latitude;
                const float epsilon = 0.001f;

                bool isLatLonZero = Math.Abs(longtitude) < epsilon || Math.Abs(latitude) < epsilon;

                if (!isLatLonZero)
                {
                    startOfNextMatch = _refereeService.GetFirstNextMatchDateTime(referee, matchToCheck).GetDataOrThrow();
                    endOfPreviousMatch = _refereeService.GetFirstPreviousMatchDateTime(referee, matchToCheck.MatchDate.ToDateTime(matchToCheck.MatchTime)).GetDataOrThrow();

                    bool hasCarOverall = referee.Referee.CarAvailability;
                    //referee is using car or public transport at the moment?
                    bool? hasCarDuring = _refereeService.CheckCarAvailabilityOfReferee(referee, matchToCheck).GetDataOrThrow();

                    bool actuallCarAvailability = hasCarDuring.HasValue ? hasCarDuring.Value : hasCarOverall;
                    //calculate the possible route with km and time if the match is less than 1 hour and half from the beggining of actuall
                    if (startOfNextMatch != null)
                    {
                        var nextMatch = (await _adminRepo.GetMatchByIdAsync(startOfNextMatch.Item2)).GetDataOrThrow();

                        bool isEndingLatLonZero = Math.Abs(nextMatch.Field.Longitude) < epsilon || Math.Abs(nextMatch.Field.Latitude) < epsilon;
                        try
                        {
                            if (!isEndingLatLonZero && actuallCarAvailability)
                            {
                                var result = (await _routeCarPlanner.CalculateRoute(latitude, longtitude, nextMatch.Field.Latitude, nextMatch.Field.Longitude)).GetDataOrThrow();
                                isManageableWTransferPostMatch = _refereeService.CheckTimeAvailabilityWithTransferOfReferee(startOfNextMatch.Item1, nextMatch.MatchId, matchToCheck, result.Item2, refereeId, false, true).GetDataOrThrow();
                                transferPost = isManageableWTransferPostMatch.Item3;
                            }
                            else if (!isEndingLatLonZero && !actuallCarAvailability)
                            {
                                var result = (await _routeBusPlanner.CalculateRoute(latitude, longtitude, nextMatch.Field.Latitude, nextMatch.Field.Longitude, matchToCheck.MatchDate.ToDateTime(matchToCheck.MatchTime).AddMinutes(135))).GetDataOrThrow();
                                isManageableWTransferPostMatch = _refereeService.CheckTimeAvailabilityWithTransferOfReferee(startOfNextMatch.Item1, nextMatch.MatchId, matchToCheck, result.Item2, refereeId, false, true).GetDataOrThrow();
                                transferPost = isManageableWTransferPostMatch.Item3;
                            }
                        }
                        catch (Exception ex)
                        {
                            succesfullyCalculatedRoutePost = false;
                            _logger.LogError(ex, "[Calculating Route] Route or transfer check failed, continuing.");
                        }
                    }
                    if (endOfPreviousMatch != null)
                    {
                        try
                        {
                            var previousMatch = (await _adminRepo.GetMatchByIdAsync(endOfPreviousMatch.Item2)).GetDataOrThrow();

                            bool isEndingLatLonZero = Math.Abs(previousMatch.Field.Longitude) < epsilon || Math.Abs(previousMatch.Field.Latitude) < epsilon;
                            if (!isEndingLatLonZero && actuallCarAvailability)
                            {
                                var result = (await _routeCarPlanner.CalculateRoute(previousMatch.Field.Latitude, previousMatch.Field.Longitude, latitude, longtitude)).GetDataOrThrow();
                                isManageableWTransferPreMatch = _refereeService.CheckTimeAvailabilityWithTransferOfReferee(endOfPreviousMatch.Item1, previousMatch.MatchId, matchToCheck, result.Item2, refereeId, true, true).GetDataOrThrow();
                                transferPre = isManageableWTransferPreMatch.Item3;
                            }
                            else if (!isEndingLatLonZero && !actuallCarAvailability)
                            {
                                var result = (await _routeBusPlanner.CalculateRoute(previousMatch.Field.Latitude, previousMatch.Field.Longitude, latitude, longtitude, endOfPreviousMatch.Item1)).GetDataOrThrow();
                                isManageableWTransferPreMatch = _refereeService.CheckTimeAvailabilityWithTransferOfReferee(endOfPreviousMatch.Item1, previousMatch.MatchId, matchToCheck, result.Item2, refereeId, true, true).GetDataOrThrow();
                                transferPre = isManageableWTransferPreMatch.Item3;
                            }
                        }
                        catch (Exception ex)
                        {
                            succesfullyCalculatedRoutePre = false;
                            _logger.LogError(ex, "[Calculating Route] Route or transfer check failed, continuing.");
                        }
                    }
                }
                if (isManageableWTransferPostMatch != null && succesfullyCalculatedRoutePost)
                {
                    if (!force && !isManageableWTransferPostMatch.Item1)
                    {
                        return StatusCode(400, "Daný rozhodčí nestíhá přijít na zápas z předzápasu o " + isManageableWTransferPostMatch.Item2 + " minut (zkontrolujte v okně rozhodčího)!");
                    }
                    var resultOfTransferPostTrans = await _adminRepo.AddTransfer(transferPost);
                }
                if (isManageableWTransferPreMatch != null && succesfullyCalculatedRoutePre)
                {
                    if (!force && !isManageableWTransferPreMatch.Item1)
                    {
                        return StatusCode(400, "Daný rozhodčí nestíhá přijít na následující zápas z tohoto zápasu o" + isManageableWTransferPreMatch.Item2 + " minut (zkontrolujte v okně rozhodčího)!");
                    }
                    var resultOfTransferPreTrans = await _adminRepo.AddTransfer(transferPre);
                }

                var resultOfTransaction = await _adminRepo.AddRefereeToTheMatch(refereeId, matchId, role,user);

                if (resultOfTransaction.Success)
                {
                    var updatedListOfMatches = UploadRefreshedMatchToCache(matchId).Result;
                    if (updatedListOfMatches.Count == 0)
                    {
                        return StatusCode(500, "Nastala chyba při přidávání rozhodčího na zápas. (získavání zápasů z cache)");
                    }

                    Referee updatedRefereeFromId = (await _refereeRepo.GetRefereeByIdAsync(refereeId)).GetDataOrThrow();
                    var updatedListOfTransfers = (await _adminRepo.GetRefereesTransfersAsync(refereeId)).GetDataOrThrow();

                    RefereeWithTimeOptions updatedReferee = (await _refereeService.AddRefereeTimeOptionsAsync(updatedRefereeFromId, updatedListOfMatches, updatedListOfTransfers, firstGameDay)).GetDataOrThrow();
		    DateTime timestampChangeHub = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
                    await _hubContext.Clients.All.SendAsync("AcceptChangeMatchAdd", matchId, refereeId, updatedRefereeFromId.Name.Substring(0, 1) + ". " + updatedRefereeFromId.Surname, role,user,timestampChangeHub);
                    await _hubContext.Clients.All.SendAsync("AcceptChangeReferee", new {
                                refereeId = refereeId,
                                refereeData = updatedReferee
                        });


                    return Ok(resultOfTransaction.Message);
                }
                else
                {
                    return StatusCode(500, resultOfTransaction.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AddRefereeFromTheMatch] Error referee controller");
                return StatusCode(500, "Nastala chyba při přidávání rozhodčího na zápas.");
            }
        }
        [HttpPost("RemoveRefereeFromTheMatch")]
        public async Task<IActionResult> RemoveRefereeFromTheMatch(string matchId, int refereeId,string user)
        {
            try
            {
                var resultOfTransaction = await _adminRepo.RemoveRefereeFromTheMatch(refereeId, matchId,user);
                var resultOfTransferTransaction = await _adminRepo.RemoveTransfersConnectedTo(refereeId, matchId);
                if (resultOfTransaction.Success)
                {
                    DateOnly firstGameDay = _adminRepo.GetStartGameDate().GetDataOrThrow();
                    var updatedListOfMatches = UploadRefreshedMatchToCache(matchId).Result;
                    if (updatedListOfMatches.Count == 0)
                    {
                        return StatusCode(500, "Nastala chyba při přidávání rozhodčího na zápas. (získavání zápasů z cache)");
                    }

                    Referee updatedRefereeFromId = (await _refereeRepo.GetRefereeByIdAsync(refereeId)).GetDataOrThrow();
                    var updatedListOfTransfers = (await _adminRepo.GetRefereesTransfersAsync(refereeId)).GetDataOrThrow();


                    RefereeWithTimeOptions updatedReferee = (await _refereeService.AddRefereeTimeOptionsAsync(updatedRefereeFromId, updatedListOfMatches, updatedListOfTransfers, firstGameDay)).GetDataOrThrow();
			DateTime timestampChangeHub = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

                    await _hubContext.Clients.All.SendAsync("AcceptChangeMatchRemove", matchId, updatedReferee.Referee.RefereeId,user,timestampChangeHub);
                    await _hubContext.Clients.All.SendAsync("AcceptChangeReferee", new {
                                refereeId = refereeId,
                                refereeData = updatedReferee
                        });

                    return Ok(resultOfTransaction.Message);
                }
                else
                {
                    return StatusCode(500, resultOfTransaction.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RemoveRefereeFromTheMatch] Error referee controller");
                return StatusCode(500, "Nastala chyba při odstraňování rozhodčího z zápasu.");
            }
        }
        [HttpPost("GetCardInfo")]
        public async Task<IActionResult> GetCardInfo(int id)
        {
            try
            {
                Referee referee = (await _refereeRepo.GetRefereeByIdAsync(id)).GetDataOrThrow();
                DateOnly firstGameDay = _adminRepo.GetStartGameDate().GetDataOrThrow();

                var listOfMatches = GetMatchesFromCache().Result;
                var listOfTransfers = (await _adminRepo.GetRefereesTransfersAsync(id)).GetDataOrThrow();
                var refereeWithTimeOptions = (await _refereeService.AddRefereeTimeOptionsAsync(referee, listOfMatches, listOfTransfers, firstGameDay)).GetDataOrThrow();
                ViewBag.FirstGameDay = firstGameDay;

                var vetoesOfReferee = (await _adminRepo.GetRefereesVetoesAsync(id)).GetDataOrThrow();

                RefereeCardViewModel refereeCardViewModel = new RefereeCardViewModel
                {
                    RefereeWTimeOptions = refereeWithTimeOptions,
                    Vetoes = vetoesOfReferee
                };

                return PartialView("~/Views/PartialViews/_RefereeCard.cshtml", refereeCardViewModel);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetCardInfo] Error referee controller");
                return StatusCode(500, "Nastala chyba při zobrazování rozhodčího.");

            }
        }
        [HttpGet("GetExcuses")]
        public async Task<IActionResult> GetExcuses()
        {
            try
            {
                DateOnly firstGameDay = _adminRepo.GetStartGameDate().GetDataOrThrow();
                ViewBag.FirstGameDay = firstGameDay;


                var excuses = (await _refereeRepo.GetExcusesAsync()).GetDataOrThrow();


                return PartialView("~/Views/PartialViews/_ExcusesTable.cshtml", excuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Excuses] Error referee controller");
                return PartialView("~/Views/Shared/_ErrorPartial.cshtml", "Nastala chyba při načítání omluv rozhodčích.");
            }
        }
        [HttpPost("UpdateRefereeAsync")]
        public async Task<IActionResult> UpdateRefereeAsync(int id,string name,string surname,string idFacr,string email,int age,int league,bool car, bool pfs,string place,string note)
        {
            try
            {
                var refereeToUpdate = new RefereeAddRequest
                {
                    FacrId = idFacr,
                    Name = name,
                    Surname = surname,
                    Email = email,
                    League = league,
                    Age = age,
                    Ofs = pfs,
                    Note = note,
                    CarAvailability = car,
                    Place = place == null ? "0" : place,
                };
                if (!string.IsNullOrWhiteSpace(email))
                {
                        var emails = new List<string> { email };
                        var sendResult = (await _emailSender.AddEmailsToAllowedListAsync(emails)).GetDataOrThrow();
                        if (!sendResult)
                        {
                                _logger.LogWarning("Some or all emails could not be added to the login DB.");
                                return StatusCode(500, "E-maily nebylo možné přidat do přihlašovací databáze.");
                        }
                }

                var responseOfTransaction = await _refereeRepo.UpdateRefereeAsync(id,refereeToUpdate);

                if (responseOfTransaction.Success)
                {
                    return Ok(responseOfTransaction.Message);
                }
                else
                {
                    return StatusCode(500, responseOfTransaction.Message);
                }
            }
            catch (InvalidOperationException inEx)
            {
                return StatusCode(500, inEx.Message);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateRefereeAsync] Error home controller");
                return StatusCode(500, "Nastala chyba při nahrávání informací o rozhodčím na server.");
            }

        }
        [HttpPost("UploadRefereesFromFileAsync")]
        public async Task<IActionResult> UploadRefereesFromFileAsync(IFormFile file)
        {
            try
            {
                var filePath = (await _excelParser.SaveAndValidateFileAsync(file)).GetDataOrThrow();
                var dictOfReferees = (await _excelParser.GetRefereesDataAsync(filePath)).GetDataOrThrow();

                var resultOfTransaction = (await _refereeRepo.UpdateRefereesAsync(dictOfReferees));

                if (resultOfTransaction.Success)
                    return Ok(resultOfTransaction.Message);
                else
                    return StatusCode(500, resultOfTransaction.Message);

            }
            catch (InvalidOperationException inEx)
            {
                return StatusCode(500, inEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UploadRefereesFromFileAsync] Error referee controller");
                return StatusCode(500, "Nastala chyba při nahrávání rozhodčích na server.");
            }
        }
        [HttpPost("UploadRefereesFromEmailFileAsync")]
        public async Task<IActionResult> UploadRefereesFromEmailFileAsync(IFormFile file)
        {
            try
            {
                var filePath = (await _excelParser.SaveAndValidateFileAsync(file)).GetDataOrThrow();
                var dictOfReferees = (await _excelParser.GetInformationsOfReferees(filePath)).GetDataOrThrow();

                // Get email list from referee dictionary
                var emailList = dictOfReferees
                        .Values
                        .OfType<FilledRefereeDto>() // filter only actual FilledRefereeDto instances
                        .Select(r => r.Email?.Trim())
                        .Where(email => !string.IsNullOrWhiteSpace(email))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                // Send emails to login container
                var sendResult = (await _emailSender.AddEmailsToAllowedListAsync(emailList)).GetDataOrThrow();
                if (!sendResult)
                {
                        _logger.LogWarning("Some or all emails could not be added to the login DB.");
                        return StatusCode(500, "Některé nebo všechny e-maily nebylo možné přidat do přihlašovací databáze..");
                }
                var resultOfTransaction = (await _refereeRepo.UpdateRefereesAsync(dictOfReferees));

                if (resultOfTransaction.Success)
                    return Ok(resultOfTransaction.Message);
                else
                    return StatusCode(500, resultOfTransaction.Message);

            }
            catch (InvalidOperationException inEx)
            {
                return StatusCode(500, inEx.Message);
            }
                catch (Exception ex)
            {
                _logger.LogError(ex, "[UploadRefereesFromEmailFileAsync] Error referee controller");
                return StatusCode(500, "Nastala chyba při nahrávání informacích o rozhodčích na server.");
            }
        }
        [HttpPost("UploadRefreshedMatch")]
        public async Task<IActionResult> UploadRefreshedMatch(string matchId)
        {
            try
            {
                var matches = await UploadRefreshedMatchToCache(matchId);

                if (matches.Count == 0)
                    return StatusCode(500, "Nastala chyba při update zápasu do cache.");
                else
                    return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UploadRefreshedMatch] Error referee controller");
                return StatusCode(500, "Nastala chyba při update zápasu do cache.(neznáma chyba)");
            }
        }

        private async Task<List<Match>> GetMatchesFromCache()
        {
            try
            {
                    if (!_memoryCache.TryGetValue(MatchesCacheKey, out List<Match> updatedlistOfMatches))
                {
                    updatedlistOfMatches = (await _adminRepo.GetPureMatchesAsync()).GetDataOrThrow();
                    _memoryCache.Set(MatchesCacheKey, updatedlistOfMatches, TimeSpan.FromHours(1));
                    return updatedlistOfMatches;
                }
                else
                {
                    return updatedlistOfMatches;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetMatchesFromCache] Error referee controller");
                return new List<Match>();
            }
        }
        private async Task<List<Match>> UploadRefreshedMatchToCache(string matchId)
        {
            try
            {
                if (!_memoryCache.TryGetValue(MatchesCacheKey, out List<Match> updatedlistOfMatches))
                {
                    updatedlistOfMatches = (await _adminRepo.GetPureMatchesAsync()).GetDataOrThrow();
                    _memoryCache.Set(MatchesCacheKey, updatedlistOfMatches, TimeSpan.FromHours(1));
                    return updatedlistOfMatches;
                }
                else
                {
                    var matchIndex = updatedlistOfMatches.FindIndex(m => m.MatchId == matchId);
                    if (matchIndex != -1)
                    {
                        var freshMatchData = (await _adminRepo.GetMatchByIdAsync(matchId)).GetDataOrThrow();

                        updatedlistOfMatches[matchIndex] = freshMatchData;

                        _memoryCache.Set(MatchesCacheKey, updatedlistOfMatches, TimeSpan.FromHours(1));
                    }
                    return updatedlistOfMatches;

                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "[UploadRefreshedMatchToCache] Error referee controller");
                return new List<Match>();
            }
        }
    }
}
