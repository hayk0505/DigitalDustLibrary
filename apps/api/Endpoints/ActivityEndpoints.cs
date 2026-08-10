using System.Security.Claims;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class ActivityEndpoints
{
    public static void MapActivityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity").WithTags("Activity").RequireAuthorization();

        // GET /api/activity — the full unfiltered list (everyone's actions,
        // including sensitive ones like role changes and deactivations)
        // stays Owner-only. ?mine=true lets any authenticated role see just
        // their own logged actions — already implicitly known to them (they
        // did it), so no new information is exposed.
        group.MapGet("/", async (bool? mine, AppDbContext db, ClaimsPrincipal user) =>
        {
            IQueryable<ActivityLogEntry> query = db.ActivityLog.Include(e => e.Actor);

            if (mine == true)
            {
                var actorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
                query = query.Where(e => e.ActorId == actorId);
            }
            else if (!user.IsInRole("Owner"))
            {
                return Results.Json(new { message = "Forbidden" }, statusCode: 403);
            }

            var entries = await query.OrderByDescending(e => e.CreatedAt).Take(50).ToListAsync();
            return Results.Ok(entries.Select(e => e.ToDto()));
        });
    }
}
