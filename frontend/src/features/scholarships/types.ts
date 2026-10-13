export type ScholarshipStatus = "researching" | "applying" | "submitted" | "awarded" | "rejected";

export interface ScholarshipDto {
  id: string;
  name: string;
  amount: number | null;
  deadline: string;
  status: ScholarshipStatus;
  requirements: string[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateScholarshipRequest {
  name: string;
  amount?: number | null;
  deadline: string;
  status: ScholarshipStatus;
  requirements?: string[] | null;
}

export interface UpdateScholarshipRequest extends CreateScholarshipRequest {}
