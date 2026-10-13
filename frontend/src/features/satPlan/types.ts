export interface SatPracticeScoreDto {
  id: string;
  testDate: string;
  readingWritingScore: number;
  mathScore: number;
  totalScore: number;
  createdAt: string;
}

export interface SatPlanDto {
  id: string;
  targetTestDate: string;
  goalScore: number;
  practiceScores: SatPracticeScoreDto[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateSatPlanRequest {
  targetTestDate: string;
  goalScore: number;
}

export interface UpdateSatPlanRequest extends CreateSatPlanRequest {}

export interface LogSatPracticeScoreRequest {
  testDate: string;
  readingWritingScore: number;
  mathScore: number;
}
