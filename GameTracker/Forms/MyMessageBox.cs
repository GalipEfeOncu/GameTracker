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

        // Basit OK mesajı
        public static DialogResult Show(string message, string title = "Warning")
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Gelişmiş Mesaj (Yes/No veya OK)
        public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Form overlay = new Form();
            try
            {
                // Overlay Ayarları
                overlay.StartPosition = FormStartPosition.Manual;
                overlay.FormBorderStyle = FormBorderStyle.None;
                overlay.Opacity = 0.50d;
                overlay.BackColor = Color.Black;
                overlay.WindowState = FormWindowState.Maximized;
                overlay.TopMost = true;
                overlay.ShowInTaskbar = false;
                overlay.Show();

                using (var msgForm = new MyMessageBox())
                {
                    msgForm.lblMessage.Text = message;
                    msgForm.lblTitle.Text = title;
                    msgForm.TopMost = true;

                    if (buttons == MessageBoxButtons.YesNo)
                    {
                        // 1. YES / NO Modu: İki buton yan yana
                        msgForm.btnOK.Text = "Yes";
                        msgForm.btnOK.DialogResult = DialogResult.Yes;
                        msgForm.btnOK.Location = new Point(190, 145);

                        msgForm.btnCancel.Text = "No";
                        msgForm.btnCancel.DialogResult = DialogResult.No;
                        msgForm.btnCancel.Visible = true;
                    }
                    else
                    {
                        msgForm.btnOK.Text = "OK";
                        msgForm.btnOK.DialogResult = DialogResult.OK;
                        // OK Butonunu Ortala
                        msgForm.btnOK.Location = new Point(110, 145);
                        // Cancel'ı gizle
                        msgForm.btnCancel.Visible = false;
                    }

                    // Ses Efekti
                    if (icon == MessageBoxIcon.Question) System.Media.SystemSounds.Question.Play();
                    else System.Media.SystemSounds.Hand.Play();

                    return msgForm.ShowDialog();
                }
            }
            finally
            {
                overlay.Dispose();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}