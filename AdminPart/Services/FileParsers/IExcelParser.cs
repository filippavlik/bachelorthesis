using AdminPart.Common;
using AdminPart.DTOs;
using AdminPart.Models;
using Aspose.Cells;
using Microsoft.Maui.Storage;

namespace AdminPart.Services.FileParsers
{
    public interface IExcelParser
    {

        /// <summary>
        /// Validation of type of file and then save file to temporary folder, from where we can extract necessary informations
        /// </summary>
        /// <param name="file">The file choosen by user, file picker initialized by jquery script.</param>
        /// <returns>Path to the file.</returns>
        Task<ServiceResult<string?>> SaveAndValidateFileAsync(IFormFile file);
        /// <summary>
        /// Extracting referee data from file
        /// </summary>
        /// <param name="path">The path provided by saveAndValidateFileAsync method.</param>
        /// <returns>Dictionary with key as first name and surname , for faster accessing and informations about referee.</returns>
        Task<ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>> GetRefereesDataAsync(string path);
        /// <summary>
        /// Extracting match data from file
        /// </summary>
        /// <param name="path">The path provided by saveAndValidateFileAsync method.</param>
        /// <returns>List of matches filled with unfilledMatchDto.</returns>
        Task<ServiceResult<List<UnfilledMatchDto>>> GetMatchesDataAsync(string path);
        /// <summary>
        /// Extracting already played matches data from file
        /// </summary>
        /// <param name="path">The path provided by saveAndValidateFileAsync method.</param>
        /// <returns>List of matches filled with filledMatchDto(informations about already playe match with delegated person).</returns>
        Task<ServiceResult<List<FilledMatchDto>>> GetPlayedMatchesDataAsync(string path);
        /// <summary>
        /// Extracts referee information from the given file.
        /// </summary>
        /// <param name="path">The path provided by saveAndValidateFileAsync method.</param>
        /// <returns>
        /// A dictionary where each key is a pair of strings (possibly representing referee identity),
        /// and each value is an object implementing IRefereeDto(email,phone number, facrId).
        /// </returns>
        Task<ServiceResult<Dictionary<KeyValuePair<string, string>, IRefereeDto>>> GetInformationsOfReferees(string path);

        /// <summary>
        /// Extracts field information from the given file.
        /// </summary>
        /// <param name="path">The path provided by saveAndValidateFileAsync method.</param>
        /// <returns>List of fields filled with FilledFieldDto.(address , latitude ,longtitude)</returns>
        Task<ServiceResult<List<FilledFieldDto>>> GetInformationsAboutFields(string path);


    }
}
