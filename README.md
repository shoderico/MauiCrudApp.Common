# MauiCrudApp.Common

**MauiCrudApp.Common** is a sleek, modular .NET MAUI library engineered to supercharge the development of cross-platform CRUD applications. Designed for developers who value clean architecture and reusable components, this library provides a robust foundation for navigation, change tracking, dialog management, and view-model orchestration. Say goodbye to boilerplate code and hello to a streamlined, scalable development experience.

## ✨ Key Features

- **Unified Navigation**: Seamlessly manage navigation with dependency-injected view models, complete with parameter passing for effortless initialization.
- **Smart Edit Protection**: Warn users before discarding unsaved changes with customizable dialogs triggered by back navigation or swipe gestures.
- **Efficient Change Tracking**: Track data modifications with minimal effort using powerful utilities like `ChangeTracker` and `ObservableDictionary`.
- **Reactive Data Binding**: Leverage `ObservableDictionary` for dynamic, UI-responsive key-value collections.
- **Extensible Base Classes**: Build on `ShellBase`, `ViewModelBase`, and `PageBase` for consistent, maintainable code.
- **Dialog Management**: Display cross-platform alerts with ease using a unified `DialogService`.

## 📂 Project Structure

```
MauiCrudApp.Common/
├── src/
│   ├── MauiCrudApp.Common/
│   │   ├── Interfaces/
│   │   │   ├── IEditableViewModel.cs        # Defines editable view-model contract
│   │   │   ├── IInitialize.cs               # Standardizes async initialization
│   │   │   ├── IDialogService.cs            # Dialog service interface
│   │   ├── Utilities/
│   │   │   ├── ChangeTracker.cs             # Tracks changes in data models
│   │   │   ├── ObservableDictionary.cs      # Reactive dictionary for UI updates
│   │   │   ├── TrackChangesAttribute.cs     # Marks properties for change tracking
│   │   ├── Navigation/
│   │   │   ├── INavigationService.cs        # Navigation service contract
│   │   │   ├── NavigationService.cs         # Implements navigation with DI
│   │   │   ├── INavigationParameterStore.cs # Stores navigation parameters
│   │   │   ├── NavigationParameterStore.cs  # Manages parameter passing
│   │   ├── Services/
│   │   │   ├── DialogService.cs             # Handles cross-platform dialogs
│   │   ├── Controls/
│   │   │   ├── ShellBase.cs                 # Base class for MAUI shells
│   │   ├── ViewModels/
│   │   │   ├── ViewModelBase.cs             # Base view-model with initialization
│   │   ├── Views/
│   │   │   ├── PageBase.cs                  # Base class for pages
│   │   ├── MauiCrudApp.Common.csproj        # Project file
```

## 🖥️ Example Project

The **MauiCrudApp.Example** project showcases how to use **MauiCrudApp.Common** to build a fully functional CRUD application for managing items. It demonstrates navigation, change tracking, and edit protection in a clean, modular structure.

### Structure

```
example/
├── MauiCrudApp.Example/
│   ├── Core/
│   │   ├── Models/
│   │   │   ├── Item.cs                    # Data model with change-tracked properties
│   │   ├── Services/
│   │   │   ├── IItemService.cs            # Interface for item CRUD operations
│   │   │   ├── ItemService.cs             # In-memory item service implementation
│   ├── Features/
│   │   ├── ItemListEidt/
│   │   │   ├── ViewModels/
│   │   │   │   ├── ItemListViewModel.cs  # View model for listing and searching items
│   │   │   │   ├── ItemListParameter.cs  # Navigation parameter for list page
│   │   │   │   ├── ItemEditViewModel.cs  # View model for editing/adding items
│   │   │   │   ├── ItemEditParameter.cs  # Navigation parameter for edit page
│   │   │   ├── Views/
│   │   │   │   ├── ItemListPage.xaml     # XAML for item list UI
│   │   │   │   ├── ItemListPage.xaml.cs  # Code-behind for item list page
│   │   │   │   ├── ItemEditPage.xaml     # XAML for item edit UI
│   │   │   │   ├── ItemEditPage.xaml.cs  # Code-behind for item edit page
│   │
│   ├── App.xaml                             # Application XAML
│   ├── App.xaml.cs                          # Application startup logic
│   ├── AppShell.xaml                        # Shell XAML for navigation
│   ├── AppShell.xaml.cs                     # Code-behind for shell with edit protection
│   ├── MauiProgram.cs                       # DI and app configuration
│   ├── MauiCrudApp.Example.csproj           # Project file
```

