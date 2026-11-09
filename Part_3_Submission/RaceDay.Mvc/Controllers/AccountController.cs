using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public sealed class AccountController(RaceDayApiClient api):Controller
{
    [HttpGet]public IActionResult Login()=>View(new LoginViewModel());
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Login(LoginViewModel model,CancellationToken token)
    {
        if(!ModelState.IsValid)return View(model);
        var r=await api.PostAsync<LoginViewModel,UserDto>("/api/auth/login",model,token);
        if(!r.Success||r.Data is null){ModelState.AddModelError(" ",r.Error??"Login failed.");return View(model);}
        HttpContext.Session.SetInt32("UserId",r.Data.UserId);HttpContext.Session.SetString("Role",r.Data.Role);HttpContext.Session.SetString("DisplayName",r.Data.FirstName+" "+r.Data.LastName);
        return RedirectToAction("Index","Dashboard");
    }
    [HttpGet]public IActionResult Register()=>View(new RegisterViewModel());
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Register(RegisterViewModel model,CancellationToken token)
    {
        if(!ModelState.IsValid)return View(model);
        if(model.Role is not ("Organiser" or "Participant")){ModelState.AddModelError(nameof(model.Role),"Choose Organiser or Participant.");return View(model);}
        var r=await api.PostAsync<RegisterViewModel,UserDto>("/api/auth/register",model,token);
        if(!r.Success){ModelState.AddModelError("",r.Error??"Registration failed.");return View(model);}
        var login=await api.PostAsync<LoginViewModel,UserDto>("/api/auth/login",new LoginViewModel{Email=model.Email,Password=model.Password},token);
        if(login.Success&&login.Data is not null){HttpContext.Session.SetInt32("UserId",login.Data.UserId);HttpContext.Session.SetString("Role",login.Data.Role);HttpContext.Session.SetString("DisplayName",login.Data.FirstName+" "+login.Data.LastName);return RedirectToAction("Index","Dashboard");}
        TempData["Notice"]="Account created. Please log in.";return RedirectToAction(nameof(Login));
    }
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Logout(CancellationToken token)
    {
        _=await api.PostAsync<object,object>("/api/auth/logout",new{},token);api.ClearApiSession();HttpContext.Session.Clear();return RedirectToAction("Index","Home");
    }
}