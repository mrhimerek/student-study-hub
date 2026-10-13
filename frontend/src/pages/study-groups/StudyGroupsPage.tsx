import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { useCreateStudyGroup, useJoinStudyGroup, useStudyGroups } from "../../features/studyGroups/hooks";
import { CreateStudyGroupForm } from "./CreateStudyGroupForm";
import { JoinStudyGroupForm } from "./JoinStudyGroupForm";
import { StudyGroupCard } from "./StudyGroupCard";

export function StudyGroupsPage() {
  const [mode, setMode] = useState<"none" | "create" | "join">("none");

  const { data: groups, isLoading, isError } = useStudyGroups();
  const createMutation = useCreateStudyGroup();
  const joinMutation = useJoinStudyGroup();

  const toggleMode = (next: "create" | "join") => {
    setMode((prev) => (prev === next ? "none" : next));
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Study Groups</h1>
        <div className="flex gap-2">
          <Button variant="secondary" className="px-3 py-1.5 text-xs" onClick={() => toggleMode("join")}>
            {mode === "join" ? "Close" : "Join"}
          </Button>
          <Button className="px-3 py-1.5 text-xs" onClick={() => toggleMode("create")}>
            {mode === "create" ? "Close" : "+ Create"}
          </Button>
        </div>
      </div>

      {mode === "create" && (
        <Card>
          <CreateStudyGroupForm
            onSubmit={async (values) => {
              await createMutation.mutateAsync({
                name: values.name,
                subject: values.subject || null,
                schoolName: values.schoolName,
              });
              setMode("none");
            }}
            onCancel={() => setMode("none")}
          />
        </Card>
      )}

      {mode === "join" && (
        <Card>
          <JoinStudyGroupForm
            onSubmit={async (values) => {
              await joinMutation.mutateAsync(values);
              setMode("none");
            }}
            onCancel={() => setMode("none")}
          />
        </Card>
      )}

      {isLoading && <p className="text-sm text-gray-400">Loading study groups…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load study groups. Please try again.</p>}

      {!isLoading && groups?.length === 0 && (
        <p className="text-sm text-gray-400">No study groups yet. Create one or join with an invite code.</p>
      )}

      <div className="space-y-2">
        {groups?.map((group) => (
          <StudyGroupCard key={group.id} group={group} />
        ))}
      </div>
    </div>
  );
}
