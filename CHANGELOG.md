# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-preview] - 2018-11-16
### Added
- Total system overhaul from `SimpleCulling` (now legacy).
- Added support for defining custom `PortalOccluders`.
- Added `PortalAgent` component for defining where to calculate visibility for at runtime.
- Added support for multiple `PortalAgents`.
- Added `Hybrid` mode for `Volumes`. Generates automatic volume hierarchy as well as collecting user `Volumes`.
- Added menu items for creating `PortalSystems`, `PortalVolumes` and `PortalOccluders`.
- Added serialization of `OccluderData`, removing `Occluder Proxy` objects after bake.
- Added `Visibility` debug view for displaying all rays that hit during visibility baking.
- Added custom icons and gizmos.
- Added Lots of new tests.

### Changed

- Redefined `SimpleCulling` as `PortalSystem`.
- Redefined `Volume` as `PortalVolume`.
- Refactored all component UIs for better user experience.
- Redesigned data structures for proper serialization.
- Changed visibility data to a lookup table for better performance.
- Abstracted bake tools and UI to `IBake` interface.
- Abstracted a lot of logic to static util classes.

### Fixed

- Fixed a number of ray leaks in bake process.
- Fixed a leak of serialized data.
- Improved bake times.
- Improved runtime performance.