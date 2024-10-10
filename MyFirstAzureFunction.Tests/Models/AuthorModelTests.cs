using NUnit.Framework;
using MyFirstAzureFunction.Models;
using System;
using System.Collections.Generic;

namespace MyFirstAzureFunction.Tests.Models
{
    [TestFixture]
    public class AuthorModelTests
    {
        [Test]
        public void AuthorModel_CanSetAndGetProperties()
        {
            // Arrange: Create a new AuthorModel and define test values
            var author = new AuthorModel();
            var testAuthorID = 5;
            var testNoteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act: Set properties
            author.AuthorID = testAuthorID;
            author.NoteIds = testNoteIds;

            // Assert: Verify the properties are set correctly
            Assert.That(author.AuthorID, Is.EqualTo(testAuthorID));
            Assert.That(author.NoteIds, Is.EqualTo(testNoteIds));
        }

        [Test]
        public void AuthorModel_InitializesNoteIdsToEmptyList()
        {
            // Arrange: Create a new AuthorModel
            var author = new AuthorModel();

            // Act: Retrieve NoteIds property

            // Assert: Verify NoteIds is initialized to an empty list
            Assert.IsNotNull(author.NoteIds);
            Assert.IsEmpty(author.NoteIds);
        }
    }
}