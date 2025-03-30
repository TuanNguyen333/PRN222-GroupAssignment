using System.Diagnostics;
using System.Text;

namespace API.Middlewares;

public class RequestResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseMiddleware> _logger;

    public RequestResponseMiddleware(RequestDelegate next, ILogger<RequestResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var request = await FormatRequest(context.Request);
        _logger.LogInformation($"Incoming request: {request}");

        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            var response = await FormatResponse(context.Response);
            _logger.LogInformation($"Outgoing response: {response}");

            await responseBody.CopyToAsync(originalBodyStream);
        }

        stopwatch.Stop();
        _logger.LogInformation($"Request processing time: {stopwatch.ElapsedMilliseconds}ms");
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();

        var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
        await request.Body.ReadAsync(buffer);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        request.Body.Seek(0, SeekOrigin.Begin);

        var headers = string.Join("; ", request.Headers.Select(h => $"{h.Key}: {h.Value}"));

        return $"Method: {request.Method}, Path: {request.Path}, QueryString: {request.QueryString}, Headers: [{headers}], Body: {bodyAsText}";
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        var headers = string.Join("; ", response.Headers.Select(h => $"{h.Key}: {h.Value}"));

        return $"Status Code: {response.StatusCode}, Headers: [{headers}], Body: {text}";
    }
}