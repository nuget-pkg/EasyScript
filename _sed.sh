#! /usr/bin/env bash
#set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
version="${ts}"

cd $cwd/EasyScript
sed -i -e "s/<Version>.*<\/Version>/<Version>${version}<\/Version>/g" EasyScript.csproj
cd $cwd/
echo ${version}>version.txt
