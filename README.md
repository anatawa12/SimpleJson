SimpleJson
===

A Simple, one-file, embeddable json parser / writer in C#.

This project is intended for source-install, embed this project using git submodule & reference the cs file in your project.

This project is intended to be used in unity project, so this uses C# 7.3 & netstandard2.0 for Unity 2019 compatibility.

How to Use
---

### using symlink in unity project (recommended if you can use symlink)

1. `git submodule add https://github.com/anatawa12/SimpleJson.git SimpleJson~`
2. `ln -s SimpleJson~/SimpleJson.cs .`

### direct importing in your project

1. `git submodule add https://github.com/anatawa12/SimpleJson.git SimpleJson`
