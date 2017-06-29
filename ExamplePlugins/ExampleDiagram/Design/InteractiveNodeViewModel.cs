﻿using System;
using System.Linq;
using NationalInstruments.Composition;
using NationalInstruments.Core;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Design;
using NationalInstruments.SourceModel;
using NationalInstruments.Shell;
using ExamplePlugins.ExampleDiagram.SourceModel;
using System.Collections.Generic;

namespace ExamplePlugins.ExampleDiagram.Design
{
    /// <summary>
    /// The ViewModel for the Interactive Node
    /// The ViewModel controls the interaction with the user
    /// </summary>
    public class InteractiveNodeViewModel : NodeViewModel
    {
        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="model">The model element we are bound to</param>
        public InteractiveNodeViewModel(Node model) :
            base(model)
        {
        }

        /// <summary>
        /// Gets the InteractiveNode that this view model is bound to
        /// </summary>
        public InteractiveNode Node
        {
            get { return (InteractiveNode)base.Model; }
        }

        public bool IsActive
        {
            get { return Node.IsActive; }
            set
            {
                using (var transaction = Node.TransactionManager.BeginTransaction("Set IsActive", TransactionPurpose.User))
                {
                    Node.IsActive = value;
                    transaction.Commit();
                }
            }
        }

        public string Sound
        {
            get { return Node.Sound; }
            set
            {
                using (var transaction = Node.TransactionManager.BeginTransaction("Set Sound", TransactionPurpose.User))
                {
                    Node.Sound = value;
                    transaction.Commit();
                }
            }
        }

        public override void ModelPropertyChanged(Element modelElement, string propertyName, TransactionItem transactionItem)
        {
            base.ModelPropertyChanged(modelElement, propertyName, transactionItem);
            if (propertyName == "IsActive")
            {
                NotifyPropertyChanged("IsActive");
            }
            else if (propertyName == nameof(Sound))
            {
                NotifyPropertyChanged(nameof(Sound));
            }
        }

        /// <summary>
        /// Returns the Uri (resource location) for our node foreground image
        /// </summary>
        protected override ResourceUri ForegroundUri
        {
            get { return new ResourceUri(GetType(), "Resources/PigNode"); }
        }


        /// <summary>
        /// Returns the render data for our node's foreground image
        /// </summary>
        public override NineGridData ForegroundImageData
        {
            get
            {
                return new ViewModelIconData(this)
                {
                    ImageUri = ForegroundUri,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    Width = 20,
                    Height = 20
                };
            }
        }
        public override PlatformVisual CreateView(PlatformVisual parent)
        {
            var view = new InteractiveNodeView();
            return view;
        }

        /// <summary>
        /// Returns the image data to use for the background
        /// </summary>
        public override NineGridData BackgroundImageData
        {
            get
            {
                // This node does not need a rendered background it is set it XAML
                return null;
            }
        }

        public static readonly ICommandEx ConfigurationItemsGroup = new ShellRelayCommand(ShellDocumentCommandHelpers.HandleNoop)
        {
            UniqueId = "ExamplePlugins.ConfigurationItemsGroup",
            LabelTitle = "Configuration"
        };

        public static readonly ICommandEx IsActiveCommand = new ShellSelectionRelayCommand(OnIsActive, UpdateIsActive)
        {
            UniqueId = "ExamplePlugins.IsActiveCommand",
            LabelTitle = "Is Active"
        };

        public static readonly ICommandEx SoundSelectionGroupCommand = new ShellRelayCommand()
        {
            UniqueId = "ExamplePlugins.SoundSelectionGroupCommand",
            LabelTitle = "Select the animal sound"
        };

        #region Sound commands

        private static readonly ICommandEx WoofCommand = CreateSoundCommand("WoofCommand", "Dog goes woof");

        private static readonly ICommandEx MeowCommand = CreateSoundCommand("MeowCommand", "Cat goes meow");

        private static readonly ICommandEx TweetCommand = CreateSoundCommand("TweetCommand", "Bird goes tweet");
        
