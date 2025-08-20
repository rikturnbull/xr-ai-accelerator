namespace XrAiAccelerator
{
    public abstract class XrAiResult
    {
        public bool IsSuccess { get; protected set; }
        public string ErrorMessage { get; protected set; }

        protected XrAiResult(bool isSuccess, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static XrAiResult<T> Success<T>(T data) => new XrAiResult<T>(data, true);
        public static XrAiResult<T> Failure<T>(string errorMessage) => new XrAiResult<T>(default(T), false, errorMessage);
    }

    public class XrAiResult<T> : XrAiResult
    {
        public T Data { get; private set; }

        internal XrAiResult(T data, bool isSuccess, string errorMessage = null) 
            : base(isSuccess, errorMessage)
        {
            Data = data;
        }
    }
}
