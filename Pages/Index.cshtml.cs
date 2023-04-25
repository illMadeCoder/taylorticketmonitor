using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexModel(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public IList<Event> Data { get; set; }
    public IList<RmEvent> RmEvents { get; set; }
    public IList<LocationDays> LocationDays { get; set; }
    public List<String> Tags { get; set; }

    public void OnGet()
    {
        Data = _context.events.ToList();
        RmEvents = _context.rmEvents.ToList();
        LocationDays = _context.locationDays.ToList();

        // Get the user's IP address
        var userIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
        // Log the user's IP address
        Console.WriteLine(userIpAddress);
    }
}