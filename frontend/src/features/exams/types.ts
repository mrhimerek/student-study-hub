export type ExamType = "quiz" | "exam" | "final";

export interface ExamDto {
  id: string;
  subject: string;
  title: string;
  examDate: string;
  type: ExamType;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateExamRequest {
  subject: string;
  title: string;
  examDate: string;
  type: ExamType;
  notes?: string | null;
}

export interface UpdateExamRequest extends CreateExamRequest {}
