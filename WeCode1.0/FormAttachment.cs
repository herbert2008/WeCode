﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Data.OleDb;

namespace WeCode1._0
{
    public partial class FormAttachment : DockContent
    {

        public FormAttachment()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FormAttachment_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = AccessAdo.ExecuteDataSet("select '' as 序号,[title] as 标题,[size] as 大小,[time] as 加入日期 from tattachment where nodeid=" + Attachment.ActiveNodeId).Tables[0];

        }

        //刷新附件
        public void ReFreshAttachGrid()
        {
            DataView dv = new DataView(AccessAdo.ExecuteDataSet("select affixid,nodeid,'' as 序号,[title] as 标题,[size] as 大小,[time] as 加入日期 from tattachment where nodeid=" + Attachment.ActiveNodeId).Tables[0]);
            dataGridView1.DataSource = dv;
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns[1].Visible = false;

            int count = dv.Count;
            for (int i = 0; i < count; i++)
            {
                dataGridView1.Rows[i].Cells[2].Value = i+1;
            }
        }

        //添加附件
        private void toolStripMenuItemAddZIP_Click(object sender, EventArgs e)
        {
            Stream myStream;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.RestoreDirectory = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    // Insert code to read the stream here.
                    FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read);
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);

                    int Datalength = (int)fs.Length;
                    fs.Close();

                    string NewAffixid = AccessAdo.ExecuteScalar("select max(affixid) from tattachment").ToString();
                    int intNewAffixid = NewAffixid == "" ? 1 : Convert.ToInt32(NewAffixid) + 1;

                    int Nodeid = Convert.ToInt32(Attachment.ActiveNodeId);
                    string Title = openFileDialog1.SafeFileName;
                    int timeSeconds = PubFunc.time2TotalSeconds();

                    //affixid
                    OleDbParameter p1 = new OleDbParameter("@affixid", OleDbType.Integer);
                    p1.Value = intNewAffixid;
                    //nodeid
                    OleDbParameter p2 = new OleDbParameter("@nodeid", OleDbType.Integer);
                    p2.Value = Nodeid;
                    //title
                    OleDbParameter p3 = new OleDbParameter("@title", OleDbType.VarChar);
                    p3.Value = Title;
                    //二进制数据
                    OleDbParameter p4 = new OleDbParameter("@Data", OleDbType.Binary);
                    p4.Value = data;
                    //size
                    OleDbParameter p5 = new OleDbParameter("@size", OleDbType.Integer);
                    p5.Value = Datalength;
                    //time
                    OleDbParameter p6 = new OleDbParameter("@time", OleDbType.Integer);
                    p6.Value = timeSeconds;

                    OleDbParameter[] arrPara = new OleDbParameter[6];
                    arrPara[0] = p1;
                    arrPara[1] = p2;
                    arrPara[2] = p3;
                    arrPara[3] = p4;
                    arrPara[4] = p5;
                    arrPara[5] = p6;
                    string SQL = "insert into tattachment values(@affixid,@nodeid,@title,@Data,@size,@time)";
                    AccessAdo.ExecuteNonQuery(SQL, arrPara);

                    myStream.Close();

                    ReFreshAttachGrid();
                }
            }
        }

        //另存为
        private void toolStripMenuItemSaveAs_Click(object sender, EventArgs e)
        {
            string Affixid = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            string Title = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
            byte[] TempData = (byte[])AccessAdo.ExecuteScalar("select data from tattachment where affixid=" + Affixid);

            string Path;
            SaveFileDialog sf=new SaveFileDialog();
            sf.FileName=Title;
            //设置文件类型
            sf.Filter = "All files(*.*)|*.*";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                Path = sf.FileName;
                FileStream fs = new FileStream(Path, FileMode.Create);
                fs.Write(TempData, 0, TempData.Length);
            }

        }
    }
}
