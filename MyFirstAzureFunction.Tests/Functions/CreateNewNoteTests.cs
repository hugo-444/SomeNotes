using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
    [TestFixture]
    public class CreateNewNoteTests
    {
        private Mock<ILogger<CreateNewNote>> _logger; // Mock logger to simulate logging
        private Mock<INoteService> _noteService; // Mock note service
        private CreateNewNote _function; // System Under Test (SUT)
        private FunctionContext _context; // Mocked function context

        // Set up the testing environment
        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<CreateNewNote>>(); // Initialize the mock logger
            _noteService = new Mock<INoteService>(); // Initialize the mock note service
            _function = new CreateNewNote(_logger.Object, _noteService.Object); // Initialize the function
            _context = new Mock<FunctionContext>().Object; // Mock function context
        }

        // Create a mock HttpRequestData object for testing purposes
        private HttpRequestData CreateHttpRequestData(FunctionContext context, object body = null)
        {
            // Mock the HttpRequestData
            var requestMock = new Mock<HttpRequestData>(context);

            // Mock the HttpResponseData and set its properties
            var responseMock = new Mock<HttpResponseData>(context);
            responseMock.SetupProperty(r => r.StatusCode);
            responseMock.SetupProperty(r => r.Body, new MemoryStream());
            responseMock.Setup(r => r.Headers).Returns(new HttpHeadersCollection()); // Set up headers collection

            // Simulate the creation of a response
            requestMock.Setup(req => req.CreateResponse()).Returns(responseMock.Object);

            // If a body is provided, serialize it to a JSON stream
            if (body != null)
            {
                var jsonString = JsonConvert.SerializeObject(body);
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                requestMock.Setup(req => req.Body).Returns(memoryStream);
            }
            else
            {
                requestMock.Setup(req => req.Body).Returns(new MemoryStream());
            }

            return requestMock.Object;
        }

        private async Task<string> ReadResponseBody(HttpResponseData response)
        {
            // Set the stream position to the beginning to read the content
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            return await reader.ReadToEndAsync();
        }

        // Unit test to check if a valid note creation works
        [Test]
        public async Task CreateNewNote_ReturnsOk_WhenValidRequest()
        {
            // Arrange
            var note = new NoteModel
            {
                Title = "Test Note",
                Content = "This is a test note.",
                AuthorID = 1,
                Tags = [ "TestTag" ]
            };

            // Mock the service behavior to return success (1)
            _noteService.Setup(service => service.CreateNewNote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<List<string>>())).Returns(1);

            // Create a mock HttpRequestData
            var request = CreateHttpRequestData(_context, note);

            // Act
            var response = await _function.RunAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo("Result: 1")); // Ensure correct result
        }

        // Unit test to check if an exception is properly handled
        [Test]
        public async Task CreateNewNote_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Step 1: Prepare invalid input data (e.g., null title)
            var note = new NoteModel
            {
                Title = null, // Invalid title
                Content = "This note has invalid data.",
                AuthorID = 1,
                NoteId = Guid.NewGuid(),
                Date = DateTime.Now
            };

            // Step 2: Mock the note service to throw an exception
            _noteService.Setup(service => service.CreateNewNote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Throws(new Exception("Invalid note data")); // Simulate an exception

            // Step 3: Create a mock HTTP request with the invalid note data
            var request = CreateHttpRequestData(_context, note);

            // Step 4: Call the function, expecting it to handle the exception
            var response = await _function.RunAsync(request);

            // Step 5: Assert the response is 400 Bad Request
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseContent = await ReadResponseBody(response);
            Assert.That(responseContent, Is.EqualTo("Error: Invalid note data")); // Verify the error message
        }
        
        [Test]
        public async Task CreateNewNote_ReturnsBadRequest_WhenContentIsMissing()
        {
            // Arrange: Prepare a note with missing content
            var note = new NoteModel
            {
                Title = "Test Note",
                Content = null, // Missing content
                AuthorID = 1,
                NoteId = Guid.NewGuid(),
                Date = DateTime.Now
            };

            // Mock the service to throw an exception when content is missing
            _noteService.Setup(service => service.CreateNewNote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Throws(new Exception("Note content is required"));

            // Create a mock HttpRequestData
            var request = CreateHttpRequestData(_context, note);

            // Act
            var response = await _function.RunAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo("Error: Note content is required")); // Verify the error message
        }
        
        [Test]
        public async Task CreateNewNote_ReturnsBadRequest_WhenAuthorIDIsInvalid()
        {
            // Arrange: Prepare a note with an invalid AuthorID
            var note = new NoteModel
            {
                Title = "Test Note",
                Content = "This is a test note.",
                AuthorID = -1, // Invalid AuthorID
                NoteId = Guid.NewGuid(),
                Date = DateTime.Now
            };

            // Mock the service to throw an exception when AuthorID is invalid
            _noteService.Setup(service => service.CreateNewNote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<List<string>>()))
                .Throws(new Exception("Invalid AuthorID"));

            // Create a mock HttpRequestData
            var request = CreateHttpRequestData(_context, note);

            // Act
            var response = await _function.RunAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo("Error: Invalid AuthorID")); // Verify the error message
        }
        
        [Test]
        public async Task CreateNewNote_ReturnsOk_WhenDuplicateTagsAreProvided()
        {
            // Arrange: Prepare a note with duplicate tags
            var note = new NoteModel
            {
                Title = "Test Note",
                Content = "This is a test note.",
                AuthorID = 1,
                NoteId = Guid.NewGuid(),
                Date = DateTime.Now,
                Tags = new List<string> { "TestTag", "TestTag" } // Duplicate tags
            };

            // Mock the service behavior to return success (1)
            _noteService.Setup(service => service.CreateNewNote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<List<string>>())).Returns(1);

            // Create a mock HttpRequestData
            var request = CreateHttpRequestData(_context, note);

            // Act
            var response = await _function.RunAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await ReadResponseBody(response);
            Assert.That(responseBody, Is.EqualTo("Result: 1")); // Ensure the note was created successfully with duplicate tags
        }
    }
}