import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { Input } from "../../components/ui/Input";
import { useAuth } from "../../features/auth/AuthContext";
import { loginSchema, type LoginFormValues } from "../../features/auth/schemas";
import { extractApiErrorMessage } from "../../lib/apiError";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      await login(values);
      navigate("/", { replace: true });
    } catch (err) {
      setServerError(extractApiErrorMessage(err, "Invalid email or password."));
    }
  };

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-sm">
        <h1 className="mb-1 text-center text-2xl font-bold text-indigo-600">Student Study Hub</h1>
        <p className="mb-6 text-center text-sm text-gray-500">Welcome back! Log in to continue.</p>

        <Card>
          <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)} noValidate>
            <Input
              label="Email"
              type="email"
              autoComplete="email"
              error={errors.email?.message}
              {...register("email")}
            />
            <Input
              label="Password"
              type="password"
              autoComplete="current-password"
              error={errors.password?.message}
              {...register("password")}
            />

            {serverError && <p className="text-sm text-red-500">{serverError}</p>}

            <Button type="submit" disabled={isSubmitting} className="w-full">
              {isSubmitting ? "Logging in…" : "Log In"}
            </Button>
          </form>
        </Card>

        <p className="mt-4 text-center text-sm text-gray-500">
          Don&apos;t have an account?{" "}
          <Link to="/register" className="font-medium text-indigo-600 hover:underline">
            Sign up
          </Link>
        </p>
      </div>
    </div>
  );
}
