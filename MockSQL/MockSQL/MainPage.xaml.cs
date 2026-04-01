using MockSQL.Entities;
using MockSQL.ViewModels;

namespace MockSQL;

public partial class MainPage : ContentPage
{
    private readonly ProductsViewModel _vm;
    private int? _activeCategoryId = null;

    public MainPage(ProductsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        SearchSpinner.IsVisible = true;
        SearchSpinner.IsRunning = true;

        await _vm.LoadAsync();

        BuildCategoryChips();
        RefreshCountBadge();

        SearchSpinner.IsVisible = false;
        SearchSpinner.IsRunning = false;

        ProductsCV.ItemsSource = _vm.Products;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PULL-TO-REFRESH
    // ═══════════════════════════════════════════════════════════════════════

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        _activeCategoryId = null;
        EntrySearch.Text  = string.Empty;
        await LoadDataAsync();
        Refresher.IsRefreshing = false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CATEGORY CHIPS
    // ═══════════════════════════════════════════════════════════════════════

    private void BuildCategoryChips()
    {
        ChipsLayout.Children.Clear();
        ChipsLayout.Children.Add(BuildChip("Barchasi", null));
        foreach (var cat in _vm.Categories)
            ChipsLayout.Children.Add(BuildChip(cat.Name, cat.Id));
    }

    private Border BuildChip(string label, int? categoryId)
    {
        bool isActive = _activeCategoryId == categoryId;

        var lbl = new Label
        {
            Text              = label,
            FontSize          = 12,
            FontAttributes    = isActive ? FontAttributes.Bold : FontAttributes.None,
            TextColor         = isActive ? Colors.White : Color.FromArgb("#64748B"),
            VerticalOptions   = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
        };

        var chip = new Border
        {
            BackgroundColor = isActive ? Color.FromArgb("#6366F1") : Color.FromArgb("#1E293B"),
            StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
            Stroke          = isActive ? Color.FromArgb("#818CF8") : Color.FromArgb("#334155"),
            Padding         = new Thickness(16, 7),
            Content         = lbl,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            _activeCategoryId = categoryId;
            BuildCategoryChips();

            if (categoryId is null)
            {
                await _vm.LoadAsync();
            }
            else
            {
                await _vm.FilterByCategoryAsync(categoryId.Value);
            }

            ProductsCV.ItemsSource = _vm.Products;
            RefreshCountBadge();
        };
        chip.GestureRecognizers.Add(tap);
        return chip;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SEARCH
    // ═══════════════════════════════════════════════════════════════════════

    private CancellationTokenSource? _searchCts;

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try { await Task.Delay(300, token); }
        catch (TaskCanceledException) { return; }

        var query = e.NewTextValue?.Trim() ?? string.Empty;

        SearchSpinner.IsRunning = true;
        SearchSpinner.IsVisible = true;

        if (string.IsNullOrEmpty(query))
        {
            _activeCategoryId = null;
            BuildCategoryChips();
            await _vm.LoadAsync();
        }
        else
        {
            await _vm.SearchAsync(query);
        }

        ProductsCV.ItemsSource  = _vm.Products;
        RefreshCountBadge();
        SearchSpinner.IsRunning = false;
        SearchSpinner.IsVisible = false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ADD
    // ═══════════════════════════════════════════════════════════════════════

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var prefill = EntryQuickAdd.Text?.Trim();
        EntryQuickAdd.Text = string.Empty;
        await PushFormPage(title: "Yangi mahsulot", product: null, prefillName: prefill);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════════════════

    private async void OnEditSwiped(object? sender, EventArgs e)
    {
        if (sender is SwipeItem { BindingContext: Product p })
            await PushFormPage(title: "Tahrirlash", product: p, prefillName: null);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════════════

    private async void OnDeleteSwiped(object? sender, EventArgs e)
    {
        if (sender is not SwipeItem { BindingContext: Product p }) return;

        bool ok = await DisplayAlert(
            "O'chirish",
            $"'{p.Name}' ni o'chirishni tasdiqlaysizmi?",
            "Ha, o'chir", "Bekor");

        if (!ok) return;

        await _vm.DeleteProductAsync(p.Id);
        ProductsCV.ItemsSource = _vm.Products;
        RefreshCountBadge();
        await ShowToastAsync($"'{p.Name}' o'chirildi");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FORM PAGE (Create / Edit)
    // ═══════════════════════════════════════════════════════════════════════

    private async Task PushFormPage(string title, Product? product, string? prefillName)
    {
        var page = BuildFormPage(title, product, prefillName);
        await Navigation.PushModalAsync(new NavigationPage(page)
        {
            BarBackgroundColor = Color.FromArgb("#0F172A"),
            BarTextColor       = Colors.White,
        });
    }

    private ContentPage BuildFormPage(string title, Product? product, string? prefillName)
    {
        var entryName = MakeEntry("Mahsulot nomi *",
            prefillName ?? product?.Name, Keyboard.Default);

        var entryDesc = MakeEntry("Tavsif (ixtiyoriy)",
            product?.Description, Keyboard.Default);

        var entryPrice = MakeEntry("Narx (so'm) *",
            product?.Price > 0 ? product.Price.ToString("F0") : null,
            Keyboard.Numeric);

        var entryStock = MakeEntry("Ombor (dona) *",
            product?.Stock > 0 ? product.Stock.ToString() : null,
            Keyboard.Numeric);

        var picker = new Picker
        {
            Title              = "Kategoriya tanlang",
            ItemsSource        = _vm.Categories,
            ItemDisplayBinding = new Binding("Name"),
            BackgroundColor    = Color.FromArgb("#1E293B"),
            TextColor          = Color.FromArgb("#F8FAFC"),
            TitleColor         = Color.FromArgb("#475569"),
            HeightRequest      = 48,
        };
        if (product != null)
            picker.SelectedItem = _vm.Categories.FirstOrDefault(c => c.Id == product.CategoryId);

        var lblError = new Label
        {
            TextColor = Color.FromArgb("#EF4444"),
            FontSize  = 12,
            IsVisible = false,
            Margin    = new Thickness(0, 4, 0, 0),
        };

        var btnSave = new Button
        {
            Text            = title == "Tahrirlash" ? "Saqlash" : "Qo'shish",
            BackgroundColor = Color.FromArgb("#6366F1"),
            TextColor       = Colors.White,
            CornerRadius    = 14,
            HeightRequest   = 52,
            FontAttributes  = FontAttributes.Bold,
            FontSize        = 16,
        };

        var btnCancel = new Button
        {
            Text            = "Bekor",
            BackgroundColor = Color.FromArgb("#1E293B"),
            TextColor       = Color.FromArgb("#94A3B8"),
            CornerRadius    = 14,
            HeightRequest   = 52,
        };

        btnSave.Clicked += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(entryName.Text))
            { lblError.Text = "Mahsulot nomi kiritilishi shart!"; lblError.IsVisible = true; return; }

            if (!decimal.TryParse(entryPrice.Text, out var price) || price <= 0)
            { lblError.Text = "To'g'ri narx kiriting (masalan: 150000)"; lblError.IsVisible = true; return; }

            if (!int.TryParse(entryStock.Text, out var stock) || stock < 0)
            { lblError.Text = "To'g'ri ombor miqdori kiriting"; lblError.IsVisible = true; return; }

            if (picker.SelectedItem is not Category selectedCat)
            { lblError.Text = "Kategoriya tanlanishi shart!"; lblError.IsVisible = true; return; }

            lblError.IsVisible = false;
            btnSave.IsEnabled  = false;
            btnSave.Text       = "Saqlanmoqda...";

            var p = new Product
            {
                Id          = product?.Id ?? 0,
                Name        = entryName.Text.Trim(),
                Description = entryDesc.Text?.Trim(),
                Price       = price,
                Stock       = stock,
                CategoryId  = selectedCat.Id,
            };

            if (product is null)
                await _vm.CreateProductAsync(p);
            else
                await _vm.UpdateProductAsync(p);

            ProductsCV.ItemsSource = _vm.Products;
            RefreshCountBadge();

            await Navigation.PopModalAsync();
            await ShowToastAsync(product is null
                ? $"'{p.Name}' qo'shildi ✓"
                : $"'{p.Name}' yangilandi ✓");
        };

        btnCancel.Clicked += async (_, _) =>
            await Navigation.PopModalAsync();

        // ── FIX: GridRowsColumns o'rniga to'g'ridan string ColumnDefinitions ──
        var btnGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
            },
            ColumnSpacing = 12,
        };
        Grid.SetColumn(btnCancel, 0);
        Grid.SetColumn(btnSave,   1);
        btnGrid.Children.Add(btnCancel);
        btnGrid.Children.Add(btnSave);

        return new ContentPage
        {
            Title           = title,
            BackgroundColor = Color.FromArgb("#0F172A"),
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(24, 20),
                    Spacing = 6,
                    Children =
                    {
                        MakeFormLabel("MAHSULOT NOMI"),
                        entryName,
                        MakeFormLabel("TAVSIF"),
                        entryDesc,
                        MakeFormLabel("NARX (SO'M)"),
                        entryPrice,
                        MakeFormLabel("OMBOR"),
                        entryStock,
                        MakeFormLabel("KATEGORIYA"),
                        picker,
                        lblError,
                        new BoxView { HeightRequest = 8 },
                        btnGrid,
                    }
                }
            }
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static Entry MakeEntry(string placeholder, string? text, Keyboard keyboard) => new()
    {
        Placeholder      = placeholder,
        Text             = text ?? string.Empty,
        Keyboard         = keyboard,
        BackgroundColor  = Color.FromArgb("#1E293B"),
        TextColor        = Color.FromArgb("#F8FAFC"),
        PlaceholderColor = Color.FromArgb("#475569"),
        HeightRequest    = 48,
        Margin           = new Thickness(0, 0, 0, 4),
    };

    private static Label MakeFormLabel(string text) => new()
    {
        Text             = text,
        FontSize         = 10,
        CharacterSpacing = 2,
        TextColor        = Color.FromArgb("#475569"),
        FontAttributes   = FontAttributes.Bold,
        Margin           = new Thickness(0, 14, 0, 6),
    };

    private void RefreshCountBadge()
    {
        LblCount.Text = $"{_vm.Products.Count} ta";
    }

    private string? _toastText;
    private async Task ShowToastAsync(string msg)
    {
        _toastText    = msg;
        LblTitle.Text = msg;
        await Task.Delay(2200);
        if (_toastText == msg)
            LblTitle.Text = "Do'kon";
    }
}
