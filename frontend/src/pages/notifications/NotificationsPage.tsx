import { Link } from "react-router-dom";
import { Card } from "../../components/ui/Card";
import { Button } from "../../components/ui/Button";
import {
  useMarkAllNotificationsAsRead,
  useMarkNotificationAsRead,
  useNotifications,
} from "../../features/notifications/hooks";
import type { NotificationDto, NotificationType } from "../../features/notifications/types";

const TYPE_ICON: Record<NotificationType, string> = {
  assignmentDue: "📋",
  examSoon: "🟣",
  scholarshipDeadline: "🟡",
  collegeDeadline: "🟢",
  groupActivity: "👥",
};

const TYPE_LINK: Record<NotificationType, string> = {
  assignmentDue: "/assignments",
  examSoon: "/exams",
  scholarshipDeadline: "/scholarships",
  collegeDeadline: "/college-applications",
  groupActivity: "/study-groups",
};

function formatTimestamp(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

function NotificationRow({ notification }: { notification: NotificationDto }) {
  const markAsRead = useMarkNotificationAsRead();

  return (
    <Card className={`flex items-start gap-3 ${notification.isRead ? "opacity-60" : ""}`}>
      <span className="text-lg">{TYPE_ICON[notification.type]}</span>
      <div className="flex-1">
        <p className="text-sm font-medium text-gray-800">{notification.title}</p>
        <p className="text-sm text-gray-600">{notification.body}</p>
        <div className="mt-1 flex items-center gap-3 text-xs text-gray-400">
          <span>{formatTimestamp(notification.createdAt)}</span>
          <Link to={TYPE_LINK[notification.type]} className="font-medium text-indigo-500 hover:underline">
            View →
          </Link>
        </div>
      </div>
      {!notification.isRead && (
        <button
          type="button"
          onClick={() => markAsRead.mutate(notification.id)}
          className="text-xs font-medium text-indigo-500 hover:underline"
        >
          Mark read
        </button>
      )}
    </Card>
  );
}

export function NotificationsPage() {
  const { data: notifications, isLoading, isError } = useNotifications();
  const markAllAsRead = useMarkAllNotificationsAsRead();

  const hasUnread = (notifications ?? []).some((n) => !n.isRead);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Notifications</h1>
        {hasUnread && (
          <Button variant="secondary" onClick={() => markAllAsRead.mutate()} disabled={markAllAsRead.isPending}>
            Mark all read
          </Button>
        )}
      </div>

      {isLoading && <p className="text-sm text-gray-400">Loading notifications…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load notifications. Please try again.</p>}

      {!isLoading && notifications?.length === 0 && (
        <p className="text-sm text-gray-400">No notifications yet. We&apos;ll remind you about upcoming deadlines here.</p>
      )}

      <div className="space-y-2">
        {notifications?.map((notification) => (
          <NotificationRow key={notification.id} notification={notification} />
        ))}
      </div>
    </div>
  );
}
