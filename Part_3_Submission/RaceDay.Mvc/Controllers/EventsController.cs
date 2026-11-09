using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class EventsController(RaceDayApiClient api):Controller
{
    public async Task<IActionResult> Details(int id,CancellationToken token){var r=await api.GetAsync<EventDetailsDto>($"/api/events/{id}",token);return !r.Success||r.Data is null?View("~/Views/Home/Error.cshtml"):View(r.Data);}
    [HttpGet]public IActionResult Create()=>Role("Organiser")?View("Form",new EventFormViewModel()):Denied();
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Create(EventFormViewModel model,IFormFile? banner,CancellationToken token)
    {
        if(!Role("Organiser"))return Denied();if(!ModelState.IsValid)return View("Form",model);
        var body=Payload(model);var r=await api.PostAsync<object,EventSummaryDto>("/api/events",body,token);
        if(!r.Success){ModelState.AddModelError("",r.Error??"Create failed.");return View("Form",model);}
        if(banner is not null&&banner.Length>0&&r.Data is not null){var up=await api.UploadAsync<System.Text.Json.JsonElement>($"/api/events/{r.Data.EventId}/banner",banner,token);if(!up.Success)TempData["Error"]="Event created, but the banner could not be uploaded: "+up.Error;}
        return RedirectToAction("Index","Dashboard");
    }
    [HttpGet]public async Task<IActionResult> Edit(int id,CancellationToken token)
    {
        if(!Role("Organiser"))return Denied();var r=await api.GetAsync<EventDetailsDto>($"/api/events/{id}",token);if(!r.Success||r.Data is null)return NotFound();
        var x=r.Data;return View("Form",new EventFormViewModel{EventId=x.EventId,Name=x.Name,Description=x.Description,EventDate=x.EventDate,RegistrationOpen=x.RegistrationOpen,RegistrationClose=x.RegistrationClose,Venue=x.Venue,City=x.City,Province=x.Province,DistanceKm=x.DistanceKm,EventType=x.EventType,Status=x.Status});
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Edit(int id,EventFormViewModel model,IFormFile? banner,CancellationToken token)
    {
        if(!Role("Organiser"))return Denied();if(!ModelState.IsValid)return View("Form",model);
        var r=await api.PutAsync<object,EventSummaryDto>($"/api/events/{id}",Payload(model),token);if(!r.Success){ModelState.AddModelError("",r.Error??"Update failed.");return View("Form",model);}
        if(banner is not null&&banner.Length>0){var up=await api.UploadAsync<System.Text.Json.JsonElement>($"/api/events/{id}/banner",banner,token);if(!up.Success)TempData["Error"]="Event updated, but the banner could not be uploaded: "+up.Error;}
        return RedirectToAction(nameof(Details),new{id});
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Delete(int id,CancellationToken token){if(!Role("Organiser"))return Denied();var r=await api.DeleteAsync($"/api/events/{id}",token);TempData[r.Success?"Notice":"Error"]=r.Success?"Event deleted.":r.Error;return RedirectToAction("Index","Dashboard");}
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Enrol(int id,int categoryId,CancellationToken token){if(!Role("Participant"))return Denied();var r=await api.PostAsync<object,EnrolmentDto>($"/api/events/{id}/enrolments",new{categoryId},token);TempData[r.Success?"Notice":"Error"]=r.Success?"Your entry has been recorded.":r.Error;return RedirectToAction(nameof(Details),new{id});}
    [HttpGet]public async Task<IActionResult> Enrolments(int id,CancellationToken token){if(!Role("Organiser"))return Denied();var ev=await api.GetAsync<EventDetailsDto>($"/api/events/{id}",token);var r=await api.GetAsync<List<EnrolmentDto>>($"/api/events/{id}/enrolments",token);if(!ev.Success||ev.Data is null)return NotFound();ViewBag.EventName=ev.Data.Name;ViewBag.Error=r.Error;return View(r.Data??[]);}
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> UploadBanner(int id,IFormFile file,CancellationToken token){if(!Role("Organiser"))return Denied();var r=await api.UploadAsync<System.Text.Json.JsonElement>($"/api/events/{id}/banner",file,token);TempData[r.Success?"Notice":"Error"]=r.Success?"Banner uploaded.":r.Error;return RedirectToAction(nameof(Details),new{id});}
    private static object Payload(EventFormViewModel x)=>new{x.Name,x.Description,x.EventDate,x.RegistrationOpen,x.RegistrationClose,x.Venue,x.City,x.Province,x.DistanceKm,x.EventType,x.Status};
    private bool Role(string role)=>HttpContext.Session.GetString("Role")==role;
    private IActionResult Denied()=>HttpContext.Session.GetString("Role") is null?RedirectToAction("Login","Account"):AccessDenied();
    private IActionResult AccessDenied(){TempData["Error"]="You do not have access to that page.";return RedirectToAction("Index","Dashboard");}
}