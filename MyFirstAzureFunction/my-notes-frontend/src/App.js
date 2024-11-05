import React, { useState } from 'react';
import CreateNote from './components/CreateNote';
import NotesList from './components/NotesList';
import './App.css'; // Import main CSS

const App = () => {
    const [activeView, setActiveView] = useState('create'); // State to track the active view

    return (
        <div className="app-container">
            <div className="tab-container">
                <h2 className={`tab ${activeView === 'create' ? 'active' : ''}`} onClick={() => setActiveView('create')}>
                    Create New Note
                </h2>
                <h2 className={`tab ${activeView === 'view' ? 'active' : ''}`} onClick={() => setActiveView('view')}>
                    View All Notes
                </h2>
            </div>
            <div className="view-container">
                {activeView === 'create' && <CreateNote />}
                {activeView === 'view' && <NotesList />}
            </div>
        </div>
    );
};

export default App;