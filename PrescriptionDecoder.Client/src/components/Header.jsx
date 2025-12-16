import React from 'react';
import { FileText, Sparkles } from 'lucide-react';

export default function Header() {
    return (
        <header style={{ padding: '3rem 0 2rem', textAlign: 'center' }}>
            <div style={{
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                background: 'rgba(79, 70, 229, 0.1)',
                padding: '1rem',
                borderRadius: '50%',
                marginBottom: '1rem'
            }}>
                <FileText size={32} color="var(--primary)" />
            </div>
            <h1 style={{
                fontSize: '2rem',
                fontWeight: 800,
                letterSpacing: '-0.025em',
                background: 'linear-gradient(to right, var(--primary), var(--secondary))',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '0.5rem',
                marginBottom: '0.5rem'
            }}>
                Prescription AI <Sparkles size={20} color="var(--secondary)" />
            </h1>
            <p style={{ color: 'var(--text-muted)', fontSize: '1.125rem' }}>
                Upload your prescription and let AI decode it instantly.
            </p>
        </header>
    );
}
