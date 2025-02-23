// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*******************************************************************
 * Purpose: Automation for the stateless MDE model TemplateModel.
 *          Calls the appropriate helpers to construct trees and
 *          verify them.
 * Contributors: Microsoft
 *
 
  
 * Revision:         $Revision: 1 $
 
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Markup;
using System.Windows.Threading;

using Microsoft.Test;
using Microsoft.Test.Logging;
using Microsoft.Test.Modeling;

using Microsoft.Test.Serialization;
using Microsoft.Test.Threading;

using Avalon.Test.CoreUI;
using Avalon.Test.CoreUI.Common;
using Avalon.Test.CoreUI.Parser;
using Avalon.Test.CoreUI.Threading;
using Avalon.Test.CoreUI.Trusted;


namespace Avalon.Test.CoreUI.PropertyEngine.Template
{
    /// <summary>
    /// TemplateModel Model class
    /// </summary>  
    [Model(@"FeatureTests\ElementServices\TemplateModel_Pairwise.xtc", 0, @"PropertyEngine\Template", TestCaseSecurityLevel.FullTrust, "Template model pairwise", 
        SupportFiles = @"FeatureTests\ElementServices\TemplateModel_empty.xaml,FeatureTests\ElementServices\TemplateModel_elements.xaml",
        ExpandModelCases=true)]
    [Model(@"FeatureTests\ElementServices\TemplateModel_ThreeTuple.xtc", 2, @"PropertyEngine\Template", TestCaseSecurityLevel.FullTrust, "Template model pairwise", 
        SupportFiles = @"FeatureTests\ElementServices\TemplateModel_empty.xaml,FeatureTests\ElementServices\TemplateModel_elements.xaml",
        ExpandModelCases=true)]
    public class TemplateModel : CoreModel
    {
        /// <summary>
        /// Creates a TemplateModel Model instance.
        /// </summary>
        public TemplateModel(): base()
        {
            Name = "TemplateModel";
            Description = "TemplateModel Model";
            
            //Add Action Handlers
            AddAction("RunTest", new ActionHandler(RunTest));
            GlobalLog.LogStatus("*******************************");
        }

        /// <summary>
        /// Single action for this model.  Constructs a tree based on
        /// the parameter combination; loads the tree. the tree is
        /// verified while it's loaded.
        /// </summary>
        /// <remarks>Handler for RunTest</remarks>
        /// <param name="endState">Expected end State object</param>
        /// <param name="inParams">Input action parameters object</param>
        /// <param name="outParams">Output action parameters object</param>
        /// <returns>false if errors</returns>
        private bool RunTest(State endState, State inParams, State outParams)
        {
            // Initialize model state instance to be used in helpers
            // and verifiers.
            _modelState = new TemplateModelState(inParams);

            TemplateModelState.Persist(_modelState);

            _modelState.LogState();

            // Create the tree with xaml or code and run a dispatcher
            // and display the tree.
            if (_modelState.Kind.Contains("Xaml"))
            {
                _RunXamlBasedTest();
            }
            else
            {
                _RunCodeBasedTest();
            }

            return true;
        }

        // Handles code-based variations.
        private void _RunCodeBasedTest()
        {
            // Code-only variations aren't implemented yet.
            throw new NotSupportedException("Non-Xaml-based tests are not supported yet.");
        }

        // Handles xaml-based variations.
        private void _RunXamlBasedTest()
        {
            // Xaml-based variations always go through round-tripping
            // or compile-and-run.
            // The trees and verification routines should be identical
            // to the code-only variations.

            TemplateModelXamlHelper xamlHelper = new TemplateModelXamlHelper();
            Stream stream = xamlHelper.GenerateXaml(_modelState);

            // Save the xml to a temporary file.
            
            string tempFileName = ".\\__" + Path.ChangeExtension(Path.GetRandomFileName(), ".xaml");
            IOHelper.SaveTextToFile(stream, tempFileName);
            CoreLogger.LogStatus("Saved generated xaml to " + tempFileName);

            // Move the mouse out of the way before running the test so it doesn't trip any events unexpectedly.
            MouseHelper.MoveOnVirtualScreenMonitor();
            DispatcherHelper.DoEvents(2000);

            try
            {
                // Go through either round-tripping or compile-and-run.
                if (_modelState.Kind.Contains("Load"))
                {
                    // round-tripping.
                    SerializationHelper helper = new SerializationHelper();
                    helper.XamlSerialized += new XamlSerializedEventHandler(_OnXamlSerialized);
                    helper.RoundTripTest(stream, XamlWriterMode.Expression, true /*display*/);
                }
                else
                {
                    List<String> assemblyReferences = new List<String>();
                    assemblyReferences.Add("CoreTestsUntrusted.dll");
                    assemblyReferences.Add("TestRuntime.dll");

                    // compile-and-run.
                    CompilerHelper runner = new CompilerHelper();
                    runner.AddDefaults();
                    runner.CompileApp(tempFileName, "Application", null, null, assemblyReferences);
                    runner.RunCompiledApp();
                }

                // Delete the temp xaml file since the test passed.
                File.Delete(tempFileName);
            }
            catch
            {
                // Save the xaml file for future analysis.
                TestLog.Current.LogFile(tempFileName);
                throw;
            }
            finally
            {
                stream.Close();
            }
        }
        /// <summary>
        /// Logs round trip status messages to CoreLogger.
        /// </summary>
        private static void _OnXamlSerialized(object sender, XamlSerializedEventArgs args)
        {
            // Save xaml to file for potential debugging.
            if (File.Exists(s_tempXamlFile))
            {
                File.Copy(s_tempXamlFile, s_tempXamlFile2, true);
            }
            
            IOHelper.SaveTextToFile(args.Xaml, s_tempXamlFile);
        }

