using System.Runtime.InteropServices;

namespace Inventory.API.Helper
{
    public class PDFHelper
    {
        public static class CustomAssemblyLoadContext
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            public static void LoadNativeLibrary()
            {
                // Architecture folder: x64 ya x86
                var architectureFolder = (IntPtr.Size == 8) ? "x64" : "x86";

                // Aapke screenshot ke mutabiq base path
                // Note: File name vahi rakha hai jo aapke Solution Explorer mein hai: wkhtmltox.dll
                var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vclibs", architectureFolder, "libwkhtmltox.dll");

                if (File.Exists(dllPath))
                {
                    LoadLibrary(dllPath);
                }
                else
                {
                    // Error message jo exact missing path batayega
                    throw new Exception($"Bhai, DLL is path par nahi mili: {dllPath}");
                }
            }
        }
    }

}

