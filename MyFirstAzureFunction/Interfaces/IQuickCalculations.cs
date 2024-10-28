using MyFirstAzureFunction.Models;

namespace MyFirstAzureFunction.Interfaces;

public interface IQuickCalculations
{
    //int CreateNewNote(string Title, string Content, int AuthorID, Guid NoteID, DateTime DateCreated);
  
    int CreateNewNote(string Title, string Content, int AuthorID, Guid NoteID, DateTime DateCreated, List<string> Tags);
    
    // Retrieve a single note by ID
    NoteModel GetNoteById(Guid noteId);  
    
    // Retrieve all notes
    List<NoteModel> GetAllNotes();  
   
    List<NoteModel> GetNotesByTag(string tagName);
}