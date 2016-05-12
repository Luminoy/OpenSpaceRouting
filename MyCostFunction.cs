using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using System.IO;

namespace OpenSpaceRouting
{
    class MyCostFunction
    {
        public class LinkNode
        {

            public double accumulate;
            public int cRow, cColumn;
            public LinkNode()
            {
                this.accumulate = -1;
                this.cColumn = -1;
                this.cRow = -1;
            }
            public LinkNode(LinkNode a)
            {
                this.accumulate = a.accumulate;
                this.cColumn = a.cColumn;
                this.cRow = a.cRow;
            }
            public static bool operator > (LinkNode a, LinkNode b)
            {
                return a.accumulate > b.accumulate;
            }
            public static bool operator < (LinkNode a, LinkNode b)
            {
                return a.accumulate < b.accumulate;
            }
            public static bool operator == (LinkNode a, LinkNode b)
            {
                return a.accumulate == b.accumulate;
            }
            public static bool operator != (LinkNode a, LinkNode b)
            {
                return a.accumulate != b.accumulate;
            }
        };
        public static int array_width = 0;
        public static int array_height = 0;
        const double NoData = float.MinValue;
        
        public static List<LinkNode> nodeque = new List<LinkNode>();


        //public static StreamWriter sw_cost = null;
        //public static StreamWriter sw_px = null;
        //public static StreamWriter sw_py = null;


        #region 将二叉小堆应用到A*
        /// <summary>
        /// 堆排序主函数
        /// </summary>
        /// <param name="array">传递待排数组名</param>
        public static void HeapSortFunction(List<LinkNode> nodeque)
        {
            try
            {
                BuildMaxHeap(nodeque);    //创建大顶推（初始状态看做：整体无序）
                for (int i = nodeque.Count - 1; i > 0; i--)
                {
                    LinkNode a = nodeque[0];
                    LinkNode b = nodeque[i];
                    Swap(ref a, ref b); //将堆顶元素依次与无序区的最后一位交换（使堆顶元素进入有序区）
                    MaxHeapify(nodeque, 0, i); //重新将无序区调整为大顶堆
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:"+ex.Message);
            }
        }


        ///<summary>
        /// 创建大顶推（根节点大于左右子节点）
        ///</summary>
        ///<param name="array">待排数组</param>
        public static void BuildMaxHeap(List<LinkNode> nodeque)
        {
            try
            {
                //根据大顶堆的性质可知：数组的前半段的元素为根节点，其余元素都为叶节点
                for (int i = nodeque.Count / 2 - 1; i >= 0; i--) //从最底层的最后一个根节点开始进行大顶推的调整
                {
                    MaxHeapify(nodeque, i, nodeque.Count); //调整大顶堆
                }
            }
            catch (Exception ex)
            { }
        }

        ///<summary>
        /// 大顶推的调整过程
        ///</summary>
        ///<param name="array">待调整的数组</param>
        ///<param name="currentIndex">待调整元素在数组中的位置（即：根节点）</param>
        ///<param name="heapSize">堆中所有元素的个数</param>
        public static void MaxHeapify(List<LinkNode> nodeque, int currentIndex, int heapSize)
        {
            try
            {
                int left = 2 * currentIndex + 1;    //左子节点在数组中的位置
                int right = 2 * currentIndex + 2;   //右子节点在数组中的位置
                int large = currentIndex;   //记录此根节点、左子节点、右子节点 三者中最大值的位置

                if (left < heapSize && nodeque[left] > nodeque[large])  //与左子节点进行比较
                {
                    large = left;
                }
                if (right < heapSize && nodeque[right] > nodeque[large])    //与右子节点进行比较
                {
                    large = right;
                }
                if (currentIndex != large)  //如果 currentIndex != large 则表明 large 发生变化（即：左右子节点中有大于根节点的情况）
                {
                    //LinkNode t = nodeque[currentIndex];
                    //nodeque[currentIndex].accumulate = nodeque[large].accumulate;
                    LinkNode a = nodeque[currentIndex];
                    LinkNode b = nodeque[large];
                    Swap(ref a, ref b);    //将左右节点中的大者与根节点进行交换（即：实现局部大顶堆）
                    MaxHeapify(nodeque, large, heapSize); //以上次调整动作的large位置（为此次调整的根节点位置），进行递归调整
                }
            }
            catch (Exception ex)
            { }
        }

        public static void HeapDownAdjustment(List<LinkNode> nodeque, int currentIndex, int heapSize)
        {
            try
            {
                int left = 2 * currentIndex + 1;    //左子节点在数组中的位置
                int right = 2 * currentIndex + 2;   //右子节点在数组中的位置
                int small = currentIndex;   //记录此根节点、左子节点、右子节点 三者中最小值的位置

                if (left < heapSize && nodeque[left] < nodeque[small])  //与左子节点进行比较
                {
                    small = left;
                }
                if (right < heapSize && nodeque[right] < nodeque[small])    //与右子节点进行比较
                {
                    small = right;
                }
                if (currentIndex != small)  //如果 currentIndex != small 则表明 small 发生变化（即：左右子节点中有大于根节点的情况）
                {
                    LinkNode a = nodeque[currentIndex];
                    LinkNode b = nodeque[small];
                    Swap(ref a, ref b);    //将左右节点中的大者与根节点进行交换（即：实现局部大顶堆）
                    HeapDownAdjustment(nodeque, small, heapSize); //以上次调整动作的large位置（为此次调整的根节点位置），进行递归调整
                }
            }
            catch (Exception ex)
            { }
        }

        public static void HeapUpAdjustment(List<LinkNode> nodeque, int currentIndex, int heapSize)
        {
            try
            {
                int parent = (currentIndex - 1) / 2;   //父节点在数组中的位置
                int left = parent * 2 + 1;
                int right = parent * 2 + 2;
                int small = parent;   //记录此根节点、左子节点、右子节点 三者中最小值的位置

                if (left < heapSize && nodeque[left] < nodeque[small])  //与左子节点进行比较
                {
                    small = left;
                }
                if (right < heapSize && nodeque[right] < nodeque[small])    //与右子节点进行比较
                {
                    small = right;
                }
                if (parent != small)  //如果 currentIndex != small 则表明 small 发生变化（即：左右子节点中有大于根节点的情况）
                {
                    LinkNode a = nodeque[parent];
                    LinkNode b = nodeque[small];
                    Swap(ref a, ref b);    //将左右节点中的大者与根节点进行交换（即：实现局部大顶堆）
                    HeapUpAdjustment(nodeque, parent, heapSize); //以上次调整动作的large位置（为此次调整的根节点位置），进行递归调整
                    if (small == 0)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            { }
        }
        ///<summary>
        /// 交换函数
        ///</summary>
        ///<param name="a">元素a</param>
        ///<param name="b">元素b</param>
        public static void Swap(ref LinkNode a, ref LinkNode b)
        {
            LinkNode temp = new LinkNode(a);

            a.accumulate = b.accumulate;
            a.cColumn = b.cColumn;
            a.cRow = b.cRow;

            b.accumulate = temp.accumulate;
            b.cColumn = temp.cColumn;
            b.cRow = temp.cRow;
        }

        #endregion

        #region BackLink方向查找表(表示src点在center点的方位)
        public static int BackLinkDirection(int center_x, int center_y, int src_x, int src_y)
        {
            int x = src_x - center_x;
            int y = src_y - center_y;
            if (x == -1)
                return x + y + 8;
            else if (x == 1)
                return x - y + 2;
            else
            {
                if (y == -1)
                    return 5;
                else if (y == 1)
                    return 1;
                else
                    return 0;
            }
        }
        //5*5以上通用型
        public static int BackLinkDirection_2(int center_r, int center_c, int new_r, int new_c)
        {
            int dr = new_r - center_r; //delta row
            int dc = new_c - center_c; //delta column
            if (dr > 4)
            {
                MessageBox.Show("ERRORRRRRR");
            }
            int round = Math.Max(Math.Abs(dr), Math.Abs(dc)); 
            int round_max = (2 * round + 1) * (2 * round + 1) - 1;
            if (dr == -round) //up of the round
            {
                return round_max + (dr + dc);
            }
            else if (dc == -round) //left of the round
            {
                return (round_max - 2 * round) + (dc - dr);
            }
            else if (dr == round) //down of the round
            {
                return (round_max - (2 + 2) * round) - (dr + dc);
            }
            else  //right of the round
            {
                return (round_max - (2 + 2 + 2) * round) - (dc - dr);
            }
        }
        #endregion

        #region 方向值转换为栅格行列号增量
        public static void BackLinkDirection2RowColumn(int direction, out int row, out int column)
        {
            switch (direction)
            {
                case 0:
                    row = 0;
                    column = 0;
                    break;
                case 1:
                    row = 0;
                    column = 1;
                    break;
                case 2:
                    row = 1;
                    column = 1;
                    break;
                case 3:
                    row = 1;
                    column = 0;
                    break;
                case 4:
                    row = 1;
                    column = -1;
                    break;
                case 5:
                    row = 0;
                    column = -1;
                    break;
                case 6:
                    row = -1;
                    column = -1;
                    break;
                case 7:
                    row = -1;
                    column = 0;
                    break;
                case 8:
                    row = -1;
                    column = 1;
                    break;
                default:
                    row = 0;
                    column = 0;
                    return;
            }
        }

        //case列举的方向值与行列号的转换表
        public static void BackLinkDirection2RowColumn_1(int direction, out int row, out int column)
        {
            switch (direction)
            {
                case 0:
                    row = 0;
                    column = 0;
                    break;
                case 1:
                    row = 0;
                    column = 1;
                    break;
                case 2:
                    row = 1;
                    column = 1;
                    break;
                case 3:
                    row = 1;
                    column = 0;
                    break;
                case 4:
                    row = 1;
                    column = -1;
                    break;
                case 5:
                    row = 0;
                    column = -1;
                    break;
                case 6:
                    row = -1;
                    column = -1;
                    break;
                case 7:
                    row = -1;
                    column = 0;
                    break;
                case 8:
                    row = -1;
                    column = 1;
                    break;
                case 9:
                    row = -1;
                    column = 2;
                    break;
                case 10:
                    row = 0;
                    column = 2;
                    break;
                case 11:
                    row = 1;
                    column = 2;
                    break;
                case 12:
                    row = 2;
                    column = 2;
                    break;
                case 13:
                    row = 2;
                    column = 1;
                    break;
                case 14:
                    row = 2;
                    column = 0;
                    break;
                case 15:
                    row = 0;
                    column = 2;
                    break;
                default:
                    row = 0;
                    column = 0;
                    return;
            }
        }
        
        //5*5以上的情况
        public static void BackLinkDirection2RowColumn_2(int direction, out int row, out int column)
        {
            //int round = (int)(Math.Sqrt((double)direction) + 1) / 2 ;
            int round = 0;
            if (direction > 24)
            {
                round = 3;
            }
            else if (direction > 8)
            {
                round = 2;
            }
            else if (direction > 0)
            {
                round = 1;
            }
            else
                round = 0;
            int round_max = (2 * round + 1) * (2 * round + 1) - 1;
            int round_min = (2 * (round - 1) + 1) * (2 * (round - 1) + 1);
            if (direction >= (round_max - 2 * round)) //up of the round
            {
                row = -round;
                column = round - (round_max - direction);
                return;
            }
            if (direction >= (round_max - (2 + 2) * round)) //left of the round
            {
                row = (round_max - 2 * round)- direction - round;
                column = -round;
                return;
            }
            if (direction >= (round_max - (2 + 2 + 2) * round)) //down of the round
            {
                column = (round_max - (2 + 2) * round) - direction - round;
                row = round;
                return;
            }
            if (direction > (round_max - (2 + 2 + 2 + 2) * round))  //right of the round
            {
                row = round - ((round_max - (2 + 2 + 2) * round) - direction);
                column = round;
                return;
            }
            //over.
            if (direction == -1)
            {
                Console.WriteLine("Error!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
            row = 0;
            column = 0;
        }
        #endregion

        #region 源像元的邻居像元耗费累积量计算
        public static void CalculateSourceNeibords(int center_x, int center_y, ref double[,] src, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            //double [,] window = {{-1,-1,-1},{-1,-1,-1},{-1,-1,-1}};
            backlink[center_x, center_y] = 0;
            double[] factor = { 1, 1.414 };
            double accu = 0.0;
            close[center_x, center_y] = 1;
            for (int i = -1; i <= 1; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -1; j <= 1; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if (src[index_row, index_col] == 1 || impedance[index_row, index_col] == NoData)
                    {
                        continue;
                    }
                    
                    accu = factor[Math.Abs(i * j)] * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                    int node_exist = FindNodeByXY(index_row, index_col);
                    if (node_exist == 0)
                    {
                        backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                        LinkNode item = new LinkNode();
                        item.accumulate = accu;
                        item.cRow = index_row;
                        item.cColumn = index_col;
                        nodeque.Add(item);//此处应有插入排序
                        cost[index_row, index_col] = accu;
                        open[index_row, index_col] = 1;
                    }
                    else
                    {
                        LinkNode node = nodeque.ElementAt(node_exist);
                        if (node.accumulate > accu)
                        {
                            backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                            node.accumulate = accu;
                            cost[index_row, index_col] = accu;
                        }
                    }
                    
                    //window[i + 1, j + 1] = impedance[index_row, index_col];
                }
            }
        }

        public static void CalculateSourceNeibords_2(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            //double [,] window = {{-1,-1,-1},{-1,-1,-1},{-1,-1,-1}};
            backlink[center_x, center_y] = 0;
            double[] factor = { 1, Math.Sqrt(2.0) };
            double accu = 0.0;
            close[center_x, center_y] = 1;
            for (int i = -1; i <= 1; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -1; j <= 1; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if (impedance[index_row, index_col] <= 0)
                    {
                        continue;
                    }
                    double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                    accu = distance + factor[Math.Abs(i * j)] * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;

                    int node_exist = FindNodeByXY(index_row, index_col);
                    if (node_exist == 0)
                    {

                        backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y); //换成_2的函数
                        LinkNode item = new LinkNode();
                        item.accumulate = accu;
                        item.cRow = index_row;
                        item.cColumn = index_col;
                        nodeque.Add(item);//此处应有插入排序

                        cost[index_row, index_col] = accu;
                        open[index_row, index_col] = 1;
                    }
                    else  //源像元只有一个的时候，以下代码不会执行
                    {

                        LinkNode node = nodeque.ElementAt(node_exist);
                        if (node.accumulate > accu)
                        {
                            backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                            node.accumulate = accu;
                            cost[index_row, index_col] = accu;
                        }
                    }

                    //window[i + 1, j + 1] = impedance[index_row, index_col];
                }
            }
        }

        //5*5的栅格掩膜范围
        public static int cycle = 2;
        public static void CalculateSourceNeibords_3(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            backlink[center_x, center_y] = 0;
            double accu = 0.0;
            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }

                    double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2)); //启发值
                    accu = distance + Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2; //

