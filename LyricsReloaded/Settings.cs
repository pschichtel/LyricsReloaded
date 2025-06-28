/*
    Copyright 2013 Phillip Schichtel
	Created by Mayibongwe (2022)

    This file is part of LyricsReloaded.

    LyricsReloaded is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LyricsReloaded is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with LyricsReloaded. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace CubeIsland.LyricsReloaded
{
	public class Settings
	{
		private readonly string name;
        private readonly string state;
		private readonly string attribute;
		private readonly string path;
		
		private bool coBoxTriggered = false;
		private string coBoxValue;
		private bool cBoxTriggered = false;
		private bool cBoxValue;
   	
		public Settings(string name, string state, string attribute, string path)
		{
			this.name = name;
			this.state = state;
			this.attribute = attribute;
			this.path = path;
			addSetting();
		}
		
		public void addSetting()
		{	
			string setting = "<" + name + state + attribute + "/>" + Environment.NewLine;
			
			if (File.Exists(path))
			{
				string newFormat = "";
				string oldFormat = "(<showSource)>(.*?)</showSource>\\r\\n<showSourcePosition>(.*?)</showSourcePosition>";
				
				using (StreamReader reader = File.OpenText(path))
				{
					newFormat = reader.ReadToEnd();
				}
				
				if (Regex.IsMatch(newFormat, oldFormat) == true)
				{
					newFormat = Regex.Replace(newFormat, oldFormat, "$1 enabled=\"$2\" position=\"$3\"/>");
					File.WriteAllText(path, newFormat);
				}
				
				if (Regex.IsMatch(newFormat, "<" + name + ".*?/>") == false) File.AppendAllText(path, setting);
			}
			else File.AppendAllText(path, setting);
		}
		
		public bool IsSettingEnabled(string check)
		{	
			string enabled = "enabled=\"true\"";
			if (check == "position") enabled = "position=\"top\"";
			
			using (StreamReader reader = File.OpenText(path))
			{
				if (Regex.IsMatch(reader.ReadToEnd(), "<" + name + ".*?" + enabled + ".*?/>") == true) return true;
			}
			
			return false;
		}
		
		public void saveSettings()
		{
			if(cBoxTriggered == true)
			{
				saveState(cBoxValue);
				cBoxTriggered = false;
			}
			
			if(coBoxTriggered == true)
			{
				savePosition(coBoxValue);
				coBoxTriggered = false;
			}
		}
		
		public void saveState(bool check)
		{
			string temp = "";
		
			if(File.Exists(path))
			{
				using (StreamReader reader = File.OpenText(path))
				{
					temp = reader.ReadLine();
					
					if(check == true) temp = Regex.Replace(temp, "(?<=enabled=\")\\w+(?=\")", "true");
					else temp = Regex.Replace(temp, "(?<=enabled=\")\\w+(?=\")", "false");
				}
			}
				
			File.WriteAllText(path, temp + Environment.NewLine);
		}
		
		public void savePosition(string placement)
		{
			string temp = "";
		
			if(File.Exists(path))
			{
				using (StreamReader reader = File.OpenText(path))
				{
					temp = Regex.Replace(reader.ReadLine(), "(?<=position=\")\\w+(?=\")", placement);
				}
			}
				
			File.WriteAllText(path, temp + Environment.NewLine);
		}
		
		public void createPanel(IntPtr panelHandle, ComboBox coBoxSkinned)
		{        
			Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
		
			CheckBox cBox = new CheckBox();
			cBox.Text = "show source";
			cBox.AutoSize = true;
			cBox.Location = new Point(0,2);
			cBox.Checked = IsSettingEnabled("state");
			cBox.CheckedChanged += cBox_CheckedChanged;
			
			ComboBox coBox = coBoxSkinned;
			coBox.DropDownStyle = ComboBoxStyle.DropDownList;				
			coBox.Width = 100;
			coBox.Location = new Point((int)(TextRenderer.MeasureText(cBox.Text, cBox.Font).Width*1.8),0);
			coBox.Items.AddRange(new Object[] {"top", "bottom"});
			
			if(IsSettingEnabled("position") == true)	coBox.Text = "top";
			else coBox.Text = "bottom";
			
			coBox.SelectedIndex = coBox.FindString(coBox.Text);
			coBox.SelectionChangeCommitted += coBox_SelectionChange;
			if(cBox.Checked == false) coBox.Enabled = false;
			
			configPanel.Controls.AddRange(new Control[] {cBox, coBox});
		}
		
		private void cBox_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox cBox = (CheckBox)sender;
			Control panel = ((CheckBox)sender).Parent;
			
			cBoxTriggered = true;
			cBoxValue = cBox.Checked;
			
			foreach (Control control in panel.Controls)
			{
				if (control is ComboBox)
				{
					if(cBox.Checked == true) control.Enabled = true;
					else control.Enabled = false;
				}
			}
		}
		
		private void coBox_SelectionChange(object sender, EventArgs e)
		{
			ComboBox coBox = (ComboBox)sender;
			coBoxTriggered = true;
			coBoxValue = coBox.SelectedItem.ToString();
		}
	}
}