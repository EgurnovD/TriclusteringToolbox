namespace Ultimate_Triclustering
{
    partial class TriclustringToolboxForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tbContext = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btContext = new System.Windows.Forms.Button();
            this.btOutput = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbMethod = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbOptions = new System.Windows.Forms.TextBox();
            this.btStart = new System.Windows.Forms.Button();
            this.ckbParallel = new System.Windows.Forms.CheckBox();
            this.ckbFirstTriples = new System.Windows.Forms.CheckBox();
            this.lblFirstTriples = new System.Windows.Forms.Label();
            this.tbFirstTriples = new System.Windows.Forms.TextBox();
            this.ckbInjections = new System.Windows.Forms.CheckBox();
            this.lblDiff = new System.Windows.Forms.Label();
            this.tbDifference = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dgConstraints = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgConstraints)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "tctx";
            this.openFileDialog.FileName = "context";
            this.openFileDialog.Filter = "Triadic context files (*.tctx)|*.tctx|All files (*.*)|*.*";
            // 
            // tbContext
            // 
            this.tbContext.Location = new System.Drawing.Point(80, 7);
            this.tbContext.Name = "tbContext";
            this.tbContext.Size = new System.Drawing.Size(498, 20);
            this.tbContext.TabIndex = 11;
            this.tbContext.TextChanged += new System.EventHandler(this.tbContext_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Context file:";
            // 
            // btContext
            // 
            this.btContext.Location = new System.Drawing.Point(584, 4);
            this.btContext.Name = "btContext";
            this.btContext.Size = new System.Drawing.Size(75, 23);
            this.btContext.TabIndex = 1;
            this.btContext.Text = "Browse...";
            this.btContext.UseVisualStyleBackColor = true;
            this.btContext.Click += new System.EventHandler(this.btContext_Click);
            // 
            // btOutput
            // 
            this.btOutput.Location = new System.Drawing.Point(584, 30);
            this.btOutput.Name = "btOutput";
            this.btOutput.Size = new System.Drawing.Size(75, 23);
            this.btOutput.TabIndex = 2;
            this.btOutput.Text = "Browse...";
            this.btOutput.UseVisualStyleBackColor = true;
            this.btOutput.Click += new System.EventHandler(this.btOutput_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output folder:";
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(80, 32);
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.Size = new System.Drawing.Size(498, 20);
            this.tbOutput.TabIndex = 12;
            this.tbOutput.TextChanged += new System.EventHandler(this.tbOutput_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Triclustering method:";
            // 
            // cbMethod
            // 
            this.cbMethod.FormattingEnabled = true;
            this.cbMethod.Items.AddRange(new object[] {
            "NOAC",
            "KMeans",
            "DataPeeler",
            "OAC (box)",
            "OAC (prime)",
            "Spectral",
            "TBox",
            "Trias"});
            this.cbMethod.Location = new System.Drawing.Point(114, 85);
            this.cbMethod.Name = "cbMethod";
            this.cbMethod.Size = new System.Drawing.Size(122, 21);
            this.cbMethod.TabIndex = 7;
            this.cbMethod.SelectedIndexChanged += new System.EventHandler(this.cbMethod_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(242, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Options:";
            // 
            // tbOptions
            // 
            this.tbOptions.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbOptions.Location = new System.Drawing.Point(294, 85);
            this.tbOptions.Name = "tbOptions";
            this.tbOptions.Size = new System.Drawing.Size(254, 20);
            this.tbOptions.TabIndex = 8;
            this.tbOptions.Text = "0";
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(3, 248);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(656, 23);
            this.btStart.TabIndex = 0;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // ckbParallel
            // 
            this.ckbParallel.AutoSize = true;
            this.ckbParallel.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ckbParallel.Location = new System.Drawing.Point(554, 87);
            this.ckbParallel.Name = "ckbParallel";
            this.ckbParallel.Size = new System.Drawing.Size(105, 17);
            this.ckbParallel.TabIndex = 9;
            this.ckbParallel.Text = "Parallel algorithm";
            this.ckbParallel.UseVisualStyleBackColor = true;
            // 
            // ckbFirstTriples
            // 
            this.ckbFirstTriples.AutoSize = true;
            this.ckbFirstTriples.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ckbFirstTriples.Location = new System.Drawing.Point(1, 63);
            this.ckbFirstTriples.Name = "ckbFirstTriples";
            this.ckbFirstTriples.Size = new System.Drawing.Size(97, 17);
            this.ckbFirstTriples.TabIndex = 3;
            this.ckbFirstTriples.Text = "Limited context";
            this.ckbFirstTriples.UseVisualStyleBackColor = true;
            this.ckbFirstTriples.CheckedChanged += new System.EventHandler(this.ckbFirstTriples_CheckedChanged);
            // 
            // lblFirstTriples
            // 
            this.lblFirstTriples.AutoSize = true;
            this.lblFirstTriples.Enabled = false;
            this.lblFirstTriples.Location = new System.Drawing.Point(111, 64);
            this.lblFirstTriples.Name = "lblFirstTriples";
            this.lblFirstTriples.Size = new System.Drawing.Size(108, 13);
            this.lblFirstTriples.TabIndex = 18;
            this.lblFirstTriples.Text = "Number of first triples:";
            // 
            // tbFirstTriples
            // 
            this.tbFirstTriples.Enabled = false;
            this.tbFirstTriples.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbFirstTriples.Location = new System.Drawing.Point(225, 62);
            this.tbFirstTriples.Name = "tbFirstTriples";
            this.tbFirstTriples.Size = new System.Drawing.Size(102, 20);
            this.tbFirstTriples.TabIndex = 4;
            this.tbFirstTriples.TextChanged += new System.EventHandler(this.tbFirstTriples_TextChanged);
            // 
            // ckbInjections
            // 
            this.ckbInjections.AutoSize = true;
            this.ckbInjections.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.ckbInjections.Location = new System.Drawing.Point(333, 63);
            this.ckbInjections.Name = "ckbInjections";
            this.ckbInjections.Size = new System.Drawing.Size(130, 17);
            this.ckbInjections.TabIndex = 5;
            this.ckbInjections.Text = "Remove set injections";
            this.ckbInjections.UseVisualStyleBackColor = true;
            this.ckbInjections.CheckedChanged += new System.EventHandler(this.ckbInjections_CheckedChanged);
            // 
            // lblDiff
            // 
            this.lblDiff.AutoSize = true;
            this.lblDiff.Enabled = false;
            this.lblDiff.Location = new System.Drawing.Point(469, 64);
            this.lblDiff.Name = "lblDiff";
            this.lblDiff.Size = new System.Drawing.Size(119, 13);
            this.lblDiff.TabIndex = 21;
            this.lblDiff.Text = "Max. density difference:";
            // 
            // tbDifference
            // 
            this.tbDifference.Enabled = false;
            this.tbDifference.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbDifference.Location = new System.Drawing.Point(594, 62);
            this.tbDifference.Name = "tbDifference";
            this.tbDifference.Size = new System.Drawing.Size(65, 20);
            this.tbDifference.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 109);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "User constraints:";
            // 
            // dgConstraints
            // 
            this.dgConstraints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgConstraints.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgConstraints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgConstraints.Location = new System.Drawing.Point(6, 125);
            this.dgConstraints.Name = "dgConstraints";
            this.dgConstraints.Size = new System.Drawing.Size(653, 117);
            this.dgConstraints.TabIndex = 10;
            // 
            // TriclustringToolboxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 283);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dgConstraints);
            this.Controls.Add(this.tbDifference);
            this.Controls.Add(this.lblDiff);
            this.Controls.Add(this.ckbInjections);
            this.Controls.Add(this.tbFirstTriples);
            this.Controls.Add(this.lblFirstTriples);
            this.Controls.Add(this.ckbFirstTriples);
            this.Controls.Add(this.ckbParallel);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.tbOptions);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbMethod);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btOutput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.btContext);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbContext);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TriclustringToolboxForm";
            this.Text = "Triclustering Toolbox";
            ((System.ComponentModel.ISupportInitialize)(this.dgConstraints)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox tbContext;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btContext;
        private System.Windows.Forms.Button btOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbMethod;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbOptions;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.CheckBox ckbParallel;
        private System.Windows.Forms.CheckBox ckbFirstTriples;
        private System.Windows.Forms.Label lblFirstTriples;
        private System.Windows.Forms.TextBox tbFirstTriples;
        private System.Windows.Forms.CheckBox ckbInjections;
        private System.Windows.Forms.Label lblDiff;
        private System.Windows.Forms.TextBox tbDifference;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView dgConstraints;
    }
}

