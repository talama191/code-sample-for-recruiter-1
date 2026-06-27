# Spatial Stacking Store

A small, self-contained C# sample that packs packages into storage **layer by layer** and
caches each computed **layer pattern**, so a large uniform fill costs a single packing
calculation instead of one per package.

It is a **clean-room, generic demonstration** of the technique — independently written, no
game- or product-specific code. The point is to show how I approach systems work: a cheap
hot path backed by pattern reuse, clear architecture, and testable units.

## The problem

Storage has a fixed footprint (width × length) but **unbounded height**. Packages of varying
sizes arrive in a stream and must be placed efficiently. In practice the same package sizes
recur constantly, so the same layer arrangement repeats over and over — recomputing it for
every package (or every layer) is wasted work.

## Approach

- **Layered fill.** Packages fill the current layer's footprint; when it's full, a new layer
  opens on top. Height grows with the layers.
- **First package sets the layer height**, floored at a configured minimum. Setting that
  minimum to the largest configured package guarantees any package fits a fresh layer
  height-wise — so only the footprint ever fills up, and "doesn't fit" always means "start a
  new layer."
- **Cache the layer pattern, not the placement.** A `LayerPattern` (the footprint slots a
  package fills, plus the layer height) is computed once per package size and reused. A
  uniform stream of 10,000 packages therefore performs **one** packing calculation; every
  subsequent layer of that size is a cache hit.
- **Pooled queries & low-overhead events.** A static `ListPool<T>` (`Get` / `Release` by `ref`)
  keeps the placement-snapshot query allocation-free. Events (`Placed`, `LayerStarted`) use
  ordinary `+=` / `-=` syntax but are backed by a `CallbackList<T>`, avoiding the delegate-array
  allocation a multicast event incurs on every subscribe / unsubscribe.

## Layout

| File | Responsibility |
|------|----------------|
| `src/PackageSize.cs` | Immutable package footprint/height + `PackagePlacement` |
| `src/Package.cs` | Package identity + size |
| `src/LayerPattern.cs` | A computed, reusable layer arrangement |
| `src/LayerPacker.cs` | Shelf-packs a uniform package into a layer pattern |
| `src/LayerPatternCache.cs` | One pattern per package size — the core optimization |
| `src/LayeredStorage.cs` | Fills layers, opens new ones, raises events |
| `src/ListPool.cs` | Static pool of reusable lists (`Get` / `Release` by `ref`) |
| `src/CallbackList.cs` | Allocation-light observer list |
| `Program.cs` | Runnable demo + self-check |

## Run

```bash
dotnet run
```

You'll see 10,000 packages fill hundreds of layers from a single computed pattern, a second
size add one more pattern, and a self-check confirming every placement is in bounds.

## Complexity & trade-offs

- Packing a layer is O(footprint / package footprint); with the cache, a uniform fill of N
  packages is **O(1) packing calculations** plus O(N) slot assignment.
- The packer handles **uniform** layers (the common, repeating case) as an exact grid. Mixed
  packages within one layer would need a fuller 2D bin-packer — a natural extension point;
  the cache and layered model stay the same.
- Pattern keys are package size, so memory is bounded by the number of distinct sizes.

## Notes

Original, generic code written for this sample. No third-party dependencies; targets .NET 8.
