#!/bin/bash

# Configuration
PROJECT_NAME="Lunar"
RELEASE_DIR="release"
DEPENDS_DIR="$RELEASE_DIR/depends"
DOCS_DIR="$RELEASE_DIR/docs"

echo "🚀 Starting release process for $PROJECT_NAME..."

# 1. Build and Publish project
echo "📦 Publishing project..."
# Use publish to ensure all dependencies are gathered in one place
dotnet publish -c Release -o publish_temp

# 2. Setup directory structure
echo "📂 Setting up release folders..."
rm -rf $RELEASE_DIR
mkdir -p $DEPENDS_DIR
mkdir -p $DOCS_DIR

# 3. Copy binaries and config
echo "📋 Copying binaries and configuration..."
# Copy the main assembly
cp publish_temp/$PROJECT_NAME.dll $RELEASE_DIR/

# Move all other DLLs to the depends directory
mv publish_temp/*.dll $DEPENDS_DIR/ 2>/dev/null || true
# Move back the main assembly if it was moved (mv *.dll includes it)
[ -f $DEPENDS_DIR/$PROJECT_NAME.dll ] && mv $DEPENDS_DIR/$PROJECT_NAME.dll $RELEASE_DIR/

# Copy native raylib dependencies (runtimes)
cp -r publish_temp/runtimes $DEPENDS_DIR/ 2>/dev/null || true

cp version.json $RELEASE_DIR/

# Cleanup temp publish dir
rm -rf publish_temp

# 4. Copy documentation
echo "📄 Copying documentation..."
cp README.md $DOCS_DIR/
[ -f ENGINE.md ] && cp ENGINE.md $DOCS_DIR/
[ -f GEMINI.md ] && cp GEMINI.md $DOCS_DIR/

# 5. Create ZIP
echo "🗜️ Creating ZIP package..."
zip -r ${PROJECT_NAME}_Release.zip $RELEASE_DIR

echo "✅ Release package created: ${PROJECT_NAME}_Release.zip"
