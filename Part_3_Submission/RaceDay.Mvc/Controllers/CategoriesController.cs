using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class CategoriesController(RaceDayApiClient api):Controller
{
    [HttpGet]public IActionResult Create(int eventId)=>IsOrganiser()?View(new CategoryFormViewModel{EventId=eventId}):Denied();
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Create(CategoryFormViewModel model,CancellationToken token)
    {
        if(!IsOrganiser())return Denied();if(!ModelState.IsValid)return View(model);
        var r=await api.PostAsync<object,CategoryDto>($"/api/events/{model.EventId}/categories",new{model.Name,model.DistanceKm,model.EntryFee,model.MaxEntries,model.MinAge,model.MaxAge},token);
        if(!r.Success){ModelState.AddModelError("",r.Error??"Category could not be created.");return View(model);}return RedirectToAction("Details","Events",new{id=model.EventId});
    }
    private bool IsOrganiser()=>HttpContext.Session.GetString("Role")=="Organiser";
    private IActionResult Denied()=>HttpContext.Session.GetString("Role") is null?RedirectToAction("Login","Account"):Forbid();
}