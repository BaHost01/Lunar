#!/bin/bash

# Configuration
PROJECT_NAME="Lunar"
RELEASE_DIR="release"
DEPENDS_DIR="$RELEASE_DIR/depends"
DOCS_DIR="$RELEASE_DIR/docs"

echo "🚀 Starting release process for $PROJECT_NAME..."

# 1. Build project
echo "📦 Building project..."
dotnet build -c Release

# 2. Setup directory structure
echo "📂 Setting up release folders..."
rm -rf $RELEASE_DIR
mkdir -p $DEPENDS_DIR
mkdir -p $DOCS_DIR

# 3. Copy binaries and config
echo "📋 Copying binaries and configuration..."
cp bin/Release/net9.0/$PROJECT_NAME.dll $RELEASE_DIR/
cp bin/Release/net9.0/MoonSharp.Interpreter.dll $DEPENDS_DIR/
cp bin/Release/net9.0/Spectre.Console*.dll $DEPENDS_DIR/
cp bin/Release/net9.0/Raylib-cs.dll $DEPENDS_DIR/
# Copy native raylib dependencies (runtimes)
cp -r bin/Release/net9.0/runtimes $DEPENDS_DIR/ 2>/dev/null || true
cp version.json $RELEASE_DIR/

# 4. Copy documentation
echo "📄 Copying documentation..."
cp README.md $DOCS_DIR/
[ -f ENGINE.md ] && cp ENGINE.md $DOCS_DIR/
[ -f GEMINI.md ] && cp GEMINI.md $DOCS_DIR/

# 5. Create ZIP
echo "🗜️ Creating ZIP package..."
zip -r ${PROJECT_NAME}_Release.zip $RELEASE_DIR

echo "✅ Release package created: ${PROJECT_NAME}_Release.zip"
