using Company.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Common.Interfaces
{
    public interface  ICompanyDbContext
    {
        public DbSet<CompanyProfile> CompanyProfiles { get; }
        public DbSet<Address> Addresses { get; }
        public DbSet<BankDetail> BankDetails { get;}
        public DbSet<AuthorizedSignatory> AuthorizedSignatories { get;}
    }
}
