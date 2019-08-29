namespace WindowsFormsApp
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            ActiproSoftware.SyntaxEditor.Document document1 = new ActiproSoftware.SyntaxEditor.Document();
            ActiproSoftware.SyntaxEditor.VisualStudio2005SyntaxEditorRenderer visualStudio2005SyntaxEditorRenderer1 = new ActiproSoftware.SyntaxEditor.VisualStudio2005SyntaxEditorRenderer();
            this.parseTreeTab = new System.Windows.Forms.TabPage();
            this.astView = new System.Windows.Forms.TreeView();
            this.parsingHistoryTab = new System.Windows.Forms.TabPage();
            this.historyGridView = new System.Windows.Forms.DataGridView();
            this.parsingTableTab = new System.Windows.Forms.TabPage();
            this.tableGridView = new System.Windows.Forms.DataGridView();
            this.parsingTab = new System.Windows.Forms.TabControl();
            this.editorTab = new System.Windows.Forms.TabPage();
            this.syntaxEditor = new ActiproSoftware.SyntaxEditor.SyntaxEditor();
            this.editor = new System.Windows.Forms.RichTextBox();
            this.grammarTab = new System.Windows.Forms.TabPage();
            this.grammar = new System.Windows.Forms.Label();
            this.canonicalTableTab = new System.Windows.Forms.TabPage();
            this.canonicalCollection = new System.Windows.Forms.RichTextBox();
            this.parseTreeTab.SuspendLayout();
            this.parsingHistoryTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.historyGridView)).BeginInit();
            this.parsingTableTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableGridView)).BeginInit();
            this.parsingTab.SuspendLayout();
            this.editorTab.SuspendLayout();
            this.grammarTab.SuspendLayout();
            this.canonicalTableTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // parseTreeTab
            // 
            this.parseTreeTab.Controls.Add(this.astView);
            this.parseTreeTab.Location = new System.Drawing.Point(4, 22);
            this.parseTreeTab.Name = "parseTreeTab";
            this.parseTreeTab.Padding = new System.Windows.Forms.Padding(3);
            this.parseTreeTab.Size = new System.Drawing.Size(792, 424);
            this.parseTreeTab.TabIndex = 4;
            this.parseTreeTab.Text = "ParseTree";
            this.parseTreeTab.UseVisualStyleBackColor = true;
            // 
            // astView
            // 
            this.astView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.astView.Location = new System.Drawing.Point(3, 3);
            this.astView.Name = "astView";
            this.astView.Size = new System.Drawing.Size(786, 418);
            this.astView.TabIndex = 0;
            // 
            // parsingHistoryTab
            // 
            this.parsingHistoryTab.Controls.Add(this.historyGridView);
            this.parsingHistoryTab.Location = new System.Drawing.Point(4, 22);
            this.parsingHistoryTab.Name = "parsingHistoryTab";
            this.parsingHistoryTab.Padding = new System.Windows.Forms.Padding(3);
            this.parsingHistoryTab.Size = new System.Drawing.Size(792, 424);
            this.parsingHistoryTab.TabIndex = 3;
            this.parsingHistoryTab.Text = "Parsing History";
            this.parsingHistoryTab.UseVisualStyleBackColor = true;
            // 
            // historyGridView
            // 
            this.historyGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.historyGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyGridView.Location = new System.Drawing.Point(3, 3);
            this.historyGridView.Name = "historyGridView";
            this.historyGridView.RowTemplate.Height = 23;
            this.historyGridView.Size = new System.Drawing.Size(786, 418);
            this.historyGridView.TabIndex = 0;
            // 
            // parsingTableTab
            // 
            this.parsingTableTab.Controls.Add(this.tableGridView);
            this.parsingTableTab.Location = new System.Drawing.Point(4, 22);
            this.parsingTableTab.Name = "parsingTableTab";
            this.parsingTableTab.Padding = new System.Windows.Forms.Padding(3);
            this.parsingTableTab.Size = new System.Drawing.Size(792, 424);
            this.parsingTableTab.TabIndex = 2;
            this.parsingTableTab.Text = "Parsing Table";
            this.parsingTableTab.UseVisualStyleBackColor = true;
            // 
            // tableGridView
            // 
            this.tableGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableGridView.Location = new System.Drawing.Point(3, 3);
            this.tableGridView.Name = "tableGridView";
            this.tableGridView.RowTemplate.Height = 23;
            this.tableGridView.Size = new System.Drawing.Size(786, 418);
            this.tableGridView.TabIndex = 0;
            this.tableGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.tableGridView_CellMouseEnter);
            // 
            // parsingTab
            // 
            this.parsingTab.Controls.Add(this.editorTab);
            this.parsingTab.Controls.Add(this.grammarTab);
            this.parsingTab.Controls.Add(this.canonicalTableTab);
            this.parsingTab.Controls.Add(this.parsingTableTab);
            this.parsingTab.Controls.Add(this.parsingHistoryTab);
            this.parsingTab.Controls.Add(this.parseTreeTab);
            this.parsingTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parsingTab.Location = new System.Drawing.Point(0, 0);
            this.parsingTab.Name = "parsingTab";
            this.parsingTab.SelectedIndex = 0;
            this.parsingTab.Size = new System.Drawing.Size(800, 450);
            this.parsingTab.TabIndex = 0;
            this.parsingTab.SelectedIndexChanged += new System.EventHandler(this.parsingTab_SelectedIndexChanged);
            // 
            // editorTab
            // 
            this.editorTab.Controls.Add(this.syntaxEditor);
            this.editorTab.Controls.Add(this.editor);
            this.editorTab.Location = new System.Drawing.Point(4, 22);
            this.editorTab.Name = "editorTab";
            this.editorTab.Padding = new System.Windows.Forms.Padding(3);
            this.editorTab.Size = new System.Drawing.Size(792, 424);
            this.editorTab.TabIndex = 0;
            this.editorTab.Text = "Editor";
            this.editorTab.UseVisualStyleBackColor = true;
            // 
            // syntaxEditor
            // 
            this.syntaxEditor.CurrentLineHighlightingVisible = true;
            this.syntaxEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.syntaxEditor.Document = document1;
            this.syntaxEditor.LineNumberMarginVisible = true;
            this.syntaxEditor.LineNumberMarginWidth = 30;
            this.syntaxEditor.Location = new System.Drawing.Point(3, 3);
            this.syntaxEditor.Name = "syntaxEditor";
            visualStudio2005SyntaxEditorRenderer1.ResetAllPropertiesOnSystemColorChange = false;
            this.syntaxEditor.Renderer = visualStudio2005SyntaxEditorRenderer1;
            this.syntaxEditor.Size = new System.Drawing.Size(786, 418);
            this.syntaxEditor.TabIndex = 2;
            this.syntaxEditor.DocumentPreTextChanging += new ActiproSoftware.SyntaxEditor.DocumentModificationEventHandler(this.syntaxEditor_DocumentPreTextChanging);
            this.syntaxEditor.DocumentTextChanged += new ActiproSoftware.SyntaxEditor.DocumentModificationEventHandler(this.syntaxEditor_DocumentTextChanged);
            this.syntaxEditor.IntelliPromptMemberListClosed += new System.ComponentModel.CancelEventHandler(this.syntaxEditor_IntelliPromptMemberListClosed);
            // 
            // editor
            // 
            this.editor.Location = new System.Drawing.Point(0, 0);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(100, 96);
            this.editor.TabIndex = 3;
            this.editor.Text = "";
            // 
            // grammarTab
            // 
            this.grammarTab.Controls.Add(this.grammar);
            this.grammarTab.Location = new System.Drawing.Point(4, 22);
            this.grammarTab.Name = "grammarTab";
            this.grammarTab.Padding = new System.Windows.Forms.Padding(3);
            this.grammarTab.Size = new System.Drawing.Size(792, 424);
            this.grammarTab.TabIndex = 5;
            this.grammarTab.Text = "grammar";
            this.grammarTab.UseVisualStyleBackColor = true;
            // 
            // grammar
            // 
            this.grammar.AutoSize = true;
            this.grammar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grammar.Location = new System.Drawing.Point(3, 3);
            this.grammar.Name = "grammar";
            this.grammar.Size = new System.Drawing.Size(56, 12);
            this.grammar.TabIndex = 0;
            this.grammar.Text = "grammar";
            // 
            // canonicalTableTab
            // 
            this.canonicalTableTab.Controls.Add(this.canonicalCollection);
            this.canonicalTableTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canonicalTableTab.Location = new System.Drawing.Point(4, 22);
            this.canonicalTableTab.Name = "canonicalTableTab";
            this.canonicalTableTab.Padding = new System.Windows.Forms.Padding(3);
            this.canonicalTableTab.Size = new System.Drawing.Size(792, 424);
            this.canonicalTableTab.TabIndex = 1;
            this.canonicalTableTab.Text = "Canonical Table";
            this.canonicalTableTab.UseVisualStyleBackColor = true;
            // 
            // canonicalCollection
            // 
            this.canonicalCollection.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.canonicalCollection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canonicalCollection.Location = new System.Drawing.Point(3, 3);
            this.canonicalCollection.Name = "canonicalCollection";
            this.canonicalCollection.ReadOnly = true;
            this.canonicalCollection.Size = new System.Drawing.Size(786, 418);
            this.canonicalCollection.TabIndex = 0;
            this.canonicalCollection.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.parsingTab);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.parseTreeTab.ResumeLayout(false);
            this.parsingHistoryTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.historyGridView)).EndInit();
            this.parsingTableTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableGridView)).EndInit();
            this.parsingTab.ResumeLayout(false);
            this.editorTab.ResumeLayout(false);
            this.grammarTab.ResumeLayout(false);
            this.grammarTab.PerformLayout();
            this.canonicalTableTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage parseTreeTab;
        private System.Windows.Forms.TabPage parsingHistoryTab;
        private System.Windows.Forms.TabPage parsingTableTab;
        private System.Windows.Forms.DataGridView tableGridView;
        private System.Windows.Forms.TabControl parsingTab;
        private System.Windows.Forms.DataGridView historyGridView;
        private System.Windows.Forms.TabPage editorTab;
        private System.Windows.Forms.RichTextBox editor;
        private System.Windows.Forms.TabPage canonicalTableTab;
        private System.Windows.Forms.TreeView astView;
        private System.Windows.Forms.RichTextBox canonicalCollection;
        private ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor;
        private System.Windows.Forms.TabPage grammarTab;
        private System.Windows.Forms.Label grammar;
    }
}

