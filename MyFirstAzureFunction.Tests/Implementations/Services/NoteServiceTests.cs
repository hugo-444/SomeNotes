using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using MyFirstAzureFunction.Implementations.Services;
using MyFirstAzureFunction.Models;
using NUnit.Framework;

namespace MyFirstAzureFunction.Tests.Services
{
    [TestFixture]
    public class NoteServiceTests
    {
        // Mock for the logger
        private Mock<ILogger<NoteService>> _mockLogger;
        // Instance of the service being tested
        private NoteService _noteService;

        // Paths for testing
        private string _notesDirectoryPath;
        private string _tagsDirectoryPath;

        // Setup method, executed before each test to initialize the environment
        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<NoteService>>();
            _noteService = new NoteService(_mockLogger.Object);

            // Set up directories for storing notes and tags
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            _notesDirectoryPath = Path.Combine(projectDirectory, "MyNotes");
            _tagsDirectoryPath = Path.Combine(projectDirectory, "Tags");

            // Ensure test directories are clean before running tests
            if (Directory.Exists(_notesDirectoryPath))
                Directory.Delete(_notesDirectoryPath, true);
            if (Directory.Exists(_tagsDirectoryPath))
                Directory.Delete(_tagsDirectoryPath, true);

            // Recreate directories for testing
            Directory.CreateDirectory(_notesDirectoryPath);
            Directory.CreateDirectory(_tagsDirectoryPath);
        }

        // Cleanup method to remove test directories after each test
        [TearDown]
        public void Cleanup()
        {
            // Clean up created files and directories after each test
            if (Directory.Exists(_notesDirectoryPath))
                Directory.Delete(_notesDirectoryPath, true);
            if (Directory.Exists(_tagsDirectoryPath))
                Directory.Delete(_tagsDirectoryPath, true);
        }

        // Test case for creating a new note successfully
        [Test]
        public void CreateNewNote_ReturnsOne_WhenNoteIsCreatedSuccessfully()
        {
            // Arrange: Set up the parameters for the note creation
            var noteId = Guid.NewGuid();
            var title = "Test Note";
            var content = "This is a test note.";
            var authorId = 1;
            var date = DateTime.Now;
            var tags = new List<string> { "Tag1", "Tag2" };

            // Act: Call the CreateNewNote function
            var result = _noteService.CreateNewNote(title, content, authorId, noteId, date, tags);

            // Assert: Verify that the function returns the correct result
            Assert.AreEqual(1, result);

            // Assert: Check if the note file was created
            var noteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{noteId}.txt");
            Assert.IsTrue(File.Exists(noteFilePath));

            // Assert: Verify the tags are properly associated with the note
            var tagFile1 = Path.Combine(_tagsDirectoryPath, "tag1.txt");
            var tagFile2 = Path.Combine(_tagsDirectoryPath, "tag2.txt");
            Assert.IsTrue(File.Exists(tagFile1));  // Tag1 should be created
            Assert.IsTrue(File.Exists(tagFile2));  // Tag2 should be created
            Assert.IsTrue(File.ReadAllText(tagFile1).Contains(noteId.ToString()));  // Note should be linked to Tag1
            Assert.IsTrue(File.ReadAllText(tagFile2).Contains(noteId.ToString()));  // Note should be linked to Tag2
        }

        // Test case to verify the behavior when tags are not provided
        [Test]
        public void CreateNewNote_ReturnsOne_WhenNoTagsProvided()
        {
            // Arrange: Set up valid data for note creation but without tags
            var noteId = Guid.NewGuid();
            var title = "Note Without Tags";
            var content = "This is a test note without tags.";
            var authorId = 1;
            var date = DateTime.Now;

            // Act: Call the CreateNewNote function without tags
            var result = _noteService.CreateNewNote(title, content, authorId, noteId, date);

            // Assert: Verify that the function returns the correct result
            Assert.AreEqual(1, result);

            // Assert: Check if the note file was created
            var noteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{noteId}.txt");
            Assert.IsTrue(File.Exists(noteFilePath));

            // Assert: Check that no tag files were created since no tags were provided
            var tagFiles = Directory.GetFiles(_tagsDirectoryPath);
            Assert.IsEmpty(tagFiles);  // There should be no tag files
        }

