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
		int		m_nowSelectID	= 0;
//		bool	m_enableNumric	= false;
		int		m_countDigit	= 0;
//		int		m_nowSerialNum	= 0;
		string	m_baseNmae		= "";

		int		m_goldWaveVer	= 5;

		string	m_preSaveName	= "";
		bool m_ratchetRetakeFlg	= false;

		public List<string[]>		m_serifList = new List<string[]>();

		int waitTime= 300;

		Form2 m_checkListDlg	= null;	
		Form3 m_voiceListDlg	= null;

		IntPtr goldWavehwnd		= IntPtr.Zero;
		

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

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindow(IntPtr hWnd);

		public static void ActiveWindowWithSetWindowPos(IntPtr hWnd)
		{
			SetWindowPos(hWnd, HWND_TOPMOST,	0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
			SetWindowPos(hWnd, HWND_NOTOPMOST,	0, 0, 0, 0,	SWP_SHOWWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd,
		int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

		private const int SWP_NOSIZE		= 0x0001;
		private const int SWP_NOMOVE		= 0x0002;
		private const int SWP_SHOWWINDOW	= 0x0040;

		private const int HWND_TOPMOST		= -1;
		private const int HWND_NOTOPMOST	= -2;

		private const int MOD_ALT			= 0x01;
		private const int MOD_CONTROL		= 0x02;
		private const int MOD_SHIFT			= 0x04;
		private const int MOD_WIN			= 0x08;
		private const int WM_HOTKEY			= 0x312;






		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);




		public short hotkeyID_D;
		public short hotkeyID_R;




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

		private const int INPUT_MOUSE = 0;				  // マウスイベント
		private const int INPUT_KEYBOARD = 1;			   // キーボードイベント
		private const int INPUT_HARDWARE = 2;			   // ハードウェアイベント

		private const int MOUSEEVENTF_MOVE = 0x1;		   // マウスを移動する
		private const int MOUSEEVENTF_ABSOLUTE = 0x8000;	// 絶対座標指定
		private const int MOUSEEVENTF_LEFTDOWN = 0x2;	   // 左　ボタンを押す
		private const int MOUSEEVENTF_LEFTUP = 0x4;		 // 左　ボタンを離す
		private const int MOUSEEVENTF_RIGHTDOWN = 0x8;	  // 右　ボタンを押す
		private const int MOUSEEVENTF_RIGHTUP = 0x10;	   // 右　ボタンを離す
		private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;	// 中央ボタンを押す
		private const int MOUSEEVENTF_MIDDLEUP = 0x40;	  // 中央ボタンを離す
		private const int MOUSEEVENTF_WHEEL = 0x800;		// ホイールを回転する
		private const int WHEEL_DELTA = 120;				// ホイール回転値

		private const int KEYEVENTF_KEYDOWN = 0x0;		  // キーを押す
		private const int KEYEVENTF_KEYUP = 0x2;			// キーを離す
		private const int KEYEVENTF_EXTENDEDKEY = 0x1;	  // 拡張コード
		private const int VK_SHIFT = 0x10;				  // SHIFTキー
		private const int VK_MENU = 0x12;				   // ALtキー
		private const int VK_RETURN = 0x0d;				  // Enterキー
		








		public List<string> subDir = new List<string>();

		public Form1()
		{
			InitializeComponent();

			this.MouseWheel += Form1_MouseWheel;
			textBox4.MouseWheel += Form1_MouseWheel;
		}

		private void 終了しますToolStripMenuItem_Click( object sender, EventArgs e )
		{
			Application.Exit();
		}



		/// <summary>
		/// goldwaveの操作実行。メイン機能
		/// </summary>
		public void Do()
		{
			toolStripStatusLabel1.Text = "";

			m_ratchetRetakeFlg = true;

			//ActiveWindowWithSetWindowPos(goldWavehwnd);
			SetForegroundWindow(goldWavehwnd);

			//System.Threading.Thread.Sleep(waitTime);
			System.Threading.Thread.Sleep(200);

			SendKeys.SendWait("%f");
			System.Threading.Thread.Sleep(100);
			SendKeys.SendWait("z");
			System.Threading.Thread.Sleep(100);
			SendKeys.SendWait("^V");
			System.Threading.Thread.Sleep(100);
			SendKeys.SendWait("~");

			System.Threading.Thread.Sleep(200);

			/*
			if (comboBox1.SelectedIndex == 0)
			{
				SendKeys.SendWait("%f");
				System.Threading.Thread.Sleep(waitTime);
				
				SendKeys.SendWait("z");
				System.Threading.Thread.Sleep(waitTime);
				
				SendKeys.SendWait("^V");
				
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




		/// <summary>
		/// goldwaveの操作実行。メイン機能
		/// </summary>
		public void Do_ver4()
		{
			toolStripStatusLabel1.Text = "";

			m_ratchetRetakeFlg = true;

			ActiveWindowWithSetWindowPos(goldWavehwnd);
			SetForegroundWindow(goldWavehwnd);

			/*
			System.Threading.Thread.Sleep(waitTime);

			if (comboBox1.SelectedIndex == 0)
			{
				SendKeys.SendWait("%f");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("{ENTER}");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.Send("^V");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("{ENTER}");
				System.Threading.Thread.Sleep(waitTime);
			}

			if (comboBox1.SelectedIndex == 1)
			{
				SendKeys.Send("%f");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}{DOWN}");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("{ENTER}");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.Send("^V");
				System.Threading.Thread.Sleep(waitTime);
				SendKeys.SendWait("{ENTER}");
				System.Threading.Thread.Sleep(waitTime);
			}

			if (comboBox1.SelectedIndex == 2)
			{
				

			}
			*/


		}









		/// <summary>
		/// F1F2 リテイクを発見！のときの処理
		/// </summary>
		private void RetakeDo()
		{
			bool ret = File.Exists(m_preSaveName + ".wav");

			if( ret )
			{ 
				int rNo = 1;
				string newName = "";

				while(true)
				{
					newName = m_preSaveName + $"_r{rNo}.wav";

					if ( File.Exists(newName) == false ) break;

					rNo++;

				}
				File.Move(m_preSaveName + ".wav", newName);
				if( m_ratchetRetakeFlg ) IncFileName(-1);
				m_ratchetRetakeFlg = false;
			}
			
		}


		private void Form1_Load( object sender, EventArgs e )
		{

			// ホットキーのために唯一無二のIDを生成する
			hotkeyID_D = GlobalAddAtom("GlobalHotKey_D " + this.GetHashCode().ToString());
			hotkeyID_R = GlobalAddAtom("GlobalHotKey_R " + this.GetHashCode().ToString());

			// Ctrl+Aキーを登録する
			//RegisterHotKey(this.Handle, hotkeyID,  0, Keys.D);
			RegisterHotKey(this.Handle, hotkeyID_D, MOD_CONTROL, Keys.D);
			RegisterHotKey(this.Handle, hotkeyID_R, MOD_CONTROL, Keys.R);

			//textBox1.Text = Clipboard.GetText();
			//comboBox1.SelectedIndex = 0;

			goldWavehwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TMainForm", null);

			Console.WriteLine(GC.GetTotalMemory(false));

			m_voiceListDlg = new Form3(this);
			m_voiceListDlg.Show();
			m_voiceListDlg.Top = this.Bottom;
			m_voiceListDlg.Left = this.Left;

		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if ( m.Msg == WM_HOTKEY )
			{
				// ホットキーが押されたときの処理
				//				Debug.WriteLine("Ctrl+A");

				short nowKey = (short)(m.WParam.ToInt32()&0x0000ffff);

				//メイン機能
				if( nowKey == hotkeyID_D)
				{ 
				//	if (m_enableNumric )
				//	{

						string copyFilename = textBox3.Text + textBox1.Text;
						Clipboard.SetText(copyFilename);

						m_preSaveName = copyFilename;

						if( m_goldWaveVer == 5 ) Do();
						else if( m_goldWaveVer == 4 ) Do_ver4();

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

					//}
				}

				//リテイク機能
				if ( nowKey == hotkeyID_R)
				{
					RetakeDo();
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
		
			string ret = Search_TakeCheck(textBox1.Text);

			if( ret != "" )	textBox4.Text = ret;

			int voiceID = Search_TakeCheckID(textBox1.Text);
			if( voiceID != -1 ) m_nowSelectID = voiceID;

			
		}

		private void IncFileName( int addNum = 1 )
		{
			
			m_nowSelectID += addNum;

			if( m_nowSelectID < 0 ) m_nowSelectID = 0;
			if (m_nowSelectID >= m_serifList.Count ) m_nowSelectID = m_serifList.Count-1;

			if (m_serifList.Count > m_nowSelectID && m_nowSelectID != -1)
			{
				textBox1.Text = m_serifList[m_nowSelectID][0];
				m_voiceListDlg.SelctListViewItem(m_nowSelectID);
			}
		}

		public void SelectVoiceID(int id)
		{
			m_nowSelectID = id;
			if (m_serifList.Count > m_nowSelectID && m_nowSelectID != -1)
			{
				textBox1.Text = m_serifList[m_nowSelectID][0];
			}
		}

		private void Form1_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
			{
				var tmpFontSize = textBox4.Font.Size;
				tmpFontSize += (e.Delta < 0 ? -1 : 1);
				textBox4.Font = new Font("メイリオ", tmpFontSize);
			}
			else {
				IncFileName( (e.Delta > 0 ?- 1:1) );
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			IncFileName(-1);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			IncFileName(1);
		}

		/*
		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			int result;
			if( int.TryParse(textBox2.Text, out result) == false ) return;
			waitTime = int.Parse(textBox2.Text);
		}
		*/

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
				"☆リテイク機能！ボイスを聞いたら保存済みセリフのリテイクがきたとき、Ctrl+Rを押すと、自動的に前に保存した音声に「_r1」と追加され、ファイル名の欄も一つ戻ります\n" +
				"\n" +

				"※ 自動保存が成功しない場合、オプションの項目、処理待ち時間を長めに設定してみください。");
		}



		private void Load_TakeCheck( string filePath )
		{
			m_serifList.Clear();
			string line;
			string serif;
			string voiceName;
			Regex regex = new Regex( "(.*?)\\t(.*)");

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

					serif = regex.Replace( line, "$2");
					voiceName = regex.Replace(line, "$1");
					m_serifList.Add( new string[2] { voiceName, serif} );

				}
			}
		}

		private string Search_TakeCheck( string voiceName)
		{
			string ret = "";

			foreach( var strData in m_serifList )
			{
				if( strData[0].IndexOf(voiceName) != -1 )
				{
					ret = strData[1];
					break;
				}
			}

			return ret;
		}

		private int Search_TakeCheckID(string voiceName)
		{
			int ret = 0;

			foreach (var strData in m_serifList)
			{
				if (strData[0].IndexOf(voiceName) != -1)
				{
					return ret;
				}
				ret++;
			}

			return -1;
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

			if(m_serifList.Count > 0 )
			{
				m_nowSelectID = 0;
				textBox1.Text = m_serifList[m_nowSelectID][0];
			}

			m_voiceListDlg.ListUpdate();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if(goldWavehwnd == IntPtr.Zero || IsWindow(goldWavehwnd) == false )
			{ 
				goldWavehwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TMainForm", "GoldWave");

				if (goldWavehwnd == IntPtr.Zero || IsWindow(goldWavehwnd) == false)
				{
					//.ForeColor = Color.Red;
					//toolStripStatusLabel1.Text = "GOLD WAVEが起動されていません。";

					if(goldWavehwnd == IntPtr.Zero || IsWindow(goldWavehwnd) == false )
					{ 
						goldWavehwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "OWL_Window", "GoldWave");

						if (goldWavehwnd == IntPtr.Zero || IsWindow(goldWavehwnd) == false)
						{
							toolStripStatusLabel1.ForeColor = Color.Red;
							toolStripStatusLabel1.Text = "GOLD WAVEが起動されていません。";
						}
						else
						{
							toolStripStatusLabel1.ForeColor = Color.Black;
							toolStripStatusLabel1.Text = "GOLD WAVE ver4 とリンクしました。";
							m_goldWaveVer = 4;
						}
					}
				}
				else
				{
					toolStripStatusLabel1.ForeColor = Color.Black;
					toolStripStatusLabel1.Text = "GOLD WAVE ver5 とリンクしました。";
					m_goldWaveVer = 5;
				}
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void button6_Click(object sender, EventArgs e)
		{
			CheckVoiceExist();
		}

		private void CheckVoiceExist()
		{
			if(m_serifList.Count == 0 )
			{
				System.Windows.Forms.MessageBox.Show("テイクチェックシートが指定されていないか、正しく読み込まれていません。\nテイクチェックシートの指定と内容を確認してください。");
				return;
			}
			string checkPath = "";
			List<int> noneIDList = new List<int>();
			int loopCount = m_serifList.Count();
			for (int i = 0; i < loopCount; i++)
			{
				checkPath = textBox3.Text + m_serifList[i][0] + ".wav";
				if (File.Exists(checkPath) == false)
				{
					noneIDList.Add(i);
				}
			}

			string showList = "";
			int tmpID;
			for (int i = 0; i < noneIDList.Count; i++)
			{
				tmpID = noneIDList[i];
				showList += m_serifList[tmpID][0] + "	" + m_serifList[tmpID][1] + Environment.NewLine;
			}
			
			if (showList != "") 
			{
				m_checkListDlg = new Form2( showList );
				
				m_checkListDlg.ShowDialog();
				m_checkListDlg.Dispose();
			}
			else
			{
				System.Windows.Forms.MessageBox.Show( "テイクチェックシートにあるボイスは全て保存先フォルダに存在します。(wav形式)");
			}
		}

		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			if( textBox3.Text.EndsWith(@"\") == false ) textBox3.Text += @"\";
		}
	}
 }
 

