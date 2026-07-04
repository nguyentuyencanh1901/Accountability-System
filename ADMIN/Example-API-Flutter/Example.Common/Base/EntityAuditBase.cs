using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Example.Common.Base.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Example.Common.Base
{
    public abstract class EntityAuditBase<T> : EntityBase<T>, IAuditable
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string? CreatedBy { get; set; }
        [MaxLength(150)]
        public string? LastModifiedBy { get; set; }
    }
}
