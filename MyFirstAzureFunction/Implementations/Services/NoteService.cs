using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;

namespace MyFirstAzureFunction.Implementations.Services
{
    public class NoteService : INoteService
    {
        private readonly ILogger<NoteService> _logger;

        public NoteService(ILogger<NoteService> logger)
        {
            _logger = logger;
        }

        public int CreateNewNote(string title, string content, int authorId, Guid noteId, DateTime date, List<string> tags = null)
        {
            try
            {
                _logger.LogInformation($"Creating new note: {title}");

                var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                var notesDirectoryPath = Path.Combine(projectDirectory, "MyNotes");
                Directory.CreateDirectory(notesDirectoryPath); // Ensure the MyNotes directory exists

                var sanitizedTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"Note_{noteId}.txt";
                var filePath = Path.Combine(notesDirectoryPath, fileName);

                // Create note content
                var noteContent = $"NoteID: {noteId}\nTitle: {title}\nContent: {content}\nAuthor ID: {authorId}\nDate: {date}";

                // Add tags if any
                if (tags != null && tags.Any())
                {
                    noteContent += $"\nTags: {string.Join(", ", tags)}";
                }
                else
                {
                    noteContent += "\nTags: None";
                }

                File.WriteAllText(filePath, noteContent);
                _logger.LogInformation($"Note saved to {filePath}");

                // Handle tags if provided
                if (tags != null && tags.Any())
                {
                    AssociateTagsWithNote(tags, noteId);
                }

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating note: {ex.Message}");
                throw;
            }
        }

        public List<NoteModel> GetAllNotes()
        {
            _logger.LogInformation("Fetching all notes.");

            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var notesDirectoryPath = Path.Combine(projectDirectory, "MyNotes");
            var files = Directory.GetFiles(notesDirectoryPath);

            var notes = new List<NoteModel>();

            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var note = ParseNoteContent(content);

                    // Fetch associated tags
                    note.Tags = GetTagsForNoteId(note.NoteId);
                    notes.Add(note);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing file {file}: {ex.Message}");
                }
            }

            return notes;
        }

        public NoteModel GetNoteById(Guid noteId)
        {
            _logger.LogInformation($"Fetching note with ID: {noteId}");

            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var notesDirectoryPath = Path.Combine(projectDirectory, "MyNotes");
            var files = Directory.GetFiles(notesDirectoryPath, $"Note_{noteId}*.txt");

            if (!files.Any())
            {
                _logger.LogError($"Note with ID {noteId} not found.");
                return null;
            }

            var content = File.ReadAllText(files.First());
            var note = ParseNoteContent(content);

            // Fetch associated tags
            note.Tags = GetTagsForNoteId(note.NoteId);

            return note;
        }

        private NoteModel ParseNoteContent(string content)
        {
            _logger.LogInformation($"Parsing note content: {content}");

            var parts = content.Split('\n');

            if (parts.Length < 5)
            {
                _logger.LogError("The note content is incomplete or malformed.");
                throw new InvalidOperationException("The note content is incomplete or malformed.");
            }

            try
            {
                return new NoteModel
                {
                    NoteId = Guid.Parse(parts[0].Replace("NoteID:", "").Trim()),
                    Title = parts[1].Replace("Title:", "").Trim(),
                    Content = parts[2].Replace("Content:", "").Trim(),
                    AuthorID = int.Parse(parts[3].Replace("Author ID:", "").Trim()),
                    Date = DateTime.Parse(parts[4].Replace("Date:", "").Trim())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing note content: {ex.Message}");
                throw new InvalidOperationException("The note content is invalid or malformed.");
            }
        }

        private void AssociateTagsWithNote(List<string> tags, Guid noteId)
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var tagsDirectoryPath = Path.Combine(projectDirectory, "Tags");
            Directory.CreateDirectory(tagsDirectoryPath);

            foreach (var tag in tags)
            {
                var sanitizedTag = string.Join("_", tag.Split(Path.GetInvalidFileNameChars()));
                var tagFileName = $"{sanitizedTag.ToLower()}.txt";
                var tagFilePath = Path.Combine(tagsDirectoryPath, tagFileName);

                if (File.Exists(tagFilePath))
                {
                    File.AppendAllText(tagFilePath, $"{noteId}\n");
                }
                else
                {
                    File.WriteAllText(tagFilePath, $"{noteId}\n");
                }

                _logger.LogInformation($"Associated note {noteId} with tag {tag}.");
            }
        }

        private List<string> GetTagsForNoteId(Guid noteId)
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

            return tags;
        }
    }
}