import { useQuery } from "@tanstack/react-query";
import { gpaApi } from "./api";

export function useGpaSummary() {
  return useQuery({
    queryKey: ["gpa"],
    queryFn: () => gpaApi.getSummary(),
  });
}
