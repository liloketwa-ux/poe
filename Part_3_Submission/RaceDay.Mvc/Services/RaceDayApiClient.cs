using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
namespace RaceDay.Mvc.Services;

/// <summary>Server-side API client. The MVC application never connects directly to SQL Server or Azure.</summary>
public sealed class RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor)
{
    private const string CookieKey = "RaceDay.ApiCookie";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken token = default) => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, path), token);
    public Task<ApiResult<TOut>> PostAsync<TIn,TOut>(string path,TIn body,CancellationToken token=default) =>
        SendAsync<TOut>(new HttpRequestMessage(HttpMethod.Post,path){Content=JsonContent.Create(body)},token);
    public Task<ApiResult<TOut>> PutAsync<TIn,TOut>(string path,TIn body,CancellationToken token=default) =>
        SendAsync<TOut>(new HttpRequestMessage(HttpMethod.Put,path){Content=JsonContent.Create(body)},token);
    public async Task<ApiResult<bool>> DeleteAsync(string path,CancellationToken token=default)
    {
        var r=await SendAsync<JsonElement>(new HttpRequestMessage(HttpMethod.Delete,path),token);
        return new ApiResult<bool>(r.Success,r.Success,r.Error,r.StatusCode);
    }
    public async Task<ApiResult<T>> UploadAsync<T>(string path,IFormFile file,CancellationToken token=default)
    {
        await using var stream=file.OpenReadStream();
        using var form=new MultipartFormDataContent();
        using var content=new StreamContent(stream);
        content.Headers.ContentType=new MediaTypeHeaderValue(file.ContentType);
        form.Add(content,"file",Path.GetFileName(file.FileName));
        return await SendAsync<T>(new HttpRequestMessage(HttpMethod.Post,path){Content=form},token);
    }
    public void ClearApiSession()=>accessor.HttpContext?.Session.Remove(CookieKey);
    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request,CancellationToken token)
    {
        var context=accessor.HttpContext;
        var cookie=context?.Session.GetString(CookieKey);
        if(!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie",cookie);
        try
        {
            using(request)
            using(var response=await http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,token))
            {
                if(context is not null && response.Headers.TryGetValues("Set-Cookie",out var values))
                {
                    var authCookie=values.FirstOrDefault(x=>x.StartsWith("RaceDay.Session=",StringComparison.OrdinalIgnoreCase));
                    if(authCookie is not null) context.Session.SetString(CookieKey,authCookie.Split(';',2)[0]);
                }
                var body=response.Content is null?null:await response.Content.ReadAsStringAsync(token);
                T? data=default;
                if(!string.IsNullOrWhiteSpace(body)){try{data=JsonSerializer.Deserialize<T>(body,JsonOptions);}catch(JsonException){}}
                return new ApiResult<T>(response.IsSuccessStatusCode,data,response.IsSuccessStatusCode?null:Error(body,response.StatusCode),response.StatusCode);
            }
        }
        catch(HttpRequestException){return new(false,default,"The RaceDay API is unavailable. Check that the API is running.",HttpStatusCode.BadGateway);}
        catch(TaskCanceledException) when(!token.IsCancellationRequested){return new(false,default,"The API request timed out.",HttpStatusCode.GatewayTimeout);}
    }
    private static string Error(string? body,HttpStatusCode status)
    {
        if(!string.IsNullOrWhiteSpace(body)){try{using var doc=JsonDocument.Parse(body);foreach(var k in new[]{"message","detail","title"})if(doc.RootElement.TryGetProperty(k,out var p))return p.GetString()??body;}catch(JsonException){}}
        return $"API returned {(int)status} ({status}).";
    }
}