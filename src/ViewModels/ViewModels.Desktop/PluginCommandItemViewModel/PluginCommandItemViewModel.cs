﻿// Copyright (c) Fantasy Copilot. All rights reserved.

using System.Collections.ObjectModel;
using System.Linq;
using FantasyCopilot.Models.App.Plugins;
using FantasyCopilot.Models.Constants;
using FantasyCopilot.Toolkits.Interfaces;
using FantasyCopilot.ViewModels.Interfaces;

namespace FantasyCopilot.ViewModels;

/// <summary>
/// Plugin command item view model.
/// </summary>
public sealed partial class PluginCommandItemViewModel : ViewModelBase, IPluginCommandItemViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginCommandItemViewModel"/> class.
    /// </summary>
    public PluginCommandItemViewModel(IResourceToolkit resourceToolkit)
    {
        Parameters = new ObservableCollection<InputParameter>();
        _resourceToolkit = resourceToolkit;
    }

    /// <inheritdoc/>
    public void InjectData(PluginCommand command)
    {
        Name = command.Name;
        Description = command.Description;
        NoParameter = !command.Parameters?.Any() ?? true;
        if (!NoParameter)
        {
            foreach (var item in command.Parameters)
            {
                Parameters.Add(item);
            }
        }

        Category = GetCategoryText(command.Category ?? "other");
    }

    private string GetCategoryText(string categoryName)
    {
        var key = categoryName.ToLower() switch
        {
            WorkflowConstants.FileGroupName => StringNames.File,
            WorkflowConstants.A11yGroupName => StringNames.Accessibility,
            WorkflowConstants.ImageGroupName => StringNames.Image,
            WorkflowConstants.TextGroupName => StringNames.Text,
            WorkflowConstants.KnowledgeGroupName => StringNames.KnowledgeBase,
            WorkflowConstants.NativeGroupName => StringNames.Native,
            WorkflowConstants.ToolGroupName => StringNames.Tool,
            WorkflowConstants.VideoGroupName => StringNames.Video,
            WorkflowConstants.VoiceGroupName => StringNames.Voice,
            WorkflowConstants.WebGroupName => StringNames.Web,
            _ => StringNames.Other,
        };

        return _resourceToolkit.GetLocalizedString(key);
    }
}
