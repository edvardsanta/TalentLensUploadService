namespace UploadFiles.Configurations
{
    public static partial class ApplicationConfiguration
    {
        public class Result<T>
        {
            public T Value { get; }
            public bool IsSuccess { get; }
            public string Error { get; }

            private Result(T value)
            {
                Value = value;
                IsSuccess = true;
            }

            private Result(string error)
            {
                Error = error;
                IsSuccess = false;
            }

            public static Result<T> Success(T value) => new(value);
            public static Result<T> Failure(string error) => new(error);
        }
    }
}
