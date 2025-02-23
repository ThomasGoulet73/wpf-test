﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;

namespace PomStringSimple
{
    /// <summary>
    /// Test app that verifies the ability to localize a simple string property
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

	public void OnLoaded(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
