using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTime CreatedOn { get; protected set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }= DateTime.UtcNow;

        public DateTime? ModifiedOn { get; set; }

        public string? ModifiedBy { get; set; }

    }    
}
