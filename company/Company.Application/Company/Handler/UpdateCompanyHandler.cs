using Company.Application.Common.Interfaces;
using Company.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment ke liye

namespace Company.Application.Company.Commands.Update.Handler
{
    public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, int>
    {
        private readonly ICompanyRepository _repo;
        private readonly IWebHostEnvironment _environment; // wwwroot access ke liye

        public UpdateCompanyHandler(ICompanyRepository repo, IWebHostEnvironment environment)
        {
            _repo = repo; //
            _environment = environment;
        }

        public async Task<int> Handle(UpdateCompanyCommand cmd, CancellationToken ct)
        {
            // Pehle existing profile load karte hain with related data
            var profile = await _repo.GetCompanyProfileAsync();

            if (profile == null) return 0;

            // --- Logo Update Logic ---
            if (!string.IsNullOrEmpty(cmd.Request.LogoUrl) && cmd.Request.LogoUrl.Contains("base64"))
            {
                // 1. Purani file delete karein agar exist karti hai
                if (!string.IsNullOrEmpty(profile.LogoUrl))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, profile.LogoUrl.TrimStart('/'));
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }

                // 2. Nayi file save karein
                string folderPath = Path.Combine(_environment.WebRootPath, "uploads", "logos");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = $"logo_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(folderPath, fileName);

                var base64Data = cmd.Request.LogoUrl.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                await File.WriteAllBytesAsync(fullPath, imageBytes);

                profile.LogoUrl = $"/uploads/logos/{fileName}"; // Relative path update
            }

            // 1. Main Profile Fields Update
            profile.Name = cmd.Request.Name;
            profile.Tagline = cmd.Request.Tagline;
            profile.RegistrationNumber = cmd.Request.RegistrationNumber;
            profile.Gstin = cmd.Request.Gstin; // Max 15 chars
            profile.PrimaryEmail = cmd.Request.PrimaryEmail;
            profile.PrimaryPhone = cmd.Request.PrimaryPhone;
            profile.Website = cmd.Request.Website;

            // 2. Address Update
            if (profile.CompanyAddress != null)
            {
                profile.CompanyAddress.AddressLine1 = cmd.Request.Address.AddressLine1;
                profile.CompanyAddress.AddressLine2 = cmd.Request.Address.AddressLine2;
                profile.CompanyAddress.City = cmd.Request.Address.City;
                profile.CompanyAddress.State = cmd.Request.Address.State;
                profile.CompanyAddress.StateCode = cmd.Request.Address.StateCode; // Max 2 chars
                profile.CompanyAddress.PinCode = cmd.Request.Address.PinCode;
                profile.CompanyAddress.Country = cmd.Request.Address.Country;
            }

            // 3. Bank Information Update
            if (profile.BankInformation != null)
            {
                profile.BankInformation.BankName = cmd.Request.BankInfo.BankName;
                profile.BankInformation.BranchName = cmd.Request.BankInfo.BranchName;
                profile.BankInformation.AccountNumber = cmd.Request.BankInfo.AccountNumber;
                profile.BankInformation.IfscCode = cmd.Request.BankInfo.IfscCode;
                profile.BankInformation.AccountType = cmd.Request.BankInfo.AccountType;
            }

            // 4. Authorized Signatories Update
            if (cmd.Request.AuthorizedSignatories != null)
            {
                // Remove signatories not in the request
                var requestIds = cmd.Request.AuthorizedSignatories.Select(s => s.Id).ToList();
                var toRemove = profile.AuthorizedSignatories.Where(s => !requestIds.Contains(s.Id)).ToList();
                foreach (var s in toRemove) profile.AuthorizedSignatories.Remove(s);

                // Add or Update
                foreach (var sDto in cmd.Request.AuthorizedSignatories)
                {
                    var existing = profile.AuthorizedSignatories.FirstOrDefault(x => x.Id == sDto.Id && x.Id != 0);
                    if (existing != null)
                    {
                        existing.PersonName = sDto.PersonName;
                        existing.Designation = sDto.Designation;
                        existing.SignatureImageUrl = sDto.SignatureImageUrl;
                        existing.IsDefault = sDto.IsDefault;
                    }
                    else
                    {
                        profile.AuthorizedSignatories.Add(new AuthorizedSignatory
                        {
                            PersonName = sDto.PersonName,
                            Designation = sDto.Designation,
                            SignatureImageUrl = sDto.SignatureImageUrl,
                            IsDefault = sDto.IsDefault
                        });
                    }
                }
            }

            // Database mein changes save karte hain

            return await _repo.UpsertCompanyProfileAsync(profile);
        }
    }
}