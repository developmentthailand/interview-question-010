import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface QuizOption {
  key: string;
  text: string;
}

export interface QuizQuestion {
  id: number;
  position: number;
  prompt: string;
  options: QuizOption[];
}

export interface QuizAnswer {
  questionId: number;
  position: number;
  prompt: string;
  selectedOption: string;
  selectedText: string;
  isCorrect: boolean;
}

export interface QuizAttempt {
  id: number;
  participantName: string;
  score: number;
  totalQuestions: number;
  submittedAtUtc: string;
  answers: QuizAnswer[];
}

export interface SubmitQuizRequest {
  participantName: string;
  answers: { questionId: number; selectedOption: string }[];
}

@Injectable({ providedIn: 'root' })
export class QuizApiService {
  private readonly http = inject(HttpClient);

  questions(): Observable<QuizQuestion[]> {
    return this.http.get<QuizQuestion[]>('/api/quiz/questions');
  }

  submit(request: SubmitQuizRequest): Observable<QuizAttempt> {
    return this.http.post<QuizAttempt>('/api/quiz/attempts', request);
  }
}
