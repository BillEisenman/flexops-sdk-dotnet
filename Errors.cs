// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk;

/// <summary>
/// Base exception for all FlexOps API errors.
/// </summary>
public class FlexOpsException : Exception
{
    /// <summary>HTTP status code returned by the API.</summary>
    public int StatusCode { get; set; }

    /// <summary>Machine-readable error code (e.g., "insufficient_funds").</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Validation or detail error messages.</summary>
    public string[]? Errors { get; set; }

    /// <summary>Initializes a new instance of <see cref="FlexOpsException"/>.</summary>
    public FlexOpsException() { }

    /// <summary>Initializes a new instance of <see cref="FlexOpsException"/> with a message.</summary>
    public FlexOpsException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="FlexOpsException"/> with a message and inner exception.</summary>
    public FlexOpsException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when authentication fails (401) or authorization is denied (403).
/// </summary>
public class FlexOpsAuthException : FlexOpsException
{
    /// <summary>Initializes a new instance of <see cref="FlexOpsAuthException"/>.</summary>
    public FlexOpsAuthException() { }

    /// <summary>Initializes a new instance of <see cref="FlexOpsAuthException"/> with a message.</summary>
    public FlexOpsAuthException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="FlexOpsAuthException"/> with a message and inner exception.</summary>
    public FlexOpsAuthException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when the API rate limit is exceeded (429).
/// </summary>
public class FlexOpsRateLimitException : FlexOpsException
{
    /// <summary>Number of seconds to wait before retrying.</summary>
    public int RetryAfter { get; set; }

    /// <summary>Initializes a new instance of <see cref="FlexOpsRateLimitException"/>.</summary>
    public FlexOpsRateLimitException() { }

    /// <summary>Initializes a new instance of <see cref="FlexOpsRateLimitException"/> with a message.</summary>
    public FlexOpsRateLimitException(string message) : base(message) { }

    /// <summary>Initializes a new instance of <see cref="FlexOpsRateLimitException"/> with a message and inner exception.</summary>
    public FlexOpsRateLimitException(string message, Exception innerException) : base(message, innerException) { }
}
