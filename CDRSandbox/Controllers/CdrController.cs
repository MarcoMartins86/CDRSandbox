using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using CDRSandbox.Attributes.OpenApi;
using CDRSandbox.Controllers.Dto;
using CDRSandbox.Services;
using CDRSandbox.Services.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using NSwag.Annotations;

namespace CDRSandbox.Controllers;

[ApiController]
[Route("[controller]")]
[OpenApiTag("Call Detail Record", Description = "Consumes CSV files and computes metrics on existent data")]
public class CdrController(CdrService service) : ControllerBase
{
    [HttpPost("[action]")]
    [ConsumesLargeFile(
        "The Call Detail Record CSV file.")] // Workaround: This will write on the OpenAPI the large file specification
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    // Code was taken for MS sample https://github.com/dotnet/AspNetCore.Docs/tree/main/aspnetcore/mvc/models/file-uploads/samples/5.x
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

                try
                {
                    var numberLinesStored = await service.ProcessCsvFileAndStoreAsync(section.Body);
                    return Ok(numberLinesStored);
                }
                catch (ReaderException e)
                {
                    return BadRequest(
                        $"Failed to parse CSV file, maybe not in the correct format. Reason: [{e.InnerException?.Message ?? e.Message}]");
                }
                catch (Exception) // So that we won't expose any sensitive data
                {
                    return new ObjectResult(null) { StatusCode = StatusCodes.Status500InternalServerError };
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        // If the code runs to this location, it means that no files have been saved
        return BadRequest("No files data in the request.");
    }

    [HttpGet("{reference}")]
    [ProducesResponseType(typeof(CdrItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ForceResponseContentType(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Item(
        [FromRoute] [Required(AllowEmptyStrings = false)] [StringLength(34, MinimumLength = 1)][RegularExpression(CdrItem.ReferencePattern)]
        string reference
    )
    {
        var item = await service.FetchItemAsync(reference);
        if (item != null)
        {
            return Ok(CdrItemDto.From(item));
        }
        
        return NotFound($"The item with reference [{reference}] was not found.");
    }
}