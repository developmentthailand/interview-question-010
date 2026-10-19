import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { App } from './app';
import { appConfig } from './app.config';

const questions = [
  { id: 1, position: 1, prompt: 'ข้อใดต่างจากข้ออื่น', options: [{ key: 'A', text: '3' }, { key: 'B', text: '5' }, { key: 'C', text: '9' }, { key: 'D', text: '11' }] },
  { id: 2, position: 2, prompt: 'X + 2 = 4 จงหาค่า X', options: [{ key: 'A', text: '1' }, { key: 'B', text: '2' }, { key: 'C', text: '3' }, { key: 'D', text: '4' }] },
];

describe('Quiz attempt and result', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [App], providers: [...(appConfig.providers ?? []), provideHttpClientTesting()] }).compileComponents();
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('loads practice questions without exposing which answer is correct', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/quiz/questions').flush(questions);
    fixture.detectChanges();

    expect(root.querySelectorAll('[data-question-id]')).toHaveLength(2);
    expect(root.textContent).toContain('ข้อใดต่างจากข้ออื่น');
    expect(root.querySelectorAll('input[type="radio"]')).toHaveLength(8);
  });

  it('submits one selected option per question and displays the stored score', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/quiz/questions').flush(questions);
    fixture.detectChanges();
    setInput(root, 'participantName', 'Narin');
    choose(root, 1, 'C');
    choose(root, 2, 'B');
    fixture.detectChanges();
    expect(root.querySelector<HTMLButtonElement>('.submit-button')?.disabled).toBe(false);
    clickButton(root, 'ส่งข้อสอบ');

    const request = http.expectOne('/api/quiz/attempts');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      participantName: 'Narin',
      answers: [{ questionId: 1, selectedOption: 'C' }, { questionId: 2, selectedOption: 'B' }],
    });
    request.flush({
      id: 42,
      participantName: 'Narin',
      score: 2,
      totalQuestions: 2,
      submittedAtUtc: '2026-10-20T00:00:00Z',
      answers: [
        { questionId: 1, position: 1, prompt: 'ข้อใดต่างจากข้ออื่น', selectedOption: 'C', selectedText: '9', isCorrect: true },
        { questionId: 2, position: 2, prompt: 'X + 2 = 4 จงหาค่า X', selectedOption: 'B', selectedText: '2', isCorrect: true },
      ],
    });
    fixture.detectChanges();

    expect(root.textContent).toContain('2 / 2');
    expect(root.textContent).toContain('Narin');
    expect(root.querySelector('[data-answer-question-id="1"]')?.textContent).toContain('9');
    expect(root.querySelector('[data-answer-question-id="2"]')?.textContent).toContain('2');
  });

  it('retake clears the participant name and every selected answer', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/quiz/questions').flush(questions);
    fixture.detectChanges();
    setInput(root, 'participantName', 'Narin');
    choose(root, 1, 'C');
    choose(root, 2, 'B');
    fixture.detectChanges();
    clickButton(root, 'ส่งข้อสอบ');
    http.expectOne('/api/quiz/attempts').flush({
      id: 42, participantName: 'Narin', score: 2, totalQuestions: 2, submittedAtUtc: '2026-10-20T00:00:00Z',
      answers: [
        { questionId: 1, position: 1, prompt: questions[0].prompt, selectedOption: 'C', selectedText: '9', isCorrect: true },
        { questionId: 2, position: 2, prompt: questions[1].prompt, selectedOption: 'B', selectedText: '2', isCorrect: true },
      ],
    });
    fixture.detectChanges();
    clickButton(root, 'สอบอีกครั้ง');
    fixture.detectChanges();

    expect(root.querySelector<HTMLInputElement>('#participantName')?.value).toBe('');
    expect(root.querySelector<HTMLInputElement>('input[name="question-1"]:checked')).toBeNull();
    expect(root.querySelector<HTMLInputElement>('input[name="question-2"]:checked')).toBeNull();
  });

  it('keeps submission disabled until name and every answer are supplied', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/quiz/questions').flush(questions);
    fixture.detectChanges();
    setInput(root, 'participantName', 'Narin');
    choose(root, 1, 'C');
    fixture.detectChanges();

    expect(root.querySelector<HTMLButtonElement>('.submit-button')?.disabled).toBe(true);
    expect(http.match('/api/quiz/attempts')).toHaveLength(0);
  });
});

function createPage(): { fixture: ReturnType<typeof TestBed.createComponent<App>>; root: HTMLElement; http: HttpTestingController } {
  const fixture = TestBed.createComponent(App);
  fixture.detectChanges();
  return { fixture, root: fixture.nativeElement as HTMLElement, http: TestBed.inject(HttpTestingController) };
}

function setInput(root: HTMLElement, id: string, value: string): void {
  const input = root.querySelector<HTMLInputElement>(`#${id}`);
  if (!input) throw new Error(`Input #${id} was not rendered.`);
  input.value = value;
  input.dispatchEvent(new Event('input', { bubbles: true }));
}

function choose(root: HTMLElement, questionId: number, option: string): void {
  const input = root.querySelector<HTMLInputElement>(`input[name="question-${questionId}"][value="${option}"]`);
  if (!input) throw new Error(`Option ${option} for question ${questionId} was not rendered.`);
  input.click();
}

function clickButton(root: HTMLElement, label: string): void {
  const button = Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find((candidate) => candidate.textContent?.includes(label));
  if (!button) throw new Error(`Button "${label}" was not rendered.`);
  button.click();
}
