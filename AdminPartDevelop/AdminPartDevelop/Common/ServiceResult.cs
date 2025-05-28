namespace AdminPartDevelop.Common
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }

        public static ServiceResult<T> Success(T data) =>
            new ServiceResult<T> { IsSuccess = true, Data = data };

        public static ServiceResult<T> Failure(string errorMessage, Exception ex = null) =>
            new ServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage, Exception = ex };

        public T GetDataOrThrow()
        {
            if (!IsSuccess)
            {
                throw Exception ?? new InvalidOperationException(ErrorMessage ?? "An unknown error occurred.");
            }
            return Data;
        }
    }
}
