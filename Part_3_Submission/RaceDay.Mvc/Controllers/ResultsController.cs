using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class ResultsController(RaceDayApiClient api):Controller
{
    [HttpGet]public async Task<IActionResult> Entry(int eventId,CancellationToken token)
    {
        if(!IsOrganiser())return Denied();var ev=await api.GetAsync<EventDetailsDto>($"/api/events/{eventId}",token);if(!ev.Success||ev.Data is null)return NotFound();
        var entries=await api.GetAsync<List<EnrolmentDto>>($"/api/events/{eventId}/enrolments",token);var results=await api.GetAsync<List<ResultDto>>($"/api/events/{eventId}/results",token);
        return View(new ResultsEntryPageViewModel{EventId=eventId,EventName=ev.Data.Name,Enrolments=entries.Data??[],Results=results.Data??[],Form=new ResultCaptureViewModel{EventId=eventId}});
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Capture(ResultCaptureViewModel form,CancellationToken token)
    {
        if(!IsOrganiser())return Denied();if(!ModelState.IsValid)return RedirectToAction(nameof(Entry),new{eventId=form.EventId});
        var r=await api.PostAsync<object,ResultDto>($"/api/events/{form.EventId}/results",new{form.EnrolmentId,form.FinishTime,form.OverallPosition,form.CategoryPosition,form.AveragePace,form.FinishStatus},token);
        TempData[r.Success?"Notice":"Error"]=r.Success?"Result recorded.":r.Error;return RedirectToAction(nameof(Entry),new{eventId=form.EventId});
    }
    private bool IsOrganiser()=>HttpContext.Session.GetString("Role")=="Organiser";
    private IActionResult Denied()=>HttpContext.Session.GetString("Role") is null?RedirectToAction("Login","Account"):AccessDenied();
    private IActionResult AccessDenied(){TempData["Error"]="You do not have access to that page.";return RedirectToAction("Index","Dashboard");}
}