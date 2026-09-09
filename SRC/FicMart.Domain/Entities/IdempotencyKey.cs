using FicMart.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Domain.Entities
{
    public class IdempotencyKey
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public required string Idempotencykey { get; set; }
        public DateTime LastRunAt { get; set; } = DateTime.UtcNow;
        public DateTime LockedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(10)]
        public required string RequestMethod { get; set; }
        public required string RequestParameter { get; set; }
        [MaxLength(100)]
        public required string RequestPath { get; set; }
        public int? ResponseCode { get; set; }
        public string? ResponseBody { get; set; }
        [MaxLength(50)]
        public required RecoveryPoint RecoveryPoint { get; set; }
        public long UserId { get; set; }

    }
}
