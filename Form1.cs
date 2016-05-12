using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.ADF;
using System.Xml;
using System.Threading;

namespace OpenSpaceRouting
{
    public partial class Form1 : Form
    {
        public static double cell_size = 1;
        public int rpRows = 0;
        public int rpColumns = 0;
        public ISpatialReference m_default_reference = new UnknownCoordinateSystemClass();
        public IEnvelope OutExtent = null;
        public string origin_title = null;

        
        public bool readstartpointfromfile = true; //指示是否从文件中读取起点
        public bool readendpointfromfile = true;  //指示是否从文件中读取终点

        public int[,] open = null;  //节点加入队列，则开启列表对应位置为1
        public int[,] close = null; //节点作为中心像元访问过，则关闭列表对应位置为1
        public int[,] parentX = null;//父节点的行坐标
        public int[,] parentY = null;//父节点的列坐标
        public double[,] impedance = null; //阻抗值
        public double[,] obstacle = null;  //障碍值

        public double[,] cost = null; //最小累计耗费值
        public int[,] backlink = null; //八邻居像元的方向值
        public double[,] terrain = null; //地形高程值
        public double[,] dems = null;

        public int source_x = 0; //起点的行号x
        public int source_y = 0; //起点的列号y
        public int end_x = 0; //终点的行号x
        public int end_y = 0; //终点的列号y

        public string default_dir = "";
        public string target_layer_name = "";
        public string terrain_layer = "";
        public string obstacle_layer_name = "";

        public string out_path_name = "";
        public string start_point_name = "";
        public string end_point_name = "";

        public static IRasterLayer pBaseRasLayer = null;
        public static IRasterLayer pTerrainLayer = null;

        public string out_accu_name = "";
        public string out_parent_x_name = "";
        public string out_parent_y_name = "";

        double[] profile_line = null; //剖面线数组

        //记录图层的类型
        public Dictionary<string, rstPixelType> LayerPixelType = new Dictionary<string, rstPixelType>(20);

        public Dictionary<string, string> LayerPath = new Dictionary<string, string>(20);

        

        //public StreamWriter sw_cost = null;
        //public StreamWriter sw_px = null;
        //public StreamWriter sw_py = null;
        public enum RasterOperationType
        {
            NodataMask, //将其视为Nodata掩膜
            Updater     //将其视为更新数据
        }

        public Form1()
        {
            InitializeComponent();
        }


        public void WriteTexts2RichTextBox(RichTextBox handle, string value)
        {
            handle.Text += value + "\n";
            handle.SelectionStart = handle.TextLength;
            handle.ScrollToCaret();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            #region 添加工具
            ////Select Elements ToolItem
            //ICommand pCom = new ControlsSelectToolClass();
            //pCom.OnCreate(axMapControl1.Object);
            //axToolbarControl1.AddItem(pCom, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);

            ////Identifier ToolItem
            //pCom = new ControlsMapIdentifyToolClass();
            //pCom.OnCreate(axMapControl1.Object);
            //axToolbarControl1.AddItem(pCom, -1, -1, false, 0, esriCommandStyles.esriCommandStyleIconOnly);
            #endregion


        }

        private XmlElement ReadXmlFile(string Xfilename, string NodeName)
        {
            try
            {
                XmlDocument ManifestDoc = new XmlDocument();
                ManifestDoc.Load(Xfilename);
                XmlElement root = ManifestDoc.DocumentElement;
                XmlElement xNode = (XmlElement)root.SelectSingleNode(NodeName);

                return xNode;
            }
            catch (Exception ex)
            {
                WriteTexts2RichTextBox(richTextBox1,"Read Xml Error. Detail Info: " + ex.Message);
                return null;
            }
        }

        private void Initialization()
        {
            DateTime t_start = DateTime.Now;
            WriteTexts2RichTextBox(richTextBox1, "Initialization Start: " + t_start.ToLongTimeString());

            ParaDialog pdlg = new ParaDialog(this);
            if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlElement xNode = ReadXmlFile("config.xml", "/DialogResults/FormNode[name = 'ParaDialog']");

                default_dir = xNode.GetElementsByTagName("default_dir").Item(0).InnerText;
                target_layer_name = xNode.GetElementsByTagName("target").Item(0).InnerText;
                terrain_layer = xNode.GetElementsByTagName("terrain").Item(0).InnerText;

                start_point_name = xNode.GetElementsByTagName("start_pt").Item(0).InnerText;
                end_point_name = xNode.GetElementsByTagName("end_pt").Item(0).InnerText;

                cell_size = Convert.ToDouble(xNode.GetElementsByTagName("cell_size_x").Item(0).InnerText);
                rpRows = Convert.ToInt32(xNode.GetElementsByTagName("row_counts").Item(0).InnerText);
                rpColumns = Convert.ToInt32(xNode.GetElementsByTagName("column_counts").Item(0).InnerText);

                IRasterLayer pRasLayer = OpenRasterFile(default_dir, target_layer_name);
                pBaseRasLayer = pRasLayer;
                OutExtent = pRasLayer.VisibleExtent;

                xmin = OutExtent.XMin;
                ymax = OutExtent.YMax;

                cell_size = (pRasLayer.Raster as IRasterProps).MeanCellSize().X;
                rpRows = pRasLayer.RowCount;
                rpColumns = pRasLayer.ColumnCount;
                MyCostFunction.array_height = rpRows;
                MyCostFunction.array_width = rpColumns;
                //SetRasterLayerAsTarget(pRasLayer);

                //if (terrain_layer != "" && terrain_layer != null)
                //{
                //    pTerrainLayer = OpenRasterFile(default_dir, terrain_layer);
                //    SetAsTerrainLayer(pTerrainLayer);
                //}
                //else
                //{
                //    MessageBox.Show("未设置地形图层","注意", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                //aaa = 1;
            }


            //MessageBox.Show(rpRows + ", " + rpColumns);
            DateTime t_end = DateTime.Now;
            WriteTexts2RichTextBox(richTextBox1, "Initialization Finish: " + t_end.ToLongTimeString());
            WriteTexts2RichTextBox(richTextBox1, "Ellapsed Timespan: " + (t_end - t_start).TotalMilliseconds / 1000.0);

            origin_title = this.Text;
            this.Text = origin_title + " - Untitled";
        }
        /// <summary>
        /// 将栅格图层设置为目标图层
        /// </summary>
        /// <param name="pRasLayer">欲设置为目标的图层</param>
        public void SetRasterLayerAsTarget(IRasterLayer pRasLayer)
        {
            //if (pRasLayer != null && pBaseRasLayer != pRasLayer)
            //{
                pBaseRasLayer = pRasLayer;
                OutExtent = pRasLayer.VisibleExtent;
                cell_size = (pRasLayer.Raster as IRasterProps).MeanCellSize().X;
                rpRows = pRasLayer.RowCount;
                rpColumns = pRasLayer.ColumnCount;
                MyCostFunction.array_height = rpRows;
                MyCostFunction.array_width = rpColumns;
                GarbageRecycling();
                VariableInitialize();

                //System.Array out_data;
                //ReadPixelValues2Array(pRasLayer, out out_data);
                //SystemArray2DoubleArray(out_data, ref impedance);

                //out_data = null;
                GC.Collect();
            //}
        }
        //private void 最小耗费ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    DateTime t_start = DateTime.Now;
        //    Console.WriteLine("Start Time: {0}", t_start.ToLongTimeString());

        //    LeastCostFunction();

        //    DateTime t_end = DateTime.Now;
        //    Console.WriteLine("End Time: {0}", t_end.ToLongTimeString());

        //    TimeSpan ts = (t_end - t_start);
        //    double runtime = ts.TotalMilliseconds / 1000.0;
        //    Console.WriteLine("total seconds : {0}", runtime);
        //}


