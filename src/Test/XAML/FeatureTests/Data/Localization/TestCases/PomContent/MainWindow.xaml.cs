﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;

namespace PomContent
{
    /// <summary>
    /// Test app that the ability to loaclize a content control
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
