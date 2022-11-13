#!/bin/bash

if [[ $# -ne 0 ]] && [[ $# -ne 1 ]]; then
    echo "usage: $0 [output_path]"
    exit 1
fi

majorVer=$(cat VERSION)
lastVer=$(git tag --sort version:refname --list "$majorVer.*" | tail -n1)
if [[ -n "$lastVer" ]]; then
    newVer=(${lastVer//./ })
    newVer[-1]="$((${newVer[-1]}+1))"
    nextVer="${newVer[*]}"
    nextVer="${nextVer// /.}"
else
    nextVer="$majorVer.0"
fi

echo "MajorVer=$majorVer LastVer=$lastVer NextVer=$nextVer"

args="--configuration Release /property:Version=$nextVer"
if [[ -n "$1" ]]; then
    dotnet build $args --output "$1"
else
    dotnet build $args
fi
