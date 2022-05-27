using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Common
{
    public static class CommonClass
    {
        // 呼び出し元アセンブリの情報をコンソール出力する
        public static void TraceInfo()
        {
            var asm = Assembly.GetCallingAssembly();
            asm.ManifestModule.GetPEKind(out var peKind, out var machine);
            Console.WriteLine($"{asm.FullName}");
            Console.WriteLine($"  Is64BitProcess: {Environment.Is64BitProcess}");
            Console.WriteLine($"  ImageRuntimeVersion: {asm.ImageRuntimeVersion}");
            Console.WriteLine($"  PEKind: {peKind}");
            Console.WriteLine($"  Machine: {machine}");
        }

        // 3種類のDLLファイル(AnyCPU,x86,x64)をAssemblyLoadContextでロードできるか試す
        public static void LoadDlls()
        {
            var basePath = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CommonClass)).Location)).Parent.Parent.Parent.Parent.FullName;
            var dllPaths = new Dictionary<string, string>()
            {
                { "Any", Path.Combine(basePath, @"DLL.AnyCPU\bin\debug\net6.0\DLL.AnyCPU.dll") },
                { "x86", Path.Combine(basePath, @"DLL.x86\bin\debug\net6.0\DLL.x86.dll") },
                { "x64", Path.Combine(basePath, @"DLL.x64\bin\debug\net6.0\DLL.x64.dll") }
            };

            Console.WriteLine("1. Assembly.LoadFile()");
            foreach (var path in dllPaths) SafeLoadFile(path.Key, path.Value);

            Console.WriteLine("2. AssemblyLoadContext.LoadFromAssemblyPath()");
            var alc = new AssemblyLoadContext("test");
            foreach (var path in dllPaths) SafeLoadFromAssemblyPath(alc, path.Key, path.Value);

            Console.WriteLine("3. MetadataLoadContext.LoadFromAssemblyPath()");
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var paths = new List<string>(runtimeAssemblies);
            var resolver = new PathAssemblyResolver(paths);
            var mlc = new MetadataLoadContext(resolver);
            foreach (var path in dllPaths) SafeLoadFromAssemblyPath(mlc, path.Key, path.Value);
        }

        // アセンブリをロード Assembly.LoadFile()
        public static void SafeLoadFile(string name, string filePath)
        {
            try
            {
                Assembly.LoadFile(filePath);
                Console.WriteLine($"  {name}: o");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {name}: x");
            }
        }

        // アセンブリをロード AssemblyLoadContext.LoadFromAssemblyPath()
        public static void SafeLoadFromAssemblyPath(AssemblyLoadContext alc, string name, string filePath)
        {
            try
            {
                alc.LoadFromAssemblyPath(filePath);
                Console.WriteLine($"  {name}: o");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {name}: x");
            }
        }

        // アセンブリをロード MetadataLoadContext.LoadFromAssemblyPath()
        public static void SafeLoadFromAssemblyPath(MetadataLoadContext mlc, string name, string filePath)
        {
            try
            {
                mlc.LoadFromAssemblyPath(filePath);
                Console.WriteLine($"  {name}: o");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {name}: x");
            }
        }
    }
}