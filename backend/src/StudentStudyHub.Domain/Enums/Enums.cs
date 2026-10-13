namespace StudentStudyHub.Domain.Enums;

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
}

public enum AssignmentStatus
{
    NotStarted = 0,
    InProgress = 1,
    Done = 2,
}

public enum GroupMemberRole
{
    Member = 0,
    Owner = 1,
}

public enum ExamType
{
    Quiz = 0,
    Exam = 1,
    Final = 2,
}

public enum ApplicationType
{
    EarlyDecision = 0,
    EarlyAction = 1,
    RegularDecision = 2,
    Rolling = 3,
}

public enum CollegeApplicationStatus
{
    NotStarted = 0,
    InProgress = 1,
    Submitted = 2,
    Accepted = 3,
    Rejected = 4,
    Waitlisted = 5,
}

public enum ScholarshipStatus
{
    Researching = 0,
    Applying = 1,
    Submitted = 2,
    Awarded = 3,
    Rejected = 4,
}

public enum NotificationType
{
    AssignmentDue = 0,
    ExamSoon = 1,
    ScholarshipDeadline = 2,
    CollegeDeadline = 3,
    GroupActivity = 4,
}
