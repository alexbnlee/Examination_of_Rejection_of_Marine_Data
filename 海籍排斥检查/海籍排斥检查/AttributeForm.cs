using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 海籍排斥检查
{
    public partial class AttributeForm : Form
    {
        public AttributeForm()
        {
            InitializeComponent();
        }

        private void Attribute_Load(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
            textBox3.Focus();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }
    }
}
