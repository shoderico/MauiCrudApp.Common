using CommunityToolkit.Mvvm.ComponentModel;
using MauiCrudApp.Common.Utilities;

namespace MauiCrudApp.Example.Core.Models;

public partial class Item : ObservableObject
{
    [ObservableProperty]
    private int id = 0;

    [ObservableProperty]
    [property: TrackChanges]
    private string name = "";

    [ObservableProperty]
    [property: TrackChanges]
    private string description = "";

}

