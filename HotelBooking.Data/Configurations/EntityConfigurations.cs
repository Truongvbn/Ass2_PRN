using HotelBooking.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelBooking.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.PricePerNight).HasPrecision(18, 2);
        builder.Property(r => r.Description).HasMaxLength(2000);
        builder.Property(r => r.ImageUrl).HasMaxLength(500);
        builder.Property(r => r.Gallery).HasColumnType("text").HasDefaultValue("[]");
        builder.Property(r => r.Amenities).HasMaxLength(2000);
        builder.Property(r => r.RowVersion).IsRowVersion();
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId);

        builder.HasIndex(r => r.HotelId);
        builder.HasIndex(r => r.RoomTypeId);
        builder.HasIndex(r => r.PricePerNight);
    }
}

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).HasMaxLength(200).IsRequired();
        builder.Property(h => h.Description).HasMaxLength(2000);
        builder.Property(h => h.Address).HasMaxLength(500);
        builder.Property(h => h.City).HasMaxLength(100);
        builder.Property(h => h.Latitude).HasPrecision(9, 6);
        builder.Property(h => h.Longitude).HasPrecision(9, 6);
        builder.Property(h => h.PhoneNumber).HasMaxLength(30);
        builder.Property(h => h.Email).HasMaxLength(200);
        builder.Property(h => h.ImageUrl).HasMaxLength(500);
        builder.Property(h => h.Gallery).HasColumnType("text").HasDefaultValue("[]");
        builder.Property(h => h.CreatedAt);
        builder.HasIndex(h => h.City);
    }
}

public class HotelStaffConfiguration : IEntityTypeConfiguration<HotelStaff>
{
    public void Configure(EntityTypeBuilder<HotelStaff> builder)
    {
        builder.HasKey(hs => hs.Id);
        builder.Property(hs => hs.Role).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(hs => hs.Hotel)
            .WithMany(h => h.StaffAssignments)
            .HasForeignKey(hs => hs.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(hs => hs.User)
            .WithMany(u => u.HotelAssignments)
            .HasForeignKey(hs => hs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(hs => new { hs.HotelId, hs.UserId }).IsUnique();
        builder.HasIndex(hs => hs.UserId);
    }
}

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Name).HasMaxLength(100).IsRequired();
        builder.Property(rt => rt.Description).HasMaxLength(500);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.TotalPrice).HasPrecision(18, 2);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.RowVersion).IsRowVersion();

        builder.Property(b => b.GuestNotes).HasMaxLength(2000);
        builder.Property(b => b.AdminNotes).HasMaxLength(2000);
        builder.Property(b => b.CancellationReason).HasMaxLength(1000);

        builder.HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.RoomId, b.CheckIn, b.CheckOut });
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.Status);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        var paymentMethodConverter = new ValueConverter<PaymentMethod, string>(
            v => v.ToString(),
            v => PaymentMethodLegacyConversion.FromDb(v));

        builder.Property(p => p.Method)
            .HasConversion(paymentMethodConverter)
            .HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.TransactionId).HasMaxLength(200);
        builder.Property(p => p.RefundReason).HasMaxLength(1000);
        builder.Property(p => p.RefundAmount).HasPrecision(18, 2);

        builder.HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId);
    }
}

