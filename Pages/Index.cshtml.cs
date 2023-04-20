using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Event> Data { get; set; }

    public void OnGet()
    {
        Data = _context.events.ToList().OrderByDescending(x => x.insertdatetime).ToList();
    }
}