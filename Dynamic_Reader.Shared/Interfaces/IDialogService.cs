using System;
using System.Threading.Tasks;

namespace Dynamic_Reader.Interfaces
{
    public interface IDialogService
    {
        Task ShowError(string message, string title, string buttonText, Action afterHideCallback);


        Task ShowMessage(string message, string title);

        Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback);

        void ShowPopUp(string message, string title);

        Task ShowMessage(
            string message,
            string title,
            string buttonConfirmText,
            string buttonCancelText,
            Action<bool> afterHideCallback);

        Task ShowMessageBox(string message, string title);
    }
}