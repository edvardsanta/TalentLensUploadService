using static UploadFiles.Configurations.ApplicationConfiguration;

namespace UploadFiles.Extensions
{
    public static class FunctionalExtensions
    {
        public static T Apply<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }

        public static Result<T> Bind<T>(this Result<T> result, Func<T, Result<T>> func) =>
            result.IsSuccess ? func(result.Value) : result;

        public static async Task<Result<T>> BindAsync<T>(
            this Result<T> result,
            Func<T, Task<Result<T>>> func
        ) => result.IsSuccess ? await func(result.Value) : result;

        public static async Task<Result<T>> ToResult<T>(this Task<T> task)
        {
            try
            {
                return Result<T>.Success(await task);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex.Message);
            }
        }
    }
}
