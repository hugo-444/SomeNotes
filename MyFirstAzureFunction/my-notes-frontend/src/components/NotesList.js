import React, { useEffect, useState } from 'react';
import { getAllNotes } from '../api/notesAPI';
import './NotesList.css'; // Import CSS

const NotesList = () => {
  const [notes, setNotes] = useState([]);

    useEffect(() => {
        (async function fetchNotes() {
            const data = await getAllNotes();
            setNotes(data);
        })();
    }, []);
    
  return (
    <div>
      <h1>Notes</h1>
      <ul>
        {notes.map(note => (
            <li key={note.date}>
                <h2>{note.title}</h2>
                <p>Note by Author ID:{note.authorId} Note ID: {note.id}</p>
                <p>Created on: {note.date}</p>
                <p>Tags: {note.tags}</p>
                
                <p className="content-paragraph">{note.content}</p>


            </li>
        ))}
      </ul>
    </div>
  );
};

export default NotesList;
