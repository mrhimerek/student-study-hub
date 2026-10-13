import { apiClient } from "../../lib/apiClient";
import type {
  CreateGroupNoteRequest,
  CreateStudyGroupRequest,
  GroupNoteDto,
  JoinStudyGroupRequest,
  StudyGroupDetailDto,
  StudyGroupDto,
  UpdateGroupNoteRequest,
  UpdateStudyGroupRequest,
} from "./types";

export const studyGroupsApi = {
  listMine: async (): Promise<StudyGroupDto[]> => {
    const { data } = await apiClient.get<StudyGroupDto[]>("/study-groups");
    return data;
  },
  get: async (id: string): Promise<StudyGroupDetailDto> => {
    const { data } = await apiClient.get<StudyGroupDetailDto>(`/study-groups/${id}`);
    return data;
  },
  create: async (payload: CreateStudyGroupRequest): Promise<StudyGroupDto> => {
    const { data } = await apiClient.post<StudyGroupDto>("/study-groups", payload);
    return data;
  },
  update: async (id: string, payload: UpdateStudyGroupRequest): Promise<StudyGroupDto> => {
    const { data } = await apiClient.put<StudyGroupDto>(`/study-groups/${id}`, payload);
    return data;
  },
  remove: async (id: string): Promise<void> => {
    await apiClient.delete(`/study-groups/${id}`);
  },
  join: async (payload: JoinStudyGroupRequest): Promise<StudyGroupDto> => {
    const { data } = await apiClient.post<StudyGroupDto>("/study-groups/join", payload);
    return data;
  },
  leave: async (id: string): Promise<void> => {
    await apiClient.post(`/study-groups/${id}/leave`);
  },
};

export const groupNotesApi = {
  list: async (groupId: string): Promise<GroupNoteDto[]> => {
    const { data } = await apiClient.get<GroupNoteDto[]>(`/study-groups/${groupId}/notes`);
    return data;
  },
  create: async (groupId: string, payload: CreateGroupNoteRequest): Promise<GroupNoteDto> => {
    const { data } = await apiClient.post<GroupNoteDto>(`/study-groups/${groupId}/notes`, payload);
    return data;
  },
  update: async (groupId: string, noteId: string, payload: UpdateGroupNoteRequest): Promise<GroupNoteDto> => {
    const { data } = await apiClient.put<GroupNoteDto>(`/study-groups/${groupId}/notes/${noteId}`, payload);
    return data;
  },
  remove: async (groupId: string, noteId: string): Promise<void> => {
    await apiClient.delete(`/study-groups/${groupId}/notes/${noteId}`);
  },
};
