// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Description: 

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Test.Logging;           // TestLog, TestStage
using Microsoft.Windows.Test.Client.AppSec.Navigation;

namespace Microsoft.Windows.Test.Client.AppSec.Navigation
{
    public partial class NavigationTests : Application
    {
        internal enum NavigateArgs_State
        {
            InitialNav, 
            NavigatedToSecondPage,
            NavigatedToThirdPage,
            CalledGoBack,
            CalledGoForward,
            CalledGoForward2,
            CancelNavigation
        }

        NavigateArgs_State _navigateArgs_curState = NavigateArgs_State.InitialNav;

        private void NavigateArgs_Startup(object sender, StartupEventArgs e)
        {
            NavigationHelper.CreateLog("EvenMorePonies [NavigateArgs]");
            NavigationHelper.Output("Initializing TestLog...");
            NavigationHelper.SetStage(TestStage.Initialize);

            NavigationHelper.ExpectedTestCount = 13;
            NavigationHelper.ActualTestCount = 0;
            NavigationHelper.ExpectedFileName = @"NavigateArgs_Page2.xaml";

            //NavigationHelper.Output("Registering application-level eventhandlers.");
            //this.Navigating += new NavigatingCancelEventHandler(OnNavigating);
            //this.Navigated += new NavigatedEventHandler(OnNavigated);
            //this.LoadCompleted += new LoadCompletedEventHandler(OnLoadCompleted);
            //this.NavigationProgress += new NavigationProgressEventHandler(OnNavigationProgress);
            //this.FragmentNavigation += new FragmentNavigationEventHandler(OnFragmentNavigation);
            //this.Exit += new ExitEventHandler(OnExit);

            NavigationHelper.SetStage(TestStage.Run);
            this.StartupUri = new Uri("NavigateArgs_Page1.xaml", UriKind.RelativeOrAbsolute);
            //base.OnStartup(e);
        }

        public void NavigateArgs_Navigating(object source, NavigatingCancelEventArgs e)
        {
            NavigationHelper.Output("Navigating event fired");
            NavigationHelper.ActualTestCount++;
            if (_navigateArgs_curState == NavigateArgs_State.CancelNavigation)
                e.Cancel = true;

            NavigateArgs_CheckEventArgs(e);
        }

        public void NavigateArgs_Navigated(object source, NavigationEventArgs e)
        {
            NavigationHelper.Output("Navigated event fired");
            NavigationHelper.ActualTestCount++;
            NavigateArgs_CheckEventArgs(e);
        }

        public void NavigateArgs_NavigationProgress(object source, NavigationProgressEventArgs e)
        {
            NavigationHelper.Output("NavigationProgress event fired");
            NavigateArgs_CheckEventArgs(e);
        }

        public void NavigateArgs_FragmentNavigation(object source, FragmentNavigationEventArgs e)
        {
            NavigationHelper.Output("FragmentNavigation event fired");
            NavigationHelper.ActualTestCount++;
            NavigateArgs_CheckEventArgs(e);
        }

        public void NavigateArgs_Exit(object source, ExitEventArgs e)
        {
            NavigationHelper.Output("Exit event fired");
            NavigationHelper.ActualTestCount++;
            NavigateArgs_CheckEventArgs(e);
        }

        public void NavigateArgs_LoadCompleted(object source, NavigationEventArgs e)
        {
            NavigationHelper.Output("LoadCompleted event fired");
            NavigationHelper.ActualTestCount++;
            NavigateArgs_CheckEventArgs(e);

            Hyperlink h;
            RequestNavigateEventArgs requestNavigateEventArgs;

            if (e.Uri == null)
            {
                NavigationHelper.Output("no uri here");
            }
            else 
            {
                NavigationHelper.Output("uri is: " + e.Uri.ToString());
                NavigationHelper.Output("uri is: " + NavigationHelper.getFileName(e.Uri));
            }

            NavigationWindow nw = Application.Current.MainWindow as NavigationWindow;
            if (nw != null)
            {
                switch(_navigateArgs_curState)
                {
                    case NavigateArgs_State.InitialNav : 
                            _navigateArgs_curState = NavigateArgs_State.NavigatedToSecondPage;
                            NavigationHelper.Output("Calling Navigate on NavigateArgs_Page2.xaml ");
                            nw.Navigate(new Uri("NavigateArgs_Page2.xaml", UriKind.RelativeOrAbsolute));
                            break;
    
                    case NavigateArgs_State.NavigatedToSecondPage : 
                            _navigateArgs_curState = NavigateArgs_State.NavigatedToThirdPage;
                            // get Hyperlink from MyOtherNewPage
                            h = LogicalTreeHelper.FindLogicalNode(nw, "hyperlink1") as Hyperlink;

                            // create RequestNavigateEventArgs here & execute them on my Hyperlink
                            requestNavigateEventArgs = new RequestNavigateEventArgs(h.NavigateUri, null);
                            requestNavigateEventArgs.Source=h;
                            h.RaiseEvent(requestNavigateEventArgs);

                            break;
    
                    case NavigateArgs_State.NavigatedToThirdPage : 
                            _navigateArgs_curState = NavigateArgs_State.CalledGoBack;
                            NavigationHelper.Output("Calling GoBack on secondPage");
                            nw.GoBack();
                            NavigationHelper.Output("Just called GoBack");
                            break;
    
                    case NavigateArgs_State.CalledGoBack : 
                            _navigateArgs_curState = NavigateArgs_State.CancelNavigation;
                            // Call another navigation but cancel it
                            nw.GoForward();
                            NavigationHelper.FinishTest((NavigationHelper.CompareFileNames(NavigationHelper.ExpectedFileName, e.Uri)) && (e.Content != null));
                            break;
                }
            }
            else 
            {
                NavigationHelper.Fail("Could not get NavigationWindow");
            }
        }

