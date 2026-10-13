type Priority = "Low" | "Medium" | "High" | "low" | "medium" | "high";

const priorityClasses: Record<string, string> = {
  low: "bg-emerald-100 text-emerald-700",
  medium: "bg-amber-100 text-amber-700",
  high: "bg-rose-100 text-rose-700",
};

// Accepts either casing so it works for both the backend's camelCase enum strings
// (e.g. "high") and any PascalCase mock/demo data still floating around the UI.
export function StatusChip({ priority, label }: { priority: Priority; label?: string }) {
  const key = priority.toLowerCase();
  const displayLabel = label ?? priority.charAt(0).toUpperCase() + priority.slice(1);

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${priorityClasses[key]}`}
    >
      {displayLabel}
    </span>
  );
}
