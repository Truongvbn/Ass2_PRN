using HotelBooking.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        builder.Property(r => r.Amenities).HasMaxLength(2000);
        builder.Property(r => r.RowVersion).IsRowVersion();
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId);

        builder.HasIndex(r => r.RoomTypeId);
        builder.HasIndex(r => r.PricePerNight);
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
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId);
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
            .HasFilter("[IsDeleted] = 0"); // 1 review per user per room
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
