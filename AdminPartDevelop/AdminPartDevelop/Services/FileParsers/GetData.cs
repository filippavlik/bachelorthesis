using AdminPart.DTOs;
using Aspose.Cells;
using AdminPart.Common;
using AdminPart.Models;

namespace AdminPart.Services.FileParsers
{
    public class GetData : IExcelParser
    {
        private readonly ILogger<GetData> _logger;

        public GetData(ILogger<GetData> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResult<string?>> SaveAndValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ServiceResult<string?>.Failure("Soubor je prázdný nebo neexistuje.");
            }

            //creation of new tmp foler , where we ll store the files provided by users
            var folderPath = Path.Combine(Path.GetTempPath(), "uploads");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Validate if the file is excel format
            if (!File.Exists(filePath) || !(filePath.EndsWith(".xls") || filePath.EndsWith(".xlsx")))
            {
                return ServiceResult<string>.Failure("Prosím vyberte validní excel soubor!");
            }

            return ServiceResult<string>.Success(filePath);
        }
        public async Task<ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>> GetRefereesDataAsync(string path)
        {
            try
            {
                Dictionary<KeyValuePair<string,string>, IRefereeDto> refs = await Task.Run(() =>
                {
                    // Load the workbook
                    Workbook workbook = new Workbook(path);
                    Worksheet worksheet = workbook.Worksheets[0];
                    Cells cells = worksheet.Cells;

                    // we need to specify how to find the cells, under whose are the informations we need
                    FindOptions findOptions = new FindOptions { LookAtType = LookAtType.StartWith };

                    List<Cell> arrayCells = new List<Cell>
                    {
                        cells.Find("PŘEBOR", null, findOptions),
                        cells.Find("1.A TŘÍDA", null, findOptions),
                        cells.Find("1.B TŘÍDA", null, findOptions),
                        cells.Find("2. - 3.TŘÍDA", null, findOptions),
                        cells.Find("KATEGORIE M", null, findOptions),
                        cells.Find("KATEGORIE N", null, findOptions)
                    };

                    // Dictionary to store referee data
                    Dictionary<KeyValuePair<string, string>, IRefereeDto> localRefs = new Dictionary<KeyValuePair<string, string>, IRefereeDto>();

                    int league = 0;
                    int indent = 0; 

                    foreach (Cell cell in arrayCells)
                    {
                        if (cell == null) continue;

                        //we need to access cells 2 rows under the head , because the first is the name of league and second is legenda as surname, name
                        int startIndex = cell.Row + 2;
                        while (true)
                        {
                            Row row = worksheet.Cells.Rows[startIndex];

                            // If current cell is empty, try to find next group of referees 
                            // by shifting 4 columns to the right (to the next league section)
                            if (row[cell.Column + indent].Value == null)
                            {
                                indent += 4;
                                // If after shifting we still find an empty cell, we've processed all referees
                                if (worksheet.Cells.Rows[cell.Row + 2][cell.Column + indent].Value == null)
                                {
                                    indent = 0;
                                    break;
                                }
                                else
                                {
                                    // Reset to first row of referee data in the next league section
                                    startIndex = cell.Row + 2;
                                    row = worksheet.Cells.Rows[startIndex];
                                }
                            }

                            string surname = row[cell.Column + indent].Value.ToString().Trim();
                            string name = row[cell.Column + indent + 1].Value.ToString().Trim();
                            int age = Convert.ToInt32(row[cell.Column + indent + 2].Value);

                            localRefs[new KeyValuePair<string, string>(name, surname)] = new RefereeLevelDto(name, surname, league, age);

                            // Move to next row
                            startIndex++;
                        }

                        league++;
                    }

                    return localRefs;
                });
                return ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>.Success(refs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetUsersData] Error fetching referees");
                return ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>.Failure("Nepodařilo se načíst data ze souboru");
            }
        }
        public async Task<ServiceResult<List<UnfilledMatchDto>>> GetMatchesDataAsync(string path)
        {
            try
            {
                List<UnfilledMatchDto> matches = await Task.Run(() =>
                {
                    Workbook workbook = new Workbook(path);
                    Worksheet sheet = workbook.Worksheets[0];
                    Cells cells = sheet.Cells;

                    int indexRow = 1;
                    List<UnfilledMatchDto> matchesLocal = new List<UnfilledMatchDto>();

                    while (indexRow < cells.Rows.Count)
                    {
                        Row row = cells.Rows[indexRow];
                        if (row == null || row.IsBlank)
                        {
                            _logger.LogInformation("[GetMatchesDataAsync] breaking ");
                            break;
                        }

                        string numberMatch = row[0].StringValue;         // Column 0 - Číslo zápasu
                        string homeTeamFull = row[1].StringValue;        // Column 1 - Domácí (full team name with ID)
                        string awayTeamFull = row[2].StringValue;        // Column 2- Hosté (full team name with ID)
                        string dateTimeStr = row[3].StringValue;         // Column 3 - Datum zápasu
                        string gameField = row[12].StringValue;          // Column 11 - Hřiště

                        
                        int indexSeparatorHome = homeTeamFull.IndexOf(" - ");
                        string idHomeRaw = homeTeamFull.Substring(0, indexSeparatorHome).Trim();
                        string nameHome = homeTeamFull.Substring(indexSeparatorHome + 3).Trim();
                        
                        int indexSeparatorAway = awayTeamFull.IndexOf(" - ");                        
                        string idAwayRaw = awayTeamFull.Substring(0, indexSeparatorAway).Trim();
                        string nameAway = awayTeamFull.Substring(indexSeparatorAway + 3).Trim();


                        
                        var czechCulture = new System.Globalization.CultureInfo("cs-CZ");
                        DateTime dateOfGame;
                        dateOfGame = DateTime.Parse(dateTimeStr, czechCulture);
                        
                        var match = new UnfilledMatchDto(
                            numberMatch,
                            idHomeRaw,
                            nameHome,
                            idAwayRaw,
                            nameAway,
                            dateOfGame,
                            gameField
                        );

                        matchesLocal.Add(match);
                        indexRow++;
                    }
                    return matchesLocal;
                });

                if (matches.Count == 0)
                    return ServiceResult<List<UnfilledMatchDto>>.Failure("Tenhle soubor neobsahuje žádné zápasy!");

                return ServiceResult<List<UnfilledMatchDto>>.Success(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetMatchesDataAsync] Error fetching matches");
                return ServiceResult<List<UnfilledMatchDto>>.Failure("Nepodařilo se načíst data ze souboru");
            }
        }
        public async Task<ServiceResult<List<FilledMatchDto>>> GetPlayedMatchesDataAsync(string path)
        {
            try
            {
                List<FilledMatchDto> matches = await Task.Run(() =>
                {
                    Workbook workbook = new Workbook(path);
                    Worksheet sheet = workbook.Worksheets[0];
                    Cells cells = sheet.Cells;

                    int indexRow = 1;
                    List<FilledMatchDto> matchesLocal = new List<FilledMatchDto>();

                    while (indexRow < cells.Rows.Count)
                    {
                        Row row = cells.Rows[indexRow];
                        if (row == null || row.IsBlank)
                        {
                            _logger.LogInformation("[GetPlayedMatchesDataAsync] breaking ");
                            break;
                        }

                        (string? Surname, string? Name, string? Id) ParseRefereeInfo(string? raw)
                        {
                            if (string.IsNullOrWhiteSpace(raw))
                                return (null, null, null);

                            string[] parts = raw.Replace("(", "").Replace(")", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            string? surname = parts.Length > 0 ? parts[0] : null;
                            string? name = parts.Length > 1 ? parts[1] : null;
                            string? id = parts.Length > 2 ? parts[2] : null;

                            return (surname, name, id);
                        }

                        string referee = row[4]?.StringValue;
                        string ar1 = row[5]?.StringValue;
                        string ar2 = row[6]?.StringValue;

                        var (surnameReferee, nameReferee, idReferee) = ParseRefereeInfo(referee);
                        var (surnameAr1, nameAr1, idAr1) = ParseRefereeInfo(ar1);
                        var (surnameAr2, nameAr2, idAr2) = ParseRefereeInfo(ar2);

                        string numberMatch = row[0].StringValue;         // Column 0 - Číslo zápasu                        
                     
                        var match = new FilledMatchDto(
                            numberMatch,
                            idReferee,
                            nameReferee,
                            surnameReferee,
                            idAr1,
                            nameAr1,
                            surnameAr1,
                            idAr2,
                            nameAr2,
                            surnameAr2
                        );

                        matchesLocal.Add(match);
                        indexRow++;
                    }
                    return matchesLocal;
                });

                if (matches.Count == 0)
                    return ServiceResult<List<FilledMatchDto>>.Failure("Tenhle soubor neobsahuje žádné zápasy!");

                return ServiceResult<List<FilledMatchDto>>.Success(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetPlayedMatchesDataAsync] Error fetching matches");
                return ServiceResult<List<FilledMatchDto>>.Failure("Nepodařilo se načíst data ze souboru");
            }
        }
        public async Task<ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>> GetInformationsOfReferees(string path)
        {
            try
            {
                Dictionary<KeyValuePair<string, string>, IRefereeDto>  referees = await Task.Run(() =>
                {
                    Workbook workbook = new Workbook(path);
                    Worksheet sheet = workbook.Worksheets[0];
                    Cells cells = sheet.Cells;

                    int indexRow = 1;
                    Dictionary<KeyValuePair<string, string>, IRefereeDto> refereesLocal = new Dictionary<KeyValuePair<string, string>, IRefereeDto> ();

                    while (indexRow < cells.Rows.Count)
                    {
                        Row row = cells.Rows[indexRow];
                        if (row == null || row.IsBlank)
                        {
                            _logger.LogInformation("[GetInformationsOfReferees] breaking ");
                            break;
                        }                      


                        string surname = row[0]?.StringValue;
                        string name = row[1]?.StringValue;
                        string? email = row[2]?.StringValue;
                        string? facrId = row[3]?.StringValue;
                        string? phoneNumber = row[4]?.StringValue;

                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(surname))
                        {
                            name = name.Trim();
                            surname = surname.Trim();

                            var referee = new FilledRefereeDto(
                                name,
                                surname,
                                email,
                                facrId,
                                phoneNumber
                            );
                            refereesLocal[new KeyValuePair<string, string>(name, surname)] = referee;
                        }    
                        indexRow++;
                    }
                    return refereesLocal;
                });

                if (referees.Count == 0)
                    return ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>.Failure("Tenhle soubor neobsahuje žádné informace!");

                return ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>.Success(referees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetInformationsOfReferees] Error fetching referees data");
                return ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>.Failure("Nepodařilo se načíst data ze souboru");
            }
        }
        public async Task<ServiceResult<List<FilledFieldDto>>> GetInformationsAboutFields(string path)
        {
            try
            {
                List<FilledFieldDto> fields = await Task.Run(() =>
                {
                    Workbook workbook = new Workbook(path);
                    Worksheet sheet = workbook.Worksheets[0];
                    Cells cells = sheet.Cells;

                    int indexRow = 1;
                    List<FilledFieldDto> fieldsLocal = new List<FilledFieldDto>();

                    while (indexRow < cells.Rows.Count)
                    {
                        Row row = cells.Rows[indexRow];
                        if (row == null || row.IsBlank)
                        {
                            _logger.LogInformation("[GetInformationsAboutFields] breaking ");
                            break;
                        }

                        string fieldName = row[0]?.StringValue;
                        string? fieldAddress = row[1]?.StringValue;
                        string? fieldLatitudeString = row[2]?.StringValue;
                        string? fieldLongtitudeString = row[3]?.StringValue;
                        if (!string.IsNullOrWhiteSpace(fieldName))
                        {
                            fieldName = fieldName.Trim();
                            float? fieldLatitude = ConvertToFloat(fieldLatitudeString);
                            float? fieldLongitude = ConvertToFloat(fieldLongtitudeString);

                            var field = new FilledFieldDto(
                                fieldName,
                                fieldAddress,
                                fieldLatitude,
                                fieldLongitude
                            );

                            fieldsLocal.Add(field);
                        }    
                        indexRow++;
                    }
                    return fieldsLocal;
                });

                if (fields.Count == 0)
                    return ServiceResult<List<FilledFieldDto>>.Failure("Tenhle soubor neobsahuje žádné hřište!");

                return ServiceResult<List<FilledFieldDto>>.Success(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetInformationsAboutFields] Error fetching fields");
                return ServiceResult<List<FilledFieldDto>>.Failure("Nepodařilo se načíst data ze souboru");
            }
        }

        private float? ConvertToFloat(string? coord)
        {
            if (string.IsNullOrWhiteSpace(coord)) return null;

            char direction = coord[^1]; // Last character (N/S/E/W)
            string numericPart = coord[..^1]; // All but last character

            if (!float.TryParse(numericPart, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float value)) return null;

            return direction switch
            {
                'N' or 'E' => value,
                'S' or 'W' => -value,
                _ => null
            };
        }



    }

}
