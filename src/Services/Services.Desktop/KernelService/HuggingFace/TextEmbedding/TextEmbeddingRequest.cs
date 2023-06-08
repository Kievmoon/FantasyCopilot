﻿// Copyright (c) Fantasy Copilot. All rights reserved.
// <auto-generated />

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FantasyCopilot.Services;

/// <summary>
/// HTTP schema to perform embedding request.
/// </summary>
[Serializable]
public sealed class TextEmbeddingRequest
{
    /// <summary>
    /// Data to embed.
    /// </summary>
    [JsonPropertyName("inputs")]
    public IList<string> Input { get; set; } = new List<string>();
}