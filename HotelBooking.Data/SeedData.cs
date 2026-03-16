using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = ["Admin", "Staff", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed users
        var admin = await EnsureUserAsync(userManager, "admin@hotel.com", "Hotel@123", "Admin User", "Admin");
        var staff = await EnsureUserAsync(userManager, "staff@hotel.com", "Hotel@123", "Staff Member", "Staff");
        var customer = await EnsureUserAsync(userManager, "customer@hotel.com", "Hotel@123", "John Customer", "Customer");
        var customer2 = await EnsureUserAsync(userManager, "customer2@hotel.com", "Hotel@123", "Alice Smith", "Customer");
        var customer3 = await EnsureUserAsync(userManager, "customer3@hotel.com", "Hotel@123", "Bob Johnson", "Customer");

        // Seed major Vietnam hotels (align with provinces API names)
        var majorHotels = new List<Hotel>
        {
            new()
            {
                Name = "Hanoi Elegance Hotel",
                Description = "Boutique stay in the Old Quarter with curated rooms and local hospitality.",
                Address = "Hoàn Kiếm, Hà Nội",
                City = "Thành phố Hà Nội",
                Latitude = 21.0285,
                Longitude = 105.8542,
                PhoneNumber = "+84 24 0000 0000",
                Email = "contact@hanoielegance.example",
                ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1501117716987-c8e1ecb2108a?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1505691723518-36a5ac3be353?auto=format&fit=crop&w=1200&q=80"]
""",
                StarRating = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Saigon Pearl Hotel",
                Description = "Modern city hotel with skyline views and fast access to central districts.",
                Address = "Quận 1, TP. Hồ Chí Minh",
                City = "Thành phố Hồ Chí Minh",
                Latitude = 10.7769,
                Longitude = 106.7009,
                PhoneNumber = "+84 28 0000 0000",
                Email = "contact@saigonpearl.example",
                ImageUrl = "https://images.unsplash.com/photo-1521783593447-5702f2f4b4d1?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1512453979798-5ea266f8880c?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1501117716987-c8e1ecb2108a?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1500043201641-4b0acfa9a41b?auto=format&fit=crop&w=1200&q=80"]
""",
                StarRating = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Da Nang Beach Resort",
                Description = "Beachfront resort with airy rooms and sunset-facing balconies.",
                Address = "Ngũ Hành Sơn, Đà Nẵng",
                City = "Thành phố Đà Nẵng",
                Latitude = 16.0544,
                Longitude = 108.2022,
                PhoneNumber = "+84 236 0000 0000",
                Email = "contact@danangbeach.example",
                ImageUrl = "https://images.unsplash.com/photo-1501117716987-c8e1ecb2108a?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1501117716987-c8e1ecb2108a?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80"]
""",
                StarRating = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Hai Phong Harbor Hotel",
                Description = "Convenient harbor-side stay for business and weekend getaways.",
                Address = "Hồng Bàng, Hải Phòng",
                City = "Thành phố Hải Phòng",
                Latitude = 20.8449,
                Longitude = 106.6881,
                PhoneNumber = "+84 225 0000 0000",
                Email = "contact@haiphongharbor.example",
                ImageUrl = "https://images.unsplash.com/photo-1479839672679-a46483c0e7c8?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1200&q=80"]
""",
                StarRating = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Can Tho Riverside Hotel",
                Description = "Riverside views near the floating market with relaxing common areas.",
                Address = "Ninh Kiều, Cần Thơ",
                City = "Thành phố Cần Thơ",
                Latitude = 10.0452,
                Longitude = 105.7469,
                PhoneNumber = "+84 292 0000 0000",
                Email = "contact@canthoriverside.example",
                ImageUrl = "https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80"]
""",
                StarRating = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Hue Imperial Hotel",
                Description = "Heritage-inspired comfort near the Perfume River and Imperial City.",
                Address = "Trung tâm, Huế",
                City = "Thành phố Huế",
                Latitude = 16.4637,
                Longitude = 107.5909,
                PhoneNumber = "+84 234 0000 0000",
                Email = "contact@hueimperial.example",
                ImageUrl = "https://images.unsplash.com/photo-1519823551271-876d8e87aa36?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1512453979798-5ea266f8880c?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1519823551271-876d8e87aa36?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1200&q=80"]
""",
                StarRating = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var h in majorHotels)
        {
            var existing = await context.Hotels.FirstOrDefaultAsync(x => x.Name == h.Name);
            if (existing is null)
            {
                context.Hotels.Add(h);
            }
            else
            {
                existing.Description = h.Description;
                existing.Address = h.Address;
                existing.City = h.City;
                existing.Latitude = h.Latitude;
                existing.Longitude = h.Longitude;
                existing.PhoneNumber = h.PhoneNumber;
                existing.Email = h.Email;
                existing.ImageUrl = h.ImageUrl;
                existing.StarRating = h.StarRating;
                existing.IsActive = h.IsActive;
            }
        }
        await context.SaveChangesAsync();

        var defaultHotel = await context.Hotels.FirstAsync(h => h.Name == "Hanoi Elegance Hotel");
        var majorHotelNames = majorHotels.Select(h => h.Name).ToList();

        // Ensure staff is assigned to the default hotel
        var existingAssignment = await context.HotelStaff
            .AnyAsync(hs => hs.HotelId == defaultHotel.Id && hs.UserId == staff.Id);
        if (!existingAssignment)
        {
            context.HotelStaff.Add(new HotelStaff
            {
                HotelId = defaultHotel.Id,
                UserId = staff.Id,
                Role = HotelStaffRole.Receptionist,
                AssignedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed room types (idempotent)
        if (!await context.RoomTypes.AnyAsync())
        {
            var roomTypes = new List<RoomType>
            {
                new() { Name = "Standard", Description = "Comfortable room with essential amenities" },
                new() { Name = "Superior", Description = "Upgraded room with city or garden view" },
                new() { Name = "Deluxe", Description = "Spacious room with premium furnishings" },
                new() { Name = "Suite", Description = "Separate living area with luxury amenities" },
                new() { Name = "Presidential", Description = "The finest accommodation with panoramic views" }
            };
            context.RoomTypes.AddRange(roomTypes);
            await context.SaveChangesAsync();
        }

        var roomTypesByName = await context.RoomTypes.ToDictionaryAsync(rt => rt.Name);

        // Seed rooms
        var rooms = new List<Room>
        {
            new()
            {
                HotelId = defaultHotel.Id,
                Name = "Standard Room 101",
                RoomTypeId = roomTypesByName["Standard"].Id,
                PricePerNight = 900_000,
                MaxOccupancy = 2,
                Description = "Cozy room with Old Quarter vibes and a calm corner view.",
                ImageUrl = "https://images.unsplash.com/photo-1505691723518-36a5ac3be353?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1499916078039-922301b0eb9b?auto=format&fit=crop&w=1200&q=80"]
""",
                Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                HotelId = defaultHotel.Id,
                Name = "Standard Room 102",
                RoomTypeId = roomTypesByName["Standard"].Id,
                PricePerNight = 950_000,
                MaxOccupancy = 2,
                Description = "Clean and comfortable with modern decor.",
                ImageUrl = "https://images.unsplash.com/photo-1501117716987-c8e1ecb2108a?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1512914890250-353c97c9e7e2?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1519710164239-da123dc03ef4?auto=format&fit=crop&w=1200&q=80"]
""",
                Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                HotelId = defaultHotel.Id,
                Name = "Superior Room 201",
                RoomTypeId = roomTypesByName["Superior"].Id,
                PricePerNight = 1_300_000,
                MaxOccupancy = 2,
                Description = "Upgraded comfort with a brighter layout.",
                ImageUrl = "https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1469796466635-455ede028aca?auto=format&fit=crop&w=1200&q=80"]
""",
                Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                HotelId = defaultHotel.Id,
                Name = "Deluxe Room 301",
                RoomTypeId = roomTypesByName["Deluxe"].Id,
                PricePerNight = 1_900_000,
                MaxOccupancy = 3,
                Description = "Spacious deluxe room ideal for couples or small families.",
                ImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80"]
""",
                Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Workspace\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                HotelId = defaultHotel.Id,
                Name = "Suite 401",
                RoomTypeId = roomTypesByName["Suite"].Id,
                PricePerNight = 2_800_000,
                MaxOccupancy = 4,
                Description = "Suite with separate seating area and upgraded bath amenities.",
                ImageUrl = "https://images.unsplash.com/photo-1520256862855-398228c41684?auto=format&fit=crop&w=1200&q=80",
                Gallery = """
["https://images.unsplash.com/photo-1512914890250-353c97c9e7e2?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1519823551271-876d8e87aa36?auto=format&fit=crop&w=1200&q=80"]
""",
                Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Living Room\"]",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
        };
        if (!await context.Rooms.AnyAsync(r => r.HotelId == defaultHotel.Id))
        {
            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();
        }

        // Seed a small set of rooms for other major hotels (3-4 per hotel)
        foreach (var hotel in await context.Hotels.Where(h => majorHotelNames.Contains(h.Name)).ToListAsync())
        {
            if (hotel.Id == defaultHotel.Id) continue;
            if (await context.Rooms.AnyAsync(r => r.HotelId == hotel.Id)) continue;

            var otherRooms = new List<Room>
            {
                new()
                {
                    HotelId = hotel.Id,
                    Name = $"{hotel.City} Standard 101",
                    RoomTypeId = roomTypesByName["Standard"].Id,
                    PricePerNight = 850_000,
                    MaxOccupancy = 2,
                    Description = "Essential comfort with reliable amenities.",
                    ImageUrl = "https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=1200&q=80",
                    Gallery = """
["https://images.unsplash.com/photo-1505691723518-36a5ac3be353?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1519710164239-da123dc03ef4?auto=format&fit=crop&w=1200&q=80"]
""",
                    Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    HotelId = hotel.Id,
                    Name = $"{hotel.City} Superior 201",
                    RoomTypeId = roomTypesByName["Superior"].Id,
                    PricePerNight = 1_250_000,
                    MaxOccupancy = 2,
                    Description = "More space and a brighter room layout.",
                    ImageUrl = "https://images.unsplash.com/photo-1505691723518-36a5ac3be353?auto=format&fit=crop&w=1200&q=80",
                    Gallery = """
["https://images.unsplash.com/photo-1512914890250-353c97c9e7e2?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1499916078039-922301b0eb9b?auto=format&fit=crop&w=1200&q=80"]
""",
                    Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    HotelId = hotel.Id,
                    Name = $"{hotel.City} Deluxe 301",
                    RoomTypeId = roomTypesByName["Deluxe"].Id,
                    PricePerNight = 1_850_000,
                    MaxOccupancy = 3,
                    Description = "Premium furnishings with a city-facing window.",
                    ImageUrl = "https://images.unsplash.com/photo-1519823551271-876d8e87aa36?auto=format&fit=crop&w=1200&q=80",
                    Gallery = """
["https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1505691723518-36a5ac3be353?auto=format&fit=crop&w=1200&q=80"]
""",
                    Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Workspace\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    HotelId = hotel.Id,
                    Name = $"{hotel.City} Suite 401",
                    RoomTypeId = roomTypesByName["Suite"].Id,
                    PricePerNight = 2_650_000,
                    MaxOccupancy = 4,
                    Description = "Suite-style comfort with a separate seating area.",
                    ImageUrl = "https://images.unsplash.com/photo-1520256862855-398228c41684?auto=format&fit=crop&w=1200&q=80",
                    Gallery = """
["https://images.unsplash.com/photo-1512914890250-353c97c9e7e2?auto=format&fit=crop&w=1200&q=80",
"https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=1200&q=80"]
""",
                    Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Living Room\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            context.Rooms.AddRange(otherRooms);
            await context.SaveChangesAsync();
        }

        // Seed a small set of bookings/payments/reviews only once
        if (!await context.Bookings.AnyAsync())
        {
            var seedRooms = await context.Rooms
                .Where(r => r.HotelId == defaultHotel.Id)
                .OrderBy(r => r.Id)
                .ToListAsync();

            if (seedRooms.Count >= 3)
            {
                var bookings = new List<Booking>
                {
                    new()
                    {
                        RoomId = seedRooms[0].Id,
                        UserId = customer.Id,
                        CheckIn = DateTime.UtcNow.Date.AddDays(-7),
                        CheckOut = DateTime.UtcNow.Date.AddDays(-5),
                        NumberOfGuests = 2,
                        TotalPrice = seedRooms[0].PricePerNight * 2,
                        Status = BookingStatus.Completed,
                        CheckedInAt = DateTime.UtcNow.Date.AddDays(-7),
                        CheckedOutAt = DateTime.UtcNow.Date.AddDays(-5)
                    },
                    new()
                    {
                        RoomId = seedRooms[1].Id,
                        UserId = customer2.Id,
                        CheckIn = DateTime.UtcNow.Date.AddDays(5),
                        CheckOut = DateTime.UtcNow.Date.AddDays(7),
                        NumberOfGuests = 2,
                        TotalPrice = seedRooms[1].PricePerNight * 2,
                        Status = BookingStatus.Confirmed,
                        ConfirmedAt = DateTime.UtcNow.Date,
                        PaymentDeadline = DateTime.UtcNow.Date.AddDays(1)
                    },
                    new()
                    {
                        RoomId = seedRooms[2].Id,
                        UserId = customer3.Id,
                        CheckIn = DateTime.UtcNow.Date.AddDays(12),
                        CheckOut = DateTime.UtcNow.Date.AddDays(14),
                        NumberOfGuests = 3,
                        TotalPrice = seedRooms[2].PricePerNight * 2,
                        Status = BookingStatus.Pending
                    }
                };
                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();

                context.Payments.AddRange(
                    new Payment { BookingId = bookings[0].Id, Amount = bookings[0].TotalPrice, Method = PaymentMethod.CreditCard, Status = PaymentStatus.Completed, PaidAt = DateTime.UtcNow.Date.AddDays(-6) },
                    new Payment { BookingId = bookings[1].Id, Amount = bookings[1].TotalPrice, Method = PaymentMethod.BankTransfer, Status = PaymentStatus.Completed, PaidAt = DateTime.UtcNow.Date.AddDays(-1) }
                );
                await context.SaveChangesAsync();

                var reviews = new List<Review>
                {
                    new() { RoomId = seedRooms[0].Id, UserId = customer.Id, Rating = 5, Content = "Great location and very clean room. Staff was friendly and helpful.", CreatedAt = DateTime.UtcNow },
                    new() { RoomId = seedRooms[0].Id, UserId = customer3.Id, Rating = 4, Content = "Nice stay overall. Good WiFi and comfortable bed.", CreatedAt = DateTime.UtcNow }
                };
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();

                context.ReviewComments.AddRange(
                    new ReviewComment { ReviewId = reviews[0].Id, UserId = staff.Id, Content = "Thank you! We hope to welcome you again soon.", CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }
        }

        // Seed tickets (idempotent)
        if (!await context.SupportTickets.AnyAsync())
        {
            context.SupportTickets.AddRange(
                new SupportTicket { UserId = customer.Id, Category = TicketCategory.Room, Priority = TicketPriority.Medium, Subject = "Air conditioning noise", Description = "The AC in room 101 was making a buzzing noise during the night.", Status = TicketStatus.Resolved, AssignedToId = staff.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SupportTicket { UserId = customer.Id, Category = TicketCategory.Payment, Priority = TicketPriority.High, Subject = "Double charge on credit card", Description = "I was charged twice for my booking #3. Please investigate.", Status = TicketStatus.InProgress, AssignedToId = staff.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SupportTicket { UserId = customer.Id, Category = TicketCategory.Service, Priority = TicketPriority.Low, Subject = "Late checkout request", Description = "Can I get a late checkout at 2 PM for my upcoming stay?", Status = TicketStatus.Open, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SupportTicket { UserId = customer2.Id, Category = TicketCategory.Other, Priority = TicketPriority.Medium, Subject = "Cannot update profile picture", Description = "The system gives an error when I try to upload a JPEG.", Status = TicketStatus.Open, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new SupportTicket { UserId = customer3.Id, Category = TicketCategory.Other, Priority = TicketPriority.Low, Subject = "Lost item", Description = "I think I left my sunglasses in room 201.", Status = TicketStatus.Closed, AssignedToId = admin.Id, ClosedAt = DateTime.UtcNow.Date.AddDays(-2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string email, string password, string fullName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, EmailConfirmed = true, CreatedAt = DateTime.UtcNow };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }
}
