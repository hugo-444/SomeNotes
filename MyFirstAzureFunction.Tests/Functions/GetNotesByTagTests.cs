using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MyFirstAzureFunction.Functions;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;

namespace MyFirstAzureFunction.Tests.Functions
{
    [TestFixture]
    public class GetNotesByTagTests
    {
        private Mock<ILogger<GetNotesByTag>> _logger; // Mock logger for simulating logging
        private Mock<ITagService> _tagService; // Mock tag service for fetching notes
        private GetNotesByTag _function; // System Under Test (SUT)
        private FunctionContext _context; // Mock function context for HttpRequestData

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<GetNotesByTag>>(); // Initialize mock logger
            _tagService = new Mock<ITagService>(); // Initialize mock tag service
            _function = new GetNotesByTag(_logger.Object, _tagService.Object); // Initialize the function
            _context = new Mock<FunctionContext>().Object; // Mock function context for HttpRequestData
        }

        /// <summary>
        /// Test when a valid tag is provided and notes are found.
        /// </summary>
        [Test]
        public async Task GetNotesByTag_ReturnsOk_WhenNotesFound()
        {
            // Step 1: Prepare mock data (a list of notes associated with the tag)
            var notes = new List<NoteModel>
            {
                new NoteModel { NoteId = Guid.NewGuid(), Title = "Note 1", Content = "Content for note 1", Tags = new List<string> { "testTag" } },
                new NoteModel { NoteId = Guid.NewGuid(), Title = "Note 2", Content = "Content for note 2", Tags = new List<string> { "testTag" } }
            };

            // Step 2: Mock the GetNotesByTag service method to return the notes
            _tagService.Setup(service => service.GetNotesByTag("testTag")).Returns(notes);

            // Step 3: Create a mock HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 4: Execute the function
            var response = await _function.GetNotesByTagAsync(request, "testTag", _context);

            // Step 5: Assert that the response is OK and contains the correct notes
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseBody = await ReadResponseBody(response);
            var expectedResponse = JsonConvert.SerializeObject(new
            {
                Count = notes.Count,
                NotePreviews = notes.Select(note => new { note.NoteId, note.Title, note.Content }).ToList()
            });
            Assert.AreEqual(expectedResponse, responseBody);
        }
        
        

        /// <summary>
        /// Test when an empty or invalid tag is provided.
        /// </summary>
        [Test]
        public async Task GetNotesByTag_ReturnsBadRequest_WhenTagIsInvalid()
        {
            // Step 1: Create a mock HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 2: Execute the function with an empty tag
            var response = await _function.GetNotesByTagAsync(request, "", _context);

            // Step 3: Assert that the response is BadRequest
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await ReadResponseBody(response);
            Assert.AreEqual("Tag name cannot be empty or null. Please provide a valid tag.", responseBody);
        }

        /// <summary>
        /// Test when no notes are found for the provided tag.
        /// </summary>
        [Test]
        public async Task GetNotesByTag_ReturnsNotFound_WhenNoNotesFound()
        {
            // Step 1: Mock the GetNotesByTag service method to return an empty list
            _tagService.Setup(service => service.GetNotesByTag("testTag")).Returns(new List<NoteModel>());

            // Step 2: Create a mock HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 3: Execute the function
            var response = await _function.GetNotesByTagAsync(request, "testTag", _context);

            // Step 4: Assert that the response is NotFound
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            var responseBody = await ReadResponseBody(response);
            Assert.AreEqual("No notes found for tag: testTag", responseBody);
        }

        /// <summary>
        /// Test when an internal server error occurs during the request.
        /// </summary>
        [Test]
        public async Task GetNotesByTag_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Step 1: Mock the GetNotesByTag service method to throw an exception
            _tagService.Setup(service => service.GetNotesByTag("testTag"))
                .Throws(new Exception("Database connection failed"));

            // Step 2: Create a mock HTTP request
            var request = CreateHttpRequestData(_context);

            // Step 3: Execute the function
            var response = await _function.GetNotesByTagAsync(request, "testTag", _context);

            // Step 4: Assert that the response is InternalServerError
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            var responseBody = await ReadResponseBody(response);
            Assert.AreEqual("An error occurred: Database connection failed", responseBody);
        }

        // Helper method to create a mock HttpRequestData
        private HttpRequestData CreateHttpRequestData(FunctionContext context, object body = null)
        {
            var requestMock = new Mock<HttpRequestData>(context);
            var responseMock = new Mock<HttpResponseData>(context);

            // Ensure headers are initialized
            var headers = new HttpHeadersCollection();
            responseMock.Setup(r => r.Headers).Returns(headers);

            responseMock.SetupProperty(r => r.StatusCode);
            responseMock.SetupProperty(r => r.Body, new MemoryStream());

            requestMock.Setup(req => req.CreateResponse()).Returns(responseMock.Object);

            // Serialize body to JSON if provided
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

        // Helper method to read the response body content
        private async Task<string> ReadResponseBody(HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            return await reader.ReadToEndAsync();
        }
    }
}