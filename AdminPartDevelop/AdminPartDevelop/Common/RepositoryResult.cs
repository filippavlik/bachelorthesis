namespace AdminPart.Common
{
    public class RepositoryResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }

        public static RepositoryResult<T> Success(T data) =>
            new RepositoryResult<T> { IsSuccess = true, Data = data };

        public static RepositoryResult<T> Failure(string errorMessage, Exception ex = null) =>
            new RepositoryResult<T> { IsSuccess = false, ErrorMessage = errorMessage, Exception = ex };

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
