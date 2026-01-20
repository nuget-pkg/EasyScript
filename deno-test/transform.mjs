#! /usr/bin/env deno-run
import { Babel } from "./babel-standalone.mjs";

const input = `const getMessage = () => "Hello World";
                   document.getElementById("app").innerText = getMessage();
				   function add2(a:number, b:number):number { return a + b }
				   `;
const output = Babel.transform(input,
  {
    presets: ["typescript"],
    filename: 'file.ts',
    sourceType: "script"
  }).code;
console.log(output);
