using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace CloudCopy.Server.Entities
{
    [Index(nameof(DayCreated))]
    public class CopiedEntity
    {
        [Required]
        public long CopiedEntityId { get; set; }

        [Required]
        public string IpAddress { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public string MimeType { get; set; }

        [Required]
        public int DayCreated { get; set; }

        [DefaultValue(typeof(DateTime), "")]        
        public DateTime Updated { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Inserted { get; set; } = DateTime.UtcNow;
    }
}
