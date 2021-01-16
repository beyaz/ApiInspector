using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        static void AddNewAssertion(Scope scope,Assertion assertion)
        {
            scope.Get(SelectedScenario).Assertions.Add(assertion);
            scope.Update(SelectedAssertion, assertion);
            scope.PublishEvent(AssertionEvent.NewAssertionAdded);
        }
      


        Scope scope;

        List<Assertion> Assertions => scope.Get(SelectedScenario).Assertions;
        Assertion selectedAssertion => scope.Get(SelectedAssertion);

        internal void Connect(Scope scope)
        {
            this.scope = scope;
            
            AttachEvents();

            BuildAssertionList();
            UpdateNumbers();
        }

        void AttachEvents()
        {
            scope.OnUpdate(SelectedAssertion, UpdateNumbers);
            scope.OnUpdate(SelectedAssertion, ShowSelectedAssertion);
            scope.OnUpdate(SelectedAssertion, ArrangeRemoveAssertionButtonVisibility);
            scope.OnUpdate(SelectedAssertion, MakePressedSelectedAssertion);

            scope.OnUpdate(SelectedScenario, BuildAssertionList);

            scope.SubscribeEvent(AssertionEvent.RemoveSelectedAssertion, () =>
            {
                Assertions.Remove(selectedAssertion);
                BuildAssertionList();

            });
        }

        void ShowSelectedAssertion()
        {
            CurrentContent = CreateEditor(selectedAssertion);
        }


        /// <summary>
        ///     Sets the content of the current.
        /// </summary>
        FrameworkElement CurrentContent
        {
            set
            {
                contentContainer.Children.Clear();
                contentContainer.Children.Add(value);
            }
        }

        void MakePressedSelectedAssertion()
        {
            var actionButton = FindSelectedActionButton();

            if (actionButton != null)
            {
                actionButton.IsPressed = true;
            }
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

        void ArrangeRemoveAssertionButtonVisibility()
        {
            if (scope.Contains(SelectedAssertion) && Assertions.Count > 1)
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

            foreach (var assertion in assertions)
            {
               AddNewAssertion(scope,assertion);
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
                    SqlDatabaseName = Databases.Boa.ToString()
                },
                Expected = new ValueAccessInfo
                {
                    SqlDatabaseName = Databases.Boa.ToString()
                }
            };

            AddNewAssertion(scope,assertion);
        }

        void OnRemoveSelectedAssertionClicked(object sender, RoutedEventArgs e)
        {
            scope.PublishEvent(AssertionEvent.RemoveSelectedAssertion);
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
                    scope.Update(SelectedAssertion,assertion);
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
                    Suggestions = AssertionOperatorNames.GetDescriptions()
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
                    Suggestions = Enum.GetNames(typeof(Databases))
                };
                Bind(databaseNameEditor,AutoCompleteTextBox.TextProperty,data,nameof(data.SqlDatabaseName));

                var calculateFromDatabase = new CheckBox
                {
                    Content = "From Database"
                };

                calculateFromDatabase.Checked += (s, e) =>
                {
                    data.ValueAccessType          = ValueAccessType.FetchFromDatabase;
                    databaseNameEditor.Visibility = Visibility.Visible;
                };

                calculateFromDatabase.Unchecked += (s, e) =>
                {
                    data.ValueAccessType          = ValueAccessType.ConstantValue;
                    databaseNameEditor.Visibility = Visibility.Collapsed;
                };

                databaseNameEditor.Visibility = Visibility.Collapsed;

                return NewGridWithColumns(new[]{"Auto","*"},calculateFromDatabase, databaseNameEditor.WithMarginLeft(5));
            });
            

            var sqlEditor = fun(() =>
            {
                var editor = new SQLTextEditor();

                editor.SetAutoComplete(new List<string>{"$Input.UserName}","$Output.UserName2}"});

                editor.TextChanged += (s, e) => { data.Sql = editor.Text; };

                return editor;
            });

            return NewStackPanel(firstRow(), sqlEditor()).WithIndent(5);
        }
    }


    

}
