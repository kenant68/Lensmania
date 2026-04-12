let observer = null;

export function observe(sentinel, dotNetRef) {
    observer = new IntersectionObserver(async (entries) => {
        if (entries[0].isIntersecting) {
            try {
                await dotNetRef.invokeMethodAsync("OnSentinelVisible");
            } catch {
                // .NET side may be disposed or unavailable
            }
        }
    }, {
        // Triggers posts loading 200px before sentinel is visible (smoother UX)
        rootMargin: "0px 0px 200px 0px"
    });
    observer.observe(sentinel);
}

export function unobserve() {
    observer?.disconnect();
}

export function getScrollY() {
    return window.scrollY;
}

export function scrollTo(x, y) {
    window.scrollTo(x, y);
}
