namespace PackRatUI {
    partial class PackRatUI {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.objectListView1 = new BrightIdeasSoftware.ObjectListView();
            this.cThumbnail = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cUSize = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cHash = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.bExtract = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(356, 410);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(437, 410);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "break";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.cThumbnail);
            this.objectListView1.AllColumns.Add(this.cName);
            this.objectListView1.AllColumns.Add(this.cUSize);
            this.objectListView1.AllColumns.Add(this.cHash);
            this.objectListView1.AllColumns.Add(this.olvColumn1);
            this.objectListView1.AllowColumnReorder = true;
            this.objectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView1.CellEditUseWholeCell = false;
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cThumbnail,
            this.cName,
            this.cUSize,
            this.cHash});
            this.objectListView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.objectListView1.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.HasCollapsibleGroups = false;
            this.objectListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.objectListView1.Location = new System.Drawing.Point(13, 13);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.SelectColumnsMenuStaysOpen = false;
            this.objectListView1.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.ModelDialog;
            this.objectListView1.ShowGroups = false;
            this.objectListView1.Size = new System.Drawing.Size(499, 391);
            this.objectListView1.TabIndex = 2;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.UseExplorerTheme = true;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.DoubleClick += new System.EventHandler(this.objectListView1_DoubleClick);
            // 
            // cThumbnail
            // 
            this.cThumbnail.IsEditable = false;
            this.cThumbnail.IsVisible = false;
            this.cThumbnail.Searchable = false;
            this.cThumbnail.ShowTextInHeader = false;
            this.cThumbnail.Sortable = false;
            this.cThumbnail.Text = "";
            this.cThumbnail.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cThumbnail.Width = 32;
            // 
            // cName
            // 
            this.cName.AspectName = "DisplayName";
            this.cName.FillsFreeSpace = true;
            this.cName.Text = "Name";
            this.cName.Width = 300;
            // 
            // cUSize
            // 
            this.cUSize.AspectName = "SizeUncompressedString";
            this.cUSize.AspectToStringFormat = "";
            this.cUSize.Text = "Size";
            this.cUSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cUSize.Width = 100;
            // 
            // cHash
            // 
            this.cHash.AspectName = "HashString";
            this.cHash.Text = "Hash";
            this.cHash.Width = 100;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "GUID";
            this.olvColumn1.Hideable = false;
            this.olvColumn1.IsEditable = false;
            this.olvColumn1.IsVisible = false;
            this.olvColumn1.Searchable = false;
            this.olvColumn1.ShowTextInHeader = false;
            this.olvColumn1.Sortable = false;
            this.olvColumn1.Text = "GUID";
            this.olvColumn1.UseFiltering = false;
            this.olvColumn1.Width = 0;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(24, 24);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // bExtract
            // 
            this.bExtract.Location = new System.Drawing.Point(13, 410);
            this.bExtract.Name = "bExtract";
            this.bExtract.Size = new System.Drawing.Size(75, 23);
            this.bExtract.TabIndex = 3;
            this.bExtract.Text = "Extract";
            this.bExtract.UseVisualStyleBackColor = true;
            this.bExtract.Click += new System.EventHandler(this.bExtract_Click);
            // 
            // PackRatUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 445);
            this.Controls.Add(this.bExtract);
            this.Controls.Add(this.objectListView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "PackRatUI";
            this.Text = "PackRatUI";
            this.Shown += new System.EventHandler(this.PackRat_Shown);
            this.Move += new System.EventHandler(this.PackRat_Move);
            this.Resize += new System.EventHandler(this.PackRat_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.ResumeLayout(false);

            }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private BrightIdeasSoftware.ObjectListView objectListView1;
        public BrightIdeasSoftware.OLVColumn cThumbnail;
        public BrightIdeasSoftware.OLVColumn cName;
        private BrightIdeasSoftware.OLVColumn cUSize;
        private BrightIdeasSoftware.OLVColumn cHash;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button bExtract;
        }
    }