        public void NavigateArgs_CheckEventArgs(EventArgs e)
        {
            // NavigationEventArgs - Uri, Content, IsNavigationInitiator, ExtraData
            if (e is NavigationEventArgs)
            {
                NavigationHelper.Output("  *NavigationEventArgs*");
                NavigationEventArgs nae = e as NavigationEventArgs;
                NavigationHelper.Output("    Content is: " + ((nae.Content != null) ? nae.Content.ToString() : "null"));
                NavigationHelper.Output("    ExtraData is: " + ((nae.ExtraData != null) ? nae.ExtraData.ToString() : "null"));
                NavigationHelper.Output("    IsNavigationInitiator is: " + nae.IsNavigationInitiator);
                NavigationHelper.Output("    Uri is: " + ((nae.Uri != null) ? nae.Uri.ToString() : "null"));
            }

            // NavigatingCancel - Uri, Content, ExtraData, NavigationMode, 
            else if (e is NavigatingCancelEventArgs)
            {
                NavigationHelper.Output("  *NavigatingCancelEventArgs*");
                NavigatingCancelEventArgs ncea = e as NavigatingCancelEventArgs;
                if (ncea != null)
                {
                    NavigationHelper.Output("    Content is: " + ((ncea.Content != null) ? ncea.Content.ToString() : "null"));
                    NavigationHelper.Output("    ExtraData is: " + ((ncea.ExtraData != null) ? ncea.ExtraData.ToString() : "null"));
                    NavigationHelper.Output("    NavigationMode is: " + ncea.NavigationMode);
                    NavigationHelper.Output("    Uri is: " + ((ncea.Uri != null) ? ncea.Uri.ToString() : "null"));
                    NavigationHelper.Output("    Cancel is: " + ncea.Cancel);
                    NavigationHelper.Output("    ContentStateToSave is : " + ((ncea.ContentStateToSave != null)? ncea.ContentStateToSave.ToString() : "null"));
                    NavigationHelper.Output("    TargetContentState is : " + ((ncea.TargetContentState  != null)? ncea.TargetContentState.ToString() : "null"));
                }
                else
                    NavigationHelper.Output("  *ncea is null*");
            }

            // NavProgEvents - Uri, BytesRead, MaxBytes
            else if (e is NavigationProgressEventArgs)
            {
                NavigationHelper.Output("  *NavigationProgressEventArgs*");
                NavigationProgressEventArgs npea = e as NavigationProgressEventArgs;
                NavigationHelper.Output("    BytesRead is: " + npea.BytesRead);
                NavigationHelper.Output("    MaxBytes is: " + npea.MaxBytes);
                NavigationHelper.Output("    Uri is: " + ((npea.Uri != null) ? npea.Uri.ToString() : "null"));
            }

            // FragmentNavigationEventArgs - Fragment, Handled
            else if (e is FragmentNavigationEventArgs)
            {
                NavigationHelper.Output("  *FragmentNavigationEventArgs*");
                FragmentNavigationEventArgs fnea = e as FragmentNavigationEventArgs;
                NavigationHelper.Output("    Fragment is: " + fnea.Fragment.ToString());
                NavigationHelper.Output("    Handled is: " + fnea.Handled);
            }

            // ExitEventArgs - ApplicationExitCode
            else if (e is ExitEventArgs)
            {
                NavigationHelper.Output("  *ExitEventArgs*");
                ExitEventArgs sea = e as ExitEventArgs;
                NavigationHelper.Output("    ApplicationExitCode is: " + sea.ApplicationExitCode);
            }
        }
    }
}

