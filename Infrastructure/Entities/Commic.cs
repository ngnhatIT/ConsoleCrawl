using System;

namespace projects.Infrastructure.Entities
{
    public class Commic : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }
}
