using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Shell;

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
