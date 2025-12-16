export const uploadPrescription = async (file) => {
    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch('/api/Prescription/upload', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            const errorText = await response.text();
            let errorData = {};
            try {
                errorData = JSON.parse(errorText);
            } catch (e) {
                // Not JSON
            }
            throw new Error(errorData.error || errorData.details || errorText || 'Failed to upload');
        }

        return await response.json();
    } catch (error) {
        console.error("Upload error:", error);
        throw error;
    }
};
