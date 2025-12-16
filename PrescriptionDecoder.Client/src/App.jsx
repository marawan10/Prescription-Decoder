import React, { useState } from 'react';
import Layout from './components/Layout';
import Header from './components/Header';
import FileUpload from './components/FileUpload';
import ResultDisplay from './components/ResultDisplay';
import { uploadPrescription } from './services/api';
import { AlertCircle, Loader2 } from 'lucide-react';

function App() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [results, setResults] = useState(null);

  const handleFileSelect = async (file) => {
    if (!file) {
      setResults(null);
      setError(null);
      return;
    }

    setLoading(true);
    setError(null);
    setResults(null);

    try {
      const response = await uploadPrescription(file);
      // Expected response: { message: "Success", data: [...] }
      if (response && response.data) {
        setResults(response.data);
      } else {
        throw new Error("Invalid response format from server");
      }
    } catch (err) {
      setError(err.message || "An unexpected error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <Header />

      <div style={{ maxWidth: '640px', margin: '0 auto' }}>
        <FileUpload onFileSelect={handleFileSelect} loading={loading} />
      </div>

      {loading && (
        <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }} className="animate-fade-in">
          <Loader2 className="animate-spin" size={32} style={{ margin: '0 auto 1rem', display: 'block' }} />
          <p>Analyzing prescription with AI...</p>
        </div>
      )}

      {error && (
        <div className="animate-fade-in" style={{
          background: 'rgba(239, 68, 68, 0.1)',
          border: '1px solid var(--error)',
          borderRadius: 'var(--radius)',
          padding: '1rem',
          color: 'var(--error)',
          marginBottom: '2rem',
          display: 'flex',
          alignItems: 'center',
          gap: '0.75rem'
        }}>
          <AlertCircle size={20} />
          <p>{error}</p>
        </div>
      )}

      <ResultDisplay results={results} />
    </Layout>
  );
}

export default App;
