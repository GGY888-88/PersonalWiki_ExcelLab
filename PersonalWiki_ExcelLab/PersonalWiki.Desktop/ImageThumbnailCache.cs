namespace PersonalWiki.Desktop;

/// <summary>Small detached-bitmap LRU cache. Returned bitmaps are caller-owned copies.</summary>
internal sealed class ImageThumbnailCache : IDisposable
{
    private sealed record Entry(Bitmap Bitmap, long Length, DateTime LastWriteUtc);
    private readonly int _capacity;
    private readonly Dictionary<string, (Entry Entry, LinkedListNode<string> Node)> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lru = new();
    private bool _disposed;

    public ImageThumbnailCache(int capacity = 48)
    {
        if (capacity is < 1 or > 512) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
    }

    public Bitmap? Get(string path)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var full = Path.GetFullPath(path);
        var info = new FileInfo(full);
        if (!info.Exists) return null;
        if (_items.TryGetValue(full, out var cached))
        {
            if (cached.Entry.Length == info.Length && cached.Entry.LastWriteUtc == info.LastWriteTimeUtc)
            {
                _lru.Remove(cached.Node); _lru.AddFirst(cached.Node);
                return new Bitmap(cached.Entry.Bitmap);
            }
            Remove(full);
        }

        Bitmap detached;
        try
        {
            using var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var source = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: true);
            detached = new Bitmap(source);
        }
        catch { return null; }

        var stored = new Bitmap(detached);
        var node = _lru.AddFirst(full);
        _items[full] = (new Entry(stored, info.Length, info.LastWriteTimeUtc), node);
        while (_items.Count > _capacity)
        {
            var oldest = _lru.Last;
            if (oldest is null) break;
            Remove(oldest.Value);
        }
        return detached;
    }

    private void Remove(string path)
    {
        if (!_items.Remove(path, out var item)) return;
        _lru.Remove(item.Node);
        item.Entry.Bitmap.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var item in _items.Values) item.Entry.Bitmap.Dispose();
        _items.Clear(); _lru.Clear();
    }
}
