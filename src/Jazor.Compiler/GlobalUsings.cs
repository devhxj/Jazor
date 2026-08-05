// File: GlobalUsings.cs
// Purpose: Declares assembly-wide imports and friend-assembly visibility for Jazor.Compiler.
// 只放编译项目共享的基础依赖；语义实现仍应在各自职责文件中保持显式。
global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Jazor.Analyzer")]
[assembly: InternalsVisibleTo("Jazor.Compiler.Generator")]
[assembly: InternalsVisibleTo("Jazor.CompilerTest")]
