using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexModel(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public IList<Event> Events { get; set; }
    public IList<RmEvent> RmEvents { get; set; }
    public IList<LocationDays> LocationDays { get; set; }

    public void OnGet(string locationDayPairs)
    {
        List<LocationDayPairViewModel> locationDays = new List<LocationDayPairViewModel>();
        if (!string.IsNullOrEmpty(locationDayPairs))
        {
            Console.WriteLine(locationDayPairs);
            Console.WriteLine("");
            locationDays = locationDayPairs.Split(',')
                .Select(ldp => {
                    Console.WriteLine(locationDayPairs);
                    return ldp.Split(':');
                    })
                .Select(parts => new LocationDayPairViewModel { Location = parts[0], Day = parts[1] })
                .ToList();
            Console.WriteLine($"{locationDays[0].Location.Split(" ").First()} {locationDays[0].Day}");            
        }
        // Get all LocationDays
        LocationDays = _context.locationDays.ToList();
        Events = new List<Event>();
        RmEvents = new List<RmEvent>();
        
        List<string> locationsProcessed = new List<string>();
        foreach (var el in locationDays) {
             //&& x.insertdatetime >= DateTime.Now.AddDays(-2).ToUniversalTime()
            if (!locationsProcessed.Contains(el.Location)) {
                locationsProcessed.Add(el.Location);
                var firstWord = el.Location.Split(" ").First();
                List<Event> query = _context.events.Where(x => x.url.Contains(firstWord)).ToList();
                Events = Events.Concat(query).ToList();
                Console.WriteLine(el.Location);
             }
        }        

// .GroupBy(x => x.rowid)
//                 .Select(group => group.MaxBy(x => x.insertdatetime))
        Events = Events
                .OrderByDescending(x => x.insertdatetime)
                .ToList();
        Console.WriteLine($"here {Events.Count}");



        // Get the user's IP address
        var userIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;

        // Log the user's IP address
        _context.userIPs.Add(new UserIPs { ip = userIpAddress.ToString() });
        _context.SaveChanges();
    }
}