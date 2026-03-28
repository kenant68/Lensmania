using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Soenneker.Blazor.Masonry;
using LensmaniaLibrary.DTOs.Posts;

namespace LensmaniaClient.Pages.Posts;

public partial class Posts : ComponentBase, IAsyncDisposable
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private List<PostResponse> _posts = [];
    private int? _cursor = null;
    private bool _hasMore = true;
    private bool _isLoading = false;
	private bool _hasError = false;
	private int _imagesLoaded = 0;
	private int _imagesToLoad = 0;
	private bool _isMasonryInitialized = false;

	private Masonry? _masonry;
    private ElementReference _sentinel;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Posts>? _dotNetRef;

	// Runs after each render. On first render : it loads first posts and sets up the sentinel <div>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMorePosts();

            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/infiniteScroll.js");

            await _jsModule.InvokeVoidAsync("observe", _sentinel, _dotNetRef);
        }
    }

    // Calld by JS when sentinel is visible : triggers loading of more posts
    [JSInvokable]
    public async Task OnSentinelVisible()
    {
        if (_isLoading || !_hasMore || _imagesToLoad > 0) return;
        await LoadMorePosts();
        StateHasChanged();
    }

	// Loads the next batch of posts and updates component state
    private async Task LoadMorePosts()
    {
		try {
			_isLoading = true;
        	_hasError = false;

			var url = $"api/posts?limit=10";
			if (_cursor.HasValue)
    			url += $"&cursor={_cursor}";

        	var result = await Http.GetFromJsonAsync<PaginatedPosts>(url);

        	if (result != null)
        	{
            	_posts.AddRange(result.Posts);
            	_cursor = result.NextCursor;
            	_hasMore = result.HasMore;
            	_imagesToLoad = result.Posts.Count;
            	_imagesLoaded = 0;
				_isMasonryInitialized = false;
        	}
		} 
		catch (Exception e)
    	{
        	_hasError = true;
    		Console.Error.WriteLine($"Erreur de chargement des posts : {e.Message}");
    	}      
		finally
		{
        	_isLoading = false;
        	StateHasChanged();
		}
    }

	// Is called every time an image is loaded, to ensure that masonry layout is applied when all images are ready
	private async Task OnImageLoaded()
	{
    	if (_isMasonryInitialized) return;
		
		_imagesLoaded++;

    	if (_imagesToLoad > 0 && _imagesLoaded >= _imagesToLoad)
    	{
			_isMasonryInitialized = true;        	
			if (_masonry != null)
        	{
            	var scrollY = await _jsModule!.InvokeAsync<double>("getScrollY");
            	await _masonry.Init();
            	await _jsModule!.InvokeVoidAsync("scrollTo", 0, scrollY);
        	}
        	_imagesLoaded = 0;
			_imagesToLoad = 0;
    	}
	}

    // Cleans up JS interop and .NET references when component is removed
    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
            await _jsModule.InvokeVoidAsync("unobserve");

        _dotNetRef?.Dispose();
    }
}