### Overview
- **Core**: Contains the `Item` model with `[TrackChanges]` properties and an `IItemService`/`ItemService` pair for in-memory CRUD operations.
- **Features**: Organizes functionality into `ItemList` (list and search items) and `ItemEdit` (create/edit items) modules, each with view models and views.
- **App Setup**: `MauiProgram.cs` configures dependency injection, while `AppShell` and `App` set up the navigation structure using `ShellBase`.
- **Purpose**: The Example project serves as a practical guide, showing how to integrate the library’s navigation, change tracking, and edit protection features into a real-world app.

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 or later
- .NET MAUI workload installed
- Visual Studio 2022 or later (recommended)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/shoderico/MauiCrudApp.Common.git
   ```

2. Build the solution:
   ```bash
   dotnet build src/MauiCrudApp.Common/MauiCrudApp.Common.csproj
   ```

3. Reference the library in your .NET MAUI project:
   - Add a project reference to `MauiCrudApp.Common.csproj` or install the NuGet package (coming soon!).

### Usage

**MauiCrudApp.Common** empowers developers to build robust .NET MAUI applications with minimal setup. The Example project demonstrates how to integrate the library to create a fully functional CRUD app for managing items. Below, we walk through key scenarios using the Example project's code, leveraging `PageBase` for pages and `IEditableViewModel` for change protection.

#### 1. **Set Up Dependency Injection**
Configure your app's services in `MauiProgram.cs` to register the library's navigation, dialog services, and your app-specific services, view models, and pages.

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        // Register Common services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<INavigationParameterStore, NavigationParameterStore>();
        builder.Services.AddSingleton<IDialogService, DialogService>();

        // Register app-specific services
        builder.Services.AddSingleton<IItemService, ItemService>();

        // Register view models
        builder.Services.AddTransient<ItemListViewModel>();
        builder.Services.AddTransient<ItemEditViewModel>();

        // Register views
        builder.Services.AddTransient<ItemListPage>();
        builder.Services.AddTransient<ItemEditPage>();

        // Register shell and app
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        return builder.Build();
    }
}
```

#### 2. **Configure the App Shell**
Use `ShellBase` to create an `AppShell` that handles navigation and protects unsaved changes with dialog prompts.

```csharp
public partial class AppShell : ShellBase
{
    public AppShell(IDialogService dialogService) : base(dialogService)
    {
        InitializeComponent();
        // Set dialog properties (optional)
        DialogTitle = "Unsaved Changes";
        DialogMessage = "You have unsaved changes. Discard them?";
        DialogAccept = "Discard";
        DialogCancel = "Stay";
    }
}
```

In `App.xaml.cs`, set the `AppShell` as the main page:

```csharp
public partial class App : Application
{
    public App(AppShell appShell)
    {
        InitializeComponent();
        MainPage = appShell;
    }
}
```

#### 3. **Create a List Page with Navigation**
Build a page to list items, deriving from `PageBase` for automatic view-model initialization. The `ItemListPage` displays items and allows searching, editing, and adding new items.

**ViewModel (`ItemListViewModel.cs`)**:
```csharp
public partial class ItemListViewModel : ViewModelBase<ItemListParameter>
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IItemService _itemService;

    public ItemListViewModel(
          INavigationParameterStore parameterStore
        , INavigationService navigationService
        , IDialogService dialogService
        , IItemService itemService) 
        : base(parameterStore)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _itemService = itemService;
        Items = new ObservableCollection<Item>();
    }

    [ObservableProperty]
    private ObservableCollection<Item> Items;

    [ObservableProperty]
    private string SearchText;

    public override async Task InitializeAsync(ItemListParameter parameter)
    {
        SearchText = parameter?.SearchText;
        await LoadItemsAsync();
    }

    [RelayCommand]
    private async Task AddItem()
    {
        var parameter = new ItemEditParameter { IsNew = true, ListItems = Items };
        await _navigationService.PushAsync(typeof(ItemEditPage), parameter);
    }

    private async Task LoadItemsAsync()
    {
        var items = await _itemService.GetItemsAsync(SearchText);
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    }
}
```

