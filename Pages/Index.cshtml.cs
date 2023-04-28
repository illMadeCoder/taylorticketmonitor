using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Linq;
using System.Text.RegularExpressions;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexModel(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        LocationsQueried = new List<string>();
    }

    public EventsViewModel EventsViewModel { get; set; }
    public IList<LocationDays> LocationDays { get; set; }
    public IList<string> LocationsQueried { get; set; }

    public PartialViewResult OnGetTableContent(string locationDayPairs)
    {
        var locationDays = ParseLocationDayPairs(locationDayPairs);
        LoadEvents(locationDays);
        LogUserIpAddress();
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
            var query = _context.events.Where(x => x.url.Contains(firstWord)).OrderByDescending(x => x.id).ToList();
            EventsViewModel.Events = EventsViewModel.Events.Concat(query).ToList();
            LocationsQueried.Add(firstWord);            
        }

        // Assuming Events is a List<Event>
        var groupedEvents = EventsViewModel.Events.OrderByDescending(x => x.id)
            .GroupBy(x =>
            {
                var match = Regex.Match(x.url, @"quantity=(\d)");
                return match.Success ? int.Parse(match.Groups[1].Value) : throw new ArgumentException();
            })    
            .Select(x => x.OrderByDescending(x => x.id).Take(100))
            .SelectMany(x => x)
            .OrderByDescending(x => x.id)
            .ToList();

        //Console.WriteLine(string.Join(",", groupedEvents.Select(x => x.id)));

        EventsViewModel.Events = groupedEvents;
        
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

        EventsViewModel.RmEvents = _context.rmEvents.ToList();
    }

    private void LogUserIpAddress()
    {
        var userIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
        var request = _httpContextAccessor.HttpContext.Request;
        var requestedUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        var locations = string.Join(",", LocationsQueried);
        Console.WriteLine($"locations {locations}");
        _context.userIPs.Add(new UserIPs { ip = userIpAddress.ToString(), url = requestedUrl, locations = locations});
        _context.SaveChanges();
    }
   }