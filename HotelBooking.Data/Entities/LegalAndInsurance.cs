namespace HotelBooking.Data.Entities;

public enum ContractStatus
{
    Active,
    Expired,
    Terminated
}

public enum ContractType
{
    Indefinite,
    FixedTerm,
    Probation
}

public class EmploymentContract
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int HotelId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public ContractType ContractType { get; set; } = ContractType.FixedTerm;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public bool InsuranceIncluded { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public string? FileUrl { get; set; }

    public Employee Employee { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
}

public class InsuranceRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}

