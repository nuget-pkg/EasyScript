#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`

#echo "globalThis.exports = {};" > babel-parser.mjs
#cat https_cdn.jsdelivr.net_npm_@babel-parser@7.28.5_lib_index.js >> babel-parser.mjs
#echo "export { parse };" >> babel-parser.mjs

echo "globalThis.exports = {};" > babel-parser.mjs
cat https_cdn.jsdelivr.net_npm_@babel-parser@7.28.6_lib_index.js >> babel-parser.mjs
echo "export { parse };" >> babel-parser.mjs
./parse.mjs
