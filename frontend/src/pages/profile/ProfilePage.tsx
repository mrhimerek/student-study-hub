import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { Input } from "../../components/ui/Input";
import { useAuth } from "../../features/auth/AuthContext";
import { useProfile, useUpdateProfile } from "../../features/profile/hooks";
import { profileSchema, type ProfileFormInput, type ProfileFormValues } from "../../features/profile/schemas";
import { extractApiErrorMessage } from "../../lib/apiError";

export function ProfilePage() {
  const { updateUser, logout } = useAuth();
  const { data: profile, isLoading, isError } = useProfile();
  const updateMutation = useUpdateProfile();

  const [isEditing, setIsEditing] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ProfileFormInput, unknown, ProfileFormValues>({
    resolver: zodResolver(profileSchema),
  });

  useEffect(() => {
    if (profile) {
      reset({
        firstName: profile.firstName,
        lastName: profile.lastName,
        gradeLevel: profile.gradeLevel,
        schoolName: profile.schoolName ?? "",
      });
    }
  }, [profile, reset]);

  const onSubmit = async (values: ProfileFormValues) => {
    setServerError(null);
    try {
      const updated = await updateMutation.mutateAsync(values);
      updateUser({ firstName: updated.firstName, lastName: updated.lastName, gradeLevel: updated.gradeLevel });
      setIsEditing(false);
    } catch (err) {
      setServerError(extractApiErrorMessage(err, "Something went wrong saving your profile. Please try again."));
    }
  };

  if (isLoading) {
    return <p className="text-sm text-gray-400">Loading your profile…</p>;
  }

  if (isError || !profile) {
    return <p className="text-sm text-red-500">Couldn&apos;t load your profile. Please try again.</p>;
  }

  if (isEditing) {
    return (
      <div className="space-y-4">
        <h1 className="text-lg font-semibold text-gray-800">Edit Profile</h1>
        <Card>
          <form className="flex flex-col gap-3" onSubmit={handleSubmit(onSubmit)} noValidate>
            <div className="grid grid-cols-2 gap-3">
              <Input label="First name" error={errors.firstName?.message} {...register("firstName")} />
              <Input label="Last name" error={errors.lastName?.message} {...register("lastName")} />
            </div>

            <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
              Grade level
              <select
                className={`rounded-xl border px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500 ${
                  errors.gradeLevel ? "border-red-400" : "border-gray-300"
                }`}
                {...register("gradeLevel")}
              >
                {[9, 10, 11, 12].map((grade) => (
                  <option key={grade} value={grade}>
                    Grade {grade}
                  </option>
                ))}
              </select>
              {errors.gradeLevel && <span className="text-xs text-red-500">{errors.gradeLevel.message}</span>}
            </label>

            <Input
              label="School (optional)"
              placeholder="e.g. Lincoln High School"
              error={errors.schoolName?.message}
              {...register("schoolName")}
            />

            {serverError && <p className="text-sm text-red-500">{serverError}</p>}

            <div className="flex gap-2">
              <Button type="submit" disabled={isSubmitting} className="flex-1">
                {isSubmitting ? "Saving…" : "Save changes"}
              </Button>
              <Button type="button" variant="secondary" onClick={() => setIsEditing(false)} disabled={isSubmitting}>
                Cancel
              </Button>
            </div>
          </form>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Profile</h1>
        <Button variant="ghost" className="px-2 py-1 text-xs" onClick={() => setIsEditing(true)}>
          Edit
        </Button>
      </div>

      <Card className="flex flex-col gap-3">
        <div className="flex items-center gap-3">
          <div className="flex h-14 w-14 items-center justify-center rounded-full bg-indigo-100 text-xl font-semibold text-indigo-600">
            {profile.firstName.charAt(0).toUpperCase()}
            {profile.lastName.charAt(0).toUpperCase()}
          </div>
          <div>
            <p className="text-base font-semibold text-gray-800">
              {profile.firstName} {profile.lastName}
            </p>
            <p className="text-xs text-gray-400">{profile.email}</p>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3 text-sm">
          <div>
            <p className="text-xs text-gray-500">Grade level</p>
            <p className="font-medium text-gray-800">Grade {profile.gradeLevel}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">School</p>
            <p className="font-medium text-gray-800">{profile.schoolName ?? "Not set"}</p>
          </div>
        </div>
      </Card>

      <Button
        variant="ghost"
        className="w-full text-rose-600 hover:bg-rose-50"
        onClick={() => void logout()}
      >
        Log out
      </Button>
    </div>
  );
}
