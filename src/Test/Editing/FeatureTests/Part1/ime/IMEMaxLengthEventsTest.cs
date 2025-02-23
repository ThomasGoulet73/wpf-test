// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Test;
using Microsoft.Test.Discovery;

using Test.Uis.Data;
using Test.Uis.Loggers;
using Test.Uis.TestTypes;
using Test.Uis.Utils;

namespace Test.Uis.TextEditing
{
    [Test(1, "IME", "IMEMaxLengthEventsTest_Korean", MethodParameters = "/TestCaseType:IMEMaxLengthEventsTest /locale=Korean", Timeout = 120, Keywords = "KoreanIME")]
    [Test(1, "IME", "IMEMaxLengthEventsTest_Japanese", MethodParameters = "/TestCaseType:IMEMaxLengthEventsTest /locale=Japanese", Timeout = 120, Keywords = "JapaneseIME")]
    [Test(1, "IME", "IMEMaxLengthEventsTest_ChinesePinyin", MethodParameters = "/TestCaseType:IMEMaxLengthEventsTest /locale=ChinesePinyin", Timeout = 120, Keywords = "ChinesePinyinIME")]
    [Test(1, "IME", "IMEMaxLengthEventsTest_ChineseQuanPin", MethodParameters = "/TestCaseType:IMEMaxLengthEventsTest /locale=ChineseQuanPin", Timeout = 120, Keywords = "ChineseQuanPinIME")]
    // This test checks for TextChanged and SelectionChanged event count for IME input with MaxLength constraint (when filtering occurs due to MaxLength).
    // The expected event count is different for XP/Vista because of variation in behavior across different IME versions on XP/Vista.
    public class IMEMaxLengthEventsTest : ManagedCombinatorialTestCase
    {
        #region Main flow

        /// <summary>Starts the combination</summary>
        protected override void DoRunCombination()
        {
            if (_textBox == null)
            {
                _textBox = new TextBox();
                _textBox.Height = 200;
                _textBox.FontSize = 24;

                _textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
                _textBox.SelectionChanged += new RoutedEventHandler(textBox_SelectionChanged);
            }
            if (_testTextBox == null)
            {
                _testTextBox = new TextBox();
                _testTextBox.Height = 100;
                _testTextBox.FontSize = 24;
            }

            if (_panel == null)
            {
                _panel = new StackPanel();
                _panel.Children.Add(_textBox);
                _panel.Children.Add(_testTextBox);
                MainWindow.Content = _panel;
            }

            SetTestVariables();

            QueueDelegate(PerformTestActions);
        }

        private void PerformTestActions()
        {
            if (!_isIMESetupDone)
            {
                Log("Load IME keyboard");
                IMEHelper.SetUpIMEKeyboardLayout(_locale, _testTextBox, MainWindow);
                _isIMESetupDone = true;
            }            

            // Put the caret at the end of the contents
            _textBox.Select(_textBox.Text.Length, 0);
            // Put the focus in the actual TextBox where test is done
            _textBox.Focus();
            Microsoft.Test.Threading.DispatcherHelper.DoEvents();

            KeyboardInput.TypeString(_contentToTypeInIME);
            Microsoft.Test.Threading.DispatcherHelper.DoEvents(IMEHelper.CiceroWaitTimeMs);
            
            VerifyEventCountAfterTyping();

            NextCombination();
        }

