using AuthorizationApi.Database.Models;
using AuthorizationService.Common;
using AuthorizationService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace AuthorizationService.Controllers;

public class RedirectionSettings
{
    public required string RoomManagerApiPath { get; set; }
    public required string StatisticServiceApiPath { get; set; }
}

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class EndpointController : Controller
{
    private readonly IJwtTokenToolsService jwtTokenToolsService;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<EndpointController> logger;
    private readonly RedirectionSettings redirectionSettings;

    public EndpointController(
        IJwtTokenToolsService jwtTokenToolsService,
        IHttpClientFactory httpClientFactory,
        ILogger<EndpointController> logger,
        IOptions<RedirectionSettings> redirectionSettings
    )
    {
        this.redirectionSettings = redirectionSettings.Value;
        this.jwtTokenToolsService = jwtTokenToolsService;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    [Route("/Redirect/{*url}")]
    public async Task<IActionResult> Redirect()
    {
        logger.LogDefaultInfo(Request);
        logger.LogInformation("Switch: " + Request.Path);
        if (
            Request.Headers.TryGetValue("JwtToken", out StringValues token)
            && jwtTokenToolsService.ValidateToken(token.ToString(), out User? user)
        )
        {
            HttpRequestMessage httpRequestMessage;
            switch (Request.Path.Value?.Replace("/Redirect", ""))
            {
                case "/Rooms":
                    httpRequestMessage = new(
                        HttpMethod.Get,
                        redirectionSettings.RoomManagerApiPath + "/Rooms"
                    );
                    break;
                case string str when str.StartsWith("/Rooms"):
                    httpRequestMessage = new(
                        HttpMethod.Get,
                        redirectionSettings.RoomManagerApiPath + str
                    );
                    break;
                case "/Statistic" when Request.Method == HttpMethod.Get.Method:
                    httpRequestMessage = new(
                        HttpMethod.Get,
                        redirectionSettings.StatisticServiceApiPath
                            + $"/Statistic?UserId={user?.Id}"
                    );
                    break;
                case "/Statistic" when Request.Method == HttpMethod.Put.Method:
                    httpRequestMessage = new(
                        HttpMethod.Put,
                        redirectionSettings.StatisticServiceApiPath
                            + $"/Statistic?UserId={user?.Id}"
                    );
                    httpRequestMessage.Headers.Add("Content-type", "application/json");
                    StreamReader body = new(Request.Body);
                    _ = body.BaseStream.Seek(0, SeekOrigin.Begin);
                    string requestBody = body.ReadToEnd();
                    httpRequestMessage.Content = new StringContent(requestBody);
                    break;
                default:
                    return BadRequest(Request);
            }

            HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(
                httpRequestMessage
            );

            return Ok(httpResponseMessage.Content.ReadAsStream());
        }
        else
        {
            return Unauthorized();
        }
    }
}
