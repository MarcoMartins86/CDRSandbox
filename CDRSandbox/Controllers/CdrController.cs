using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using CDRSandbox.Attributes.OpenApi;
using CDRSandbox.Controllers.Dto;
using CDRSandbox.Helpers;
using CDRSandbox.Services;
using CDRSandbox.Services.Models;
using CDRSandbox.Services.Models.ValueObjects;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using NSwag.Annotations;

namespace CDRSandbox.Controllers;

[ApiController]
[Route("[controller]")]
[OpenApiTag("Call detail record", Description = "Consumes CSV files into the database, fetch records and computes metrics")]
public class CdrController(CdrService service) : ControllerBase
{
    [HttpPost("[action]")]
    [ConsumesLargeFile("The call detail record CSV file.")] // Workaround: This will write on the OpenAPI the large file specification
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ForceResponseContentType(StatusCodes.Status200OK, MediaTypeNames.Application.Json, 
        "Returns the number of records inserted in the database")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [OpenApiOperation("Upload CSV file", "Upload CSV file with the call detail record dataset to store them on the database")]
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
    [ForceResponseContentType(StatusCodes.Status200OK, MediaTypeNames.Application.Json, 
        "Returns a call detail record item")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ForceResponseContentType(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [OpenApiOperation("Fetch a call detail record", "Fetch a call detail record given its reference")]
    public async Task<IActionResult> Item(
        [FromRoute] [Required] [RegularExpression(CdrItem.ReferencePattern)]
        [Description("The call detail record reference")]
        string reference
    )
    {
        CdrItem? item;

        try
        {
            item = await service.FetchRecordAsync(new CdrReference(reference));
            if (item != null)
            {
                return Ok(CdrItemDto.FromOrNull(item));
            }
        }
        catch (Exception) // So that we won't expose any sensitive data
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        return NotFound($"The item with reference [{reference}] was not found.");
    }

    [HttpGet("[action]")]
    [ProducesResponseType(typeof(IEnumerable<CdrItemDto>), StatusCodes.Status200OK)]
    [ForceResponseContentType(StatusCodes.Status200OK, MediaTypeNames.Application.Json, 
        "Returns an array of call detail record items or empty when not found")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ForceResponseContentType(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [OpenApiOperation("Fetch call detail records", "Fetch call detail records of a caller given the caller id, a time frame that is 1 month at max and optionally the call type")]
    public async Task<IActionResult> Records(
        [FromQuery] [Required] [RegularExpression(CdrItem.PhoneNumberPattern)]
        [Description("The caller id (phone number)")]
        string callerId,
        [FromQuery] [Required] [RegularExpression(CdrItem.DatePattern)]
        [Description("A valid date with format " + CdrItem.CallDateFormat)]
        string from,
        [FromQuery] [Required] [RegularExpression(CdrItem.DatePattern)]
        [Description("A valid date with format " + CdrItem.CallDateFormat)]
        string to,
        [FromQuery] 
        [Description("(Optional) Call type (1 = Domestic, 2 = International)")]
        CdrCallTypeEnum? type = null
    )
    {
        return await FetchRecordsAsync(callerId, from, to, type);
    }
    
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(IEnumerable<CdrItemDto>), StatusCodes.Status200OK)]
    [ForceResponseContentType(StatusCodes.Status200OK, MediaTypeNames.Application.Json, 
        "Returns an array of call detail record items or empty when not found")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ForceResponseContentType(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [OpenApiOperation("Fetch call detail records of N most expensive calls", "Fetch call detail records of N most expensive calls of a caller given the caller id, a time frame that is 1 month at max and optionally the call type")]
    public async Task<IActionResult> ExpensiveCallsRecords(
        [FromQuery] [Required] [Range(1, 1000)]
        [Description("The (N)umber of most expensive calls")]
        long n,
        [FromQuery] [Required] [RegularExpression(CdrItem.PhoneNumberPattern)]
        [Description("The caller id (phone number)")]
        string callerId,
        [FromQuery] [Required] [RegularExpression(CdrItem.DatePattern)]
        [Description("A valid date with format " + CdrItem.CallDateFormat)]
        string from,
        [FromQuery] [Required] [RegularExpression(CdrItem.DatePattern)]
        [Description("A valid date with format " + CdrItem.CallDateFormat)]
        string to,
        [FromQuery] 
        [Description("(Optional) Call type (1 = Domestic, 2 = International)")]
        CdrCallTypeEnum? type = null
    )
    {
        return await FetchRecordsAsync(callerId, from, to, type, n);
    }
    
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(CountTotalDurationDto), StatusCodes.Status200OK)]
    [ForceResponseContentType(StatusCodes.Status200OK, MediaTypeNames.Application.Json, 
        "Returns a count and total duration of all calls")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ForceResponseContentType(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [OpenApiOperation("Fetch a count and total duration of all calls", "Fetch a count and total duration of all calls given a time frame that is 1 month at max and optionally the call type")]
    public async Task<IActionResult> CountTotalDurationCalls(
        [FromQuery] [Required] [RegularExpression(CdrItem.DatePattern)]
        [Description("A valid date with format " + CdrItem.CallDateFormat)]
        string from,
        [FromQuery] [Required] [RegularExpression(CdrItem.DatePattern)]
        [Description("A valid date with format " + CdrItem.CallDateFormat)]
        string to,
        [FromQuery] 
        [Description("(Optional) Call type (1 = Domestic, 2 = International)")]
        CdrCallTypeEnum? type = null
    )
    {
        // let us do the final validations
        ValidateFromToParsing(from, to,  out var fromDate, out var toDate);

        if (ModelState.ErrorCount > 0)
            return BadRequest(ModelState);

        ValidateTimeInterval(fromDate, toDate);
        
        if (ModelState.ErrorCount > 0)
            return BadRequest(ModelState);
        
        try
        { 
            var (count, duration) = await service.FetchCountTotalDurationCalls(
                new Date(fromDate),
                new Date(toDate),
                type);

            return Ok(new CountTotalDurationDto() { Count = count, TotalDuration = duration });
        }
        catch (Exception) // So that we won't expose any sensitive data
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    private void ValidateFromToParsing(string from, string to, out DateTime fromDate, out DateTime toDate)
    {
        if (!DateTimeHelper.TryParseDate(from, out var fromDateTemp))
            ModelState.AddModelError("from", "Invalid or wrong date format. Must be dd/MM/yyyy.");
        
        if (!DateTimeHelper.TryParseDate(to, out var toDateTemp))
            ModelState.AddModelError("to", "Invalid or wrong date format. Must be dd/MM/yyyy.");

        fromDate = fromDateTemp;
        toDate = toDateTemp;
    }

    private void ValidateTimeInterval(DateTime from, DateTime to)
    {
        if (from >= to || (to - from).TotalDays > 31) // Let's consider that a month it's 31 days. TODO: should be a config
            ModelState.AddModelError("from/to", $"Invalid time period. Must be positive (>0) and cannot exceed 1 month (<=31 days).");
    }

    private async Task<ObjectResult> FetchRecordsAsync(string callerId, string from, string to, CdrCallTypeEnum? type, long? nExpensiveCall = null)
    {
        // let us do the final validations
        ValidateFromToParsing(from, to,  out var fromDate, out var toDate);

        if (ModelState.ErrorCount > 0)
            return BadRequest(ModelState);

        ValidateTimeInterval(fromDate, toDate);
        
        if (ModelState.ErrorCount > 0)
            return BadRequest(ModelState);
        
        try
        {
            var items = await service.FetchRecordsAsync(
                new Phone(callerId),
                new Date(fromDate),
                new Date(toDate),
                type,
                nExpensiveCall);
            
            return Ok(items.Select(CdrItemDto.From));
        }
        catch (Exception) // So that we won't expose any sensitive data
        {
            return new ObjectResult(null) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}