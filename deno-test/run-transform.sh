#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`

cp https_cdn.jsdelivr.net_npm_@babel-standalone@7.28.6_babel.js babel-standalone.mjs
echo "const Babel = globalThis.Babel;" >> babel-standalone.mjs
echo "export { Babel };" >> babel-standalone.mjs
./transform.mjs
