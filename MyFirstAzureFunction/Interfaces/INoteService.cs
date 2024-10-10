using MyFirstAzureFunction.Models;
using System;
using System.Collections.Generic;

namespace MyFirstAzureFunction.Interfaces
{
    public interface INoteService
    {
        int CreateNewNote(string title, string content, int authorId, Guid noteId, DateTime date, List<string> tags = null);
        List<NoteModel> GetAllNotes();
        NoteModel GetNoteById(Guid noteId);
    }
}