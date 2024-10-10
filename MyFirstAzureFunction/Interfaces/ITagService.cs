using System;
using System.Collections.Generic;
using MyFirstAzureFunction.Models;

namespace MyFirstAzureFunction.Interfaces
{
    public interface ITagService
    {
        List<string> GetTagsForNoteId(Guid noteId);
        List<NoteModel> GetNotesByTag(string tagName);
    }
    
}