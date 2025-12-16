import React from 'react';

export default function Layout({ children }) {
    return (
        <div className="layout-wrapper" style={{ paddingBottom: '4rem' }}>
            <main className="container animate-fade-in">
                {children}
            </main>
        </div>
    );
}
