using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;

namespace ShopNest.Infrastructure.Services;

public sealed class ReviewService(
    ShopNestDbContext db,
    IHubContext<OrderHub> hub,
    INotificationService notificationService
) : IReviewService
{
    public async Task<PagedResult<ReviewDto>> GetReviewsAsync(
        Guid? productId,
        Guid? userId,
        ReviewStatus? status,
        int page,
        int pageSize,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Reviews
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.ReviewImages)
            .Include(x => x.HelpfulVotes)
            .AsNoTracking();

        if (productId.HasValue)
        {
            query = query.Where(x => x.ProductId == productId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        else
        {
            // If not specified, default to Approved for public view
            query = query.Where(x => x.Status == ReviewStatus.Approved);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x => MapReviewToDto(x, currentUserId)).ToList();
        return new PagedResult<ReviewDto>(dtos, page, pageSize, total);
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var review = await db.Reviews
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.ReviewImages)
            .Include(x => x.HelpfulVotes)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);

        return review == null ? null : MapReviewToDto(review, currentUserId);
    }

    public async Task<RatingSummaryDto> GetRatingSummaryAsync(Guid productId, CancellationToken cancellationToken)
    {
        var reviews = await db.Reviews
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.Status == ReviewStatus.Approved && !x.IsDeleted)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);

        var total = reviews.Count;
        var average = total > 0 ? Math.Round(reviews.Average(), 1) : 0.0;

        var distribution = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var r in reviews)
        {
            if (distribution.ContainsKey(r))
            {
                distribution[r]++;
            }
        }

        return new RatingSummaryDto(average, total, distribution);
    }

    public async Task<ReviewDto> AddReviewAsync(Guid userId, AddReviewRequest request, CancellationToken cancellationToken)
    {
        // 1. Business Rule: Only verified purchasers can review
        var orderItemsQuery = db.OrderItems
            .Include(x => x.Order)
            .Where(x => x.Order.UserId == userId && x.ProductId == request.ProductId && x.Order.Status == OrderStatus.Delivered);

        if (request.OrderId.HasValue)
        {
            orderItemsQuery = orderItemsQuery.Where(x => x.OrderId == request.OrderId.Value);
        }

        var purchased = await orderItemsQuery.AnyAsync(cancellationToken);
        if (!purchased)
        {
            throw new InvalidOperationException("You can only review products you have purchased and received.");
        }

        // Determine Order ID if not supplied
        var matchedOrderId = request.OrderId;
        if (!matchedOrderId.HasValue)
        {
            var matchedOrder = await db.OrderItems
                .Include(x => x.Order)
                .Where(x => x.Order.UserId == userId && x.ProductId == request.ProductId && x.Order.Status == OrderStatus.Delivered)
                .OrderByDescending(x => x.Order.CreatedAtUtc)
                .Select(x => x.OrderId)
                .FirstOrDefaultAsync(cancellationToken);
            matchedOrderId = matchedOrder;
        }

        // 2. Business Rule: One review per product per order
        if (matchedOrderId.HasValue)
        {
            var alreadyReviewed = await db.Reviews
                .AnyAsync(x => x.UserId == userId && x.ProductId == request.ProductId && x.OrderId == matchedOrderId.Value, cancellationToken);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("You have already reviewed this product for this order.");
            }
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Rating), "Rating must be between 1 and 5.");
        }

        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            OrderId = matchedOrderId,
            Rating = request.Rating,
            ReviewTitle = request.ReviewTitle,
            ReviewDescription = request.ReviewDescription,
            IsRecommended = request.IsRecommended,
            Status = ReviewStatus.Pending
        };

        if (request.ReviewImages is not null)
        {
            foreach (var imgUrl in request.ReviewImages)
            {
                review.ReviewImages.Add(new ReviewImage { ImageUrl = imgUrl });
            }
        }

        db.Reviews.Add(review);

        // Audit Log
        db.AuditLogs.Add(new AuditLog
        {
            Action = "ReviewCreated",
            UserId = userId,
            EntityName = "Review",
            EntityId = review.Id.ToString(),
            Details = $"Review created with rating {request.Rating} for Product {request.ProductId}."
        });

        await db.SaveChangesAsync(cancellationToken);

        // Send Notification
        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Review Submitted",
            Message = "Your review for the product has been submitted and is pending approval.",
            NotificationType = "Info"
        });
        await db.SaveChangesAsync(cancellationToken);

        var saved = await db.Reviews
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.ReviewImages)
            .Include(x => x.HelpfulVotes)
            .FirstAsync(x => x.Id == review.Id, cancellationToken);

        return MapReviewToDto(saved, userId);
    }

    public async Task<ReviewDto> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await db.Reviews
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.ReviewImages)
            .Include(x => x.HelpfulVotes)
            .FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken)
            ?? throw new KeyNotFoundException("Review not found.");

        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to edit this review.");
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Rating), "Rating must be between 1 and 5.");
        }

        review.Rating = request.Rating;
        review.ReviewTitle = request.ReviewTitle;
        review.ReviewDescription = request.ReviewDescription;
        review.IsRecommended = request.IsRecommended;
        review.Status = ReviewStatus.Pending; // Force re-moderation

        // Update Images
        db.ReviewImages.RemoveRange(review.ReviewImages);
        review.ReviewImages.Clear();
        if (request.ReviewImages is not null)
        {
            foreach (var imgUrl in request.ReviewImages)
            {
                review.ReviewImages.Add(new ReviewImage { ImageUrl = imgUrl });
            }
        }

        db.AuditLogs.Add(new AuditLog
        {
            Action = "ReviewUpdated",
            UserId = userId,
            EntityName = "Review",
            EntityId = review.Id.ToString(),
            Details = "Review contents modified."
        });

        await db.SaveChangesAsync(cancellationToken);

        return MapReviewToDto(review, userId);
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var review = await db.Reviews
            .FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);

        if (review is null) return false;

        if (review.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this review.");
        }

        review.IsDeleted = true;
        review.DeletedAtUtc = DateTime.UtcNow;
        review.DeletedBy = userId.ToString();

        db.AuditLogs.Add(new AuditLog
        {
            Action = "ReviewDeleted",
            UserId = userId,
            EntityName = "Review",
            EntityId = reviewId.ToString(),
            Details = $"Review deleted. Action by Admin: {isAdmin}."
        });

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreReviewAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var review = await db.Reviews
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);

        if (review is null) return false;

        review.IsDeleted = false;
        review.DeletedAtUtc = null;
        review.DeletedBy = null;

        db.AuditLogs.Add(new AuditLog
        {
            Action = "ReviewRestored",
            EntityName = "Review",
            EntityId = reviewId.ToString(),
            Details = "Review restored from soft-deleted state."
        });

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReportReviewAsync(Guid reviewId, ReportReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await db.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);
        if (review is null) return false;

        review.IsReported = true;
        review.ReportReason = request.Reason;
        review.Status = ReviewStatus.Hidden; // Hide pending moderation

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> LikeHelpfulReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken)
    {
        var exists = await db.ReviewHelpfulVotes
            .AnyAsync(x => x.ReviewId == reviewId && x.UserId == userId, cancellationToken);
        if (exists) return true;

        var review = await db.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);
        if (review is null) return false;

        db.ReviewHelpfulVotes.Add(new ReviewHelpfulVote
        {
            ReviewId = reviewId,
            UserId = userId
        });

        review.HelpfulCount++;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnlikeHelpfulReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken)
    {
        var vote = await db.ReviewHelpfulVotes
            .FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.UserId == userId, cancellationToken);
        if (vote is null) return true;

        var review = await db.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);
        if (review is null) return false;

        db.ReviewHelpfulVotes.Remove(vote);
        review.HelpfulCount = Math.Max(0, review.HelpfulCount - 1);
        
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ReviewDto?> ModerateReviewAsync(Guid reviewId, ModerateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await db.Reviews
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.ReviewImages)
            .Include(x => x.HelpfulVotes)
            .FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);

        if (review is null) return null;

        review.Status = request.Status;
        review.AdminNotes = request.AdminNotes;

        var subject = $"Product Review {request.Status}";
        var msg = $"Your review for '{review.Product.Name}' has been {request.Status.ToString().ToLower()}.";
        if (!string.IsNullOrWhiteSpace(request.AdminNotes))
        {
            msg += $" Reason/Notes: {request.AdminNotes}";
        }

        await notificationService.SendManualNotificationAsync(new SendManualNotificationRequest(
            review.UserId,
            subject,
            msg,
            request.Status == ReviewStatus.Approved ? "Success" : "Warning",
            "Email",
            "Medium"
        ), cancellationToken);

        var dto = MapReviewToDto(review, review.UserId);
        await hub.Clients.Group(review.UserId.ToString()).SendAsync("reviewStatusChanged", dto, cancellationToken);
        return dto;
    }

    // Q&A
    public async Task<PagedResult<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.ProductQuestions
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.Answers)
                .ThenInclude(a => a.User)
            .Where(x => x.ProductId == productId)
            .AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapQuestionToDto).ToList();
        return new PagedResult<ProductQuestionDto>(dtos, page, pageSize, total);
    }

    public async Task<ProductQuestionDto> AskQuestionAsync(Guid userId, AddQuestionRequest request, CancellationToken cancellationToken)
    {
        var question = new ProductQuestion
        {
            ProductId = request.ProductId,
            UserId = userId,
            QuestionText = request.QuestionText
        };

        db.ProductQuestions.Add(question);

        db.AuditLogs.Add(new AuditLog
        {
            Action = "QuestionAsked",
            UserId = userId,
            EntityName = "Question",
            EntityId = question.Id.ToString(),
            Details = $"Question text: {request.QuestionText}"
        });

        await db.SaveChangesAsync(cancellationToken);

        var saved = await db.ProductQuestions
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.Answers)
            .FirstAsync(x => x.Id == question.Id, cancellationToken);

        return MapQuestionToDto(saved);
    }

    public async Task<ProductQuestionDto> ReplyToQuestionAsync(Guid userId, Guid questionId, bool isAdminOrSeller, AddAnswerRequest request, CancellationToken cancellationToken)
    {
        var question = await db.ProductQuestions
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question not found.");

        var answer = new ProductAnswer
        {
            QuestionId = questionId,
            UserId = userId,
            AnswerText = request.AnswerText,
            IsAdminOrSeller = isAdminOrSeller
        };

        db.ProductAnswers.Add(answer);

        // Notify question asker
        db.Notifications.Add(new Notification
        {
            UserId = question.UserId,
            Title = "Question Answered",
            Message = "Your question has been replied to.",
            NotificationType = "Info"
        });

        db.AuditLogs.Add(new AuditLog
        {
            Action = "QuestionAnswered",
            UserId = userId,
            EntityName = "Answer",
            EntityId = answer.Id.ToString(),
            Details = $"Answer: {request.AnswerText}"
        });

        await db.SaveChangesAsync(cancellationToken);

        var refreshed = await db.ProductQuestions
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.Answers)
                .ThenInclude(a => a.User)
            .FirstAsync(x => x.Id == questionId, cancellationToken);

        return MapQuestionToDto(refreshed);
    }

    public async Task<ProductQuestionDto> UpdateQuestionAsync(Guid userId, Guid questionId, UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var question = await db.ProductQuestions
            .Include(x => x.Product)
            .Include(x => x.User)
            .Include(x => x.Answers)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(x => x.Id == questionId, cancellationToken)
            ?? throw new KeyNotFoundException("Question not found.");

        if (question.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to edit this question.");
        }

        question.QuestionText = request.QuestionText;
        await db.SaveChangesAsync(cancellationToken);
        return MapQuestionToDto(question);
    }

    public async Task<ProductAnswerDto> UpdateAnswerAsync(Guid userId, Guid answerId, UpdateAnswerRequest request, CancellationToken cancellationToken)
    {
        var answer = await db.ProductAnswers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == answerId, cancellationToken)
            ?? throw new KeyNotFoundException("Answer not found.");

        if (answer.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to edit this answer.");
        }

        answer.AnswerText = request.AnswerText;
        await db.SaveChangesAsync(cancellationToken);
        return MapAnswerToDto(answer);
    }

    public async Task<bool> DeleteQuestionAsync(Guid userId, Guid questionId, bool isAdmin, CancellationToken cancellationToken)
    {
        var question = await db.ProductQuestions.FirstOrDefaultAsync(x => x.Id == questionId, cancellationToken);
        if (question is null) return false;

        if (question.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this question.");
        }

        db.ProductQuestions.Remove(question);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAnswerAsync(Guid userId, Guid answerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var answer = await db.ProductAnswers.FirstOrDefaultAsync(x => x.Id == answerId, cancellationToken);
        if (answer is null) return false;

        if (answer.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this answer.");
        }

        db.ProductAnswers.Remove(answer);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Static Mappers
    private static ReviewDto MapReviewToDto(Review r, Guid? currentUserId)
    {
        var hasLiked = currentUserId.HasValue && r.HelpfulVotes.Any(v => v.UserId == currentUserId.Value);
        return new ReviewDto(
            r.Id,
            r.ProductId,
            r.Product.Name,
            r.UserId,
            r.User.FullName,
            r.OrderId,
            r.Rating,
            r.ReviewTitle,
            r.ReviewDescription,
            r.IsRecommended,
            r.HelpfulCount,
            r.Status,
            r.AdminNotes,
            r.IsReported,
            r.ReportReason,
            r.ReviewImages.Select(x => x.ImageUrl).ToList(),
            r.CreatedAtUtc,
            r.UpdatedAtUtc,
            hasLiked
        );
    }

    private static ProductAnswerDto MapAnswerToDto(ProductAnswer a) => new(
        a.Id,
        a.QuestionId,
        a.UserId,
        a.User.FullName,
        a.AnswerText,
        a.IsAdminOrSeller,
        a.CreatedAtUtc
    );

    private static ProductQuestionDto MapQuestionToDto(ProductQuestion q) => new(
        q.Id,
        q.ProductId,
        q.Product.Name,
        q.UserId,
        q.User.FullName,
        q.QuestionText,
        q.CreatedAtUtc,
        q.Answers.Select(MapAnswerToDto).ToList()
    );
}
