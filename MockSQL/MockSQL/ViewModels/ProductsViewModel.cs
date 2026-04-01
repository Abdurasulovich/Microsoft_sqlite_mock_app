using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MockSQL.Entities;
using MockSQL.Services;

namespace MockSQL.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService  _productService;
    private readonly ICategoryService _categoryService;

    [ObservableProperty] private List<Product>  _products   = new();
    [ObservableProperty] private List<Category> _categories = new();
    [ObservableProperty] private bool           _isBusy;

    public ProductsViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService  = productService;
        _categoryService = categoryService;
    }

    // ── READ ──────────────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Products   = await _productService.GetAllAsync();
            Categories = await _categoryService.GetAllAsync();
        }
        finally { IsBusy = false; }
    }

    public async Task SearchAsync(string query)
    {
        IsBusy = true;
        try { Products = await _productService.SearchAsync(query); }
        finally { IsBusy = false; }
    }

    public async Task FilterByCategoryAsync(int categoryId)
    {
        IsBusy = true;
        try { Products = await _productService.GetByCategoryAsync(categoryId); }
        finally { IsBusy = false; }
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    // [RelayCommand] YO'Q — MainPage.xaml.cs dan to'g'ridan chaqiriladi
    public async Task CreateProductAsync(Product product)
    {
        await _productService.CreateAsync(product);
        await LoadAsync();
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public async Task UpdateProductAsync(Product product)
    {
        await _productService.UpdateAsync(product);
        await LoadAsync();
    }

    // UpdateStock — 2 parametr, [RelayCommand] ishlamaydi, oddiy metod sifatida
    public async Task UpdateStockAsync(int productId, int newStock)
    {
        await _productService.UpdateStockAsync(productId, newStock);
        await LoadAsync();
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    public async Task DeleteProductAsync(int productId)
    {
        await _productService.DeleteAsync(productId);
        await LoadAsync();
    }
}
