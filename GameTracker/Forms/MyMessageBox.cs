using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace GameTracker
{
    public partial class MyMessageBox : XtraForm
    {
        public MyMessageBox()
        {
            InitializeComponent();
        }

        // Statik metot, böylece MyMessageBox.Show(...) şeklinde çağrılabilir.
        public static DialogResult Show(string message, string title = "Warning")
        {
            Form overlay = new Form();
            try
            {
                // Ekranı karartan overlay formu oluştur
                overlay.StartPosition = FormStartPosition.Manual;
                overlay.FormBorderStyle = FormBorderStyle.None;
                overlay.Opacity = 0.50d; // %50 saydamlık
                overlay.BackColor = Color.Black;
                overlay.WindowState = FormWindowState.Maximized;
                overlay.TopMost = true;
                overlay.ShowInTaskbar = false;
                overlay.Show();

                using (var msgForm = new MyMessageBox())
                {
                    msgForm.lblMessage.Text = message;
                    msgForm.lblTitle.Text = title;

                    // Mesaj kutusu overlay'in üzerinde olsun
                    msgForm.TopMost = true;

                    System.Media.SystemSounds.Hand.Play();
                    return msgForm.ShowDialog();
                }
            }
            finally
            {
                overlay.Dispose();
            }
        }

        // Butona tıklandığında formu kapat
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}