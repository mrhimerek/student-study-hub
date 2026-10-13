import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { groupNotesApi, studyGroupsApi } from "./api";
import type {
  CreateGroupNoteRequest,
  CreateStudyGroupRequest,
  JoinStudyGroupRequest,
  UpdateGroupNoteRequest,
  UpdateStudyGroupRequest,
} from "./types";

const STUDY_GROUPS_KEY = "study-groups";
const STUDY_GROUP_KEY = "study-group";
const GROUP_NOTES_KEY = "group-notes";

export function useStudyGroups() {
  return useQuery({
    queryKey: [STUDY_GROUPS_KEY],
    queryFn: () => studyGroupsApi.listMine(),
  });
}

export function useStudyGroup(id: string | undefined) {
  return useQuery({
    queryKey: [STUDY_GROUP_KEY, id],
    queryFn: () => studyGroupsApi.get(id as string),
    enabled: !!id,
  });
}

function invalidateStudyGroups(queryClient: ReturnType<typeof useQueryClient>, id?: string) {
  queryClient.invalidateQueries({ queryKey: [STUDY_GROUPS_KEY] });
  if (id) {
    queryClient.invalidateQueries({ queryKey: [STUDY_GROUP_KEY, id] });
  }
}

export function useCreateStudyGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateStudyGroupRequest) => studyGroupsApi.create(payload),
    onSuccess: () => invalidateStudyGroups(queryClient),
  });
}

export function useUpdateStudyGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateStudyGroupRequest }) => studyGroupsApi.update(id, payload),
    onSuccess: (_data, variables) => invalidateStudyGroups(queryClient, variables.id),
  });
}

export function useDeleteStudyGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => studyGroupsApi.remove(id),
    onSuccess: () => invalidateStudyGroups(queryClient),
  });
}

export function useJoinStudyGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: JoinStudyGroupRequest) => studyGroupsApi.join(payload),
    onSuccess: () => invalidateStudyGroups(queryClient),
  });
}

export function useLeaveStudyGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => studyGroupsApi.leave(id),
    onSuccess: () => invalidateStudyGroups(queryClient),
  });
}

export function useGroupNotes(groupId: string | undefined) {
  return useQuery({
    queryKey: [GROUP_NOTES_KEY, groupId],
    queryFn: () => groupNotesApi.list(groupId as string),
    enabled: !!groupId,
  });
}

function invalidateGroupNotes(queryClient: ReturnType<typeof useQueryClient>, groupId: string) {
  queryClient.invalidateQueries({ queryKey: [GROUP_NOTES_KEY, groupId] });
}

export function useCreateGroupNote(groupId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateGroupNoteRequest) => groupNotesApi.create(groupId, payload),
    onSuccess: () => invalidateGroupNotes(queryClient, groupId),
  });
}

export function useUpdateGroupNote(groupId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ noteId, payload }: { noteId: string; payload: UpdateGroupNoteRequest }) =>
      groupNotesApi.update(groupId, noteId, payload),
    onSuccess: () => invalidateGroupNotes(queryClient, groupId),
  });
}

export function useDeleteGroupNote(groupId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (noteId: string) => groupNotesApi.remove(groupId, noteId),
    onSuccess: () => invalidateGroupNotes(queryClient, groupId),
  });
}
