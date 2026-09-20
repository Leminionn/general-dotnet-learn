using FirstAPIProject.Domain.Common.Enums;
using System;

namespace FirstAPIProject.Domain.Entities
{
    public class Announcement : LifecycleEntity
    {
        public string Title { get; private set; } = null!;
        public string Content { get; private set; } = null!;
        public AnnouncementStatus Status { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public Guid AuthorId { get; private set; }

        public User Author { get; private set; } = null!;

        private Announcement()
        {
        }

        private Announcement(string title, string content, Guid authorId, AnnouncementStatus status = AnnouncementStatus.Draft)
        {
            Title = title;
            Content = content;
            AuthorId = authorId;
            Status = status;

            if (status == AnnouncementStatus.Published)
            {
                PublishedAt = DateTime.UtcNow;
            }
        }

        public static Announcement Create(string title, string content, Guid authorId, AnnouncementStatus status = AnnouncementStatus.Draft)
        {
            return new Announcement(title, content, authorId, status);
        }

        public void Update(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public void Publish()
        {
            Status = AnnouncementStatus.Published;
            PublishedAt = DateTime.UtcNow;
            Activate();
        }

        public void Archive()
        {
            Status = AnnouncementStatus.Archived;
        }
    }
}
