using DGP.Genshin.Common.Core.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DGP.Genshin.Sample.Plugin
{
    [ViewModel(ViewModelType.Transient)]
    internal class SampleViewModel : ObservableObject
    {
        private string displayText;

        public string DisplayText 
        { 
            get => displayText; 
            [MemberNotNull(nameof(displayText))]
            set => SetProperty(ref displayText, value); 
        }

        public SampleViewModel()
        {
            DisplayText = 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        }
    }
}
