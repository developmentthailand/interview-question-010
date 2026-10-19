import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { finalize } from 'rxjs';
import { QuizApiService, QuizAttempt, QuizQuestion } from './services/quiz-api.service';

type QuizPage = 'quiz' | 'result';

@Component({
  imports: [DecimalPipe],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly api = inject(QuizApiService);

  protected readonly page = signal<QuizPage>('quiz');
  protected readonly questions = signal<QuizQuestion[]>([]);
  protected readonly participantName = signal('');
  protected readonly selectedOptions = signal<Record<number, string>>({});
  protected readonly attempt = signal<QuizAttempt | null>(null);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.loadQuestions();
  }

  protected updateParticipantName(event: Event): void {
    this.participantName.set((event.target as HTMLInputElement).value);
    this.errorMessage.set(null);
  }

  protected selectAnswer(questionId: number, option: string): void {
    this.selectedOptions.update((answers) => ({ ...answers, [questionId]: option }));
    this.errorMessage.set(null);
  }

  protected selectedOption(questionId: number): string | undefined {
    return this.selectedOptions()[questionId];
  }

  protected canSubmit(): boolean {
    const name = this.participantName().trim();
    const answers = this.selectedOptions();
    return !this.loading() && !this.submitting() && name.length > 0 && this.questions().length > 0 &&
      this.questions().every((question) => Boolean(answers[question.id]));
  }

  protected submitQuiz(): void {
    if (!this.canSubmit()) {
      this.errorMessage.set('กรุณากรอกชื่อและเลือกคำตอบให้ครบทุกข้อ');
      return;
    }
    const request = {
      participantName: this.participantName().trim(),
      answers: this.questions().map((question) => ({
        questionId: question.id,
        selectedOption: this.selectedOptions()[question.id],
      })),
    };
    this.errorMessage.set(null);
    this.submitting.set(true);
    this.api
      .submit(request)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (attempt) => {
          this.attempt.set(attempt);
          this.page.set('result');
        },
        error: () => this.errorMessage.set('ส่งข้อสอบไม่สำเร็จ กรุณาตรวจคำตอบและลองอีกครั้ง'),
      });
  }

  protected retake(): void {
    this.participantName.set('');
    this.selectedOptions.set({});
    this.attempt.set(null);
    this.errorMessage.set(null);
    this.page.set('quiz');
  }

  private loadQuestions(): void {
    this.loading.set(true);
    this.api
      .questions()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (questions) => this.questions.set(questions),
        error: () => this.errorMessage.set('โหลดข้อสอบไม่สำเร็จ กรุณารีเฟรชหน้า'),
      });
  }
}
