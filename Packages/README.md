# Packages

This folder defines Unity package dependencies for the project.

## Key Files

- `manifest.json` - Lists all required Unity packages and versions
- `packages-lock.json` - Lock file for dependency resolution (auto-generated)

## Included Packages

### Core Packages
- **TextMeshPro** (3.0.6) - Advanced text rendering
- **Visual Scripting** (1.8.0) - Node-based scripting
- **UGUI** (1.0.0) - UI system
- **Timeline** (1.7.4) - Cinematic sequencing

### Editor Tools
- **Visual Studio Editor** (2.0.18) - VS integration
- **VS Code Editor** (1.2.5) - VS Code integration
- **Rider** (3.0.24) - JetBrains Rider integration

### Development
- **Test Framework** (1.1.33) - Unit testing
- **Collab Proxy** (2.0.5) - Version control

### Platform Modules
All standard Unity modules (Physics, Audio, UI, etc.)

## Note

Unity will automatically download and install these packages on first project load. The `packages-lock.json` file is auto-generated and excluded from version control.
