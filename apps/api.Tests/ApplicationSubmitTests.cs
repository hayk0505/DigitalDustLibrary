using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

// Deliberately its own dedicated ApiFactory (own Postgres container), not part
// of ApiCollection — see this file's note in the plan for why: it's the only
// test that exercises the rate-limited POST /api/applications endpoint, and
// the limiter's in-memory state lives for the app instance's whole lifetime.
public class ApplicationSubmitTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_FirstFiveRequestsSucceed_SixthIsRateLimited()
    {
        var client = factory.CreateClient();

        for (var i = 0; i < 5; i++)
        {
            var response = await client.PostAsJsonAsync("/api/applications",
                new CreateAuthorApplicationRequest($"Applicant {i}", $"applicant{i}@example.com", "I want to write."));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            if (i == 0)
            {
                var created = await response.Content.ReadFromJsonAsync<AuthorApplicationDto>(AuthHelper.JsonOptions);
                Assert.Equal("Applicant 0", created!.Name);
                Assert.Equal("applicant0@example.com", created.Email);
                Assert.Equal("I want to write.", created.Pitch);
                Assert.Equal(ApplicationStatus.Pending, created.Status);
            }
        }

        var sixthResponse = await client.PostAsJsonAsync("/api/applications",
            new CreateAuthorApplicationRequest("Applicant 5", "applicant5@example.com", "I want to write."));

        Assert.Equal(HttpStatusCode.TooManyRequests, sixthResponse.StatusCode);
    }
}
