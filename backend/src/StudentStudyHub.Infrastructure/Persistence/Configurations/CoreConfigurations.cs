using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentStudyHub.Domain.Entities;

namespace StudentStudyHub.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();

        builder.HasOne(u => u.School)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.SchoolId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class StudyGroupConfiguration : IEntityTypeConfiguration<StudyGroup>
{
    public void Configure(EntityTypeBuilder<StudyGroup> builder)
    {
        builder.HasIndex(g => g.InviteCode).IsUnique();
        builder.Property(g => g.InviteCode).HasMaxLength(8).IsRequired();
        builder.Property(g => g.Name).HasMaxLength(150).IsRequired();

        builder.HasOne(g => g.School)
            .WithMany(s => s.StudyGroups)
            .HasForeignKey(g => g.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.OwnerUser)
            .WithMany()
            .HasForeignKey(g => g.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StudyGroupMemberConfiguration : IEntityTypeConfiguration<StudyGroupMember>
{
    public void Configure(EntityTypeBuilder<StudyGroupMember> builder)
    {
        builder.HasKey(m => new { m.StudyGroupId, m.UserId });

        builder.HasOne(m => m.StudyGroup)
            .WithMany(g => g.Members)
            .HasForeignKey(m => m.StudyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany(u => u.StudyGroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.Property(a => a.Subject).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();

        // Composite index for the common "upcoming assignments for user" query.
        builder.HasIndex(a => new { a.UserId, a.DueDate });

        builder.HasOne(a => a.User)
            .WithMany(u => u.Assignments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
