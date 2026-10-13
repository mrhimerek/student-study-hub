export type ApplicationType = "earlyDecision" | "earlyAction" | "regularDecision" | "rolling";

export type CollegeApplicationStatus = "notStarted" | "inProgress" | "submitted" | "accepted" | "rejected" | "waitlisted";

export interface CollegeApplicationDto {
  id: string;
  collegeName: string;
  applicationType: ApplicationType;
  deadline: string;
  status: CollegeApplicationStatus;
  requirements: string[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCollegeApplicationRequest {
  collegeName: string;
  applicationType: ApplicationType;
  deadline: string;
  status: CollegeApplicationStatus;
  requirements?: string[] | null;
}

export interface UpdateCollegeApplicationRequest extends CreateCollegeApplicationRequest {}
