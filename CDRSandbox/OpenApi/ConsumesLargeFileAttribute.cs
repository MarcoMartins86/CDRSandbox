using NJsonSchema;
using NSwag;
using NSwag.Annotations;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace CDRSandbox.OpenApi;

public class ConsumesLargeFileAttribute(string description = "")
    : OpenApiOperationProcessorAttribute(typeof(ConsumesLargeFileOperationProcessor), description)
{
    private class ConsumesLargeFileOperationProcessor(string description) : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            // This will basically write the correct specification manually for a large file as a parameter
            // https://swagger.io/docs/specification/describing-request-body/file-upload/

            context.OperationDescription.Operation.RequestBody ??= new OpenApiRequestBody() { IsRequired = true };

            context.OperationDescription.Operation.RequestBody.Content.Add(
                new KeyValuePair<string, OpenApiMediaType>("multipart/form-data", new OpenApiMediaType
                {
                    Schema = new JsonSchema
                    {
                        Type = JsonObjectType.Object,
                        Properties =
                        {
                            ["file"] = new JsonSchemaProperty
                            {
                                Type = JsonObjectType.String,
                                Format = JsonFormatStrings.Binary,
                                IsNullableRaw = false,
                                Description = description,
                            }
                        }
                    }
                })
            );

            return true;
        }
    }
}