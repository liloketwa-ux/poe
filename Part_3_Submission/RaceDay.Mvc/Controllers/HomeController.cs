using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class HomeController(RaceDayApiClient api):Controller
{
    public async Task<IActionResult> Index(string? eventType,string? province,CancellationToken token)
    {
        var query=new List<string>();if(!string.IsNullOrWhiteSpace(eventType))query.Add("eventType="+Uri.EscapeDataString(eventType));if(!string.IsNullOrWhiteSpace(province))query.Add("province="+Uri.EscapeDataString(province));
        var path="/api/events"+(query.Count>0?"?"+string.Join("&",query):"");
        var r=await api.GetAsync<List<EventSummaryDto>>(path,token);ViewBag.Error=r.Error;ViewBag.EventType=eventType;ViewBag.Province=province;return View(r.Data??[]);
    }
    public IActionResult Error()=>View();
}