using StudentStudyHub.Domain.Common;

namespace StudentStudyHub.Domain.Entities;

public class School : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<StudyGroup> StudyGroups { get; set; } = new List<StudyGroup>();
}