        // Test case to verify that tags are associated correctly with the note
        [Test]
        public void CreateNewNote_AssociatesTagsWithNote_Correctly()
        {
            // Arrange: Set up valid data for note creation with tags
            var noteId = Guid.NewGuid();
            var title = "Note With Tags";
            var content = "This note has associated tags.";
            var authorId = 1;
            var date = DateTime.Now;
            var tags = new List<string> { "Tag1", "Tag2" };

            // Act: Call the CreateNewNote function
            var result = _noteService.CreateNewNote(title, content, authorId, noteId, date, tags);

            // Assert: Verify that the function returns the correct result
            Assert.AreEqual(1, result);

            // Assert: Check if the note file was created
            var noteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{noteId}.txt");
            Assert.IsTrue(File.Exists(noteFilePath));

            // Assert: Verify the tags are associated correctly
            var tagFile1 = Path.Combine(_tagsDirectoryPath, "tag1.txt");
            var tagFile2 = Path.Combine(_tagsDirectoryPath, "tag2.txt");
            Assert.IsTrue(File.Exists(tagFile1));
            Assert.IsTrue(File.Exists(tagFile2));
            Assert.IsTrue(File.ReadAllText(tagFile1).Contains(noteId.ToString()));
            Assert.IsTrue(File.ReadAllText(tagFile2).Contains(noteId.ToString()));
        }

        [Test]
        public void CreateNewNote_HandlesNoTagsProperly()
        {
            // Arrange
            var title = "Test Note";
            var content = "This is a test note.";
            var authorId = 1;
            var noteId = Guid.NewGuid();
            var date = DateTime.Now;
    
            // Act
            var result = _noteService.CreateNewNote(title, content, authorId, noteId, date);
    
            // Assert
            Assert.AreEqual(1, result, "Expected the note to be created even without tags.");
        }
        
        
        
        [Test]
        public void GetAllNotes_ReturnsListOfNotes_WhenFilesExist()
        {
            // Arrange: Create two well-formed note files in the MyNotes directory
            var noteId1 = Guid.NewGuid();
            var noteId2 = Guid.NewGuid();
    
            var wellFormedContent1 = $"NoteID: {noteId1}\nTitle: Note 1\nContent: This is note 1.\nAuthor ID: 1\nDate: {DateTime.Now}";
            var wellFormedContent2 = $"NoteID: {noteId2}\nTitle: Note 2\nContent: This is note 2.\nAuthor ID: 2\nDate: {DateTime.Now}";

            var noteFilePath1 = Path.Combine(_notesDirectoryPath, $"Note_{noteId1}.txt");
            var noteFilePath2 = Path.Combine(_notesDirectoryPath, $"Note_{noteId2}.txt");
    
            File.WriteAllText(noteFilePath1, wellFormedContent1);
            File.WriteAllText(noteFilePath2, wellFormedContent2);

            // Act: Fetch all notes
            var result = _noteService.GetAllNotes();

            // Assert: Ensure that both notes are returned and parsed correctly
            Assert.AreEqual(2, result.Count);
    
            var note1 = result.First(n => n.NoteId == noteId1);
            Assert.AreEqual("Note 1", note1.Title);
            Assert.AreEqual("This is note 1.", note1.Content);
            Assert.AreEqual(1, note1.AuthorID);
    
            var note2 = result.First(n => n.NoteId == noteId2);
            Assert.AreEqual("Note 2", note2.Title);
            Assert.AreEqual("This is note 2.", note2.Content);
            Assert.AreEqual(2, note2.AuthorID);
        }

        [Test]
        public void GetAllNotes_ReturnsPartialList_WhenSomeFilesAreMalformed()
        {
            // Arrange
            var validNoteId = Guid.NewGuid();
            var malformedNoteId = Guid.NewGuid();

            var validNoteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{validNoteId}.txt");
            var malformedNoteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{malformedNoteId}.txt");

            File.WriteAllText(validNoteFilePath, $"NoteID: {validNoteId}\nTitle: Test\nContent: Valid note\nAuthor ID: 1\nDate: {DateTime.Now}");
            File.WriteAllText(malformedNoteFilePath, $"Malformed content");

            // Act
            var result = _noteService.GetAllNotes();

            // Assert
            Assert.AreEqual(1, result.Count, "Expected only the valid note to be returned.");
        }
        
        
        
        [Test]
        public void GetNoteById_ReturnsNoteWithCorrectTags_WhenTagsExist()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var noteContent = $"NoteID: {noteId}\nTitle: Test Note\nContent: This is a test note.\nAuthor ID: 1\nDate: {DateTime.Now}";
            var noteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{noteId}.txt");

            // Create note file
            File.WriteAllText(noteFilePath, noteContent);

            // Create tag files
            var tagFilePath1 = Path.Combine(_tagsDirectoryPath, "tag1.txt");
            var tagFilePath2 = Path.Combine(_tagsDirectoryPath, "tag2.txt");

            File.WriteAllText(tagFilePath1, noteId.ToString());
            File.WriteAllText(tagFilePath2, noteId.ToString());

