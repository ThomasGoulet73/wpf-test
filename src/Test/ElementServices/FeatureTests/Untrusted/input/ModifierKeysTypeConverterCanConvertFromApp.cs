// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Avalon.Test.CoreUI.Trusted;
using Avalon.Test.CoreUI;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;

using Avalon.Test.CoreUI.Common;

using Avalon.Test.CoreUI.CoreInput.Common;
using Avalon.Test.CoreUI.CoreInput.Common.Controls;
using Microsoft.Test.Win32;
using Microsoft.Test.Discovery;
using Microsoft.Test;
using Microsoft.Test.Logging;

namespace Avalon.Test.CoreUI.CoreInput
{
    /// <summary>
    /// Verify ModifierKeysConverter CanConvertFrom method.
    /// </summary>
    /// <description>
    /// This is part of a collection of unit tests for input events.
    /// </description>
    /// <author>Microsoft</author>
 
    [CoreTestsLoader(CoreTestsTestType.MethodBase)]
    public class ModifierKeysConverterCanConvertFromApp : TestApp
    {
        /// <summary>
        /// Launch our test.
        /// </summary>
        [TestCase("1", @"CoreInput\TypeConverter", "HwndSource", @"Verify ModifierKeysConverter CanConvertFrom in HwndSource.")]
        public static void LaunchTest()
        {
            HostType hostType = (HostType)Enum.Parse(typeof(HostType), DriverState.DriverParameters["TestParameters"]);

            ExeStubContainerFramework exe = new ExeStubContainerFramework(hostType);
            exe.Run(new ModifierKeysConverterCanConvertFromApp(), "Run");

        }


        /// <summary>
        /// Setup the test.
        /// </summary>
        /// <param name="sender">App sending the callback.</param>
        /// <returns>Null object.</returns>
        public override object DoSetup(object sender)
        {
            CoreLogger.LogStatus("Getting Converter....");
            _converter = TypeDescriptor.GetConverter(typeof(ModifierKeys));
            Debug.Assert(_converter is ModifierKeysConverter);

            return null;
        }

        /// <summary>
        /// Validate the test.
        /// </summary>
        /// <param name="arg">Not used.</param>
        /// <returns>Null object.</returns>
        public override object DoValidate(object arg)
        {
            CoreLogger.LogStatus("Validating...");

            // Note: for this test we are concerned about whether the proper conversion can happen.

            bool bConvertFromString = _converter.CanConvertFrom(typeof(String));
            bool bConvertFromInt = _converter.CanConvertFrom(typeof(int));
            bool bConvertFromNull = _converter.CanConvertFrom(null);

            CoreLogger.LogStatus("Convertible from string,int,null? " +
                               bConvertFromString + "," + bConvertFromInt + "," + bConvertFromNull);

            bool actual = (bConvertFromString) && (!bConvertFromInt) && (!bConvertFromNull);
            bool expected = true;
            CoreLogger.LogStatus("Convertible? " + actual + ", expected: " + expected);

            bool eventFound = (actual == expected);

            CoreLogger.LogStatus("Setting log result to " + eventFound);
            this.TestPassed = eventFound;

            CoreLogger.LogStatus("Validation complete!");

            return null;
        }

        private TypeConverter _converter;
    }
}

