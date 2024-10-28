using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using MyFirstAzureFunction.Interfaces;
using Newtonsoft.Json;
using System;

namespace MyFirstAzureFunction.Functions
{
    public class GetNotesByTag
    {
        private readonly ILogger<GetNotesByTag> _logger;
        private readonly ITagService _tagService;

        public GetNotesByTag(ILogger<GetNotesByTag> logger, ITagService tagService)
        {
            _logger = logger;
            _tagService = tagService;
        }

       
        [Function("GetNotesByTag")]
        public async Task<HttpResponseData> GetNotesByTagAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tag/{tagName}")] HttpRequestData req,
            string tagName,
            FunctionContext executionContext)
        {
            
            _logger.LogInformation($"Fetching notes with tag: {tagName}");

            try
            {
                // Step 1: Check if the tag is valid (e.g., not null or empty)
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    _logger.LogWarning("Tag name is missing from the request.");
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Tag name cannot be empty or null. Please provide a valid tag.");
                    return badRequestResponse;
                }

                // Step 2: Fetch notes based on the provided tag
                var notes = _tagService.GetNotesByTag(tagName);

                // Step 3: Handle case when no notes are found for the tag
                //needs to check for the actual tag
                if (notes == null || !notes.Any())
                {
                    _logger.LogInformation($"No notes found for tag: {tagName}");
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"No notes found for tag: {tagName}");
                    return notFoundResponse;
                }

                // Step 4: Prepare the response object with the notes found
                var result = new
                {
                    Count = notes.Count,
                    NotePreviews = notes.Select(note => new
                    {
                        NoteId = note.NoteId,
                        Title = note.Title,
                        Content = note.Content
                    }).ToList()
                };

                // Step 5: Return the result as a JSON response
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonConvert.SerializeObject(result));
                _logger.LogInformation($"Successfully returned {notes.Count} notes for tag: {tagName}");
                return response;
            }
            catch (Exception ex)
            {
                // Step 6: Catch any unexpected errors and return a 500 error response
                _logger.LogError($"An error occurred while fetching notes by tag: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"An error occurred: {ex.Message}");
                return errorResponse;
            }
        }
    }
}