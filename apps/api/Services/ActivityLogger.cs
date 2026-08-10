using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Services;

public static class ActivityLogger
{
    public static void Log(AppDbContext db, Guid actorId, string action)
    {
        db.ActivityLog.Add(new ActivityLogEntry { ActorId = actorId, Action = action });
    }
}
