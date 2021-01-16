using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.Components;
using ApiInspector.Models;
using BOA.Common.Types;
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
        public static readonly DependencyProperty AssertionDataProperty = DependencyProperty.Register("AssertionData", typeof(Assertion), typeof(AssertionsEditor), new PropertyMetadata(default(Assertion)));

        bool HasSelectedAssertion => scope.Contains(SelectedAssertion) && scope.Get(SelectedAssertion) != null;

        void AddNewAssertion(Assertion assertion)
        {
            Assertions.Add(assertion);
            selectedAssertion = assertion;
        }
      


        Scope scope;

        List<Assertion> Assertions => scope.Get(SelectedScenario).Assertions;

        Assertion selectedAssertion
        {
            get => scope.Get(SelectedAssertion);
            set=>scope.Update(SelectedAssertion,value);
        }
            

        internal void Connect(Scope scope)
        {
            this.scope = new Scope
            {
                {SelectedScenario, scope.Get(SelectedScenario)}
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
                    var assertion = (Assertion)child.GetValue(AssertionDataProperty);

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
            var assertions = new List<Assertion>(Assertions);

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
            var assertion = new Assertion
            {
                Actual = new ValueAccessInfo
                {
                    DatabaseName = Databases.Boa.ToString()
                },
                Expected = new ValueAccessInfo
                {
                    DatabaseName = Databases.Boa.ToString()
                }
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


        static FrameworkElement CreateEditor(Assertion assertion)
        {
            var descriptionEditor = new TextBox();

            Bind(descriptionEditor,TextBox.TextProperty,assertion,nameof(assertion.Description));

            var firstRow = NewStackPanel(NewBoldTextBlock("Description"), descriptionEditor);

            var createLeft = fun(() =>
            {
                var editor = CreateEditor(assertion.Actual);

                return NewGroupBox(NewBoldTextBlock("Actual"), editor).UpdatePadding(5);
            });

            var createExpected = fun(() =>
            {
                var editor = CreateEditor(assertion.Expected);

                return NewGroupBox(NewBoldTextBlock("Expected"), editor).UpdatePadding(5);
            });

            var createOperatorEditor = fun(() =>
            {
                var editor = new IntellisenseTextBox
                {
                    Suggestions = AssertionOperatorNames.GetDescriptions(),
                    IsTextAlignmentCenter = true
                };

                Bind(editor,AutoCompleteTextBox.TextProperty,assertion.OperatorName,nameof(assertion.OperatorName));

                return NewStackPanel(NewBoldTextBlock("Operator"), editor)
                       .WithMargin(new Thickness(10, 0, 10, 0))
                       .WithVerticalAlignmentCenter();
            });


            var secondRow = NewGridWithColumns(new[] {10, 4, 10}, createLeft() , createOperatorEditor(), createExpected());

            return NewStackPanel(firstRow, secondRow);

        }

        static FrameworkElement CreateEditor(ValueAccessInfo data)
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

                editor.SetAutoComplete(new List<string>{"Input.UserName}","Output.UserName2}"});

                editor.TextChanged += (s, e) => { data.Text = editor.Text; };

                return editor;
            });

            return NewStackPanel(firstRow(), sqlEditor()).WithIndent(5);
        }
    }


    

}
