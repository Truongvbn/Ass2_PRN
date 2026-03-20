namespace HotelBooking.Data.Entities;

public enum PayrollStatus
{
    Open,
    Calculated,
    Approved,
    Paid
}

public class PayrollPeriod
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Hotel Hotel { get; set; } = null!;
    public ICollection<PayrollEntry> Entries { get; set; } = new List<PayrollEntry>();
}

public class PayrollEntry
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public double TotalHours { get; set; }
    public decimal CalculatedSalary { get; set; }
    public string? Notes { get; set; }

    public PayrollPeriod PayrollPeriod { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

