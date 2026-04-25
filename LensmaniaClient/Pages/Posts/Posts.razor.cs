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

    private string ApiBaseUrl => Http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
    private List<PostListItemResponse> _posts = [];
    private int? _cursor = null;
    private bool _hasMore = true;
    private bool _isLoading = false;
	private bool _hasError = false;
	private int _imagesLoaded = 0;
	private int _imagesToLoad = 0;
	private bool _isMasonryInitialized = false;
	private string _errorMessage = string.Empty;
	private Masonry? _masonry;
    private ElementReference _sentinel;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Posts>? _dotNetRef;

	// Runs after each render. On first render : it loads first posts and sets up the sentinel <div>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/infiniteScroll.js");

            await LoadMorePosts();
            // Waits for the re-render triggered by LoadMorePosts() to be finished
            await Task.Yield();

            if (_sentinel.Id != null && _hasMore)
            {
	            await _jsModule.InvokeVoidAsync("observe", _sentinel, _dotNetRef);
            }
        }
    }

    // Calld by JS when sentinel is visible : triggers loading of more posts
    [JSInvokable]
    public async Task OnSentinelVisible()
    {
        if (_isLoading || !_hasMore || _imagesToLoad > 0 || _hasError) return;
        await LoadMorePosts();
        StateHasChanged();
    }

	// Loads the next batch of posts and updates component state
    private async Task LoadMorePosts()
    {
		try {
			_isLoading = true;
        	_hasError = false;
			StateHasChanged();

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
		catch (HttpRequestException)
    	{
        	_hasError = true;
        	_errorMessage = "Une erreur est survenue lors du chargement des posts. Vérifier la connexion.";
    	}
		catch (Exception e)
    	{
        	_hasError = true;
			_errorMessage = "Erreur inattendue lors du chargement des posts.";
    		Console.Error.WriteLine($"Erreur : {e.Message}");
    	}      
		finally
		{
        	_isLoading = false;
        	StateHasChanged();
		}
    }

	// Called by @onload — photo path OK
	private async Task OnImageLoaded()
	{
		await OnImageSettled();
	}
	
	// Called by @onerror — broken photo path
	private async Task OnImageFailed()
	{
		await OnImageSettled();
	}
	
	// Is called every time an image is loaded or failed, to ensure that masonry layout is applied when all images are ready
	private async Task OnImageSettled()
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
		try 
		{
        	if (_jsModule != null)
			{
            	await _jsModule.InvokeVoidAsync("unobserve");
				await _jsModule.DisposeAsync();
			}
		}
        finally
    	{
        	_dotNetRef?.Dispose();
    	}
    }

	private void ResetPosts()
	{
    	_posts.Clear();
    	_cursor = null;
    	_hasMore = true;
	}
    
    private async Task HandlePostCreated(PostListItemResponse? post)
	{
		if (post is null) return;
 	
		_posts.Insert(0, post);
	    _imagesToLoad = 1;
    	_imagesLoaded = 0;
    	_isMasonryInitialized = false;
    	
		StateHasChanged();
    }
}
