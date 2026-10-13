import { type InputHTMLAttributes, forwardRef } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, id, className = "", ...props },
  ref,
) {
  const inputId = id ?? props.name;

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={inputId} className="text-sm font-medium text-gray-700">
        {label}
      </label>
      <input
        id={inputId}
        ref={ref}
        className={`rounded-xl border px-3 py-2 text-sm outline-none transition-colors focus:ring-2 focus:ring-indigo-500 ${
          error ? "border-red-400" : "border-gray-300"
        } ${className}`}
        {...props}
      />
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  );
});
