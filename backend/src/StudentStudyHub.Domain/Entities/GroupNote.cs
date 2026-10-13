using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class GroupNote : BaseEntity
{
    public Guid StudyGroupId { get; set; }
    public StudyGroup? StudyGroup { get; set; }

    public Guid AuthorUserId { get; set; }
    public User? AuthorUser { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileBlobUrl { get; set; }
}
