using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using PdfiumViewer;

namespace HRAdmin.UserControl
{
    public partial class PdfViewer : System.Windows.Forms.UserControl
    {
        public PdfiumViewer.PdfViewer Viewer { get; private set; }
        public PdfViewer()
        {
            InitializeComponent();
            Viewer = new PdfiumViewer.PdfViewer
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(Viewer);
        }
        public void LoadPdfFromStream(Stream pdfStream)
        {
            if (Viewer.Document != null)
                Viewer.Document.Dispose();
            Viewer.ZoomMode = PdfViewerZoomMode.FitBest; // or FitHeight or FitBest

            Viewer.Document = PdfiumViewer.PdfDocument.Load(pdfStream);
        }

        private void PdfViewer_Load(object sender, EventArgs e)
        {

        }
    }
}
