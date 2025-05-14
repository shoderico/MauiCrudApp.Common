using MauiCrudApp.Common.Interfaces;
using MauiCrudApp.Common.Controls;

namespace $safeprojectname$
{
    public partial class AppShell : ShellBase
    {
        public AppShell(IDialogService dialogService) : base(dialogService)
        {
            InitializeComponent();
        }
    }
}
