﻿// Copyright (c) Fantasy Copilot. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FantasyCopilot.DI.Container;
using FantasyCopilot.Models.App.Workspace;
using FantasyCopilot.Toolkits.Interfaces;
using FantasyCopilot.ViewModels.Interfaces;

namespace FantasyCopilot.ViewModels;

/// <summary>
/// Image skills module view model.
/// </summary>
public sealed partial class ImageSkillsModuleViewModel : ViewModelBase, IImageSkillsModuleViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSkillsModuleViewModel"/> class.
    /// </summary>
    public ImageSkillsModuleViewModel(
        ICacheToolkit cacheToolkit,
        IImageSkillEditModuleViewModel editModuleVM)
    {
        _cacheToolkit = cacheToolkit;
        _editModuleVM = editModuleVM;
        Skills = new ObservableCollection<ImageSkillConfig>();
        Skills.CollectionChanged += OnSkillsCollectionChanged;
        _cacheToolkit.ImageSkillListChanged += OnImageSkillListChanged;
        AttachIsRunningToAsyncCommand(p => IsLoading = p, InitializeCommand);
        CheckIsEmpty();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (IsLoading || _isInitialized)
        {
            return;
        }

        TryClear(Skills);

        var skills = await _cacheToolkit.GetImageSkillsAsync();
        foreach (var skill in skills)
        {
            Skills.Add(skill);
        }

        _isInitialized = true;
    }

    [RelayCommand]
    private void EditConfig(ImageSkillConfig config)
    {
        _editModuleVM.InjectData(config);
        IsEditing = true;
    }

    [RelayCommand]
    private async Task DeleteConfigAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var source = Skills.FirstOrDefault(p => p.Id == id);
        if (source == null)
        {
            return;
        }

        Skills.Remove(source);
        await _cacheToolkit.DeleteImageSkillAsync(id);
    }

    private void CheckIsEmpty()
        => IsEmpty = Skills.Count == 0;

    private void OnImageSkillListChanged(object sender, EventArgs e)
    {
        _isInitialized = false;
        InitializeCommand.Execute(default);
    }

    private void OnSkillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        => CheckIsEmpty();

    partial void OnIsEditingChanged(bool value)
    {
        var appVM = Locator.Current.GetService<IAppViewModel>();
        appVM.IsBackButtonShown = value;
    }
}
