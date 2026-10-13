import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { profileApi } from "./api";
import type { UpdateProfileRequest } from "./types";

const PROFILE_KEY = "profile";

export function useProfile() {
  return useQuery({
    queryKey: [PROFILE_KEY],
    queryFn: () => profileApi.get(),
  });
}

export function useUpdateProfile() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateProfileRequest) => profileApi.update(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [PROFILE_KEY] }),
  });
}
