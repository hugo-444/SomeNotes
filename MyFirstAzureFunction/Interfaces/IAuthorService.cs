using MyFirstAzureFunction.Models;
using System;

namespace MyFirstAzureFunction.Interfaces
{
    public interface IAuthorService
    {
        List<Guid> GetNotesByAuthorId(int authorId);
        void AddNoteToAuthor(int authorId, Guid noteId);
    }
}