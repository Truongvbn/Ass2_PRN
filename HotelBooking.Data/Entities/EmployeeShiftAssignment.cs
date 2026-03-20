namespace HotelBooking.Data.Entities;

public enum ShiftAssignmentStatus
{
    Planned,
    Completed,
    Cancelled
}

public class EmployeeShiftAssignment
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int HotelId { get; set; }
    public int WorkShiftId { get; set; }
    public DateTime ShiftDate { get; set; }
    public ShiftAssignmentStatus Status { get; set; } = ShiftAssignmentStatus.Planned;
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
    public WorkShift WorkShift { get; set; } = null!;
}

