# Interview Question 10 — Quiz and Saved Results

A two-question quiz for IT 10-1 and IT 10-2. The API seeds the mock questions, returns answer options without exposing the correct answer, validates that each question has exactly one selection, computes the score on the server, and stores the participant name, score, and selected answers. “สอบอีกครั้ง” clears the current attempt and starts a new one.

## Run locally

Requires .NET 10, Node.js 22.22.3+, and npm 10.9.8+.

```powershell
dotnet run --project backend/Question010.Api/Question010.Api.csproj --launch-profile http
```

Run `npm ci` and `npm start` from `frontend`, then open `http://localhost:4209`. Angular proxies `/api` to `http://localhost:5225`. SQLite creates `backend/Question010.Api/quiz.db` and seeds the two mock questions at first start.

## API

- `GET /api/quiz/questions` returns questions and options without answer keys.
- `POST /api/quiz/attempts` accepts a participant name and one selected option per question. It returns the server-calculated score and saved response details.
- `GET /api/quiz/attempts/{id}` returns a persisted result.

## Verify

```powershell
dotnet test tests/backend/Question010.Api.Tests/Question010.Api.Tests.csproj --configuration Release
dotnet build backend/Question010.Api/Question010.Api.csproj --configuration Release
cd frontend
npm ci
npm test
npm run build
```

Results: backend **3 passed**, Angular **4 passed**, Release API build clean, and production Angular bundle **165.31 kB** initial (estimated transfer **48.65 kB**).

Package identity: `@example-com/interview-question-010`; namespace: `Example.Com.InterviewQuestion010`.
