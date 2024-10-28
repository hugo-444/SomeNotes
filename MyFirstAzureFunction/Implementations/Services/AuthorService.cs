using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;

namespace MyFirstAzureFunction.Implementations.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly ILogger<AuthorService> _logger;
        
        public AuthorService(ILogger<AuthorService> logger)
        {
            _logger = logger;
        }
        
        public List<Guid> GetNotesByAuthorId(int authorId)
        {
            _logger.LogInformation($"Fetching notes for author {authorId}");

            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var authorsDirectoryPath = Path.Combine(projectDirectory, "Authors");
            var authorFilePath = Path.Combine(authorsDirectoryPath, $"Author_{authorId}.txt");

            if (!File.Exists(authorFilePath))
            {
                _logger.LogInformation($"No notes found for author {authorId}");
                return new List<Guid>();
            }

            var noteIds = File.ReadAllLines(authorFilePath).Select(Guid.Parse).ToList();
            return noteIds;
        }

        public void AddNoteToAuthor(int authorId, Guid noteId)
        {
            _logger.LogInformation($"Associating note {noteId} with author {authorId}");

            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var authorsDirectoryPath = Path.Combine(projectDirectory, "Authors");
            Directory.CreateDirectory(authorsDirectoryPath); // Ensure the Author directory exists

            var authorFilePath = Path.Combine(authorsDirectoryPath, $"Author_{authorId}.txt");

            // Add NoteID to the author file (create the file if it doesn't exist)
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