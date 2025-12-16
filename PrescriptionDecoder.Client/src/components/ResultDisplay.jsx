import React from 'react';
import { Pill, Clock, Hash, CheckCircle, Info, Activity } from 'lucide-react';

export default function ResultDisplay({ results }) {
    // results is now { doctorName, specialist, medicines: [], ... }
    const medicines = results?.medicines || [];
    const doctorName = results?.doctorName;
    const specialist = results?.specialist;

    if (!medicines || medicines.length === 0) return null;

    const getConfidenceColor = (score) => {
        if (score >= 80) return 'var(--success)';
        if (score >= 50) return '#f59e0b'; // Amber
        return 'var(--error)';
    };

    return (
        <div className="animate-fade-in">
            {/* New Header for Doctor Info */}
            {(doctorName || specialist) && (
                <div style={{
                    marginBottom: '1.5rem',
                    padding: '1rem',
                    background: 'var(--surface-sunken)',
                    borderRadius: 'var(--radius)',
                    border: '1px solid var(--border)'
                }}>
                    {doctorName && <h3 style={{ margin: 0, fontSize: '1.1rem' }}>{doctorName}</h3>}
                    {specialist && <p style={{ margin: 0, color: 'var(--text-muted)', fontSize: '0.9rem' }}>{specialist}</p>}
                </div>
            )}

            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
                <h2 style={{ fontSize: '1.5rem', fontWeight: 700 }}>Decoded Medicines</h2>
                <span style={{
                    background: 'rgba(79, 70, 229, 0.1)',
                    color: 'var(--primary)',
                    padding: '0.25rem 0.75rem',
                    borderRadius: '20px',
                    fontSize: '0.875rem',
                    fontWeight: 600,
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.25rem'
                }}>
                    <CheckCircle size={14} />
                    {medicines.length} Found
                </span>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.5rem' }}>
                {medicines.map((item, index) => {
                    const confidence = item.Confidence || item.confidence || 0;
                    const confColor = getConfidenceColor(confidence);

                    return (
                        <div
                            key={index}
                            className="glass-panel"
                            style={{
                                padding: '1.5rem',
                                display: 'flex',
                                flexDirection: 'column',
                                gap: '1rem',
                                borderLeft: `4px solid ${confColor}`,
                                transition: 'transform 0.2s ease',
                            }}
                            onMouseEnter={(e) => e.currentTarget.style.transform = 'translateY(-4px)'}
                            onMouseLeave={(e) => e.currentTarget.style.transform = 'translateY(0)'}
                        >
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                                <div style={{ display: 'flex', alignItems: 'start', gap: '0.75rem' }}>
                                    <div style={{
                                        background: 'rgba(79, 70, 229, 0.1)',
                                        padding: '0.5rem',
                                        borderRadius: '8px',
                                        color: 'var(--primary)'
                                    }}>
                                        <Pill size={24} />
                                    </div>
                                    <div>
                                        <span style={{ fontSize: '0.75rem', textTransform: 'uppercase', color: 'var(--text-muted)', fontWeight: 600, letterSpacing: '0.05em' }}>Drug Name</span>
                                        <h3 style={{ fontSize: '1.25rem', fontWeight: 800, lineHeight: 1.2 }}>{item.Drug || item.drug}</h3>
                                    </div>
                                </div>

                                {/* Confidence Badge */}
                                <div style={{ textAlign: 'right' }}>
                                    <div style={{
                                        display: 'inline-flex',
                                        alignItems: 'center',
                                        gap: '0.25rem',
                                        color: confColor,
                                        fontWeight: 700,
                                        fontSize: '0.875rem'
                                    }}>
                                        <Activity size={14} />
                                        {confidence}%
                                    </div>
                                    <div style={{ fontSize: '0.65rem', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Confidence</div>
                                </div>
                            </div>

                            <div style={{ borderTop: '1px solid var(--border)', paddingTop: '1rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                                <div>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.25rem', marginBottom: '0.25rem', color: 'var(--text-muted)' }}>
                                        <Hash size={14} />
                                        <span style={{ fontSize: '0.75rem', fontWeight: 600 }}>Dose</span>
                                    </div>
                                    <p style={{ fontWeight: 500 }}>{item.Dose || item.dose || 'N/A'}</p>
                                </div>

                                <div>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.25rem', marginBottom: '0.25rem', color: 'var(--text-muted)' }}>
                                        <Clock size={14} />
                                        <span style={{ fontSize: '0.75rem', fontWeight: 600 }}>Frequency</span>
                                    </div>
                                    <p style={{ fontWeight: 500 }}>{item.Freq || item.freq || 'N/A'}</p>
                                </div>
                            </div>

                            {/* AI Notes Section */}
                            {(item.Notes || item.notes) && (
                                <div style={{
                                    marginTop: '0.5rem',
                                    background: 'var(--background)',
                                    padding: '0.75rem',
                                    borderRadius: '8px',
                                    border: '1px solid var(--border)',
                                    fontSize: '0.875rem',
                                    color: 'var(--text-muted)'
                                }}>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.25rem', marginBottom: '0.25rem', color: 'var(--primary)', fontWeight: 600 }}>
                                        <Info size={12} />
                                        <span>AI Reasoning</span>
                                    </div>
                                    {item.Notes || item.notes}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