                    int node_exist = FindNodeByXY(index_row, index_col);
                    if (node_exist == 0)
                    {
                        
                        backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y); //换成_2的函数
                        LinkNode item = new LinkNode(); 
                        item.accumulate = accu;
                        item.cRow = index_row;
                        item.cColumn = index_col;
                        nodeque.Add(item);//此处应有插入排序
                        cost[index_row, index_col] = accu;
                        open[index_row, index_col] = 1;
                    }
                    else  //源像元只有一个的时候，以下代码不会执行
                    {
                        
                        LinkNode node = nodeque.ElementAt(node_exist);
                        if (node.accumulate > accu)
                        {
                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                            node.accumulate = accu;
                            cost[index_row, index_col] = accu;
                        }
                    }

                    //window[i + 1, j + 1] = impedance[index_row, index_col];
                }
            }
        }

        public static void CalculateSourceNeibords_4(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX,ref int[,] parentY)
        {
            parentX[center_x, center_y] = 0;
            parentY[center_x, center_y] = 0;

            double accu = 0.0;
            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    accu = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    parentX[index_row, index_col] = center_x;
                    parentY[index_row, index_col] = center_y;

                    LinkNode item = new LinkNode();
                    item.accumulate = accu;
                    item.cRow = index_row;
                    item.cColumn = index_col;
                    nodeque.Add(item);
                    cost[index_row, index_col] = accu;
                    open[index_row, index_col] = 1;
                }
            }
            HeapSortFunction(nodeque);
        }

        public static void CalculateSourceNeibords_5(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            backlink[center_x, center_y] = 0;

            double accu = 0.0;
            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    double new_cost = 0;
                    bool los_s_s1;
                    DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out new_cost);

                    
                    

                    LinkNode item = new LinkNode();
                    item.accumulate = accu;
                    item.cRow = index_row;
                    item.cColumn = index_col;
                    nodeque.Add(item);

                    cost[index_row, index_col] = accu;
                    backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y); 
                    open[index_row, index_col] = 1;
                }
            }
            HeapSortFunction(nodeque);
        }
        
        public static void CalculateSourceNeibords_5(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY)
        {
            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    //double heuri_value = HeuristicValue(index_row, index_col, dest_x, dest_y);
                    double accu = 0;
                    bool los_s_s1;
                    DDA_Line(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out accu);
                    //dis_s_s1 += heuri_value;

                    LinkNode item = new LinkNode();
                    item.accumulate = dis_s_s1;
                    item.cRow = index_row;
                    item.cColumn = index_col;
                    nodeque.Add(item);

                    parentX[index_row, index_col] = center_x;
                    parentY[index_row, index_col] = center_y;
                    cost[index_row, index_col] = dis_s_s1;
                    open[index_row, index_col] = 1;
                }
            }
            parentX[center_x, center_y] = 0;
            parentY[center_x, center_y] = 0;

            HeapSortFunction(nodeque);
        }
        
        public static void CalculateSourceNeibords_6(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            backlink[center_x, center_y] = 0;

            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    double dis_s_s1 = 0;
                    bool los_s_s1;
                    DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                    LinkNode item = new LinkNode();
                    item.accumulate = dis_s_s1;
                    item.cRow = index_row;
                    item.cColumn = index_col;
                    nodeque.Add(item);

                    cost[index_row, index_col] = dis_s_s1;
                    backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                    open[index_row, index_col] = 1;
                }
            }
            HeapSortFunction(nodeque);
        }
        
        public static void CalculateSourceNeibords_6(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY)
        {
            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double heuri_value = HeuristicValue(index_row, index_col, dest_x, dest_y);
                    double dis_s_s1 = 0;
                    bool los_s_s1;
                    DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);
                    //dis_s_s1 += heuri_value;

                    LinkNode item = new LinkNode();
                    item.accumulate = dis_s_s1;
                    item.cRow = index_row;
                    item.cColumn = index_col;
                    nodeque.Add(item);

                    parentX[index_row, index_col] = center_x;
                    parentY[index_row, index_col] = center_y;
                    cost[index_row, index_col] = dis_s_s1;
                    open[index_row, index_col] = 1;
                }
            }
            parentX[center_x, center_y] = 0;
            parentY[center_x, center_y] = 0;

            HeapSortFunction(nodeque);
        }

        public static Queue<LinkNode> queue2 = new Queue<LinkNode>(1000);
        public static void CalculateSourceNeibords_7(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY)
        {
            close[center_x, center_y] = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double heuri_value = HeuristicValue(index_row, index_col, dest_x, dest_y);
                    double dis_s_s1 = 0;
                    bool los_s_s1;
                    DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);
                    //dis_s_s1 += heuri_value;

                    LinkNode item = new LinkNode();
                    item.accumulate = dis_s_s1;
                    item.cRow = index_row;
                    item.cColumn = index_col;
                    queue2.Enqueue(item);

                    parentX[index_row, index_col] = center_x;
                    parentY[index_row, index_col] = center_y;
                    cost[index_row, index_col] = dis_s_s1;
                    open[index_row, index_col] = 1;
                }
            }
            parentX[center_x, center_y] = 0;
            parentY[center_x, center_y] = 0;
        }
        
        #endregion

        public static double HeuristicValue(int curr_x, int curr_y, int dest_x, int dest_y)
        {
            return Math.Sqrt((curr_x - dest_x) * (curr_x - dest_x) + (curr_y - dest_y) * (curr_y - dest_y));
        }
        /// <summary>
        /// Line of Sight
        /// </summary>
        /// <param name="curr_x"></param>
        /// <param name="curr_y"></param>
        /// <param name="parent_x"></param>
        /// <param name="parent_y"></param>
        /// <param name="space"></param>
        /// <returns>true or false</returns>
        public static void LineofSight(int curr_x, int curr_y, int parent_x, int parent_y, ref double[,] space, out bool los, out double new_cost)
        {
            new_cost = 0;
            if (curr_x == parent_x && curr_y == parent_y)
            {
                los = false;
                return;
            }

            int x0 = curr_x;
            int y0 = curr_y;
            int x1 = parent_x;
            int y1 = parent_y;
            int dr = parent_x - curr_x;
            int dc = parent_y - curr_y;
            int sign_x = 1;
            int sign_y = 1;
            int f = 0;

            if (dc < 0)
            {
                dc = -dc;
                sign_y = -1;
            }
            if (dr < 0)
            {
                dr = -dr;
                sign_x = -1;
            }
            if (dr >= dc)
            {
                while (x0 != x1)
                {
                    f = f + dc;
                    if (f >= dr)
                    {
                        if (space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                        {
                            los = false;
                            return;
                        }
                        y0 = y0 + sign_y;
                        f = f - dr;
                    }
                    if (f != 0 && space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                    {
                        los = false;
                        return;
                    }
                    if (dc == 0 && space[x0 + ((sign_x - 1) / 2), y0] <= 0 && space[x0 + ((sign_x - 1) / 2), y0 - 1] <= 0)
                    {
                        los = false;
                        return;
                    }
                    new_cost += space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)];
                    x0 = x0 + sign_x;
                }
            }
            else
            {
                while (y0 != y1)
                {
                    
                    f = f + dr;
                    if (f >= dc)
                    {
                        if (space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                        {
                            los = false;
                            return;
                        }
                        x0 = x0 + sign_x;
                        f = f - dc;
                    }
                    if (f != 0 && space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                    {
                        los = false;
                        return;
                    }
                    if (dr == 0 && space[x0, y0 + ((sign_y - 1) / 2)] <= 0 && space[x0 - 1, y0 + ((sign_y - 1) / 2)] <= 0)
                    {
                        los = false;
                        return;
                    }
                    new_cost += space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)];
                    y0 = y0 + sign_y;
                }
            }
            los = true;
        }

        /// <summary>
        /// 计算一点curr与另一点parent之间是否通视，如果通视，返回los为true
        /// </summary>
        /// <param name="curr_x"></param>
        /// <param name="curr_y"></param>
        /// <param name="parent_x"></param>
        /// <param name="parent_y"></param>
        /// <param name="space">阻抗栅格</param>
        /// <param name="los">通视与否</param>
        /// <param name="new_cost">加权路径长度(未使用)</param>
        public static void DDA_Line(int curr_x, int curr_y, int parent_x, int parent_y, ref double[,] space,out bool los, out double new_cost)
        {
            int x1 = curr_x, y1 = curr_y, x2 = parent_x, y2 = parent_y;
            double k, dx, dy, x, y, xend, yend;

            new_cost = 0;
            dx = x2 - x1;
            dy = y2 - y1;
            if (curr_x == parent_x && curr_y == parent_y)
            {
                los = false;
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
                if (space[(int)x, (int)y] < 0)
                {
                    los = false;
                    return;
                }
                while (x < xend)
                {
                    y = y + k;
                    x = x + 1;
                    //SetDevicePixel((int)x, ROUND_INT(y));
                    if (space[(int)x, (int)Math.Floor(y)] < 0)
                    {
                        los = false;
                        return;
                    }
                    else
                    {
                        int yf0 = (int)Math.Floor(y - k); //前一个点的y值
                        int yf1 = (int)Math.Floor(y);     //当前点的y值
                        new_cost += Math.Sqrt(1 + (yf0 - yf1) * (yf0 - yf1)) * (space[(int)(x - 1), yf0] + space[(int)x, yf1]) / 2;
                    }
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
                if (space[(int)x, (int)y] < 0)
                {
                    los = false;
                    return;
                }
                while (y < yend)
                {
                    x = x + k;
                    y = y + 1;
                    if (space[(int)Math.Floor(x),(int)y] < 0)
                    {
                        los = false;
                        return;
                    }
                    else
                    {
                        int xf0 = (int)Math.Floor(x - k);
                        int xf1 = (int)Math.Floor(x);
                        new_cost += Math.Sqrt(1 + (xf0 - xf1) * (xf0 - xf1)) * (space[xf0,(int)(y - 1)] + space[xf1,(int)y]) / 2;
                    }
                }
            }
            los = true;
            
        }
        /// <summary>
        /// 计算一点curr与另一点parent之间是否通视，如果通视，返回los为true，并且计算出加权路径长度
        /// </summary>
        /// <param name="curr_x"></param>
        /// <param name="curr_y"></param>
        /// <param name="parent_x"></param>
        /// <param name="parent_y"></param>
        /// <param name="space">阻抗栅格</param>
        /// <param name="los">通视与否</param>
        /// <param name="new_cost">加权路径长度</param>
        public static void DDA_Line_2(int curr_x, int curr_y, int parent_x, int parent_y, ref double[,] space, out bool los, out double new_cost)
        {
            int x1 = curr_x, y1 = curr_y, x2 = parent_x, y2 = parent_y;
            double k, dx, dy, x, y, xend, yend;

            new_cost = 0;
            dx = x2 - x1;
            dy = y2 - y1;
            if (curr_x == parent_x && curr_y == parent_y)
            {
                los = false;
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
                    if (y < 0 || y > array_width - 1)
                    {
                        break;
                    }
                    //SetDevicePixel((int)x, ROUND_INT(y));
                    if (space[(int)x, (int)Math.Floor(y)] < 0)
                    {
                        los = false;
                        return;
                    }
                    else
                    {
                        new_cost += space[(int)x, (int)Math.Floor(y)];
                    }
                    y = y + k;
                    x = x + 1;
                }
                new_cost = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) * new_cost / (Math.Abs(x2 - x1) + 1); //平面几何距离乘以路径平均阻抗，如果中间是平整的，则后面的比值为1

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
                    if (x < 0 || x > array_height - 1)
                    {
                        break;
                    }
                    if (space[(int)Math.Floor(x), (int)y] < 0)
                    {
                        los = false;
                        return;
                    }
                    else
                    {
                        new_cost += space[(int)Math.Floor(x), (int)y];
                    }
                    x = x + k;
                    y = y + 1;
                }
                new_cost = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) * new_cost / (Math.Abs(y2 - y1) + 1);
            }
            los = true;

        }

        public static void DDA_Line_3(int curr_x, int curr_y, int parent_x, int parent_y, ref double[,] space, out bool los, out double new_cost)
        {
            int x1 = curr_x, y1 = curr_y, x2 = parent_x, y2 = parent_y;
            double k, dx, dy, x, y, xend, yend;
            int count = 0;

            new_cost = 0;
            dx = x2 - x1;
            dy = y2 - y1;
            if (curr_x == parent_x && curr_y == parent_y)
            {
                los = false;
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

                    //SetDevicePixel((int)x, ROUND_INT(y));
                    if (space[(int)x, (int)Math.Floor(y)] < 0)
                    {
                        count++;
                        if (count > 1)
                        {
                            los = false;
                            return;
                        } 
                    }
                    else
                    {
                        new_cost += space[(int)x, (int)Math.Floor(y)];
                    }
                    y = y + k;
                    x = x + 1;
                }
                new_cost = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) * new_cost / (Math.Abs(x2 - x1) + count + 1); //平面几何距离乘以路径平均阻抗，如果中间是平整的，则比值为1

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
                    if (space[(int)Math.Floor(x), (int)y] < 0)
                    {
                        count++;
                        if (count > 1)
                        {
                            los = false;
                            return;
                        } 
                    }
                    else
                    {
                        new_cost += space[(int)Math.Floor(x), (int)y];
                    }
                    x = x + k;
                    y = y + 1;
                }
                new_cost = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) * new_cost / (Math.Abs(y2 - y1) + count + 1);
            }
            los = true;

        }
        

        public static void LineofSight_2(int curr_x, int curr_y, int parent_x, int parent_y, ref double[,] space, out bool los, out double new_cost)
        {
            int x0 = curr_x;
            int y0 = curr_y;
            int x1 = parent_x;
            int y1 = parent_y;
            int dr = parent_x - curr_x;
            int dc = parent_y - curr_y;
            int sign_x = 1;
            int sign_y = 1;
            int f = 0;

            new_cost = 0;

            if (dc < 0)
            {
                dc = -dc;
                sign_y = -1;
            }
            if (dr < 0)
            {
                dr = -dr;
                sign_x = -1;
            }
            if (dr >= dc)
            {
                while (x0 != x1)
                {
                    f = f + dc;
                    if (f >= dr)
                    {
                        if (space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                        {
                            los = false;
                            return;
                        }
                        y0 = y0 + sign_y;
                        f = f - dr;
                    }
                    if (f != 0 && space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                    {
                        los = false;
                        return;
                    }
                    if (dc == 0 && space[x0 + ((sign_x - 1) / 2), y0] <= 0 && space[x0 + ((sign_x - 1) / 2), y0 - 1] <= 0)
                    {
                        los = false;
                        return;
                    }
                    x0 = x0 + sign_x;
                }
            }
            else
            {
                while (y0 != y1)
                {
                    f = f + dr;
                    if (f >= dc)
                    {
                        if (space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                        {
                            los = false;
                            return;
                        }
                        x0 = x0 + sign_x;
                        f = f - dc;
                    }
                    if (f != 0 && space[x0 + ((sign_x - 1) / 2), y0 + ((sign_y - 1) / 2)] <= 0)
                    {
                        los = false;
                        return;
                    }
                    if (dr == 0 && space[x0, y0 + ((sign_y - 1) / 2)] <= 0 && space[x0 - 1, y0 + ((sign_y - 1) / 2)] <= 0)
                    {
                        los = false;
                        return;
                    }
                    y0 = y0 + sign_y;
                }
            }
            los = true;
        }
        #region 添加NoData区域的处理
        //public static void CalculateSourceNeibords(int center_x, int center_y, ref double[,] src, ref double[,] impedance, ref double[,] cost, ref double[,] backlink, ref double[,] array_nodata)
        //{
        //    //double [,] window = {{-1,-1,-1},{-1,-1,-1},{-1,-1,-1}};
        //    backlink[center_x, center_y] = 0;
        //    double factor = 1;
        //    double accu = 0.0;
        //    int sign = 0;
        //    for (int i = -1; i <= 1; i++)
        //    {
        //        int index_row = center_x + i;
        //        if ((index_row < 0) || (index_row > array_height - 1))
        //        {
        //            continue;
        //        }
        //        for (int j = -1; j <= 1; j++)
        //        {
        //            int index_col = center_y + j;
        //            if ((index_col < 0) || (index_col > array_width - 1))
        //            {
        //                continue;
        //            }
        //            if (src[index_row, index_col] == 1 || impedance[index_row, index_col]  < -1e38 || array_nodata[index_row,index_col]  < -1e38)
        //            {
        //                continue;
        //            }
        //            sign = Math.Abs(i * j);
        //            if (sign == 1)
        //                factor = 1.414;
        //            else
        //                factor = 1;
        //            accu = factor * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
        //            int node_exist = FindNodeByXY(index_row, index_col);
        //            if (node_exist == 0)
        //            {
        //                backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
        //                LinkNode item = new LinkNode();
        //                item.accumulate = accu;
        //                item.cRow = index_row;
        //                item.cColumn = index_col;
        //                nodeque.Add(item);
        //                cost[index_row, index_col] = accu;
        //            }
        //            else
        //            {
        //                LinkNode node = nodeque.ElementAt(node_exist);
        //                if (node.accumulate > accu)
        //                {
        //                    backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
        //                    node.accumulate = accu;
        //                    cost[index_row, index_col] = accu;
        //                }
        //            }

        //            //window[i + 1, j + 1] = impedance[index_row, index_col];
        //        }
        //    }
        //}

        #endregion

        #region 邻居像元耗费累积量计算

        //public static int stop = 0;
        public static void CalculateNodeAccumulation(int center_x, int center_y, int dest_x, int dest_y, ref double[,] src, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            //double [,] window = {{-1,-1,-1},{-1,-1,-1},{-1,-1,-1}};
            double[] factor = {1.0,Math.Sqrt(2.0)};
            double accu = cost[center_x, center_y];
            src[center_x, center_y] = 1;
            for (int i = -1; i <= 1; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -1; j <= 1; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1))
                    {
                        continue;
                    }
                    if (src[index_row, index_col] == 1 || impedance[index_row, index_col] == NoData)
                    {
                        continue;
                    }
                    double accu2 = accu + factor[Math.Abs(i * j)] * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                    
                    if (open[index_row, index_col] == 0) //节点不在队列中，则新建节点
                    {
                        LinkNode item = new LinkNode();
                        item.accumulate = accu2;
                        item.cRow = index_row;
                        item.cColumn = index_col;

                        //小堆的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;

                        cost[index_row, index_col] = accu2;
                        backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col],index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);
                        if (accu2 < cost[index_row, index_col])
                        {
                            cost[index_row, index_col] = accu2;
                            node.accumulate = accu2;
                            backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                        }
                        if (accu2 == cost[index_row, index_col])
                        {
                            double distance_1 = Math.Sqrt(Math.Pow(center_x - dest_x, 2) + Math.Pow(center_y - dest_y, 2));
                            int x_offset = 0;
                            int y_offset = 0;
                            BackLinkDirection2RowColumn(Convert.ToInt32(backlink[index_row, index_col]), out x_offset, out y_offset);
                            int compare_x = index_row + x_offset;
                            int compare_y = index_col + y_offset;
                            double distance_2 = Math.Sqrt(Math.Pow(compare_x - dest_x, 2) + Math.Pow(compare_y - dest_y, 2));

                            if (distance_1 < distance_2)
                            {
                                cost[index_row, index_col] = accu2;
                                node.accumulate = accu2;
                                backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                            }
                        }
                    }

                    //window[i + 1, j + 1] = impedance[index_row, index_col];
                }
            }
        }
        

        public const double pixel_depth = 0.1;
        public const double heuri_weight = 0.01;
        public static void CalculateNodeAccumulation_2(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            //double [,] window = {{-1,-1,-1},{-1,-1,-1},{-1,-1,-1}};
            double[] factor = { 1.0, Math.Sqrt(2.0) };
            double accu = cost[center_x, center_y];
            //open[center_x, center_y] = 1;
            for (int i = -1; i <= 1; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -1; j <= 1; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1))
                    {
                        continue;
                    }
                    if (impedance[index_row, index_col] <= 0)
                    {
                        continue;
                    }
                    double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                    double accu2 = accu + distance + factor[Math.Abs(i * j)] * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;

                    if (open[index_row, index_col] == 0) //节点不在队列中，则新建节点
                    {
                        LinkNode item = new LinkNode();
                        item.accumulate = accu2;
                        item.cRow = index_row;
                        item.cColumn = index_col;

                        //小堆的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;

                        cost[index_row, index_col] = accu2;
                        backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col], index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);
                        if (accu2 < cost[index_row, index_col])
                        {
                            cost[index_row, index_col] = accu2;
                            node.accumulate = accu2;
                            backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                        }

                        //耗费值相同时的处理方案是，取距离较近的像元
                        //if (accu2 == cost[index_row, index_col])
                        //{
                        //    double distance_1 = Math.Sqrt(Math.Pow(center_x - dest_x, 2) + Math.Pow(center_y - dest_y, 2));
                        //    int x_offset = 0;
                        //    int y_offset = 0;
                        //    BackLinkDirection2RowColumn(Convert.ToInt32(backlink[index_row, index_col]), out x_offset, out y_offset);
                        //    int compare_x = index_row + x_offset;
                        //    int compare_y = index_col + y_offset;
                        //    double distance_2 = Math.Sqrt(Math.Pow(compare_x - dest_x, 2) + Math.Pow(compare_y - dest_y, 2));

                        //    if (distance_1 < distance_2)
                        //    {
                        //        cost[index_row, index_col] = accu2;
                        //        node.accumulate = accu2;
                        //        backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                        //    }
                        //}
                    }

                    //window[i + 1, j + 1] = impedance[index_row, index_col];
                }
            }
        }

        public static void CalculateNodeAccumulation_3(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            double accu = cost[center_x, center_y];
            int prio = 1;
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    prio = Math.Max(Math.Abs(i), Math.Abs(j));
                    if (prio == 0)
                    {
                        prio = 1;
                    }
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                    double accu2 = accu + distance + Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;

                    if (open[index_row, index_col] == 0) //节点不在队列中，则新建节点
                    {
                        LinkNode item = new LinkNode();
                        item.accumulate = accu2;
                        item.cRow = index_row;
                        item.cColumn = index_col;

                        //小堆插入节点后的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;

                        cost[index_row, index_col] = accu2;
                        backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col], index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);
                        if (accu2 < cost[index_row, index_col])
                        {
                            cost[index_row, index_col] = accu2;
                            node.accumulate = accu2;

                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                        }

                        //耗费值相同时的处理方案是，取距离较近的像元
                        //if (accu2 == cost[index_row, index_col])
                        //{
                        //    double distance_1 = Math.Sqrt(Math.Pow(center_x - dest_x, 2) + Math.Pow(center_y - dest_y, 2));
                        //    int x_offset = 0;
                        //    int y_offset = 0;
                        //    BackLinkDirection2RowColumn(Convert.ToInt32(backlink[index_row, index_col]), out x_offset, out y_offset);
                        //    int compare_x = index_row + x_offset;
                        //    int compare_y = index_col + y_offset;
                        //    double distance_2 = Math.Sqrt(Math.Pow(compare_x - dest_x, 2) + Math.Pow(compare_y - dest_y, 2));

                        //    if (distance_1 < distance_2)
                        //    {
                        //        cost[index_row, index_col] = accu2;
                        //        node.accumulate = accu2;
                        //        backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
                        //    }
                        //}
                    }

                    //window[i + 1, j + 1] = impedance[index_row, index_col];
                }
            }
        }

        /// <summary>
        /// los检测范围不限，检测中心像元某邻居与中心像元的父像元是否可视（s1与parent_s），可视则替换
        /// </summary>
        /// <param name="center_x"></param>
        /// <param name="center_y"></param>
        /// <param name="dest_x"></param>
        /// <param name="dest_y"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <param name="impedance"></param>
        /// <param name="cost"></param>
        /// <param name="parentX"></param>
        /// <param name="parentY"></param>
        public static void CalculateNodeAccumulation_4(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX,ref int[,] parentY)
        {
            double accu_center = cost[center_x, center_y];
            double accu_center_parent = cost[parentX[center_x, center_y],parentY[center_x, center_y]];
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                    //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                    double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    double dis_parent_s_s1 = Math.Sqrt(Math.Pow(index_row - parentX[center_x, center_y], 2) + Math.Pow(index_col - parentY[center_x, center_y], 2));

                    if (open[index_row, index_col] == 0)
                    {
                        double new_cost = 0;
                        bool los;
                        LineofSight(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y], ref impedance, out los, out new_cost);
                        LinkNode item = new LinkNode();
                        item.cRow = index_row;
                        item.cColumn = index_col;
                        //bool los = Form1.IsLineIntersectWithPolygons(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y]);
                        if (los == true)
                        {
                            parentX[index_row, index_col] = parentX[center_x, center_y];
                            parentY[index_row, index_col] = parentY[center_x, center_y];
                            cost[index_row, index_col] = accu_center_parent + dis_parent_s_s1;
                            item.accumulate = accu_center_parent + dis_parent_s_s1;
                        }
                        else
                        {
                            parentX[index_row, index_col] = center_x;
                            parentY[index_row, index_col] = center_y;
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            item.accumulate = accu_center + dis_s_s1;
                        }

                        //小堆插入节点后的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col], index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);
                        if (accu_center + dis_s_s1 < cost[index_row, index_col])
                        {
                            parentX[index_row, index_col] = center_x;
                            parentY[index_row, index_col] = center_y;
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            node.accumulate = accu_center + dis_s_s1;
                            HeapUpAdjustment(nodeque, index, nodeque.Count);
                        }
                    }
  
                }
            }
        }
        
        /// <summary>
        /// los检测范围为中心像元的邻域，圈数为cycle
        /// </summary>
        /// <param name="center_x"></param>
        /// <param name="center_y"></param>
        /// <param name="dest_x"></param>
        /// <param name="dest_y"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        /// <param name="impedance"></param>
        /// <param name="cost"></param>
        /// <param name="backlink"></param>
        public static void CalculateNodeAccumulation_5(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            double accu_center = cost[center_x, center_y];
            int delta_row, delta_column;
            BackLinkDirection2RowColumn_2(backlink[center_x, center_y], out delta_row, out delta_column);
            int out_row = delta_row + center_x;
            int out_column = delta_column + center_y;
            double accu_center_parent = cost[out_row, out_column];
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                    //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                    double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    double dis_parent_s_s1 = Math.Sqrt(Math.Pow(index_row - out_row, 2) + Math.Pow(index_col - out_column, 2));

                    
                    if (open[index_row, index_col] == 0)
                    {
                        double new_cost = 0;
                        bool los_parent_s_s1;
                        DDA_Line_2(index_row, index_col, out_row, out_column, ref impedance, out los_parent_s_s1, out new_cost);

                        LinkNode item = new LinkNode();
                        item.cRow = index_row;
                        item.cColumn = index_col;
                        //bool los = Form1.IsLineIntersectWithPolygons(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y]);
                        if (los_parent_s_s1 == true)
                        {
                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, out_row, out_column);
                            cost[index_row, index_col] = accu_center_parent + dis_parent_s_s1;
                            item.accumulate = accu_center_parent + dis_parent_s_s1;
                        }
                        else
                        {
                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            item.accumulate = accu_center + dis_s_s1;
                        }
                        //小堆插入节点后的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col], index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);
                        if (accu_center + dis_s_s1 < cost[index_row, index_col])
                        {
                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            node.accumulate = accu_center + dis_s_s1;
                            HeapUpAdjustment(nodeque, index, nodeque.Count);
                        }
                    } 

                }
            }
        }
        
        public static void CalculateNodeAccumulation_5(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY)
        {
            double accu_center = cost[center_x, center_y];
            int parent_row = parentX[center_x, center_y];
            int parent_column = parentY[center_x, center_y];
            double accu_center_parent = cost[parent_row, parent_column];
            for (int i = -cycle; i <= cycle; i++)
            {
                int neibor_row = center_x + i;
                if ((neibor_row < 0) || (neibor_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int neibor_column = center_y + j;
                    if ((neibor_column < 0) || (neibor_column > array_width - 1))
                    {
                        continue;
                    }
                    if ((neibor_row < 0) || (neibor_row > array_height - 1) || (impedance[neibor_row, neibor_column] <= 0))
                    {
                        continue;
                    }
                    double dis_s_s1 = Math.Sqrt(Math.Pow(neibor_row - center_x, 2) + Math.Pow(neibor_column - center_y, 2));
                    double dis_parent_s_s1 = Math.Sqrt(Math.Pow(neibor_row - parent_row, 2) + Math.Pow(neibor_column - parent_column, 2));
                    //double heuri_value = HeuristicValue(index_row, index_col, dest_x, dest_y);
                    if (open[neibor_row, neibor_column] == 0)
                    {
                        LinkNode item = new LinkNode();
                        item.cRow = neibor_row;
                        item.cColumn = neibor_column;

                        double dis_parent_s_s1_2 = 0;
                        bool los_parent_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, parent_row, parent_column, ref impedance, out los_parent_s_s1, out dis_parent_s_s1_2);
                        //dis_parent_s_s1 += heuri_value;

                        //bool los = Form1.IsLineIntersectWithPolygons(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y]);
                        if (los_parent_s_s1 == true)
                        {
                            parentX[neibor_row, neibor_column] = parent_row;
                            parentY[neibor_row, neibor_column] = parent_column;
                            cost[neibor_row, neibor_column] = accu_center_parent + dis_parent_s_s1;
                            item.accumulate = accu_center_parent + dis_parent_s_s1;
                        }
                        else
                        {
                            double dis_s_s1_2 = 0;
                            bool los_s_s1;
                            DDA_Line(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1_2);
                            //dis_s_s1 += heuri_value;

                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;
                            cost[neibor_row, neibor_column] = accu_center + dis_s_s1;
                            item.accumulate = accu_center + dis_s_s1;
                        }
                        //小堆插入节点后的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[neibor_row, neibor_column] = 1;
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[neibor_row, neibor_column], neibor_row, neibor_column);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);

                        double dis_s_s1_2 = 0;
                        bool los_s_s1;
                        DDA_Line(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1_2);
                        //dis_s_s1 += heuri_value;

                        if ((accu_center + dis_s_s1 < cost[neibor_row, neibor_column]))
                        {
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_center + dis_s_s1;
                            node.accumulate = accu_center + dis_s_s1;
                            HeapUpAdjustment(nodeque, index, nodeque.Count);
                        }
                    }

                }
            }
        }

        public static void CalculateNodeAccumulation_6(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            double accu_center = cost[center_x, center_y];
            int delta_row, delta_column;
            BackLinkDirection2RowColumn_2(backlink[center_x, center_y], out delta_row, out delta_column);
            int parent_row = delta_row + center_x;
            int parent_column = delta_column + center_y;
            double accu_center_parent = cost[parent_row, parent_column];
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    //double dis_parent_s_s1 = Math.Sqrt(Math.Pow(index_row - out_row, 2) + Math.Pow(index_col - out_column, 2));

                    if (open[index_row, index_col] == 0)
                    {
                        LinkNode item = new LinkNode();
                        item.cRow = index_row;
                        item.cColumn = index_col;

                        double dis_parent_s_s1 = 0;
                        bool los_parent_s_s1;
                        DDA_Line_2(index_row, index_col, parent_row, parent_column, ref impedance, out los_parent_s_s1, out dis_parent_s_s1);

                        
                        //bool los = Form1.IsLineIntersectWithPolygons(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y]);
                        if (los_parent_s_s1 == true)
                        {
                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, parent_row, parent_column);
                            cost[index_row, index_col] = accu_center_parent + dis_parent_s_s1;
                            item.accumulate = accu_center_parent + dis_parent_s_s1;
                        }
                        else
                        {
                            double dis_s_s1 = 0;
                            bool los_s_s1;
                            DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            item.accumulate = accu_center + dis_s_s1;
                        }
                        //小堆插入节点后的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col], index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);

                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (accu_center + dis_s_s1 < cost[index_row, index_col]))
                        {
                            backlink[index_row, index_col] = BackLinkDirection_2(index_row, index_col, center_x, center_y);
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            node.accumulate = accu_center + dis_s_s1;
                            HeapUpAdjustment(nodeque, index, nodeque.Count);
                        }
                    }
                    
                }
            }
        }

        public static void WriteData2TXTFile(ref double[,] content, ref double[,] impedance, string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            for (int i = 0; i < array_height; i++)
            {
                for (int j = 0; j < array_width; j++)
                {
                    if (impedance[i, j] < 0)
                    {
                        sw.Write("{0:000}{1:000}-XXXXXXX, ", i, j);
                        continue;
                    }
                    if (content[i, j] <= 0)
                    {
                        sw.Write("{0:000}{1:000}--------, ", i, j);
                        continue;
                    }
                    sw.Write("{0:000}{1:000}-{2:0000.00}, ", i, j, content[i, j]);
                }
                sw.Write("\n");
            }
            sw.Close();
        }

        public static void WriteData2TXTFile(ref int[,] content, ref double[,] impedance, string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            for (int i = 0; i < array_height; i++)
            {
                for (int j = 0; j < array_width; j++)
                {
                    if (impedance[i, j] < 0)
                    {
                        sw.Write("{0:000}{1:000}-XXXXXXX, ", i, j);
                        continue;
                    }
                    if (content[i, j] <= 0)
                    {
                        sw.Write("{0:000}{1:000}--------, ", i, j);
                        continue;
                    }
                    sw.Write("{0:000}{1:000}-{2:0000.00}, ", i, j, content[i, j]);
                }
                sw.Write("\n");
            }
            sw.Close();
        }
        public static void CalculateNodeAccumulation_6(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY)
        {
            double accu_center = cost[center_x, center_y];
            int parent_row = parentX[center_x, center_y];
            int parent_column = parentY[center_x, center_y];
            double accu_center_parent = cost[parent_row, parent_column];
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    //double dis_parent_s_s1 = Math.Sqrt(Math.Pow(index_row - parent_row, 2) + Math.Pow(index_col - parent_column, 2));
                    //double heuri_value = HeuristicValue(index_row, index_col, dest_x, dest_y);
                    if (open[index_row, index_col] == 0)
                    {
                        LinkNode item = new LinkNode();
                        item.cRow = index_row;
                        item.cColumn = index_col;

                        double dis_parent_s_s1 = 0;
                        bool los_parent_s_s1;
                         DDA_Line_2(index_row, index_col, parent_row, parent_column, ref impedance, out los_parent_s_s1, out dis_parent_s_s1);
                        //dis_parent_s_s1 += heuri_value;

                        //bool los = Form1.IsLineIntersectWithPolygons(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y]);
                        if (los_parent_s_s1 == true)
                        {
                            parentX[index_row, index_col] = parent_row;
                            parentY[index_row, index_col] = parent_column;
                            cost[index_row, index_col] = accu_center_parent + dis_parent_s_s1;
                            item.accumulate = accu_center_parent + dis_parent_s_s1;
                        }
                        else
                        {
                            double dis_s_s1 = 0;
                            bool los_s_s1;
                            DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);
                            //dis_s_s1 += heuri_value;

                            parentX[index_row, index_col] = center_x;
                            parentY[index_row, index_col] = center_y;
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            item.accumulate = accu_center + dis_s_s1;
                        }
                        //小堆插入节点后的调整
                        nodeque.Add(item);
                        HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                        open[index_row, index_col] = 1;
                    }
                    else
                    {
                        int index = FindNodeFromCandidateList(cost[index_row, index_col], index_row, index_col);
                        if (index == -1)
                        {
                            continue;
                        }
                        LinkNode node = nodeque.ElementAt(index);
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);
                        //dis_s_s1 += heuri_value;

                        if ((cost[index_row, index_col] - (accu_center + dis_s_s1) < 1E-3) && (cost[index_row, index_col] - (accu_center + dis_s_s1) >= 0))
                        {
                            parentX[index_row, index_col] = center_x;
                            parentY[index_row, index_col] = center_y;

                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            node.accumulate = accu_center + dis_s_s1;
                            HeapUpAdjustment(nodeque, index, nodeque.Count);
                        }
                    }

                }
            }
        }

        public static void CalculateNodeAccumulation_7(int center_x, int center_y, int dest_x, int dest_y, ref int[,] open, ref int[,] close, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY)
        {
            double accu_center = cost[center_x, center_y];
            int parent_row = parentX[center_x, center_y];
            int parent_column = parentY[center_x, center_y];
            double accu_center_parent = cost[parent_row, parent_column];
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    //double dis_parent_s_s1 = Math.Sqrt(Math.Pow(index_row - parent_row, 2) + Math.Pow(index_col - parent_column, 2));
                    //double heuri_value = HeuristicValue(index_row, index_col, dest_x, dest_y);
                    if (open[index_row, index_col] == 0)
                    {
                        LinkNode item = new LinkNode();
                        item.cRow = index_row;
                        item.cColumn = index_col;

                        double dis_parent_s_s1 = 0;
                        bool los_parent_s_s1;
                        DDA_Line_2(index_row, index_col, parent_row, parent_column, ref impedance, out los_parent_s_s1, out dis_parent_s_s1);
                        //dis_parent_s_s1 += heuri_value;

                        //bool los = Form1.IsLineIntersectWithPolygons(index_row, index_col, parentX[center_x, center_y], parentY[center_x, center_y]);
                        if (los_parent_s_s1 == true)
                        {
                            parentX[index_row, index_col] = parent_row;
                            parentY[index_row, index_col] = parent_column;
                            cost[index_row, index_col] = accu_center_parent + dis_parent_s_s1;
                            item.accumulate = accu_center_parent + dis_parent_s_s1;
                        }
                        else
                        {
                            double dis_s_s1 = 0;
                            bool los_s_s1;
                            DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);
                            //dis_s_s1 += heuri_value;

                            parentX[index_row, index_col] = center_x;
                            parentY[index_row, index_col] = center_y;
                            cost[index_row, index_col] = accu_center + dis_s_s1;
                            item.accumulate = accu_center + dis_s_s1;
                        }
                        queue2.Enqueue(item);
                        open[index_row, index_col] = 1;
                    }
                }
            }
        }

        public static void UpdateNodeValues(int center_x, int center_y, int dest_x, int dest_y, ref double[,] impedance, ref double[,] cost, ref int[,] backlink)
        {
            for (int i = -cycle; i <= cycle; i++)
            {
                int index_row = center_x + i;
                if ((index_row < 0) || (index_row > array_height - 1))
                {
                    continue;
                }
                for (int j = -cycle; j <= cycle; j++)
                {
                    int index_col = center_y + j;
                    if ((index_col < 0) || (index_col > array_width - 1))
                    {
                        continue;
                    }
                    if ((index_row < 0) || (index_row > array_height - 1) || (impedance[index_row, index_col] <= 0))
                    {
                        continue;
                    }
                    //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                    //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                    //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                    double accu_index = cost[index_row, index_col];
                    double dis_s_s1 = 0;
                    bool los_s_s1;
                    DDA_Line_2(index_row, index_col, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                    if (los_s_s1 && accu_index + dis_s_s1 < cost[center_x, center_y])
                    {
                        backlink[center_x, center_y] = BackLinkDirection_2(center_x, center_y, index_row, index_col);
                        cost[center_x, center_y] = accu_index + dis_s_s1;
                    }

                }
            }
        }

        public static void UpdateNodeValues(int center_x, int center_y, int dest_x, int dest_y, ref double[,] impedance, ref double[,] cost, ref int[,] parentX,ref int[,] parentY,bool inverse_flag = false)
        {
            if (!inverse_flag) //逆序更新节点标志inverse_flag，true则逆序，否则顺序
            {
                for (int i = -cycle; i <= cycle; i++)
                {
                    int index_row = center_x + i;
                    if ((index_row < 0) || (index_row > array_height - 1))
                    {
                        continue;
                    }
                    for (int j = -cycle; j <= cycle; j++)
                    {
                        int index_col = center_y + j;
                        if ((index_col < 0) || (index_col > array_width - 1))
                        {
                            continue;
                        }
                        if (impedance[index_row, index_col] <= 0)
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                        int parent_row = parentX[index_row, index_col];
                        int parent_column = parentY[index_row, index_col];
                        double accu_parent_s1 = cost[parent_row, parent_column];
                        double dis_parent_s1_s;
                        bool los_parent_s1_s;
                        DDA_Line_2(center_x, center_y, parent_row, parent_column, ref impedance, out los_parent_s1_s, out dis_parent_s1_s);

                        if (los_parent_s1_s && (accu_parent_s1 + dis_parent_s1_s < cost[center_x, center_y]))
                        {
                            parentX[center_x, center_y] = parent_row;
                            parentY[center_x, center_y] = parent_column;

                            cost[center_x, center_y] = accu_parent_s1 + dis_parent_s1_s;
                        }

                    }
                }
            }
            else
            {
                for (int i = cycle; i >= -cycle; i--)
                {
                    int index_row = center_x + i;
                    if ((index_row < 0) || (index_row > array_height - 1))
                    {
                        continue;
                    }
                    for (int j = cycle; j >= -cycle; j--)
                    {
                        int index_col = center_y + j;
                        if ((index_col < 0) || (index_col > array_width - 1))
                        {
                            continue;
                        }
                        if (impedance[index_row, index_col] <= 0)
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                        int parent_row = parentX[index_row, index_col];
                        int parent_column = parentY[index_row, index_col];
                        double accu_parent_s1 = cost[parent_row, parent_column];
                        double dis_parent_s1_s;
                        bool los_parent_s1_s;
                        DDA_Line_2(center_x, center_y, parent_row, parent_column, ref impedance, out los_parent_s1_s, out dis_parent_s1_s);

                        if (los_parent_s1_s && (accu_parent_s1 + dis_parent_s1_s < cost[center_x, center_y]))
                        {
                            parentX[center_x, center_y] = parent_row;
                            parentY[center_x, center_y] = parent_column;

                            cost[center_x, center_y] = accu_parent_s1 + dis_parent_s1_s;
                        }

                    }
                }
            }
        }

        public static void UpdateNodeValues_2(int center_x, int center_y, int dest_x, int dest_y, ref double[,] impedance, ref double[,] cost, ref int[,] parentX,ref int[,] parentY,bool invert = false)
        {
            if (!invert)
            {
                for (int i = -cycle; i <= cycle; i++)
                {
                    double accu_s = cost[center_x, center_y];
                    int neibor_row = center_x + i;
                    if ((neibor_row < 0) || (neibor_row > array_height - 1))
                    {
                        continue;
                    }
                    for (int j = -cycle; j <= cycle; j++)
                    {
                        int neibor_column = center_y + j;
                        if ((neibor_column < 0) || (neibor_column > array_width - 1) || (impedance[neibor_row, neibor_column] <= 0))
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));

                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (cost[neibor_row, neibor_column] - (accu_s + dis_s_s1) >= 1E-2))
                        {
                            ticks++;
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_s + dis_s_s1;
                        }

                    }
                }
            }
            else
            {
                for (int i = cycle; i >= -cycle; i--)
                {
                    double accu_s = cost[center_x, center_y];
                    int neibor_row = center_x + i;
                    if ((neibor_row < 0) || (neibor_row > array_height - 1))
                    {
                        continue;
                    }
                    for (int j = cycle; j >= -cycle; j--)
                    {
                        int neibor_column = center_y + j;
                        if ((neibor_column < 0) || (neibor_column > array_width - 1) || (impedance[neibor_row, neibor_column] <= 0))
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));

                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (cost[neibor_row, neibor_column] - (accu_s + dis_s_s1) >= 1E-2))
                        {
                            ticks++;
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_s + dis_s_s1;
                        }

                    }
                }
            }
        }

        /// <summary>
        /// 按当前圈层数进行更新
        /// </summary>
        public static void UpdateNodeValues_3(int center_x, int center_y, int dest_x, int dest_y, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY, bool invert = false)
        {
            if (!invert)
            {
                for (int i = -cycle; i <= cycle; i += 2 * cycle)
                {
                    double accu_s = cost[center_x, center_y];
                    int neibor_row = center_x + i;
                    if ((neibor_row < 0) || (neibor_row > array_height - 1))
                    {
                        continue;
                    }
                    for (int j = -cycle; j <= cycle; j++)
                    {
                        int neibor_column = center_y + j;
                        if ((neibor_column < 0) || (neibor_column > array_width - 1) || (impedance[neibor_row, neibor_column] <= 0))
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));

                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (cost[neibor_row, neibor_column] - (accu_s + dis_s_s1) >= 1E-2))
                        {
                            ticks++;
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_s + dis_s_s1;
                        }

                    }
                }
                for (int j = -cycle; j <= cycle; j += 2 * cycle)
                {
                    double accu_s = cost[center_x, center_y];
                    int neibor_column = center_y + j;
                    if ((neibor_column < 0) || (neibor_column > array_width - 1))
                    {
                        continue;
                    }

                    for (int i = -cycle; i <= cycle; i++)
                    {
                        int neibor_row = center_x + i;
                        if ((neibor_row < 0) || (neibor_row > array_height - 1) || (impedance[neibor_row, neibor_column] <= 0))
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (cost[neibor_row, neibor_column] - (accu_s + dis_s_s1) >= 1E-2))
                        {
                            ticks++;
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_s + dis_s_s1;
                        }

                    }
                }
            }
            else
            {
                for (int i = cycle; i >= -cycle; i -= 2 * cycle)
                {
                    double accu_s = cost[center_x, center_y];
                    int neibor_row = center_x + i;
                    if ((neibor_row < 0) || (neibor_row > array_height - 1))
                    {
                        continue;
                    }
                    for (int j = cycle; j >= -cycle; j--)
                    {
                        int neibor_column = center_y + j;
                        if ((neibor_column < 0) || (neibor_column > array_width - 1) || (impedance[neibor_row, neibor_column] <= 0))
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));

                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (cost[neibor_row, neibor_column] - (accu_s + dis_s_s1) >= 1E-2))
                        {
                            ticks++;
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_s + dis_s_s1;
                        }

                    }
                }
                for (int j = cycle; j >= -cycle; j -= 2 * cycle)
                {
                    double accu_s = cost[center_x, center_y];
                    int neibor_column = center_y + j;
                    if ((neibor_column < 0) || (neibor_column > array_width - 1))
                    {
                        continue;
                    }

                    for (int i = cycle; i >= -cycle; i--)
                    {
                        int neibor_row = center_x + i;
                        if ((neibor_row < 0) || (neibor_row > array_height - 1) || (impedance[neibor_row, neibor_column] <= 0))
                        {
                            continue;
                        }
                        //double distance = heuri_weight * pixel_depth * Math.Sqrt(Math.Pow(index_row - dest_x, 2) + Math.Pow(index_col - dest_y, 2));
                        //double dis_s_s1 = Math.Sqrt(i * i + j * j) * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
                        //double dis_s_s1 = Math.Sqrt(Math.Pow(index_row - center_x, 2) + Math.Pow(index_col - center_y, 2));
                        double dis_s_s1 = 0;
                        bool los_s_s1;
                        DDA_Line_2(neibor_row, neibor_column, center_x, center_y, ref impedance, out los_s_s1, out dis_s_s1);

                        if (los_s_s1 && (cost[neibor_row, neibor_column] - (accu_s + dis_s_s1) >= 1E-2))
                        {
                            ticks++;
                            parentX[neibor_row, neibor_column] = center_x;
                            parentY[neibor_row, neibor_column] = center_y;

                            cost[neibor_row, neibor_column] = accu_s + dis_s_s1;
                        }

                    }
                }
            }
        }

        #endregion

        public static bool PathLengthFilter(int row, int column,  ref double[,]  impedance, ref double[,] cost,double max_length)
        {
            
            return cost[row, column] <= max_length;
        }

        public static long ticks = 0;
        public static long set_as_nodata_count = 0;
        public delegate void UpdateNodesFunctionDelegate(int center_x, int center_y, int dest_x, int dest_y, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY, bool invert = false);
        public delegate void WriteLineFunction(string value);
        public static void UpdateNodesViaHeapFun(int center_x, int center_y, int dest_x, int dest_y, ref double[,] impedance, ref double[,] cost, ref int[,] parentX, ref int[,] parentY, bool invert = false,StreamWriter sw = null,RichTextBox handle = null)
        {
            DateTime t1 = DateTime.Now;

            //程序运行日志记录所用的委托
            WriteLineFunction writer_function;
            if (sw != null)
            {
                writer_function = new WriteLineFunction(sw.WriteLine);
            }
            else
            {
                writer_function = new WriteLineFunction(Console.WriteLine);
            }
            
            writer_function("====================START===========================");
            handle.Text += "====================START===========================\n";
            writer_function("cycle: " + cycle);
            handle.Text += "cycle: " + cycle + "\n";
            writer_function("Start Time: " + t1.ToLongTimeString());
            handle.Text += "Start Time: " + t1.ToLongTimeString() + "\n";
            handle.SelectionStart = handle.TextLength;
            handle.ScrollToCaret();
            //更新节点的委托
            UpdateNodesFunctionDelegate update_nodes_function;
            if (Form1.circle_search_flag)
            {
                update_nodes_function = new UpdateNodesFunctionDelegate(UpdateNodeValues_3);
            }
            else
            {
                update_nodes_function = new UpdateNodesFunctionDelegate(UpdateNodeValues_2);
            }
            
            double path_cost = cost[dest_x, dest_y];
            for (int i = 0; i < array_height; i++)
            {
                for (int j = 0; j < array_width; j++)
                {
                    //if (cost[i, j] > path_cost + 10)
                    //{
                    //    set_as_nodata_count++;
                    //    impedance[i, j] = NoData;
                    //    continue;
                    //}
                    if (impedance[i, j] < 0)
                    {
                        continue;
                    }
                    LinkNode item = new LinkNode();
                    item.cRow = i;
                    item.cColumn = j;
                    item.accumulate = cost[i, j];
                    nodeque.Add(item);
                    HeapUpAdjustment(nodeque, nodeque.Count - 1, nodeque.Count);
                }
            }
            DateTime t2 = DateTime.Now;

            while (nodeque.Count != 0)
            {
                LinkNode node = new LinkNode(nodeque[0]);
                LinkNode a = nodeque[0];
                LinkNode b = nodeque[nodeque.Count - 1];
                Swap(ref a, ref b);
                nodeque.RemoveAt(nodeque.Count - 1);
                HeapDownAdjustment(nodeque, 0, nodeque.Count);

                update_nodes_function(node.cRow, node.cColumn, dest_x, dest_y, ref impedance, ref cost, ref parentX, ref parentY, invert);
            }
            DateTime t3 = DateTime.Now;

            writer_function("End Time: " + t3.ToLongTimeString());
            handle.Text += "End Time: " + t3.ToLongTimeString() + "\n";
            writer_function("t2 - t1: " + (t2 - t1).TotalMilliseconds / 1000.0 + "\nt3 - t1: " + (t3 - t2).TotalMilliseconds / 1000.0 + "\nupdated node counts: " + ticks);
            handle.Text += "t2 - t1: " + (t2 - t1).TotalMilliseconds / 1000.0 + "\nt3 - t1: " + (t3 - t2).TotalMilliseconds / 1000.0 + "\nupdated node counts: " + ticks +"\n";
            writer_function("=====================END============================");
            handle.Text += "=====================END============================\n";
            handle.SelectionStart = handle.TextLength;
            handle.ScrollToCaret();
        }

        #region DEM表面下源像元的邻居像元耗费累积量计算
        //public static void CalculateSourceNeibords(int center_x, int center_y, ref double[,] src, ref double[,] impedance, ref double[,] dem, ref double[,] cost, ref double[,] backlink)
        //{
        //    double[,] slope = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        //    backlink[center_x, center_y] = 0;
        //    for (int i = -1; i <= 1; i++)
        //    {
        //        for (int j = -1; j <= 1; j++)
        //        {
        //            slope[i + 1, j + 1] = (90 + Math.Atan(dem[center_x, center_y] - dem[center_x + i, center_y + j])) / 20;
        //        }
        //    }
        //    double factor = 1;
        //    double accu = 0.0;
        //    int sign = 0;
        //    for (int i = -1; i <= 1; i++)
        //    {
        //        int index_row = center_x + i;
        //        if ((index_row < 0) || (index_row > array_height - 1))
        //        {
        //            continue;
        //        }
        //        for (int j = -1; j <= 1; j++)
        //        {
        //            int index_col = center_y + j;
        //            if ((index_col < 0) || (index_col > array_width - 1))
        //            {
        //                continue;
        //            }
        //            if (src[index_row, index_col] == 1 || impedance[index_row, index_col] < -1e38)
        //            {
        //                continue;
        //            }
        //            sign = Math.Abs(i * j);
        //            if (sign == 1)
        //                factor = 1.414;
        //            else
        //                factor = 1;
        //            accu = factor * (impedance[index_row, index_col] + impedance[center_x, center_y] + slope[i + 1, j + 1]) / 2;
        //            int node_exist = FindNodeByXY(index_row, index_col);
        //            if (node_exist == 0)
        //            {
        //                backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
        //                LinkNode item = new LinkNode();
        //                item.accumulate = accu;
        //                item.cRow = index_row;
        //                item.cColumn = index_col;
        //                nodeque.Add(item);
        //                cost[index_row, index_col] = accu;
        //            }
        //            else
        //            {
        //                LinkNode node = nodeque.ElementAt(node_exist);
        //                if (node.accumulate > accu)
        //                {
        //                    backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
        //                    node.accumulate = accu;
        //                    cost[index_row, index_col] = accu;
        //                }
        //            }

        //            //window[i + 1, j + 1] = impedance[index_row, index_col];
        //        }
        //    }
        //}

        #endregion
        #region 考虑DEM表面的邻居像元耗费累积量计算
        //public static void CalculateNodeAccumulation(int center_x, int center_y, ref double[,] src, ref double[,] impedance, ref double[,] cost, ref double[,] backlink, ref double[,] dem, double dem_resolution = 1)
        //{
        //    //double [,] window = {{-1,-1,-1},{-1,-1,-1},{-1,-1,-1}};
        //    double[,] slope = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        //    double[] diagnal = { Math.Sqrt(2), 1 };
        //    for (int i = -1; i <= 1; i++)
        //    {
        //        for (int j = -1; j <= 1; j++)
        //        {
        //            slope[i + 1, j + 1] = (90 + 180 / Math.PI * Math.Atan2(dem[center_x, center_y] - dem[center_x + i, center_y + j], dem_resolution * diagnal[Math.Abs(i * j)])) / 20;
        //        }
        //    }

        //    //// -1  0  1
        //    //-1  1  0 -1
        //    //0   0  0  0
        //    //1  -1  0  1
        //    double factor = 1;
        //    double accu = cost[center_x, center_y];
        //    src[center_x, center_y] = 1;
        //    int sign = 0;
        //    for (int i = -1; i <= 1; i++)
        //    {
        //        int index_row = center_x + i;
        //        if ((index_row < 0) || (index_row > array_height - 1))
        //        {
        //            continue;
        //        }
        //        for (int j = -1; j <= 1; j++)
        //        {
        //            int index_col = center_y + j;
        //            if ((index_col < 0) || (index_col > array_width - 1))
        //            {
        //                continue;
        //            }
        //            if (src[index_row, index_col] == 1)
        //            {
        //                continue;
        //            }
        //            sign = Math.Abs(i * j);
        //            if (sign == 1)
        //                factor = 1.414;
        //            else
        //                factor = 1;
        //            double accu2 = accu + factor * (impedance[index_row, index_col] + impedance[center_x, center_y]) / 2;
        //            int node_exist = FindNodeByXY(index_row, index_col);
        //            if (node_exist == 0)
        //            {
        //                LinkNode item = new LinkNode();
        //                item.accumulate = accu2;
        //                item.cRow = index_row;
        //                item.cColumn = index_col;
        //                nodeque.Add(item);
        //                cost[index_row, index_col] = accu2;
        //                backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
        //            }
        //            else
        //            {
        //                LinkNode node = nodeque.ElementAt(node_exist);
        //                if (accu2 < cost[index_row, index_col])
        //                {
        //                    cost[index_row, index_col] = accu2;
        //                    node.accumulate = accu2;
        //                    backlink[index_row, index_col] = BackLinkDirection(index_row, index_col, center_x, center_y);
        //                }
        //            }

        //            //window[i + 1, j + 1] = impedance[index_row, index_col];
        //        }
        //    }
        //}
        #endregion

        //查找效率太低，是否可考虑用二分法或者hash散列？
        public static int FindNodeByXY(int index_row, int index_col)
        {
            //暴力遍历法
            foreach (LinkNode node in nodeque)
            {
                if (node.cRow == index_row && node.cColumn == index_col)
                {
                    return nodeque.IndexOf(node); 
                }
            }

            //二分法
            return 0;
        }

        /// <summary>
        /// 按照耗费累计值以及行列号信息搜寻队列中已存在的节点，并获取索引index
        /// </summary>
        /// <param name="value">目标耗费值</param>
        /// <param name="index_row">目标行号</param>
        /// <param name="index_col">目标列号</param>
        /// <returns>目标耗费值在队列中对应的节点索引</returns>
        public static int FindNodeByAccuValue(double value,int index_row, int index_col)
        {
            //二分法查找。当节点累计值相同时会出现问题，但已解决
            //堆排序引发的问题:队列不是完全有序的,只是能够保证队首元素最小,因此队列相同值的节点并不是相邻的
            //解决思路应该是用堆查找法
            int index = -1;

            int lowIndex = 0;
            int highIndex = nodeque.Count - 1;
            int middleIndex = -1;

            while (lowIndex <= highIndex)
            {
                middleIndex = (lowIndex + highIndex) / 2;
                if (Math.Abs(value - nodeque[middleIndex].accumulate) < 1e-6 )
                {
                    index = middleIndex;
                    break;
                }
                if (value > nodeque[middleIndex].accumulate)
                    lowIndex = middleIndex + 1;
                else
                    highIndex = middleIndex - 1;
            }
            //值相近的时候，首先向队列尾部方向搜索
            int k = index;
            while (k < nodeque.Count && Math.Abs(value - nodeque[k].accumulate) < 1e-6)
            {
                if (index_row == nodeque[k].cRow && index_col == nodeque[k].cColumn)
                {
                    break;
                }
                k++;
            }
            //然后向队列头部方向搜索
            k = index - 1;
            while (k >= 0 && Math.Abs(value - nodeque[k].accumulate) < 1e-6)
            {
                if (index_row == nodeque[k].cRow && index_col == nodeque[k].cColumn)
                {
                    break;
                }
                k--;
            }
            return index;
        }

        public static List<int> CandidateNodeList = new List<int>(100);
        public static void FindCandidateNodeByHeapSort(double value, int heap_start, int quesize)
        {

            //堆查找法
            int parent = heap_start;
            int left = parent * 2 + 1;
            int right = parent * 2 + 2;
            if (parent < quesize && Math.Abs(value - nodeque[parent].accumulate) < 1e-6)
            {
                CandidateNodeList.Add(parent);
            }
            if (left < quesize && Math.Abs(value - nodeque[left].accumulate) < 1e-6)
            {
                CandidateNodeList.Add(left);
                FindCandidateNodeByHeapSort(value, left, quesize);
            }
            if (right < quesize && Math.Abs(value - nodeque[right].accumulate) < 1e-6)
            {
                CandidateNodeList.Add(right);
                FindCandidateNodeByHeapSort(value, right, quesize);
            }
        }

        public static int FindNodeFromCandidateList(double value,int index_row, int index_col)
        {
            CandidateNodeList.Clear();
            FindCandidateNodeByHeapSort(value, 0, nodeque.Count);
            int index = -1;
            if (CandidateNodeList.Count == 0)
            {
                return -1;
            }
            foreach(int i in CandidateNodeList)
            {
                if (nodeque[i].cRow == index_row && nodeque[i].cColumn == index_col)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }


        public static void AddData2ImpedenceRaster(ref double[,] impedance, ref double[,] array_nodata, Form1.RasterOperationType mask, int rows, int columns)
        {

            for (int i = 0; i < rows; i++)
            {

                for (int j = 0; j < columns; j++)
                {
                    switch (mask)
                    {
                        case Form1.RasterOperationType.NodataMask:
                            if (array_nodata[i, j] != 255 || impedance[i, j] < -1e38)
                                impedance[i, j] = NoData;
                            break;
                        case Form1.RasterOperationType.Updater:
                            if ((array_nodata[i, j] != NoData) && (impedance[i, j] != NoData))
                                impedance[i, j] = array_nodata[i, j];
                            break;
                    }

                }
            }
        }


        /// <summary>
        /// 根据终点反算路径
        /// </summary>
        /// <param name="x">终点的行号</param>
        /// <param name="y">终点的列号</param>
        /// <param name="cost">计算过的耗费栅格数据</param>
        /// <param name="backlink">计算过的方向数据</param>
        /// <param name="output">输出double[,]类型的二维数组</param>
        public static void LeastCostPath(int x, int y, ref double[,] cost, ref int[,] backlink, ref double[,] output)
        {
            Console.WriteLine("(x,y):({0},{1})", x, y);
            Console.WriteLine("Accumulation :{0}", cost[x, y]);
            while (backlink[x, y] != 0)
            {
                int direction = (int)backlink[x, y];
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
                output[x, y] = 1;
            }
        }

        #region List<T>比较器的建立
        public static int CompareNode(LinkNode node1, LinkNode node2)
        {
            double accu1 = node1.accumulate;
            double accu2 = node2.accumulate;
            if (accu1 > accu2)
                return 1;
            else if (accu1 < accu2)
                return -1;
            else
                return 0;

        }
        #endregion

        public static void PrintScreen(double[,] cost)
        {
            for (int i = 0; i < array_height; i++)
            {
                for (int j = 0; j < array_width; j++)
                {
                    Console.Write("{0}, ", cost[i, j]);
                }
                Console.Write("\n");
            }
        }

        #region 最长非减子序列
        public static void LongestIncreasingSubsequence()
        {
            int[] d = { 2, 1, 5, 3, 6, 4, 8, 7, 9, 14, 11, 17 };
            int[] B = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int dN = d.Count();
            int i = 0;
            while (B[i] != 0)
                Console.Write("{0}\t", B[i++]);
            Console.Write("\n");
            int length = 1;
            B[0] = d[0];
            for (i = 1; i < dN; i++)
            {

                if (d[i] >= B[length - 1])
                {
                    B[length] = d[i];
                    length++;
                }
                else
                {
                    if (length == 1 || (length >= 2 && B[length - 2] < d[i]))
                        B[length - 1] = d[i];
                }
            }
        }
        #endregion

        public static void SetBarrierManual(ref double[,] barrier, int row, int begin, int end)
        {
            if ((begin > end) || (begin >= array_width) || (end >= array_width))
                return;
            for (int i = begin; i <= end; i++)
                barrier[row, i] = 1;
        }

        public static void AddBarrier2TerrainByWeight(ref double[,] terrain, ref double[,] barrier, double weight)
        {
            for (int i = 0; i < array_height; i++)
            {

                for (int j = 0; j < array_width; j++)
                {

                    if ((barrier[i, j] == 0) || (terrain[i, j]  < -1e38))
                        continue;
                    terrain[i, j] += weight * barrier[i, j];
                }
            }
        }

        //测试用的TEST数据及Main函数
        //static void Main(string[] args)
        //{



        //    double[,] data = { 
        //                        {1,3,4,4,3,2,5,8,7,5,6,6},
        //                        {4,6,2,3,7,6,4,3,2,5,8,2},
        //                        {5,8,7,5,6,9,6,2,3,7,6,4},
        //                        {1,4,5,7,5,1,5,8,7,5,6,6},
        //                        {4,7,5,3,2,3,2,5,8,7,5,6},
        //                        {1,2,2,1,3,4,4,5,7,5,1,5},
        //                        {5,8,7,5,6,6,3,2,3,2,5,8},
        //                        {1,3,4,4,3,2,2,3,2,5,8,7},
        //                        {4,6,2,3,7,6,2,1,3,4,4,5},
        //                        {7,5,3,2,6,4,3,2,2,3,2,8},
        //                        {1,2,2,1,3,4,5,6,6,3,2,3},
        //                        {5,8,7,5,6,6,7,5,6,9,6,2}
        //                     };

        //    double[,] dem = (double[,])data.Clone();
        //    double[,] data2 = { 
        //                        {1,3,4,4,3,2},
        //                        {4,6,2,3,7,6},
        //                        {5,8,7,5,6,6},
        //                        {1,4,5,7,NoData,1},
        //                        {4,7,5,3,NoData,6},
        //                        {1,2,2,1,3,4}
        //                     };
        //    double[,] source = { 
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0},
        //                        {0,0,0,0,0,0,0,0,0,0,0,0}
        //                     };
        //    double[,] barrier = (double[,])source.Clone();
        //    //SetBarrierManual(ref barrier, 6, 8, 11);
        //    //SetBarrierManual(ref barrier, 6, 0, 4);
        //    SetBarrierManual(ref barrier, 3, 3, 9);
        //    SetBarrierManual(ref barrier, 4, 3, 4);
        //    SetBarrierManual(ref barrier, 4, 8, 9);
        //    SetBarrierManual(ref barrier, 5, 3, 4);
        //    SetBarrierManual(ref barrier, 5, 8, 9);
        //    SetBarrierManual(ref barrier, 6, 3, 4);
        //    SetBarrierManual(ref barrier, 6, 8, 9);
        //    SetBarrierManual(ref barrier, 7, 3, 4);
        //    SetBarrierManual(ref barrier, 7, 8, 9);
        //    PrintScreen(barrier);

        //    AddBarrier2TerrainByWeight(ref data, ref barrier, 150);
        //    //PrintScreen(data);
        //    double[,] cost = (double[,])source.Clone();
        //    double[,] backlink = (double[,])source.Clone();
        //    double[,] output = (double[,])source.Clone();

        //    int source_x = 1;
        //    int source_y = 6;

        //    source[source_x, source_y] = 1;
        //    //CalculateSourceNeibords(0, 2, ref source, ref data, ref cost,ref backlink);
        //    //CalculateSourceNeibords(0, 1, ref source, ref data, ref cost,ref backlink);
        //    //CalculateSourceNeibords(1, 2, ref source, ref data, ref cost,ref backlink);

        //    CalculateSourceNeibords(source_x, source_y, ref source, ref data, ref cost, ref backlink);

        //    backlink[source_x, source_y] = 0;

        //    PrintScreen(cost);
        //    nodeque.Sort(CompareNode);

        //    while (nodeque.Count != 0)
        //    {
        //        LinkNode node = nodeque.First();
        //        CalculateNodeAccumulation(node.cRow, node.cColumn, ref source, ref data, ref cost, ref backlink);
        //        Console.WriteLine("NodeCount:{0}.", nodeque.Count);
        //        PrintScreen(cost);
        //        nodeque.Remove(node);
        //        nodeque.Sort(CompareNode);
        //    }

        //    PrintScreen(backlink);

        //    LeastCostPath(11, 6, ref cost, ref backlink, ref output);
        //    Console.WriteLine("\n");
        //    PrintScreen(output);
        //    Console.WriteLine("finish.\n");
        //}

    }
}