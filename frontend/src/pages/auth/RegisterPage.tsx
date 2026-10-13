import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { Input } from "../../components/ui/Input";
import { useAuth } from "../../features/auth/AuthContext";
import {
  registerSchema,
  type RegisterFormInput,
  type RegisterFormValues,
} from "../../features/auth/schemas";
import { extractApiErrorMessage } from "../../lib/apiError";

export function RegisterPage() {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormInput, unknown, RegisterFormValues>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (values: RegisterFormValues) => {
    setServerError(null);
    try {
      await registerUser({
        email: values.email,
        password: values.password,
        firstName: values.firstName,
        lastName: values.lastName,
        gradeLevel: values.gradeLevel,
        schoolId: null,
      });
      navigate("/", { replace: true });
    } catch (err) {
      setServerError(extractApiErrorMessage(err, "Something went wrong. Please try again."));
    }
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 px-4 py-8">
      <div className="w-full max-w-sm">
        <h1 className="mb-1 text-center text-2xl font-bold text-indigo-600">Student Study Hub</h1>
        <p className="mb-6 text-center text-sm text-gray-500">Create your account to get started.</p>

        <Card>
          <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)} noValidate>
            <div className="grid grid-cols-2 gap-3">
              <Input label="First name" autoComplete="given-name" error={errors.firstName?.message} {...register("firstName")} />
              <Input label="Last name" autoComplete="family-name" error={errors.lastName?.message} {...register("lastName")} />
            </div>

            <Input label="Email" type="email" autoComplete="email" error={errors.email?.message} {...register("email")} />

            <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
              Grade level
              <select
                className={`rounded-xl border px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500 ${
                  errors.gradeLevel ? "border-red-400" : "border-gray-300"
                }`}
                defaultValue=""
                {...register("gradeLevel")}
              >
                <option value="" disabled>
                  Select grade
                </option>
                {[9, 10, 11, 12].map((grade) => (
                  <option key={grade} value={grade}>
                    Grade {grade}
                  </option>
                ))}
              </select>
              {errors.gradeLevel && <span className="text-xs text-red-500">{errors.gradeLevel.message}</span>}
            </label>

            <Input
              label="Password"
              type="password"
              autoComplete="new-password"
              error={errors.password?.message}
              {...register("password")}
            />
            <Input
              label="Confirm password"
              type="password"
              autoComplete="new-password"
              error={errors.confirmPassword?.message}
              {...register("confirmPassword")}
            />

            {serverError && <p className="text-sm text-red-500">{serverError}</p>}

            <Button type="submit" disabled={isSubmitting} className="w-full">
              {isSubmitting ? "Creating account…" : "Sign Up"}
            </Button>
          </form>
        </Card>

        <p className="mt-4 text-center text-sm text-gray-500">
          Already have an account?{" "}
          <Link to="/login" className="font-medium text-indigo-600 hover:underline">
            Log in
          </Link>
        </p>
      </div>
    </div>
  );
}
