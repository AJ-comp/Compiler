using ApplicationLayer.Models.GraphModels;
using GalaSoft.MvvmLight;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class GraphLayoutViewModel : ViewModelBase
    {
        private bool _isVisible;
        private string layoutAlgorithmType;
        private List<string> _layoutAlgorithmTypes = new List<string>();

        #region Public Properties
        public PocGraph Graph { get; private set; }
        public IReadOnlyList<string> LayoutAlgorithmTypes => _layoutAlgorithmTypes;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
            }
        }

        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                this.RaisePropertyChanged(nameof(LayoutAlgorithmType));
            }
        }
        #endregion

        public GraphLayoutViewModel(PocGraph graph)
        {
            Graph = graph;

            //Add Layout Algorithm Types
            _layoutAlgorithmTypes.Add("BoundedFR");
            _layoutAlgorithmTypes.Add("Circular");
            _layoutAlgorithmTypes.Add("CompoundFDP");
            _layoutAlgorithmTypes.Add("EfficientSugiyama");
            _layoutAlgorithmTypes.Add("FR");
            _layoutAlgorithmTypes.Add("ISOM");
            _layoutAlgorithmTypes.Add("KK");
            _layoutAlgorithmTypes.Add("LinLog");
            _layoutAlgorithmTypes.Add("Tree");

            //Pick a default Layout Algorithm Type
            LayoutAlgorithmType = "Tree";
        }
    }
}
