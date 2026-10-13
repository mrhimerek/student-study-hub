type Priority = "Low" | "Medium" | "High";

const priorityClasses: Record<Priority, string> = {
  Low: "bg-emerald-100 text-emerald-700",
  Medium: "bg-amber-100 text-amber-700",
  High: "bg-rose-100 text-rose-700",
};

export function StatusChip({ priority, label }: { priority: Priority; label?: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${priorityClasses[priority]}`}
    >
      {label ?? priority}
    </span>
  );
}
