export interface CourseDto {
  id: string;
  name: string;
  creditHours: number;
  gradeValue: string;
  isHonorsOrAp: boolean;
  term: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCourseRequest {
  name: string;
  creditHours: number;
  gradeValue: string;
  isHonorsOrAp: boolean;
  term: string;
}

export interface UpdateCourseRequest extends CreateCourseRequest {}
