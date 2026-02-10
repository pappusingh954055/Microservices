using Company.Application.Common.Interfaces;
using Company.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Company.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly CompanyDbContext _context;
        public CompanyRepository(CompanyDbContext context) => _context = context;

        // --- CREATE ---
        public async Task<int> InsertCompanyAsync(CompanyProfile company)
        {
            _context.CompanyProfiles.Add(company);
            await _context.SaveChangesAsync();
            return company.Id;
        }

        // --- UPDATE ---
        public async Task<int> UpsertCompanyProfileAsync(CompanyProfile profile)
        {
            _context.CompanyProfiles.Update(profile);
            await _context.SaveChangesAsync();
            return profile.Id;
        }

        // --- READ: GET MASTER PROFILE (Optimized) ---
        public async Task<CompanyProfile?> GetCompanyProfileAsync()
        {
            // AsNoTracking fast execution ensure karta hai read-only data ke liye
            return await _context.CompanyProfiles
                .Include(c => c.CompanyAddress)   // Related Address fetch
                .Include(c => c.BankInformation)  // Related Bank info fetch
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // --- READ: GET BY ID (Optimized) ---
        public async Task<CompanyProfile?> GetByIdAsync(int id)
        {
            // ID based search with NoTracking for performance
            return await _context.CompanyProfiles
                .Include(c => c.CompanyAddress)
                .Include(c => c.BankInformation)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // --- DELETE ---
        public async Task<bool> DeleteCompanyProfileAsync(int id)
        {
            // Record search kar rahe hain delete karne se pehle
            var company = await _context.CompanyProfiles.FindAsync(id);

            if (company == null) return false;

            // Profile remove karenge, cascade delete baqi records handle kar lega
            _context.CompanyProfiles.Remove(company);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}