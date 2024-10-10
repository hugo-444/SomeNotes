namespace MyFirstAzureFunction.Models;

public class NoteModel
{
    public Guid NoteId { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Content { get; set; }
    public int AuthorID { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public List<string> Tags { get; set; } = new List<string>(); // hold associated tags
}