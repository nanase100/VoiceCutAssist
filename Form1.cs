using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Runtime.InteropServices;

using System.IO;


namespace VoiceCutAssist
{
	public partial class Form1 : Form
	{

		bool	m_enableNumric = false;
		int		m_countDigit = 0;
		int		m_nowSerialNum = 0;
		string	m_baseNmae = "";

		List<string[]>		m_serifList = new List<string[]>();

		int waitTime= 0;

		IntPtr goldWavehwnd;

		// C#
		// Windows API functions and constants
		[DllImport("user32")]
		static extern int RegisterHotKey(IntPtr hwnd,int id, int fsModifiers, Keys vk) ;
		[DllImport("user32")]
		static extern int UnregisterHotKey(IntPtr hwnd,int id);
		[DllImport("kernel32",
		EntryPoint="GlobalAddAtomA")]
		static extern short GlobalAddAtom(string lpString);
		[DllImport("kernel32")]
		static extern short GlobalDeleteAtom(short nAtom);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDesktopWindow();
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		private static extern int VkKeyScan(char ch);

		public static void ActiveWindowWithSetWindowPos(IntPtr hWnd)
		{
			SetWindowPos(hWnd, HWND_TOPMOST,	0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
			SetWindowPos(hWnd, HWND_NOTOPMOST,	0, 0, 0, 0,	SWP_SHOWWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd,
		int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

		private const int SWP_NOSIZE = 0x0001;
		private const int SWP_NOMOVE = 0x0002;
		private const int SWP_SHOWWINDOW = 0x0040;

		private const int HWND_TOPMOST	= -1;
		private const int HWND_NOTOPMOST = -2;

		private const int MOD_ALT		= 0x01;
		private const int MOD_CONTROL	= 0x02;
		private const int MOD_SHIFT		= 0x04;
		private const int MOD_WIN		= 0x08;
		private const int WM_HOTKEY		= 0x312;






		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);




		public short hotkeyID;




		// マウスイベント(mouse_eventの引数と同様のデータ)
		[StructLayout(LayoutKind.Sequential)]
		private struct MOUSEINPUT
		{
			public int dx;
			public int dy;
			public int mouseData;
			public int dwFlags;
			public int time;
			public int dwExtraInfo;
		};

		// キーボードイベント(keybd_eventの引数と同様のデータ)
		[StructLayout(LayoutKind.Sequential)]
		private struct KEYBDINPUT
		{
			public short wVk;
			public short wScan;
			public int dwFlags;
			public int time;
			public int dwExtraInfo;
		};

		// ハードウェアイベント
		[StructLayout(LayoutKind.Sequential)]
		private struct HARDWAREINPUT
		{
			public int uMsg;
			public short wParamL;
			public short wParamH;
		};

		// 各種イベント(SendInputの引数データ)
		[StructLayout(LayoutKind.Explicit)]
		private struct INPUT
		{
			[FieldOffset(0)] public int type;
			[FieldOffset(4)] public MOUSEINPUT mi;
			[FieldOffset(4)] public KEYBDINPUT ki;
			[FieldOffset(4)] public HARDWAREINPUT hi;
		};

		// キー操作、マウス操作をシミュレート(擬似的に操作する)
		[DllImport("user32.dll")]
		private extern static void SendInput(
			int nInputs, ref INPUT pInputs, int cbsize);

		// 仮想キーコードをスキャンコードに変換
		[DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
		private extern static int MapVirtualKey(
			int wCode, int wMapType);

		private const int INPUT_MOUSE = 0;                  // マウスイベント
		private const int INPUT_KEYBOARD = 1;               // キーボードイベント
		private const int INPUT_HARDWARE = 2;               // ハードウェアイベント

		private const int MOUSEEVENTF_MOVE = 0x1;           // マウスを移動する
		private const int MOUSEEVENTF_ABSOLUTE = 0x8000;    // 絶対座標指定
		private const int MOUSEEVENTF_LEFTDOWN = 0x2;       // 左　ボタンを押す
		private const int MOUSEEVENTF_LEFTUP = 0x4;         // 左　ボタンを離す
		private const int MOUSEEVENTF_RIGHTDOWN = 0x8;      // 右　ボタンを押す
		private const int MOUSEEVENTF_RIGHTUP = 0x10;       // 右　ボタンを離す
		private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;    // 中央ボタンを押す
		private const int MOUSEEVENTF_MIDDLEUP = 0x40;      // 中央ボタンを離す
		private const int MOUSEEVENTF_WHEEL = 0x800;        // ホイールを回転する
		private const int WHEEL_DELTA = 120;                // ホイール回転値

		private const int KEYEVENTF_KEYDOWN = 0x0;          // キーを押す
		private const int KEYEVENTF_KEYUP = 0x2;            // キーを離す
		private const int KEYEVENTF_EXTENDEDKEY = 0x1;      // 拡張コード
		private const int VK_SHIFT = 0x10;                  // SHIFTキー
		private const int VK_MENU = 0x12;                  // ALtキー
		private const int VK_RETURN = 0x0d;                  // Enterキー
		








		public List<string> subDir = new List<string>();

		public Form1()
		{
			InitializeComponent();

			this.MouseWheel += Form1_MouseWheel;
		}

		private void 終了しますToolStripMenuItem_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}


		public void Do()
		{
			toolStripStatusLabel1.Text = "";
			

			goldWavehwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TMainForm", "GoldWave");

//			ActiveWindowWithSetWindowPos(goldWavehwnd);
			SetForegroundWindow(goldWavehwnd);

			System.Threading.Thread.Sleep(waitTime);

			if (comboBox1.SelectedIndex == 0)
			{
				SendKeys.SendWait("%f");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("z");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("^V");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("~");
				System.Threading.Thread.Sleep(waitTime);
			}

			if (comboBox1.SelectedIndex == 1)
			{
				SendKeys.Send("%f");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.Send("z");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.Send("^V");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.Send("~");
				System.Threading.Thread.Sleep(waitTime);
			}

			if (comboBox1.SelectedIndex == 2)
			{
				// キーボード操作実行用のデータ
				const int num = 10;
				INPUT[] inp = new INPUT[num];

				// (1)キーボード(alt)を押す
				inp[0].type = INPUT_KEYBOARD;
				inp[0].ki.wVk = VK_MENU;
				inp[0].ki.wScan = (short)MapVirtualKey(inp[0].ki.wVk, 0);
				inp[0].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN;
				inp[0].ki.dwExtraInfo = 0;
				inp[0].ki.time = 0;

				// (2)キーボード(f)を押す
				inp[1].type = INPUT_KEYBOARD;
				inp[1].ki.wVk = (short)Keys.F;
				inp[1].ki.wScan = (short)MapVirtualKey(inp[1].ki.wVk, 0);
				inp[1].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN;
				inp[1].ki.dwExtraInfo = 0;
				inp[1].ki.time = 0;

				// (3)キーボード(f)を離す
				inp[2].type = INPUT_KEYBOARD;
				inp[2].ki.wVk = (short)Keys.F;
				inp[2].ki.wScan = (short)MapVirtualKey(inp[2].ki.wVk, 0);
				inp[2].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
				inp[2].ki.dwExtraInfo = 0;
				inp[2].ki.time = 0;

				// (4)キーボード(alt)を離す
				inp[3].type = INPUT_KEYBOARD;
				inp[3].ki.wVk = VK_MENU;
				inp[3].ki.wScan = (short)MapVirtualKey(inp[3].ki.wVk, 0);
				inp[3].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
				inp[3].ki.dwExtraInfo = 0;
				inp[3].ki.time = 0;

				// (5)キーボード(z)を押す
				inp[4].type = INPUT_KEYBOARD;
				inp[4].ki.wVk = (short)Keys.Z;
				inp[4].ki.wScan = (short)MapVirtualKey(inp[1].ki.wVk, 0);
				inp[4].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN;
				inp[4].ki.dwExtraInfo = 0;
				inp[4].ki.time = 0;

				// (6)キーボード(z)を離す
				inp[5].type = INPUT_KEYBOARD;
				inp[5].ki.wVk = (short)Keys.Z;
				inp[5].ki.wScan = (short)MapVirtualKey(inp[2].ki.wVk, 0);
				inp[5].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
				inp[5].ki.dwExtraInfo = 0;
				inp[5].ki.time = 0;

				// (7)キーボード(V)を押す
				inp[6].type = INPUT_KEYBOARD;
				inp[6].ki.wVk = (short)Keys.V;
				inp[6].ki.wScan = (short)MapVirtualKey(inp[1].ki.wVk, 0);
				inp[6].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN;
				inp[6].ki.dwExtraInfo = 0;
				inp[6].ki.time = 0;

				// (8)キーボード(V)を離す
				inp[7].type = INPUT_KEYBOARD;
				inp[7].ki.wVk = (short)Keys.V;
				inp[7].ki.wScan = (short)MapVirtualKey(inp[2].ki.wVk, 0);
				inp[7].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
				inp[7].ki.dwExtraInfo = 0;
				inp[7].ki.time = 0;

				// (9)キーボード(enter)を押す
				inp[8].type = INPUT_KEYBOARD;
				inp[8].ki.wVk = (short)Keys.Enter;
				inp[8].ki.wScan = (short)MapVirtualKey(inp[1].ki.wVk, 0);
				inp[8].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYDOWN;
				inp[8].ki.dwExtraInfo = 0;
				inp[8].ki.time = 0;

				// (10)キーボード(V)を離す
				inp[9].type = INPUT_KEYBOARD;
				inp[9].ki.wVk = (short)Keys.Enter;
				inp[9].ki.wScan = (short)MapVirtualKey(inp[2].ki.wVk, 0);
				inp[9].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;
				inp[9].ki.dwExtraInfo = 0;
				inp[9].ki.time = 0;

				// キーボード操作実行
				SendInput(num, ref inp[0], Marshal.SizeOf(inp[0]));

				// 1000ミリ秒スリープ
				System.Threading.Thread.Sleep(1000);

			}
			/*
			if (comboBox1.SelectedIndex == 2)
			{
				PostMessage(goldWavehwnd, WM_SYSKEYDOWN, (int)Keys.Menu, 0);
				PostMessage(goldWavehwnd, WM_SYSKEYDOWN, (int)Keys.F, 0);

				System.Threading.Thread.Sleep(waitTime);

				PostMessage(goldWavehwnd, WM_SYSKEYUP, (int)Keys.F, 0);
				PostMessage(goldWavehwnd, WM_SYSKEYUP, (int)Keys.Menu, 0);

				System.Threading.Thread.Sleep(waitTime);

				PostMessage(goldWavehwnd, WM_KEYDOWN, (int)Keys.Z, 0);

				System.Threading.Thread.Sleep(waitTime);

				PostMessage(goldWavehwnd, WM_KEYUP, (int)Keys.Z, 0);

				System.Threading.Thread.Sleep(waitTime);

				PostMessage(goldWavehwnd, WM_KEYDOWN, (int)Keys.Return, 0);

				System.Threading.Thread.Sleep(waitTime);

				PostMessage(goldWavehwnd, WM_KEYUP, (int)Keys.Return, 0);			

			}
			/**/

		}



		private void Form1_Load( object sender, EventArgs e )
		{

			// ホットキーのために唯一無二のIDを生成する
			hotkeyID = GlobalAddAtom("GlobalHotKey " + this.GetHashCode().ToString());
			// Ctrl+Aキーを登録する
			//RegisterHotKey(this.Handle, hotkeyID,  0, Keys.D);
			RegisterHotKey(this.Handle, hotkeyID, MOD_CONTROL, Keys.D);

			//textBox1.Text = Clipboard.GetText();
			comboBox1.SelectedIndex = 0;

			goldWavehwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TMainForm", "GoldWave");

			Console.WriteLine(GC.GetTotalMemory(false));
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if ( m.Msg == WM_HOTKEY )
			{
				// ホットキーが押されたときの処理
				//				Debug.WriteLine("Ctrl+A");

				if (m_enableNumric )
				{

					string copyFilename = textBox3.Text + textBox1.Text;
					Clipboard.SetText(copyFilename);

					Do();

					System.Threading.Thread.Sleep(100);

					if ( Check_IsComplete(copyFilename) == false )
					{
						toolStripStatusLabel1.ForeColor = Color.Red;
						toolStripStatusLabel1.Text = "ファイルが保存されませんでした。再度、保存(Ctrl+D)の操作を行ってください。";
						return;
					}

					toolStripStatusLabel1.ForeColor = Color.Black;
					toolStripStatusLabel1.Text = "";
					IncFileName();

				}
			}
		}
	 
			

		private bool Check_IsComplete( string path )
		{
			bool ret = false;

			ret = File.Exists(path+".wav");


			return ret;
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

			Match mr = Regex.Match( textBox1.Text, @"(.*_)(\d+)\D*");

			m_enableNumric = mr.Success;

			if ( mr.Success )
			{
				textBox1.BackColor = Color.White;

				m_baseNmae = mr.Groups[1].ToString();

				
				m_countDigit = mr.Groups[2].ToString().Length;
				m_nowSerialNum = int.Parse(mr.Groups[2].ToString());
				
			}
			else
			{
				textBox1.BackColor = Color.Red;
			}


			textBox4.Text  = Search_TakeCheck( textBox1.Text );

		}

		private void IncFileName( int addNum = 1 )
		{
			textBox1.Text = m_baseNmae + (m_nowSerialNum + addNum).ToString().PadLeft(m_countDigit, '0');
		}

		private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
			if( e.Delta > 0 )	IncFileName(-1);
			else				IncFileName(1);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			IncFileName(-1);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			IncFileName(1);
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			int result;
			if( int.TryParse(textBox2.Text, out result) == false ) return;
			waitTime = int.Parse(textBox2.Text);
		}

		private void textBox3_DragDrop(object sender, DragEventArgs e)
		{
			//ドロップされたファイルの一覧を取得
			string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
			if (fileName.Length <= 0)
			{
				return;
			}


			var isDirectory = File.GetAttributes(fileName[0]).HasFlag(FileAttributes.Directory);

			if(isDirectory == false )
			{
				System.Windows.Forms.MessageBox.Show("フォルダをドラッグ・アンド・ドロップしてください。");
				return;
			}

			textBox3.Text = fileName[0]+@"\";
		}

		private void textBox3_DragEnter(object sender, DragEventArgs e)
		{
			//ファイルがドラッグされている場合、カーソルを変更する
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

        private void textBox3_Enter(object sender, EventArgs e)
        {
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "保存するフォルダ名を指定してください。保存先フォルダをドラッグアンドドロップもできます。";
		}

        private void textBox1_Enter(object sender, EventArgs e)
        {
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "保存するファイル名を入力してください。ファイル名の後ろに必ず \"_0001\" のような数字をふってください。桁数は自由です。";		}

        private void comboBox1_MouseEnter(object sender, EventArgs e)
        {
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "自動で保存するやり方を変更できます。PC環境によって最適なものは違ってきます。";
		}

        private void textBox2_MouseEnter(object sender, EventArgs e)
        {
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "自動で保存するときの待ち時間を設定します。高スペック:100前後、平均スペック:300、低スペックPCでは500～1000が目安です。";
		}

        private void textBox3_MouseEnter(object sender, EventArgs e)
        {
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "保存するフォルダ名を指定してください。保存先フォルダをドラッグアンドドロップもできます。";
		}

		private void textBox1_MouseEnter(object sender, EventArgs e)
        {
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "保存するファイル名を入力してください。ファイル名の後ろに必ず \"_0001\" のような数字をふってください。桁数は自由です。";		
		}

		private void textBox5_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "参考用に表示するテイクチェックシートのパスを入力してください。既存のファイルをドラッグアンドドロップもできます。";
		}

		private void textBox2_Enter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "自動で処理が進むときの間の待ち時間を指定します。うまく動作しないときは数字を少しずつ大きくしてみてください。";
		}


