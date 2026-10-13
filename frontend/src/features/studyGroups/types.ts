export type GroupMemberRole = "member" | "owner";

export interface StudyGroupDto {
  id: string;
  name: string;
  subject: string | null;
  inviteCode: string;
  schoolName: string;
  ownerUserId: string;
  isOwner: boolean;
  memberCount: number;
  createdAt: string;
}

export interface StudyGroupMemberDto {
  userId: string;
  firstName: string;
  lastName: string;
  role: GroupMemberRole;
  joinedAt: string;
}

export interface StudyGroupDetailDto {
  id: string;
  name: string;
  subject: string | null;
  inviteCode: string;
  schoolName: string;
  ownerUserId: string;
  isOwner: boolean;
  members: StudyGroupMemberDto[];
  createdAt: string;
}

export interface CreateStudyGroupRequest {
  name: string;
  subject?: string | null;
  schoolName: string;
}

export interface UpdateStudyGroupRequest {
  name: string;
  subject?: string | null;
}

export interface JoinStudyGroupRequest {
  inviteCode: string;
}

export interface GroupNoteDto {
  id: string;
  studyGroupId: string;
  authorUserId: string;
  authorName: string;
  isMine: boolean;
  title: string;
  content: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateGroupNoteRequest {
  title: string;
  content?: string | null;
}

export interface UpdateGroupNoteRequest {
  title: string;
  content?: string | null;
}
