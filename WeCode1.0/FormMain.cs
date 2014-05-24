using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WeCode1._0
{
    public partial class FormMain : Form
    {
        private FormTreeLeft frTree;


        #region Fields

        private int _newDocumentCount = 0;
        private string[] _args;
        private int _zoomLevel=0;
        private const int LINE_NUMBERS_MARGIN_WIDTH = 35;

        #endregion Fields

        #region Properties

        public DocumentForm ActiveDocument
        {
            get
            {
                return dockPanel1.ActiveDocument as DocumentForm;
            }
        }

        #endregion Properties


        public FormMain()
        {
            InitializeComponent();

            //显示树窗口
            frTree = new FormTreeLeft();
            frTree.formParent = this;
            frTree.Show(dockPanel1);

            //显示附件窗口
            Attachment.ActiveNodeId = "-1";
            FormAttachment frmAttchment = new FormAttachment();
            Attachment.AttForm = frmAttchment;
            frmAttchment.Show(dockPanel1);
        }

        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            frTree.setNodeUp();
        }

        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            frTree.setNodeDown();
        }

        //关闭文章
        public void CloseDoc(string nodeId)
        {
            foreach (DocumentForm documentForm in dockPanel1.Documents)
            {
                if (nodeId.Equals(documentForm.NodeId, StringComparison.OrdinalIgnoreCase))
                {
                    documentForm.Close();
                    break;
                }
            }
        }

        //打开文章
        public void openNew(string nodeId)
        {

                // 如果已经打开，则定位，否则新窗口打开
                bool isOpen = false;
                foreach (DocumentForm documentForm in dockPanel1.Documents)
                {
                    if (nodeId.Equals(documentForm.NodeId, StringComparison.OrdinalIgnoreCase))
                    {
                        documentForm.Select();
                        isOpen = true;
                        break;
                    }
                }

                // Open the files
                if (!isOpen)
                    OpenFile(nodeId);
        }


        private DocumentForm OpenFile(string nodeId)
        {

            //获取文章信息
            string SQL = "select Title,Content from TContent inner join TTree on TContent.NodeId=Ttree.NodeId where TContent.NodeId=" + nodeId;
            DataTable temp = AccessAdo.ExecuteDataSet(SQL, null).Tables[0];
            if (temp.Rows.Count == 0)
                return null;
            string Title = temp.Rows[0]["Title"].ToString();
            string Content = temp.Rows[0]["Content"].ToString();

            DocumentForm doc = new DocumentForm();
            SetScintillaToCurrentOptions(doc);
            doc.Scintilla.Text = Content;
            doc.Scintilla.UndoRedo.EmptyUndoBuffer();
            doc.Scintilla.Modified = false;
            doc.Text = Title;
            doc.NodeId = nodeId;
            doc.Show(dockPanel1);
            

            return doc;
        }

        //配置相关显示参数
        private void SetScintillaToCurrentOptions(DocumentForm doc)
        {
            //// Turn on line numbers?
            //if (lineNumbersToolStripMenuItem.Checked)
                doc.Scintilla.Margins.Margin0.Width = LINE_NUMBERS_MARGIN_WIDTH;
            //else
            //    doc.Scintilla.Margins.Margin0.Width = 0;

            //// Turn on white space?
            //if (whitespaceToolStripMenuItem.Checked)
            //    doc.Scintilla.Whitespace.Mode = WhitespaceMode.VisibleAlways;
            //else
            //    doc.Scintilla.Whitespace.Mode = WhitespaceMode.Invisible;

            //// Turn on word wrap?
            //if (wordWrapToolStripMenuItem.Checked)
            //    doc.Scintilla.LineWrapping.Mode = LineWrappingMode.Word;
            //else
            //    doc.Scintilla.LineWrapping.Mode = LineWrappingMode.None;

            //// Show EOL?
            //doc.Scintilla.EndOfLine.IsVisible = endOfLineToolStripMenuItem.Checked;

            // Set the zoom
            doc.Scintilla.ZoomFactor = _zoomLevel;
        }


        private void toolStripButtonNewText_Click(object sender, EventArgs e)
        {
            frTree.NewDoc();
        }

        private void toolStripButtonNewDir_Click(object sender, EventArgs e)
        {
            frTree.NewDir();
        }

        //保存
        private void toolStripButtonSv_Click(object sender, EventArgs e)
        {
            if (ActiveDocument != null)
                ActiveDocument.Save();
        }


        //设置语言
        public void SetLanguage(string language)
        {
            if ("ini".Equals(language, StringComparison.OrdinalIgnoreCase))
            {
                // Reset/set all styles and prepare _scintilla for custom lexing
                ActiveDocument.IniLexer = true;
                IniLexer.Init(ActiveDocument.Scintilla);
            }
            else
            {
                // Use a built-in lexer and configuration
                ActiveDocument.IniLexer = false;
                ActiveDocument.Scintilla.ConfigurationManager.Language = language;

                // Smart indenting...
                if ("cs".Equals(language, StringComparison.OrdinalIgnoreCase))
                    ActiveDocument.Scintilla.Indentation.SmartIndentType = ScintillaNET.SmartIndent.CPP;
                else
                    ActiveDocument.Scintilla.Indentation.SmartIndentType = ScintillaNET.SmartIndent.None;
            }
        }
        
        //保存所有
        private void toolStripButtonSvAll_Click(object sender, EventArgs e)
        {
            foreach (DocumentForm doc in dockPanel1.Documents)
            {
                doc.Activate();
                doc.Save();
            }
        }
    }
}
