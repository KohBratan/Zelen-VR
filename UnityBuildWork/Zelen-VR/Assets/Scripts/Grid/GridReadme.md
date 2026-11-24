# Grid drawing module

This folder contains the self-contained grid drawing and snapping prototype
for the Zelen-VR project. Nothing here is wired into existing scenes or
prefabs yet; the scripts are designed to be attached later without modifying
legacy assets.

## Architecture

- `Core` - coordinate, cell and settings value types plus pure conversion
  maths between world space and the discrete grid.
- `Runtime` - `GridManager` owns the sparse cell map, `GridRenderer` draws the
  lattice with LineRenderers and `GridDebugDrawer` exposes gizmos.
- `Input` - `IDrawInputSource` abstracts mouse, touch and XR tips;
  `DrawInputRouter` turns samples into a `DrawStroke` with optional smoothing.
- `Raster` - `GridRasterizer` converts strokes into cells using Bresenham or a
  supercover line, `GridBrush` defines the footprint, `GridLayer`/`GridCanvas`
  store the result and `GridRasterStats` records diagnostics.
- `Snapping` - `SnapProfile` is a shared asset, `GridSnapProvider` quantizes
  transforms and `SnapAnchor` applies snapping to individual objects.
- `History` - reversible `IGridCommand` implementations and a small undo/redo
  stack in `GridCommandHistory`.
- `Serialization` - DTOs and `GridSerializer` for JSON saves under
  `Application.persistentDataPath`.
- `Utils` - palette asset, general helpers and validation reports.
- `Editor` - menu items under `Tools/ZelenVR/Grid` for creating assets and
  validating a scene. The file is wrapped in `#if UNITY_EDITOR`.

## Typical wiring (later)

1. Create a `GridSettings` asset and assign it to a `GridManager`.
2. Add `GridRenderer` to the same object to visualise the lattice.
3. Add `PointerDrawInputSource` or `ControllerDrawInputSource` plus
   `DrawInputRouter` to produce strokes.
4. Add `GridCanvas` and subscribe its `CellsChanged` event to gameplay code.
5. Add `GridSnapProvider` and a `SnapProfile` to snap props to the grid.
6. Add `GridCommandHistory` if undo/redo is required.

## Notes

- All runtime code lives in the `ZelenVR.Grid` namespace.
- No scene, prefab or existing project script is modified by this module.
- The renderer is intentionally simple; replace it with a chunked mesh or
  shader-based solution if a much larger grid is required.
- Snap offsets are applied in world space around the profile offset so props
  can keep their pivot while still aligning to cell centres.
