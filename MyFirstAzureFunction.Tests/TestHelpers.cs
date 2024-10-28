using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace MyFirstAzureFunction.Tests
{
    public static class TestHelpers
    {
        // Create a mock HttpRequestData object for testing purposes
        public static HttpRequestData CreateHttpRequestData(FunctionContext context, object body = null)
        {
            // Create a mock HttpRequestData object
            var request = new Mock<HttpRequestData>(context);

            // Mock the necessary properties of the HttpRequestData object
            var responseMock = new Mock<HttpResponseData>(context);
            responseMock.SetupProperty(r => r.StatusCode);
            responseMock.SetupProperty(r => r.Body, new MemoryStream());

            request.Setup(req => req.CreateResponse()).Returns(responseMock.Object);

            // If a request body is provided, serialize it and set up the request's body stream
            if (body != null)
            {
                var jsonString = JsonConvert.SerializeObject(body);
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                request.Setup(req => req.Body).Returns(memoryStream);
            }
            else
            {
                request.Setup(req => req.Body).Returns(new MemoryStream()); // Empty stream for no body
            }

            return request.Object;
        }

        // Read the response body content from an HttpResponseData object
        public static async Task<string> ReadResponseBody(HttpResponseData response)
        {
            // Set the stream position to the beginning so we can read it
            response.Body.Position = 0;

            using var reader = new StreamReader(response.Body, Encoding.UTF8);
            return await reader.ReadToEndAsync(); // Read the content and return it
        }
    }
}