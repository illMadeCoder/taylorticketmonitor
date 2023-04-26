using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Linq;

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
        var locationDays = ParseLocationDayPairs(locationDayPairs);
        LoadLocationDays();
        LoadEvents(locationDays);
        LogUserIpAddress();
    }

    private List<LocationDayPairViewModel> ParseLocationDayPairs(string locationDayPairs)
    {
        if (string.IsNullOrEmpty(locationDayPairs))
        {
            return new List<LocationDayPairViewModel>();
        }

        return locationDayPairs.Split(',')
            .Select(ldp => ldp.Split(':'))
            .Select(parts => new LocationDayPairViewModel { Location = parts[0], Day = parts[1] })
            .ToList();
    }

    private void LoadLocationDays()
    {
        LocationDays = _context.locationDays.ToList();
    }

    private void LoadEvents(List<LocationDayPairViewModel> locationDays)
    {
        Events = new List<Event>();
        RmEvents = new List<RmEvent>();

        var uniqueLocations = locationDays.Select(ld => ld.Location).Distinct();

        foreach (var location in uniqueLocations)
        {
            var firstWord = location.Split(" ").First();
            var query = _context.events.Where(x => x.url.Contains(firstWord)).ToList();
            Events = Events.Concat(query).ToList();
        }

        Events = Events
            .OrderByDescending(x => x.insertdatetime)
            .ToList();
    }

    private void LogUserIpAddress()
    {
        var userIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
        _context.userIPs.Add(new UserIPs { ip = userIpAddress.ToString() });
        _context.SaveChanges();
    }
}