internal static class PaymentMethodLegacyConversion
{
    public static PaymentMethod FromDb(string value)
    {
        if (string.Equals(value, "Card", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.CreditCard;
        if (string.Equals(value, "Credit Card", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.CreditCard;
        if (string.Equals(value, "Credit", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.CreditCard;
        if (string.Equals(value, "Debit Card", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.DebitCard;
        if (string.Equals(value, "Debit", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.DebitCard;
        if (string.Equals(value, "Bank", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.BankTransfer;
        if (string.Equals(value, "Bank Transfer", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.BankTransfer;
        if (string.Equals(value, "Transfer", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.BankTransfer;
        if (string.Equals(value, "CashPayment", StringComparison.OrdinalIgnoreCase)) return PaymentMethod.Cash;

        return Enum.Parse<PaymentMethod>(value, ignoreCase: true);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Content).HasMaxLength(2000).IsRequired();
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.Room)
            .WithMany(rm => rm.Reviews)
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.RoomId);
        builder.HasIndex(r => new { r.RoomId, r.UserId }).IsUnique()
            .HasFilter("\"IsDeleted\" = false"); // 1 review per user per room
    }
}

public class ReviewCommentConfiguration : IEntityTypeConfiguration<ReviewComment>
{
    public void Configure(EntityTypeBuilder<ReviewComment> builder)
    {
        builder.HasKey(rc => rc.Id);
        builder.Property(rc => rc.Content).HasMaxLength(1000).IsRequired();
        builder.HasQueryFilter(rc => !rc.IsDeleted);

        builder.HasOne(rc => rc.Review)
            .WithMany(r => r.Comments)
            .HasForeignKey(rc => rc.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rc => rc.User)
            .WithMany(u => u.ReviewComments)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Subject).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000).IsRequired();
        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.User)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.UserId);
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Gender).HasMaxLength(20);
        builder.Property(e => e.PhoneNumber).HasMaxLength(30);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.IdentityNumber).HasMaxLength(50);
        builder.Property(e => e.Position).HasMaxLength(100);
        builder.Property(e => e.BaseSalary).HasPrecision(18, 2);
        builder.Property(e => e.EmploymentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(e => e.Hotel)
            .WithMany()
            .HasForeignKey(e => e.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.HotelId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
    }
}

public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
        builder.HasKey(ws => ws.Id);
        builder.Property(ws => ws.Name).HasMaxLength(100).IsRequired();

        builder.HasOne(ws => ws.Hotel)
            .WithMany()
            .HasForeignKey(ws => ws.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ws => new { ws.HotelId, ws.IsActive });
    }
}

public class EmployeeShiftAssignmentConfiguration : IEntityTypeConfiguration<EmployeeShiftAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeShiftAssignment> builder)
    {
        builder.HasKey(sa => sa.Id);
        builder.Property(sa => sa.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(sa => sa.Notes).HasMaxLength(1000);

        builder.HasOne(sa => sa.Employee)
            .WithMany(e => e.ShiftAssignments)
            .HasForeignKey(sa => sa.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Hotel)
            .WithMany()
            .HasForeignKey(sa => sa.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.WorkShift)
            .WithMany(ws => ws.ShiftAssignments)
            .HasForeignKey(sa => sa.WorkShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sa => new { sa.HotelId, sa.ShiftDate });
        builder.HasIndex(sa => new { sa.EmployeeId, sa.ShiftDate });
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Hotel)
            .WithMany()
            .HasForeignKey(a => a.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.WorkShift)
            .WithMany()
            .HasForeignKey(a => a.WorkShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.HotelId, a.ShiftDate });
        builder.HasIndex(a => new { a.EmployeeId, a.ShiftDate });
    }
}

public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(p => p.Hotel)
            .WithMany()
            .HasForeignKey(p => p.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.HotelId, p.StartDate, p.EndDate });
        builder.HasIndex(p => p.Status);
    }
}

public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.BaseSalary).HasPrecision(18, 2);
        builder.Property(e => e.CalculatedSalary).HasPrecision(18, 2);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.PayrollPeriod)
            .WithMany(p => p.Entries)
            .HasForeignKey(e => e.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Employee)
            .WithMany(emp => emp.PayrollEntries)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.PayrollPeriodId, e.EmployeeId });
    }
}

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);

        builder.HasOne(t => t.Hotel)
            .WithMany()
            .HasForeignKey(t => t.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.HotelId);
    }
}

public class TrainingEnrollmentConfiguration : IEntityTypeConfiguration<TrainingEnrollment>
{
    public void Configure(EntityTypeBuilder<TrainingEnrollment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Feedback).HasMaxLength(2000);

        builder.HasOne(e => e.TrainingProgram)
            .WithMany(t => t.Enrollments)
            .HasForeignKey(e => e.TrainingProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Employee)
            .WithMany(emp => emp.TrainingEnrollments)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TrainingProgramId, e.EmployeeId });
    }
}

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.OverallRating);
        builder.Property(r => r.Strengths).HasMaxLength(2000);
        builder.Property(r => r.Improvements).HasMaxLength(2000);
        builder.Property(r => r.Goals).HasMaxLength(2000);

        builder.HasOne(r => r.Employee)
            .WithMany(e => e.PerformanceReviews)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Hotel)
            .WithMany()
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.EmployeeId, r.ReviewDate });
        builder.HasIndex(r => r.HotelId);
    }
}

public class EmploymentContractConfiguration : IEntityTypeConfiguration<EmploymentContract>
{
    public void Configure(EntityTypeBuilder<EmploymentContract> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ContractNumber).HasMaxLength(100).IsRequired();
        builder.Property(c => c.ContractType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.BaseSalary).HasPrecision(18, 2);
        builder.Property(c => c.FileUrl).HasMaxLength(500);

        builder.HasOne(c => c.Employee)
            .WithMany(e => e.Contracts)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Hotel)
            .WithMany()
            .HasForeignKey(c => c.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.HotelId, c.Status });
        builder.HasIndex(c => c.EmployeeId);
    }
}

public class InsuranceRecordConfiguration : IEntityTypeConfiguration<InsuranceRecord>
{
    public void Configure(EntityTypeBuilder<InsuranceRecord> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProviderName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.PolicyNumber).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(2000);

        builder.HasOne(i => i.Employee)
            .WithMany(e => e.InsuranceRecords)
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.EmployeeId);
    }
}

