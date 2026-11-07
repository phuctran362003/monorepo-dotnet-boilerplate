using Microsoft.AspNetCore.Mvc;

namespace {{Namespace}}.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
