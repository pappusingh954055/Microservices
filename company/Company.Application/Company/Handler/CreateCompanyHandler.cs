using Company.Application.Common.Interfaces;
using Company.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Hosting;

namespace Company.Application.Company.Commands.Create.Handler
{
    public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, int>
    {
        private readonly ICompanyRepository _repo;
        private readonly IWebHostEnvironment _environment; // Path handle karne ke liye

        public CreateCompanyHandler(ICompanyRepository repo, IWebHostEnvironment environment)
        {
            _repo = repo; //
            _environment = environment;
        }

        public async Task<int> Handle(CreateCompanyCommand cmd, CancellationToken ct)
        {
            string logoPath = string.Empty;

            // --- Photo Upload Logic ---
            if (!string.IsNullOrEmpty(cmd.Request.LogoUrl) && cmd.Request.LogoUrl.Contains("base64"))
            {
                // 1. wwwroot ke andar folder path set karein
                string folderPath = Path.Combine(_environment.WebRootPath, "uploads", "logos");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 2. Unique file name generate karein
                string fileName = $"logo_{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(folderPath, fileName);

                // 3. Base64 string se data nikal kar file save karein
                var base64Data = cmd.Request.LogoUrl.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                await File.WriteAllBytesAsync(fullPath, imageBytes);

                // 4. DB mein relative path save karein
                logoPath = $"/uploads/logos/{fileName}";
            }
            else
            {
                logoPath = cmd.Request.LogoUrl; // Agar simple URL hai toh
            }

            // Mapping to Domain Entity
            var company = new CompanyProfile
            {
                Name = cmd.Request.Name,
                Tagline = cmd.Request.Tagline,
                RegistrationNumber = cmd.Request.RegistrationNumber,
                Gstin = cmd.Request.Gstin, // Max 15
                LogoUrl = logoPath, // Physical file ka path
                PrimaryEmail = cmd.Request.PrimaryEmail,
                PrimaryPhone = cmd.Request.PrimaryPhone,
                Website = cmd.Request.Website,
                IsActive = true,

                CompanyAddress = new Address
                {
                    AddressLine1 = cmd.Request.Address.AddressLine1,
                    AddressLine2 = cmd.Request.Address.AddressLine2,
                    City = cmd.Request.Address.City,
                    State = cmd.Request.Address.State,
                    StateCode = cmd.Request.Address.StateCode, // Max 2
                    PinCode = cmd.Request.Address.PinCode,
                    Country = cmd.Request.Address.Country ?? "India"
                },

                BankInformation = new BankDetail
                {
                    BankName = cmd.Request.BankInfo.BankName,
                    BranchName = cmd.Request.BankInfo.BranchName,
                    AccountNumber = cmd.Request.BankInfo.AccountNumber,
                    IfscCode = cmd.Request.BankInfo.IfscCode,
                    AccountType = cmd.Request.BankInfo.AccountType ?? "Current"
                },

                AuthorizedSignatories = cmd.Request.AuthorizedSignatories?.Select(s => new AuthorizedSignatory
                {
                    PersonName = s.PersonName,
                    Designation = s.Designation,
                    SignatureImageUrl = s.SignatureImageUrl, // Base64 handling could be added here if needed
                    IsDefault = s.IsDefault
                }).ToList() ?? new List<AuthorizedSignatory>()
            };

            return await _repo.InsertCompanyAsync(company); //

        }
    }
}