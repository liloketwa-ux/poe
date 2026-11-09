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
        if(!r.Success){ModelState.AddModelError("",r.Error??"Category could not be created.");return View(model);}TempData["Notice"]="Category created.";return RedirectToAction("Details","Events",new{id=model.EventId});
    }
    [HttpGet]public async Task<IActionResult> Edit(int eventId,int categoryId,CancellationToken token)
    {
        if(!IsOrganiser())return Denied();
        var response=await api.GetAsync<EventDetailsDto>($"/api/events/{eventId}",token);
        var category=response.Data?.Categories.FirstOrDefault(x=>x.CategoryId==categoryId);
        if(!response.Success||category is null)return NotFound();
        return View("Create",new CategoryFormViewModel{EventId=eventId,CategoryId=categoryId,Name=category.Name,DistanceKm=category.DistanceKm,EntryFee=category.EntryFee,MaxEntries=category.MaxEntries,MinAge=category.MinAge,MaxAge=category.MaxAge});
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Edit(CategoryFormViewModel model,CancellationToken token)
    {
        if(!IsOrganiser())return Denied();if(!ModelState.IsValid)return View("Create",model);
        var response=await api.PutAsync<object,CategoryDto>($"/api/events/{model.EventId}/categories/{model.CategoryId}",new{model.Name,model.DistanceKm,model.EntryFee,model.MaxEntries,model.MinAge,model.MaxAge},token);
        if(!response.Success){ModelState.AddModelError("",response.Error??"Category could not be updated.");return View("Create",model);}
        TempData["Notice"]="Category updated.";return RedirectToAction("Details","Events",new{id=model.EventId});
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Delete(int eventId,int categoryId,CancellationToken token)
    {
        if(!IsOrganiser())return Denied();
        var response=await api.DeleteAsync($"/api/events/{eventId}/categories/{categoryId}",token);
        TempData[response.Success?"Notice":"Error"]=response.Success?"Category deleted.":response.Error??"Category could not be deleted.";
        return RedirectToAction("Details","Events",new{id=eventId});
    }
    private bool IsOrganiser()=>HttpContext.Session.GetString("Role")=="Organiser";
    private IActionResult Denied()=>HttpContext.Session.GetString("Role") is null?RedirectToAction("Login","Account"):AccessDenied();
    private IActionResult AccessDenied(){TempData["Error"]="You do not have access to that page.";return RedirectToAction("Index","Dashboard");}
}