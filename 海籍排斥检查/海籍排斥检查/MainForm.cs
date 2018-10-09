using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using 海籍排斥检查;
using ESRI.ArcGIS.SystemUI;

namespace lesson1._1
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        IGeometry pBasicGeo;
        IGeometry pGeometry;
        IFeature pFeature;
        string typeValue;
        IElement pElement;

        private IGeometry GetBasicGeo(string typevalue) //获取ParcelBasic图层中指定Type值的的Geometry！
        {
            IFeatureLayer pFeatureLayer = null;
            for (int i = 0; i < axMapControl1.Map.LayerCount;i++ )
            {
                if (axMapControl1.Map.get_Layer(i).Name == "ParcelBasic")
                {
                    pFeatureLayer = axMapControl1.Map.get_Layer(i) as IFeatureLayer;
                    break;
                }
            }
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = "Type = '" + typevalue + "'";
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(pQueryFilter, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IGeometry pTempGeo = pFeature.Shape;
            while (true)
            {
                pFeature = pFeatureCursor.NextFeature();
                if(pFeature == null)
                    break;
                ITopologicalOperator pTopo = pTempGeo as ITopologicalOperator;
                pTempGeo = pTopo.Union(pFeature.Shape);
            }
            return pTempGeo;
        }

        private void button1_Click(object sender, EventArgs e)  //执行双击的事件
        {
            IEnumFeature pEnumFeature = axMapControl1.Map.FeatureSelection as IEnumFeature;
            pFeature = pEnumFeature.Next();
            if (pFeature == null)
                return;
            pGeometry = new PolygonClass();
            pGeometry = pFeature.Shape;

            //加载检测文本框
            CheckForm checkForm = new CheckForm();
            checkForm.treeView1.Nodes[0].Text = axMapControl1.Map.get_Layer(0).Name;    //加载图层名称
            checkForm.treeView1.Nodes[0].Nodes[0].Text = pFeature.get_Value(pFeature.Fields.FindField("Id")).ToString();    //加载Id值
            checkForm.textBox1.Text = pFeature.get_Value(pFeature.Fields.FindField("FID")).ToString();  //加载FID的值
            checkForm.textBox2.Text = pFeature.get_Value(pFeature.Fields.FindField("Id")).ToString();   //加载Id的值
            checkForm.comboBox1.Text = pFeature.get_Value(pFeature.Fields.FindField("Type")).ToString(); //加载Type的值
            checkForm.button2.Click += new System.EventHandler(CheckButton2);   //添加检测事件
            checkForm.button3.Click += new System.EventHandler(CheckButton3);
            checkForm.Load += new System.EventHandler(CheckLoad);
            checkForm.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CheckClosing);
            checkForm.ShowDialog();
        }

        private void CheckButton2(object sender, EventArgs e)   //检测是否有冲突
        {
            //检查属性值是否有错误
            typeValue = ((sender as Button).FindForm() as CheckForm).comboBox1.Text;
            bool OverlapsOrContains = IsOverlaps(pGeometry, typeValue.Trim());
            if (OverlapsOrContains)
            {
                //MessageBox.Show("该绘制区域与基础图层发生冲突！", "警告", MessageBoxButtons.OK , MessageBoxIcon.Warning);
                ((sender as Button).FindForm() as CheckForm).result.Text = "该绘制区域与基础图层发生冲突！";
                ((sender as Button).FindForm() as CheckForm).result.ForeColor = Color.Red;
                ((sender as Button).FindForm() as CheckForm).button3.Enabled = true;
                ((sender as Button).FindForm() as CheckForm).button3.ForeColor = Color.Red;
            }
            else
            {
                //MessageBox.Show("恭喜~没有冲突！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                ((sender as Button).FindForm() as CheckForm).result.Text = "恭喜~没有冲突！";
                ((sender as Button).FindForm() as CheckForm).result.ForeColor = Color.Black;
                ((sender as Button).FindForm() as CheckForm).button3.Enabled = false;
                ((sender as Button).FindForm() as CheckForm).button3.ForeColor = Color.Black;
            }
        }

        private void CheckButton3(object sender, EventArgs e)   //显示冲突区域
        {
            IGraphicsContainer pGC = axMapControl1.Map as IGraphicsContainer;
            ITopologicalOperator pTopo = pBasicGeo as ITopologicalOperator;
            IGeometry pTempGeo = pTopo.Intersect(pGeometry, esriGeometryDimension.esriGeometry2Dimension);
            pElement = new PolygonElementClass();
            pElement.Geometry = pTempGeo;
            pGC.AddElement(pElement, 0);
            axMapControl1.ActiveView.Refresh();
            (sender as Button).ForeColor = Color.Black;
        }

        private void CheckLoad(object sender, EventArgs e)  //窗体加载事件
        {
            (sender as CheckForm).Location = new System.Drawing.Point(Left + Width - (sender as CheckForm).Width - 20, 
                Top + (Height - (sender as CheckForm).Height) / 2);
        }  

        private void CheckClosing(object sender, FormClosingEventArgs e)    //窗体关闭事件
        {
            //检查属性值是否有错误
            typeValue = (sender as CheckForm).comboBox1.Text;
            bool OverlapsOrContains = IsOverlaps(pGeometry, typeValue.Trim());
            if (OverlapsOrContains)
            {
                DialogResult dr = MessageBox.Show("是否保留绘制的图形？\n选择 “确定”，则保留绘制图形并为 Type 赋值为 “空”，\n选择 “取消”，则删除绘制图形！",
                "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.Cancel)
                {
                    pFeature.Delete();
                    IGraphicsContainer pGC = axMapControl1.Map as IGraphicsContainer;
                    pGC.DeleteElement(pElement);
                    axMapControl1.ActiveView.Refresh();
                }
                else if (dr == DialogResult.OK)
                {
                    pFeature.set_Value(pFeature.Fields.FindField("Type"), "空");
                }
            }
        }

        private bool IsOverlaps(IGeometry pFeatureGeo, string typevalue)    //判断几何图形间是否有重叠
        {
            bool isoverlaps = false;
            switch(typevalue)
            {
                case "001":
                    pBasicGeo = GetBasicGeo("003");
                    break;
                case "002":
                    pBasicGeo = GetBasicGeo("004");
                    break;
                case "003":
                    pBasicGeo = GetBasicGeo("001");
                    break;
                case "004":
                    pBasicGeo = GetBasicGeo("002");
                    break;
                default:
                    break;
            }

            IRelationalOperator pRelaOperator = pBasicGeo as IRelationalOperator;
            isoverlaps = pRelaOperator.Overlaps(pFeatureGeo) || pRelaOperator.Contains(pFeatureGeo);
            return isoverlaps;
        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)    //图形画完后会双击
        {
            button1_Click(button1, new EventArgs());
        }
    }
}
