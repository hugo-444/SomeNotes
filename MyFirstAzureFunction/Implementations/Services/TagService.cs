using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;

namespace MyFirstAzureFunction.Implementations.Services
{
    public class TagService : ITagService
    {
        private readonly ILogger<TagService> _logger;
        private readonly INoteService _noteService;

        public TagService(ILogger<TagService> logger, INoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        }

        public List<string> GetTagsForNoteId(Guid noteId)
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var tagsDirectoryPath = Path.Combine(projectDirectory, "Tags");

            var tags = new List<string>();

            if (Directory.Exists(tagsDirectoryPath))
            {
                var tagFiles = Directory.GetFiles(tagsDirectoryPath, "*.txt");

                foreach (var tagFile in tagFiles)
                {
                    var tagContent = File.ReadAllText(tagFile);
                    if (tagContent.Contains(noteId.ToString()))
                    {
                        var tagName = Path.GetFileNameWithoutExtension(tagFile);
                        tags.Add(tagName);
                    }
                }
            }

            _logger.LogInformation($"Fetched {tags.Count} tags for note ID: {noteId}");
            return tags;
        }

        public List<NoteModel> GetNotesByTag(string tagName)
        {
            _logger.LogInformation($"Fetching notes associated with tag: {tagName}");

            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var tagsDirectoryPath = Path.Combine(projectDirectory, "Tags");
            
            var associatedNotes = new List<NoteModel>();

            // Make the tagName case-insensitive
            var tagFileName = $"{tagName.ToLower()}.txt";
            var tagFilePath = Path.Combine(tagsDirectoryPath, tagFileName);

            _logger.LogInformation($"Reading notes associated with tag: {tagFilePath}");

            if (File.Exists(tagFilePath))
            {
                var noteIds = File.ReadAllLines(tagFilePath).Select(Guid.Parse).ToList();

                _logger.LogInformation($"Note IDs found: {string.Join(", ", noteIds)}");

                // Now fetch each note by its GUID using the NoteService
                foreach (var noteId in noteIds)
                {
                    var note = _noteService.GetNoteById(noteId);
                    if (note != null)
                    {
                        associatedNotes.Add(note);
                    }
                }
            }
            else
            {
                _logger.LogInformation($"No tag file found for tag: {tagName}");
            }

            _logger.LogInformation($"Found {associatedNotes.Count} notes for tag: {tagName}");
            return associatedNotes;
        }
    }
}