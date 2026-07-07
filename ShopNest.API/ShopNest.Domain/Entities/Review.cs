using System;
using System.Collections.Generic;
using ShopNest.Domain.Common;
using ShopNest.Domain.Enums;

namespace ShopNest.Domain.Entities;

public sealed class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public int Rating { get; set; } // 1 to 5
    public string ReviewTitle { get; set; } = string.Empty;
    public string ReviewDescription { get; set; } = string.Empty;
    
    public bool IsRecommended { get; set; }
    public int HelpfulCount { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public bool IsApproved { get => Status == ReviewStatus.Approved; set => Status = value ? ReviewStatus.Approved : ReviewStatus.Pending; }
    public string AdminNotes { get; set; } = string.Empty;

    public bool IsReported { get; set; }
    public string ReportReason { get; set; } = string.Empty;

    public List<ReviewImage> ReviewImages { get; set; } = [];
    public List<ReviewHelpfulVote> HelpfulVotes { get; set; } = [];
}
