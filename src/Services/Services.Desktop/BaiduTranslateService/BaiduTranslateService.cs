﻿// Copyright (c) Fantasy Copilot. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FantasyCopilot.DI.Container;
using FantasyCopilot.Models.App;
using FantasyCopilot.Services.Interfaces;
using FantasyCopilot.Toolkits.Interfaces;
using Windows.Storage;

namespace FantasyCopilot.Services;

/// <summary>
/// Baidu translate service.
/// </summary>
public sealed partial class BaiduTranslateService : ITranslateService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaiduTranslateService"/> class.
    /// </summary>
    public BaiduTranslateService()
    {
        _settingsToolkit = Locator.Current.GetService<ISettingsToolkit>();
        _httpClient = new HttpClient();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<LocaleInfo>> GetSupportLanguagesAsync()
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/BaiduTranslateSupportLanguages.json"));
        var json = await FileIO.ReadTextAsync(file);
        var data = JsonSerializer.Deserialize<List<string>>(json);
        return data.Select(p => new LocaleInfo(new CultureInfo(p))).ToList();
    }

    /// <inheritdoc/>
    public void ReloadConfig()
    {
        CheckConfig();
        _salt = new Random().Next(100000).ToString();
    }

    /// <inheritdoc/>
    public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(sourceLanguage))
        {
            sourceLanguage = "auto";
        }

        var dict = new Dictionary<string, string>
        {
            { "q", text },
            { "from", sourceLanguage },
            { "to", targetLanguage },
            { "appid", _appId },
            { "salt", _salt },
            { "sign", GenerateSign(text) },
        };

        var content = new FormUrlEncodedContent(dict);
        var request = new HttpRequestMessage(HttpMethod.Post, TranslateApi);
        request.Content = content;
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        var jele = JsonSerializer.Deserialize<JsonElement>(result);
        if (jele.TryGetProperty("error_msg", out var errorEle))
        {
            throw new Exception(errorEle.GetRawText());
        }
        else
        {
            var hasResult = jele.TryGetProperty("trans_result", out var transResultEle);
            if (!hasResult)
            {
                throw new Exception("No result.");
            }

            transResultEle.EnumerateArray().FirstOrDefault().TryGetProperty("dst", out var dstEle);
            return dstEle.ToString();
        }
    }
}
