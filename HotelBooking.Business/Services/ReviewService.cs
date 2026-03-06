using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IReviewCommentRepository _commentRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IMapper _mapper;
    private readonly IReviewHubNotifier _notifier;

    public ReviewService(IReviewRepository reviewRepo, IReviewCommentRepository commentRepo,
        IBookingRepository bookingRepo, IMapper mapper, IReviewHubNotifier notifier)
    {
        _reviewRepo = reviewRepo;
        _commentRepo = commentRepo;
        _bookingRepo = bookingRepo;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ServiceResult<ReviewDto>> CreateReviewAsync(CreateReviewDto dto, string userId, CancellationToken ct = default)
    {
        // Validate rating
        if (dto.Rating is < 1 or > 5)
            return ServiceResult<ReviewDto>.Failure("Rating must be between 1 and 5", "VALIDATION");
        if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length < 10)
            return ServiceResult<ReviewDto>.Failure("Review content must be at least 10 characters", "VALIDATION");
        if (dto.Content.Length > 2000)
            return ServiceResult<ReviewDto>.Failure("Review content cannot exceed 2000 characters", "VALIDATION");

        // Check user has completed booking for this room
        var userBookings = await _bookingRepo.GetByUserAsync(userId, ct);
        var hasCompletedStay = userBookings.Any(b => b.RoomId == dto.RoomId && b.Status == BookingStatus.Completed);
        if (!hasCompletedStay)
            return ServiceResult<ReviewDto>.Failure("You can only review rooms where you've completed a stay", "NO_COMPLETED_BOOKING");

        // Check 1 review per user per room
        if (await _reviewRepo.HasUserReviewedRoomAsync(userId, dto.RoomId, ct))
            return ServiceResult<ReviewDto>.Failure("You have already reviewed this room", "DUPLICATE_REVIEW");

        var review = new Review
        {
            RoomId = dto.RoomId,
            UserId = userId,
            Rating = dto.Rating,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reviewRepo.AddAsync(review, ct);
        var reviews = await _reviewRepo.GetByRoomWithCommentsAsync(dto.RoomId, ct);
        var created = reviews.First(r => r.Id == review.Id);
        
        var reviewDto = _mapper.Map<ReviewDto>(created);
        await _notifier.ReviewCreated(reviewDto);

        return ServiceResult<ReviewDto>.Success(reviewDto);
    }

    public async Task<ServiceResult<ReviewDto>> UpdateReviewAsync(UpdateReviewDto dto, string userId, CancellationToken ct = default)
    {
        var review = await _reviewRepo.GetByIdAsync(dto.Id, ct);
        if (review is null) return ServiceResult<ReviewDto>.Failure("Review not found", "NOT_FOUND");
        if (review.UserId != userId)
            return ServiceResult<ReviewDto>.Failure("You can only edit your own reviews", "FORBIDDEN");
        if (dto.Rating is < 1 or > 5)
            return ServiceResult<ReviewDto>.Failure("Rating must be between 1 and 5", "VALIDATION");

        review.Rating = dto.Rating;
        review.Content = dto.Content;
        review.UpdatedAt = DateTime.UtcNow;
        await _reviewRepo.UpdateAsync(review, ct);

        var reviews = await _reviewRepo.GetByRoomWithCommentsAsync(review.RoomId, ct);
        var updated = reviews.First(r => r.Id == review.Id);
        
        var reviewDto = _mapper.Map<ReviewDto>(updated);
        await _notifier.ReviewUpdated(reviewDto);

        return ServiceResult<ReviewDto>.Success(reviewDto);
    }

    public async Task<ServiceResult> DeleteReviewAsync(int id, string userId, bool isAdmin, CancellationToken ct = default)
    {
        var review = await _reviewRepo.GetByIdAsync(id, ct);
        if (review is null) return ServiceResult.Failure("Review not found", "NOT_FOUND");
        if (review.UserId != userId && !isAdmin)
            return ServiceResult.Failure("You can only delete your own reviews", "FORBIDDEN");

        review.IsDeleted = true;
        review.UpdatedAt = DateTime.UtcNow;
        await _reviewRepo.UpdateAsync(review, ct);

        await _notifier.ReviewDeleted(id, review.RoomId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyList<ReviewDto>>> GetRoomReviewsAsync(int roomId, CancellationToken ct = default)
    {
        var reviews = await _reviewRepo.GetByRoomWithCommentsAsync(roomId, ct);
        return ServiceResult<IReadOnlyList<ReviewDto>>.Success(_mapper.Map<IReadOnlyList<ReviewDto>>(reviews));
    }

    public async Task<ServiceResult<ReviewCommentDto>> AddCommentAsync(CreateReviewCommentDto dto, string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return ServiceResult<ReviewCommentDto>.Failure("Comment cannot be empty", "VALIDATION");
        if (dto.Content.Length > 1000)
            return ServiceResult<ReviewCommentDto>.Failure("Comment cannot exceed 1000 characters", "VALIDATION");

        var review = await _reviewRepo.GetByIdAsync(dto.ReviewId, ct);
        if (review is null) return ServiceResult<ReviewCommentDto>.Failure("Review not found", "NOT_FOUND");

        var comment = new ReviewComment
        {
            ReviewId = dto.ReviewId,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _commentRepo.AddAsync(comment, ct);

        // Re-fetch to get user data
        var reviews = await _reviewRepo.GetByRoomWithCommentsAsync(review.RoomId, ct);
        var parentReview = reviews.First(r => r.Id == dto.ReviewId);
        var createdComment = parentReview.Comments.First(c => c.Id == comment.Id);
        
        var commentDto = _mapper.Map<ReviewCommentDto>(createdComment);
        await _notifier.CommentAdded(commentDto);

        return ServiceResult<ReviewCommentDto>.Success(commentDto);
    }

    public async Task<ServiceResult> DeleteCommentAsync(int id, string userId, bool isAdmin, CancellationToken ct = default)
    {
        var comment = await _commentRepo.GetByIdAsync(id, ct);
        if (comment is null) return ServiceResult.Failure("Comment not found", "NOT_FOUND");
        if (comment.UserId != userId && !isAdmin)
            return ServiceResult.Failure("You can only delete your own comments", "FORBIDDEN");

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentRepo.UpdateAsync(comment, ct);

        await _notifier.CommentDeleted(id, comment.ReviewId);

        return ServiceResult.Success();
    }
}