        // Specifies the load type of the current run.
        TemplateModelState _modelState = null;

        // Filenames for saving serialized xaml.
        static string s_tempXamlFile = "___SerializationTempFile.xaml";
        static string s_tempXamlFile2 = "___SerializationTempFile2.xaml";
    }

    [Serializable()]
    class TemplateModelState : CoreModelState
    {
        public TemplateModelState(State state)
        {
            Kind = state["Kind"];
            TemplateType = state["TemplateType"];
            TemplatedControlType = state["TemplatedControlType"];
            TemplatePropertyName = state["TemplatePropertyName"];
            TargetTypeSet = state["TargetTypeSet"];
            TemplateRootType = state["TemplateRootType"];
            TemplateChildType = state["TemplateChildType"];
            HasPropertyTrigger = state["HasPropertyTrigger"].ToLower().Equals("true");

            HasFreezableTriggerSetter = state["HasFreezableTriggerSetter"].ToLower().Equals("true");
            FreezableSetterValue = state["FreezableSetterValue"];

            HasEventTrigger = state["HasEventTrigger"];
            HasTemplateBind = state["HasTemplateBind"].ToLower().Equals("true");
            DoesReferToStyle = state["DoesReferToStyle"].ToLower().Equals("true");
            HasEventSet = state["HasEventSet"].ToLower().Equals("true");
            IsKeySet = state["IsKeySet"].ToLower().Equals("true");
            HowTemplateIsSet = state["HowTemplateIsSet"];
            DataSourceType = state["DataSourceType"];
            HasDataBinding = state["HasDataBinding"].ToLower().Equals("true");
            TriggerTarget = state["TriggerTarget"];
            TriggerSource = state["TriggerSource"];
        }

        public override void LogState()
        {

            CoreLogger.LogStatus("  Kind: " + Kind +
                           "\r\n  TemplateType: " + TemplateType +
                           "\r\n  TemplatedControlType: " + TemplatedControlType +
                           "\r\n  TemplatePropertyName: " + TemplatePropertyName +
                           "\r\n  TargetTypeSet: " + TargetTypeSet +
                           "\r\n  TemplateRootType: " + TemplateRootType +
                           "\r\n  TemplateChildType: " + TemplateChildType +
                           "\r\n  HasPropertyTrigger: " + HasPropertyTrigger +

                           "\r\n  HasFreezableTriggerSetter: " + HasFreezableTriggerSetter +
                           "\r\n  FreezableSetterValue: " + FreezableSetterValue +

                           "\r\n  HasEventTrigger: " + HasEventTrigger +
                           "\r\n  HasTemplateBind: " + HasTemplateBind +
                           "\r\n  DoesReferToStyle: " + DoesReferToStyle +
                           "\r\n  HasEventSet: " + HasEventSet +
                           "\r\n  IsKeySet: " + IsKeySet +
                           "\r\n  HowTemplateIsSet: " + HowTemplateIsSet +
                           "\r\n  DataSourceType: " + DataSourceType +
                           "\r\n  HasDataBinding: " + HasDataBinding + 
                           "\r\n  TriggerTarget: " + TriggerTarget + 
                           "\r\n  TriggerSource: " + TriggerSource);
        }

        public string Kind;
        public string TemplateType;
        public string TemplatedControlType;
        public string TemplatePropertyName;
        public string TargetTypeSet;
        public string TemplateRootType;
        public string TemplateChildType;
        public bool HasPropertyTrigger;

        public bool HasFreezableTriggerSetter;
        public string FreezableSetterValue;

        public string HasEventTrigger;
        public bool HasTemplateBind;
        public bool DoesReferToStyle;
        public bool HasEventSet;
        public bool IsKeySet;
        public string HowTemplateIsSet;
        public string DataSourceType;
        public bool HasDataBinding;
        public string TriggerTarget;
        public string TriggerSource;
    }
}

