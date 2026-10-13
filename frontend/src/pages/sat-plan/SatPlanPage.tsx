import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { CountdownBadge } from "../../components/shared/CountdownBadge";
import {
  useCreateSatPlan,
  useDeleteSatPlan,
  useDeleteSatPracticeScore,
  useLogSatPracticeScore,
  useSatPlan,
  useUpdateSatPlan,
} from "../../features/satPlan/hooks";
import type { SatPlanFormValues } from "../../features/satPlan/schemas";
import { SatPlanForm } from "./SatPlanForm";
import { SatPracticeScoreForm } from "./SatPracticeScoreForm";

function daysUntil(isoDate: string): number {
  const msPerDay = 24 * 60 * 60 * 1000;
  const diff = new Date(`${isoDate}T00:00:00`).getTime() - Date.now();
  return Math.max(0, Math.ceil(diff / msPerDay));
}

export function SatPlanPage() {
  const [isEditingPlan, setIsEditingPlan] = useState(false);
  const [isLoggingScore, setIsLoggingScore] = useState(false);

  const { data: plan, isLoading, isError } = useSatPlan();
  const createMutation = useCreateSatPlan();
  const updateMutation = useUpdateSatPlan();
  const deleteMutation = useDeleteSatPlan();
  const logScoreMutation = useLogSatPracticeScore();
  const deleteScoreMutation = useDeleteSatPracticeScore();

  const handleCreateOrUpdate = async (values: SatPlanFormValues) => {
    const payload = { targetTestDate: values.targetTestDate, goalScore: Number(values.goalScore) };
    if (plan) {
      await updateMutation.mutateAsync(payload);
    } else {
      await createMutation.mutateAsync(payload);
    }
    setIsEditingPlan(false);
  };

  const handleDeletePlan = () => {
    if (window.confirm("Delete your SAT plan and all logged practice scores?")) {
      deleteMutation.mutate();
    }
  };

  const handleLogScore = async (values: { testDate: string; readingWritingScore: string; mathScore: string }) => {
    await logScoreMutation.mutateAsync({
      testDate: values.testDate,
      readingWritingScore: Number(values.readingWritingScore),
      mathScore: Number(values.mathScore),
    });
    setIsLoggingScore(false);
  };

  const handleDeleteScore = (scoreId: string) => {
    if (window.confirm("Delete this practice score?")) {
      deleteScoreMutation.mutate(scoreId);
    }
  };

  if (isLoading) {
    return <p className="text-sm text-gray-400">Loading your SAT plan…</p>;
  }

  if (isError) {
    return <p className="text-sm text-red-500">Couldn&apos;t load your SAT plan. Please try again.</p>;
  }

  if (!plan || isEditingPlan) {
    return (
      <div className="space-y-4">
        <h1 className="text-lg font-semibold text-gray-800">SAT Study Planner</h1>
        <Card>
          <p className="mb-3 text-sm text-gray-500">
            {plan ? "Update your target date and goal score." : "Set a target test date and goal score to get started."}
          </p>
          <SatPlanForm
            initialValues={plan ? { targetTestDate: plan.targetTestDate.slice(0, 10), goalScore: String(plan.goalScore) } : undefined}
            submitLabel={plan ? "Save changes" : "Create plan"}
            onSubmit={handleCreateOrUpdate}
            onCancel={plan ? () => setIsEditingPlan(false) : undefined}
          />
        </Card>
      </div>
    );
  }

  const latestScore = plan.practiceScores[plan.practiceScores.length - 1];
  const bestScore = plan.practiceScores.reduce<number | null>(
    (best, s) => (best === null || s.totalScore > best ? s.totalScore : best),
    null,
  );

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">SAT Study Planner</h1>
        <Button variant="ghost" className="px-2 py-1 text-xs" onClick={() => setIsEditingPlan(true)}>
          Edit plan
        </Button>
      </div>

      <Card className="flex flex-col gap-2">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-xs text-gray-500">Target test date</p>
            <p className="text-sm font-medium">
              {new Date(`${plan.targetTestDate}T00:00:00`).toLocaleDateString(undefined, {
                month: "short",
                day: "numeric",
                year: "numeric",
              })}
            </p>
          </div>
          <CountdownBadge days={daysUntil(plan.targetTestDate)} />
        </div>
        <div className="grid grid-cols-3 gap-2 text-center">
          <div>
            <p className="text-xs text-gray-500">Goal</p>
            <p className="text-lg font-semibold text-indigo-600">{plan.goalScore}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">Latest</p>
            <p className="text-lg font-semibold text-gray-800">{latestScore ? latestScore.totalScore : "—"}</p>
          </div>
          <div>
            <p className="text-xs text-gray-500">Best</p>
            <p className="text-lg font-semibold text-emerald-600">{bestScore ?? "—"}</p>
          </div>
        </div>
        <Button variant="ghost" className="self-start px-2 py-1 text-xs text-rose-600 hover:bg-rose-50" onClick={handleDeletePlan}>
          Delete plan
        </Button>
      </Card>

      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-gray-700">Practice scores</h2>
        <Button
          className="px-2 py-1 text-xs"
          onClick={() => setIsLoggingScore((prev) => !prev)}
        >
          {isLoggingScore ? "Close" : "+ Log score"}
        </Button>
      </div>

      {isLoggingScore && (
        <Card>
          <SatPracticeScoreForm onSubmit={handleLogScore} onCancel={() => setIsLoggingScore(false)} />
        </Card>
      )}

      {plan.practiceScores.length === 0 && (
        <p className="text-sm text-gray-400">No practice scores logged yet.</p>
      )}

      <div className="space-y-2">
        {[...plan.practiceScores].reverse().map((score) => (
          <Card key={score.id} className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium">
                {new Date(`${score.testDate}T00:00:00`).toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" })}
              </p>
              <p className="text-xs text-gray-400">
                R&W {score.readingWritingScore} + Math {score.mathScore}
              </p>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-lg font-semibold text-gray-800">{score.totalScore}</span>
              <Button
                variant="ghost"
                className="px-2 py-1 text-xs text-rose-600 hover:bg-rose-50"
                onClick={() => handleDeleteScore(score.id)}
              >
                Delete
              </Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