        /// <summary>
        /// 将像元值写入栅格文件
        /// </summary>
        /// <param name="InputData">输入像元数组</param>
        /// <param name="esri_type">像元类型（自定义的结构体）</param>
        /// <param name="filepath">文件目录</param>
        /// <param name="filename">文件名带后缀</param>
        #region 像元值写入栅格文件
        public void WriteArray2RasterFile(ref System.Array InputData, rstPixelType pixel_type, string filepath, string filename)
        {
            IWorkspaceFactory pFactory;
            IRasterWorkspace pRasterSpace;

            pFactory = new RasterWorkspaceFactoryClass();
            pRasterSpace = (IRasterWorkspace)pFactory.OpenFromFile(filepath, 0);

            IRasterDataset pRasterDataset = pRasterSpace.OpenRasterDataset(filename);
            //IRasterLayer pRasterLayer = new RasterLayerClass();
            IRasterDataset2 pRasterDataset2 = (IRasterDataset2)pRasterDataset;
            IRaster pRaster = (IRaster)pRasterDataset2.CreateFullRaster();
            IRaster2 pRaster2 = (IRaster2)pRaster;
            IRasterEdit pRasterEdit = (IRasterEdit)pRaster2;

            IRasterBandCollection pRasterBandCollection = (IRasterBandCollection)pRasterDataset;
            IRawPixels rawPixels = (IRawPixels)pRasterBandCollection.Item(0);
            IRawBlocks rawBlocks = (IRawBlocks)pRasterBandCollection.Item(0);
            IPnt pBlockSize = new PntClass();
            //IEnvelope pEnvelope = OutExtent;
            //pBlockSize.SetCoords(pEnvelope.Width, pEnvelope.Height);
            pBlockSize.SetCoords(rpColumns, rpRows);

            IPixelBlock pixelBlock = pRaster2.CreateCursorEx(pBlockSize).PixelBlock;
            int w = pixelBlock.Width;
            int h = pixelBlock.Height;
            //read the first pixel block
            IPnt topleftCorner = new PntClass();
            topleftCorner.SetCoords(0, 0);
            //pRaster.Read(topleftCorner, pixelBlock);
            //modify one pixel value at location (assume the raster has a pixel type of uchar)
            //IPixelBlock3 pixelBlock3 = (IPixelBlock3)pixelBlock;
            System.Array pixels = (System.Array)pixelBlock.get_SafeArray(0);

            #region 像元清零
            //for (int i = 0; i < rpColumns; i++)
            //{
            //    for (int j = 0; j < rpRows; j++)
            //    {
            //        pixels.SetValue(0, i, j);
            //    }
            //}
            //pixelBlock.set_SafeArray(0, (object)pixels);
            #endregion

            #region double[,] to float[,]
            switch (pixel_type)
            {
                case rstPixelType.PT_FLOAT:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            //pixels.SetValue(float.MinValue, i, j);
                            pixels.SetValue(Convert.ToSingle(InputData.GetValue(i, j)), i, j);
                        }
                    }
                    break;
                case rstPixelType.PT_UCHAR:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            double t = Convert.ToDouble(InputData.GetValue(i, j));
                            if (t == float.MinValue)
                                pixels.SetValue(256, i, j);
                            else
                                pixels.SetValue(Convert.ToByte(t), i, j);
                        }
                    }
                    break;
                case rstPixelType.PT_LONG:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            pixels.SetValue(Convert.ToInt32(InputData.GetValue(j, i)), i, j);
                        }
                    }
                    break;
            }
            #endregion
            pixelBlock.set_SafeArray(0, (object)pixels);

            //write the modified pixel block to the raster dataset
            pRasterEdit.Write(topleftCorner, pixelBlock);
            pRasterEdit.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pRasterEdit);

            pFactory = null;
            pRasterSpace = null;
            pRasterDataset = null;
            pRasterDataset2 = null;
            pRaster = null;
            pRaster2 = null;
            pRasterEdit = null;
            pRasterBandCollection = null;
            rawPixels = null;
            rawBlocks = null;
            pBlockSize = null;
            pixelBlock = null;
            pixels = null;
            GC.Collect();
        }
        #endregion

        /// <summary>
        /// 将double[,]数组写入已有文件
        /// </summary>
        /// <param name="InputData">输入数组</param>
        /// <param name="pixel_type">像元类型</param>
        /// <param name="filepath">文件目录</param>
        /// <param name="filename">文件名</param>
        #region 将double[,]数组写入已有文件
        public void WriteArray2RasterFile(ref double[,] InputData, rstPixelType pixel_type, string filepath, string filename)
        {
            IWorkspaceFactory pFactory = new RasterWorkspaceFactoryClass();
            IRasterWorkspace pRasterSpace = (IRasterWorkspace)pFactory.OpenFromFile(filepath, 0);
            IRasterDataset pRasterDataset = pRasterSpace.OpenRasterDataset(filename);

            //IRasterLayer pRasterLayer = new RasterLayerClass();
            IRasterDataset2 pRasterDataset2 = (IRasterDataset2)pRasterDataset;
            IRaster pRaster = (IRaster)pRasterDataset2.CreateFullRaster();
            IRaster2 pRaster2 = (IRaster2)pRaster;
            IRasterEdit pRasterEdit = (IRasterEdit)pRaster2;

            IRasterLayer pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromDataset(pRasterDataset);
            
            IRasterProps pRasProps = pRasterLayer.Raster as IRasterProps;
            
            //IRasterBandCollection pRasterBandCollection = (IRasterBandCollection)pRasterDataset;
            //IRawPixels rawPixels = (IRawPixels)pRasterBandCollection.Item(0);
            //IRawBlocks rawBlocks = (IRawBlocks)pRasterBandCollection.Item(0);
            //IPnt pBlockSize = new PntClass();
            //IEnvelope pEnvelope = OutExtent;
            //pBlockSize.SetCoords(pEnvelope.Width, pEnvelope.Height);
            IPnt pBlockSize = new PntClass();
            pBlockSize.SetCoords(rpColumns, rpRows);

            IPixelBlock pixelBlock = pRaster2.CreateCursorEx(pBlockSize).PixelBlock;
            int w = pixelBlock.Width;
            int h = pixelBlock.Height;
            //read the first pixel block
            IPnt topleftCorner = new PntClass();
            topleftCorner.SetCoords(0, 0);
            //pRaster.Read(topleftCorner, pixelBlock);
            //modify one pixel value at location (assume the raster has a pixel type of uchar)
            //IPixelBlock3 pixelBlock3 = (IPixelBlock3)pixelBlock;
            System.Array pixels = (System.Array)pixelBlock.get_SafeArray(0);

            object o1;
            float f1;

            #region double[,] to float[,]
            switch (pixel_type)
            {
                case rstPixelType.PT_FLOAT:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            f1 = Convert.ToSingle(InputData[j,i]);
                            o1 = (object)f1;
                            pixels.SetValue(o1, i, j);
                            //pixels.SetValue(Convert.ToSingle(InputData.GetValue(j, i)), i, j); //double[,] 数组与pixelblock的内部格式行列颠倒！！
                        }
                    }
                    break;
                case rstPixelType.PT_UCHAR:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            o1 = InputData.GetValue(j, i);
                            f1 = Convert.ToSingle(o1);
                            int t = Convert.ToInt32(f1);
                            if (t < Byte.MinValue)
                                pixels.SetValue(Byte.MinValue, i, j);
                            else if (t > Byte.MaxValue)
                                pixels.SetValue(Byte.MaxValue, i, j);
                            else
                                pixels.SetValue(Convert.ToByte(t), i, j);
                        }
                    }
                    break;
                case rstPixelType.PT_SHORT:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            o1 = InputData.GetValue(j, i);
                            f1 = Convert.ToSingle(o1);

                            if (f1 < -100)
                            {
                                pixels.SetValue(-100, i, j);
                            }
                            else
                            {
                                short s = Convert.ToInt16(f1);
                                pixels.SetValue(s, i, j);
                            }
                        }
                    }
                    break;
                case rstPixelType.PT_LONG:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            o1 = InputData.GetValue(j, i);
                            f1 = Convert.ToSingle(o1);
                            int t = Convert.ToInt32(f1);
                            if (t < -100)
                            {
                                pixels.SetValue(-100, i, j);
                            }
                            else
                            {
                                pixels.SetValue(t, i, j);
                            }
                            
                        }
                    }
                    break;
            //switch (pRasProps.PixelType)
            //{
            //    case rstPixelType.PT_FLOAT:
            //        for (int i = 0; i < rpColumns; i++)
            //        {
            //            for (int j = 0; j < rpRows; j++)
            //            {
            //                o1 = InputData.GetValue(j, i);
            //                f1 = Convert.ToSingle(o1);
            //                o1 = (object)f1;
            //                pixels.SetValue(o1, i, j); //double[,] 数组与pixelblock的内部格式行列颠倒！！
            //            }
            //        }
            //        break;
            //    case rstPixelType.PT_UCHAR:
            //        for (int i = 0; i < rpColumns; i++)
            //        {
            //            for (int j = 0; j < rpRows; j++)
            //            {
            //                double t = Convert.ToDouble(InputData.GetValue(j, i));
            //                if (t == float.MinValue)
            //                    pixels.SetValue(256, i, j);
            //                else
            //                    pixels.SetValue(Convert.ToByte(t), i, j);
            //            }
            //        }
            //        break;
            //    case rstPixelType.PT_LONG:
            //        Int32[] nodata = (Int32[])pRasProps.NoDataValue;
            //        for (int i = 0; i < rpColumns; i++)
            //        {
            //            for (int j = 0; j < rpRows; j++)
            //            {
            //                double t = Convert.ToDouble(InputData.GetValue(j, i));
            //                if(t == float.MinValue)
            //                    pixels.SetValue(nodata[0], i, j);
            //                else
            //                    pixels.SetValue(Convert.ToInt32(t), i, j);
            //            }
            //        }
            //        break;
            }
            #endregion
            pixelBlock.set_SafeArray(0, (object)pixels);


            //write the modified pixel block to the raster dataset
            pRasterEdit.Write(topleftCorner, pixelBlock);

            //funColorForRaster_Classify(pRasterLayer, 255, 255, 255, 0, 0, 0);
            pRasterEdit.Refresh();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pFactory);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRasterSpace);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRasterDataset);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRasterDataset2);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRasProps);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRasterLayer);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRaster);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRaster2);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pRasterEdit);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pixelBlock);
            //pFactory = null;
            //pRasterSpace = null;
            //pRasterDataset = null;
            //pRasterDataset2 = null;
            //pRaster = null;
            //pRaster2 = null;
            //pRasterEdit = null;
            //pBlockSize = null;
            //pixelBlock = null;
            pixels = null;
            GC.Collect();
        }

        public void WriteArray2RasterFile(ref int[,] InputData, rstPixelType pixel_type, string filepath, string filename)
        {
            IWorkspaceFactory pFactory = new RasterWorkspaceFactoryClass(); ;
            IRasterWorkspace pRasterSpace = (IRasterWorkspace)pFactory.OpenFromFile(filepath, 0); ;
            IRasterDataset pRasterDataset = pRasterSpace.OpenRasterDataset(filename);

            //IRasterLayer pRasterLayer = new RasterLayerClass();
            IRasterDataset2 pRasterDataset2 = (IRasterDataset2)pRasterDataset;
            IRaster pRaster = (IRaster)pRasterDataset2.CreateFullRaster();
            IRaster2 pRaster2 = (IRaster2)pRaster;
            IRasterEdit pRasterEdit = (IRasterEdit)pRaster2;

            //IRasterBandCollection pRasterBandCollection = (IRasterBandCollection)pRasterDataset;
            //IRawPixels rawPixels = (IRawPixels)pRasterBandCollection.Item(0);
            //IRawBlocks rawBlocks = (IRawBlocks)pRasterBandCollection.Item(0);
            //IPnt pBlockSize = new PntClass();
            //IEnvelope pEnvelope = OutExtent;
            //pBlockSize.SetCoords(pEnvelope.Width, pEnvelope.Height);
            IPnt pBlockSize = new PntClass();
            pBlockSize.SetCoords(rpColumns, rpRows);

            IPixelBlock pixelBlock = pRaster2.CreateCursorEx(pBlockSize).PixelBlock;
            int w = pixelBlock.Width;
            int h = pixelBlock.Height;
            //read the first pixel block
            IPnt topleftCorner = new PntClass();
            topleftCorner.SetCoords(0, 0);
            //pRaster.Read(topleftCorner, pixelBlock);
            //modify one pixel value at location (assume the raster has a pixel type of uchar)
            //IPixelBlock3 pixelBlock3 = (IPixelBlock3)pixelBlock;
            System.Array pixels = (System.Array)pixelBlock.get_SafeArray(0);

            #region double[,] to float[,]
            switch (pixel_type)
            {
                case rstPixelType.PT_FLOAT:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            pixels.SetValue(Convert.ToSingle(InputData.GetValue(j, i)), i, j); //double[,] 数组与pixelblock的内部格式行列颠倒！！
                        }
                    }
                    break;
                case rstPixelType.PT_UCHAR:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            int t = Convert.ToInt32(InputData.GetValue(j, i));
                            if (t == -100)
                                pixels.SetValue(255, i, j);
                            else
                                pixels.SetValue(Convert.ToByte(t), i, j);
                        }
                    }
                    break;
                case rstPixelType.PT_SHORT:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            short t = Convert.ToInt16(InputData.GetValue(j, i));
                            if (t == -100)
                                pixels.SetValue(Int16.MinValue, i, j);
                            else
                                pixels.SetValue(t, i, j);
                        }
                    }
                    break;
                case rstPixelType.PT_LONG:
                    for (int i = 0; i < rpColumns; i++)
                    {
                        for (int j = 0; j < rpRows; j++)
                        {
                            pixels.SetValue(Convert.ToInt32(InputData.GetValue(j, i)), i, j);
                        }
                    }
                    break;
            }
            #endregion
            pixelBlock.set_SafeArray(0, (object)pixels);

            //write the modified pixel block to the raster dataset
            pRasterEdit.Write(topleftCorner, pixelBlock);
            pRasterEdit.Refresh();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pRasterEdit);

            pFactory = null;
            pRasterSpace = null;
            pRasterDataset = null;
            pRasterDataset2 = null;
            pRaster = null;
            pRaster2 = null;
            pRasterEdit = null;
            pBlockSize = null;
            pixelBlock = null;
            pixels = null;
            GC.Collect();
        }
        #endregion

        //地图图片输出

        private void outPicture_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPEG(*.jpg)|*.jpg|BMP(*.BMP)|*.bmp|EMF(*.emf)|*.emf|GIF(*.gif)|*.gif|AI(*.ai)|*.ai|PDF(*.pdf)|*.pdf|PNG(*.png)|*.png|EPS(*.eps)|*.eps|SVG(*.svg)|*.svg|TIFF(*.tif)|*.tif";
            saveFileDialog1.Title = "输出地图";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.ShowDialog();
            IActiveView pActiveView = axMapControl1.ActiveView;
            bool flag = ExportMapToImage(pActiveView, saveFileDialog1.FileName, saveFileDialog1.FilterIndex);

            saveFileDialog1 = null;
        }

        //private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        //{
        //    string fileName = saveFileDialog1.FileName;
        //    int filterIndex = saveFileDialog1.FilterIndex;
        //     axPageLayoutControl1.ActiveView;
        //    //ExportPic exportPic = new ExportPic();
        //    //bool flag = exportPic.ExportMapToImage(pActiveView,fileName,filterIndex);
        //    bool flag = ExportMapToImage(pActiveView, fileName, filterIndex);
        //    saveFileDialog1.Dispose();
        //    if (flag)
        //    {
        //        MessageBox.Show("图片输出成功！", "成功");
        //    }
        //    else
        //    {
        //        MessageBox.Show("图片输出失败，请重新生成！", "失败");
        //    }
        //}

        public bool ExportMapToImage(IActiveView pActiveView, string fileName, int filterIndex)
        {
            try
            {
                IExport pExporter = null;
                switch (filterIndex)
                {
                    case 1:
                        pExporter = new ExportJPEGClass();
                        break;
                    case 2:
                        pExporter = new ExportBMPClass();
                        break;
                    case 3:
                        pExporter = new ExportEMFClass();
                        break;
                    case 4:
                        pExporter = new ExportGIFClass();
                        break;
                    case 5:
                        pExporter = new ExportAIClass();
                        break;
                    case 6:
                        pExporter = new ExportPDFClass();
                        break;
                    case 7:
                        pExporter = new ExportPNGClass();
                        break;
                    case 8:
                        pExporter = new ExportPSClass();
                        break;
                    case 9:
                        pExporter = new ExportSVGClass();
                        break;
                    case 10:
                        pExporter = new ExportTIFFClass();
                        break;
                    default:
                        MessageBox.Show("输出格式错误");
                        return false;
                }
                IEnvelope pEnvelope = new EnvelopeClass();
                ITrackCancel pTrackCancel = new CancelTrackerClass();
                tagRECT ptagRECT;
                ptagRECT.left = 0;
                ptagRECT.top = 0;
                ptagRECT.right = (int)(pActiveView.Extent.Width / 0.5);
                ptagRECT.bottom = (int)(pActiveView.Extent.Height / 0.5);
                int pResolution = (int)(pActiveView.ScreenDisplay.DisplayTransformation.Resolution);
                pEnvelope.PutCoords(ptagRECT.left, ptagRECT.bottom, ptagRECT.right, ptagRECT.top);
                pExporter.Resolution = (int)(pResolution / 0.5);
                pExporter.ExportFileName = fileName;
                pExporter.PixelBounds = pEnvelope;
                pActiveView.Output(pExporter.StartExporting(), pResolution, ref ptagRECT, pActiveView.Extent, pTrackCancel);
                pExporter.FinishExporting();
                //释放资源
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pExporter);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "输出图片", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }
        #region 地图输出为图片

        /// <summary>
        /// 打印输出.该功能目前测试只适用于JPEG,BMP.格式
        /// </summary>
        /// <param name="pExport"></param>
        /// <param name="dResolution">分辨率</param>
        /// <param name="outputPath">输出路径</param>
        /// <param name="pVisibleBounds">自定义可见区域</param>
        //private void FunExportImage(IExport pExport, double dResolution, string outputPath, IEnvelope pVisibleBounds)
        //{
        //    IActiveView m_ActiveView = axMapControl1.ActiveView;
        //    IEnvelope pPixelBounds;
        //    tagRECT outtagRECT;
        //    tagRECT DisplayBounds = m_ActiveView.ExportFrame;
        //    double iScreenResolution;
        //    if (pExport == null)
        //    {
        //        MessageBox.Show("打印类型未指定");
        //        return;
        //    }

        //    if (pVisibleBounds != null) //自定义框范围
        //    {
        //        IDisplayTransformation pDisplayTransformation = m_ActiveView.ScreenDisplay.DisplayTransformation;
        //        pDisplayTransformation.TransformRect(pVisibleBounds, ref DisplayBounds, 8);//8代表esriDisplayTransformEnum.esriTransformToDevice
        //    }

        //    if (pExport is IExportImage)
        //    {
        //        IExportImage pExportImage = pExport as IExportImage;
        //        pExportImage.ImageType = esriExportImageType.esriExportImageTypeTrueColor;
        //    }

        //    iScreenResolution = m_ActiveView.ScreenDisplay.DisplayTransformation.Resolution;

        //    double l_resolution = Convert.ToDouble(domainResolution.Value);

        //    //=====================================================================
        //    //
        //    //方法一
        //    //
        //    //一厘米包含37.79524个像素
        //    //double dPixel = 37.79524;
        //    ////按照设置输入分辨率计算一厘米所含的像素数
        //    //double tempratio = l_resolution * dPixel / iScreenResolution;
        //    ////输出图形的高度
        //    //double tempbottom = (DisplayBounds.bottom - DisplayBounds.top) * tempratio;
        //    ////输出图形的宽度
        //    //double tempright = (DisplayBounds.right - DisplayBounds.left) * tempratio + 0.1 * tempratio;
        //    //======================================================================
        //    //
        //    //方法二:暂时可行.
        //    //
        //    double tempratio = l_resolution / iScreenResolution;
        //    double tempbottom = (DisplayBounds.bottom - DisplayBounds.top) * tempratio;
        //    double tempright = (DisplayBounds.right - DisplayBounds.left) * tempratio;
        //    //=====================================================================
        //    outtagRECT.left = 0;
        //    outtagRECT.top = 0;
        //    outtagRECT.bottom = Convert.ToInt32(Math.Truncate(tempbottom));
        //    outtagRECT.right = Convert.ToInt32(Math.Truncate(tempright));

        //    pPixelBounds = new EnvelopeClass();
        //    pPixelBounds.PutCoords(outtagRECT.left, outtagRECT.top, outtagRECT.right, outtagRECT.bottom);

        //    pExport.Resolution = dResolution;
        //    pExport.PixelBounds = pPixelBounds;
        //    pExport.ExportFileName = outputPath;

        //    try
        //    {
        //        ITrackCancel pTrackCancel = new TrackCancelClass();
        //        int hDC;
        //        hDC = pExport.StartExporting();

        //        m_ActiveView.Output(hDC, (int)dResolution, ref outtagRECT, pVisibleBounds, pTrackCancel);
        //        pExport.FinishExporting();


        //        if (DialogResult.Yes == MessageBox.Show("出图成功! \n图片保存在" + txtOutPutPath.Text + "\n是否需要打开文件所在的目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
        //        {
        //            System.Diagnostics.Process.Start("explorer.exe", txtOutPutPath.Text);
        //        }
        //        pExport.Cleanup();
        //    }
        //    catch
        //    {
        //        MessageBox.Show("出图失败!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        //释放变量

        //        pExport = null;
        //        pVisibleBounds = null;
        //    }
        //    //释放变量
        //    pExport = null;
        //    pVisibleBounds = null;

        //}
        #endregion

        /// <summary>
        /// 打开栅格数据
        /// </summary>
        /// <param name="filepath">文件目录</param>
        /// <param name="filename">文件名带后缀</param>
        /// <returns>IRasterLayer接口</returns>
        #region 打开栅格数据
        public IRasterLayer OpenRasterFile(string filepath, string filename,bool add_to_layer = true)
        {
            IWorkspaceFactory pFactory;
            IRasterWorkspace pRasterSpace;

            pFactory = new RasterWorkspaceFactoryClass();
            pRasterSpace = (IRasterWorkspace)pFactory.OpenFromFile(filepath, 0);

            IRasterDataset pRasterDataset = pRasterSpace.OpenRasterDataset(filename);
            IRasterLayer pRasterLayer = new RasterLayerClass();
            pRasterLayer.CreateFromDataset(pRasterDataset);
            m_default_reference = axMapControl1.SpatialReference;

            if (add_to_layer)
            {
                axMapControl1.AddLayer(pRasterLayer);
                axMapControl1.ActiveView.Refresh();
            }
            IRasterProps pProps = pRasterLayer.Raster as IRasterProps;
            try
            {
                LayerPixelType.Add(filename, pProps.PixelType);
                LayerPath.Add(filename, filepath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + "\n");
                LayerPixelType[filename] = pProps.PixelType;
                LayerPath[filename] = filepath;
            }
            
            
            //funColorForRaster_Classify(pRasterLayer);

            pFactory = null;
            pRasterSpace = null;
            pRasterDataset = null;
            GC.Collect();

            //funColorForRaster_Classify(pRasterLayer);

            return pRasterLayer;
        }

        #endregion

        #region 删除文件夹
        /// <summary> 
        /// 删除文件夹(针对栅格数据的删除所写) 
        /// </summary> 
        /// <param name="dir">文件夹全路径</param>
        
        public void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir)) //如果存在这个文件夹删除之 
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d); //直接删除其中的文件 
                    else
                        DeleteFolder(d); //递归删除子文件夹 
                }
                if (File.Exists(dir + ".ovr"))
                {
                    File.Delete(dir + ".ovr");
                }
                Directory.Delete(dir); //删除已空文件夹 
                Console.WriteLine(dir + " 文件夹删除成功.\n");
            }
            //else
            //Console.Write(dir + " 该文件夹不存在"); //如果文件夹不存在则提示 
        }
        #endregion

        #region 通过名字获取图层的IRaster
        /// <summary> 
        /// 通过名字获取图层的IRaster
        /// </summary> 
        /// <param name="filename">已打开的图层名</param>
        
        public ILayer GetLayerByName(string filename)
        {
            ILayer pLayer = null;
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                pLayer = axMapControl1.get_Layer(i);
                if (filename == pLayer.Name)
                {
                    break;
                }
            }
            return pLayer;
        }
        #endregion

        #region 获取非空节点链表的第一个节点元素
        /// <summary> 
        /// 获取非空节点链表的第一个节点元素
        /// </summary> 
        /// <param name="nodelist">节点链表nodeque</param>
        
        private MyCostFunction.LinkNode GetListFirstElement(ref List<MyCostFunction.LinkNode> nodelist)
        {
            MyCostFunction.LinkNode ret_node = new MyCostFunction.LinkNode();
            ret_node.accumulate = 0;
            ret_node.cColumn = 0;
            ret_node.cRow = 0;
            foreach (MyCostFunction.LinkNode node in nodelist)
            {
                if (node.accumulate > 0)
                {
                    return node;
                }
            }
            return ret_node;
        }
        #endregion

        #region 像元值读取到数组
        /// <summary>
        /// 像元值读取到数组
        /// </summary>
        /// <param name="pRasLyr">包含像元值的栅格图层</param>
        /// <param name="OutputData">Array类型的数组</param>
        public void ReadPixelValues2Array(IRasterLayer pRasLyr, out System.Array OutputData)
        {
            IRasterProps pRasProps = (IRasterProps)pRasLyr.Raster;

            //object oo = pRasProps.NoDataValue;
            //Single[] s = oo as Single[];
            //System.Type t = oo.GetType();
            //string aaaa =t.Name;

            rpRows = pRasProps.Height;
            rpColumns = pRasProps.Width;
            double cSizeX = pRasProps.MeanCellSize().X;
            double cSizeY = pRasProps.MeanCellSize().Y;

            IPixelBlock pixelBlock = pRasLyr.Raster.CreatePixelBlock(new PntClass() { X = rpColumns, Y = rpRows });
            pRasLyr.Raster.Read(new PntClass() { X = 0, Y = 0 }, pixelBlock);
            OutputData = (System.Array)pixelBlock.get_SafeArray(0);

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pixelBlock);
            pixelBlock = null;
            GC.Collect();
        }

        public void RasterConversion(rstPixelType src_pixel_type, rstPixelType dst_pixel_type)
        {
            Type aa = typeof(string);
            aa.GetType();
        }
        
        public double[,] ReadPixelValues2DoubleArray(IRasterLayer pRasLyr)
        {
            
            IRasterProps pRasProps = (IRasterProps)pRasLyr.Raster;

            object oo = pRasProps.NoDataValue;
            

            rpRows = pRasProps.Height;
            rpColumns = pRasProps.Width;
            double cSizeX = pRasProps.MeanCellSize().X;
            double cSizeY = pRasProps.MeanCellSize().Y;

            IPixelBlock pixelBlock = pRasLyr.Raster.CreatePixelBlock(new PntClass() { X = rpColumns, Y = rpRows });
            pRasLyr.Raster.Read(new PntClass() { X = 0, Y = 0 }, pixelBlock);
            System.Array ArrayData = (System.Array)pixelBlock.get_SafeArray(0);
            double[,] pixels = new double[rpRows, rpColumns];

            object o1;
            switch (pRasProps.PixelType)
            {
                case rstPixelType.PT_FLOAT:
                    Single[] nodata_s = oo as Single[];
                    Single NData_s = nodata_s[0];
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            o1 = ArrayData.GetValue(j, i);
                            pixels[i, j] = Convert.ToSingle(o1);
                        }
                    }
                    break;
                case rstPixelType.PT_LONG:
                    Int32[] nodata_i32 = oo as Int32[];
                    Int32 NData_i32 = nodata_i32[0];
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            o1 = ArrayData.GetValue(j, i);
                            Int32 t = Convert.ToInt32(o1);
                            if (t == NData_i32)
                                pixels[i, j] = Convert.ToSingle(float.MinValue);
                            else
                                pixels[i, j] = Convert.ToSingle(o1);
                        }
                    }
                    break;
                case rstPixelType.PT_SHORT:
                    Int16[] nodata_i16 = oo as Int16[];
                    Int16 NData_i16 = nodata_i16[0];
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            o1 = ArrayData.GetValue(j, i);
                            Int16 s = Convert.ToInt16(o1);
                            if (s == NData_i16)
                            {
                                pixels[i, j] = Convert.ToSingle(float.MinValue);
                            }
                            else
                            {
                                pixels[i, j] = Convert.ToSingle(o1);
                            }
                        }
                    }
                    break;
                case rstPixelType.PT_USHORT:
                    UInt16[] nodata_ui16 = oo as UInt16[];
                    UInt16 NData_ui16 = nodata_ui16[0];
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            o1 = ArrayData.GetValue(j, i);
                            ushort s = Convert.ToUInt16(o1);
                            if (s == NData_ui16)
                            {
                                pixels[i, j] = Convert.ToSingle(float.MinValue);
                            }
                            else
                            {
                                pixels[i, j] = Convert.ToSingle(o1);
                            }
                        }
                    }
                    break;

                case rstPixelType.PT_CHAR:
                    SByte[] nodata_ui8 = oo as SByte[];
                    SByte NData_ui8 = nodata_ui8[0];
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            o1 = ArrayData.GetValue(j, i);
                            SByte t = Convert.ToSByte(o1);
                            if (t == NData_ui8)
                                pixels[i, j] = Convert.ToSingle(float.MinValue);
                            else
                                pixels[i, j] = Convert.ToSingle(o1);
                        }
                    }
                    break;
                case rstPixelType.PT_UCHAR:
                    Byte[] nodata_i8 = oo as Byte[];
                    Byte NData_i8 = nodata_i8[0];
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            o1 = ArrayData.GetValue(j, i);
                            Byte t = Convert.ToByte(o1);
                            if (t == NData_i8)
                                pixels[i, j] = Convert.ToSingle(float.MinValue);
                            else
                                pixels[i, j] = Convert.ToSingle(o1);
                        }
                    }
                    break;
            }
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(pixelBlock);
            pixelBlock = null;
            ArrayData = null;
            GC.Collect();

            return pixels;
        }

        #endregion

        #region 将数据追加写入已打开的TXT文件中
        /// <summary> 
        /// 将数据追加写入已打开的TXT文件中
        ///</summary> 
        /// <param name="sw">已打开的输出文件流</param>
        /// <param name="content">输出内容</paparts
        /// <param napolumns">content的列数</param>
        /// <param name="rows">content的行数</param>
        private void WriteData2TXTFile(StreamWriter sw, ref double[,] content, int rows, int columns)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {

                    if (content[i, j] == float.MinValue)
                    {
                        sw.Write("-1, ");
                        continue;
                    }
                    sw.Write("{0}, ", content[i, j]);
                }
                sw.Write("\n");
            }
        }
        #endregion

        #region System.Array 转化为double[,]类型数组
        /// <summary>
        /// System.Array 转化为double[,]类型数组
        /// </summary>
        /// <param name="input">输入的Array数组</param>
        /// <param name="output">输出的double[,]数组</param>
        public void SystemArray2DoubleArray(System.Array input, ref double[,] output)
        {
            if (output == null)
            {
                output = (double[,])System.Array.CreateInstance(typeof(double), rpRows, rpColumns);
            }
            for (int i = 0; i < rpRows; i++)
            {
                for (int j = 0; j < rpColumns; j++)
                {
                    output[i, j] = Convert.ToDouble(input.GetValue(j, i));
                }
            }
        }
        #endregion

        #region  double[,]类型数组转化为System.Array
        private void DoubleArray2SystemArray(ref double[,] input, out System.Array output, int rows, int columns)
        {
            //output = (double[,])input.Clone();
            output = System.Array.CreateInstance(typeof(double), columns, rows);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    output.SetValue((object)input[i, j], j, i);
        }
        #endregion

        /// <summary>
        /// 依据已有栅格创建大小、坐标系等完全一样的空白栅格数据
        /// </summary>
        /// <param name="Path">文件目录</param>
        /// <param name="FileName">文件名带后缀</param>
        /// <param name="raster1">参考栅格数据</param>
        /// <param name="value">初始化值</param>
        /// <returns>栅格数据集</returns>
        public IRasterDataset CreateRasterDataset_2(string Path, string FileName, rstPixelType pixel_type, IRaster raster1,ref double[,] value)
        {
            try
            {
                IRasterProps rasterProps1 = (IRasterProps)raster1;

                IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();
                IRasterWorkspace2 rasterWs = workspaceFactory.OpenFromFile(Path, 0) as IRasterWorkspace2;
                //Define the spatial reference of the raster dataset.
                ISpatialReference sr = new UnknownCoordinateSystemClass();
                //Define the origin for the raster dataset, which is the lower left corner of the raster.
                IPoint origin = new PointClass();
                origin.PutCoords(rasterProps1.Extent.XMin, rasterProps1.Extent.YMin);
                //Define the dimensions of the raster dataset.
                int width = rasterProps1.Width; //This is the width of the raster dataset.
                int height = rasterProps1.Height; //This is the height of the raster dataset.
                double xCell = (rasterProps1.Extent.XMax - rasterProps1.Extent.XMin) / width; //This is the cell size in x direction.
                double yCell = (rasterProps1.Extent.YMax - rasterProps1.Extent.YMin) / height; //This is the cell size in y direction.
                int NumBand = 1; // This is the number of bands the raster dataset contains.
                //Create a raster dataset in TIFF format.
                //////////////////////////////////////////////////DeleteFolder(Path + FileName);
                //origin is the upper left corner of the dataset!!!
                IRasterDataset rasterDataset = rasterWs.CreateRasterDataset(FileName, "GRID", origin, width, height, xCell, yCell, NumBand, pixel_type, m_default_reference, true);
                //If you need to set NoData for some of the pixels, you need to set it on band //to get the raster band.
                IRasterBandCollection rasterBands = (IRasterBandCollection)rasterDataset;
                IRasterBand rasterBand;
                IRasterProps rasterProps;
                rasterBand = rasterBands.Item(0);
                rasterProps = (IRasterProps)rasterBand;
                //Set NoData if necessary. For a multiband image, a NoData value needs to be set for each band.
                //rasterProps.NoDataValue = float.MinValue;
                //Create a raster from the dataset.
                IRasterDataset2 rasterDataset2 = rasterDataset as IRasterDataset2;
                IRaster raster = rasterDataset2.CreateFullRaster();
                //Create a pixel block using the weight and height of the raster dataset.
                //If the raster dataset is large, a smaller pixel block should be used. 
                //Refer to the topic "How to access pixel data using a raster cursor".
                IPnt blocksize = new PntClass();
                blocksize.SetCoords(width, height);
                IPixelBlock3 pixelBlock3 = raster.CreatePixelBlock(blocksize) as IPixelBlock3;
                //IPixelBlock pixelBlock = raster.CreatePixelBlock(blocksize) as IPixelBlock;
                //Populate some pixel values to the pixel block.

                System.Array pixelData = (System.Array)pixelBlock3.get_PixelData(0);
                //System.Array pixelData = (System.Array)pixelBlock.get_SafeArray(0);
                //Loop through all the pixels and assign value

                switch (pixel_type)
                {
                    case rstPixelType.PT_FLOAT:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToSingle(value[j, i]), i, j);
                        break;
                    case rstPixelType.PT_UCHAR:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToByte(value[j, i]), i, j);
                        break;
                    case rstPixelType.PT_SHORT:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToInt16(value[j, i]), i, j);
                        break;
                    default:
                        break;
                }


                // Write the pixeldata back
                pixelBlock3.set_PixelData(0, pixelData);
                //pixelBlock.set_SafeArray(0, pixelData);
                //Define the location that the upper left corner of the pixel block is to write.
                IPnt upperLeft = new PntClass();
                upperLeft.SetCoords(0, 0);
                //Write the pixel block.
                IRasterEdit rasterEdit = (IRasterEdit)raster;
                //rasterProps.Extent = rasterProps1.Extent;
                rasterEdit.Write(upperLeft, (IPixelBlock)pixelBlock3);
                //rasterEdit.Write(upperLeft, pixelBlock);
                //Release rasterEdit explicitly.
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rasterEdit);

                return rasterDataset;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.Message);
                
                return null;
            }
        }

        public IRasterDataset CreateRasterDataset_2(string Path, string FileName, rstPixelType pixel_type, IRaster raster1, int[,] value)
        {
            try
            {
                IRasterProps rasterProps1 = (IRasterProps)raster1;

                IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();
                IRasterWorkspace2 rasterWs = workspaceFactory.OpenFromFile(Path, 0) as IRasterWorkspace2;

                //Define the spatial reference of the raster dataset.
                ISpatialReference sr = new UnknownCoordinateSystemClass();

                //Define the origin for the raster dataset, which is the lower left corner of the raster.
                IPoint origin = new PointClass();
                origin.PutCoords(rasterProps1.Extent.XMin, rasterProps1.Extent.YMin);

                //Define the dimensions of the raster dataset.
                int width = rasterProps1.Width; //This is the width of the raster dataset.
                int height = rasterProps1.Height; //This is the height of the raster dataset.
                double xCell = (rasterProps1.Extent.XMax - rasterProps1.Extent.XMin) / width; //This is the cell size in x direction.
                double yCell = (rasterProps1.Extent.YMax - rasterProps1.Extent.YMin) / height; //This is the cell size in y direction.
                int NumBand = 1; // This is the number of bands the raster dataset contains.

                //Create a raster dataset in TIFF format.
                DeleteFolder(Path + FileName);

                //origin is the upper left corner of the dataset!!!
                IRasterDataset rasterDataset = rasterWs.CreateRasterDataset(FileName, "GRID", origin, width, height, xCell, yCell, NumBand, pixel_type, m_default_reference, true);

                //If you need to set NoData for some of the pixels, you need to set it on band //to get the raster band.
                IRasterBandCollection rasterBands = (IRasterBandCollection)rasterDataset;
                IRasterBand rasterBand;
                IRasterProps rasterProps;
                rasterBand = rasterBands.Item(0);
                rasterProps = (IRasterProps)rasterBand;

                //Set NoData if necessary. For a multi-band image, a NoData value needs to be set for each band.
                //rasterProps.NoDataValue = float.MinValue;
                //Create a raster from the dataset.
                IRasterDataset2 rasterDataset2 = rasterDataset as IRasterDataset2;
                IRaster raster = rasterDataset2.CreateFullRaster();

                //Create a pixel block using the weight and height of the raster dataset.
                //If the raster dataset is large, a smaller pixel block should be used. 
                //Refer to the topic "How to access pixel data using a raster cursor".
                IPnt blocksize = new PntClass();
                blocksize.SetCoords(width, height);
                //IPixelBlock3 pixelBlock3 = raster.CreatePixelBlock(blocksize) as IPixelBlock3;
                IPixelBlock pixelBlock = raster.CreatePixelBlock(blocksize) as IPixelBlock;
                //Populate some pixel values to the pixel block.

                //System.Array pixelData = (System.Array)pixelBlock3.get_PixelData(0);
                System.Array pixelData = (System.Array)pixelBlock.get_SafeArray(0);
                //Loop through all the pixels and assign value

                switch (pixel_type)
                {
                    case rstPixelType.PT_FLOAT:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToSingle(value[j, i]), i, j);
                        break;
                    case rstPixelType.PT_UCHAR:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToByte(value[j, i]), i, j);
                        break;
                    case rstPixelType.PT_SHORT:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToInt16(value[j, i]), i, j);
                        break;
                    case rstPixelType.PT_LONG:
                        for (int i = 0; i < rasterProps.Width; i++)
                            for (int j = 0; j < rasterProps.Height; j++)
                                pixelData.SetValue(Convert.ToInt32(value[j, i]), i, j);
                        break;
                    default:
                        break;
                }


                // Write the pixeldata back
                //pixelBlock3.set_PixelData(0, pixelData);
                pixelBlock.set_SafeArray(0, pixelData);
                //Define the location that the upper left corner of the pixel block is to write.
                IPnt upperLeft = new PntClass();
                upperLeft.SetCoords(0, 0);
                //Write the pixel block.
                IRasterEdit rasterEdit = (IRasterEdit)raster;

                //rasterProps.Extent = rasterProps1.Extent;
                //rasterEdit.Write(upperLeft, (IPixelBlock)pixelBlock3);
                rasterEdit.Write(upperLeft, pixelBlock);
                //Release rasterEdit explicitly.
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rasterEdit);

                return rasterDataset;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 将点文件内的坐标读取到IPoint接口中
        /// </summary>
        /// <param name="filepath">路径</param>
        /// <param name="filename">文件名</param>
        /// <returns>IPoint接口数据</returns>
        private IPoint ConvertPointFeatureFile2XYCoords(string filepath, string filename)
        {
            IFeatureLayer pFeatureLayer = OpenFeatureFile(filepath, filename);
            IFeatureCursor pFeatureCursor = pFeatureLayer.Search(null, false);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IPoint pt = pFeature.Shape as IPoint;

            double x1 = pt.X;
            double y1 = pt.Y;
            double width = OutExtent.Width;
            double height = OutExtent.Height;
            double ul_x = OutExtent.UpperLeft.X;
            double ul_y = OutExtent.UpperLeft.Y;
            //MessageBox.Show(width.ToString() + ", " + height.ToString());
            //MessageBox.Show(ul_x.ToString() + ", " + ul_y.ToString());
            double cell_size_x = cell_size;
            double cell_size_y = cell_size;
            IPoint pt2 = new PointClass();
            pt2.X = (x1 - ul_x) / cell_size_x;
            pt2.Y = (ul_y - y1) / cell_size_y;
            return pt2;
        }
        public static bool IsLineIntersectWithPolygons(IFeatureLayer pBarriersLayer, int r1, int c1, int r2, int c2)
        {
            UpdatePolylineVertex(r1, c1, r2, c2);
            ITopologicalOperator pTopoOperator = pPLine as ITopologicalOperator;
            IFeatureCursor pFCursor = pBarriersLayer.Search(null, false);
            IFeature pF = null;
            IPolygon pPolygon = null;
            pF = pFCursor.NextFeature();
            while (pF != null)
            {
                pPolygon = pF.ShapeCopy as IPolygon;
                IGeometry pGeo = pTopoOperator.Intersect(pPolygon as IGeometry, esriGeometryDimension.esriGeometry1Dimension);
                if (!pGeo.IsEmpty)
                {
                    return true;
                }
                pF = pFCursor.NextFeature();
            }
            return false;
        }

        public void CreatePolylineFeature(int r1, int c1, int r2, int c2)
        {
            object o = Type.Missing;
            ISegmentCollection pSegColl = new PathClass();
            ILine pLine = new LineClass();

            IPoint pt1 = RowCol2MapXY(r1, c1, OutExtent.XMin, OutExtent.YMax);
            IPoint pt2 = RowCol2MapXY(r2, c2, OutExtent.XMin, OutExtent.YMax);
            pLine.PutCoords(pt1, pt2);

            pSegColl.AddSegment(pLine as ISegment, ref o, ref o);
            pPLine.AddGeometry(pSegColl as IGeometry, ref o, ref o);

        }
        public static double xmin, ymax;
        public static IGeometryCollection pPLine = new PolylineClass();
        public static bool UpdatePolylineVertex(int r1, int c1, int r2, int c2)
        {
            try
            {
                object o = Type.Missing;
                pPLine.RemoveGeometries(0, pPLine.GeometryCount);
                ISegmentCollection pSegColl = new PathClass();
                ILine pLine = new LineClass();

                IPoint pt1 = RowCol2MapXY(r1, c1, xmin, ymax);
                IPoint pt2 = RowCol2MapXY(r2, c2, xmin, ymax);
                pLine.PutCoords(pt1, pt2);

                pSegColl.AddSegment(pLine as ISegment, ref o, ref o);
                pPLine.AddGeometry(pSegColl as IGeometry, ref o, ref o);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error! " + e.Message);
                return false;
            }
            return true;
        }
        public void SplitLineWithPolygons(IPolyline pPLine, IFeatureLayer pFLayer)
        {

            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureClass pFeatureClass;

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile("E:\\TEST\\", 0);
            pFeatureClass = pFeatureWorkspace.OpenFeatureClass("path_3.shp");
            IFeatureClass pFCChecker = pFeatureClass;
            if (pFCChecker != null)
            {
                IDataset pds = pFCChecker as IDataset;
                pds.Delete();
            }

            CreateShapeFile("E:\\TEST\\", "path_3.shp", esriGeometryType.esriGeometryPolyline);
            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass("path_3.shp");
            pFeatureClass = pFeatureLayer.FeatureClass;
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            ITopologicalOperator pTopoOperator = pPLine as ITopologicalOperator;
            IFeatureCursor pFCursor = pFLayer.Search(null, false);
            IFeature pF = null;
            IPolygon pPolygon = null;
            pF = pFCursor.NextFeature();
            while (pF != null)
            {
                pPolygon = pF.ShapeCopy as IPolygon;
                IGeometry pGeo = pTopoOperator.Intersect(pPolygon as IGeometry, esriGeometryDimension.esriGeometry1Dimension);
                if (!pGeo.IsEmpty)
                {
                    //IPolyline polyline = pGeo as IPolyline;
                    IPointCollection pPC = pGeo as IPointCollection;
                    IFeature pF2 = pFeatureClass.CreateFeature();
                    pF2.Shape = pPC as IGeometry;
                    pF2.Store();
                    //CreateFeatureLayerByPointCollections("E:\\TEST\\","path_2Copy.shp",pPC);
                }
                pF = pFCursor.NextFeature();
            }
        }

        /// <summary>
        /// 栅格数据叠加
        /// </summary>
        /// <param name="data1">要更改的数据data1</param>
        /// <param name="data2">用以更新或作为掩膜的数据data2</param>
        /// <param name="optype">更新或者NoData掩膜</param>
        /// <param name="weight1">data1的权重</param>
        /// <param name="weight2">data2的权重</param>
        private void RasterAddOperation(ref double[,] data1,ref double[,] data2, RasterOperationType optype, double weight1, double weight2)
        {
            for (int i = 0; i < rpRows; i++)
            {
                for (int j = 0; j < rpColumns; j++)
                {
                    switch (optype)
                    {
                        case RasterOperationType.NodataMask:
                            if (data2[i, j] != float.MinValue)
                                data1[i, j] = float.MinValue;
                            break;
                        case RasterOperationType.Updater:
                            if (data1[i, j] > 0 && data2[i, j] > 0)
                                data1[i, j] = data1[i, j] * weight1 + data2[i, j] * weight2;
                            break;
                    }

                }
            }
        }

        /// <summary>
        /// 从点集中创建矢量数据
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="pPC"></param>
        private void CreateFeatureLayerByPointCollections(string filepath, string filename, IPointCollection pPC)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureClass pFeatureClass;

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filepath, 0);

            pFeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            IFeature pF = pFeatureClass.Search(null, false).NextFeature();
            pF.Shape = pPC as IGeometry;
            pF.Store();
        }
        #region 最小耗费距离的主函数


        private void GarbageRecycling()
        {
            //垃圾回收
            open = null;
            close = null;
            parentX = null;
            parentY = null;
            impedance = null;
            terrain = null;
            
            cost = null;
            backlink = null;
            MyCostFunction.nodeque.Clear();
            GC.Collect();
        }
        /// <summary>
        /// 最小耗费函数（before 2015.11）
        /// </summary>
        /// <param name="save_filename"></param>
        private void LeastCostFunction(string save_filename = null)
        {
            DateTime t1 = DateTime.Now;
            GarbageRecycling();
            VariableInitialize();
            DateTime t2 = DateTime.Now;
            Console.WriteLine("Initialize TimeSpan:{0}", (t2 - t1).TotalMilliseconds / 1000);

            //double[,] result = (double[,])source.Clone();

            //RasterAddOperation(@"E:\TEST\", "grassland", RasterOperationType.NodataMask,ref open_area_2,rows,columns);
            //RasterAddOperation(@"E:\TEST\", "m_building", RasterOperationType.NodataMask, ref open_area_2, rows, columns);
            //RasterAddOperation(@"E:\TEST\", "ding_nodata", RasterOperationType.NodataMask, ref open_area_2, rows, columns);
            //RasterAddOperation(@"E:\TEST\", "road_medium", RasterOperationType.Updater, ref open_area_2, rows, columns);

            //System.Array output;
            //DoubleArray2SystemArray(ref open_area_2,out output,rows,columns);
            //WriteArray2RasterLayer(ref output, rstPixelType.PT_FLOAT, @"E:\TEST\", "open_areacopy");


            //for (int i = 0; i < rpRows; i++)
            //{
            //    for (int j = 0; j < rpColumns; j++)
            //    {
            //        source[i, j] = 0;
            //    }
            //}

            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            //从文件读取起点与终点
            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            //pBarriersLayer = OpenFeatureFile(@"E:\\TEST\\", "grass_land2.shp");

            //UpdatePolylineVertex(source_x, source_y, end_x, end_y);

            //int source_x = 201;
            //int source_y = 66;
            //int end_x = 172;
            //int end_y = 211;


            if (impedance[source_x, source_y] <= 0 || impedance[source_x, source_y] <= 0)
            {
                MessageBox.Show("两点之间不可到达！！请重新选点！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //source[source_x, source_y] = 1;
            //CalculateSourceNeibords(0, 2, ref source, ref data, ref cost,ref backlink);
            //CalculateSourceNeibords(0, 1, ref source, ref data, ref cost,ref backlink);
            //CalculateSourceNeibords(1, 2, ref source, ref data, ref cost,ref backlink);

            //MyCostFunction.CalculateSourceNeibords_2(source_x, source_y, ref source, ref open, ref close, ref open_area_2, ref cost, ref backlink);
            //MyCostFunction.CalculateSourceNeibords_4(source_x, source_y, end_x, end_y, ref open, ref close, ref open_area_2, ref cost, ref parentX, ref parentY);
            MyCostFunction.CalculateSourceNeibords_6(source_x, source_y, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref backlink);
            //backlink[source_x, source_y] = 0;
            parentX[source_x, source_y] = 0;
            parentY[source_x, source_y] = 0;

            DateTime t_start = DateTime.Now;
            while (MyCostFunction.nodeque.Count != 0)
            {
                MyCostFunction.LinkNode node = new MyCostFunction.LinkNode(MyCostFunction.nodeque[0]);
                MyCostFunction.LinkNode a = MyCostFunction.nodeque[0];
                MyCostFunction.LinkNode b = MyCostFunction.nodeque[MyCostFunction.nodeque.Count - 1];
                MyCostFunction.Swap(ref a, ref b);
                MyCostFunction.nodeque.RemoveAt(MyCostFunction.nodeque.Count - 1);
                MyCostFunction.HeapDownAdjustment(MyCostFunction.nodeque, 0, MyCostFunction.nodeque.Count);

                if (close[node.cRow, node.cColumn] == 1)
                {
                    continue;
                }
                //if (node.cRow == end_x && node.cColumn == end_y)
                //{
                //    Console.WriteLine("Find the end point.\n");
                //    break;
                //}
                MyCostFunction.CalculateNodeAccumulation_6(node.cRow, node.cColumn, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref backlink);
                //MyCostFunction.CalculateNodeAccumulation_4(node.cRow, node.cColumn, end_x, end_y, ref open, ref close, ref open_area_2, ref cost, ref parentX, ref parentY);
                close[node.cRow, node.cColumn] = 1;


                //Console.WriteLine("NodeCount:{0}, {1}%,{2},{3}.", MyCostFunction.nodeque.Count, close_node_count * 100.0 / total_node_count, close_node_count, total_node_count);
                //close_node_count++;
                //DateTime t2 = DateTime.Now;

                //MyCostFunction.nodeque.Remove(node);
                //MyCostFunction.MaxHeapify(MyCostFunction.nodeque, 0, MyCostFunction.nodeque.Count - 1); //???????????
                //MyCostFunction.nodeque.Sort(MyCostFunction.CompareNode);

                //Console.WriteLine("total seconds : {0},{1}", (t3 - t_start).TotalMilliseconds / 1000.0, (DateTime.Now - t3).TotalMilliseconds / 1000.0);
            }
            DateTime t_end = DateTime.Now;
            Console.WriteLine("total seconds(Calculation) : {0}.\n", (t_end - t_start).TotalMilliseconds / 1000.0);

            for (int i = 2; i < rpRows - 2; i++)
            {
                for (int j = 2; j < rpColumns - 2; j++)
                {
                    MyCostFunction.UpdateNodeValues(i, j, end_x, end_y, ref impedance, ref cost, ref backlink);
                }
            }
            DateTime t_end_2 = DateTime.Now;
            Console.WriteLine("total seconds(Update) : {0}.\n", (t_end_2 - t_end).TotalMilliseconds / 1000.0);
            //WriteArray2RasterFile(ref backlink, rstPixelType.PT_FLOAT, "E:\\TEST\\", "plane_5_dem");
            //MyCostFunction.LeastCostPath(end_x, end_y, ref cost, ref backlink, ref result);

            IFeatureLayer pFLayer = OpenFeatureFile(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref backlink);
            //IFeatureLayer pFLayer = OpenFeatureFile_4("E:\\TEST\\", "path_1Copy8.shp", end_x, end_y, cost[end_x, end_y] * pixel_depth, ref parentX,ref parentY);
            //WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, "E:\\TEST\\", "image51Copy2");
            //WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, "E:\\TEST\\", "image51Copy3");

            //open_area_2 = null;
            //MyCostFunction.nodeque.Clear();
            //open = null;
            //close = null;
            ////result = null;
            //GC.Collect();


            //tmp_result = (double[,])result.Clone();
            //System.Array result_2;
            //DoubleArray2SystemArray(ref result, out result_2, rpRows, rpColumns);

            //if (save_filename != null)
            //{
            //    int Index = save_filename.LastIndexOf("\\");
            //    string filePath = save_filename.Substring(0, Index);
            //    string fileName = save_filename.Substring(Index);
            //    //CreateRasterDataset_2(filePath, fileName, rstPixelType.PT_UCHAR, pRas.Raster, result);
            //}
            //else
            //{
            //    //WriteArray2RasterFile(ref backlink, rstPixelType.PT_FLOAT, "E:\\TEST\\", "image51Copy2");
            //}

        }

         
        /// <summary>
        /// 参数的初始化工作
        /// </summary>
        private void VariableInitialize()
        {

            System.Array open_area_1;
            ReadPixelValues2Array(pBaseRasLayer, out open_area_1);
            impedance = new double[rpRows, rpColumns];
            obstacle = new double[rpRows, rpColumns];

            close = new int[rpRows, rpColumns];
            open = new int[rpRows, rpColumns];
            parentX = new int[rpRows, rpColumns];
            parentY = new int[rpRows, rpColumns];

            cost = new double[rpRows, rpColumns];
            backlink = null;
            //backlink = new int[rpRows, rpColumns];
            switch (LayerPixelType[pBaseRasLayer.Name])
            {
                case rstPixelType.PT_FLOAT:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            Single t = Convert.ToSingle(open_area_1.GetValue(j, i));
                            if (t == Single.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break;
                case rstPixelType.PT_LONG:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            Int32 t = Convert.ToInt32(open_area_1.GetValue(j, i));
                            if (t == Int32.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break;
                case rstPixelType.PT_SHORT:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            Int16 t = Convert.ToInt16(open_area_1.GetValue(j, i));
                            if (t == Int16.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break;
                case rstPixelType.PT_CHAR:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            SByte t = Convert.ToSByte(open_area_1.GetValue(j, i));
                            if (t == SByte.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break; 
                case rstPixelType.PT_ULONG:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            UInt32 t = Convert.ToUInt32(open_area_1.GetValue(j, i));
                            if (t == UInt32.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break;
                case rstPixelType.PT_USHORT:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            UInt16 t = Convert.ToUInt16(open_area_1.GetValue(j, i));
                            if (t == UInt16.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break;
                case rstPixelType.PT_UCHAR:
                    for (int i = 0; i < rpRows; i++)
                    {
                        for (int j = 0; j < rpColumns; j++)
                        {
                            Byte t = Convert.ToByte(open_area_1.GetValue(j, i));
                            if (t == Byte.MinValue)
                            {
                                impedance[i, j] = Single.MinValue;
                            }
                            else
                            {
                                impedance[i, j] = t;
                            }
                            //terrain[i, j] = Single.MinValue;
                            //backlink[i, j] = -1;
                            parentX[i, j] = -1;
                            parentY[i, j] = -1;
                        }
                    }
                    break;
            }
            
            //垃圾回收
            open_area_1 = null;
            GC.Collect();

            //MyCostFunction.sw_cost = new StreamWriter("cost.txt");
            //MyCostFunction.sw_px = new StreamWriter("parent_x.txt");
            //MyCostFunction.sw_py = new StreamWriter("parent_y.txt");
        }
        #endregion

        private void 添加栅格数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = default_dir;

            string strFullPath = null;
            if (DialogResult.OK == fbd.ShowDialog())
            {
                strFullPath = fbd.SelectedPath;
                //MessageBox.Show(strFullPath);
                int Index = strFullPath.LastIndexOf("\\");
                string filePath = strFullPath.Substring(0, Index);
                string fileName = strFullPath.Substring(Index + 1);
                OpenRasterFile(filePath, fileName);
                //IRasterLayer pRasLyr = (IRasterLayer)GetLayerByName(fileName);
                //Array output;
                //ReadPixelValues2Array(pRasLyr, out output);
                //SystemArray2DoubleArray(output, out tmp_result);
            }
        }

        private void 保存数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTempDataDialog stddlg = new SaveTempDataDialog(default_dir);
            stddlg.StartPosition = FormStartPosition.CenterParent;
            stddlg.Owner = this;
            stddlg.ShowDialog();
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            //string strFullPath = null;
            //if (DialogResult.OK == fbd.ShowDialog())
            //{
            //    strFullPath = fbd.SelectedPath;
            //    //MessageBox.Show(strFullPath);
            //    int Index = strFullPath.LastIndexOf("\\");
            //    string filePath = strFullPath.Substring(0, Index);
            //    string fileName = strFullPath.Substring(Index + 1);
            //    OpenRasterFile(filePath, fileName);
            //    WriteArray2RasterFile(ref tmp_result, rstPixelType.PT_UCHAR, filePath, fileName);
            //}

        }

        public delegate void CreateRasterFileDelegate(string filepath, string filename);
        /// <summary>
        /// 在指定路径下创建一个指定名字的空Shapefile
        /// </summary>
        /// <param name="strShapeFolder">指定路径</param>
        /// <param name="strShapeName">文件名</param>
        #region 在指定路径下创建一个指定名字的空Shapefile
        public void CreateShapeFile(string strShapeFolder, string strShapeName, esriGeometryType geometry_type)
        {
            //打开工作空间
            const string strShapeFieldName = "shape";
            IWorkspaceFactory pWSF = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pWS = (IFeatureWorkspace)pWSF.OpenFromFile(strShapeFolder, 0);

            //设置字段集
            IFields pFields = new FieldsClass();
            IFieldsEdit pFieldsEdit = (IFieldsEdit)pFields;

            //设置字段
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;

            //创建类型为几何类型的字段
            pFieldEdit.Name_2 = strShapeFieldName;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            //为esriFieldTypeGeometry类型的字段创建几何定义，包括类型和空间参照 
            IGeometryDef pGeoDef = new GeometryDefClass();     //The geometry definition for the field if IsGeometry is TRUE.
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.GeometryType_2 = geometry_type;
            pGeoDefEdit.SpatialReference_2 = axMapControl1.SpatialReference;

            pFieldEdit.GeometryDef_2 = pGeoDef;
            pFieldsEdit.AddField(pField);

            //添加其他的字段
            pField = new FieldClass();
            pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = "length";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            pFieldEdit.Precision_2 = 7;//数值精度
            pFieldEdit.Scale_2 = 3;//小数点位数
            pFieldsEdit.AddField(pField);

            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)pWS;
            fieldChecker.Validate(pFields, out enumFieldError, out validatedFields);


            //创建shapefile
            pWS.CreateFeatureClass(strShapeName, pFields, null, null, esriFeatureType.esriFTSimple, strShapeFieldName, "");

        }
        #endregion
        /// <summary>
        /// 给已有的矢量数据添加字段
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="fieldname"></param>
        /// <param name="fieldtype"></param>
        /// <param name="fieldcontent"></param>
        public void AddFieldForFeatureLayer(string filepath, string filename, string fieldname, esriFieldType fieldtype, double fieldcontent)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filepath, 0);

            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            IFields pFields = pFeatureLayer.FeatureClass.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;

            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = fieldname;
            pFieldEdit.Type_2 = fieldtype;
            pFieldEdit.Precision_2 = 7;//数值精度
            pFieldEdit.Scale_2 = 3;//小数点位数
            pFieldEdit.DefaultValue_2 = fieldcontent;
            pFieldsEdit.AddField(pField);
        }
        #region 打开矢量数据
        public IFeatureLayer OpenFeatureFile(string filepath, string filename, double x = 0, double y = 0,bool add_to_layer = true)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filepath, 0);

            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            if (x != 0 && y != 0)
            {
                //将原要素清空，再重新打开
                IFeatureClass pFCChecker = pFeatureWorkspace.OpenFeatureClass(filename);
                if (pFCChecker != null)
                {
                    IDataset pds = pFCChecker as IDataset;
                    pds.Delete();
                }//MessageBox.Show("Delete Succeed!");
                CreateShapeFile(filepath, filename, esriGeometryType.esriGeometryPoint);
                pFeatureLayer = new FeatureLayerClass();
                pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
                pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;
                IFeature pFeat = null;
                try
                {
                    pFeat = pFeatureLayer.FeatureClass.CreateFeature();
                    
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString());
                }

                IPoint pt = new PointClass();
                pt.PutCoords(x, y);
                pFeat.Shape = pt;
                pFeat.Store();
            }

            if (add_to_layer)
            {
                for (int k = axMapControl1.LayerCount - 1; k >= 0; k--)
                {
                    if (axMapControl1.get_Layer(k).Name == filename)
                    {
                        axMapControl1.DeleteLayer(k);
                        break;
                    }
                }
                FeaturesUniqueRenderer(pFeatureLayer);
                axMapControl1.Map.AddLayer(pFeatureLayer);
                axMapControl1.ActiveView.Refresh();
            }
            return pFeatureLayer;
        }

        
        public IFeatureLayer OpenFeatureFile(string filepath, string filename, int x, int y, double length, ref int[,] backlink)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            #region 获取当前路径和文件名
            //OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Filter = "Shape(*.shp)|*.shp|All Files(*.*)|*.*";
            //dlg.Title = "Open Shapefile data";
            //dlg.ShowDialog();
            //string strFullPath = dlg.FileName;
            //if (strFullPath == "") return;
            //int Index = strFullPath.LastIndexOf("\\");
            //string filePath = strFullPath.Substring(0, Index);
            //string fileName = strFullPath.Substring(Index + 1);
            #endregion
            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filepath, 0);

            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            //将原要素清空，再重新打开
            IFeatureClass pFCChecker = pFeatureWorkspace.OpenFeatureClass(filename);
            if (pFCChecker != null)
            {
                IDataset pds = pFCChecker as IDataset;
                pds.Delete();
            }
            //MessageBox.Show("Delete Succeed!");
            CreateShapeFile(filepath, filename, esriGeometryType.esriGeometryPolyline);
            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            ISegmentCollection pSegColl = new PathClass();
            IGeometryCollection pPolyline = new PolylineClass();

            double xmin = OutExtent.XMin;
            double ymax = OutExtent.YMax;

            IPoint pt1 = null;
            IPoint pt2 = null;
            IFeature pFeat = null;
            object o = Type.Missing;
            try
            {
                pFeat = pFeatureLayer.FeatureClass.CreateFeature();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
            pt1 = RowCol2MapXY(x, y, xmin, ymax);
            while (backlink[x, y] != 0)
            {
                int delta_x = 0, delta_y = 0;
                MyCostFunction.BackLinkDirection2RowColumn_2(backlink[x, y], out delta_x, out delta_y);
                x += delta_x;
                y += delta_y;
                pt2 = RowCol2MapXY(x, y, xmin, ymax);
                ILine pLine = new LineClass();
                pLine.PutCoords(pt1, pt2);
                ISegment pSeg = pLine as ISegment;
                pSegColl.AddSegment(pSeg, ref o, ref o);
                pt1 = pt2;
            }
            pPolyline.AddGeometry(pSegColl as IGeometry, ref o, ref o);
            pFeat.Shape = pPolyline as IPolyline;
            int i = pFeatureLayer.FeatureClass.FindField("length");
            pFeat.set_Value(i, length);
            pFeat.Store();

            #region 获取每个feature的点坐标
            //IFeatureCursor pFeatureCursor = pFeatureLayer.Search(null, false);
            //IFeature pFeature = pFeatureCursor.NextFeature();
            //while (pFeature != null)
            //{
            //    IGeometry pGeometry = pFeature.Shape;
            //    IPointCollection pPointCollection = pGeometry as IPointCollection;
            //    //IPointCollection pPointCollection = pGeometry as IPointCollection;///  nullll
            //    IPoint pPoint = pGeometry as IPoint;
            //    pPoint.PutCoords(x, y);
            //    MessageBox.Show(x.ToString()+", "+ y.ToString());
            //    //MessageBox.Show(pPointCollection.PointCount.ToString());
            //    //for (int i = 0; i < pPointCollection.PointCount; i++)
            //    //{
            //    //    pPoint = pPointCollection.get_Point(i);
            //    //    MessageBox.Show(pPoint.X.ToString() + "," + pPoint.Y.ToString());
            //    //}

            //    pFeature = pFeatureCursor.NextFeature();
            //}
            #endregion

            FeaturesUniqueRenderer(pFeatureLayer);
            axMapControl1.Map.AddLayer(pFeatureLayer);
            axMapControl1.ActiveView.Refresh();

            return pFeatureLayer;
        }

        public IFeatureLayer OpenFeatureFile_4(string filepath, string filename, int x, int y, double length, ref int[,] parentX, ref int[,] parentY)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filepath, 0);

            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            //将原要素清空，再重新打开
            IFeatureClass pFCChecker = pFeatureWorkspace.OpenFeatureClass(filename);
            if (pFCChecker != null)
            {
                IDataset pds = pFCChecker as IDataset;
                pds.Delete();
            }
            //MessageBox.Show("Delete Succeed!");
            CreateShapeFile(filepath, filename, esriGeometryType.esriGeometryPolyline);
            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            ISegmentCollection pSegColl = new PathClass();
            IGeometryCollection pPolyline = new PolylineClass();

            double xmin = OutExtent.XMin;
            double ymax = OutExtent.YMax;

            IPoint pt1 = null;
            IPoint pt2 = null;
            IFeature pFeat = null;
            object o = Type.Missing;
            try
            {
                pFeat = pFeatureLayer.FeatureClass.CreateFeature();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
            pt1 = RowCol2MapXY(x, y, xmin, ymax);
            int x0 = parentX[x, y];
            int y0 = parentY[x, y];
            while (x0 > 0 && y0 > 0)
            {
                x = x0;
                y = y0;
                pt2 = RowCol2MapXY(x, y, xmin, ymax);
                ILine pLine = new LineClass();
                pLine.PutCoords(pt1, pt2);
                ISegment pSeg = pLine as ISegment;
                pSegColl.AddSegment(pSeg, ref o, ref o);
                pt1 = pt2;
                x0 = parentX[x, y];
                y0 = parentY[x, y];
            }
            pPolyline.AddGeometry(pSegColl as IGeometry, ref o, ref o);
            pFeat.Shape = pPolyline as IPolyline;
            int i = pFeatureLayer.FeatureClass.FindField("length");
            pFeat.set_Value(i, length);
            pFeat.Store();

            #region 获取每个feature的点坐标
            //IFeatureCursor pFeatureCursor = pFeatureLayer.Search(null, false);
            //IFeature pFeature = pFeatureCursor.NextFeature();
            //while (pFeature != null)
            //{
            //    IGeometry pGeometry = pFeature.Shape;
            //    IPointCollection pPointCollection = pGeometry as IPointCollection;
            //    //IPointCollection pPointCollection = pGeometry as IPointCollection;///  nullll
            //    IPoint pPoint = pGeometry as IPoint;
            //    pPoint.PutCoords(x, y);
            //    MessageBox.Show(x.ToString()+", "+ y.ToString());
            //    //MessageBox.Show(pPointCollection.PointCount.ToString());
            //    //for (int i = 0; i < pPointCollection.PointCount; i++)
            //    //{
            //    //    pPoint = pPointCollection.get_Point(i);
            //    //    MessageBox.Show(pPoint.X.ToString() + "," + pPoint.Y.ToString());
            //    //}

            //    pFeature = pFeatureCursor.NextFeature();
            //}
            #endregion

            FeaturesUniqueRenderer(pFeatureLayer);
            axMapControl1.Map.AddLayer(pFeatureLayer);
            axMapControl1.ActiveView.Refresh();

            return pFeatureLayer;
        }

        public IFeatureLayer OpenFeatureFile_5(string filepath, string filename, int x, int y, double length, ref int[,] parentX, ref int[,] parentY)
        {
            IWorkspaceFactory pWorkspaceFactory;
            IFeatureWorkspace pFeatureWorkspace;
            IFeatureLayer pFeatureLayer;

            //打开工作空间并添加shp文件
            pWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
            pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filepath, 0);

            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            //将原要素清空，再重新打开
            IFeatureClass pFCChecker = pFeatureWorkspace.OpenFeatureClass(filename);
            if (pFCChecker != null)
            {
                IDataset pds = pFCChecker as IDataset;
                pds.Delete();
            }
            //MessageBox.Show("Delete Succeed!");
            CreateShapeFile(filepath, filename, esriGeometryType.esriGeometryPolyline);
            pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureWorkspace.OpenFeatureClass(filename);
            pFeatureLayer.Name = pFeatureLayer.FeatureClass.AliasName;

            ISegmentCollection pSegColl = new PathClass();
            IGeometryCollection pPolyline = new PolylineClass();

            double xmin = OutExtent.XMin;
            double ymax = OutExtent.YMax;

            IPoint pt1 = null;
            IPoint pt2 = null;
            IFeature pFeat = null;
            object o = Type.Missing;
            try
            {
                pFeat = pFeatureLayer.FeatureClass.CreateFeature();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
            double p_length = 0;
            pt1 = RowCol2MapXY(x, y, xmin, ymax);
            int x0 = parentX[x, y];
            int y0 = parentY[x, y];
            while (x0 > 0 && y0 > 0)
            {
                p_length += Math.Sqrt((x - x0)*(x - x0) + (y - y0) * (y - y0));
                x = x0;
                y = y0;
                pt2 = RowCol2MapXY(x, y, xmin, ymax);
                ILine pLine = new LineClass();
                pLine.PutCoords(pt1, pt2);
                ISegment pSeg = pLine as ISegment;
                pSegColl.AddSegment(pSeg, ref o, ref o);
                pt1 = pt2;
                x0 = parentX[x, y];
                y0 = parentY[x, y];
                
            }
            pPolyline.AddGeometry(pSegColl as IGeometry, ref o, ref o);
            pFeat.Shape = pPolyline as IPolyline;
            int i = pFeatureLayer.FeatureClass.FindField("length");
            pFeat.set_Value(i, length);
            pFeat.Store();

            #region 获取每个feature的点坐标
            //IFeatureCursor pFeatureCursor = pFeatureLayer.Search(null, false);
            //IFeature pFeature = pFeatureCursor.NextFeature();
            //while (pFeature != null)
            //{
            //    IGeometry pGeometry = pFeature.Shape;
            //    IPointCollection pPointCollection = pGeometry as IPointCollection;
            //    //IPointCollection pPointCollection = pGeometry as IPointCollection;///  nullll
            //    IPoint pPoint = pGeometry as IPoint;
            //    pPoint.PutCoords(x, y);
            //    MessageBox.Show(x.ToString()+", "+ y.ToString());
            //    //MessageBox.Show(pPointCollection.PointCount.ToString());
            //    //for (int i = 0; i < pPointCollection.PointCount; i++)
            //    //{
            //    //    pPoint = pPointCollection.get_Point(i);
            //    //    MessageBox.Show(pPoint.X.ToString() + "," + pPoint.Y.ToString());
            //    //}

            //    pFeature = pFeatureCursor.NextFeature();
            //}
            #endregion

            FeaturesUniqueRenderer(pFeatureLayer);
            axMapControl1.Map.AddLayer(pFeatureLayer);
            axMapControl1.ActiveView.Refresh();

            if (p_length != 0)
            {
                MessageBox.Show("起点与终点之间的最短路径长度为" + p_length + "m， 步行所需时间为 " + length + " s.", "路径结果");
            }
            else
            {
                MessageBox.Show("两点之间无通路！！","路径结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return pFeatureLayer;
        }
        #endregion

        public void GetXYFromDirection(int x, int y, int direction, out int ox, out int oy)
        {
            switch (direction)
            {
                case 1: y += 1;
                    break;
                case 2: x += 1; y += 1;
                    break;
                case 3: x += 1;
                    break;
                case 4: x += 1; y -= 1;
                    break;
                case 5: y -= 1;
                    break;
                case 6: x -= 1; y -= 1;
                    break;
                case 7: x -= 1;
                    break;
                case 8: x -= 1; y += 1;
                    break;
            }
            ox = x;
            oy = y;
        }

        public void SetWorkingExtent(ref IEnvelope OutExtent)
        {
            int l_count = axMapControl1.LayerCount;
            if (l_count == 0)
            {
                MessageBox.Show("无可用图层！", "Error！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] l_names = new string[l_count];
            for (int i = 0; i < l_count; )
            {
                ILayer p = axMapControl1.get_Layer(i);
                if (p is IRasterLayer)
                {
                    l_names[i++] = p.Name;
                }
            }
            LayerListDialog lld = new LayerListDialog(l_names,l_count);
            if (lld.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string l_name = lld.GetReturnString();
                IRasterLayer pRLayer = GetLayerByName(l_name) as IRasterLayer;
                IRasterProps pRProps = (IRasterProps)pRLayer.Raster;
                OutExtent = pRProps.Extent;
                string v1  = ShowEnvelope(OutExtent);
                string v2 = ShowEnvelope(axMapControl1.Extent);
                WriteTexts2RichTextBox(richTextBox1, v1);
                WriteTexts2RichTextBox(richTextBox1, v2);
            }
        }

        public string ShowEnvelope(IEnvelope pEnvel)
        {
            if (pEnvel == null)
                return null;
            return "XMax: " + pEnvel.XMax + ", XMin :" + pEnvel.XMin + ";\n" + "YMax: " + pEnvel.YMax + ", YMin: " + pEnvel.YMin + ".";
        }

        public bool bool_start_pt = false, bool_end_pt = false;
        public int sRow = 0, sColumn = 0;
        public int eRow = 0, eColumn = 0;
        public bool bool_recalculation = true;
        private void axMapControl1_OnMouseDown(object sender, AxESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 1)
            {
                if (default_dir == "")
                {
                    MessageBox.Show("未设置工作目录！", "Error！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (OutExtent == null)
                {
                    MessageBox.Show("未设置坐标参考图层！","Error！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //LayerListDialog lld = new LayerListDialog();
                    SetWorkingExtent(ref OutExtent);
                }
                if (bool_start_pt)
                {
                    sRow = (int)((OutExtent.YMax - e.mapY) / cell_size);
                    sColumn = (int)((e.mapX - OutExtent.XMin) / cell_size);

                    OpenFeatureFile(default_dir, start_point_name, e.mapX, e.mapY);
                    readstartpointfromfile = false;
                    bool_recalculation = true;
                }
                if (bool_end_pt)
                {
                    eRow = (int)((OutExtent.YMax - e.mapY) / cell_size);
                    eColumn = (int)((e.mapX - OutExtent.XMin) / cell_size);

                    OpenFeatureFile(default_dir, end_point_name, e.mapX, e.mapY);
                    readendpointfromfile = false;
                }
                if (this.设置终点ToolStripMenuItem.Checked && !bool_recalculation)
                {
                    IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, eRow, eColumn, cost[eRow, eColumn] * cell_size, ref parentX, ref parentY);
                }
            }
        }

        private void 设置起点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (start_point_name == "")
            {
                OpenFileDialog ofg = new OpenFileDialog();
                ofg.Title = "打开起点文件";
                ofg.Filter = "Shapefile | *.shp";
                if (ofg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fullpath = ofg.FileName;
                    start_point_name = fullpath.Substring(fullpath.LastIndexOf("\\") + 1);
                }
            }
            if (this.设置起点ToolStripMenuItem.CheckState == CheckState.Unchecked)
            {
                this.设置起点ToolStripMenuItem.Checked = true;
                this.设置终点ToolStripMenuItem.Checked = false;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                bool_start_pt = true;
                if (bool_end_pt)
                {
                    bool_end_pt = false;
                }
            }
            else
            {
                this.设置起点ToolStripMenuItem.Checked = false;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
                bool_start_pt = false;
            }
        }

        private void 设置终点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (end_point_name == "")
            {
                OpenFileDialog ofg = new OpenFileDialog();
                ofg.Title = "打开终点文件";
                ofg.Filter = "Shapefile | *.shp";
                if (ofg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fullpath = ofg.FileName;
                    end_point_name = fullpath.Substring(fullpath.LastIndexOf("\\") + 1);
                }
            }
            if (this.设置终点ToolStripMenuItem.CheckState == CheckState.Unchecked)
            {
                this.设置终点ToolStripMenuItem.Checked = true;
                this.设置起点ToolStripMenuItem.Checked = false;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                bool_end_pt = true;
                if (bool_start_pt)
                {
                    bool_start_pt = false;
                }
            }
            else
            {
                this.设置终点ToolStripMenuItem.Checked = false;
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
                bool_end_pt = false;
            }

        }

        public void SimpleRenderer(IFeatureLayer featLayer, string fieldName, IColorRamp colorRamp)
        {
            IGeoFeatureLayer pGeoFeatureLayer = featLayer as IGeoFeatureLayer;
            IFeatureClass pFeatureClass = featLayer.FeatureClass;      //获取图层上的featureClass            
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            IUniqueValueRenderer pUniqueValueRenderer = new UniqueValueRendererClass();   //唯一值渲染器

            //设置渲染字段对象
            pUniqueValueRenderer.FieldCount = 1;
            pUniqueValueRenderer.set_Field(0, fieldName);
            ISimpleFillSymbol pSimFillSymbol = new SimpleFillSymbolClass();   //创建填充符号
            pUniqueValueRenderer.DefaultSymbol = (ISymbol)pSimFillSymbol;
            pUniqueValueRenderer.UseDefaultSymbol = false;
            int n = pFeatureClass.FeatureCount(null);
            for (int i = 0; i < n; i++)
            {
                IFeature pFeature = pFeatureCursor.NextFeature();
                IClone pSourceClone = pSimFillSymbol as IClone;
                ISimpleFillSymbol pSimpleFillSymbol = pSourceClone.Clone() as ISimpleFillSymbol;
                string pFeatureValue = pFeature.get_Value(pFeature.Fields.FindField(fieldName)).ToString();
                pUniqueValueRenderer.AddValue(pFeatureValue, "", (ISymbol)pSimpleFillSymbol);

            }

            //为每个符号设置颜色

            for (int i = 0; i <= pUniqueValueRenderer.ValueCount - 1; i++)
            {
                string xv = pUniqueValueRenderer.get_Value(i);

                if (xv != "")
                {
                    ISimpleFillSymbol pNextSymbol = (ISimpleFillSymbol)pUniqueValueRenderer.get_Symbol(xv);
                    pNextSymbol.Color = colorRamp.get_Color(127);
                    pUniqueValueRenderer.set_Symbol(xv, (ISymbol)pNextSymbol);
                }
            }

            pGeoFeatureLayer.Renderer = (IFeatureRenderer)pUniqueValueRenderer;
        }

        public void FeaturesUniqueRenderer(IFeatureLayer featLayer,int line_width=4,string fieldName="FID")
        {
            if (featLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                IGeoFeatureLayer pGeoFeatureLayer = featLayer as IGeoFeatureLayer;
                IUniqueValueRenderer pUniqueValueRenderer = new UniqueValueRendererClass();

                IRgbColor pLineColor = new RgbColorClass();
                pLineColor.Red = 255;
                pLineColor.Green = 0;
                pLineColor.Blue = 0;

                ISimpleLineSymbol pLineSymbol = new SimpleLineSymbolClass();
                pLineSymbol.Color = pLineColor;
                pLineSymbol.Width = line_width;

                //设置渲染字段对象
                pUniqueValueRenderer.FieldCount = 1;
                pUniqueValueRenderer.set_Field(0, fieldName);
                pUniqueValueRenderer.DefaultSymbol = (ISymbol)pLineSymbol;
                pUniqueValueRenderer.UseDefaultSymbol = true;

                pGeoFeatureLayer.Renderer = (IFeatureRenderer)pUniqueValueRenderer;
            }
        }


        public void ClassifyColorRampForRaster(IRasterLayer pRasterLayer)
        {
            //值分级
            IBasicHistogram pBasicHis = new BasicTableHistogramClass();
            ITableHistogram pTabHis = (ITableHistogram)pBasicHis;
            pTabHis.Field = "w1";

            ITable pTab = (ITable)axMapControl1.get_Layer(0);
            pTabHis.Table = pTab;

            object doubleArrVal, longArrFreq;
            pBasicHis.GetHistogram(out doubleArrVal, out longArrFreq);

            //NaturalBreaksClass
            IClassifyGEN pClassify = new EqualIntervalClass();

            int nDes = 5;

            pClassify.Classify(doubleArrVal, longArrFreq, ref nDes);
            object classes = pClassify.ClassBreaks;

            System.Array pArr = (System.Array)classes;

            //算法梯度颜色
            IAlgorithmicColorRamp pAlgoColorRamp = new AlgorithmicColorRampClass();
            pAlgoColorRamp.Size = pArr.Length;
            IRgbColor pFromColor = new RgbColorClass(), pToColor = new RgbColorClass();
            pFromColor.Red = 0;
            pFromColor.Green = 255;
            pFromColor.Blue = 0;
            pToColor.Red = 255;
            pToColor.Green = 0;
            pToColor.Blue = 255;

            pAlgoColorRamp.FromColor = pFromColor;
            pAlgoColorRamp.ToColor = pToColor;
            bool ok = true;
            try
            {
                pAlgoColorRamp.CreateRamp(out ok);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //颜色梯度结束

            IClassBreaksRenderer pRender = new ClassBreaksRendererClass();
            pRender.BreakCount = pArr.Length;
            pRender.Field = "w1";
            ISimpleFillSymbol pSym;
            for (int i = 0; i < pArr.Length; i++)
            {
                pRender.set_Break(i, (double)pArr.GetValue(i));
                pSym = new SimpleFillSymbolClass();
                pSym.Color = pAlgoColorRamp.get_Color(i);
                pRender.set_Symbol(i, (ISymbol)pSym);
            }

            IGeoFeatureLayer pGeoLyr = (IGeoFeatureLayer)axMapControl1.get_Layer(0);
            pGeoLyr.Renderer = (IFeatureRenderer)pRender;

            axMapControl1.Refresh();
            axTOCControl1.Update();

        }
        private static IColor GetColor(int red, int green, int blue)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = red;
            rgbColor.Green = green;
            rgbColor.Blue = blue;
            IColor color = rgbColor as IColor;
            return color;
        }

        public bool UniqueValueRenderer(IRasterLayer rasterLayer, int from_red = 0, int from_green = 0, int from_blue = 0, int to_red = 0, int to_green = 0, int to_blue = 255, string renderfiled = "Value")
        {
            try
            {
                
                //IColorRamp colorRamp = new RasterStretchColorRampRendererClass()
                IRgbColor pFromColor = GetColor(from_red, from_green, from_blue) as IRgbColor;
                IRgbColor pToColor = GetColor(to_red, to_green, to_blue) as IRgbColor;

                IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRamp() as IAlgorithmicColorRamp;
                colorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
                colorRamp.FromColor = pFromColor;
                colorRamp.ToColor = pToColor;

                IRasterUniqueValueRenderer uniqueValueRenderer = new RasterUniqueValueRendererClass();
                IRasterRenderer pRasterRenderer = uniqueValueRenderer as IRasterRenderer;
                pRasterRenderer.Raster = rasterLayer.Raster;
                pRasterRenderer.Update();
                IUniqueValues uniqueValues = new UniqueValuesClass();
                IRasterCalcUniqueValues calcUniqueValues = new RasterCalcUniqueValuesClass();
                calcUniqueValues.AddFromRaster(rasterLayer.Raster, 0, uniqueValues);//iBand=0
                IRasterRendererUniqueValues renderUniqueValues = uniqueValueRenderer as IRasterRendererUniqueValues;
                renderUniqueValues.UniqueValues = uniqueValues;
                uniqueValueRenderer.Field = renderfiled;

                int unique_value_counts = uniqueValues.Count > 1 ? uniqueValues.Count : 2;
                colorRamp.Size = unique_value_counts;

                uniqueValueRenderer.HeadingCount = 1;
                uniqueValueRenderer.set_Heading(0, "All Data Value");
                uniqueValueRenderer.set_ClassCount(0, uniqueValues.Count);
                bool pOk;
                colorRamp.CreateRamp(out pOk);
                IRasterRendererColorRamp pRasterRendererColorRamp = uniqueValueRenderer as IRasterRendererColorRamp;
                pRasterRendererColorRamp.ColorRamp = colorRamp;
                for (int i = 0; i < uniqueValues.Count; i++)
                {
                    uniqueValueRenderer.AddValue(0, i, uniqueValues.get_UniqueValue(i));
                    uniqueValueRenderer.set_Label(0, i, uniqueValues.get_UniqueValue(i).ToString());
                    IFillSymbol fs = new SimpleFillSymbol();
                    fs.Color = colorRamp.get_Color(i);
                    uniqueValueRenderer.set_Symbol(0, i, fs as ISymbol);
                }
                pRasterRenderer.Update();
                rasterLayer.Renderer = pRasterRenderer;
            }
            catch (Exception ex)
            {
                WriteTexts2RichTextBox(richTextBox1, ex.Source + ": " + ex.Message);
                return false;
            }
            return true;
        }

        public bool UniqueValueRenderer(IRasterLayer rasterLayer,ref IRgbColor pFromColor,  ref IRgbColor pToColor, string renderfiled = "Value")
        {
            try
            {
                IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRamp() as IAlgorithmicColorRamp;
                colorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
                colorRamp.FromColor = pFromColor;
                colorRamp.ToColor = pToColor;

                IRasterUniqueValueRenderer uniqueValueRenderer = new RasterUniqueValueRendererClass();
                IRasterRenderer pRasterRenderer = uniqueValueRenderer as IRasterRenderer;
                pRasterRenderer.Raster = rasterLayer.Raster;
                pRasterRenderer.Update();
                IUniqueValues uniqueValues = new UniqueValuesClass();
                IRasterCalcUniqueValues calcUniqueValues = new RasterCalcUniqueValuesClass();
                calcUniqueValues.AddFromRaster(rasterLayer.Raster, 0, uniqueValues);//iBand=0
                IRasterRendererUniqueValues renderUniqueValues = uniqueValueRenderer as IRasterRendererUniqueValues;
                renderUniqueValues.UniqueValues = uniqueValues;
                uniqueValueRenderer.Field = renderfiled;

                int unique_value_counts = uniqueValues.Count > 1 ? uniqueValues.Count : 2;
                colorRamp.Size = unique_value_counts;
                
                uniqueValueRenderer.HeadingCount = 1;
                uniqueValueRenderer.set_Heading(0, "All Data Value");
                uniqueValueRenderer.set_ClassCount(0, uniqueValues.Count);
                bool pOk;
                colorRamp.CreateRamp(out pOk);
                IRasterRendererColorRamp pRasterRendererColorRamp = uniqueValueRenderer as IRasterRendererColorRamp;
                pRasterRendererColorRamp.ColorRamp = colorRamp;
                for (int i = 0; i < uniqueValues.Count; i++)
                {
                    uniqueValueRenderer.AddValue(0, i, uniqueValues.get_UniqueValue(i));
                    uniqueValueRenderer.set_Label(0, i, uniqueValues.get_UniqueValue(i).ToString());
                    IFillSymbol fs = new SimpleFillSymbol();
                    fs.Color = colorRamp.get_Color(i);
                    uniqueValueRenderer.set_Symbol(0, i, fs as ISymbol);
                }
                pRasterRenderer.Update();
                rasterLayer.Renderer = pRasterRenderer;

            }
            catch (Exception ex)
            {
                WriteTexts2RichTextBox(richTextBox1, ex.Source + ": " + ex.Message);
                return false;
            }
            return true;
        }

        //栅格分层设色
        public void funColorForRaster_Classify(IRasterLayer pRasterLayer, int from_red = 255, int from_green = 255, int from_blue = 255, int to_red = 0, int to_green = 0, int to_blue = 255)
        {
            IRasterClassifyColorRampRenderer pRClassRend = new RasterClassifyColorRampRenderer() as IRasterClassifyColorRampRenderer;
            IRasterRenderer pRRend = pRClassRend as IRasterRenderer;

            #region 遗弃代码

            //IRaster pRaster = pRasterLayer.Raster;
            //IRasterBandCollection pRBandCol = pRaster as IRasterBandCollection;
            //IRasterBand pRBand = pRBandCol.Item(0);

            //if (pRBand.Histogram == null)
            //{
            //    pRBand.ComputeStatsAndHist();
            //}

            //IRasterHistogram rh = pRBand.Histogram;
            ////MessageBox.Show("th.Counts: "+(double)rh.Counts);

            //int counts = 0;
            //double[] his = (double[])rh.Counts;
            //for (int i = 0; i < 256; i++)
            //{
            //    if (his[i] != 0)
            //    {
            //        counts++;
            //    }
            //}

            ////MessageBox.Show(counts.ToString());
            //IRasterStatistics pRasterStatistic = pRBand.Statistics;
            //double dMaxValue = pRasterStatistic.Maximum;
            //double dMinValue = pRasterStatistic.Minimum;


            //if (counts <= 1)
            //{
            //    counts = 2;
            //}
            #endregion

            IUniqueValues uniqueValues = new UniqueValuesClass();
            IRasterCalcUniqueValues calcUniqueValues = new RasterCalcUniqueValuesClass();
            calcUniqueValues.AddFromRaster(pRasterLayer.Raster, 0, uniqueValues);//iBand=0

            pRClassRend.ClassCount = uniqueValues.Count;
            pRRend.Update();

            IRgbColor pFromColor = GetColor(from_red, from_green, from_blue) as IRgbColor;
            IRgbColor pToColor = GetColor(to_red, to_green, to_blue) as IRgbColor;

            IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRamp() as IAlgorithmicColorRamp;
            colorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
            colorRamp.Size = uniqueValues.Count > 1 ? uniqueValues.Count : 2;
            colorRamp.FromColor = pFromColor;
            colorRamp.ToColor = pToColor;
            bool createColorRamp;

            try
            {
                colorRamp.CreateRamp(out createColorRamp);
            }
            catch(Exception ex)
            {
                richTextBox1.Text += "Create colorRamp Error: " + ex.Message + "\n";
            }

            IFillSymbol fillSymbol = new SimpleFillSymbol() as IFillSymbol;
            for (int i = 0; i < pRClassRend.ClassCount; i++)
            {
                fillSymbol.Color = colorRamp.get_Color(i);
                pRClassRend.set_Symbol(i, fillSymbol as ISymbol);
                pRClassRend.set_Label(i, pRClassRend.get_Break(i).ToString("0.00"));
            }
            pRasterLayer.Renderer = pRRend;
            pRClassRend.SortClassesAscending = true;
        }

        public void funColorForRaster_Classify(IRasterLayer pRasterLayer, ref IRgbColor pFromColor, ref IRgbColor pToColor)
        {
            IRasterClassifyColorRampRenderer pRClassRend = new RasterClassifyColorRampRenderer() as IRasterClassifyColorRampRenderer;
            IRasterRenderer pRRend = pRClassRend as IRasterRenderer;

            IUniqueValues uniqueValues = new UniqueValuesClass();
            IRasterCalcUniqueValues calcUniqueValues = new RasterCalcUniqueValuesClass();

            calcUniqueValues.AddFromRaster(pRasterLayer.Raster, 0, uniqueValues);//iBand=0
            int counts = uniqueValues.Count > 1 ? uniqueValues.Count : 2;
            if (counts > 10) counts = 10;
            pRClassRend.ClassCount = counts;
            pRRend.Update();

            IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRamp() as IAlgorithmicColorRamp;
            colorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
            colorRamp.Size = counts; //Size 为 1 则会创建出错！！！
            colorRamp.FromColor = pFromColor;
            colorRamp.ToColor = pToColor;
            bool createColorRamp;

            try
            {
                colorRamp.CreateRamp(out createColorRamp);
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "error: " + ex.Message + "\n";
            }

            IFillSymbol fillSymbol = new SimpleFillSymbol() as IFillSymbol;
            for (int i = 0; i < counts; i++)
            {
                fillSymbol.Color = colorRamp.get_Color(i);
                pRClassRend.set_Symbol(i, fillSymbol as ISymbol);
                pRClassRend.set_Label(i, pRClassRend.get_Break(i).ToString("0.00"));
            }
            pRasterLayer.Renderer = pRRend;
            pRClassRend.SortClassesAscending = true;
        }


        public string CurrentLayerName = null;
        public ILayer pSelectedLayer = null;
        private void axTOCControl1_OnMouseUp(object sender, AxESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseUpEvent e)
        {
            if (e.button == 2)
            {
                //contextMenuStrip1.Show(axTOCControl1, new Point(e.x, e.y));
                esriTOCControlItem tool_item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pBasicMap = null;

                System.Object unk = null;
                System.Object data = null;
                axTOCControl1.HitTest(e.x, e.y, ref tool_item, ref pBasicMap, ref pSelectedLayer, ref unk, ref data);

                if (tool_item == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    contextMenuStrip1.Show(axTOCControl1, new System.Drawing.Point(e.x, e.y));
                    CurrentLayerName = pSelectedLayer.Name;
                    //m_ToolbarMenu.AddItem(new OpenAttriuteTable(pLayer), 0, -1, true, esriCommandStyles.esriCommandStyleTextOnly);
                    //m_ToolbarMenu.PopupMenu(e.x, e.y, m_TOCControl.hWnd);
                    //m_ToolbarMenu.RemoveAll();



                }
                else if (tool_item == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    if (pSelectedLayer is IRasterLayer)
                    {
                        ColorCustomize cc = new ColorCustomize();
                        if (cc.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            IRgbColor pFromColor = cc.GetFromColor() as IRgbColor;
                            IRgbColor pToColor = cc.GetToColor() as IRgbColor;

                            if (cc.radio_discrete.Checked)
                            {
                                UniqueValueRenderer(pSelectedLayer as IRasterLayer, ref pFromColor, ref pToColor);
                            }
                            if(cc.radio_continous.Checked)
                            {
                                funColorForRaster_Classify(pSelectedLayer as IRasterLayer, ref pFromColor, ref pToColor);
                            }
                        }
                    }
                    if (pSelectedLayer is IFeatureLayer)
                    {
                        FeaturesUniqueRenderer(pSelectedLayer as IFeatureLayer);
                        #region 被遗弃的代码
                        
                        //IRgbColor pLineColor = new RgbColorClass();
                        //pLineColor.Red = 255;
                        //pLineColor.Green = 0;
                        //pLineColor.Blue = 0;

                        //ICartographicLineSymbol pCartoLineSymbol = new CartographicLineSymbolClass();
                        //pCartoLineSymbol.Width = 2;
                        //pCartoLineSymbol.Color = pLineColor;




                        
                        ////创建一个填充符号
                        //ISimpleFillSymbol pSmplFillSymbol = new SimpleFillSymbol();

                        ////设置填充符号的属性
                        //IColor pRgbClr = new RgbColorClass();
                        //IFillSymbol pFillSymbol = pSmplFillSymbol;
                        //pFillSymbol.Color = pRgbClr;
                        //pFillSymbol.Outline = pCartoLineSymbol;

                        //FeaturesUniqueRenderer(pSelectedLayer as IFeatureLayer,"FID");
                        //ILegendClass pLC = new LegendClassClass();
                        //ILegendGroup pLG = new LegendGroupClass();
                        //if (unk is ILegendGroup)
                        //{
                        //    pLG = (ILegendGroup)unk;
                        //}
                        //pLC = pLG.get_Class((int)data);
                        //ISymbol pSym = pLC.Symbol;
                        
                        //try
                        //{
                        //    ISymbolSelector pSS = new SymbolSelectorClass(); 
                        //    pSS.AddSymbol(pSym);
                        //    bool bOK = pSS.SelectSymbol(0);
                        //    if (bOK)
                        //    {
                        //        pLC.Symbol = pSS.GetSymbolAt(0);
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    WriteTexts2RichTextBox(richTextBox1, "矢量图层渲染错误: " + ex.Message);

                        //}
                        #endregion
                    }


                    axTOCControl1.Update();
                    axMapControl1.ActiveView.Refresh();
                }
            }
        }

        private void zoomToLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ILayer pLayer = GetLayerByName(CurrentLayerName);
            axMapControl1.Extent = pLayer.AreaOfInterest; //Zoom To Layer的关键代码
            axMapControl1.ActiveView.Refresh();
        }

        private void deleteLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pSelectedLayer != null)
                axMapControl1.Map.DeleteLayer(pSelectedLayer);
            pSelectedLayer = null;
        }

        private static IPoint RowCol2MapXY(int row, int col, double xmin, double ymax)
        {
            double x1 = xmin + col * cell_size + 0.05;
            double y1 = ymax - row * cell_size - 0.05;
            IPoint pt = new PointClass();
            pt.PutCoords(x1, y1);
            return pt;
        }

        private void 设置目标图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            string strFullPath = null;
            if (DialogResult.OK == fbd.ShowDialog())
            {
                strFullPath = fbd.SelectedPath;
                //MessageBox.Show(strFullPath);
                int Index = strFullPath.LastIndexOf("\\");
                string filePath = strFullPath.Substring(0, Index);
                string fileName = strFullPath.Substring(Index + 1);

                target_layer_name = fileName;
                SetRasterLayerAsTarget(pBaseRasLayer);
                
            }
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {

            IFeatureLayer pFLayer = OpenFeatureFile("E:\\TEST\\", "path_1.shp");
            IFeatureCursor pFCursor = pFLayer.Search(null, false);

            IPolyline pPLine = pFCursor.NextFeature().Shape as IPolyline;
            SplitLineWithPolygons(pPLine, OpenFeatureFile("E:\\TEST\\", "grass_land2.shp"));


            //OpenFeatureFile("E:\\TEST\\","grass_land");
            //double xmin = OutExtent.XMin;
            //double ymax = OutExtent.YMax;
            //IFeatureLayer pFLayer = (IFeatureLayer)GetLayerByName("grass_land");
            //IPolyline pPLine = new PolylineClass();

            //IPoint fpoint = RowCol2MapXY(819, 194, xmin, ymax);
            //IPoint tpoint = RowCol2MapXY(1677, 2024, xmin, ymax);
            //pPLine.FromPoint = fpoint;
            //pPLine.ToPoint = tpoint;

            //SplitLineWithPolygons(pPLine, pFLayer);
        }

        private void updaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IRasterLayer pRas1 = OpenRasterFile("E:\\TEST\\", "plane_5");
            System.Array m_array1;
            ReadPixelValues2Array(pRas1, out m_array1);
            double[,] m_double1 = new double[rpRows,rpColumns];;
            SystemArray2DoubleArray(m_array1, ref m_double1);

            IRasterLayer pRas2 = OpenRasterFile("E:\\TEST\\new\\", "Buff2Ras2");
            System.Array m_array2;
            ReadPixelValues2Array(pRas2, out m_array2);
            double[,] m_double2 = new double[rpRows,rpColumns];
            SystemArray2DoubleArray(m_array2, ref m_double2);

            //RasterAddOperation(ref m_double1, ref m_double2, RasterOperationType.Updater, 0, 0.3);
            WriteArray2RasterFile(ref m_double1, rstPixelType.PT_FLOAT, "E:\\TEST\\", "plane_7");

            m_array1 = null;
            m_array2 = null;
            m_double1 = null;
            m_double2 = null;
            GC.Collect();
        }

        //public int tmp_start_x = 0;
        //public int tmp_start_y = 0;
        //public int tmp_end_x = 0;
        //public int tmp_end_y = 0;
        //private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    OpenFeatureFile("E:\\TEST\\", "path_1Copy5.shp", tmp_end_x, tmp_end_y, cost[tmp_end_x, tmp_end_y] * pixel_depth, ref backlink);
        //}

        private void 耗费6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Type aa = typeof(Double);
            aa.GetType();

            if (default_dir == "")
            {
                MessageBox.Show("Default Directory Error!");
                return;
            }

            OutputDialog opdfg = new OutputDialog(default_dir);
            if (opdfg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlElement xNode = ReadXmlFile("config.xml", "/DialogResults/FormNode[name = 'OutputDialog']");

                start_point_name = xNode.GetElementsByTagName("start_pt").Item(0).InnerText;
                end_point_name = xNode.GetElementsByTagName("end_pt").Item(0).InnerText;
                target_layer_name = xNode.GetElementsByTagName("target").Item(0).InnerText;
                obstacle_layer_name = xNode.GetElementsByTagName("obstacle").Item(0).InnerText;

                out_path_name = xNode.GetElementsByTagName("out_path_name").Item(0).InnerText;
                out_accu_name = xNode.GetElementsByTagName("accu_name").Item(0).InnerText;
                out_parent_x_name = xNode.GetElementsByTagName("p_x_name").Item(0).InnerText;
                out_parent_y_name = xNode.GetElementsByTagName("p_y_name").Item(0).InnerText;

            }
            else
            {
                return;
            }


            int i = 0;
            for (; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name == target_layer_name)
                {
                    break;
                }
            }
            if (i == axMapControl1.LayerCount)
            {
                //axMapControl1.AddLayerFromFile(default_dir + target_layer_name);
                OpenRasterFile(default_dir , target_layer_name);
                
            }

            pBaseRasLayer = GetLayerByName(target_layer_name) as IRasterLayer;
            SetRasterLayerAsTarget(pBaseRasLayer);

            if (obstacle_layer_name != "" && Directory.Exists(default_dir + obstacle_layer_name))
            {
                /////
                IRasterLayer tmp_raster_layer2 = OpenRasterFile(default_dir, obstacle_layer_name, false);
                obstacle = ReadPixelValues2DoubleArray(tmp_raster_layer2);
                //SystemArray2DoubleArray(array2, ref obstacle);
                RasterAddOperation(ref impedance, ref obstacle, RasterOperationType.NodataMask, 1.0, 0.0);


                try
                {

                    if (!Directory.Exists(default_dir + "tmp_terrain"))
                    {
                        CreateRasterDataset_2(default_dir, "tmp_terrain", rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, ref impedance);
                    }
                    else
                    {
                        WriteArray2RasterFile(ref impedance, rstPixelType.PT_FLOAT, default_dir, "tmp_terrain");
                    }

                    IRasterLayer tmp_layer = OpenRasterFile(default_dir, "tmp_terrain");
                    //funColorForRaster_Classify(tmp_layer, 0, 0, 0, 100, 100, 100);
                    IRgbColor pFromColor = new RgbColorClass();
                    pFromColor.Red = 0;
                    pFromColor.Green = 120;
                    pFromColor.Blue = 0;
                    IRgbColor pToColor = new RgbColorClass();
                    pToColor.Red = 0;
                    pToColor.Green = 0;
                    pToColor.Blue = 120;
                    UniqueValueRenderer(tmp_layer as IRasterLayer, ref pFromColor, ref pToColor);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                obstacle = null;
                GC.Collect();
            }

            //最小时间模型
            for (i = 0; i < rpRows; i++)
            {
                for (int j = 0; j < rpColumns; j++)
                {
                    if (impedance[i, j] != float.MinValue && impedance[i, j] != 0)
                    {
                        impedance[i, j] = cell_size / impedance[i, j];
                    }
                }
            }

            //if(selected 最小时间耗费)

            OpenFeatureFile(default_dir, start_point_name);
            OpenFeatureFile(default_dir, end_point_name);

            //System.Array array;
            //IRasterLayer tmp_raster_layer = OpenRasterFile(default_dir, target_layer_name, false);
            ////IRasterProps tmp_raster_property = tmp_raster_layer as IRasterProps;
            //ReadPixelValues2Array(tmp_raster_layer, out array);
            //double[,] impedance = new double[rpRows, rpColumns];
            //SystemArray2DoubleArray(array, ref impedance);

            WriteTexts2RichTextBox(richTextBox1, "Read Target Layer...Done!");

            MyCostFunction.cycle = 1;
            DateTime t1 = DateTime.Now;
            richTextBox1.Text += "Start Time: " + t1.ToLongTimeString() + "\n";
            //GarbageRecycling();
            //VariableInitialize(target_layer);
            //DateTime t2 = DateTime.Now;
            //Console.WriteLine("Initialize TimeSpan : {0}", (t2 - t1).TotalMilliseconds / 1000);

            if (open == null)
            {
                open = new int[rpRows, rpColumns];
            }

            if (close == null)
            {
                close = new int[rpRows, rpColumns];
            }


            if (impedance == null)
            {
                MessageBox.Show("未设置障碍数据！", "运行参数错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //if (obstacle == null)
            //{
            //    MessageBox.Show("未设置地形数据！", "注意", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //else
            //{
            //    for (int i = 0; i < rpRows; i++)
            //    {
            //        for (int j = 0; j < rpColumns; j++)
            //        {
            //            if (impedance[i, j] > 0)
            //            {
            //                if (terrain[i, j] > 0)
            //                {
            //                    impedance[i, j] += terrain[i, j];
            //                }
            //                else
            //                {
            //                    impedance[i, j] = float.MinValue;
            //                    //terrain[i, j] = 1;
            //                }

            //            }
            //            else
            //            {
            //                impedance[i, j] = float.MinValue;
            //                terrain[i, j] = float.MinValue;
            //            }
            //        }
            //    }
            //    try
            //    {

            //        if (!Directory.Exists(default_dir + "tmp_terrain"))
            //        {
            //            CreateRasterDataset_2(default_dir, "tmp_terrain", rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, impedance);
            //        }
            //        else
            //        {
            //            WriteArray2RasterFile(ref impedance, rstPixelType.PT_FLOAT, default_dir, "tmp_terrain");
            //        }

            //        IRasterLayer tmp_layer = OpenRasterFile(default_dir, "tmp_terrain");
            //        funColorForRaster_Classify(tmp_layer, 0, 0, 0, 100, 100, 100);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //    Console.WriteLine("Add Operation Finished!");
            //}


            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            string filepath = default_dir;
            if (!filepath.EndsWith("\\"))
            {
                filepath += "\\";
            }

            if (impedance[source_x, source_y] <= 0 || impedance[end_x, end_y] <= 0)
            {
                MessageBox.Show("两点之间不可到达！！请重新选点！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DateTime t_start = DateTime.Now;


            MyCostFunction.CalculateSourceNeibords_6(source_x, source_y, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref parentX, ref parentY);
            while (MyCostFunction.nodeque.Count != 0)
            {
                MyCostFunction.LinkNode node = new MyCostFunction.LinkNode(MyCostFunction.nodeque[0]);
                MyCostFunction.LinkNode a = MyCostFunction.nodeque[0];
                MyCostFunction.LinkNode b = MyCostFunction.nodeque[MyCostFunction.nodeque.Count - 1];
                MyCostFunction.Swap(ref a, ref b);
                MyCostFunction.nodeque.RemoveAt(MyCostFunction.nodeque.Count - 1);
                MyCostFunction.HeapDownAdjustment(MyCostFunction.nodeque, 0, MyCostFunction.nodeque.Count);

                if (close[node.cRow, node.cColumn] == 1)
                {
                    continue;
                }
                //if (node.cRow == end_x && node.cColumn == end_y)
                //{
                //    Console.WriteLine("Find the end point.\n");
                //    break;
                //}
                MyCostFunction.CalculateNodeAccumulation_6(node.cRow, node.cColumn, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref parentX, ref parentY);
                close[node.cRow, node.cColumn] = 1;

            }
            DateTime t_end = DateTime.Now;
            richTextBox1.Text += "End Time: " + t_end.ToLongTimeString() + "\n";
            richTextBox1.Text += "total seconds(Calculation) : " + (t_end - t_start).TotalMilliseconds / 1000.0 + ".\n";
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();

            #region  abandon codes
            ////5*5更新耗费值
            //for (int i = 2; i < rpRows - 2; i++)
            //{
            //    for (int j = 2; j < rpColumns - 2; j++)
            //    {
            //        MyCostFunction.UpdateNodeValues(i, j, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY);
            //    }
            //}
            //DateTime t_end_2 = DateTime.Now;
            //Console.WriteLine("total seconds(Update) : {0}.\n", (t_end_2 - t_end).TotalMilliseconds / 1000.0);
            //IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

            //MyCostFunction.WriteData2TXTFile(ref cost, ref impedance, "cost.txt");
            //MyCostFunction.WriteData2TXTFile(ref parentX, ref impedance, "parent_x.txt");
            //MyCostFunction.WriteData2TXTFile(ref parentY, ref impedance, "parent_y.txt");

            //try
            //{
            //    CreateRasterDataset_2(filepath, out_accu_name, rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, cost);
            //    CreateRasterDataset_2(filepath, out_parent_x_name, rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, parentX);
            //    CreateRasterDataset_2(filepath, out_parent_y_name, rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, parentY);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("创建栅格失败！！！", "栅格数据错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            //terrain = null;
            //impedance = null;
            #endregion

            open = null;
            close = null;
            backlink = null;
            GC.Collect();

            try
            {
                if (!System.IO.Directory.Exists(filepath + out_accu_name))
                {
                    CreateRasterDataset_2(filepath, out_accu_name, rstPixelType.PT_FLOAT, pBaseRasLayer.Raster,ref cost);
                    WriteTexts2RichTextBox(richTextBox1, "创建栅格数据：" + filepath + out_accu_name);
                }
                WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, filepath, out_accu_name);


                if (!System.IO.Directory.Exists(filepath + out_parent_x_name))
                {
                    CreateRasterDataset_2(filepath, out_parent_x_name, rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, parentX);
                    WriteTexts2RichTextBox(richTextBox1, "创建栅格数据：" + filepath + out_parent_x_name);
                }
                WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, filepath, out_parent_x_name);


                if (!System.IO.Directory.Exists(filepath + out_parent_y_name))
                {
                    CreateRasterDataset_2(filepath, out_parent_y_name, rstPixelType.PT_FLOAT, pBaseRasLayer.Raster, parentY);
                    WriteTexts2RichTextBox(richTextBox1, "创建栅格数据：" + filepath + out_parent_y_name);
                }
                WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, filepath, out_parent_y_name);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存数据时发生意外错误：" + ex.Message, "保存数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteTexts2RichTextBox(richTextBox1, ex.Message);
            }
            //string filepath = default_dir;
            //if (!filepath.EndsWith("\\"))
            //{
            //    filepath += "\\";
            //}
            //if (Directory.Exists(filepath + out_accu_name))
            //{
            //    MessageBox.Show(filepath + out_accu_name + " 文件已存在！！");
            //    //DeleteFolder(filepath + out_accu_name);
            //}
            ////CopyRasterGridFiles(filepath + target_layer,filepath + out_accu_name);
            //WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, default_dir, out_accu_name);

            //if (Directory.Exists(filepath + out_parent_x_name))
            //{
            //    MessageBox.Show(filepath + out_parent_x_name + " 文件已存在！！");
            //    //DeleteFolder(filepath + out_parent_x_name);
            //}
            ////CopyRasterGridFiles(filepath + target_layer, filepath + out_parent_x_name);
            //WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, default_dir, out_parent_x_name);

            //if (Directory.Exists(filepath + out_parent_y_name))
            //{
            //    MessageBox.Show(filepath + out_parent_y_name + " 文件已存在！！");
            //    //DeleteFolder(filepath + out_parent_x_name);
            //}
            ////CopyRasterGridFiles(filepath + target_layer, filepath + out_parent_y_name);
            //WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, default_dir, out_parent_y_name);

            IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

            bool flag = ExportMapToImage(axMapControl1.ActiveView, DateTime.Now.ToLongDateString().Replace(':', '-') + ".jpg", 1);
            bool_recalculation = false;

            richTextBox1.Text += "计算完成(耗时：" + ((DateTime.Now - t_start).TotalMilliseconds / 1000.0).ToString() + ")！\n";
        }


        private void 耗费5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyCostFunction.cycle = 1;
            DateTime t1 = DateTime.Now;
            Console.WriteLine("Start Time : {0}", t1.ToLongTimeString());
            GarbageRecycling();
            VariableInitialize();
            DateTime t2 = DateTime.Now;
            Console.WriteLine("Initialize TimeSpan : {0}", (t2 - t1).TotalMilliseconds / 1000);

            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            if (impedance[source_x, source_y] <= 0 || impedance[source_x, source_y] <= 0)
            {
                MessageBox.Show("两点之间不可到达！！请重新选点！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MyCostFunction.CalculateSourceNeibords_5(source_x, source_y, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref parentX, ref parentY);

            DateTime t_start = DateTime.Now;
            while (MyCostFunction.nodeque.Count != 0)
            {
                MyCostFunction.LinkNode node = new MyCostFunction.LinkNode(MyCostFunction.nodeque[0]);
                MyCostFunction.LinkNode a = MyCostFunction.nodeque[0];
                MyCostFunction.LinkNode b = MyCostFunction.nodeque[MyCostFunction.nodeque.Count - 1];
                MyCostFunction.Swap(ref a, ref b);
                MyCostFunction.nodeque.RemoveAt(MyCostFunction.nodeque.Count - 1);
                MyCostFunction.HeapDownAdjustment(MyCostFunction.nodeque, 0, MyCostFunction.nodeque.Count);

                if (close[node.cRow, node.cColumn] == 1)
                {
                    continue;
                }
                //if (node.cRow == end_x && node.cColumn == end_y)
                //{
                //    Console.WriteLine("Find the end point.\n");
                //    break;
                //}
                MyCostFunction.CalculateNodeAccumulation_5(node.cRow, node.cColumn, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref parentX, ref parentY);
                close[node.cRow, node.cColumn] = 1;

            }
            DateTime t_end = DateTime.Now;
            Console.WriteLine("total seconds(Calculation) : {0}.\n", (t_end - t_start).TotalMilliseconds / 1000.0);

            //5*5更新耗费值
            //for (int i = 2; i < rpRows - 2; i++)
            //{
            //    for (int j = 2; j < rpColumns - 2; j++)
            //    {
            //        MyCostFunction.UpdateNodeValues(i, j, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY);
            //    }
            //}
            //DateTime t_end_2 = DateTime.Now;
            //Console.WriteLine("total seconds(Update) : {0}.\n", (t_end_2 - t_end).TotalMilliseconds / 1000.0);

            IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

            WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, default_dir, out_accu_name);
            WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, default_dir, out_parent_x_name);
            WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, default_dir, out_parent_y_name);

        }

        private void updateNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cost == null && parentX == null || parentY == null)
            {
                MessageBox.Show("未计算accumulation、parent_x与parent_y！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DateTime t_1 = DateTime.Now;

            for (int i = 2; i < rpRows - 2; i++)
            {
                for (int j = 2; j < rpColumns - 2; j++)
                {
                    MyCostFunction.UpdateNodeValues(i, j, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY);
                }
            }
            DateTime t_2 = DateTime.Now;
            Console.WriteLine("total seconds(Update) : {0}.\n", (t_2 - t_1).TotalMilliseconds / 1000.0);
            //WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, "E:\\TEST\\", "plane_5_dem");
            //WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, "E:\\TEST\\", "plane_5_dem_2");
            IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

            WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, default_dir, out_accu_name);
            WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, default_dir, out_parent_x_name);
            WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, default_dir, out_parent_y_name);

            MessageBox.Show("更新耗时：{0}", ((t_2 - t_1).TotalMilliseconds / 1000.0).ToString());
        }

        public static int aaa = 0;
        private void detectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            bool los = false;
            double cost = 0;
            MyCostFunction.DDA_Line_2(source_x, source_y, end_x, end_y, ref impedance, out los, out cost);
            cost = cost * cell_size;
            if (los)
            {
                MessageBox.Show("(" + source_x + "," + source_y + ") 与 (" + end_x + "," + end_y + ") 之间可通视。\n耗费值为：" + cost.ToString() + "。");
            }
            else
            {
                MessageBox.Show("(" + source_x + "," + source_y + ") 与 (" + end_x + "," + end_y + ") 之间不可通视。");
            }

        }
        private void 耗费7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            target_layer_name = "plane_5_dem_6";
            OpenRasterFile(default_dir, target_layer_name);
            MyCostFunction.cycle = 1;
            DateTime t1 = DateTime.Now;
            Console.WriteLine("Start Time: {0}", t1.ToLongTimeString());
            GarbageRecycling();
            VariableInitialize();
            DateTime t2 = DateTime.Now;
            Console.WriteLine("Initialize TimeSpan: {0}", (t2 - t1).TotalMilliseconds / 1000);

            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;


            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }
            //MessageBox.Show(source_x.ToString() + ", " + source_y.ToString());
            //MessageBox.Show(end_x.ToString() + ", " + end_y.ToString());

            if (impedance[source_x, source_y] <= 0 || impedance[source_x, source_y] <= 0)
            {
                MessageBox.Show("两点之间不可到达！！请重新选点！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MyCostFunction.CalculateSourceNeibords_7(source_x, source_y, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref parentX, ref parentY);

            DateTime t_start = DateTime.Now;
            while (MyCostFunction.queue2.Count != 0)
            {
                MyCostFunction.LinkNode node = MyCostFunction.queue2.Dequeue();
                if (close[node.cRow, node.cColumn] == 1)
                {
                    continue;
                }
                if (node.cRow == end_x && node.cColumn == end_y)
                {
                    Console.WriteLine("Find the end point.\n");
                    break;
                }
                MyCostFunction.CalculateNodeAccumulation_7(node.cRow, node.cColumn, end_x, end_y, ref open, ref close, ref impedance, ref cost, ref parentX, ref parentY);
                close[node.cRow, node.cColumn] = 1;

            }
            DateTime t_end = DateTime.Now;
            Console.WriteLine("total seconds(Calculation) : {0}.\n", (t_end - t_start).TotalMilliseconds / 1000.0);

            //5*5更新耗费值
            //for (int i = 2; i < rpRows - 2; i++)
            //{
            //    for (int j = 2; j < rpColumns - 2; j++)
            //    {
            //        MyCostFunction.UpdateNodeValues(i, j, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY);
            //    }
            //}
            //DateTime t_end_2 = DateTime.Now;
            //Console.WriteLine("total seconds(Update) : {0}.\n", (t_end_2 - t_end).TotalMilliseconds / 1000.0);
            IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

            WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, default_dir, out_accu_name);
            WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, default_dir, out_parent_x_name);
            WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, default_dir, out_parent_y_name);

        }

        private void createRasterFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[,] value = new int[rpRows, rpColumns];
            CreateRasterDataset_2(default_dir, "parent_x", ESRI.ArcGIS.Geodatabase.rstPixelType.PT_LONG, pBaseRasLayer.Raster, value);
            OpenRasterFile(default_dir, "parent_x");
            CreateRasterDataset_2(default_dir, "parent_y", ESRI.ArcGIS.Geodatabase.rstPixelType.PT_LONG, pBaseRasLayer.Raster, value);
            OpenRasterFile(default_dir, "parent_y");
        }

        private void 加载MXDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.InitialDirectory = default_dir;
            dlgOpen.Title = "Open Map Document";
            dlgOpen.Filter = "ArcMap Document(*.mxd)|*.mxd";
            string sMxdFileName = null;
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                m_strMxdFileName = dlgOpen.FileName;
                sMxdFileName = m_strMxdFileName.Substring(m_strMxdFileName.LastIndexOf('\\') + 1);
                if (axMapControl1.CheckMxFile(m_strMxdFileName))
                {
                    axMapControl1.LoadMxFile(m_strMxdFileName);
                    this.Text = origin_title + " - " + sMxdFileName;
                }
            }
            int layer_count = axMapControl1.LayerCount;
            for (int i = 0; i < layer_count; i++)
            {
                ILayer p = axMapControl1.get_Layer(i);
                richTextBox1.Text += p.Name + "\n";
            }
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

        private void 另存为MXDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.InitialDirectory = default_dir;
            dlgSave.Title = "Open Map Document";
            dlgSave.Filter = "ArcMap Document(*.mxd)|*.mxd";
            string sMxdFileName = null;
            if (dlgSave.ShowDialog() == DialogResult.OK)
            {
                m_strMxdFileName = dlgSave.FileName;
                sMxdFileName = m_strMxdFileName.Substring(m_strMxdFileName.LastIndexOf('\\') + 1);
                IMxdContents pMxdC;
                pMxdC = axMapControl1.Map as IMxdContents;
                IMapDocument pMapDocument = new MapDocumentClass();
                if (System.IO.File.Exists(m_strMxdFileName))
                {
                    File.Delete(m_strMxdFileName);
                }
                pMapDocument.New(m_strMxdFileName);
                IActiveView pActiveView = axMapControl1.Map as IActiveView;
                pMapDocument.ReplaceContents(pMxdC);
                pMapDocument.Save(true, true);
                this.Text = origin_title + " - " + sMxdFileName;
            }
        }

        private void SetAsZeroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = default_dir;

            string strFullPath = null;
            if (DialogResult.OK == fbd.ShowDialog())
            {
                strFullPath = fbd.SelectedPath;
                if (MessageBox.Show("该操作会擦除已有数据，是否继续？", "Dangerous！", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    int Index = strFullPath.LastIndexOf("\\");
                    string filePath = strFullPath.Substring(0, Index);
                    string fileName = strFullPath.Substring(Index + 1);
                    int[,] value = new int[rpRows, rpColumns];
                    try
                    {
                        WriteArray2RasterFile(ref value, rstPixelType.PT_FLOAT, filePath, fileName);
                        MessageBox.Show("( " + strFullPath + " )栅格数据清零成功！");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("操作取消。");
                }
                
                //MessageBox.Show(strFullPath);

            }

        }

        public string m_strMxdFileName = null;
        private void 保存MXDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_strMxdFileName == null)
            {
                SaveFileDialog dlgSave = new SaveFileDialog();
                dlgSave.InitialDirectory = default_dir;
                dlgSave.Title = "Open Map Document";
                dlgSave.Filter = "ArcMap Document(*.mxd)|*.mxd";

                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    m_strMxdFileName = dlgSave.FileName;
                    IMxdContents pMxdC;
                    pMxdC = axMapControl1.Map as IMxdContents;
                    IMapDocument pMapDocument = new MapDocumentClass();
                    if (System.IO.File.Exists(m_strMxdFileName))
                    {
                        File.Delete(m_strMxdFileName);
                    }
                    pMapDocument.New(m_strMxdFileName);
                    IActiveView pActiveView = axMapControl1.Map as IActiveView;
                    pMapDocument.ReplaceContents(pMxdC);
                    pMapDocument.Save(true, true);
                }
                return;
            }
            if (axMapControl1.CheckMxFile(m_strMxdFileName))
            {
                IMapDocument pMapDocument = new MapDocumentClass();
                pMapDocument.Open(m_strMxdFileName, string.Empty);

                if (pMapDocument.get_IsReadOnly(m_strMxdFileName))
                {
                    MessageBox.Show("Map document is read only!");
                    pMapDocument.Close();
                    return;
                }

                pMapDocument.ReplaceContents(axMapControl1.Map as IMxdContents);
                pMapDocument.Save(pMapDocument.UsesRelativePaths, false);
                pMapDocument.Close();
            }
        }


        private void InverseUpdateNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (cost == null && parentX == null || parentY == null)
            {
                MessageBox.Show("未计算accumulation、parent_x与parent_y！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DateTime t_1 = DateTime.Now;

            for (int i = rpRows - 2; i >= 2; i--)
            {
                for (int j = rpColumns - 2; j >= 2; j--)
                {
                    MyCostFunction.UpdateNodeValues(i, j, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY, inverse_search_flag);
                }
            }
            DateTime t_2 = DateTime.Now;
            Console.WriteLine("total seconds(Update) : {0}.\n", (t_2 - t_1).TotalMilliseconds / 1000.0);
            //WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, "E:\\TEST\\", "plane_5_dem");
            //WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, "E:\\TEST\\", "plane_5_dem_2");
            IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

            WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, default_dir, out_accu_name);
            WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, default_dir, out_parent_x_name);
            WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, default_dir, out_parent_y_name);

            MessageBox.Show("更新耗时：{0}", ((t_2 - t_1).TotalMilliseconds / 1000.0).ToString());
        }

        private void 设置为地形图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pTerrainLayer = pSelectedLayer as IRasterLayer;
            SetAsTerrainLayer(pTerrainLayer);
        }

        private void SetAsTerrainLayer(IRasterLayer pRasLayer)
        {
            if (pRasLayer is IRasterLayer)
            {
                //pBaseRasLayer = pSelectedLayer as IRasterLayer;
                terrain = null;
                GC.Collect();
                terrain = new double[rpRows, rpColumns];
                System.Array array = null;
                ReadPixelValues2Array(pRasLayer, out array);
                SystemArray2DoubleArray(array, ref terrain);

                dems = (double[,])terrain.Clone();

                MessageBox.Show("设置成功！");

                richTextBox1.Text += "设置成功！\n地形图层已更改为：" + pTerrainLayer.Name + "\n";
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("请选择栅格图层！", "源数据错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void 设置为目标图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pSelectedLayer is IRasterLayer)
            {
                //pBaseRasLayer = pSelectedLayer as IRasterLayer;
                SetRasterLayerAsTarget(pSelectedLayer as IRasterLayer);
                MessageBox.Show("设置成功！");

                richTextBox1.Text += "设置成功！\n目标图层已更改为：" + pSelectedLayer.Name + "\n";
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("请选择栅格图层！", "源数据错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }

        }

        private void LOSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            //if (aaa == 0)
            //{
            //    VariableInitialize(target_layer);
            //}
            //aaa++;

            Lineofsight losdlg = new Lineofsight(target_layer_name, source_x, source_y, end_x, end_y);
            losdlg.Owner = this;
            losdlg.ShowDialog();
        }

        public static bool inverse_search_flag = false;
        public static bool order_search_flag = false;
        public static bool circle_search_flag = false;
        public static int circulation_start_num = 1;
        public static int circulation_end_num = 5;
        private void heapUpdateNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (impedance == null)
            {
                MessageBox.Show("障碍数据丢失！请先进行最短路径计算！", "运行参数错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (cost == null || parentX == null || parentY == null)
            {
                WriteTexts2RichTextBox(richTextBox1, "未计算accumulation、parent_x与parent_y！");
                MessageBox.Show("未计算accumulation、parent_x与parent_y！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            
            string t_str = DateTime.Now.ToLongDateString() + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
            StreamWriter sw = new StreamWriter("recorder_"+t_str+".txt"); //运行日志的写入流

            UpdateNodesDialog undlg = new UpdateNodesDialog();
            undlg.Owner = this;
            
            if (undlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //System.Array ArrayData;
                //ReadPixelValues2Array(pBaseRasLayer, out ArrayData);
                //SystemArray2DoubleArray(ArrayData, ref impedance);
                //ArrayData = null;

                string filename = "snapshot_";
                IActiveView pActiveView = axMapControl1.ActiveView;

                for (int k = circulation_start_num; k <= circulation_end_num; k++)
                {
                    if (order_search_flag)
                    {
                        MyCostFunction.nodeque.Clear();
                        MyCostFunction.cycle = k;
                        MyCostFunction.ticks = 0;

                        MyCostFunction.UpdateNodesViaHeapFun(source_x, source_y, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY, false, null, richTextBox1);
                        IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

                        bool flag = ExportMapToImage(pActiveView, filename + k + t_str + ".jpg", 1);
                    }

                    if (inverse_search_flag)
                    {
                        MyCostFunction.nodeque.Clear();
                        MyCostFunction.cycle = k;
                        MyCostFunction.ticks = 0;

                        MyCostFunction.UpdateNodesViaHeapFun(source_x, source_y, end_x, end_y, ref impedance, ref cost, ref parentX, ref parentY, true, null, richTextBox1);
                        IFeatureLayer pFLayer = OpenFeatureFile_5(default_dir, out_path_name, end_x, end_y, cost[end_x, end_y] * cell_size, ref parentX, ref parentY);

                        bool flag = ExportMapToImage(pActiveView, filename + "invert_" + k + t_str + ".jpg", 1);
                    }
                    try
                    {
                        WriteArray2RasterFile(ref cost, rstPixelType.PT_FLOAT, default_dir, out_accu_name);
                        WriteArray2RasterFile(ref parentX, rstPixelType.PT_FLOAT, default_dir, out_parent_x_name);
                        WriteArray2RasterFile(ref parentY, rstPixelType.PT_FLOAT, default_dir, out_parent_y_name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("写入数据错误！！" + ex.Message);
                        sw.WriteLine("写入数据错误！！" + ex.Message);
                    }
                }
                sw.Close();
            }
        }

        private void 还原现场ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (parentX == null)
            //{
            //    parentX = new int[rpRows, rpColumns];
            //}
            //if (parentY == null)
            //{
            //    parentY = new int[rpRows, rpColumns];
            //}
            //if (cost == null)
            //{
            //    cost = new double[rpRows, rpColumns];
            //}
            SaveTempDataDialog stddlg = new SaveTempDataDialog(default_dir);
            stddlg.StartPosition = FormStartPosition.CenterParent;
            stddlg.Owner = this;
            stddlg.ShowDialog();
            stddlg = null;
        }

        //srcdir-源文件路径desdir-目标路径  result-运行结果返回值True或False  Time-使用旧文件名+当前时间重命名文件夹
        public bool CopyRasterGridFiles(string srcdir = "", string desdir = "")
        {
            try
            {
                string fileName = "";
                DeleteFolder(desdir.Trim());
                Directory.CreateDirectory(desdir.Trim());
                string[] filenames = Directory.GetFileSystemEntries(srcdir);
                foreach (string file in filenames)
                {
                    fileName = file.Substring(file.LastIndexOf("\\") + 1);
                    File.Copy(file, desdir + "\\" + fileName, true);
                }
                return true;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void 地形因子设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int layer_count = axMapControl1.LayerCount;
            List<string> _list = new List<string>();
            for (int i = 0; i < layer_count; i++)
            {
                ILayer p = axMapControl1.get_Layer(i);
                if (p is IRasterLayer)
                {
                    _list.Add(p.Name);
                }
            }
            layer_count = _list.Count;
            string[] list = new string[layer_count];
            for (int i = 0; i < layer_count; i++)
            {
                ILayer p = axMapControl1.get_Layer(i);
                if (p is IRasterLayer)
                {
                    list[i] = _list[i];
                }
            }
            //if (terrain == null)
            //{
            //    MessageBox.Show("未设置地形图层！", "错误！", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            //    return;
            //}

            PlaneReclassify prcls = new PlaneReclassify(this, list, ref terrain);
            prcls.ShowDialog();
            //if (prcls.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    //terrain = prcls.output_reclassify();
            //}

        }


        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveFileDialog sdlg = new SaveFileDialog();
                sdlg.InitialDirectory = default_dir;
                sdlg.FileName = "日志输出-" + DateTime.Now.ToLongDateString() + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".txt";
                sdlg.Filter = "文本文件|*.txt";
                sdlg.DefaultExt = "txt";
                if (sdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StreamWriter sw = new StreamWriter(sdlg.FileName);
                    sw.Write(richTextBox1.Text.Replace("\n","\r\n"));
                    sw.Close();
                }
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomHelp ch = new CustomHelp();
            ch.ShowDialog();
        }

        private void 设置工作路径ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = default_dir;

            if (DialogResult.OK == fbd.ShowDialog())
            {
                default_dir = fbd.SelectedPath + "\\";
                WriteTexts2RichTextBox(richTextBox1, "当前工作路径为："+default_dir);
                MessageBox.Show("设置成功！");
            }
        }

        private void 初始化设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Initialization();
        }

        private void CalcuSlopeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] list = GetRasterLayerList();
            SlopeCalculation sc = new SlopeCalculation(this, list);
            sc.ShowDialog();
        }

        private void CalcuSlopeToolStripMenuItem_Click_old(object sender, EventArgs e)
        {
            string[] list = GetRasterLayerList();

            if (terrain == null)
            {
                MessageBox.Show("未设置地形图层！", "错误！", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return;
            }
            SlopeCalculation sc = new SlopeCalculation(this, list, ref terrain);
            sc.ShowDialog();
            if (sc.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //if (terrain == null)
                //{
                //    MessageBox.Show("未设置DEM地形图层数据！！");
                //    return;
                //}

                //terrain = sc.get_slopes_arrays();
            }
            //ReadXmlFile("config.xml", "/DialogResults/FormNode[name = 'ParaDialog']");
        }

        public string[] GetRasterLayerList()
        {
            int layer_count = axMapControl1.LayerCount;
            string[] list = new string[layer_count];
            for (int i = 0; i < layer_count; i++)
            {
                ILayer p = axMapControl1.get_Layer(i);
                if (p is IRasterLayer)
                {
                    list[i] = p.Name;
                }
            }
            return list;
        }

        public void ExtractDEMs(int source_x, int source_y, int end_x, int end_y, ref double[,] space, ref List<double> extract)
        {
            
            int x1 = source_x, y1 = source_y, x2 = end_x, y2 = end_y;
            double k, dx, dy, x, y, xend, yend;

            dx = x2 - x1;
            dy = y2 - y1;

            if (source_x == end_x && source_y == end_y)
            {
                extract.Add(space[source_x, source_y]);
                return;
            }
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                k = dy / dx;
                if (dx > 0)
                {
                    x = x1;
                    y = y1;
                    xend = x2;
                }
                else
                {
                    x = x2;
                    y = y2;
                    xend = x1;
                }
                while (x <= xend)
                {
                    if (y < 0 || y > rpColumns - 1)
                    {
                        Console.WriteLine("Out Of Bounds! ");
                        break;
                    }
                    //SetDevicePixel((int)x, ROUND_INT(y));
                    if (space[(int)x, (int)Math.Floor(y)] < 0)
                    {
                        extract.Add(0);
                    }
                    else
                    {
                        extract.Add(space[(int)x, (int)Math.Floor(y)]);
                    }
                    y = y + k;
                    x = x + 1;
                }
            }
            else
            {
                k = dx / dy;
                if (dy > 0)
                {
                    x = x1;
                    y = y1;
                    yend = y2;
                }
                else
                {
                    x = x2;
                    y = y2;
                    yend = y1;
                }
                while (y <= yend)
                {
                    if (x < 0 || x > rpRows - 1)
                    {
                        Console.WriteLine("Out Of Bounds! ");
                        break;
                    }
                    if (space[(int)Math.Floor(x), (int)y] < 0)
                    {
                        extract.Add(0);
                    }
                    else
                    {
                        extract.Add(space[(int)Math.Floor(x), (int)y]);
                    }
                    x = x + k;
                    y = y + 1;
                }
            }
        }

        private void 直线剖面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pTerrainLayer == null)
            {
                if (MessageBox.Show("未定义地形数据！！是否现在打开？", "参数错误！", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.SelectedPath = default_dir;
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string fullpath = fbd.SelectedPath;
                        int index = fullpath.LastIndexOf('\\');
                        string filePath = fullpath.Substring(0, index);
                        string fileName = fullpath.Substring(index + 1);
                        pTerrainLayer = OpenRasterFile(filePath, fileName);
                        
                    }
                    else
                    {
                        return;
                    }
                }

            }
            if (dems == null)
            {

                dems = ReadPixelValues2DoubleArray(pTerrainLayer);

            }
            

            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            List<double> extract_dem = new List<double>(1000);
            //extract = new double[(int)(Math.Abs(dx) >= Math.Abs(dy) ? Math.Abs(dx) : Math.Abs(dy)) + 1];
            ExtractDEMs(source_x, source_y, end_x, end_y, ref dems, ref extract_dem);

            profile_line = new double[extract_dem.Count];
            for (int i = 0; i < extract_dem.Count; i++)
            {
                profile_line[i] = extract_dem[i];
            }
            ProfileDrawer pd = new ProfileDrawer(ref profile_line);
            pd.ShowDialog();
            //Thread t = new Thread(new ParameterizedThreadStart(ShowProfileWin));
            //t.Start();
        }

        private void 最短路径剖面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bool_recalculation)
            {
                MessageBox.Show("未计算最短路径！","参数错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            source_x = sRow;
            source_y = sColumn;
            end_x = eRow;
            end_y = eColumn;

            if (readstartpointfromfile)
            {
                IPoint source_pt = ConvertPointFeatureFile2XYCoords(default_dir, start_point_name);
                source_x = (int)source_pt.Y;
                source_y = (int)source_pt.X;
            }
            if (readendpointfromfile)
            {
                IPoint dest_pt = ConvertPointFeatureFile2XYCoords(default_dir, end_point_name);
                end_x = (int)dest_pt.Y;
                end_y = (int)dest_pt.X;
            }

            List<double> values = new List<double>(1000);

            IPoint pt = new PointClass();
            int x = end_x;
            int y = end_y;
            values.Add(impedance[end_x, end_y]);

            int x0 = parentX[x, y];
            int y0 = parentY[x, y];

            while (x0 > 0 && y0 > 0)
            {
                ExtractDEMs(x, y, x0, y0, ref impedance, ref values);
                x = x0;
                y = y0;

                x0 = parentX[x, y];
                y0 = parentY[x, y];
            }

            Console.WriteLine(values.Count);

            profile_line = new double[values.Count];
            for (int i = values.Count - 1; i >= 0; i--)
            {
                profile_line[i] = values[i];
            }

            ProfileDrawer pd = new ProfileDrawer(ref profile_line);
            pd.ShowDialog();

        }

        public void ShowProfileWin(object a)
        {
            ProfileDrawer pd = new ProfileDrawer(ref profile_line);
            pd.ShowDialog();
        }

        private void 清除程序缓存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GarbageRecycling();
        }


    }
}