        private void SetTestVariables()
        {
            switch (_locale)
            {
                case IMELocales.Korean:
                    _contentToTypeInIME = koreanTypeSequence;
                    _composedStringByIME = koreanCompositedString;
                    if (Environment.OSVersion.Version.Major < 6)
                    {
                        _expectedTextChangedEventCount = koreanExpectedTextChangedEventCountXP;
                        _expectedSelectionChangedEventCount = koreanExpectedSelectionChangedEventCountXP;
                    }
                    else
                    {
                        _expectedTextChangedEventCount = koreanExpectedTextChangedEventCountVista;
                        _expectedSelectionChangedEventCount = koreanExpectedSelectionChangedEventCountVista;
                    }
                    break;
                case IMELocales.Japanese:
                    _contentToTypeInIME = japaneseTypeSequence;
                    _composedStringByIME = japaneseCompositedString;
                    _expectedTextChangedEventCount = japaneseExpectedTextChangedEventCount;
                    _expectedSelectionChangedEventCount = japaneseExpectedSelectionChangedEventCount;
                    break;
                case IMELocales.ChinesePinyin:
                    _contentToTypeInIME = chinesePinyinTypeSequence;
                    _composedStringByIME = chinesePinyinCompositedString;
                    if (Environment.OSVersion.Version.Major < 6)
                    {
                        _expectedTextChangedEventCount = chinesePinyinExpectedTextChangedEventCountXP;
                        _expectedSelectionChangedEventCount = chinesePinyinExpectedSelectionChangedEventCountXP;
                    }
                    else
                    {
                        _ver = Environment.OSVersion.Version;
                        if (_ver.Major > 6 || ((6 == _ver.Major) && _ver.Minor > 1))
                        {
                            //The result "你好" here type by chinese pinyin simplafast is as same as chineseQuanPinTypeSequence in textbox,
                            //So the expectedTextChangedEventCount is as same as IME QuanPin,
                            //and the expectedSelectionChangedEventCount is as same as IME QuanPin;
                            _expectedTextChangedEventCount = chineseQuanPinExpectedTextChangedEventCountVista;
                            _expectedSelectionChangedEventCount = chineseQuanPinExpectedSelectionChangedEventCountVista;
                        }
                        else
                        {
                            _expectedTextChangedEventCount = chinesePinyinExpectedTextChangedEventCountVista;
                            _expectedSelectionChangedEventCount = chinesePinyinExpectedSelectionChangedEventCountVista;
                        }
                    }
                    break;
                case IMELocales.ChineseQuanPin:
                    _contentToTypeInIME = chineseQuanPinTypeSequence;
                    _composedStringByIME = chineseQuanPinCompositedString;
                    if (Environment.OSVersion.Version.Major < 6)
                    {
                        _expectedTextChangedEventCount = chineseQuanPinExpectedTextChangedEventCountXP;
                        _expectedSelectionChangedEventCount = chineseQuanPinExpectedSelectionChangedEventCountXP;
                    }
                    else
                    {
                        _expectedTextChangedEventCount = chineseQuanPinExpectedTextChangedEventCountVista;
                        _expectedSelectionChangedEventCount = chineseQuanPinExpectedSelectionChangedEventCountVista;
                    }
                    break;
            }

            _textBox.Text = _testTextBox.Text = string.Empty; // Clean up from previous combinations
            _textBox.IsUndoEnabled = _isUndoEnabled;
            _textBox.MaxLength = _maxLength;
            _textChangedEventCount = _selectionChangedEventCount = 0;
        }

        private void VerifyEventCountAfterTyping()
        {                
            Verifier.Verify(_textBox.Text == _composedStringByIME, "Verifying contents after typing followed by undo: Actual[" +
                _textBox.Text + "] Expected[" + _composedStringByIME + "]", true);
            Verifier.Verify(_textChangedEventCount == _expectedTextChangedEventCount,
                "Verifying TextChanged event count. Expected [" + _expectedTextChangedEventCount +
                "] Actual [" + _textChangedEventCount + "]", true);
            Verifier.Verify(_selectionChangedEventCount == _expectedSelectionChangedEventCount,
                "Verifying SelectionChanged event count. Expected [" + _expectedSelectionChangedEventCount +
                "] Actual [" + _selectionChangedEventCount + "]", true);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _textChangedEventCount++;
        }

        private void textBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            _selectionChangedEventCount++;
        }

        #endregion

        #region Private fields

        // Combinatorial engine variables; set to default values
        private IMELocales _locale = IMELocales.Korean;
        private int _maxLength = 0;
        private bool _isUndoEnabled = true;

        private StackPanel _panel;
        private TextBox _textBox;
        private TextBox _testTextBox; // Used just to set the appropriate Ime mode
        private bool _isIMESetupDone = false;

        private int _textChangedEventCount = 0;
        private int _selectionChangedEventCount = 0;

        private string _contentToTypeInIME = string.Empty;
        private string _composedStringByIME = string.Empty;

        private int _expectedTextChangedEventCount = 0;
        private int _expectedSelectionChangedEventCount = 0;

        private const string koreanTypeSequence = "qixmsqix{ENTER}";
        private const string koreanCompositedString = "뱌튼";
        private const int koreanExpectedTextChangedEventCountXP = 12;
        private const int koreanExpectedSelectionChangedEventCountXP = 13;
        private const int koreanExpectedTextChangedEventCountVista = 10;
        private const int koreanExpectedSelectionChangedEventCountVista = 13;

        private const string japaneseTypeSequence = "ae{ENTER}ae{ENTER}";
        private const string japaneseCompositedString = "あえ";
        private const int japaneseExpectedTextChangedEventCount = 6;
        private const int japaneseExpectedSelectionChangedEventCount = 9;

        private const string chinesePinyinTypeSequence = "nihao{SPACE}{ENTER}nihao{SPACE}{ENTER}";
        private const string chinesePinyinCompositedString = "你好";
        private const int chinesePinyinExpectedTextChangedEventCountXP = 8;
        private const int chinesePinyinExpectedSelectionChangedEventCountXP = 9;
        private const int chinesePinyinExpectedTextChangedEventCountVista = 14;        
        private const int chinesePinyinExpectedSelectionChangedEventCountVista = 17;

        private const string chineseQuanPinTypeSequence = "nihao{SPACE}nihao{SPACE}";
        private const string chineseQuanPinCompositedString = "你好";
        private const int chineseQuanPinExpectedTextChangedEventCountXP = 8;
        private const int chineseQuanPinExpectedSelectionChangedEventCountXP = 9;
        private const int chineseQuanPinExpectedTextChangedEventCountVista = 6;
        private const int chineseQuanPinExpectedSelectionChangedEventCountVista = 9;

        Version _ver;

        #endregion
    }
}