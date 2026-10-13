import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { notificationsApi } from "./api";

const NOTIFICATIONS_KEY = "notifications";
const UNREAD_COUNT_KEY = "notifications-unread-count";

export function useNotifications(unreadOnly = false) {
  return useQuery({
    queryKey: [NOTIFICATIONS_KEY, unreadOnly],
    queryFn: () => notificationsApi.list(unreadOnly),
  });
}

export function useUnreadNotificationCount() {
  return useQuery({
    queryKey: [UNREAD_COUNT_KEY],
    queryFn: () => notificationsApi.getUnreadCount(),
    refetchInterval: 60_000,
  });
}

function invalidateNotifications(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: [NOTIFICATIONS_KEY] });
  queryClient.invalidateQueries({ queryKey: [UNREAD_COUNT_KEY] });
}

export function useMarkNotificationAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => invalidateNotifications(queryClient),
  });
}

export function useMarkAllNotificationsAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => invalidateNotifications(queryClient),
  });
}
