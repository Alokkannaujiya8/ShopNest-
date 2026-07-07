using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Application.Common;
using ShopNest.Application.Dtos;
using ShopNest.Application.Interfaces;
using ShopNest.Domain.Enums;

namespace ShopNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetReviews(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? userId,
        [FromQuery] ReviewStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? User.UserId() : (Guid?)null;
        var result = await reviewService.GetReviewsAsync(productId, userId, status, page, pageSize, currentUserId, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> GetReviewById(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? User.UserId() : (Guid?)null;
        var review = await reviewService.GetReviewByIdAsync(id, currentUserId, cancellationToken);
        return review is null ? NotFound() : Ok(review);
    }

    [AllowAnonymous]
    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetProductReviews(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = User.Identity?.IsAuthenticated == true ? User.UserId() : (Guid?)null;
        var result = await reviewService.GetReviewsAsync(productId, null, ReviewStatus.Approved, page, pageSize, currentUserId, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("product/{productId:guid}/summary")]
    public async Task<ActionResult<RatingSummaryDto>> GetRatingSummary(Guid productId, CancellationToken cancellationToken)
    {
        var summary = await reviewService.GetRatingSummaryAsync(productId, cancellationToken);
        return Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> AddReview([FromBody] AddReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await reviewService.AddReviewAsync(User.UserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await reviewService.UpdateReviewAsync(id, User.UserId(), request, cancellationToken);
        return Ok(review);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var success = await reviewService.DeleteReviewAsync(id, User.UserId(), isAdmin, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult> RestoreReview(Guid id, CancellationToken cancellationToken)
    {
        var success = await reviewService.RestoreReviewAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/report")]
    public async Task<ActionResult> ReportReview(Guid id, [FromBody] ReportReviewRequest request, CancellationToken cancellationToken)
    {
        var success = await reviewService.ReportReviewAsync(id, request, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/helpful")]
    public async Task<ActionResult> LikeHelpful(Guid id, CancellationToken cancellationToken)
    {
        var success = await reviewService.LikeHelpfulReviewAsync(id, User.UserId(), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}/helpful")]
    public async Task<ActionResult> UnlikeHelpful(Guid id, CancellationToken cancellationToken)
    {
        var success = await reviewService.UnlikeHelpfulReviewAsync(id, User.UserId(), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/moderate")]
    public async Task<ActionResult<ReviewDto>> ModerateReview(Guid id, [FromBody] ModerateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await reviewService.ModerateReviewAsync(id, request, cancellationToken);
        return review is null ? NotFound() : Ok(review);
    }

    // Q&A
    [AllowAnonymous]
    [HttpGet("product/{productId:guid}/questions")]
    public async Task<ActionResult<PagedResult<ProductQuestionDto>>> GetQuestions(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await reviewService.GetProductQuestionsAsync(productId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("questions")]
    public async Task<ActionResult<ProductQuestionDto>> AskQuestion([FromBody] AddQuestionRequest request, CancellationToken cancellationToken)
    {
        var question = await reviewService.AskQuestionAsync(User.UserId(), request, cancellationToken);
        return Ok(question);
    }

    [HttpPost("questions/{questionId:guid}/reply")]
    public async Task<ActionResult<ProductQuestionDto>> ReplyToQuestion(Guid questionId, [FromBody] AddAnswerRequest request, CancellationToken cancellationToken)
    {
        var isAdminOrSeller = User.IsInRole("Admin") || User.IsInRole("Seller");
        var result = await reviewService.ReplyToQuestionAsync(User.UserId(), questionId, isAdminOrSeller, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("questions/{questionId:guid}")]
    public async Task<ActionResult<ProductQuestionDto>> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await reviewService.UpdateQuestionAsync(User.UserId(), questionId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("answers/{answerId:guid}")]
    public async Task<ActionResult<ProductAnswerDto>> UpdateAnswer(Guid answerId, [FromBody] UpdateAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await reviewService.UpdateAnswerAsync(User.UserId(), answerId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("questions/{questionId:guid}")]
    public async Task<ActionResult> DeleteQuestion(Guid questionId, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var success = await reviewService.DeleteQuestionAsync(User.UserId(), questionId, isAdmin, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("answers/{answerId:guid}")]
    public async Task<ActionResult> DeleteAnswer(Guid answerId, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var success = await reviewService.DeleteAnswerAsync(User.UserId(), answerId, isAdmin, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
