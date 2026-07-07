using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Domain.Enums;

namespace ShopNest.Application.Interfaces;

public interface IReviewService
{
    // Reviews
    Task<PagedResult<ReviewDto>> GetReviewsAsync(Guid? productId, Guid? userId, ReviewStatus? status, int page, int pageSize, Guid? currentUserId, CancellationToken cancellationToken);
    Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId, Guid? currentUserId, CancellationToken cancellationToken);
    Task<RatingSummaryDto> GetRatingSummaryAsync(Guid productId, CancellationToken cancellationToken);
    Task<ReviewDto> AddReviewAsync(Guid userId, AddReviewRequest request, CancellationToken cancellationToken);
    Task<ReviewDto> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task<bool> RestoreReviewAsync(Guid reviewId, CancellationToken cancellationToken);
    Task<bool> ReportReviewAsync(Guid reviewId, ReportReviewRequest request, CancellationToken cancellationToken);
    Task<bool> LikeHelpfulReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken);
    Task<bool> UnlikeHelpfulReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken);
    Task<ReviewDto?> ModerateReviewAsync(Guid reviewId, ModerateReviewRequest request, CancellationToken cancellationToken);

    // Q&A
    Task<PagedResult<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<ProductQuestionDto> AskQuestionAsync(Guid userId, AddQuestionRequest request, CancellationToken cancellationToken);
    Task<ProductQuestionDto> ReplyToQuestionAsync(Guid userId, Guid questionId, bool isAdminOrSeller, AddAnswerRequest request, CancellationToken cancellationToken);
    Task<ProductQuestionDto> UpdateQuestionAsync(Guid userId, Guid questionId, UpdateQuestionRequest request, CancellationToken cancellationToken);
    Task<ProductAnswerDto> UpdateAnswerAsync(Guid userId, Guid answerId, UpdateAnswerRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteQuestionAsync(Guid userId, Guid questionId, bool isAdmin, CancellationToken cancellationToken);
    Task<bool> DeleteAnswerAsync(Guid userId, Guid answerId, bool isAdmin, CancellationToken cancellationToken);
}
