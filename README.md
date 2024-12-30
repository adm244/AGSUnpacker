# AGSUnpacker
A program to work with *compiled* Adventure Game Studio (AGS) engine resources.

It features extraction of game assets from executable (\*.exe) or archive files (\*.ags, \*.xxx), unpacking and repacking of sprites (\*.spr), previewing and changing room backgrounds (\*.crm), generating translation files (\*.trs, \*.tra), replacing strings directly in scripts and more.

Written in .NET C#.

## Supported versions

This project aims to support **all officially released** 2.x and 3.x AGS versions. It includes support for obsolete engines before 2.54 that have no support in publically released source code.

Original Adventure Creator 1.x and all future AGS 4.x (this includes 3.99.x) or custom versions are beyond the scope (for now).

## Features

The list of features is not definitive nor full it just outlines high-level representation of what this project has or lacks at the moment.

- [ ] Assets
    - [x] Extraction from executable (\*.exe)
    - [x] Extraction from single archive (\*.ags)
    - [x] Extraction from multiple archives (\*.xxx)
    - [ ] Packaging into executable
    - [ ] Packaging into single archive
    - [ ] Packaging into multiple archives
    - [ ] Forward conversion (upgrade) to newer engines (for old unmaintained games mostly)
- [x] Sprites
    - [x] Unpacking sprites (\*.spr)
    - [x] Repacking sprites
- [x] Translations
    - [x] Generation of translation file from assets
    - [x] "Decompilation" of existing translation files (\*.tra)
    - [x] "Compilation" of translation files (\*.trs)
- [ ] Scripts
    - [x] Scripts extraction (\*.scom3)
    - [x] Scripts injection into game data (\*.dta)
    - [x] Scripts injection into rooms (\*.crm)
    - [x] Replacing strings in scripts from translation file
    - [ ] Script disassembler
    - [ ] Script assembler
    - [ ] Script decompiler
- [ ] Rooms
    - [x] Preview backgrounds
    - [x] Replace backgrounds
    - [ ] Preview room areas (walkable, regions, etc.) (?)
    - [ ] Preview room objects (?)
- [ ] GUI
    - [ ] Preview GUI elements (?)
    - [ ] Change GUI elements (?)
- [ ] Project structure reconstruction

## Third-party requirements
* [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime)

**AGSUnpacker.UI:**
* [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm/)

**AGSUnpacker.Graphics.GDI:**
* [System.Drawing.Common](https://www.nuget.org/packages/System.Drawing.Common)

## Compilation

### Install .NET SDK

For windows host you can use .NET 6 or .NET 8. If you're cross-compiling on linux then prefer .NET 8.

On Windows:

- Download and install [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

On Linux:

- Depending on the distribution install either from your distro repository or follow [instructions](https://learn.microsoft.com/en-us/dotnet/core/install/linux) from M\*.

### Get source code

Clone repository:
```sh
git clone https://github.com/adm244/AGSUnpacker.git
```
or [download](https://github.com/adm244/AGSUnpacker/archive/refs/heads/master.zip) master branch and unpack it.

### Build using makefile

In terminal navigate to AGSUnpacker root folder and use provided makefile to build:
```sh
make publish CONFIG=Release ARCH=x64
```
or to compile 32-bit executables:
```sh
make publish CONFIG=Release ARCH=x86
```

If everything went OK you'll find compiled program in `build/package` folder.

### Build using dotnet

*If you can't use `make` then follow this instructions.*

In terminal navigate to AGSUnpacker root folder and use this dotnet command:
```sh
dotnet publish AGSUnpacker.UI/AGSUnpacker.UI.csproj --os win -c Release -a x64 --no-self-contained -o build/package/
```
or to compile 32-bit executables:
```sh
dotnet publish AGSUnpacker.UI/AGSUnpacker.UI.csproj --os win -c Release -a x86 --no-self-contained -o build/package/
```

If everything went OK you'll find compiled program in `build/package` folder.

## Similar projects

Other projects that work with AGS engine I am aware of:

- (C) [agsutils](https://github.com/rofl0r/agsutils) by rofl0r
- (Delphi) [AGS-Tool](https://github.com/SileNTViP/AGS-Tool) by SileNTViP

## License
This is free and unencumbered software released into the **public domain**.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
