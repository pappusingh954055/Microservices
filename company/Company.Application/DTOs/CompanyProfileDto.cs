namespace Company.Application.DTOs
{
    // Response ke liye use hoga (Read Operations)
    public record CompanyProfileDto(
        int Id,
        string Name,
        string Tagline,
        string RegistrationNumber,
        string Gstin,
        string LogoUrl,
        string PrimaryEmail,
        string PrimaryPhone,
        string Website,
        bool IsActive,
        AddressDto Address, // Aapke request ke hisaab se name match
        BankDetailDto BankInfo,
        List<AuthorizedSignatoryDto> AuthorizedSignatories
    );

    // Shared Records
    public record AddressDto(int Id, string AddressLine1, string AddressLine2, string City, string State, string StateCode, string PinCode, string Country);

    public record BankDetailDto(int Id, string BankName, string BranchName, string AccountNumber, string IfscCode, string AccountType);

    public record AuthorizedSignatoryDto(int Id, string PersonName, string Designation, string SignatureImageUrl, bool IsDefault);

    // Request ke liye use hoga (Create/Update)
    public record UpsertCompanyRequest(
        string Name,
        string Tagline,
        string RegistrationNumber,
        string Gstin,
        string LogoUrl,
        string PrimaryEmail,
        string PrimaryPhone,
        string Website,
        AddressDto Address,
        BankDetailDto BankInfo,
        List<AuthorizedSignatoryDto> AuthorizedSignatories
    );
}
