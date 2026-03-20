namespace HotelBooking.Data.Entities;

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    OnLeave
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int HotelId { get; set; }
    public DateTime ShiftDate { get; set; }
    public int? WorkShiftId { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public double HoursWorked { get; set; }
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
    public WorkShift? WorkShift { get; set; }
}

