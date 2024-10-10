namespace MyFirstAzureFunction.Models;

public class TagModel
{
    public string TagName { get; set; }
    public List<Guid> NoteIds { get; set; }
}