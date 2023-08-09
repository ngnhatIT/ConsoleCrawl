using System;
using System.Collections.Generic;

namespace projects.Infrastructure.Entities
{
    public class Commic
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string LinkImage { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LengthChapter { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string Performance { get; set; } = string.Empty;
        public string Reads { get; set; } = string.Empty;
        public string Motips { get; set; } = string.Empty;
    }
}