		private void textBox4_MouseEnter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "ボイスファイル名がテイクチェックシートに存在する場合、セリフが表示されます。";
		}

		private void comboBox2_Enter(object sender, EventArgs e)
		{
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "テイクチェックシートのtxtやcsvの文字コードに合わせてください。";
		}

		private void comboBox2_MouseHover(object sender, EventArgs e)
		{
			toolStripStatusLabel1.ForeColor = Color.Black;
			toolStripStatusLabel1.Text = "テイクチェックシートのtxtやcsvの文字コードに合わせてください。";
		}

		private void button3_Click(object sender, EventArgs e)
        {
			System.Windows.Forms.MessageBox.Show("○このツールは GOLD WAV での音切り作業を補助するツールです。\n\n" +
				"■1 このツール上の保存先フォルダやファイル名を設定します。必要ならオプション項目も調整します。\n"+
				"■2 GOLD WAV 上で切り取りたい範囲を選択した状態で、キーボードで【CTRL + D】を押します。\n"+
				"■3 ファイルが保存され、ファイル名の連番がひとつ進みます。\n"+
				"■4 あとは切り取りたい範囲を選ぶ→CTRL+Dを繰り返すだけで、どんどんボイスをカットしていくことができます。\n"+
				"\n"+
				"■＋α　テイクチェックシートを下部の欄に読み込ませておくと、ファイル名が一致するセリフ内容を表示できます。\n" +
				"\n" +
				"※ 自動保存が成功しない場合、オプションの項目、処理待ち時間を長めに設定してみください。");
        }



