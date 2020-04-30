using GalaSoft.MvvmLight;

namespace ApplicationLayer.ViewModels.DialogViewModels.OptionViewModels
{
    public abstract class OptionDialogMainViewModel : ViewModelBase
    {
        public abstract void Commit();
        public abstract void RollBack();
    }
}
