using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using BOA.Common.Types;
using Mono.Cecil;
using WpfControls;
using static ApiInspector.Keys;
using static ApiInspector.WPFExtensions;
using static FunctionalPrograming.FPExtensions;

namespace ApiInspector.MainWindow
{
    /// <summary>
    /// Interaction logic for AssertionsEditor.xaml
    /// </summary>
    public partial class AssertionsEditor
    {
        public static readonly DependencyProperty AssertionDataProperty = DependencyProperty.Register("AssertionData", typeof(AssertionInfo), typeof(AssertionsEditor), new PropertyMetadata(default(AssertionInfo)));

        bool HasSelectedAssertion => scope.Contains(SelectedAssertion) && scope.Get(SelectedAssertion) != null;

        void AddNewAssertion(AssertionInfo assertionInfo)
        {
            Assertions.Add(assertionInfo);
            selectedAssertion = assertionInfo;
        }

        MethodDefinition methodDefinition => scope.Get(SelectedMethodDefinition);
      


        internal Scope scope;

        List<AssertionInfo> Assertions => scope.Get(SelectedScenario).Assertions;

        public AssertionInfo selectedAssertion
        {
            get => scope.Get(SelectedAssertion);
            set=>scope.Update(SelectedAssertion,value);
        }
            

        internal void Connect(Scope scope)
        {
            this.scope = new Scope
            {
                {SelectedScenario, scope.Get(SelectedScenario)},
                {SelectedMethodDefinition, scope.Get(SelectedMethodDefinition)}
            };

            AttachEvents();

            BuildAssertionList();
        }

        void AttachEvents()
        {
            scope.OnUpdate(SelectedAssertion, UpdateNumbers);
            scope.OnUpdate(SelectedAssertion, ShowSelectedAssertion);
            scope.OnUpdate(SelectedAssertion, ArrangeRemoveAssertionButtonVisibility);
            scope.OnUpdate(SelectedAssertion, MakePressedSelectedAssertion);   
            
        }

        void ShowSelectedAssertion()
        {
            if (HasSelectedAssertion)
            {
                CurrentContent = CreateEditor(selectedAssertion);    
            }
            else
            {
                CurrentContent = null;
            }
            
        }


        /// <summary>
        ///     Sets the content of the current.
        /// </summary>
        FrameworkElement CurrentContent
        {
            set
            {
                contentContainer.Children.Clear();
                if (value != null)
                {
                    contentContainer.Children.Add(value);    
                }
                
            }
        }

        void MakePressedSelectedAssertion()
        {
            if (!HasSelectedAssertion)
            {
                return;
            }

            ActionButton FindSelectedActionButton()
            {
                foreach (ActionButton child in assertionNumbersContainer.Children)
                {
                    var assertion = (AssertionInfo)child.GetValue(AssertionDataProperty);

                    if (assertion == selectedAssertion)
                    {
                        return child;
                    }
                }

                return null;
            }

            var actionButton = FindSelectedActionButton();
            if (actionButton != null)
            {
                actionButton.IsPressed = true;
            }
        }

        

        

        void ArrangeRemoveAssertionButtonVisibility()
        {
            if (HasSelectedAssertion)
            {
                removeScenarioButton.Visibility = Visibility.Visible;
                return;
            }

            removeScenarioButton.Visibility = Visibility.Hidden;
        }

        void BuildAssertionList()
        {
            var assertions = new List<AssertionInfo>(Assertions);

            Assertions.Clear();

            if (assertions.Count >0)
            {
                foreach (var assertion in assertions)
                {
                    AddNewAssertion(assertion);
                }
            }
            else
            {
                selectedAssertion = null;
            }

        
        }



        public AssertionsEditor()
        {
            InitializeComponent();
        }

        void OnAddNewAssertionClicked(object sender, RoutedEventArgs e)
        {
            var assertion = new AssertionInfo
            {
                Actual = new ValueAccessInfo
                {
                    DatabaseName = Databases.Boa.ToString()
                },
                Expected = new ValueAccessInfo
                {
                    DatabaseName = Databases.Boa.ToString()
                },
                OperatorName = AssertionOperatorNames.IsEquals
            };

            AddNewAssertion(assertion);
        }

        void OnRemoveSelectedAssertionClicked(object sender, RoutedEventArgs e)
        {
            Assertions.Remove(selectedAssertion);
            BuildAssertionList();
        }

        void UpdateNumbers()
        {
            assertionNumbersContainer.Children.Clear();

            var i = 1;

            foreach (var assertion in Assertions)
            {
                var actionButton = new ActionButton
                {
                    Text = i.ToString()
                };

                actionButton.SetValue(AssertionDataProperty,assertion);

                actionButton.Click += (s, e) =>
                {
                    selectedAssertion = assertion;
                };

                assertionNumbersContainer.Children.Add(actionButton);
                
                i++;
            }

            VerticalIndent(assertionNumbersContainer,10);
        }


