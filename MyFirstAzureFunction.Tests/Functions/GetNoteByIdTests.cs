using System.Net;

using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Newtonsoft.Json;

using MyFirstAzureFunction.Functions;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MyFirstAzureFunction.Tests.Functions
{
    [TestFixture]
    public class GetNoteByIdTests
    {
        private Mock<ILogger<GetNoteById>> _logger;
        private Mock<INoteService> _noteService;
        private GetNoteById _function;
        private FunctionContext _context;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<GetNoteById>>();
            _noteService = new Mock<INoteService>();
            _function = new GetNoteById(_logger.Object, _noteService.Object);
            _context = new Mock<FunctionContext>().Object;
        }

        // Helper method to create a mock HttpRequestData
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

        [Test]
        public async Task GetNoteById_ReturnsOk_WhenNoteExists()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var note = new NoteModel
            {
                NoteId = noteId,
                Title = "Test Note",
                Content = "Test Content",
                AuthorID = 1
            };

            // Mock the GetNoteById method to return a valid note
            _noteService.Setup(s => s.GetNoteById(noteId)).Returns(note);

            var request = CreateHttpRequestData(_context);

            // Act
            var response = await _function.RunAsync(request, noteId.ToString());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Read response body
            var responseBody = await ReadResponseBody(response);
            var expectedJson = JsonConvert.SerializeObject(note);

            // Compare the serialized note with the response body
            Assert.That(responseBody, Is.EqualTo(expectedJson));
        }
        

        [Test]
        public async Task GetNoteById_ReturnsBadRequest_WhenInvalidGuid()
        {
            // Arrange
            var invalidNoteId = "InvalidGUID";
            var request = CreateHttpRequestData(_context);

            // Act
            var response = await _function.RunAsync(request, invalidNoteId);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo($"Invalid Note ID: {invalidNoteId}. Must be a valid GUID."));
        }

        [Test]
        public async Task GetNoteById_ReturnsNotFound_WhenNoteDoesNotExist()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            // Mock the GetNoteById method to return null (note not found)
            _noteService.Setup(s => s.GetNoteById(noteId)).Returns((NoteModel)null);

            var request = CreateHttpRequestData(_context);

            // Act
            var response = await _function.RunAsync(request, noteId.ToString());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo($"Note with ID {noteId} not found."));
        }

        [Test]
        public async Task GetNoteById_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            // Mock the GetNoteById method to throw an exception
            _noteService.Setup(s => s.GetNoteById(noteId)).Throws(new Exception("Database connection failed"));

            var request = CreateHttpRequestData(_context);

            // Act
            var response = await _function.RunAsync(request, noteId.ToString());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo("An error occurred while fetching the note: Database connection failed"));
        }

        // Helper method to read the response body
        private async Task<string> ReadResponseBody(HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            return await reader.ReadToEndAsync();
        }
    }
}