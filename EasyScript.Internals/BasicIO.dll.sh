#! /usr/bin/env bash
script_dir="$(dirname "$0")"
script_dir="$(realpath $script_dir)"
str="$1"
if [[ "${str:0:1}" == "@" ]]; then
  if [[ "$str" == "@run" ]]; then
    shift
  fi
  "$script_dir/.r.BasicIO" "$@"
else
  cscs "$script_dir/BasicIO.dll.cs" "$@"
fi
