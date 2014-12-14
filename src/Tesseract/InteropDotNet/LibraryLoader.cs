//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT

namespace Tesseract.InteropDotNet {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public sealed class LibraryLoader {
        private readonly ILibraryLoaderLogic logic;

        private LibraryLoader( ILibraryLoaderLogic logic ) {
            this.logic = logic;
        }

        private readonly object syncLock = new object();
        private readonly Dictionary<string, IntPtr> loadedAssemblies = new Dictionary<string, IntPtr>();

        public IntPtr LoadLibrary( string fileName, string platformName = null ) {
            fileName = this.FixUpLibraryName( fileName );
            lock ( this.syncLock ) {
                if ( !this.loadedAssemblies.ContainsKey( fileName ) ) {
                    if ( platformName == null )
                        platformName = SystemManager.GetPlatformName();
                    LibraryLoaderTrace.TraceInformation( "Current platform: " + platformName );
                    var dllHandle = this.CheckExecutingAssemblyDomain( fileName, platformName );
                    if ( dllHandle == IntPtr.Zero )
                        dllHandle = this.CheckCurrentAppDomain( fileName, platformName );
                    if ( dllHandle == IntPtr.Zero )
                        dllHandle = this.CheckWorkingDirecotry( fileName, platformName );

                    if ( dllHandle != IntPtr.Zero )
                        this.loadedAssemblies[ fileName ] = dllHandle;
                    else
                        LibraryLoaderTrace.TraceError( "Failed to find library \"{0}\" for platform {1}.", fileName, platformName );
                }
            }
            return this.loadedAssemblies[ fileName ];
        }

        private IntPtr CheckExecutingAssemblyDomain( string fileName, string platformName ) {
            var baseDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            return this.InternalLoadLibrary( baseDirectory, platformName, fileName );
        }

        private IntPtr CheckCurrentAppDomain( string fileName, string platformName ) {
            var baseDirectory = Path.GetFullPath( AppDomain.CurrentDomain.BaseDirectory );
            return this.InternalLoadLibrary( baseDirectory, platformName, fileName );
        }

        private IntPtr CheckWorkingDirecotry( string fileName, string platformName ) {
            var baseDirectory = Path.GetFullPath( Environment.CurrentDirectory );
            return this.InternalLoadLibrary( baseDirectory, platformName, fileName );
        }

        private IntPtr InternalLoadLibrary( string baseDirectory, string platformName, string fileName ) {
            var fullPath = Path.Combine( baseDirectory, Path.Combine( platformName, fileName ) );
            return File.Exists( fullPath ) ? this.logic.LoadLibrary( fullPath ) : IntPtr.Zero;
        }

        public bool FreeLibrary( string fileName ) {
            fileName = this.FixUpLibraryName( fileName );
            lock ( this.syncLock ) {
                if ( !this.IsLibraryLoaded( fileName ) ) {
                    LibraryLoaderTrace.TraceWarning( "Failed to free library \"{0}\" because it is not loaded", fileName );
                    return false;
                }
                if ( this.logic.FreeLibrary( this.loadedAssemblies[ fileName ] ) ) {
                    this.loadedAssemblies.Remove( fileName );
                    return true;
                }
                return false;
            }
        }

        public IntPtr GetProcAddress( IntPtr dllHandle, string name ) => this.logic.GetProcAddress( dllHandle, name );

        public bool IsLibraryLoaded( string fileName ) {
            fileName = this.FixUpLibraryName( fileName );
            lock ( this.syncLock )
                return this.loadedAssemblies.ContainsKey( fileName );
        }

        private string FixUpLibraryName( string fileName ) => this.logic.FixUpLibraryName( fileName );

        #region Singleton

        private static LibraryLoader instance;

        public static LibraryLoader Instance {
            get {
                if ( instance == null ) {
                    var operatingSystem = SystemManager.GetOperatingSystem();
                    switch ( operatingSystem ) {
                        case OperatingSystem.Windows:
                            LibraryLoaderTrace.TraceInformation( "Current OS: Windows" );
                            instance = new LibraryLoader( new WindowsLibraryLoaderLogic() );
                            break;

                        case OperatingSystem.Unix:
                            LibraryLoaderTrace.TraceInformation( "Current OS: Unix" );
                            instance = new LibraryLoader( new UnixLibraryLoaderLogic() );
                            break;

                        case OperatingSystem.MacOSX:
                            throw new Exception( "Unsupported operation system" );
                        default:
                            throw new Exception( "Unsupported operation system" );
                    }
                }
                return instance;
            }
        }

        #endregion Singleton
    }
}