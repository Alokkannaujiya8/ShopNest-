using System;

namespace ShopNest.Application.Dtos;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string Title,
    string Message,
    string NotificationType,
    string Channel,
    string Priority,
    string Status,
    string RelatedEntity,
    string RelatedEntityId,
    bool IsRead,
    DateTime? SentTime
);

public sealed record NotificationTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string Subject,
    string Body,
    string Channel
);

public sealed record NotificationLogDto(
    Guid Id,
    Guid NotificationId,
    string Channel,
    string Recipient,
    string Status,
    string ErrorMessage,
    DateTime SentAtUtc
);

public sealed record SendManualNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    string NotificationType, // Information, Success, Warning, Error, Promotion
    string Channel, // Email, InApp, SMS, Push, WhatsApp, Telegram
    string Priority // Low, Medium, High
);

public sealed record BroadcastNotificationRequest(
    string Title,
    string Message,
    string NotificationType,
    string Channel,
    string Priority
);

public sealed record CreateTemplateRequest(
    string Code,
    string Name,
    string Subject,
    string Body,
    string Channel
);

public sealed record UpdateTemplateRequest(
    string Name,
    string Subject,
    string Body,
    string Channel
);
