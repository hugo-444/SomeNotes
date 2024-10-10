using Microsoft.Extensions.Logging;
using MyFirstAzureFunction.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using MyFirstAzureFunction.Models;

namespace MyFirstAzureFunction.Implementations.Services;

public class QuickCalculationsService : IQuickCalculations
{
    private readonly ILogger<QuickCalculationsService> _logger;

    public QuickCalculationsService(ILogger<QuickCalculationsService> logger)
    {
        _logger = logger;
    }

    public int CreateNewNote(string Title, string Content, int AuthorID, Guid NoteID, DateTime Date,
        List<string> Tags = null)
    {
        try
        {
            _logger.LogInformation($"Note Add function accessed at: {DateTime.Now}");

            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var directoryPath = Path.Combine(projectDirectory, "MyNotes");
            Directory.CreateDirectory(directoryPath); // Ensure the MyNotes folder exists

            // Create note file
            var sanitizedTitle = string.Join("_", Title.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"Note_{NoteID}.txt";
            var filePath = Path.Combine(directoryPath, fileName);
            _authorService = new AuthorService(_logger.Object); // Initialize the service_authorService = new AuthorService(_logger.Object); // Initialize the service
            var noteContent =
                $"NoteID: {NoteID}\nTitle: {Title}\nContent: {Content}\nAuthor ID: {AuthorID}\nDate: {Date}";
            

            // Add tags if any
            if (Tags != null && Tags.Any())
            {
                noteContent += $"\nTags: {string.Join(", ", Tags)}"; // Join tags with commas
            }
            else
            {
                noteContent += "\nTags: None"; // If no tags are present, explicitly state that
            }

            // Write the note content to the file

            File.WriteAllText(filePath, noteContent);

            _logger.LogInformation($"Note saved to {filePath}");

            // Handle Tags
            if (Tags != null && Tags.Any())
            {
                _logger.LogInformation("Associating tags with the note.");
                var tagsDirectoryPath = Path.Combine(projectDirectory, "Tags");
                Directory.CreateDirectory(tagsDirectoryPath); // Ensure the Tags folder exists

                foreach (var tag in Tags)
                {
                    var sanitizedTag = string.Join("_", tag.Split(Path.GetInvalidFileNameChars()));
                    var tagFileName = $"{sanitizedTag.ToLower()}.txt";
                    var tagFilePath = Path.Combine(tagsDirectoryPath, tagFileName);
                    // Add NoteID to the tag file (create file if it doesn't exist)
                    if (File.Exists(tagFilePath))
                    {
                        File.AppendAllText(tagFilePath, $"{NoteID}\n");
                    }
                    else
                    {
                        File.WriteAllText(tagFilePath, $"{NoteID}\n");
                    }

                    _logger.LogInformation($"Associated note {NoteID} with tag {tag}.");
                }
            }

            _logger.LogInformation($"Note creation process completed: {DateTime.Now}");
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            throw;
        }
    }
    
    

    public List<NoteModel> GetAllNotes()
    {
        _logger.LogInformation("Getting all notes");

        var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        var directoryPath = Path.Combine(projectDirectory, "MyNotes");
        var files = Directory.GetFiles(directoryPath);

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
                // Continue without crashing for malformed notes
            }
        }

        return notes;
    }

    public NoteModel GetNoteById(Guid noteId)
    {
        _logger.LogInformation($"Fetching note with ID: {noteId}");

        var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        var directoryPath = Path.Combine(projectDirectory, "MyNotes");
        var files = Directory.GetFiles(directoryPath, $"Note_{noteId}*.txt");
    
        if (!files.Any())
        {
            _logger.LogError($"Note with ID {noteId} not found.");
            return null;
        }

        var content = File.ReadAllText(files.First());
        var note = ParseNoteContent(content);

        // Fetch associated tags
        note.Tags = GetTagsForNoteId(noteId);

        return note;
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
    
    public List<NoteModel> GetNotesByTag(string tagName)
    {
        _logger.LogInformation($"Getting notes associated with tag: {tagName}");

        var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        var tagsDirectoryPath = Path.Combine(projectDirectory, "Tags");
        
        _logger.LogInformation($"Directory path: {tagsDirectoryPath}");
        
        var associatedNotes = new List<NoteModel>();
        

        // Make the tagName case-insensitive
        var tagFileName = $"{tagName.ToLower()}.txt";
        var tagFilePath = Path.Combine(tagsDirectoryPath, tagFileName);

        _logger.LogInformation($"Reading notes associated with tag: {tagFilePath}");
        
        if (File.Exists(tagFilePath))
        {
            var noteIds = File.ReadAllLines(tagFilePath).Select(Guid.Parse).ToList();

            _logger.LogInformation($"NoteId: {noteIds}");
            
            // Now fetch each note by its GUID
            foreach (var noteId in noteIds)
            {
                var note = GetNoteById(noteId);
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

        return associatedNotes;
    }

    private NoteModel ParseNoteContent(string content)
    {
        // Log the raw content to check its structure
        _logger.LogInformation($"Parsing note content: {content}");

        var parts = content.Split('\n'); // Or your specific delimiter

        // Ensure the array has enough parts to avoid out-of-bounds access
        if (parts.Length < 5) // Adjust based on how many parts you expect
        {
            _logger.LogError("The note content is incomplete or malformed.");
            throw new InvalidOperationException("The note content is incomplete or malformed.");
        }

        try
        {
            // Proceed to parse the parts into a NoteModel
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

}

         