**Page (`ItemListPage.xaml.cs`)**:
```csharp
public partial class ItemListPage : PageBase
{
    public ItemListPage(ItemListViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

**XAML (`ItemListPage.xaml`)**:
```xaml
<commonViews:PageBase
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:commonViews="clr-namespace:MauiCrudApp.Common.Views;assembly=MauiCrudApp.Common"
    xmlns:vm="clr-namespace:MauiCrudApp.Example.Features.ItemListEdit.ViewModels">
    <StackLayout>
        <SearchBar Text="{Binding SearchText}" SearchButtonPressed="{Binding SearchCommand}" />
        <CollectionView ItemsSource="{Binding Items}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <StackLayout>
                        <Label Text="{Binding Name}" />
                        <Label Text="{Binding Description}" />
                        <Button Text="Edit" Command="{Binding Source={RelativeSource AncestorType={x:Type vm:ItemListViewModel}}, Path=EditItemCommand}" CommandParameter="{Binding .}" />
                        <Button Text="Delete" Command="{Binding Source={RelativeSource AncestorType={x:Type vm:ItemListViewModel}}, Path=DeleteItemCommand}" CommandParameter="{Binding .}" />
                    </StackLayout>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        <Button Text="Add Item" Command="{Binding AddItemCommand}" />
    </StackLayout>
</commonViews:PageBase>
```

#### 4. **Implement an Edit Page with Change Tracking**
Create an edit page that tracks changes and prevents accidental data loss. The `ItemEditPage` allows users to create or update items, with `ItemEditViewModel` implementing `IEditableViewModel` to manage editing state and trigger warnings when navigating away from unsaved changes.

**ViewModel (`ItemEditViewModel.cs`)**:
```csharp
public partial class ItemEditViewModel : ViewModelBase<ItemEditParameter>, IEditableViewModel, IDisposable
{
    private readonly INavigationService _navigationService;
    private readonly IItemService _itemService;
    private ChangeTracker _itemChangeTracker;
    private bool _hasChanges;
    private bool _changesHandled;

    public ItemEditViewModel(
          INavigationParameterStore parameterStore
        , INavigationService navigationService
        , IItemService itemService) 
        : base(parameterStore)
    {
        _navigationService = navigationService;
        _itemService = itemService;
    }

    [ObservableProperty]
    private Item Item;

    [ObservableProperty]
    private bool IsNew;

    public bool IsEditing => _itemChangeTracker?.HasChanges == true && !_changesHandled;

    public override async Task InitializeAsync(ItemEditParameter parameter)
    {
        IsNew = parameter.IsNew;
        var originalItem = IsNew ? new Item() : await _itemService.GetItemByIdAsync(parameter.ItemId);
        Item = new Item { Id = originalItem.Id, Name = originalItem.Name, Description = originalItem.Description };

        _itemChangeTracker = new ChangeTracker(typeof(Item), Item);
        _itemChangeTracker.TrackProperty(this, nameof(Item), () => _hasChanges = _itemChangeTracker.HasChanges);
    }

    [RelayCommand]
    private async Task Save()
    {
        if (IsNew)
            await _itemService.AddItemAsync(Item);
        else
            await _itemService.UpdateItemAsync(Item);

        _changesHandled = true;
        await _navigationService.GoBackAsync();
    }

    public void Dispose()
    {
        _itemChangeTracker?.Dispose();
    }
}
```

**Model (`Item.cs`)**:
```csharp
public partial class Item : ObservableObject
{
    [ObservableProperty]
    private int Id;

    [ObservableProperty]
    [property: TrackChanges]
    private string Name;

