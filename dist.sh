#!/bin/bash

# Declare an array of RIDs
RIDS=("win-x64" "linux-x64" "osx-x64")

# Loop through each RID and publish
for RID in "${RIDS[@]}"; do 
    dotnet publish \
        -r $RID \
        -c Release \
        /p:PublishSingleFile=true \
        /p:PublishTrimmed=true \
        --self-contained true \
        -o "./publish/$RID"
done
