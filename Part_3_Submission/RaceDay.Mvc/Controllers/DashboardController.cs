using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class DashboardController(RaceDayApiClient api):Controller
{
    public async Task<IActionResult> Index(CancellationToken token)
    {
        var role=HttpContext.Session.GetString("Role");if(role is null)return RedirectToAction("Login","Account");
        var model=new DashboardViewModel{Role=role};
        if(role=="Organiser")
        {
            var events=await api.GetAsync<List<EventSummaryDto>>("/api/events/mine",token);if(!events.Success){ViewBag.Error=events.Error;return View(model);}model.Events=events.Data??[];
            foreach(var ev in model.Events){var entries=await api.GetAsync<List<EnrolmentDto>>($"/api/events/{ev.EventId}/enrolments",token);model.EnrolmentCounts[ev.EventId]=entries.Data?.Count(x=>x.Status=="Confirmed")??0;}
        }
        else if(role=="Participant")
        {
            var entries=await api.GetAsync<List<EnrolmentDto>>("/api/enrolments/me",token);var results=await api.GetAsync<List<ResultDto>>("/api/results/me",token);model.Enrolments=entries.Data??[];model.Results=results.Data??[];
        }
        else{HttpContext.Session.Clear();return RedirectToAction("Login","Account");}
        return View(model);
    }
}