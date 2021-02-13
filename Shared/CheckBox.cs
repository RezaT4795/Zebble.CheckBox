namespace Zebble
{
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;

    public class CheckBox : Stack, FormField.IControl, IBindableInput
    {
        bool @checked, IsToggling;
        event InputChanged InputChanged;
        public readonly AsyncEvent CheckedChanged = new AsyncEvent(ConcurrentEventRaisePolicy.Queue);
        public readonly ImageView CheckedImage = new ImageView().Id("CheckedImage").Hide();
        public Alignment Alignment { get; set; } = Alignment.Right;

        public CheckBox() => CheckedChanged.Handle(UpdateCheckedState);

        event InputChanged IBindableInput.InputChanged { add => InputChanged += value; remove => InputChanged -= value; }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();

            Tapped.Handle(ToggleChanged);
            Swiped.Handle(ToggleChanged);
            PanFinished.Handle(ToggleChanged);

            await Add(CheckedImage);
        }

        public override async Task OnPreRender()
        {
            if (IsDisposing) return;
            await base.OnPreRender();

            if (Alignment == Alignment.Right)
            {
                var left = parent.ActualWidth - ActualWidth - Margin.Left() - parent.Padding.Left();

                if ((parent as Stack)?.Direction == RepeatDirection.Horizontal)
                    left -= CurrentSiblings.Except(x => x.Absolute).Sum(c => c.CalculateTotalWidth());

                Css.Margin.Left = left.LimitMin(0);
            }
        }

        object FormField.IControl.Value { get => Checked; set => Checked = (bool)value; }

        public bool Checked
        {
            get => @checked;
            set
            {
                if (@checked == value) return;
                @checked = value;
                CheckedChanged.Raise();
            }
        }

        public async Task ToggleChanged()
        {
            if (IsToggling) return;
            else IsToggling = true;
            try
            {
                @checked = !@checked;
                await CheckedChanged.Raise();
            }
            finally
            {
                IsToggling = false;
            }
        }

        Task UpdateCheckedState()
        {
            InputChanged?.Invoke(nameof(Checked));
            CheckedImage.Visible(Checked);
            return SetPseudoCssState("checked", Checked);
        }

        public override void Dispose()
        {
            CheckedChanged?.Dispose();
            base.Dispose();
        }
    }
}