using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;
using Newtonsoft.Json;

namespace MyFirstAzureFunction.Functions
{
    public class CreateNewNote
    {
        private readonly ILogger<CreateNewNote> _logger;
        private readonly INoteService _noteService;

        // Constructor for dependency injection
        public CreateNewNote(ILogger<CreateNewNote> logger, INoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        }

        // The main function that gets triggered when an HTTP POST request is made to create a new note
        [Function("CreateNewNote")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Step 1: Read and parse the incoming request body to create a NoteModel object
            var requestBody = await req.ReadAsStringAsync(); // Async to avoid blocking
            var requestObject = JsonConvert.DeserializeObject<NoteModel>(requestBody); // Deserializes the request to NoteModel

            try
            {
                // Step 2: Log the function's start for debugging and tracking purposes
                _logger.LogInformation($"Function Triggered: CreateNewNote at {DateTime.Now}");

                // Step 3: Create the new note using the note service
                var result = _noteService.CreateNewNote(requestObject.Title, requestObject.Content, requestObject.AuthorID,
                                                        requestObject.NoteId, requestObject.Date, requestObject.Tags);

                // Step 4: Log the successful creation of the note and result
                _logger.LogInformation($"Function completed: CreateNewNote at {DateTime.Now} \n Result: {result}");

                // Step 5: Handle author-note association (associates the note with the author in a separate file)
                AssociateNoteWithAuthor(requestObject.AuthorID, requestObject.NoteId);

                // Step 6: Create an HTTP response for the client with the result of the operation
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync($"Result: {result}");

                return response; // Return the success response to the client
            }
            catch (Exception ex) // Catch any exception that occurs during execution
            {
                // Log the error for debugging and return a 400 Bad Request response
                _logger.LogError($"Error: {ex.Message}");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }

        // Helper function to associate a note with an author by writing to a file
        private void AssociateNoteWithAuthor(int authorId, Guid noteId)
        {
            _logger.LogInformation($"Associating note {noteId} with author {authorId}");

            // Step 1: Set up the directory for author files
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var authorsDirectoryPath = Path.Combine(projectDirectory, "Authors");
            Directory.CreateDirectory(authorsDirectoryPath); // Ensure the Author directory exists

            // Step 2: Define the author file path, which will store the note IDs for this author
            var authorFilePath = Path.Combine(authorsDirectoryPath, $"Author_{authorId}.txt");

            // Step 3: Append the noteId to the author's file if it exists, or create a new file otherwise
            if (File.Exists(authorFilePath))
            {
                File.AppendAllText(authorFilePath, $"{noteId}\n");
            }
            else
            {
                File.WriteAllText(authorFilePath, $"{noteId}\n");
            }

            _logger.LogInformation($"Associated note {noteId} with author {authorId} successfully.");
        }
    }
}