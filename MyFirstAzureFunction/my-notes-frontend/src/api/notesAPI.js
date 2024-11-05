// src/api/notesApi.js
const API_BASE_URL = process.env.REACT_APP_API_URL;

// Function to create a new note
export async function createNote(data) {
  //http://localhost:7115/api
  
  try {
    const response = await fetch(`${API_BASE_URL}/CreateNewNote`, { 
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorData = await response.text(); // Try to parse error data if available
      throw new Error(`HTTP error! status: ${response.status}, message: ${errorData?.message || response.statusText}`);
    }

    return await response.text(); // Parse the successful response as JSON
  } catch (error) {
    console.error('Error creating note:', error);
    throw error; // Re-throw the error to be caught in the handleSubmit function
  }
}

// Function to get all notes
export async function getAllNotes() {
  const response = await fetch(`${API_BASE_URL}/allNotes`, {
    method: 'GET',
  });
  const data = await response.json();
  return data.map(note => ({
    id: note.NoteId,
    title: note.Title,
    content: note.Content,
    authorId: note.AuthorID,
    date: note.Date,
    tags: note.Tags,
  })); // Adjusting the structure to match response
}

// Function to get a note by its ID
export async function getNoteById(id) {
  const response = await fetch(`${API_BASE_URL}/notes/${id}`, {
    method: 'GET',
  });
  const note = await response.json();
  return {
    id: note.NoteId,
    title: note.Title,
    content: note.Content,
    authorId: note.AuthorID,
    date: note.Date,
    tags: note.Tags,
  };
}

// Function to get notes by a specific tag
export async function getNotesByTag(tagName) {
  const response = await fetch(`${API_BASE_URL}/tag/${tagName}`, {
    method: 'GET',
  });
  const data = await response.json();
  return data.NotePreviews.map(note => ({
    id: note.NoteId,
    title: note.Title,
    content: note.Content,
  }));
}
