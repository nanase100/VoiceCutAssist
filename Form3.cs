using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoiceCutAssist
{
	public partial class Form3: Form
	{
		private Form1 m_parent = null;


		public Form3(Form1 parent)
		{
			InitializeComponent();
			m_parent = parent;
		}
		
		public void ListUpdate()
		{
			listView1.Items.Clear();
			listView1.BeginUpdate();

			int index = 0;

			foreach ( var seri in m_parent.m_serifList )
			{
				var tmpItem = listView1.Items.Add( index.ToString() );
				tmpItem.UseItemStyleForSubItems = false;

				tmpItem.SubItems.Add(seri[0]);

				tmpItem.SubItems.Add(seri[1]);

				string checkStr = "";
				
				index++;
			}

			listView1.EndUpdate();


		}

		public void SetCheckText( int index, string text )
		{
			if( listView1.Items.Count <= index ) return;

			listView1.Items[index].SubItems[3].Text = text;
		}

		public void ListViewVisbleItem( int index )
		{
			if(listView1.Items.Count <= index ) return;
			listView1.EnsureVisible(index);
			
			listView1.Items[index].Selected = true;
		}

		private void Form3_Load(object sender, EventArgs e)
		{

		}

		public void SetListViewCheck(int index,bool check = true )
		{
			if(index >= listView1.Items.Count ) return;
			listView1.Items[index].Checked = check;
		}

		public bool GetListViewCheck( int index )
		{
			return listView1.Items[index].Checked;
		}

		private void Form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
		}

		private void listView1_Click(object sender, EventArgs e)
		{
			//if (listView1.SelectedItems.Count != 0) m_parent.SetSelectNo(listView1.SelectedItems[0].Index);
		}

		private void Form3_Load_1(object sender, EventArgs e)
		{

		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if( listView1.SelectedItems == null || listView1.SelectedItems.Count == 0 ) return;
			m_parent.SelectVoiceID( listView1.SelectedItems[0].Index );
		}

		public void SelctListViewItem(int index)
		{
			if (listView1.Items.Count <= index) return;
			listView1.Items[index].Selected = true;
		}
	}
}
