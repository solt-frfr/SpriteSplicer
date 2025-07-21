#!/bin/bash

RIDS=("win-x64" "win-arm64" "win-arm" "win-x86" "linux-x64" "linux-arm64" "linux-arm" "linux-musl-x64" "osx-x64" "osx-arm64")
PROJECT_PATH="./SpriteSplicer/SpriteSplicer.csproj"
CONFIG="Release"
OUTDIR="./publish"

for RID in "${RIDS[@]}"; do
  echo "Publishing for $RID..."
  dotnet publish "$PROJECT" \
    -c "$CONFIG" \
    -r "$RID" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -o "$OUTDIR/$RID"
done