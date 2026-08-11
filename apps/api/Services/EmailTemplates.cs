namespace DigitalDustLibrary.Api.Services;

public static class EmailTemplates
{
    public static (string Subject, string Html) Approved(string name, string inviteUrl) => (
        "Your Digital Dust Library author application was approved",
        $"""
        <p>Hi {name},</p>
        <p>Your application to write for Digital Dust Library has been approved.</p>
        <p><a href="{inviteUrl}">Set your password</a> to finish setting up your account.</p>
        """
    );

    public static (string Subject, string Html) Rejected(string name) => (
        "Your Digital Dust Library author application",
        $"""
        <p>Hi {name},</p>
        <p>Thanks for your interest in writing for Digital Dust Library. We won't be moving
        forward with your application at this time.</p>
        """
    );

    public static (string Subject, string Html) Invited(string name, string inviteUrl) => (
        "You've been invited to write for Digital Dust Library",
        $"""
        <p>Hi {name},</p>
        <p>You've been invited to write for Digital Dust Library.</p>
        <p><a href="{inviteUrl}">Set your password</a> to finish setting up your account.</p>
        """
    );
}
