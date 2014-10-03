//  Copyright (c) 2014 Andrey Akinshin
//  Project URL: https://github.com/AndreyAkinshin/InteropDotNet
//  Distributed under the MIT License: http://opensource.org/licenses/MIT

namespace Tesseract.InteropDotNet {

    using System;
    using System.Diagnostics;
    using System.Globalization;

    internal static class LibraryLoaderTrace {
        private const bool printToConsole = false;
        private static readonly TraceSource trace = new TraceSource( "Tesseract" );

        private static void Print( string message ) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( message );
            Console.ResetColor();
        }

        public static void TraceInformation( string format, params object[] args ) {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if ( printToConsole )
                Print( string.Format( CultureInfo.CurrentCulture, format, args ) );
            trace.TraceEvent( TraceEventType.Information, 0, string.Format( CultureInfo.CurrentCulture, format, args ) );
        }

        public static void TraceError( string format, params object[] args ) {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if ( printToConsole )
                Print( string.Format( CultureInfo.CurrentCulture, format, args ) );
            trace.TraceEvent( TraceEventType.Error, 0, string.Format( CultureInfo.CurrentCulture, format, args ) );
        }

        public static void TraceWarning( string format, params object[] args ) {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if ( printToConsole )
                Print( string.Format( CultureInfo.CurrentCulture, format, args ) );
            trace.TraceEvent( TraceEventType.Warning, 0, string.Format( CultureInfo.CurrentCulture, format, args ) );
        }
    }
}