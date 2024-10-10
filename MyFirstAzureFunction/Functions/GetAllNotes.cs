using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;
using Newtonsoft.Json;

namespace MyFirstAzureFunction.Functions
{
    public class GetAllNotes
    {
        private readonly ILogger<GetAllNotes> _logger;
        private readonly INoteService _noteService;

        public GetAllNotes(ILogger<GetAllNotes> logger, INoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        }

        // Main entry point for the Azure Function
        [Function("GetAllNotes")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "allNotes/")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP GET request received for retrieving all notes.");

            try
            {
                // Retrieve all notes using the note service
                var notes = _noteService.GetAllNotes();

                // Handle case where no notes are found
                if (notes == null || notes.Count == 0)
                {
                    _logger.LogWarning("No notes were found.");
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync("No notes found.");
                    return notFoundResponse;
                }

                // Manually serialize the notes list to a JSON string
                var notesJson = JsonConvert.SerializeObject(notes);

                // Write the JSON string to the response
                _logger.LogInformation($"Successfully retrieved {notes.Count} notes.");
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(notesJson); // Use WriteStringAsync for manual serialization
                return response;
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                _logger.LogError($"An error occurred while retrieving notes: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"An error occurred while retrieving notes: {ex.Message}");
                return errorResponse;
            }
        }
    }
}