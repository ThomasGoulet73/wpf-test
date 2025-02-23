// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Avalon.Test.CoreUI.Trusted;
using Avalon.Test.CoreUI;
using System.Windows;
using System.Windows.Controls;
using Avalon.Test.CoreUI.Common;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections;

namespace Avalon.Test.CoreUI.Serialization
{
    /// <summary>
    /// Cell resources verification
    /// </summary>
    public class CellResources
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uie"></param>
        public static void Verify(UIElement uie)
        {
            CoreLogger.LogStatus("Inside TestResources.Verify()...");
            
            DockPanel myPanel = uie as DockPanel;

            if (null == myPanel)
            {
                throw new Microsoft.Test.TestValidationException("Should be DockPanel");
            }

            UIElementCollection myChildren = myPanel.Children;

            if (myChildren.Count != 1)
            {
                throw new Microsoft.Test.TestValidationException("Should has only 1 child");
            }
			FlowDocumentScrollViewer myTextPanel = myChildren[0] as FlowDocumentScrollViewer;

			if(null == myTextPanel)
			{
				throw new Microsoft.Test.TestValidationException("Should be FlowDocumentScrollViewer");
			}
			IEnumerator e = LogicalTreeHelper.GetChildren(myTextPanel.Document).GetEnumerator();
			e.MoveNext();


			Table myTable = (Table)e.Current;
			if (null == myTable)
            {
                throw new Microsoft.Test.TestValidationException("Should Have a table");
            }
            TableRowGroup myBody = myTable.RowGroups[0];
            TableRow myRow = myBody.Rows[0] as TableRow;
            if (null == myRow)
            {
                throw new Microsoft.Test.TestValidationException("Should Have a Row");
            }
            CoreLogger.LogStatus("Verifying Resources ...");

            ResourceDictionary myResources = myRow.Resources;

            VerifyElement.VerifyBool(null == myResources, false);
            VerifyElement.VerifyInt(myResources.Count, 1);

            String[] myKeys = new String[1];

            myResources.Keys.CopyTo(myKeys, 0);
            foreach (string key in myKeys)
                CoreLogger.LogStatus("key: " + key);

            CoreLogger.LogStatus("Verify foreground ...");
            if (false == myResources.Contains("foreground"))
                CoreLogger.LogStatus("no resources for foreground");
            else
            {
                Type myType = myResources["foreground"].GetType();

                if (null == myType)
                    CoreLogger.LogStatus("null myResources[foreground]");
                else
                    CoreLogger.LogStatus("Type1: " + myType.FullName);
            }

            SolidColorBrush myForeground = myResources["foreground"] as SolidColorBrush;

            VerifyElement.VerifyBool(null == myForeground, false);
            VerifyElement.VerifyColor(myForeground.Color, Colors.Red);


            TableCell myCell = myRow.Cells[0] as TableCell;
            if (null == myCell)
            {
                throw new Microsoft.Test.TestValidationException("Should Have a Cell");
            }

            if (((SolidColorBrush)(myCell.Foreground)).Color != ((SolidColorBrush)Brushes.Red).Color)
            {
                throw new Microsoft.Test.TestValidationException("Foreground should be red, actually: " + myCell.Foreground.ToString());
            }
        }
    }
}
