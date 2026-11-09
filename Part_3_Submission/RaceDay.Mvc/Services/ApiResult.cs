using System.Net;
namespace RaceDay.Mvc.Services;
public sealed record ApiResult<T>(bool Success, T? Data, string? Error, HttpStatusCode StatusCode);