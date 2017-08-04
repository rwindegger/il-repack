#!/bin/bash

pushd "$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"/..
export year=$(date +%Y)

export BUILD_VERSION=1.0.0
export BUILD_PRODUCT="dotnet-confuse"
export BUILD_CONFIGURATION="Release"
export BUILD_OUTPUT=$(pwd)\artifacts\${BUILD_CONFIGURATION}

export BUILD_COMPANY="WR"
export BUILD_COPYRIGHT="Copyright (c) ${BUILD_COMPANY} ${year}"

while [[ $# -gt 1 ]]
do
key="$1"

case $key in
    -v|--version)
    export BUILD_VERSION="$2"
    shift # past argument
    ;;
    -p|--product)
    export BUILD_PRODUCT="$2"
    shift # past argument
    ;;
    -c|--configuration)
    export BUILD_CONFIGURATION="$2"
    shift # past argument
    ;;
	-o|--output)
    export BUILD_OUTPUT="$2"
    shift # past argument
    ;;
	-r|--company)
	export BUILD_COMPANY="$2"
	export BUILD_COPYRIGHT="Copyright (c) ${BUILD_COMPANY} ${year}"
    shift # past argument
    ;;
    *)
            # unknown option
    ;;
esac
shift # past argument or value
done

dotnet restore
dotnet pack -c ${BUILD_CONFIGURATION} -o ${BUILD_OUTPUT} /p:Version=${BUILD_VERSION};Product=${BUILD_PRODUCT};Copyright=${BUILD_COPYRIGHT};Company=${BUILD_COMPANY};InformalVersion="v${BUILD_VERSION}"

popd