    [ObservableProperty]
    [property: TrackChanges]
    private string Description;
}
```

**Page (`ItemEditPage.xaml.cs`)**:
```csharp
public partial class ItemEditPage : PageBase
{
    public ItemEditPage(ItemEditViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

**XAML (`ItemEditPage.xaml`)**:
```xaml
<commonViews:PageBase
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:commonViews="clr-namespace:MauiCrudApp.Common.Views;assembly=MauiCrudApp.Common"
    xmlns:converters="clr-namespace:MauiCrudApp.Example.Converters">
    <StackLayout>
        <Label Text="{Binding IsNew, Converter={StaticResource BoolToStringConverter}}" />
        <Entry Text="{Binding Item.Name}" Placeholder="Name" />
        <Entry Text="{Binding Item.Description}" Placeholder="Description" />
        <Button Text="Save" Command="{Binding SaveCommand}" />
        <Button Text="Cancel" Command="{Binding CancelCommand}" />
    </StackLayout>
</commonViews:PageBase>
```

**Automatic Change Tracking with `TrackChanges`**:
The `Item` model's `Name` and `Description` properties are marked with `[property: TrackChanges]`, enabling `ChangeTracker` to automatically monitor changes. This eliminates manual change detection, as `ChangeTracker` compares initial and current values, updating `IsEditing` via the `TrackProperty` callback. Combined with `IEditableViewModel`, this ensures `ShellBase` displays a dialog when users attempt to navigate away (e.g., via back button or swipe) with unsaved changes, preventing accidental data loss. This seamless integration delivers a polished UX with minimal developer effort.

## 🧩 Visual Studio Templates

To make starting with **MauiCrudApp.Common** even easier, we provide **Visual Studio Project and Item Templates** to scaffold your projects and features effortlessly. These templates are located in the `template/` directory and can be installed for use in Visual Studio.

### 📂 Template Directory Structure

```
template/
├── MauiCrudApp.ProjectTemplate/
│   ├── MauiCrudApp.ProjectTemplate.zip  # Project template for new CRUD apps
│   ├── ...
├── MauiCrudApp.ItemTemplate/
│   ├── MauiCrudApp.ItemTemplate.zip    # Item template for adding new features
│   ├── ...
```

### 🚀 Using the Templates

#### 1. **Install the Templates**
To use the templates in Visual Studio, copy the ZIP files to the appropriate Visual Studio template directories:

- **Project Template**:
  ```bash
  %USERPROFILE%\Documents\Visual Studio <version>\Templates\ProjectTemplates
  ```
- **Item Template**:
  ```bash
  %USERPROFILE%\Documents\Visual Studio <version>\Templates\ItemTemplates
  ```

Replace `<version>` with your Visual Studio version (e.g., 2022). Once copied, the templates will appear in Visual Studio’s **New Project** and **Add New Item** dialogs.

#### 2. **Set Up a New Project with the Project Template**
The **MauiCrudApp.ProjectTemplate** creates a new .NET MAUI project preconfigured to work with **MauiCrudApp.Common**. After creating the project in Visual Studio, follow these steps to integrate the library:

- **Clone the MauiCrudApp.Common Repository**:
  Clone the **MauiCrudApp.Common** repository into a folder adjacent to your new project’s root directory, so the structure looks like this:
  ```
  ├── YourCreatedProject/
  │   ├── ...  # Your project files
  ├── MauiCrudApp.Common/
  │   ├── src/
  │   │   ├── MauiCrudApp.Common/
  │   │   │   ├── MauiCrudApp.Common.csproj  # The library project
  │   ├── ...
  ```

  Run the following command to clone the repository:
  ```bash
  git clone https://github.com/shoderico/MauiCrudApp.Common.git
  ```

- **Add a Project Reference**:
  In Visual Studio, add a reference to `MauiCrudApp.Common.csproj` in your project:
  1. Right-click on your project in Solution Explorer.
  2. Select **Add > Project Reference**.
  3. Browse to `MauiCrudApp.Common/src/MauiCrudApp.Common/MauiCrudApp.Common.csproj` and select it.

This sets up your project to leverage the library’s navigation, change tracking, and dialog features.

#### 3. **Add Features with the Item Template**
The **MauiCrudApp.ItemTemplate** lets you quickly add new features to the `Features/` folder of your project. When you add a new item named `Feature1` using the template, it generates a fully structured feature module:

```
├── YourCreatedProject/
│   ├── Features/
│   │   ├── Feature1/
│   │   │   ├── ViewModels/
│   │   │   │   ├── Feature1ViewModel.cs     # View model for the feature
│   │   │   │   ├── Feature1Parameter.cs     # Navigation parameter for the feature
│   │   │   ├── Views/
│   │   │   │   ├── Feature1Page.xaml        # XAML for the feature’s UI
│   │   │   │   ├── Feature1Page.xaml.cs     # Code-behind for the feature page
```

To add a new feature:
1. In Visual Studio, right-click the `Features/` folder in Solution Explorer.
2. Select **Add > New Item**.
3. Choose the **MauiCrudApp.ItemTemplate** and name it (e.g., `Feature1`).
4. The template generates the view model, navigation parameter, XAML page, and code-behind, ready to integrate with the library’s navigation and change-tracking features.

These templates streamline your workflow, letting you focus on building great apps with **MauiCrudApp.Common**! 🚀

## 🛠️ Contributing

We welcome contributions to make **MauiCrudApp.Common** even better! To contribute:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m "Add YourFeature"`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a pull request.

Please ensure your code follows the existing style and includes tests where applicable.

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 📬 Contact

Have questions or ideas? Open an issue on [GitHub](https://github.com/shoderico/MauiCrudApp.Common/issues) or reach out via [email](yanagida@shoderitz.com).

---

**MauiCrudApp.Common**: Empowering .NET MAUI developers to build smarter, faster, and cleaner CRUD apps. 🚀