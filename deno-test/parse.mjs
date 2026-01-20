#! /usr/bin/env deno-run
import { parse } from './babel-parser.mjs';

var ast = parse('return 11 + 22', { plugins: ["typescript"], allowReturnOutsideFunction: true });
console.log(JSON.stringify(ast, null, 2));

// Option to turn off start/end/loc on AST @babel/parser produced
// https://github.com/babel/babel/issues/11239
const removeASTLocation = ast => {
   if (ast == null) return;
   if (Array.isArray(ast)) {
      ast.forEach(a => removeASTLocation(a));
   } else if (typeof ast === 'object') {
      delete ast['loc'];
      delete ast['start'];
      delete ast['end'];
      const values = Object.values(ast).filter(v => Array.isArray(v) || typeof v === 'object');
      removeASTLocation(values);
   }
};

removeASTLocation(ast);
console.log(JSON.stringify(ast, null, 2));
