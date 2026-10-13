export function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="flex h-64 flex-col items-center justify-center text-center text-gray-400">
      <p className="text-lg font-medium">{title}</p>
      <p className="text-sm">Coming soon — see DESIGN.md for the full spec.</p>
    </div>
  );
}
