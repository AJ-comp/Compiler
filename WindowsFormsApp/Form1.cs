using ActiproSoftware.SyntaxEditor;
using Parse.FrontEnd.Ast;
using Parse.Extensions;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.LR;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class MainForm : Form
    {
        private ToolTip toolTip1 = new ToolTip();
        private LRParser parser = new SLRParser(new MiniCGrammar());
        private int recentRowIdx = -1;
        private int recentColIdx = -1;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ConnectAstToTreeView(AstSymbol astSymbol, TreeNodeCollection nodes)
        {
            if (astSymbol is AstTerminal) nodes.Add((astSymbol as AstTerminal).Token.Input);
            else if (astSymbol is AstNonTerminal)
            {
                AstNonTerminal astNT = (astSymbol as AstNonTerminal);
                int index = nodes.Count;
                nodes.Add(astNT.SignPost.Name);

                for (int i = 0; i < astNT.Count; i++)
                {
                    var child = astNT[i];
                    this.ConnectAstToTreeView(child, nodes[index].Nodes);
                }
            }
        }

        private void ConnectAstToTreeView(IList<AstSymbol> astSymbols, TreeNodeCollection nodes)
        {
            foreach (var symbol in astSymbols) this.ConnectAstToTreeView(symbol, nodes);
        }

        private void CreateMemberList(IntelliPromptMemberList memberList)
        {
            if (memberList.Count != 0) return;

            foreach (var terminal in this.parser.PossibleTerminalSet)
                memberList.Add(new ActiproSoftware.SyntaxEditor.IntelliPromptMemberListItem(terminal.Value, 0));

            if (memberList.Count > 0) memberList.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.editor.AcceptsTab = true;
            this.editor.SelectionTabs = new int[] { 20, 40, 60, 80, 100, 120, 140, 160, 180, 200 };
            this.tableGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            this.historyGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            //            this.tableGridView.AutoResizeColumns();
            this.tableGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.historyGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            this.grammar.Text = parser.Grammar.ToString();
            this.canonicalCollection.Text = parser.AnalysisResult;
            this.tableGridView.DataSource = parser.ParsingTable;
            this.tableGridView.Refresh();
            this.tableGridView.Update();
            this.historyGridView.DataSource = parser.ParsingHistory;
            //            Console.WriteLine(parser.OptimizeList);
            //            Console.WriteLine(parser.PossibleTerminalSet.ToString());

            //            parser.Parse("a*b+ad");
            //            this.ConnectAstToTreeView(parser.AstRoot, this.astView.Nodes);

            this.parser.Parse(this.syntaxEditor.Text);
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            this.astView.Nodes.Clear();
            this.parser.Parse(this.editor.Text);
        }

        private void parsingTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.parsingTab.SelectedTab.Name == "parsingHistoryTab")
            {
                this.historyGridView.DataSource = parser.ParsingHistory;
                parser.ParsingHistory.Print();
                this.historyGridView.Update();
                this.historyGridView.Refresh();
            }
            else if (this.parsingTab.SelectedTab.Name == "parseTreeTab")
            {
                this.astView.Nodes.Clear();
                this.ConnectAstToTreeView(parser.ParseTree, this.astView.Nodes);
            }
        }

        private void tableGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (this.recentColIdx == e.ColumnIndex && this.recentRowIdx == e.RowIndex) return;
            this.recentColIdx = e.ColumnIndex;
            this.recentRowIdx = e.RowIndex;

            toolTip1.Hide(this.tableGridView);
            if (e.ColumnIndex != 0 || e.RowIndex == -1) return;

            this.tableGridView.ShowCellToolTips = false;

            var cell = this.tableGridView[e.ColumnIndex, e.RowIndex];
            Canonical canonical = this.parser.C0.GetStatusFromIxIndex(Convert.ToInt32(cell.Value.ToString().Substring(1)));

            var data = canonical.ToLineString();
            var lineCount = Regex.Matches(data, Environment.NewLine).Count;
            if (lineCount == 0 || lineCount == -1) lineCount = 1;

            var popDelay = 3000 * lineCount;
            if (popDelay > 30000) popDelay = 30000;
            toolTip1.Show(canonical.ToLineString(), this.tableGridView, popDelay);
        }

        private void syntaxEditor_IntelliPromptMemberListClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.syntaxEditor.IntelliPrompt.MemberList.Clear();
        }

        private void syntaxEditor_DocumentPreTextChanging(object sender, ActiproSoftware.SyntaxEditor.DocumentModificationEventArgs e)
        {
            if (e.Modification.InsertedText == "\n") return;
            if (e.Modification.InsertedText == string.Empty) return;

            if (e.DirtyTextRange.StartOffset == 0) this.CreateMemberList(this.syntaxEditor.IntelliPrompt.MemberList);
            else
            {
                char data = e.Document.Text[e.DirtyTextRange.StartOffset - 1];
                if (this.parser.DelimiterList.Contains(data.ToString()) == false) return;

                this.CreateMemberList(this.syntaxEditor.IntelliPrompt.MemberList);
            }
        }

        private void syntaxEditor_DocumentTextChanged(object sender, DocumentModificationEventArgs e)
        {
            this.parser.Parse(this.syntaxEditor.Text);
        }
    }
}
