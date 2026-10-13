import { isAxiosError } from "axios";
import type { ApiErrorBody } from "../features/auth/types";

export function extractApiErrorMessage(error: unknown, fallback: string): string {
  if (isAxiosError<ApiErrorBody>(error)) {
    const body = error.response?.data;
    if (body?.error) return body.error;
    if (body?.errors?.length) return body.errors.join(" ");
  }
  return fallback;
}
