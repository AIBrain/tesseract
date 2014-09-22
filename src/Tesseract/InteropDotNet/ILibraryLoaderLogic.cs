//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT

namespace Tesseract.InteropDotNet
{
    using System;

    interface ILibraryLoaderLogic
    {
        IntPtr LoadLibrary(string fileName);
        bool FreeLibrary(IntPtr libraryHandle);
        IntPtr GetProcAddress(IntPtr libraryHandle, string functionName);
        string FixUpLibraryName(string fileName);
    }
}