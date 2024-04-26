using System.Net.Mime;
using CDRSandbox.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using NSwag.Annotations;

namespace CDRSandbox.Controllers;

[ApiController]
[Route("[controller]")]
[OpenApiTag("Call Detail Record", Description = "Consumes CSV files and computes metrics on existent data")]
public class CdrController : ControllerBase
{
    public CdrController(ILogger<CdrController> logger)
    {
    }

    
    // Code was taken for MS sample https://github.com/dotnet/AspNetCore.Docs/tree/main/aspnetcore/mvc/models/file-uploads/samples/5.x
    [HttpPost("[action]")]
    [ConsumesLargeFile("The CDR CSV file.")] // Workaround: This will write on the OpenAPI the large file specification
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest, MediaTypeNames.Text.Plain)]
    [ForceResponseContentType(StatusCodes.Status400BadRequest, MediaTypeNames.Text.Plain)] // Workaround: This will write on the OpenAPI the correct output headers
    public async Task<IActionResult> UploadFile()
    {
        var request = HttpContext.Request;

        // validation of Content-Type
        // 1. first, it must be a form-data request
        // 2. a boundary should be found in the Content-Type
        if (!request.HasFormContentType ||
            !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
            string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
        {
            return new UnsupportedMediaTypeResult();
        }

        var boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeader.Boundary.Value).Value;
        var reader = new MultipartReader(boundary, request.Body);
        var section = await reader.ReadNextSectionAsync();

        // This sample try to get the first file from request and save it
        // Make changes according to your needs in actual use
        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                out var contentDisposition);

            if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                // TODO
                // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                // In short, it is necessary to restrict and verify the upload
                // Here, we just use the temporary folder and a random file name

                // Get the temporary folder, and combine a random file name with it
                var fileName = Path.GetRandomFileName();
                var saveToPath = Path.Combine(Path.GetTempPath(), fileName);

                using (var targetStream = System.IO.File.Create(saveToPath))
                {
                    await section.Body.CopyToAsync(targetStream);
                }

                return Ok();
            }

            section = await reader.ReadNextSectionAsync();
        }

        // If the code runs to this location, it means that no files have been saved
        return BadRequest("No files data in the request.");
    }
}