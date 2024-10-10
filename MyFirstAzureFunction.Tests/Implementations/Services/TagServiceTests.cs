using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using MyFirstAzureFunction.Implementations.Services;
using MyFirstAzureFunction.Interfaces;
using MyFirstAzureFunction.Models;
using NUnit.Framework;

namespace MyFirstAzureFunction.Tests.Services
{
    [TestFixture]
    public class TagServiceTests
    {
        private Mock<ILogger<TagService>> _mockLogger; // Mock logger
        private Mock<INoteService> _mockNoteService; // Mock note service
        private TagService _tagService; // System under test (SUT)
        private string _testTagsDirectoryPath; // Path for test tag files

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockLogger = new Mock<ILogger<TagService>>();
            _mockNoteService = new Mock<INoteService>();

            // Initialize the service being tested
            _tagService = new TagService(_mockLogger.Object, _mockNoteService.Object);

            // Set up the actual directory path for test purposes
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            _testTagsDirectoryPath = Path.Combine(projectDirectory, "Tags");

            // Ensure the test directory exists
            Directory.CreateDirectory(_testTagsDirectoryPath);
        }

        [TearDown]
        public void Cleanup()
        {
            // Clean up the test directory after each test
            if (Directory.Exists(_testTagsDirectoryPath))
            {
                Directory.Delete(_testTagsDirectoryPath, true);
            }
        }

        // Utility method to create a tag file for a test case
        private void CreateTestTagFile(string tagName, List<Guid> noteIds)
        {
            var tagFilePath = Path.Combine(_testTagsDirectoryPath, $"{tagName.ToLower()}.txt");
            File.WriteAllLines(tagFilePath, noteIds.Select(id => id.ToString()));
        }

        // Test case for getting tags by note ID when tags exist
        [Test]
        public void GetTagsForNoteId_ReturnsTags_WhenTagsExist()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var expectedTags = new List<string> { "work", "project" };
            CreateTestTagFile("work", new List<Guid> { noteId });
            CreateTestTagFile("project", new List<Guid> { noteId });

            // Act
            var result = _tagService.GetTagsForNoteId(noteId);

            // Assert
            Assert.AreEqual(expectedTags, result); // Ensure all expected tags are returned
            
        }

        // Test case for getting tags by note ID when no tags exist
        [Test]
        public void GetTagsForNoteId_ReturnsEmptyList_WhenNoTagsExist()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            // Act
            var result = _tagService.GetTagsForNoteId(noteId);

            // Assert
            Assert.IsEmpty(result); // Expect an empty list
            
        }

        // Test case for getting notes by tag when notes exist
        [Test]
        public void GetNotesByTag_ReturnsNotes_WhenNotesExist()
        {
            // Arrange
            var tagName = "project";
            var noteId1 = Guid.NewGuid();
            var noteId2 = Guid.NewGuid();
            var expectedNotes = new List<NoteModel>
            {
                new NoteModel { NoteId = noteId1, Title = "Note 1" },
                new NoteModel { NoteId = noteId2, Title = "Note 2" }
            };

            // Create a tag file
            CreateTestTagFile(tagName, new List<Guid> { noteId1, noteId2 });

            // Mock the behavior of the note service to return expected notes
            _mockNoteService.Setup(s => s.GetNoteById(noteId1)).Returns(expectedNotes[0]);
            _mockNoteService.Setup(s => s.GetNoteById(noteId2)).Returns(expectedNotes[1]);

            // Act
            var result = _tagService.GetNotesByTag(tagName);

            // Assert
            Assert.AreEqual(expectedNotes.Count, result.Count);
            Assert.That(result, Is.EquivalentTo(expectedNotes)); // Ensure both notes are returned
         
        }

        // Test case for getting notes by tag when no tag file exists
        [Test]
        public void GetNotesByTag_ReturnsEmptyList_WhenTagFileDoesNotExist()
        {
            // Arrange
            var tagName = "nonexistent";

            // Act
            var result = _tagService.GetNotesByTag(tagName);

            // Assert
            Assert.IsEmpty(result); // Expect an empty list
          
        }

    }
}