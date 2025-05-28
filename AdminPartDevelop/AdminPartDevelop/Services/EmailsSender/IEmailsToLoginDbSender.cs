using AdminPartDevelop.Common;

namespace AdminPartDevelop.Services.EmailsSender
{
    public interface IEmailsToLoginDbSender
    {
        Task<ServiceResult<bool>> AddEmailsToAllowedListAsync(List<string> emails);
    }
}
