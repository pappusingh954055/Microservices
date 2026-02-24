using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Common
{
    public abstract class BaseAuditableEntity
    {
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? ModifiedBy { get; set; }
    }    
}
