# Prescription Decoder API

An AI-powered application for decoding handwritten prescriptions using Multi-Modal LLMs (Gemini, Groq/Llama) and OCR.

## Features

-   **Hybrid AI Decoding**: Combines OCR text hints with Visual AI models for maximum accuracy.
-   **Deep Reasoning**: Models use "Visual Deconstruction" to trace overlapping words and validate against doctor specialty.
-   **Unified Hosting**: React Frontend served directly from the ASP.NET Core API Backend.
-   **Structure Extraction**: Returns structured JSON with Doctor, Specialist, and Medicines.

## Prerequisites

-   .NET 8.0 SDK
-   Node.js (for building the client)

## Setup

1.  **Clone the repository**:
    ```bash
    git clone <repo-url>
    cd PrescriptionDecoder.API
    ```

2.  **Configuration**:
    The application requires API keys for Groq, Gemini, and OCR.Space.
    Update `PrescriptionDecoder.API/appsettings.json` with your keys:
    ```json
    "ApiKeys": {
      "Groq": "YOUR_GROQ_API_KEY",
      "Gemini": "YOUR_GEMINI_API_KEY",
      "OcrSpace": "YOUR_OCR_API_KEY"
    }
    ```

3.  **Build Client**:
    ```bash
    cd PrescriptionDecoder.Client
    npm install
    npm run build
    # The output is automatically copied to the API's wwwroot
    ```

4.  **Run Application**:
    Navigate back to the API folder and run:
    ```bash
    cd ../PrescriptionDecoder.API
    dotnet run
    ```
    Open `http://localhost:5000` (or the port shown in console).

## Deployment

### Option A: Azure (Recommended for .NET)
Use the **Deployment Center** in Azure App Service to connect directly to this GitHub repository.

### Option B: Render (Free Docker Hosting)
1.  **Sign up** at [render.com](https://render.com).
2.  Click **New +** -> **Web Service**.
3.  Connect your GitHub account and select this repository (`Prescription-Decoder`).
4.  Render will automatically detect the `Dockerfile`.
5.  **Environment Variables**:
    You MUST add your API keys in the specific format .NET expects (using `__` for nesting):
    
    | Key | Value |
    | :--- | :--- |
    | `ApiKeys__Groq` | `your_groq_key` |
    | `ApiKeys__Gemini` | `your_gemini_key` |
    | `ApiKeys__OcrSpace` | `your_ocr_key` |
    | `ASPNETCORE_ENVIRONMENT` | `Production` |

6.  Click **Create Web Service**. Render will build the Docker image (takes ~3-5 mins) and deploy it.

## License

MIT