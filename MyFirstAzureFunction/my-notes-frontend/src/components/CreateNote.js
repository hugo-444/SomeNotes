import React, { useState } from 'react';
import { createNote } from '../api/notesAPI';
import './CreateNote.css'; // Import CSS


const CreateNote = () => {
    const [title, setTitle] = useState('');
    const [authorId, setAuthorId] = useState('');
    const [content, setContent] = useState('');
    const [tags, setTags] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        const formattedTags = tags ? tags.split(',').map(tag => tag.trim()).filter(Boolean) : [];
        const authorIdValue = parseInt(authorId, 10);

        if (isNaN(authorIdValue)) {
            alert('Invalid Author ID. Please enter a valid number.');
            return;
        }

        const newNote = {
            title,
            content,
            authorId: authorIdValue,
            tags: formattedTags,
        };

        try {
            const response = await createNote(newNote); // This should already return parsed data or text
            console.log('Note created successfully:', response);
            setTitle('');
            setContent('');
            setAuthorId('');
            setTags('');
            alert('Note created!');
        } catch (error) {
            console.error('Error creating note:', error);
            alert(`Failed to create note: ${error.message}`);
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <h2>Create New Note</h2>
            <input
                type="text"
                placeholder="Title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                required
            />
            <input
                type="number"
                placeholder="Author ID"
                value={authorId}
                onChange={(e) => setAuthorId(e.target.value ? parseInt(e.target.value, 10) : '')}
                required
            />
            <textarea
                placeholder="Content"
                value={content}
                onChange={(e) => setContent(e.target.value)}
                required
            />
            <input
                type="text"
                placeholder="Tags (comma-separated)"
                value={tags}
                onChange={(e) => setTags(e.target.value)}
            />
            <button type="submit">Create Note</button>
        </form>
    );
};

export default CreateNote;