using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Controls;

namespace MauiCrudApp.Example
{
    public partial class AppShell : ShellBase
    {
        public AppShell(IDialogService dialogService) : base(dialogService)
        {
            InitializeComponent();
        }
    }
}