		private void Load_TakeCheck( string filePath )
        {
			m_serifList.Clear();
			string line;
			string serif;
			Regex regex = new Regex( ".*(「.*」).*");

			Match	result;
			System.Text.Encoding fileEncode;
			GetEncodeClass encCheck = new GetEncodeClass();
			if (File.Exists(filePath))
			{
				fileEncode = encCheck.GetEncoding(filePath);
			}
			else
            {
				return;
            }	

			using ( System.IO.StreamReader file = new System.IO.StreamReader(filePath, fileEncode) )
			{
				while ((line = file.ReadLine()) != null)
				{
					result = regex.Match(line);

					if( result.Success == false ) continue;

					serif = regex.Replace( line, "$1");
					m_serifList.Add( new string[2] { line, serif} );
					
				}
			}
		}

		private string Search_TakeCheck( string fileName )
        {
			string ret = "";

			foreach( var strData in m_serifList )
            {
				if( strData[0].IndexOf(fileName) != -1 )
                {
					ret = strData[1];
					break;
                }
            }


			return ret;
        }

        private void textBox5_DragEnter(object sender, DragEventArgs e)
        {
			//ファイルがドラッグされている場合、カーソルを変更する
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

        private void textBox5_DragDrop(object sender, DragEventArgs e)
        {
			//ドロップされたファイルの一覧を取得
			string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
			if (fileName.Length <= 0)
			{
				return;
			}

			var isDirectory = File.GetAttributes(fileName[0]).HasFlag(FileAttributes.Directory);

			if (isDirectory == true)
			{
				System.Windows.Forms.MessageBox.Show("ファイルをドラッグ・アンド・ドロップしてください。");
				return;
			}

			textBox5.Text = fileName[0];

		}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
			if( checkBox1.Checked == true )
            {
				groupBox2.Visible = false;
				groupBox3.Visible = false;

				this.MaximumSize = new Size(2560,220);
				this.MinimumSize = new Size(650, 220);
			}
            else
            {
				groupBox2.Visible = true;
				groupBox3.Visible = true;

				this.MaximumSize = new Size(2560,380);
				this.MinimumSize = new Size(650, 380);
			}
        }

        private void button4_Click(object sender, EventArgs e)
        {
			using (var ofd = new OpenFileDialog() { FileName = "SelectFolder", Filter = "Folder|.", CheckFileExists = false })
			{
				if (ofd.ShowDialog() == DialogResult.OK)
				{
					textBox3.Text =  Path.GetDirectoryName(ofd.FileName) + "\\";
				}
			}
		}

        private void button5_Click(object sender, EventArgs e)
        {
			using (var ofd = new OpenFileDialog() { FileName = "SelectFile", Filter = "すべてのファイル(*.*)|*.*", CheckFileExists = true })
			{
				if (ofd.ShowDialog() == DialogResult.OK)
				{
					textBox5.Text = ofd.FileName;
					
				}
			}
		}

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
			Load_TakeCheck(textBox5.Text);
		}

        
    }
 }
 

