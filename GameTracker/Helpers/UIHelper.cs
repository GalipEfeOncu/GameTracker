using DevExpress.XtraEditors;
using System.Reflection;
using System.Windows.Forms;

namespace GameTracker.Helpers
{
    /// <summary>
    /// UI bileşenlerinin yapılandırılması ve görsel optimizasyonlar için yardımcı sınıf.
    /// </summary>
    public static class UiHelper
    {
        /// <summary>
        /// FlowLayoutPanel kontrollerinin temel ayarlarını yapar.
        /// </summary>
        public static void InitializeFlowPanels(params FlowLayoutPanel[] panels)
        {
            foreach (var panel in panels)
            {
                panel.AutoSize = false;
                panel.FlowDirection = FlowDirection.LeftToRight;
                panel.WrapContents = true;
                panel.Padding = new Padding(0);
                panel.AutoScroll = false;
            }
        }

        /// <summary>
        /// Titremeyi önlemek için (Double Buffering) özelliğini formdaki belirli kontrollere açar.
        /// </summary>
        public static void EnableDoubleBuffering(params Control[] controls)
        {
            if (SystemInformation.TerminalServerSession)
                return;

            var doubleBufferedProperty = typeof(Control).GetProperty(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            foreach (var control in controls)
            {
                doubleBufferedProperty?.SetValue(control, true, null);
            }
        }
    }
}