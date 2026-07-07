using System;
using System.Collections.Generic;
using ShopNest.Domain.Enums;

namespace ShopNest.Application.Dtos;

public sealed record ReviewDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string UserFullName,
    Guid? OrderId,
    int Rating,
    string ReviewTitle,
    string ReviewDescription,
    bool IsRecommended,
    int HelpfulCount,
    ReviewStatus Status,
    string AdminNotes,
    bool IsReported,
    string ReportReason,
    IReadOnlyList<string> ReviewImages,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    bool HasLiked
);

public sealed record RatingSummaryDto(
    double AverageRating,
    int TotalReviews,
    IReadOnlyDictionary<int, int> RatingDistribution
);

public sealed record ProductAnswerDto(
    Guid Id,
    Guid QuestionId,
    Guid UserId,
    string UserFullName,
    string AnswerText,
    bool IsAdminOrSeller,
    DateTime CreatedAtUtc
);

public sealed record ProductQuestionDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string UserFullName,
    string QuestionText,
    DateTime CreatedAtUtc,
    IReadOnlyList<ProductAnswerDto> Answers
);

public sealed record AddReviewRequest(
    Guid ProductId,
    Guid? OrderId,
    int Rating,
    string ReviewTitle,
    string ReviewDescription,
    bool IsRecommended,
    List<string>? ReviewImages
);

public sealed record UpdateReviewRequest(
    int Rating,
    string ReviewTitle,
    string ReviewDescription,
    bool IsRecommended,
    List<string>? ReviewImages
);

public sealed record ReportReviewRequest(string Reason);

public sealed record ModerateReviewRequest(ReviewStatus Status, string AdminNotes);

public sealed record AddQuestionRequest(Guid ProductId, string QuestionText);

public sealed record AddAnswerRequest(string AnswerText);
public sealed record UpdateQuestionRequest(string QuestionText);
public sealed record UpdateAnswerRequest(string AnswerText);
