﻿//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT

namespace Tesseract.InteropDotNet {
    using System;
    using System.Runtime.InteropServices;

    [ComVisible( true )]
    [AttributeUsage( AttributeTargets.Method, Inherited = false, AllowMultiple = false )]
    internal sealed class RuntimeDllImportAttribute : Attribute {
        public string EntryPoint;

        public CallingConvention CallingConvention;

        public CharSet CharSet;

        public bool SetLastError;

        public bool BestFitMapping;

        public bool ThrowOnUnmappableChar;

        public string LibraryFileName {
            get;
            private set;
        }

        public RuntimeDllImportAttribute( string libraryFileName ) {
            this.LibraryFileName = libraryFileName;
        }
    }
}