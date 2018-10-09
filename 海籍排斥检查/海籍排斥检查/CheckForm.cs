using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;
using lesson1._1;

namespace 海籍排斥检查
{
    public partial class CheckForm : Form
    {
        public CheckForm()
        {
            InitializeComponent();
        }

        private void CheckForm_Load(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }

    }
}
