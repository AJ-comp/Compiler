using System.Collections.ObjectModel;

namespace WpfApp.Models.MicroControllerModels.ArmModels
{
    public class ARM : MicroController, IHierarchical<MicroController>
    {
        public ObservableCollection<MicroController> Items { get; } = new ObservableCollection<MicroController>();

        public ARM()
        {
            this.Name = "ARM";

            this.Items.Add(new ARMv6());
            this.Items.Add(new ARMv7_A());
            this.Items.Add(new ARMv8_A());
        }
    }

    public class ARMv6 : MicroController, IHierarchical<MicroController>
    {
        public ObservableCollection<MicroController> Items { get; } = new ObservableCollection<MicroController>();

        public ARMv6()
        {
            this.Name = "ARMv6";
        }
    }

    public class ARMv7_A : MicroController, IHierarchical<MicroController>
    {
        public ObservableCollection<MicroController> Items { get; } = new ObservableCollection<MicroController>();

        public ARMv7_A()
        {
            this.Name = "ARMv7_A";
        }
    }

    public class ARMv8_A : MicroController, IHierarchical<MicroController>
    {
        public ObservableCollection<MicroController> Items { get; } = new ObservableCollection<MicroController>();

        public ARMv8_A()
        {
            this.Name = "ARMv8_A";

            this.Items.Add(new Stm32L152RB());
        }
    }

    public class CortexM : MicroController, IHierarchical<MicroController>
    {
        public ObservableCollection<MicroController> Items { get; } = new ObservableCollection<MicroController>();

        public CortexM()
        {
            this.Name = "CortexM";
        }
    }

    public class Stm32L152RB : MicroController
    {
        public Stm32L152RB()
        {
            this.Name = "Stm32L152RB";
        }
    }
}
