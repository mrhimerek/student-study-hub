export interface ProfileDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  gradeLevel: number;
  schoolName: string | null;
  avatarUrl: string | null;
  createdAt: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  gradeLevel: number;
  schoolName: string | null;
}
