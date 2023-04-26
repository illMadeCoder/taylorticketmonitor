using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
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

    public EventsViewModel EventsViewModel { get; set; }
    public IList<LocationDays> LocationDays { get; set; }

    public PartialViewResult OnGetTableContent(string locationDayPairs)
    {
        var locationDays = ParseLocationDayPairs(locationDayPairs);
        LoadEvents(locationDays);
        return Partial("_TableContent", EventsViewModel);
    }

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
        EventsViewModel = new EventsViewModel();
        EventsViewModel.Events = new List<Event>();
        EventsViewModel.RmEvents = new List<RmEvent>();

        var uniqueLocations = locationDays.Select(ld => ld.Location).Distinct();

        foreach (var location in uniqueLocations)
        {
            Console.WriteLine(location);
            var firstWord = location.Split(" ").First();
            Console.WriteLine(firstWord);
            var query = _context.events.Where(x => x.url.Contains(firstWord)).Take(100).ToList();
            EventsViewModel.Events = EventsViewModel.Events.Concat(query).ToList();
        }

        // Assuming Events is a List<Event>
        EventsViewModel.Events = EventsViewModel.Events.OrderByDescending(x => x.insertdatetime).ToList();

        EventsViewModel.EventPrevPrice = EventsViewModel.Events.Select(x =>
        {
            var previousEvent = EventsViewModel.Events
                .Where(e => e.rowid == x.rowid && e.url == x.url && e.id != x.id)
                .OrderByDescending(e => e.insertdatetime)                
                .FirstOrDefault();

            return new EventPrevPrice
            {
                id = x.id,
                prevprice = previousEvent == null ? 0 : previousEvent.price
            };
        }).ToList();
    }

    private void LogUserIpAddress()
    {
        var userIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
        _context.userIPs.Add(new UserIPs { ip = userIpAddress.ToString() });
        _context.SaveChanges();
    }
}