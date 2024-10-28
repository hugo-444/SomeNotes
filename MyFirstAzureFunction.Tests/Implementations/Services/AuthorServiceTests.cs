using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using MyFirstAzureFunction.Implementations.Services;
using NUnit.Framework;

namespace MyFirstAzureFunction.Tests.Services
{
    [TestFixture]
    public class AuthorServiceTests
    {
        private ILogger<AuthorService> _logger;
        private AuthorService _authorService; // The service being tested
        private string _authorsDirectoryPath; // Path to the working Authors directory
        private List<string> _testFiles; // Keep track of created test files for deletion

        [SetUp]
        public void Setup()
        {
            // Set up the logger for AuthorService
            var mockLogger = new Mock<ILogger<AuthorService>>();
            _logger = mockLogger.Object;
            _authorService = new AuthorService(_logger);

            // Set up the actual directory path for the working "Authors" directory
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            _authorsDirectoryPath = Path.Combine(projectDirectory, "Authors");

            // Ensure the Authors directory exists
            Directory.CreateDirectory(_authorsDirectoryPath);

            // Initialize the list to keep track of test files
            _testFiles = new List<string>();
        }

        // Utility method to create a test author file in the working Authors directory
        private void CreateAuthorFile(int authorId, List<Guid> noteIds)
        {
            var authorFilePath = Path.Combine(_authorsDirectoryPath, $"Author_{authorId}.txt");
            File.WriteAllLines(authorFilePath, noteIds.Select(id => id.ToString()));
            _testFiles.Add(authorFilePath); // Keep track of this file for deletion after the test
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up the created test files after each test
            foreach (var filePath in _testFiles)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        // Test case for when an author has notes
        [Test]
        public void GetNotesByAuthorId_ReturnsNoteIds_WhenFileExists()
        {
            // Arrange
            var authorId = 40;
            var expectedNoteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            CreateAuthorFile(authorId, expectedNoteIds); // Create file for author with two note IDs

            // Act
            var result = _authorService.GetNotesByAuthorId(authorId);

            // Assert
            Assert.AreEqual(expectedNoteIds, result); // Verify the returned notes match the file contents
        }

        // Test case for when no notes are found for an author
        [Test]
        public void GetNotesByAuthorId_ReturnsEmptyList_WhenFileDoesNotExist()
        {
            // Arrange
            var authorId = 20; // No file will be created for this author

            // Act
            var result = _authorService.GetNotesByAuthorId(authorId);

            // Assert
            Assert.IsEmpty(result); // Expect an empty list
        }

        [Test]
        public void AddNoteToAuthor_AppendsNote_WhenFileExists()
        {
            // Arrange
            var authorId = 30;
            var noteId1 = Guid.NewGuid(); // First note ID
            var noteId2 = Guid.NewGuid(); // Second note ID to append

            // Create a file with the first note ID
            var authorFilePath = Path.Combine(_authorsDirectoryPath, $"Author_{authorId}.txt");
            File.WriteAllLines(authorFilePath, new[] { noteId1.ToString() });
            _testFiles.Add(authorFilePath); // Track this file for cleanup

            // Act
            _authorService.AddNoteToAuthor(authorId, noteId2); // Add the second note to the file

            // Assert
            var fileContent = File.ReadAllLines(authorFilePath); // Read the contents of the file
            Assert.Contains(noteId1.ToString(), fileContent); // Ensure first note is still present
            Assert.Contains(noteId2.ToString(), fileContent); // Ensure second note is appended
        }

        // Test case for adding a note when no file exists
        [Test]
        public void AddNoteToAuthor_CreatesNewFile_WhenFileDoesNotExist()
        {
            // Arrange
            var authorId = 40;
            var noteId = Guid.NewGuid(); // Note to add

            var authorFilePath = Path.Combine(_authorsDirectoryPath, $"Author_{authorId}.txt");

            // Ensure the file does not exist before the test
            if (File.Exists(authorFilePath))
            {
                File.Delete(authorFilePath);
            }

            // Act
            _authorService.AddNoteToAuthor(authorId, noteId);
            _testFiles.Add(authorFilePath); // Track this file for cleanup

            // Assert
            var fileContent = File.ReadAllLines(authorFilePath); // Read the contents of the file
            Assert.Contains(noteId.ToString(), fileContent); // Ensure note is present
        }
        
    }
}