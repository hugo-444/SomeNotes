using NUnit.Framework;
using MyFirstAzureFunction.Models;
using System;
using System.Collections.Generic;

namespace MyFirstAzureFunction.Tests.Models
{
    [TestFixture]
    public class TagModelTests
    {
        [Test]
        public void TagModel_CanSetAndGetProperties()
        {
            // Arrange: Create a new TagModel and define test values
            var tag = new TagModel();
            var testTagName = "TestTag";
            var testNoteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act: Set properties
            tag.TagName = testTagName;
            tag.NoteIds = testNoteIds;

            // Assert: Verify the properties are set correctly
            Assert.That(tag.TagName, Is.EqualTo(testTagName));
            Assert.That(tag.NoteIds, Is.EqualTo(testNoteIds));
        }

        
    }
}

