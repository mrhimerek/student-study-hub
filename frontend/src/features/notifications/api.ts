import { apiClient } from "../../lib/apiClient";
import type { NotificationDto } from "./types";

export const notificationsApi = {
  list: async (unreadOnly = false): Promise<NotificationDto[]> => {
    const { data } = await apiClient.get<NotificationDto[]>("/notifications", {
      params: unreadOnly ? { unread: "true" } : undefined,
    });
    return data;
  },
  getUnreadCount: async (): Promise<number> => {
    const { data } = await apiClient.get<{ count: number }>("/notifications/unread-count");
    return data.count;
  },
  markAsRead: async (id: string): Promise<void> => {
    await apiClient.post(`/notifications/${id}/read`);
  },
  markAllAsRead: async (): Promise<void> => {
    await apiClient.post("/notifications/read-all");
  },
};
