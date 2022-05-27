# LoadAssembly検証

- [1. 結論](#1-結論)
  - [1.1. 実行アセンブリ](#11-実行アセンブリ)
  - [1.2. ロード可能なアセンブリ](#12-ロード可能なアセンブリ)
- [2. 実行結果](#2-実行結果)
  - [2.1. プラットフォームターゲット=AnyCPU](#21-プラットフォームターゲットanycpu)
  - [2.2. プラットフォームターゲット=x86](#22-プラットフォームターゲットx86)
  - [2.3. プラットフォームターゲット=x64](#23-プラットフォームターゲットx64)


----

プラットフォームターゲットに AnyCPU, x86, x64 を指定した場合の動作を調査した。

1. 実行プロセスのビット数
2. 動的アセンブリロードの可否

アセンブリのロードは以下の3種類の方法を調査した。

1. Assembly.LoadFile() - .NET Framework, .NET Standard で使える仕組み
2. AssemblyLoadContext.LoadFromAssemblyPath() - .NET Core で導入されたアセンブリ読み込みコンテキスト
3. MetadataLoadContext.LoadFromAssemblyPath() - .NET Standard 2.0 で使用できるOSS。メタデータのみを読み込める

* System.Reflection.MetadataLoadContext   
    https://www.nuget.org/packages/System.Reflection.MetadataLoadContext

## 1. 結論

### 1.1. 実行アセンブリ
AnyCPUはOSのビット数、x86,x64はそれぞれ指定したビット数のプロセスとして実行される。

| プラットフォームターゲット | モジュールのPEKind値 | 64bitOSでの実行ビット数 |
|:---|:---|:---|
| AnyCPU | (ビット数指定なし) | 64bit |
| x86 | Required32Bit (32bit) | 32bit |
| x64 | PE32Plus (64bit) | 64bit |

### 1.2. ロード可能なアセンブリ
`Assembly`,`AssemblyLoadContext`のどちらを使った場合でも、AnyCPUまたは実行プロセスのビット数と一致するアセンブリをロードできる。  
それ以外は `BadImageFileException`例外がthrowされてロードに失敗する。

| プラットフォームターゲット | 64bitOSでの実行ビット数 | AnyCPU | x86 | x64 |
|:---|:---|:---|:---|:---|
| AnyCPU |  64bit | ⭕ | ❌ | ⭕ |
| x86 | 32bit | ⭕ | ⭕ | ❌ |
| x64 | 64bit | ⭕ | ❌ | ⭕ |

ただし、`MetadataLoadContext`を使うとビット数が異なるアセンブリもロードできる。

## 2. 実行結果

### 2.1. プラットフォームターゲット=AnyCPU
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

### 2.2. プラットフォームターゲット=x86
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

### 2.3. プラットフォームターゲット=x64
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
