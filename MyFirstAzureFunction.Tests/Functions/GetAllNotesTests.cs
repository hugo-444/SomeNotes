using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MyFirstAzureFunction.Functions;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MyFirstAzureFunction.Tests.Functions
{
    [TestFixture] // Indicates that this class contains unit tests
    public class GetAllNotesTests
    {
        private Mock<ILogger<GetAllNotes>> _loggerMock; // Mock logger to simulate logging
        private Mock<INoteService> _noteServiceMock; // Mock note service
        private GetAllNotes _function; // System Under Test (SUT)
        private FunctionContext _context; // Function context for creating HttpRequestData

        // Set up the testing environment
        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<GetAllNotes>>(); // Initialize the mock logger
            _noteServiceMock = new Mock<INoteService>(); // Initialize the mock note service
            _function = new GetAllNotes(_loggerMock.Object, _noteServiceMock.Object); // Initialize the function
            _context = new Mock<FunctionContext>().Object; // Mock function context for HttpRequestData creation
        }

        // Unit test to check if notes are successfully retrieved
        [Test]
        public async Task GetAllNotes_ReturnsOk_WhenNotesExist()
        {
            // Step 1: Prepare the mock data
            var notes = new List<NoteModel>
            {
                new NoteModel { NoteId = Guid.NewGuid(), Title = "Note 1", Content = "Content 1", AuthorID = 1 },
                new NoteModel { NoteId = Guid.NewGuid(), Title = "Note 2", Content = "Content 2", AuthorID = 2 }
            };

            // Step 2: Set up the mock service to return the mock notes
            _noteServiceMock.Setup(service => service.GetAllNotes()).Returns(notes);

            // Step 3: Create a fake HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 4: Call the function with the fake request
            var response = await _function.RunAsync(request);

            // Step 5: Assert the response is 200 OK
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Step 6: Read the response content and check if it matches the mock data
            var responseBody = await ReadResponseBody(response);
            var returnedNotes = JsonConvert.DeserializeObject<List<NoteModel>>(responseBody);
            Assert.That(returnedNotes.Count, Is.EqualTo(2)); // Verify the number of notes returned
        }

        // Unit test to check if the function handles the case where no notes are found
        [Test]
        public async Task GetAllNotes_ReturnsNotFound_WhenNoNotesExist()
        {
            // Step 1: Set up the mock service to return an empty list
            _noteServiceMock.Setup(service => service.GetAllNotes()).Returns(new List<NoteModel>());

            // Step 2: Create a fake HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 3: Call the function
            var response = await _function.RunAsync(request);

            // Step 4: Assert the response is 404 Not Found
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            // Step 5: Read the response content and check if it contains the correct message
            var responseBody = await ReadResponseBody(response);
            Assert.AreEqual("No notes found.", responseBody);
        }

        // Unit test to check if the function handles an internal server error
        [Test]
        public async Task GetAllNotes_ReturnsServerError_WhenExceptionThrown()
        {
            // Step 1: Set up the mock service to throw an exception
            _noteServiceMock.Setup(service => service.GetAllNotes()).Throws(new Exception("Service error"));

            // Step 2: Create a fake HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 3: Call the function
            var response = await _function.RunAsync(request);

            // Step 4: Assert the response is 500 Internal Server Error
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

            // Step 5: Read the response content and check if it contains the correct error message
            var responseBody = await ReadResponseBody(response);
            Assert.AreEqual("An error occurred while retrieving notes: Service error", responseBody);
        }

        // Helper method to create a mock HttpRequestData object
        private HttpRequestData CreateHttpRequestData(FunctionContext context)
        {
            var requestMock = new Mock<HttpRequestData>(context);
            var responseMock = new Mock<HttpResponseData>(context);

            // Initialize headers for the response
            responseMock.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
            responseMock.SetupProperty(r => r.StatusCode);
            responseMock.SetupProperty(r => r.Body, new MemoryStream());

            // Simulate the creation of a response
            requestMock.Setup(req => req.CreateResponse()).Returns(responseMock.Object);

            return requestMock.Object;
        }
        // Helper method to read the response body content from an HttpResponseData object
        private async Task<string> ReadResponseBody(HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            return await reader.ReadToEndAsync();
        }
    }
}