namespace DigitalDustLibrary.Api.Models;


public enum Role
{
    Author,
    Editor,
    Owner,
}


public enum PostStatus
{
    Draft,
    PendingReview,
    ChangesRequested,
    Published,
}

public enum MediaTag
{
    Featured,
    Inline,
    OgImage,
    Avatar,
}

public enum PostFormat
{
    Mini,
    Shelf,
}
