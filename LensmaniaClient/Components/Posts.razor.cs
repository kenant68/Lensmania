using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Soenneker.Blazor.Masonry;
using LensmaniaLibrary.DTOs.Posts;
using LensmaniaClient.Services.Posts;
using LensmaniaLibrary.Enums;

namespace LensmaniaClient.Components;

public partial class Posts : ComponentBase, IAsyncDisposable
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private PostService _postService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;


    // Parameters
    [Parameter] public string? Username { get; set; }
    [Parameter] public bool DisplayCreateButton { get; set; } = false;
    [Parameter] public bool IsOwnProfile { get; set; } = false;
    [Parameter] public EventCallback<PostListItemResponse> OnPostCreated { get; set; }
    [Parameter] public bool ShowSortToggle { get; set; } = false;
    [Parameter] public int? EventId { get; set; }

    private PostSortOrder _sortOrder = PostSortOrder.DateDesc;
    private int _offset = 0;

    private string ApiBaseUrl => Http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
    private List<PostListItemResponse> _posts = [];
    private int? _cursor = null;
    private bool _hasMore = true;
    private bool _isLoading = false;
	private bool _hasError = false;
	private string _errorMessage = string.Empty;
    private readonly HashSet<int> _pendingLikes = new();
	
	// Masonry / images
	private Masonry? _masonry;
	private int _imagesLoaded = 0;
	private int _imagesToLoad = 0;
	private bool _isMasonryInitialized = false;
	private bool _needsMasonryLayout;
	
	// Infinite scroll
    private ElementReference _sentinel;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<Posts>? _dotNetRef;
    
    // Modal details
    private PostListItemResponse? _selectedPostItem = null;
    private PostResponse? _selectedPostDetailed = null;
    private bool _isLoadingDetail = false;
    

    // Manages Username changes
    protected override async Task OnParametersSetAsync()
    {
	    await ReloadAsync();
    }
    
	// Runs after each render. On first render : it loads first posts and sets up the sentinel <div>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
	    if (firstRender)
	    {
		    _dotNetRef = DotNetObjectReference.Create(this);
		    _jsModule = await JS.InvokeAsync<IJSObjectReference>(
			    "import", "./js/infiniteScroll.js");

		    if (_sentinel.Id != null && _hasMore)
			    await _jsModule.InvokeVoidAsync("observe", _sentinel, _dotNetRef);  
	    }
        
        if (_needsMasonryLayout && _masonry != null)
        {
	        _needsMasonryLayout = false;
        
	        await Task.Delay(30);
	        await _masonry.Init();
        }
    }

    // --- Infinite scroll ---
    // Calld by JS when sentinel is visible : triggers loading of more posts
    [JSInvokable]
    public async Task OnSentinelVisible()
    {
        if (_isLoading || !_hasMore || _imagesToLoad > 0 || _hasError) return;
        await LoadMorePosts();
        StateHasChanged();
    }

    // --- Loading ---
    
	// Loads the next batch of posts and updates component state
    private async Task LoadMorePosts()
    {
        try {
            _isLoading = true;
            _hasError = false;
            StateHasChanged();

            var isLikesSort = _sortOrder is PostSortOrder.LikesDesc or PostSortOrder.LikesAsc || EventId.HasValue;
            var sortParam = _sortOrder switch
            {
                PostSortOrder.DateAsc   => "date_asc",
                PostSortOrder.LikesDesc => "likes_desc",
                PostSortOrder.LikesAsc  => "likes_asc",
                _                       => "date_desc",
            };

            string url;
            if (EventId.HasValue)
            {
                url = $"api/posts?eventId={EventId.Value}&sort=likes_desc&limit=10";
                if (_offset > 0)
                    url += $"&offset={_offset}";
            }
            else if (!string.IsNullOrWhiteSpace(Username))
            {
                url = $"api/posts/{Uri.EscapeDataString(Username)}?limit=10&sort={sortParam}";
                if (!isLikesSort && _cursor.HasValue)
                    url += $"&cursor={_cursor}";
                else if (isLikesSort && _offset > 0)
                    url += $"&offset={_offset}";
            }
            else
            {
                url = $"api/posts?limit=10&sort={sortParam}";
                if (!isLikesSort && _cursor.HasValue)
                    url += $"&cursor={_cursor}";
                else if (isLikesSort && _offset > 0)
                    url += $"&offset={_offset}";
            }

            var result = await Http.GetFromJsonAsync<PaginatedPosts>(url);

            if (result != null)
            {
                _posts.AddRange(result.Posts);
                _hasMore = result.HasMore;
                _imagesToLoad += result.Posts.Count;
                _imagesLoaded = 0;
                _isMasonryInitialized = false;

                if (isLikesSort)
                    _offset = result.NextOffset ?? _offset;
                else
                    _cursor = result.NextCursor;
            }
        }
        catch (HttpRequestException)
        {
            _hasError = true;
            _errorMessage = "Erreur de connexion lors du chargement des posts.";
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
    
    private void ResetState()
    {
        _posts.Clear();
        _cursor = null;
        _hasMore = true;
        _imagesToLoad = 0;
        _imagesLoaded = 0;
        _isMasonryInitialized = false;
        _selectedPostItem = null;
        _offset = 0;
    }
    
    private async Task OnSortChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<PostSortOrder>(e.Value?.ToString(), out var sortOrder))
        {
            _sortOrder = sortOrder;
            await ReloadAsync();
        }
    }

    // Reload method exposed for parent pages
    public async Task ReloadAsync()
    {
	    ResetState();
	    await LoadMorePosts();
	    await Task.Yield();

	    // Observe sentinel after reload
	    if (_jsModule != null && _sentinel.Id != null && _hasMore)
		    await _jsModule.InvokeVoidAsync("observe", _sentinel, _dotNetRef);
    }
    
	// --- Masonry ---
	
	// Called by @onload — photo path OK
	private async Task OnImageLoaded() => await OnImageSettled();
	
	// Called by @onerror — broken photo path
	private async Task OnImageFailed() => await OnImageSettled();
	
	// Is called every time an image is loaded or failed, to ensure that masonry layout is applied when all images are ready
	private async Task OnImageSettled()
	{
    	if (_isMasonryInitialized) return;
		
		_imagesLoaded++;

    	if (_imagesToLoad > 0 && _imagesLoaded >= _imagesToLoad)
    	{
			_isMasonryInitialized = true;
			
			if (_masonry != null && _jsModule != null)
        	{
		        var scrollY = await _jsModule.InvokeAsync<double>("getScrollY");
            	await _masonry.Init();
	            await _jsModule.InvokeVoidAsync("scrollTo", 0, scrollY);
        	}
			
        	_imagesLoaded = 0;
			_imagesToLoad = 0;
    	}
	}
    
	// --- Post creation ---
    private async Task HandlePostCreated(PostListItemResponse? post)
	{
		if (post is null) return;

        _posts.Insert(0, post);
        
        if (_isMasonryInitialized)
        {
            // Previous batch already laid out : start a fresh single-image cycle.
            _imagesToLoad = 1;
            _imagesLoaded = 0;
            _isMasonryInitialized = false;
        }
        else
        {
            // A batch is still loading : add to the pending count.
            _imagesToLoad++;
        }
    	
        await OnPostCreated.InvokeAsync(post);
		StateHasChanged();
    }
    
    // Post details
    private async Task SelectPost(PostListItemResponse item)
    {
	    _selectedPostItem = item;
	    _selectedPostDetailed = null;
	    _isLoadingDetail = true;
	    StateHasChanged();
	    
	    var requestedPostId = item.Id;

	    try
	    {
		    var detail = await Http.GetFromJsonAsync<PostResponse>($"api/posts/{requestedPostId}");
		    
		    if (_selectedPostItem?.Id == requestedPostId)
			    _selectedPostDetailed = detail;
	    }
	    catch (Exception e)
	    {
		    Console.Error.WriteLine($"Erreur chargement détail : {e.Message}");
	    }
	    finally
	    {
		    if (_selectedPostItem?.Id == requestedPostId)
			    _isLoadingDetail = false;
		    
		    StateHasChanged();
	    }
    }

    private void CloseDetail()
    {
	    _selectedPostDetailed = null;
	    _selectedPostItem = null;
    }
    
    // --- Likes ---
    private async Task ToggleLike(PostListItemResponse post)
    {
        if (!_pendingLikes.Add(post.Id)) return;
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated != true)
            {
                Nav.NavigateTo("/login");
                return;
            }

            var index = _posts.IndexOf(post);
            if (index < 0) return;

            var newIsLiked = !post.IsLikedByCurrentUser;
            var newCount = newIsLiked ? post.LikesCount + 1 : Math.Max(0, post.LikesCount - 1);
            _posts[index] = post with { IsLikedByCurrentUser = newIsLiked, LikesCount = newCount };
            if (EventId.HasValue)
                _posts.Sort((a, b) => b.LikesCount.CompareTo(a.LikesCount));
            StateHasChanged();

            var success = await _postService.ToggleLikeAsync(post.Id);
            if (!success)
            {
                var currentIndex = _posts.FindIndex(p => p.Id == post.Id);
                if (currentIndex >= 0)
                {
                    _posts[currentIndex] = post;
                    if (EventId.HasValue)
                        _posts.Sort((a, b) => b.LikesCount.CompareTo(a.LikesCount));
                    StateHasChanged();
                }
            }
        }
        finally
        {
            _pendingLikes.Remove(post.Id);
        }
    }

    private static string FormatLikeCount(int count)
    {
        if (count < 1_000) return count.ToString();
        if (count < 1_000_000)
        {
            var k = count / 1_000.0;
            return k == Math.Floor(k) ? $"{(int)k}K" : $"{k:0.0}K".Replace('.', ',');
        }
        var m = count / 1_000_000.0;
        return m == Math.Floor(m) ? $"{(int)m}M" : $"{m:0.0}M".Replace('.', ',');
    }

    // --- Dispose ---
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
}
