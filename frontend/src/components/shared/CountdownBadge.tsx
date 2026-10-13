export function CountdownBadge({ days }: { days: number }) {
  const urgent = days <= 2;
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-semibold ${
        urgent ? "bg-rose-100 text-rose-700" : "bg-indigo-100 text-indigo-700"
      }`}
    >
      ⏳ {days === 0 ? "Today" : `${days}d`}
    </span>
  );
}
