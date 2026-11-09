using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;
namespace RaceDay.Mvc.Controllers;
public sealed class ProfileController(RaceDayApiClient api):Controller
{
    [HttpGet]public async Task<IActionResult> Index(CancellationToken token)
    {
        if(HttpContext.Session.GetString("Role") is null)return RedirectToAction("Login","Account");
        var r=await api.GetAsync<UserDto>("/api/profile/me",token);if(!r.Success||r.Data is null){ViewBag.Error=r.Error;return View(new ProfileViewModel());}
        var x=r.Data;return View(new ProfileViewModel{FirstName=x.FirstName,LastName=x.LastName,Email=x.Email,Role=x.Role,Phone=x.Phone,ProfileImageUrl=x.ProfileImageUrl});
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Index(ProfileViewModel model,CancellationToken token)
    {
        if(HttpContext.Session.GetString("Role") is null)return RedirectToAction("Login","Account");if(!ModelState.IsValid)return View(model);
        var r=await api.PutAsync<object,UserDto>("/api/profile/me",new{model.FirstName,model.LastName,model.Phone},token);
        if(!r.Success){ModelState.AddModelError("",r.Error??"Profile update failed.");return View(model);}TempData["Notice"]="Profile updated.";return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> UploadPicture(IFormFile file,CancellationToken token)
    {
        if(HttpContext.Session.GetString("Role") is null)return RedirectToAction("Login","Account");var r=await api.UploadAsync<JsonElement>("/api/profile/me/picture",file,token);TempData[r.Success?"Notice":"Error"]=r.Success?"Profile picture uploaded.":r.Error;return RedirectToAction(nameof(Index));
    }
}