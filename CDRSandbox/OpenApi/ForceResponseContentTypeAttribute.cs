﻿using NSwag.Annotations;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace CDRSandbox.OpenApi;

public class ForceResponseContentTypeAttribute(int statusCode, string contentType)
    : OpenApiOperationProcessorAttribute(typeof(ForceResponseContentTypeOperationProcessor), statusCode, contentType)
{
    private class ForceResponseContentTypeOperationProcessor(int statusCode, string contentType) : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var statusCodeString = statusCode.ToString();
            foreach (var response in context.OperationDescription.Operation.Responses)
            {
                if (response.Key == statusCodeString)
                { 
                    if (response.Value.Content is { Count: >= 1 })
                    {
                        var existentContent = response.Value.Content.First();
                        response.Value.Content.Remove(existentContent.Key);
                        response.Value.Content.Add(contentType, existentContent.Value);
                    }
                }
            }

            return true;
        }
    }
}