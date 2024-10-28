using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;
using Newtonsoft.Json;

namespace MyFirstAzureFunction.Functions
{
    public class GetNoteById
    {
        private readonly ILogger<GetNoteById> _logger;
        private readonly INoteService _noteService;

        public GetNoteById(ILogger<GetNoteById> logger, INoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        }

        /// <summary>
        /// Function to retrieve a note by its ID. 
        /// Handles invalid ID input and non-existent notes.
        /// </summary>
        [Function("GetNoteById")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "notes/{note_id}")] HttpRequestData req, string note_id)
        {
            _logger.LogInformation($"Attempting to retrieve note with ID: {note_id}");

            // Step 1: Validate the note ID format (GUID)
            if (!Guid.TryParse(note_id, out var noteId))
            {
                _logger.LogWarning($"Invalid Note ID format: {note_id}");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync($"Invalid Note ID: {note_id}. Must be a valid GUID.");
                return errorResponse;
            }

            // Step 2: Fetch the note using the service
            NoteModel note;
            try
            {
                note = _noteService.GetNoteById(noteId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching note with ID {note_id}: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"An error occurred while fetching the note: {ex.Message}");
                return errorResponse;
            }

            // Step 3: Handle non-existent note
            if (note == null)
            {
                _logger.LogInformation($"Note with ID {note_id} not found.");
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Note with ID {note_id} not found.");
                return notFoundResponse;
            }

            // Step 4: Return the note if found
            var noteJson = JsonConvert.SerializeObject(note);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            // Serialize the note manually to a JSON string
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(noteJson); // Use WriteStringAsync instead of WriteAsJsonAsync
            _logger.LogInformation($"Successfully retrieved note with ID: {note_id}");
            return response;
        }
    }
}