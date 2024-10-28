using System;
using System.Collections.Generic;

namespace MyFirstAzureFunction.Models
{
    public class AuthorModel
    {
        public int AuthorID { get; set; }

        public List<Guid> NoteIds { get; set; } = new List<Guid>();
    }
}