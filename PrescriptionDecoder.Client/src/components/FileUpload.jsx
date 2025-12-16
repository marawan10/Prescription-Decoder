import React, { useState, useRef } from 'react';
import { UploadCloud, Image as ImageIcon, X } from 'lucide-react';

export default function FileUpload({ onFileSelect, loading }) {
    const [dragActive, setDragActive] = useState(false);
    const [preview, setPreview] = useState(null);
    const inputRef = useRef(null);

    const handleDrag = (e) => {
        e.preventDefault();
        e.stopPropagation();
        if (e.type === "dragenter" || e.type === "dragover") {
            setDragActive(true);
        } else if (e.type === "dragleave") {
            setDragActive(false);
        }
    };

    const handleDrop = (e) => {
        e.preventDefault();
        e.stopPropagation();
        setDragActive(false);

        if (e.dataTransfer.files && e.dataTransfer.files[0]) {
            handleFile(e.dataTransfer.files[0]);
        }
    };

    const handleChange = (e) => {
        e.preventDefault();
        if (e.target.files && e.target.files[0]) {
            handleFile(e.target.files[0]);
        }
    };

    const handleFile = (file) => {
        // Create preview
        const reader = new FileReader();
        reader.onloadend = () => {
            setPreview(reader.result);
        };
        reader.readAsDataURL(file);

        // Notify parent
        onFileSelect(file);
    };

    const clearFile = (e) => {
        e.stopPropagation();
        setPreview(null);
        onFileSelect(null);
        if (inputRef.current) inputRef.current.value = '';
    };

    return (
        <div className="glass-panel" style={{ padding: '2rem', marginBottom: '2rem' }}>
            <div
                className={`upload-zone ${dragActive ? 'active' : ''}`}
                onDragEnter={handleDrag}
                onDragLeave={handleDrag}
                onDragOver={handleDrag}
                onDrop={handleDrop}
                onClick={() => inputRef.current?.click()}
                style={{
                    border: '2px dashed var(--border)',
                    borderRadius: 'var(--radius)',
                    padding: '3rem 1.5rem',
                    textAlign: 'center',
                    cursor: loading ? 'not-allowed' : 'pointer',
                    backgroundColor: dragActive ? 'rgba(79, 70, 229, 0.05)' : 'transparent',
                    borderColor: dragActive ? 'var(--primary)' : 'var(--border)',
                    transition: 'all 0.2s ease',
                    position: 'relative',
                    overflow: 'hidden'
                }}
            >
                <input
                    ref={inputRef}
                    type="file"
                    accept="image/*"
                    onChange={handleChange}
                    style={{ display: 'none' }}
                    disabled={loading}
                />

                {preview ? (
                    <div className="preview-container" style={{ position: 'relative', display: 'inline-block' }}>
                        <img
                            src={preview}
                            alt="Prescription Preview"
                            style={{
                                maxHeight: '300px',
                                maxWidth: '100%',
                                borderRadius: 'var(--radius)',
                                boxShadow: 'var(--shadow-md)'
                            }}
                        />
                        <button
                            onClick={clearFile}
                            style={{
                                position: 'absolute',
                                top: '-10px',
                                right: '-10px',
                                background: 'var(--surface)',
                                border: '1px solid var(--border)',
                                borderRadius: '50%',
                                padding: '0.25rem',
                                cursor: 'pointer',
                                boxShadow: 'var(--shadow-sm)',
                                color: 'var(--text-muted)'
                            }}
                        >
                            <X size={16} />
                        </button>
                    </div>
                ) : (
                    <div style={{ pointerEvents: 'none' }}>
                        <div style={{
                            background: 'var(--background)',
                            borderRadius: '50%',
                            width: '64px',
                            height: '64px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            margin: '0 auto 1rem'
                        }}>
                            <UploadCloud size={32} color="var(--primary)" />
                        </div>
                        <h3 style={{ fontSize: '1.25rem', fontWeight: 600, marginBottom: '0.5rem' }}>
                            Click to upload or drag & drop
                        </h3>
                        <p style={{ color: 'var(--text-muted)' }}>
                            Supports JPG, PNG, WEBP
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
}
