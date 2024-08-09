using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Shared;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }
        else
        {
            return new ObjectResult(result.Error) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public static ActionResult ToActionResult<T>(this Result<T> result, Func<T, ActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }
        else
        {
            return new ObjectResult(result.Error) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    public static Result<T> PropagateResult<T>(this Result<T?> result)
        where T : class
    {
        if (!result.IsSuccess)
        {
            return Result<T>.Failure(result.Error);
        }

        if (result.Value is null)
        {
            return Result<T>.Failure("Value is null");
        }

        return Result<T>.Success(result.Value);
    }

    public static Result<T> PropagateResult<T>(this Result<bool> result, T successValue)
    {
        if (!result.IsSuccess)
        {
            return Result<T>.Failure(result.Error);
        }

        return Result<T>.Success(successValue);
    }

}
