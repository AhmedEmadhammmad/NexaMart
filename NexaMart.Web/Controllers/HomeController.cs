using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NexaMart.Application.Interfaces.Services;
using NexaMart.Web.Mappings;
using NexaMart.Web.Models;

namespace NexaMart.Web.Controllers;

/// <summary>
/// Storefront entrance controller powering the homepage with featured categories,
/// trending products, and new arrivals.
/// </summary>
public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        ILogger<HomeController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllActiveCategoriesAsync(HttpContext.RequestAborted);
        var topSelling = await _productService.GetTopSellingProductsAsync(8, HttpContext.RequestAborted);
        var allProducts = await _productService.GetAllProductsAsync(activeOnly: true, HttpContext.RequestAborted);

        var model = new HomeViewModel
        {
            FeaturedCategories = categories.Take(6).Select(c => c.ToCardViewModel()).ToList(),
            TopSellingProducts = topSelling.Select(p => p.ToCardViewModel()).ToList(),
            LatestProducts = allProducts.Take(8).Select(p => p.ToCardViewModel()).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult ContactUs()
    {
        return View(new ContactComplaintViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitComplaint(ContactComplaintViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all required fields accurately before submitting.";
            return View("ContactUs", model);
        }

        var ticketId = $"NX-{Random.Shared.Next(100000, 999999)}";
        _logger.LogInformation("New complaint/inquiry registered: Ticket {TicketId}, Type {InquiryType}, Priority {Priority}, Email {Email}",
            ticketId, model.InquiryType, model.Priority, model.Email);

        TempData["SuccessMessage"] = $"Thank you, {model.FullName}! Your message has been logged under Ticket #{ticketId}. Our dedicated resolution team will review your case and contact you at {model.Email} within 24 hours.";

        return RedirectToAction(nameof(ContactUs));
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