            // Act
            var result = _noteService.GetNoteById(noteId);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("tag1", result.Tags);
            Assert.Contains("tag2", result.Tags);
        }
        
        [Test]
        public void GetNoteById_ReturnsNull_WhenNoteDoesNotExist()
        {
            // Arrange: Define a note ID that does not have a corresponding file
            var nonExistentNoteId = Guid.NewGuid();

            // Act: Try to fetch the non-existent note by ID
            var result = _noteService.GetNoteById(nonExistentNoteId);

            // Assert: Ensure that the result is null since no file was found
            Assert.IsNull(result);
        }
        
        [Test]
        public void GetNoteById_ReturnsNoteWithTags_WhenTagsExist()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var noteContent = $"NoteID: {noteId}\nTitle: Test Note\nContent: This is a test note.\nAuthor ID: 1\nDate: {DateTime.Now}";
            var noteFilePath = Path.Combine(_notesDirectoryPath, $"Note_{noteId}.txt");

            // Create note file
            File.WriteAllText(noteFilePath, noteContent);

            // Create tag files
            var tagFilePath = Path.Combine(_tagsDirectoryPath, "tag1.txt");
            File.WriteAllText(tagFilePath, noteId.ToString());

            // Act
            var result = _noteService.GetNoteById(noteId);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Test Note", result.Title);
            Assert.Contains("tag1", result.Tags);
        }
        
        
        
        [Test]
        public void ParseNoteContent_ReturnsNoteModel_WhenContentIsWellFormed()
        {
            // Arrange
            var validContent = "NoteID: a6d8fcf3-1256-4e9e-bc3d-7b41f2a01385\nTitle: Test Note\nContent: Some content\nAuthor ID: 1\nDate: 2024-10-09";

            // Act
            var result = _noteService.GetType()
                .GetMethod("ParseNoteContent", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(_noteService, new object[] { validContent }) as NoteModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.NoteId, Is.EqualTo(Guid.Parse("a6d8fcf3-1256-4e9e-bc3d-7b41f2a01385")));
            Assert.That(result.Title, Is.EqualTo("Test Note"));
            Assert.That(result.Content, Is.EqualTo("Some content"));
            Assert.That(result.AuthorID, Is.EqualTo(1));
            Assert.That(result.Date, Is.EqualTo(DateTime.Parse("2024-10-09")));
        }
        
        [Test]
        public void ParseNoteContent_ThrowsException_WhenContentIsIncomplete()
        {
            // Arrange: Provide content with fewer than 5 lines
            var incompleteContent = "NoteID: a6d8fcf3-1256-4e9e-bc3d-7b41f2a01385\nTitle: Test Note";

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                _noteService.GetType()
                    .GetMethod("ParseNoteContent", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(_noteService, new object[] { incompleteContent })
            );

            Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            Assert.That(ex.InnerException?.Message, Is.EqualTo("The note content is incomplete or malformed."));
        }
        
        [Test]
        public void ParseNoteContent_ThrowsException_WhenNoteIdIsInvalid()
        {
            // Arrange: Provide invalid Guid value
            var invalidGuidContent = "NoteID: invalid_guid\nTitle: Test Note\nContent: Some content\nAuthor ID: 1\nDate: 2024-10-09";

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                _noteService.GetType()
                    .GetMethod("ParseNoteContent", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(_noteService, new object[] { invalidGuidContent })
            );

            Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            Assert.That(ex.InnerException?.Message, Is.EqualTo("The note content is invalid or malformed."));
        }
        
        [Test]
        public void ParseNoteContent_ThrowsException_WhenAuthorIdIsInvalid()
        {
            // Arrange: Provide an invalid AuthorID
            var invalidAuthorContent = "NoteID: a6d8fcf3-1256-4e9e-bc3d-7b41f2a01385\nTitle: Test Note\nContent: Some content\nAuthor ID: invalid_id\nDate: 2024-10-09";

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                _noteService.GetType()
                    .GetMethod("ParseNoteContent", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(_noteService, new object[] { invalidAuthorContent })
            );

            Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            Assert.That(ex.InnerException?.Message, Is.EqualTo("The note content is invalid or malformed."));
        }
        
        [Test]
        public void ParseNoteContent_ThrowsException_WhenDateIsInvalid()
        {
            // Arrange: Provide an invalid Date value
            var invalidDateContent = "NoteID: a6d8fcf3-1256-4e9e-bc3d-7b41f2a01385\nTitle: Test Note\nContent: Some content\nAuthor ID: 1\nDate: invalid_date";

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                _noteService.GetType()
                    .GetMethod("ParseNoteContent", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(_noteService, new object[] { invalidDateContent })
            );

            Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
            Assert.That(ex.InnerException?.Message, Is.EqualTo("The note content is invalid or malformed."));
        }
        
    }
}