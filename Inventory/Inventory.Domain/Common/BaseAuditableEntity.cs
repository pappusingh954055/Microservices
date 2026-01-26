using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Common
{
    public abstract class BaseAuditableEntity
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }= DateTime.UtcNow;

        //public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

    }    
}