        private static readonly ICommandEx SqueakCommand = CreateSoundCommand("SqueakCommand", "Mouse goes squeak");

        private static readonly ICommandEx MooCommand = CreateSoundCommand("MooCommand", "Cow goes moo");

        private static readonly ICommandEx CroakCommand = CreateSoundCommand("CroakCommand", "Frog goes croak");

        private static readonly ICommandEx TootCommand = CreateSoundCommand("TootCommand", "Elephant goes toot");

        private static readonly ICommandEx QuackCommand = CreateSoundCommand("QuackCommand", "Duck say quack");

        private static readonly ICommandEx BlubCommand = CreateSoundCommand("BlubCommand", "Fish go blub");

        private static readonly ICommandEx OwowowCommand = CreateSoundCommand("OwowowCommand", "Seal goes ow ow ow");

        private static readonly ICommandEx FoxCommand = new ShellRelayCommand(OnFoxCommand)
        {
            UniqueId = "ExamplePlugins.FoxCommand",
            LabelTitle = "Fox says..."
        };

        private static ICommandEx CreateSoundCommand(string name, string phrase)
        {
            return new ShellSelectionRelayCommand(OnSoundCommand)
            {
                UniqueId = $"ExamplePlugins.{name}",
                LabelTitle = phrase
            };
        }

        private static void OnSoundCommand(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            var viewModel = selection.FirstOrDefault() as InteractiveNodeViewModel;
            if (viewModel != null)
            {
                viewModel.Sound = string.Concat(parameter.LabelTitle.Split(new[] { ' ' }, 3).Skip(2));
            }
        }

        private static void OnFoxCommand(ICommandParameter parameter, ICompositionHost host, DocumentEditSite site)
        {
            NIMessageBox.Show("What does the fox say?");
        }

        #endregion

        public static bool UpdateIsActive(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            var checkableParameter = parameter as ICheckableCommandParameter;
            if (checkableParameter != null)
            {
                checkableParameter.IsChecked = CommandHelpers.GetCheckedState(selection.OfType<InteractiveNodeViewModel>(), vm => vm.IsActive);
            }
            return true;
        }

        /// <summary>
        /// This command handler is used in the right rail to change the state of the IsActive property.
        /// </summary>
        public static void OnIsActive(ICommandParameter parameter, IEnumerable<IViewModel> selection, ICompositionHost host, DocumentEditSite site)
        {
            // Get the selected interactive node view models
            var selected = selection.OfType<InteractiveNodeViewModel>();
            var checkableParameter = (ICheckableCommandParameter)parameter;
            if (selected.Any())
            {
                // Create a transaction around the setting on all view models.  This is make it a single undoable action
                using (var transaction = selected.First().TransactionManager.BeginTransaction("Set IsActive", TransactionPurpose.User))
                {
                    // Set the state of the IsActive property based on the state of the checkbox
                    foreach (var selectedItem in selected)
                    {
                        selectedItem.IsActive = (bool)checkableParameter.IsChecked;
                    }
                    // don't forget to commit the transaction
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Creates the content to put into the command bar when the node is selected
        /// </summary>
        /// <param name="context">The current presentation context</param>
        public override void CreateCommandContent(ICommandPresentationContext context)
        {
            base.CreateCommandContent(context);
            // This will add an IsActive checkbox to the right rail
            using (context.AddConfigurationPaneContent())
            {
                using (context.AddGroup(ConfigurationItemsGroup))
                {
                    context.Add(IsActiveCommand, CheckBoxFactory.ForConfigurationPane);

                    using (context.AddGroup(SoundSelectionGroupCommand, ListBoxLayoutFactory.ForConfigurationPane))
                    {
                        context.Add(WoofCommand);
                        context.Add(MeowCommand);
                        context.Add(TweetCommand);
                        context.Add(SqueakCommand);
                        context.Add(MooCommand);
                        context.Add(CroakCommand);
                        context.Add(TootCommand);
                        context.Add(QuackCommand);
                        context.Add(BlubCommand);
                        context.Add(OwowowCommand);
                        context.Add(FoxCommand);
                    }
                }
            }
        }
    }
}