# LoadAssembly検証

- [1. 結論](#1-結論)
  - [1.1. 実行ファイル](#11-実行ファイル)
  - [1.2. DLLファイル](#12-dllファイル)
- [2. 調査方法](#2-調査方法)
  - [2.1. モジュールの種類を調べる方法](#21-モジュールの種類を調べる方法)
  - [2.2. 実行中のプロセスが64ビットかどうか調べる方法](#22-実行中のプロセスが64ビットかどうか調べる方法)
- [3. 実行結果](#3-実行結果)
  - [3.1. プラットフォームターゲット=AnyCPU](#31-プラットフォームターゲットanycpu)
  - [3.2. プラットフォームターゲット=x86](#32-プラットフォームターゲットx86)
  - [3.3. プラットフォームターゲット=x64](#33-プラットフォームターゲットx64)


----

プラットフォームターゲットに AnyCPU, x86, x64 を指定した場合の動作を調査した。

1. 実行プロセスのビット数
2. 動的アセンブリロードの可否

MSDNの下記ページに書かれている内容の追試行という位置づけになる。

* コンパイラの出力オプション PlatformTarget  
  https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/compiler-options/output#platformtarget


アセンブリのロードは以下の3種類の方法を調査した。

1. Assembly.LoadFile() - .NET Framework, .NET Standard で使える仕組み
2. AssemblyLoadContext.LoadFromAssemblyPath() - .NET Core で導入されたアセンブリ読み込みコンテキスト
3. MetadataLoadContext.LoadFromAssemblyPath() - .NET Standard 2.0 で使用できるOSS。メタデータのみを読み込める

* System.Reflection.MetadataLoadContext   
    https://www.nuget.org/packages/System.Reflection.MetadataLoadContext

## 1. 結論

### 1.1. 実行ファイル
プラットフォームターゲットにより異なる。    
AnyCPUはOSのビット数、x86,x64はそれぞれ指定したビット数のプロセスとして実行される。

| プラットフォームターゲット | モジュールのPEKind値 | 64bitOSでの実行ビット数 |
|:---|:---|:---|
| AnyCPU | (ビット数指定なし) | 64bit |
| x86 | Required32Bit (32bit) | 32bit |
| x64 | PE32Plus (64bit) | 64bit |

### 1.2. DLLファイル
実行プロセスのビット数と一致するアセンブリのみをロードできる。    
それ以外は `BadImageFileException`例外がthrowされてロードに失敗する。

| 実行プロセスのビット数 | AnyCPU | x86 | x64 |
|:---|:---|:---|:---|
| 64bit | ⭕ | ❌ | ⭕ |
| 32bit | ⭕ | ⭕ | ❌ |

`Assembly`,`AssemblyLoadContext`のどちらを使ってロードしても同じ。

インスタンス化を必要とせず、メタデータの参照のみをしたい場合、
`MetadataLoadContext`を使うとビット数が異なるアセンブリもロードでき、メタデータを参照できる。    

| 実行プロセスのビット数 | AnyCPU | x86 | x64 |
|:---|:---|:---|:---|
| 64bit | ⭕ | ⭕ | ⭕ |
| 32bit | ⭕ | ⭕ | ⭕ |


※ただし、メソッドによってはインスタンス化が行われるものがあり、実行プロセスと一致しないアセンブリに対してそのようなメソッドを呼び出した場合は`BadImageFileException`例外がthrowされる。

## 2. 調査方法
### 2.1. モジュールの種類を調べる方法
アセンブリからPE種別を取得する。

```cs
var asm = Assembly.GetCallingAssembly();
asm.ManifestModule.GetPEKind(out var peKind, out var machine);
```

PortableExecutableKinds列挙型の詳細は [PortableExecutableKinds 列挙型](https://docs.microsoft.com/ja-jp/dotnet/api/system.reflection.portableexecutablekinds?view=net-6.0) を参照。

### 2.2. 実行中のプロセスが64ビットかどうか調べる方法
`Environment.Is64BitProcess`の値で判断する。

## 3. 実行結果

### 3.1. プラットフォームターゲット=AnyCPU
```sh
V:\My\LoadAssembly>App.AnyCPU\bin\Debug\net6.0\App.AnyCPU.exe
App.AnyCPU, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  Is64BitProcess: True
  ImageRuntimeVersion: v4.0.30319
  PEKind: ILOnly
  Machine: I386
1. Assembly.LoadFile()
  Any: o
  x86: x
  x64: o
2. AssemblyLoadContext.LoadFromAssemblyPath()
  Any: o
  x86: x
  x64: o
3. MetadataLoadContext.LoadFromAssemblyPath()
  Any: o
  x86: o
  x64: o
```

### 3.2. プラットフォームターゲット=x86
```sh
V:\My\LoadAssembly>App.x86\bin\Debug\net6.0\App.x86.exe
App.x86, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  Is64BitProcess: False
  ImageRuntimeVersion: v4.0.30319
  PEKind: ILOnly, Required32Bit
  Machine: I386
1. Assembly.LoadFile()
  Any: o
  x86: o
  x64: x
2. AssemblyLoadContext.LoadFromAssemblyPath()
  Any: o
  x86: o
  x64: x
3. MetadataLoadContext.LoadFromAssemblyPath()
  Any: o
  x86: o
  x64: o
```

### 3.3. プラットフォームターゲット=x64
```sh
V:\My\LoadAssembly>App.x64\bin\Debug\net6.0\App.x64.exe
App.x64, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  Is64BitProcess: True
  ImageRuntimeVersion: v4.0.30319
  PEKind: ILOnly, PE32Plus
  Machine: AMD64
1. Assembly.LoadFile()
  Any: o
  x86: x
  x64: o
2. AssemblyLoadContext.LoadFromAssemblyPath()
  Any: o
  x86: x
  x64: o
3. MetadataLoadContext.LoadFromAssemblyPath()
  Any: o
  x86: o
  x64: o
```  
