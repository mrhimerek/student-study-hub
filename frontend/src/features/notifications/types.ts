export type NotificationType =
  | "assignmentDue"
  | "examSoon"
  | "scholarshipDeadline"
  | "collegeDeadline"
  | "groupActivity";

export interface NotificationDto {
  id: string;
  type: NotificationType;
  title: string;
  body: string;
  relatedEntityId: string | null;
  isRead: boolean;
  sentViaEmail: boolean;
  createdAt: string;
}
