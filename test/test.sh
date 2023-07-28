#!/bin/bash

# Build the project
dotnet build ../h.csproj

# Read the CSV file and test
while IFS=, read -r a b
do
    c=$(dotnet run --project ../h.csproj "$a" "$b")
    echo "$c - $a - $b"
done < testcases.csv