         FrameworkElement CreateEditor(AssertionInfo assertionInfo)
         {

             var firstRow = fun(() =>
             {
                 var descriptionEditor = new TextBox();

                 Bind(descriptionEditor,TextBox.TextProperty,assertionInfo,nameof(assertionInfo.Description));

                 return NewStackPanel(NewBoldTextBlock("Description"), descriptionEditor);
             });

             var secondRow = fun(() =>
             {
                 var createLeft = fun(() =>
                 {
                     var editor = CreateEditor(assertionInfo.Actual);

                     return NewGroupBox(NewBoldTextBlock("Actual"), editor).UpdatePadding(5);
                 });

                 var createExpected = fun(() =>
                 {
                     var editor = CreateEditor(assertionInfo.Expected);

                     return NewGroupBox(NewBoldTextBlock("Expected"), editor).UpdatePadding(5);
                 });

                 var createOperatorEditor = fun(() =>
                 {
                     var editor = new IntellisenseTextBox
                     {
                         Suggestions           = AssertionOperatorNames.GetDescriptions(),
                         IsTextAlignmentCenter = true
                     };

                     Bind(editor,AutoCompleteTextBox.TextProperty,assertionInfo,nameof(assertionInfo.OperatorName));

                     return NewStackPanel(NewBoldTextBlock("Operator"), editor)
                            .WithMargin(new Thickness(10, 0, 10, 0))
                            .WithVerticalAlignmentCenter();
                 });


                 return NewGridWithColumns(new[] {10, 4, 10}, createLeft() , createOperatorEditor(), createExpected());

             });

            var thirdRow = fun(() =>
            {
                var getErrorText = fun(() =>
                {
                    if (!scope.Contains(AssertionErrorMap))
                    {
                        return null;
                    }

                    var map = scope.Get(AssertionErrorMap);
                    if (map.Key == assertionInfo)
                    {
                        return map.Value;
                    }

                    return null;

                });

                var foreground = Brushes.Crimson;

                var resultIndicator = new TextBox
                {
                    TextWrapping                = TextWrapping.Wrap,
                    AcceptsReturn               = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Text                        = getErrorText(),
                    Foreground                  = foreground,
                    BorderBrush                 = foreground,
                };

                var label = new TextBlock
                {
                    FontWeight = FontWeights.Bold,
                    Text       = "Result",
                    Foreground = foreground,
                };

                var panel = NewGroupBox(label, resultIndicator);

                panel.Visibility = string.IsNullOrWhiteSpace(resultIndicator.Text) ? Visibility.Collapsed : Visibility.Visible;

                return panel;
            });

            return NewGridWithRows(new []{"Auto","70","30"},firstRow(), secondRow(),thirdRow());

        }




        FrameworkElement CreateEditor(ValueAccessInfo data)
        {
            var firstRow = fun(() =>
            {
                var databaseNameEditor = new IntellisenseTextBox
                {
                    Suggestions = Enum.GetNames(typeof(Databases)),
                    Text = data.DatabaseName
                };
                Bind(databaseNameEditor,AutoCompleteTextBox.TextProperty,data,nameof(data.DatabaseName));

                var arrangeDatabaseNameEditorVisibility = fun(() =>
                {
                    if (data.FetchFromDatabase)
                    {
                        databaseNameEditor.Visibility = Visibility.Visible;
                        return;
                    }
                    databaseNameEditor.Visibility = Visibility.Collapsed;
                });

                var createFetchFromDatabaseEditor = fun(() =>
                {
                    var checkBox = new CheckBox
                    {
                        Content   = "From Database",
                        IsChecked = data.FetchFromDatabase
                    };

                    checkBox.Checked += (s, e) =>
                    {
                        data.FetchFromDatabase = true;
                        arrangeDatabaseNameEditorVisibility();
                    };

                    checkBox.Unchecked += (s, e) =>
                    {
                        data.FetchFromDatabase = false;
                        arrangeDatabaseNameEditorVisibility();
                    };

                    return checkBox;

                });
               

                arrangeDatabaseNameEditorVisibility();

                return NewGridWithColumns(new[]{"Auto","*"},createFetchFromDatabaseEditor(), databaseNameEditor.WithMarginLeft(5));
            });
            

            var sqlEditor = fun(() =>
            {
                var editor = new SQLTextEditor
                {
                    Text = data.Text
                };

                var suggestions = CecilHelper.GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(methodDefinition);

                editor.SetAutoComplete(suggestions.ToList());

                editor.TextChanged += (s, e) => { data.Text = editor.Text; };

                return editor;
            });

            return NewGridWithRows(new[] {"Auto", "*"}, firstRow(), sqlEditor());
        }

      
    }
}
