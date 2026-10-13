export type AssignmentPriority = "low" | "medium" | "high";
export type AssignmentStatus = "notStarted" | "inProgress" | "done";

export interface AssignmentDto {
  id: string;
  subject: string;
  title: string;
  description: string | null;
  dueDate: string;
  priority: AssignmentPriority;
  status: AssignmentStatus;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateAssignmentRequest {
  subject: string;
  title: string;
  description?: string | null;
  dueDate: string;
  priority: AssignmentPriority;
}

export interface UpdateAssignmentRequest extends CreateAssignmentRequest {
  status: AssignmentStatus;
}
