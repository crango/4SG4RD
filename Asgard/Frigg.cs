﻿using Bogus;

using CefSharp;
using CefSharp.OffScreen;

using FontAwesome.Sharp;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Asgard
{
    public partial class Frigg : Form, IForm
    {
        public new IAesir Owner { get; set; }
        public new IChecker OChecker { get; set; }
        private string Bin { set; get; }
        private string Quatity { set; get; }
        private int UserId { set; get; }
        private string Token { set; get; }
        private int GateId { set; get; }
        private string FullName { set; get; }

        List<List<string>> CreditCards { set; get; }
        List<List<string>> LiveCreditCards { set; get; }
        List<List<string>> DieCreditCards { set; get; }
        List<List<string>> Valkyries { set; get; }
        List<List<string>> LiveValkyries { set; get; }

        #region RangeCreditCards
        private static readonly string[] rangeAmex = new[] { "34", "37" };//American Express
        private static readonly string[] rangeCUP = new[] { "62" };//China Union Pay
        private static readonly string[] rangeDCCB = new[] { "300-305" };//Diners Club Carte Blanche
        private static readonly string[] rangeDCI = new[] { "36", "38" };//Diners Club Int32ernational
        private static readonly string[] rangeDCUSC = new[] { "54", "55" };//Diners Club United States & Canada
        private static readonly string[] rangeDC = new[] { "6011", "622126-622925", "644-649", "65" };//Discover Card
        private static readonly string[] rangeInt32P = new[] { "636" };//Int32erPayment
        private static readonly string[] rangeInsP = new[] { "637-639" };//InstaPayment
        private static readonly string[] rangeJCB = new[] { "3528-3589" };//JCB
        private static readonly string[] rangeM = new[] { "50", "56-58", "6" };//Maestro
        private static readonly string[] rangeMC = new[] { "2221-2720", "51-55" };//Master Card
        private static readonly string[] rangeUATP = new[] { "1" };//UATP
        private static readonly string[] rangeV = new[] { "4" };//Visa
        #endregion

        private string cvvDigits = "###";

        private static ChromiumWebBrowser browser;


        private const string web = "www.wish.com";
        private const string initialUrl = "https://" + web;

        private Faker Faker { set; get; }

        private bool IconButtonGenerarClick = false;
        private bool ShowPanelValkyrie = false;
        private bool IconButtonValkyrieVerifyClick = false;
        private bool IconButtonGenerarStopClick = false;
        private Color IconButtonGenerarIconColor;
        private bool IconButtonGenerarEnabled;
        private bool forsetiError = false;

        private CancellationTokenSource tokenCancel;

        //CancellationToken token;

        //private bool requiredValkyrie = false;

        SoundPlayer Valhalla;
        SoundPlayer Valkyrie;

        private double OriginalCreditCardsCount;

        private static IFrame Iframe;
        private string sourceImageOriginal;

        public Frigg()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Faker = new Faker();
        }

        private void Frigg_Load(object sender, EventArgs e)
        {
            ControlsInit();
        }

        public void InitializeParameters(params object[] parameters)
        {
            if (parameters.Length == 4)
            {
                UserId = (int)parameters[0];
                Token = parameters[1].ToString();
                Owner = (Aesir)parameters[2];
                OChecker = (Checker)parameters[3];
            }
        }

        #region General
        #region Controls Events
        private void IconButtonBack_Click(object sender, EventArgs e)
        {
            Form form = Parent.Controls.OfType<Aesir>().FirstOrDefault();
            if (form == null)
            {
                Parent.Parent.Show();
                Parent.Parent.BringToFront();
                form.TopLevel = false;
                form.Dock = DockStyle.Fill;
                Parent.Parent.Controls.Add(form);
                Parent.Parent.Tag = form;
                form.BringToFront();
                form.Show();
            }
            else
            {
                form.BringToFront();
            }
        }

        private async void IconButtonClose_Click(object sender, EventArgs e)
        {
            PanelConfirm.Show();
            await IconButtonConfirm.OnClickAsync();
            Owner.HideIconActive("frigg");
            PanelConfirm.Hide();
            if (browser != null)
            {
                await browser.Kill(web);
            }
            this.Close();
        }

        #endregion
        #region Methods
        private void ControlsInit()
        {
            Yggdrasil.God = "frigg";
            GateId = 2;
            Bin = string.Empty;
            TextBoxBin.ContextMenuStrip = new ContextMenuStrip();
            TextBoxBin.Placeholder("######xxxxxxxxxx");
            MaskedTextBoxDate.ContextMenuStrip = new ContextMenuStrip();
            MaskedTextBoxDate.Placeholder("dd/aa");
            TextBoxCvv.ContextMenuStrip = new ContextMenuStrip();
            TextBoxCvv.Placeholder(cvvDigits);
            TextBoxCvv.MaxLength = 3;
            TextBoxQuantity.ContextMenuStrip = new ContextMenuStrip();

            IconButtonGenerar.IconColor = Color.Black;
            IconButtonGenerar.Enabled = false;
            IconButtonGenerarStop.Hide();
            IconButtonVerify.IconColor = Color.Black;
            IconButtonVerify.Enabled = false;
            IconButtonStop.IconColor = Color.Black;
            IconButtonStop.Enabled = false; ;
            IconButtonClear.IconColor = Color.Black;
            IconButtonClear.Enabled = false;
            IconButtonValkyrieClear.IconColor = Color.Black;
            IconButtonValkyrieClear.Enabled = false;
            IconButtonValkyrieStart.IconColor = Color.Black;
            IconButtonValkyrieStart.Enabled = false;
            IconButtonValkyrieVerify.IconColor = Color.Black;
            IconButtonValkyrieVerify.Enabled = false;
            IconButtonValkyrieStop.Hide();
            PanelConfirm.Hide();
            CircularProgressBarGeneral.Minimum = 0;
            CircularProgressBarGeneral.Maximum = 100;
            pictureBox1.Hide();
            pictureBox2.Hide();
            pictureBox3.Hide();
            pictureBox4.Hide();
            pictureBox5.Hide();
            pictureBox6.Hide();
            PanelValkyrie.Hide();
            PanelValkyrieLive.Hide();
            PanelValkyrieDie.Hide();
            PanelBlockGate.Hide();
            PanelReCaptcha.Hide();
            PanelReCaptcha3x3.Hide();
            PanelReCaptcha4x4.Hide();
            CircularProgressBarGeneral.Value = 0;
            CircularProgressBarDetail.Value = 0;
            Valhalla = new SoundPlayer(@"Valhalla.wav");
            Valkyrie = new SoundPlayer(@"Valkyrie.wav");
        }
        #endregion
        #endregion

        #region Checker
        #region Panel Generator
        #region Panel Events
        private void PanelGenerator_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxBin = new Rectangle(TextBoxBin.Location.X, TextBoxBin.Location.Y, TextBoxBin.ClientSize.Width, TextBoxBin.ClientSize.Height);
            borderTextBoxBin.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxBin, Color.White, ButtonBorderStyle.Solid);

            Rectangle borderMaskedTextBoxDate = new Rectangle(MaskedTextBoxDate.Location.X, MaskedTextBoxDate.Location.Y, MaskedTextBoxDate.ClientSize.Width, MaskedTextBoxDate.ClientSize.Height);
            borderMaskedTextBoxDate.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, borderMaskedTextBoxDate, Color.White, ButtonBorderStyle.Solid);

            Rectangle borderTextBoxCvv = new Rectangle(TextBoxCvv.Location.X, TextBoxCvv.Location.Y, TextBoxCvv.ClientSize.Width, TextBoxCvv.ClientSize.Height);
            borderTextBoxCvv.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxCvv, Color.White, ButtonBorderStyle.Solid);

            Rectangle borderTextBoxQuantity = new Rectangle(TextBoxQuantity.Location.X, TextBoxQuantity.Location.Y, TextBoxQuantity.ClientSize.Width, TextBoxQuantity.ClientSize.Height);
            borderTextBoxQuantity.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxQuantity, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion
        #region Controls Events
        #region TextBoxBin Events
        private void TextBoxBin_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Placeholder("######xxxxxxxxxx");
        }
        private void TextBoxBin_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Placeholder("######xxxxxxxxxx");
            Bin = textBox.Text;
            CompleteBin(textBox);
        }
        private void TextBoxBin_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Bin == string.Empty && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
                {
                    e.SuppressKeyPress = true;
                    return;
                }
            }
            catch (Exception) { }
        }
        private void TextBoxBin_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (Bin == string.Empty && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0))
                {
                    e.SuppressKeyPress = true;
                    return;
                }

                if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                {
                    Bin += (char)e.KeyValue;
                }
                else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                {
                    Bin += Convert.ToChar(e.KeyCode - 48);
                }
                else if (e.KeyCode == Keys.Back)
                {
                    int startIndex = Bin.Length - 1;
                    if (startIndex >= 0)
                    {
                        Bin = Bin.Remove(startIndex);
                        Task.Run(() => InfoBinList());
                    }
                    else
                    {
                        e.SuppressKeyPress = true;
                        return;
                    }
                }
                else if (e.KeyCode == Keys.Tab && Bin.Length < 6)
                {
                    e.SuppressKeyPress = true;
                    return;
                }
                else if (e.KeyCode == Keys.V && e.Control)
                {
                    e.SuppressKeyPress = false;
                }
                else
                {
                    e.SuppressKeyPress = true;
                    return;
                }
                Task.Run(() => InfoBinList(Bin));
            }
            catch (Exception) { }
        }
        private void TextBoxBin_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Bin = textBox.Text;
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                int textLenght = textBox.TextLength;
                for (int i = 0; i < textLenght; i++)
                {
                    if (i < 6)
                    {
                        if (Regex.IsMatch(textBox.Text.Substring(i, 1), "[^0-9]"))
                        {
                            textBox.Clear();
                            break;
                        }
                    }
                    else if (Regex.IsMatch(textBox.Text.Substring(i, 1), @"[^x-xX-X0-9]"))
                    {
                        textBox.Clear();
                        break;
                    }
                }
                CompleteBin(textBox);
                Bin = textBox.Text;
                Task.Run(() => InfoBinList(Bin));
            }
        }
        private void TextBoxBin_Validating(object sender, CancelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;
            if (text == "######xxxxxxxxxx")
            {
                text = string.Empty;
            }

            if (string.IsNullOrEmpty(text))
            {
                IconButtonGenerar.IconColor = Color.Black;
                IconButtonGenerar.Enabled = false;
            }
            else
            {
                if (text.Length < 6)
                {
                    IconButtonGenerar.IconColor = Color.Black;
                    IconButtonGenerar.Enabled = false;
                }
                else
                {
                    if (!ValidateBin(text))
                    {
                        IconButtonGenerar.IconColor = Color.Black;
                        IconButtonGenerar.Enabled = false;
                    }
                    else
                    {
                        IconButtonGenerar.IconColor = Color.White;
                        IconButtonGenerar.Enabled = true;
                        IconButtonClear.IconColor = Color.White;
                        IconButtonClear.Enabled = true;

                    }
                }
            }
        }
        #endregion
        #region MaskedBoxDate Events
        private void MaskedTextBoxDate_Enter(object sender, EventArgs e)
        {
            MaskedTextBox maskedTextBox = (MaskedTextBox)sender;
            maskedTextBox.Placeholder("dd/aa");
            MaskedTextBoxDate.Mask = "##/##";
            string text = maskedTextBox.Text.Replace("/", String.Empty).Trim();
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                if (text.Length > 2)
                {
                    maskedTextBox.Select(text.Length + 1, 0);
                }
                else
                {
                    maskedTextBox.Select(text.Length, 0);
                }
            });
        }
        private void MaskedTextBoxDate_Leave(object sender, EventArgs e)
        {
            MaskedTextBox maskedTextBox = (MaskedTextBox)sender;
            string text = maskedTextBox.Text.Replace("/", String.Empty).Trim();
            if (string.IsNullOrEmpty(text) || text.Length < 4)
            {
                maskedTextBox.Clear();
                MaskedTextBoxDate.Mask = "";
                maskedTextBox.Placeholder("dd/aa");
                maskedTextBox.Select(0, 0);
            }

        }
        private void MaskedTextBoxDate_KeyUp(object sender, KeyEventArgs e)
        {
            MaskedTextBox maskedTextBox = (MaskedTextBox)sender;
            string text = maskedTextBox.Text.Replace("/", String.Empty).Trim();
            if (text.Length > 0)
            {
                if (text.Length == 1)
                {
                    if (Regex.IsMatch(text, @"([^0-1])$"))
                    {
                        maskedTextBox.Text = maskedTextBox.Text.Remove(text.Length - 1);
                    }
                }
                else if (text.Length == 2)
                {
                    if (Regex.IsMatch(text, @"^(0[^1-9]|1[^0-2])$"))
                    {
                        maskedTextBox.Text = maskedTextBox.Text.Remove(text.Length - 1);
                    }
                }
                else if (text.Length == 3)
                {
                    if (Regex.IsMatch(text, @"([^2-2])$"))
                    {
                        maskedTextBox.Text = maskedTextBox.Text.Remove(text.Length - 1);
                    }
                }
                else if (text.Length == 4)
                {
                    if (Regex.IsMatch(text, @"([^0-9])$"))
                    {
                        maskedTextBox.Text = maskedTextBox.Text.Remove(text.Length - 1);
                    }
                }
            }
        }
        private void MaskedTextBoxDate_TextChanged(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                MaskedTextBox maskedTextBox = (MaskedTextBox)sender;
                string text = maskedTextBox.Text.Replace("/", String.Empty).Trim();
                if (text.Length > 0)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (i == 0)
                        {
                            if (Regex.IsMatch(text.Substring(i, 1), @"([^0-1])$"))
                            {
                                maskedTextBox.Clear();
                                break;
                            }
                        }
                        else if (i == 1)
                        {
                            if (Regex.IsMatch(text.Substring(i, 1), @"^(0[^1-9]|1[^0-2])$"))
                            {
                                maskedTextBox.Clear();
                                break;
                            }
                        }
                        else if (i == 2)
                        {
                            if (Regex.IsMatch(text.Substring(i, 1), @"([^2-2])$"))
                            {
                                maskedTextBox.Clear();
                                break;
                            }
                        }
                        else if (i == 3)
                        {
                            if (Regex.IsMatch(text.Substring(i, 1), @"([^0-9])$"))
                            {
                                maskedTextBox.Clear();
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region TxtBoxCvv Events
        private void TextBoxCvv_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Placeholder(cvvDigits);
        }
        private void TextBoxCvv_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.TextLength < cvvDigits.Length)
            {
                textBox.Clear();
            }
            textBox.Placeholder(cvvDigits);
        }
        private void TextBoxCvv_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (Regex.IsMatch(textBox.Text, "[^0-9]"))
            {
                textBox.Text = textBox.Text.Remove(textBox.TextLength - 1);
            }
        }
        private void TextBoxCvv_TextChanged(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                TextBox textBox = (TextBox)sender;
                if (Regex.IsMatch(textBox.Text, "[^0-9]"))
                {
                    textBox.Clear();
                    textBox.Select(0, 0);
                }
            }
        }
        #endregion
        #region TextBoxQuantity Events
        private void TextBoxQuatity_TextChanged(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                TextBox textBox = (TextBox)sender;
                if (Regex.IsMatch(textBox.Text, "[^0-9]"))
                {
                    textBox.Clear();
                    textBox.Select(0, 0);
                }
            }
        }
        private void TextBoxQuatity_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (Regex.IsMatch(textBox.Text, "[^0-9]"))
            {
                textBox.Text = textBox.Text.Remove(textBox.TextLength - 1);
            }
        }
        private void TextBoxQuatity_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == string.Empty)
            {
                textBox.Text = "10";
            }
        }
        #endregion
        #region IconButtonGenerar Events
        private async void IconButtonGenerar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateChildren(ValidationConstraints.Enabled))
                {
                    IconButton iconButton = (IconButton)sender;
                    iconButton.IconColor = Color.Black;
                    iconButton.Enabled = false;
                    IconButtonVerify.IconColor = Color.Black;
                    IconButtonVerify.Enabled = false;
                    IconButtonStop.IconColor = Color.Black;
                    IconButtonStop.Enabled = false;
                    IconButtonClear.IconColor = Color.Black;
                    IconButtonClear.Enabled = false;
                    IconButtonValkyrie.IconColor = Color.Black;
                    IconButtonValkyrie.Enabled = false;
                    IconButtonGenerarStop.Show();
                    IconButtonGenerarClick = true;
                    await ConsoleProgressGeneral("Iniciando Motor del Generador.", 0);
                    await GenerateCreditCards();
                    await ConsoleProgressGeneral("Material de pago generado correctamente.", 100, "Success");
                    await ConsoleProgressGeneral("FRIGG REINA DE LOS ÆSIR.", 0);
                    IconButtonGenerarStop.Hide();
                    iconButton.IconColor = Color.White;
                    iconButton.Enabled = true;
                    IconButtonVerify.IconColor = Color.White;
                    IconButtonVerify.Enabled = true;
                    IconButtonClear.IconColor = Color.White;
                    IconButtonClear.Enabled = true;
                    IconButtonValkyrie.IconColor = Color.White;
                    IconButtonValkyrie.Enabled = true;
                    IconButtonGenerarClick = false;
                }
            }
            catch (Exception) { }
        }
        #endregion
        #region IconButtonGenerarStop Events
        private void IconButtonGenerarStop_Click(object sender, EventArgs e)
        {
            if (tokenCancel != null)
            {
                IconButton iconbutton = (IconButton)sender;
                iconbutton.IconColor = Color.Black;
                iconbutton.Enabled = false;
                IconButtonGenerarStopClick = true;
                tokenCancel.Cancel();
                tokenCancel.Dispose();
                tokenCancel = null;
                //IconButtonGenerar.IconColor = Color.White;
                //IconButtonGenerar.Enabled = true;
                //if(CreditCards != null)
                //{
                //    if(CreditCards.Count > 0)
                //    {
                //        IconButtonVerify.IconColor = Color.White;
                //        IconButtonVerify.Enabled = true;
                //    }
                //}
            }
        }
        #endregion
        #endregion
        #region Validations
        private bool ValidateBin(string text)
        {
            try
            {
                RangeAndLength[] range = AllCards();
                int rangeLength = range.Length;
                for (int j = 0; j < rangeLength; j++)
                {
                    string pattern = @"\b" + range[j].Range;
                    Match match = Regex.Match(text, pattern);
                    if (match.Success)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception) { }
            return false;
        }

        #endregion
        #region Methods
        private void CompleteBin(TextBox textBox)
        {
            if (textBox.TextLength >= 6)
            {

                int textLenght = textBox.TextLength;
                int cardLenght = 0;
                Dictionary<int, int> precision = new Dictionary<int, int>();
                RangeAndLength[] range = AllCards();
                int rangeLength = range.Length;
                for (int j = 0; j < rangeLength; j++)
                {
                    string pattern = @"\b" + range[j].Range;
                    Match match = Regex.Match(textBox.Text, pattern);
                    if (match.Success)
                    {
                        precision.Add(j, int.Parse(match.Value));
                    }
                }

                if (precision.Count > 0)
                {
                    RangeAndLength card = range[precision.OrderByDescending(x => x.Value).First().Key];
                    cardLenght = card.Length;
                    textBox.MaxLength = cardLenght;

                    if (card.Range == "34" || card.Range == "37")
                    {
                        cvvDigits = "####";
                        TextBoxCvv.MaxLength = 4;
                    }
                    else
                    {
                        cvvDigits = "###";
                        TextBoxCvv.MaxLength = 3;
                    }

                    if (textLenght >= 6 && textLenght < cardLenght)
                    {
                        int limit = cardLenght - textLenght;
                        string ext = "";
                        for (int i = 0; i < limit; i++)
                        {
                            ext += "x";
                        }
                        textBox.Text += ext;
                    }
                }
                else
                {
                    cvvDigits = "###";
                    TextBoxCvv.MaxLength = 3;
                }

                textBox.Select(textBox.TextLength, 0);
                TextBoxCvv.ForeColor = Color.Silver;
                TextBoxCvv.Text = cvvDigits;
            }
        }
        #endregion
        #endregion
        #region Panel Console
        #region Methods
        public async Task ConsoleProgressGeneral(string info, int value, string status = null)
        {
            try
            {
                LabelInfoGeneralConsole.ForeColor = Color.White;
                int delay = 100;
                if (status == "Success")
                {
                    LabelInfoGeneralConsole.ForeColor = Color.FromArgb(17, 97, 238);
                    delay = 3000;
                }
                else if (status == "Fail")
                {
                    LabelInfoGeneralConsole.ForeColor = Color.FromArgb(255, 127, 0);
                    delay = 5000;
                }

                if (value < 100)
                {
                    CircularProgressBarGeneral.Value = value;
                    CircularProgressBarGeneral.Text = CircularProgressBarGeneral.Value.ToString();
                }
                await Task.Delay(100);
                LabelInfoGeneralConsole.Text = info;
                await Task.Delay(delay);
            }
            catch (Exception) {/* throw;*/ }
        }

        public async Task ConsoleProgressDetail(string info, int value, string status = null)
        {
            try
            {
                LabelInfoGeneralConsole.ForeColor = Color.White;
                int delay = 10;
                if (status == "Success")
                {
                    LabelInfoGeneralConsole.ForeColor = Color.FromArgb(17, 97, 238);
                    delay = 3000;
                }
                else if (status == "Fail")
                {
                    LabelInfoGeneralConsole.ForeColor = Color.FromArgb(255, 127, 0);
                    delay = 5000;
                }

                if (value < 100)
                {
                    CircularProgressBarDetail.Value = value;
                    CircularProgressBarDetail.Text = CircularProgressBarDetail.Value.ToString();
                }
                await Task.Delay(100);

                LabelInfoGeneralConsole.Text = info;
                await Task.Delay(delay);
            }
            catch (Exception) { }
        }
        #endregion
        #endregion
        #region Panel Information
        #region Cotrols Events
        private async void IconButtonValkyrie_Click(object sender, EventArgs e)
        {
            if (ShowPanelValkyrie)
            {
                IconButtonGenerar.IconColor = IconButtonGenerarIconColor;
                IconButtonGenerar.Enabled = IconButtonGenerarEnabled;
                PanelValkyrie.Hide();
                PanelValkyrieLive.Hide();
                PanelValkyrieDie.Hide();
                ShowPanelValkyrie = false;
                await ConsoleProgressGeneral("FRIGG REINA DE LOS ÆSIR.", 0);
                await ConsoleProgressDetail("FRIGG REINA DE LOS ÆSIR.", 0);
            }
            else
            {
                IconButtonGenerarIconColor = IconButtonGenerar.IconColor;
                IconButtonGenerarEnabled = IconButtonGenerar.Enabled;

                if (IconButtonGenerarEnabled)
                {
                    IconButtonGenerar.IconColor = Color.Black;
                    IconButtonGenerar.Enabled = false;
                }
                PanelValkyrie.Show();
                PanelValkyrieLive.Show();
                PanelValkyrieDie.Show();
                ShowPanelValkyrie = true;
                await ConsoleProgressGeneral("ValquiriaS (PROXYS)", 0);
                await ConsoleProgressDetail("ValquiriaS (PROXYS)", 0);
            }
        }
        #endregion
        #region Methods
        private async Task InfoBinList(string bin = null)
        {
            try
            {
                if (bin != null)
                {
                    if (bin.Length >= 8)
                    {
                        bin = bin.Substring(0, 8);

                        pictureBox1.Show();
                        pictureBox2.Show();
                        pictureBox3.Show();
                        pictureBox4.Show();
                        pictureBox5.Show();
                        pictureBox6.Show();

                        dynamic binList = await Asatru.InfoBin(bin);
                        if (binList != null)
                        {
                            pictureBox1.Hide();
                            pictureBox2.Hide();
                            pictureBox3.Hide();
                            pictureBox4.Hide();
                            pictureBox5.Hide();
                            pictureBox6.Hide();
                            LabelGetBin.Text = bin;
                            LabelGetSchema.Text = binList.scheme ?? "-";
                            LabelGetType.Text = binList.type ?? "-";
                            LabelGetBrand.Text = binList.brand ?? "-";
                            LabelGetBank.Text = binList.bank.name ?? "-";
                            LabelGetCountry.Text = binList.country.name ?? "-";
                        }
                        else
                        {
                            pictureBox1.Hide();
                            pictureBox2.Hide();
                            pictureBox3.Hide();
                            pictureBox4.Hide();
                            pictureBox5.Hide();
                            pictureBox6.Hide();
                            LabelGetBin.Text = bin;
                            LabelGetSchema.Text = "-";
                            LabelGetType.Text = "-";
                            LabelGetBrand.Text = "-";
                            LabelGetBank.Text = "-";
                            LabelGetCountry.Text = "-";
                        }
                    }
                    else
                    {
                        pictureBox1.Hide();
                        pictureBox2.Hide();
                        pictureBox3.Hide();
                        pictureBox4.Hide();
                        pictureBox5.Hide();
                        pictureBox6.Hide();
                        LabelGetBin.Text = bin;
                        LabelGetSchema.Text = "-";
                        LabelGetType.Text = "-";
                        LabelGetBrand.Text = "-";
                        LabelGetBank.Text = "-";
                        LabelGetCountry.Text = "-";
                    }
                }
            }
            catch (Exception) { }
        }
        #endregion
        #endregion
        #region Panel Credit Cards
        #region Panel Events
        private void PanelCreditCards_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxCreditCards = new Rectangle(TextBoxCreditCards.Location.X, TextBoxCreditCards.Location.Y, TextBoxCreditCards.ClientSize.Width + 17, TextBoxCreditCards.ClientSize.Height);
            borderTextBoxCreditCards.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxCreditCards, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion
        #region Controls Events
        private void TextBoxCreditCards_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            try
            {
                LabelCountCreditCards.Text = textBox.Lines.Count().ToString();
                string creditCards = textBox.Text.Trim();
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (creditCards.Contains("\r\n"))
                    {
                        string[] listCreditCards = creditCards.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        int listcreditCardsLength = listCreditCards.Length;
                        CreditCards = new List<List<string>>();
                        for (int i = 0; i < listcreditCardsLength; i++)
                        {
                            string[] listCreditCard = listCreditCards[i].Split('|');
                            CreditCards.Add(new List<string>());
                            CreditCards[i].Add(listCreditCard[0]);
                            CreditCards[i].Add(listCreditCard[1]);
                            CreditCards[i].Add(listCreditCard[2]);
                            CreditCards[i].Add(listCreditCard[3]);
                        }
                        if (!IconButtonGenerarClick)
                        {
                            IconButtonVerify.IconColor = Color.White;
                            IconButtonVerify.Enabled = true;
                            IconButtonClear.IconColor = Color.White;
                            IconButtonClear.Enabled = true;
                        }
                        Task.Run(() => InfoBinList(CreditCards[0][0]));
                    }
                    else if (creditCards.Contains("|"))
                    {
                        CreditCards = new List<List<string>>();
                        string[] listCreditCard = creditCards.Split('|');
                        CreditCards.Add(new List<string>());
                        CreditCards[0].Add(listCreditCard[0]);
                        CreditCards[0].Add(listCreditCard[1]);
                        CreditCards[0].Add(listCreditCard[2]);
                        CreditCards[0].Add(listCreditCard[3]);
                        if (!IconButtonGenerarClick)
                        {
                            IconButtonVerify.IconColor = Color.White;
                            IconButtonVerify.Enabled = true;
                            IconButtonClear.IconColor = Color.White;
                            IconButtonClear.Enabled = true;
                        }
                        Task.Run(() => InfoBinList(CreditCards[0][0]));
                    }
                    else
                    {
                        textBox.Text = string.Empty;
                        IconButtonVerify.IconColor = Color.Black;
                        IconButtonVerify.Enabled = false;
                        IconButtonClear.IconColor = Color.Black;
                        IconButtonClear.Enabled = false;
                    }
                }
                else
                {
                    if (creditCards == string.Empty)
                    {
                        textBox.Text = string.Empty;
                        IconButtonVerify.IconColor = Color.Black;
                        IconButtonVerify.Enabled = false;
                        if (TextBoxValhalla.Text == string.Empty && TextBoxHelheim.Text == string.Empty && IconButtonGenerar.Enabled == false)
                        {
                            IconButtonClear.IconColor = Color.Black;
                            IconButtonClear.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                textBox.Text = string.Empty;
                IconButtonVerify.IconColor = Color.Black;
                IconButtonVerify.Enabled = false;
                IconButtonClear.IconColor = Color.Black;
                IconButtonClear.Enabled = false;
            }
        }
        private async void IconButtonVerify_Click(object sender, EventArgs e)
        {
            try
            {
                IconButtonGenerarIconColor = IconButtonGenerar.IconColor;
                IconButtonGenerarEnabled = IconButtonGenerar.Enabled;
                IconButton iconButton = (IconButton)sender;
                iconButton.IconColor = Color.Black;
                iconButton.Enabled = false;
                IconButtonGenerar.IconColor = Color.Black;
                IconButtonGenerar.Enabled = false;
                IconButtonClear.IconColor = Color.Black;
                IconButtonClear.Enabled = false;
                IconButtonValkyrie.IconColor = Color.Black;
                IconButtonValkyrie.Enabled = false;
                IconButtonStop.IconColor = Color.White;
                IconButtonStop.Enabled = true;
                //running = true;
                OriginalCreditCardsCount = CreditCards.Count;
                await InvokeFrigg();
                await ConsoleProgressGeneral("FRIGG REINA DE LOS ÆSIR.", 0);
                await ConsoleProgressDetail("FRIGG REINA DE LOS ÆSIR.", 0);
                IconButtonClear.IconColor = Color.White;
                IconButtonClear.Enabled = true;
                IconButtonValkyrie.IconColor = Color.White;
                IconButtonValkyrie.Enabled = true;
            }
            catch (Exception) { }
        }
        #endregion
        #endregion
        #region Panel Valhalla
        #region Panel Events
        private void PanelValhalla_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxValhalla = new Rectangle(TextBoxValhalla.Location.X, TextBoxValhalla.Location.Y, TextBoxValhalla.ClientSize.Width + 17, TextBoxValhalla.ClientSize.Height);
            borderTextBoxValhalla.Inflate(1, 1); // border thickness
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxValhalla, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion
        #region Contols Events
        private void TextBoxValhalla_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox textBox = (TextBox)sender;
                LabelCountValhalla.Text = textBox.Lines.Count().ToString();
                Valhalla.Play();
            }
            catch (Exception) { }
        }

        private async void IconButtonStop_Click(object sender, EventArgs e)
        {
            try
            {
                IconButton iconButton = (IconButton)sender;
                iconButton.IconColor = Color.Black;
                iconButton.Enabled = false;

                await tokenCancel.Kill();
                await browser.Kill(web);

                await ConsoleProgressGeneral("Frigg esta siendo detenida.", 0, "Success");
                await ConsoleProgressDetail("Frigg esta siendo detenida.", 0, "Success");
                await ConsoleProgressGeneral("FRIGG REINA DE LOS ÆSIR.", 0);
                await ConsoleProgressDetail("FRIGG REINA DE LOS ÆSIR.", 0);
                IconButtonGenerar.IconColor = IconButtonGenerarIconColor;
                IconButtonGenerar.Enabled = IconButtonGenerarEnabled;
                IconButtonClear.IconColor = Color.White;
                IconButtonClear.Enabled = true;
                IconButtonValkyrie.IconColor = Color.White;
                IconButtonValkyrie.Enabled = true;
                if (CreditCards != null)
                {
                    if (CreditCards.Count > 0)
                    {
                        IconButtonVerify.IconColor = Color.White;
                        IconButtonVerify.Enabled = true;
                    }
                    else
                    {
                        TextBoxCreditCards.Clear();
                        IconButtonVerify.IconColor = Color.Black;
                        IconButtonVerify.Enabled = false;
                    }
                }

            }
            catch (Exception) { }
        }
        #endregion
        #endregion
        #region Panel Hellheim
        #region Panel Events
        private void PanelHellheim_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxHelheim = new Rectangle(TextBoxHelheim.Location.X, TextBoxHelheim.Location.Y, TextBoxHelheim.ClientSize.Width + 17, TextBoxHelheim.ClientSize.Height);
            borderTextBoxHelheim.Inflate(1, 1); // border thickness
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxHelheim, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion
        #region Contols Events
        private void TextBoxHelheim_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            try
            {
                LabelCountHellheim.Text = textBox.Lines.Count().ToString();
            }
            catch (Exception) { /*throw;*/ }
        }
        private void IconButtonClear_Click(object sender, EventArgs e)
        {
            IconButton iconButton = (IconButton)sender;
            iconButton.IconColor = Color.Black;
            iconButton.Enabled = false;
            TextBoxBin.Clear();
            TextBoxBin.Placeholder("######xxxxxxxxxx");
            MaskedTextBoxDate.Clear();
            MaskedTextBoxDate.Placeholder("dd/aa");
            TextBoxCvv.Clear();
            TextBoxCvv.Placeholder(cvvDigits);
            TextBoxQuantity.Clear();
            TextBoxQuantity.Text = "10";
            TextBoxCreditCards.Text = string.Empty;
            TextBoxValhalla.Text = string.Empty;
            TextBoxHelheim.Text = string.Empty;
            IconButtonGenerar.IconColor = Color.Black;
            IconButtonGenerar.Enabled = false;
            IconButtonVerify.IconColor = Color.Black;
            IconButtonVerify.Enabled = false;
            LabelGetBin.Text = "-";
            LabelGetSchema.Text = "-";
            LabelGetType.Text = "-";
            LabelGetBrand.Text = "-";
            LabelGetBank.Text = "-";
            LabelGetCountry.Text = "-";
        }
        #endregion
        #endregion
        #endregion

        #region Generator Credit Cards
        #region Methods
        private async Task GenerateCreditCards()
        {
            tokenCancel = new CancellationTokenSource();
            CancellationToken token = tokenCancel.Token;

            //  await Task.Factory.StartNew(async () =>
            await Task.Run(async () =>
            {
                if (!token.IsCancellationRequested)
                {
                    string bin = TextBoxBin.Text.Replace("x", string.Empty).Replace("X", string.Empty);
                    string listCreditCard = string.Empty;
                    if (bin.Length >= 6)
                    {
                        Dictionary<int, int> precision = new Dictionary<int, int>();
                        RangeAndLength[] range = AllCards();
                        int rangeLength = range.Length;
                        for (int j = 0; j < rangeLength; j++)
                        {
                            string pattern = @"\b" + range[j].Range;
                            Match match = Regex.Match(bin, pattern);
                            if (match.Success)
                            {
                                precision.Add(j, int.Parse(match.Value));
                            }
                        }
                        if (precision.Count > 0)
                        {
                            RangeAndLength card = range[precision.OrderByDescending(x => x.Value).First().Key];
                            int cardLenght = card.Length;
                            Quatity = TextBoxQuantity.Text;
                            int quantity = int.Parse(Quatity);
                            CreditCards = new List<List<string>>();
                            for (int i = 0; i < quantity; i++)
                            {
                                double progress = Convert.ToDouble((i + 1) * 100 / quantity);
                                await ConsoleProgressGeneral("Generando", (int)Math.Round(progress));
                                CreditCards.Add(new List<string>());
                                CreditCards[i].Add(CreateCardNumber(bin, cardLenght));
                                CreditCards[i].Add(GetDate()[0]);
                                CreditCards[i].Add(GetDate()[1]);
                                CreditCards[i].Add(GetCvv());
                                //await Task.Delay(50);
                                if (IconButtonGenerarStopClick)
                                {
                                    IconButtonGenerarStopClick = false;
                                    await ConsoleProgressGeneral("¡Detenido!", 0, "Success");
                                    break;
                                }
                            }

                            if (CreditCards.Count > 0)
                            {
                                int creditCardsCount = CreditCards.Count;
                                for (int i = 0; i < creditCardsCount; i++)
                                {
                                    if (i != creditCardsCount - 1)
                                    {
                                        listCreditCard += string.Join("|", CreditCards[i].ToArray()) + "\r\n";
                                    }
                                    else
                                    {
                                        listCreditCard += string.Join("|", CreditCards[i].ToArray());
                                    }
                                }
                            }
                        }
                    }
                    TextBoxCreditCards.Text = listCreditCard;
                }
            }, token);
        }
        private static RangeAndLength[] AllCards()
        {
            return BuildRangeAndLengthList(rangeAmex, 15).
                Union(BuildRangeAndLengthList(rangeCUP, 16)).
                Union(BuildRangeAndLengthList(rangeDCCB, 14)).
                Union(BuildRangeAndLengthList(rangeDCI, 14)).
                Union(BuildRangeAndLengthList(rangeDCUSC, 16)).
                Union(BuildRangeAndLengthList(rangeDC, 14)).
                Union(BuildRangeAndLengthList(rangeInsP, 16)).
                Union(BuildRangeAndLengthList(rangeInt32P, 16)).
                Union(BuildRangeAndLengthList(rangeJCB, 16)).
                Union(BuildRangeAndLengthList(rangeM, 16)).
                Union(BuildRangeAndLengthList(rangeMC, 16)).
                Union(BuildRangeAndLengthList(rangeUATP, 15)).
                Union(BuildRangeAndLengthList(rangeV, 16)).
                ToArray();
        }
        private static IEnumerable<RangeAndLength> BuildRangeAndLengthList(string[] rangeList, int length)
        {
            string[] x = GenerateRange(rangeList);
            return from range in x select new RangeAndLength(range, length);
        }
        private static string[] GenerateRange(string[] rangesList)
        {
            List<string> range = new List<string>();
            foreach (string rangeList in rangesList)
            {
                if (rangeList.Contains("-"))
                {
                    string[] ranges = rangeList.Split('-');
                    int start = int.Parse(ranges[0]);
                    int end = int.Parse(ranges[1]);

                    for (int i = start; i <= end; i++)
                    {
                        range.Add(i.ToString());
                    }
                }
                else
                {
                    range.Add(rangeList);
                }
            }
            return range.ToArray();
        }
        private struct RangeAndLength
        {
            public RangeAndLength(string range, int length)
            {
                Range = range;
                Length = length;
            }
            public string Range { get; set; }
            public int Length { get; set; }
        }
        private string CreateCardNumber(string bin, int binLenght)
        {
            Random random = new Random();
            string ccnumber = bin;
            while (ccnumber.Length < (binLenght - 1))
            {
                double rnd = (random.NextDouble() * 1.0f - 0f);
                ccnumber += Math.Floor(rnd * 10);
            }

            // reverse number and convert to int
            IEnumerable<char> reversedCCnumberstring = ccnumber.ToCharArray().Reverse();
            IEnumerable<int> reversedCCnumberList = reversedCCnumberstring.Select(c => Convert.ToInt32(c.ToString()));

            // calculate sum //Luhn Algorithm http://en.wikipedia.org/wiki/Luhn_algorithm
            int sum = 0;
            int pos = 0;
            int[] reversedCCnumber = reversedCCnumberList.ToArray();

            while (pos < binLenght - 1)
            {
                int odd = reversedCCnumber[pos] * 2;

                if (odd > 9)
                    odd -= 9;

                sum += odd;

                if (pos != (binLenght - 2))
                    sum += reversedCCnumber[pos + 1];

                pos += 2;
            }

            // calculate check digit
            int checkdigit = Convert.ToInt32((Math.Floor((decimal)sum / 10) + 1) * 10 - sum) % 10;
            ccnumber += checkdigit;
            return ccnumber;
        }
        private List<string> GetDate()
        {
            List<string> date = new List<string>();
            if (MaskedTextBoxDate.Text != "dd/aa")
            {
                string[] split = MaskedTextBoxDate.Text.Split('/');
                date.Add(split[0]);
                date.Add("20" + split[1]);
            }
            else
            {
                date.Add(new Random().Next(1, 12).ToString("00"));
                date.Add("20" + new Random().Next(20, 29).ToString());
            }
            return date;
        }
        private string GetCvv()
        {
            string cvv;
            if (TextBoxCvv.Text == cvvDigits)
            {
                if (TextBoxCvv.MaxLength == 4)
                {
                    cvv = new Random().Next(1, 9999).ToString("0000");
                }
                else
                {
                    cvv = new Random().Next(1, 999).ToString("000");
                }
            }
            else
            {
                cvv = TextBoxCvv.Text;
            }
            return cvv;
        }

        #endregion
        #endregion

        #region Valkyrie
        #region Panel Valkyrie
        #region Panel Events
        private void PanelValkyrie_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxValkyrie = new Rectangle(TextBoxValkyrie.Location.X, TextBoxValkyrie.Location.Y, TextBoxValkyrie.ClientSize.Width + 17, TextBoxValkyrie.ClientSize.Height);
            borderTextBoxValkyrie.Inflate(1, 1); // border thickness
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxValkyrie, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion
        #region Contols Events
        private void TextBoxValkyrie_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            try
            {
                LabelCountValkyrie.Text = textBox.Lines.Count().ToString();
                string valkyries = textBox.Text.Trim();
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (valkyries.Contains("\r\n"))
                    {
                        string[] listValkyries = valkyries.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        int listValkyriesLength = listValkyries.Length;
                        Valkyries = new List<List<string>>();
                        for (int i = 0; i < listValkyriesLength; i++)
                        {
                            string[] listValkyrie = listValkyries[i].Split('|');
                            Valkyries.Add(new List<string>());
                            Valkyries[i].Add(listValkyrie[0]);
                            Valkyries[i].Add(listValkyrie[1]);
                            Valkyries[i].Add(listValkyrie[2]);
                        }
                        if (!IconButtonValkyrieVerifyClick)
                        {
                            IconButtonValkyrieVerify.IconColor = Color.White;
                            IconButtonValkyrieVerify.Enabled = true;
                            IconButtonValkyrieClear.IconColor = Color.White;
                            IconButtonValkyrieClear.Enabled = true;
                        }
                    }
                    else if (valkyries.Contains("|"))
                    {
                        Valkyries = new List<List<string>>();
                        string[] listValkyrie = valkyries.Split('|');
                        Valkyries.Add(new List<string>());
                        Valkyries[0].Add(listValkyrie[0]);
                        Valkyries[0].Add(listValkyrie[1]);
                        Valkyries[0].Add(listValkyrie[2]);
                        if (!IconButtonValkyrieVerifyClick)
                        {
                            IconButtonValkyrieVerify.IconColor = Color.White;
                            IconButtonValkyrieVerify.Enabled = true;
                            IconButtonValkyrieClear.IconColor = Color.White;
                            IconButtonValkyrieClear.Enabled = true;
                        }
                    }
                    else
                    {
                        textBox.Text = string.Empty;
                    }
                }
                else
                {
                    if (valkyries == string.Empty)
                    {
                        textBox.Text = string.Empty;
                        IconButtonVerify.IconColor = Color.Black;
                        IconButtonVerify.Enabled = false;
                        if (TextBoxValkyrieLive.Text == string.Empty && TextBoxValkyrieDie.Text == string.Empty)
                        {
                            IconButtonClear.IconColor = Color.Black;
                            IconButtonClear.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                textBox.Text = string.Empty;
            }
        }
        private async void IconButtonValkyrieVerify_Click(object sender, EventArgs e)
        {
            IconButton iconButton = (IconButton)sender;
            iconButton.IconColor = Color.Black;
            iconButton.Enabled = false;
            IconButtonValkyrieStart.IconColor = Color.Black;
            IconButtonValkyrieStart.Enabled = false;
            IconButtonValkyrieClear.IconColor = Color.Black;
            IconButtonValkyrieClear.Enabled = false;
            IconButtonValkyrie.IconColor = Color.Black;
            IconButtonValkyrie.Enabled = false;
            IconButtonValkyrieVerifyClick = true;
            IconButtonValkyrieStop.Enabled = true;
            IconButtonValkyrieStop.IconColor = Color.White;
            IconButtonValkyrieStop.Show();
            await InvokeValkyrie();
            await ConsoleProgressGeneral("Finalizo llamado a Valquirias.", 100, "Success");
            await ConsoleProgressDetail("Finalizo llamado a Valquirias.", 100, "Success");
            await ConsoleProgressGeneral("VALQUIRIAS (PROXYS).", 0);
            await ConsoleProgressDetail("VALQUIRIAS (PROXYS).", 0);
            IconButtonValkyrieClear.IconColor = Color.White;
            IconButtonValkyrieClear.Enabled = true;
            if (!string.IsNullOrEmpty(TextBoxValkyrieLive.Text))
            {
                if (TextBoxValkyrieLive.Lines.Count() > 0)
                {
                    IconButtonValkyrieStart.IconColor = Color.White;
                    IconButtonValkyrieStart.Enabled = true;
                    IconButtonValkyrie.IconColor = Color.White;
                    IconButtonValkyrie.Enabled = true;
                }
            }
            IconButtonValkyrieVerifyClick = false;
            IconButtonValkyrieStop.Hide();
        }
        #endregion
        #endregion
        #region Panel Valkyrie Live
        #region Panel events
        private void PanelValkyrieLive_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxValkyrieLive = new Rectangle(TextBoxValkyrieLive.Location.X, TextBoxValkyrieLive.Location.Y, TextBoxValkyrieLive.ClientSize.Width + 17, TextBoxValkyrieLive.ClientSize.Height);
            borderTextBoxValkyrieLive.Inflate(1, 1); // border thickness
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxValkyrieLive, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion
        #region Controls Events
        private void TextBoxValkyrieLive_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            LabelCountValkyrieLive.Text = textBox.Lines.Count().ToString();
            Valkyrie.Play();
        }
        private void IconButtonValkyrieStart_Click(object sender, EventArgs e)
        {
            try
            {
                IconButton iconButton = (IconButton)sender;
                iconButton.IconColor = Color.Black;
                iconButton.Enabled = false;
                string valkyries = TextBoxValkyrieLive.Text.Trim();
                if (valkyries.Contains("\r\n"))
                {
                    string[] listValkyries = valkyries.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    int listValkyriesLength = listValkyries.Length;
                    LiveValkyries = new List<List<string>>();
                    for (int i = 0; i < listValkyriesLength; i++)
                    {
                        string[] listValkyrie = listValkyries[i].Split('|');
                        LiveValkyries.Add(new List<string>());
                        LiveValkyries[i].Add(listValkyrie[0] + "://" + listValkyrie[1] + ":" + listValkyrie[2]);
                    }
                }
                else if (valkyries.Contains("|"))
                {
                    LiveValkyries = new List<List<string>>();
                    string[] listValkyrie = valkyries.Split('|');
                    LiveValkyries.Add(new List<string>());
                    LiveValkyries[0].Add(listValkyrie[0] + "://" + listValkyrie[1] + ":" + listValkyrie[2]);
                }

            }
            catch (Exception) { }
            IconButtonGenerar.IconColor = IconButtonGenerarIconColor;
            IconButtonGenerar.Enabled = IconButtonGenerarEnabled;
            PanelValkyrie.Hide();
            PanelValkyrieLive.Hide();
            PanelValkyrieDie.Hide();
            ShowPanelValkyrie = false;
        }
        private async void IconButtonValkyrieStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (tokenCancel != null)
                {
                    IconButton iconbutton = (IconButton)sender;
                    iconbutton.IconColor = Color.Black;
                    iconbutton.Enabled = false;
                    //await browser.Kill();
                    tokenCancel.Cancel();
                    tokenCancel.Dispose();
                    tokenCancel = null;
                    await ConsoleProgressGeneral("¡Detenido!", 0, "Success");
                    await ConsoleProgressDetail("¡Detenido!", 0, "Success");
                    await ConsoleProgressGeneral("ValquiriaS (PROXYS)", 0);
                    await ConsoleProgressDetail("ValquiriaS (PROXYS)", 0);
                    IconButtonValkyrieClear.IconColor = Color.White;
                    IconButtonValkyrieClear.Enabled = true;
                    IconButtonValkyrie.IconColor = Color.White;
                    IconButtonValkyrie.Enabled = true;
                    if (Valkyries.Count > 0)
                    {
                        IconButtonValkyrieVerify.IconColor = Color.White;
                        IconButtonValkyrieVerify.Enabled = true;
                    }

                    if (!string.IsNullOrEmpty(TextBoxValkyrieLive.Text))
                    {
                        if (TextBoxValkyrieLive.Lines.Count() > 0)
                        {
                            IconButtonValkyrieStart.IconColor = Color.White;
                            IconButtonValkyrieStart.Enabled = true;
                            IconButtonValkyrie.IconColor = Color.White;
                            IconButtonValkyrie.Enabled = true;
                        }
                    }
                    iconbutton.Hide();
                }
            }
            catch (Exception) { }
        }
        #endregion
        #endregion
        #region  Panel Valkyrie Die
        #region Panel events
        private void PanelValkyrieDie_Paint(object sender, PaintEventArgs e)
        {
            Rectangle borderTextBoxValkyrieDie = new Rectangle(TextBoxValkyrieDie.Location.X, TextBoxValkyrieDie.Location.Y, TextBoxValkyrieDie.ClientSize.Width + 17, TextBoxValkyrieDie.ClientSize.Height);
            borderTextBoxValkyrieDie.Inflate(1, 1); // border thickness
            ControlPaint.DrawBorder(e.Graphics, borderTextBoxValkyrieDie, Color.White, ButtonBorderStyle.Solid);
        }
        #endregion

        #region Controls Events
        private void TextBoxValkyrieDie_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            LabelCountValkyrieDie.Text = textBox.Lines.Count().ToString();
        }
        private void IconButtonValkyrieClear_Click(object sender, EventArgs e)
        {
            IconButton iconButton = (IconButton)sender;
            iconButton.IconColor = Color.Black;
            iconButton.Enabled = false;
            IconButtonValkyrieVerify.IconColor = Color.Black;
            IconButtonValkyrieVerify.Enabled = false;
            IconButtonValkyrieStart.IconColor = Color.Black;
            IconButtonValkyrieStart.Enabled = false;
            TextBoxValkyrie.Text = string.Empty;
            TextBoxValkyrieLive.Text = string.Empty;
            TextBoxValkyrieDie.Text = string.Empty;
            if (Valkyries != null)
            {
                if (Valkyries.Count > 0)
                {
                    Valkyries.Clear();
                }
            }

            if (LiveValkyries != null)
            {
                if (LiveValkyries.Count > 0)
                {
                    LiveValkyries.Clear();
                }
            }
            IconButtonValkyrieVerifyClick = false;
        }
        #endregion
        #endregion
        #region Methods
        private async Task InvokeValkyrie()
        {
            try
            {
                string listValkyrieLive = string.Empty;
                string listValkyrieDie = string.Empty;
                int valkyriesCount = Valkyries.Count;
                await ConsoleProgressGeneral("Frigg hace llamado a Valquirias.", 0);
                int x = 0;
                for (int i = 0; i < valkyriesCount;)
                {
                    string proxy = Valkyries[i][0] + "://" + Valkyries[i][1] + ":" + Valkyries[i][2];
                    bool friggVerifyValkyrie = await FriggVerifyValkyrie(proxy);
                    if (friggVerifyValkyrie)
                    {
                        if (TextBoxValkyrieLive.Text == string.Empty)
                        {
                            listValkyrieLive += string.Join("|", Valkyries[i].ToArray());
                        }
                        else
                        {
                            listValkyrieLive = TextBoxValkyrieLive.Text;
                            listValkyrieLive += "\r\n" + string.Join("|", Valkyries[i].ToArray());
                        }
                        await ReduceListValkyrie();
                        TextBoxValkyrieLive.Text = listValkyrieLive;
                        double progress = Convert.ToDouble((x + 1) * 100 / valkyriesCount);
                        await ConsoleProgressGeneral("Frigg hace llamado a Valquirias.", (int)Math.Round(progress));
                    }
                    else
                    {
                        if (TextBoxValkyrieDie.Text == string.Empty)
                        {
                            listValkyrieDie += string.Join("|", Valkyries[i].ToArray());
                        }
                        else
                        {
                            listValkyrieDie = TextBoxValkyrieDie.Text;
                            listValkyrieDie += "\r\n" + string.Join("|", Valkyries[i].ToArray());
                        }
                        await ReduceListValkyrie();
                        TextBoxValkyrieDie.Text = listValkyrieDie;
                        double progress = Convert.ToDouble((x + 1) * 100 / valkyriesCount);
                        await ConsoleProgressGeneral("Frigg hace llamado a Valquirias.", (int)Math.Round(progress));
                    }
                    //await Task.Delay(50);
                    x++;
                }
            }
            catch (Exception) { }
        }
        private async Task<bool> FriggVerifyValkyrie(string proxy)
        {
            bool frigg = false;
            try
            {
                browser = new ChromiumWebBrowser();
                await Task.Delay(500);
                tokenCancel = new CancellationTokenSource();
                CancellationToken token = tokenCancel.Token;
                await ConsoleProgressDetail("Valquirias preparadas.", 0);
                await Task.Run(async () =>
                {
                    await ConsoleProgressDetail("Valquirias preparadas.", 20);
                    if (!token.IsCancellationRequested)
                    {
                        string url = initialUrl;
                        bool loadPage = await browser.LoadPage(url, proxy);
                        await ConsoleProgressDetail("Analizando Valquiria. " + proxy, 40);
                        await Task.Delay(500);
                        if (loadPage)
                        {
                            bool startFriggValkyrie = await StartFriggValkyrie();
                            if (startFriggValkyrie)
                            {
                                await ConsoleProgressDetail("Escogida Valquiria. " + proxy, 100, "Success");
                                frigg = true;
                            }
                            else
                            {
                                await ConsoleProgressDetail("Rechazada Valquiria. " + proxy, 100, "Fail");
                                frigg = false;
                            }
                        }
                        else
                        {
                            await FriggVerifyValkyrie(proxy);
                        }
                    }
                    else
                    {
                        frigg = false;
                    }
                }, token);
            }
            catch (Exception) { }
            return frigg;
        }

        private async Task<bool> StartFriggValkyrie()
        {
            try
            {
                await ConsoleProgressDetail("Comprobando estado de Valquiria.", 80);
                if (await browser.ElementVisible("#product-landing > div > div.c-product-landing__grid-wrapper > div > button", "ExceptionLoadValkyrie"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception) { }
            return false;
        }
        private async Task ReduceListValkyrie()
        {
            try
            {
                Valkyries.RemoveAt(0);
                string listValkyries = string.Empty;
                int ValkyriesCount = Valkyries.Count;
                for (int i = 0; i < ValkyriesCount; i++)
                {
                    if (Valkyries.Count > 0)
                    {
                        if (i < Valkyries.Count - 1)
                        {
                            listValkyries += string.Join("|", Valkyries[i].ToArray()) + "\r\n";
                        }
                        else
                        {
                            listValkyries += string.Join("|", Valkyries[i].ToArray());
                        }
                    }
                    else
                    {
                        listValkyries = string.Empty;
                    }
                    //await Task.Delay(50);
                }
                TextBoxValkyrie.Text = listValkyries;
            }

            catch (Exception) { }
        }
        #endregion
        #endregion

        #region Web Browser
        #region Load Browser
        private async Task InvokeFrigg()
        {
            bool frigg = false;
            try
            {
                await ConsoleProgressGeneral("Invocando a Frigg.", 0);
                browser = new ChromiumWebBrowser();
                await Task.Delay(500);

                tokenCancel = new CancellationTokenSource();
                CancellationToken token = tokenCancel.Token;
                frigg = await Task<bool>.Run(async () =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        return await LoadBrowser();
                    }
                    else
                    {
                        return false;
                    }
                }, token);
            }
            catch (Exception) { }

            //////await browser.Screenshot("100.Last_" + frigg.ToString() + "_" + Asatru.GetTimestamp(DateTime.Now));
            await browser.Kill(web);
            await tokenCancel.Kill();

            if (frigg)
            {
                await ConsoleProgressGeneral("Frigg finalizo los procesos correctamente.", 100, "Success");
                IconButtonClear.IconColor = Color.White;
                IconButtonClear.Enabled = true;
                IconButtonValkyrie.IconColor = Color.White;
                IconButtonValkyrie.Enabled = true;
            }
            else
            {

                //if (!running)
                //{
                //    await ConsoleProgressGeneral("Frigg fue detenida.", 0, "Fail");
                //    IconButtonGenerar.IconColor = IconButtonGenerarIconColor;
                //    IconButtonGenerar.Enabled = IconButtonGenerarEnabled;
                //    IconButtonClear.IconColor = Color.White;
                //    IconButtonClear.Enabled = true;
                //    IconButtonValkyrie.IconColor = Color.White;
                //    IconButtonValkyrie.Enabled = true;
                //    if (CreditCards != null)
                //    {
                //        if (CreditCards.Count > 0)
                //        {
                //            IconButtonVerify.IconColor = Color.White;
                //            IconButtonVerify.Enabled = true;

                //        }
                //        else
                //        {
                //            TextBoxCreditCards.Clear();
                //            IconButtonVerify.IconColor = Color.Black;
                //            IconButtonVerify.Enabled = false;
                //        }
                //    }
                //}
                //else if (requiredValkyrie)
                //{
                //    await ConsoleProgressGeneral("Frigg requiere el apoyo de Valquirias.", 0, "Fail");
                //    IconButtonGenerar.IconColor = IconButtonGenerarIconColor;
                //    IconButtonGenerar.Enabled = IconButtonGenerarEnabled;
                //    IconButtonClear.IconColor = Color.White;
                //    IconButtonClear.Enabled = true;
                //    IconButtonValkyrie.IconColor = Color.White;
                //    IconButtonValkyrie.Enabled = true;
                //    if (CreditCards != null)
                //    {
                //        if (CreditCards.Count > 0)
                //        {
                //            IconButtonVerify.IconColor = Color.White;
                //            IconButtonVerify.Enabled = true;

                //        }
                //        else
                //        {
                //            TextBoxCreditCards.Clear();
                //            IconButtonVerify.IconColor = Color.Black;
                //            IconButtonVerify.Enabled = false;
                //        }
                //    }
                //}
                if (CreditCards.Count > 0)
                {
                    await ConsoleProgressDetail("", 0);
                    await ConsoleProgressGeneral("Frigg requiere un sacrificio.", 0, "Fail");
                    await ConsoleProgressGeneral("Ofreciendo un sacrificio a Frigg.", 0, "Success");
                    await InvokeFrigg();
                }
                else
                {
                    await ConsoleProgressDetail("", 0);
                    await ConsoleProgressGeneral("Frigg encontro un elfo oscuro y se detuvo.", 0, "Fail");
                }
            }
            await Task.Delay(2000);
            await ConsoleProgressDetail("", 0);
            await ConsoleProgressGeneral("FRIGG REINA DE LOS ÆSIR.", 0);
        }

        private async Task<bool> LoadBrowser()
        {
            bool loadBrowser = false;
            try
            {
                await ConsoleProgressGeneral("Invocando a Frigg..", 2);
                string url = initialUrl;
                if (LiveValkyries != null)
                {
                    if (LiveValkyries.Count > 0)
                    {
                        List<string> proxy = LiveValkyries.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                        await ConsoleProgressGeneral("Valquiria: " + proxy[0], 2);
                        loadBrowser = await browser.LoadPage(url, proxy[0]);
                    }
                    else
                    {
                        loadBrowser = await browser.LoadPage(url);
                    }
                }
                else
                {
                    loadBrowser = await browser.LoadPage(url);
                }
            }
            catch (Exception) { }

            await Task.Delay(500);
            if (loadBrowser)
            {
                return await LoadPage();
            }
            await ConsoleProgressGeneral("Invocando a Frigg.. ¡Fallo!", 2, "Fail");
            return false;

        }

        private async Task<bool> LoadPage()
        {

            try
            {
                //////await browser.Screenshot("0.LoadPage_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressGeneral("Invocando a Frigg...", 3);
                return await StartFrigg();
            }
            catch (Exception) { throw; }

            await ConsoleProgressGeneral("Invocando a Frigg... ¡Fallo!", 0, "Fail");
            return false;
        }
        #endregion
        #region  Gate Step By Step
        private async Task<bool> StartFrigg()
        {
            try
            {
                await browser.Screenshot("1.StartFrigg_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressGeneral("Zombificando ambiente", 10);
                await browser.DisableAlerts();
            }
            catch (Exception) { }
            //await Task.Delay(10000);
            //await browser.GetSource("registry");
            bool divGoToSingUp = await browser.ElementVisible("div[data-testid=login-signuptab]", "ExceptionStartFrigg");
            if (divGoToSingUp)
            {
                return await GoToSingUp();
            }
            await ConsoleProgressGeneral("Zombificando ambiente. !Fallo¡", 10, "Fail");
            return false;
        }
        private async Task<bool> GoToSingUp()
        {
            try
            {
                await browser.Screenshot("2.GoToSingUp_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressGeneral("Zombificando ambiente", 20);
                await browser.DisableAlerts();
                await browser.Click("div[data-testid=login-signuptab]");
            }
            catch (Exception) { }
            //await Task.Delay(10000);
            //await browser.GetSource("registry");
            bool inputSignUp = await browser.ElementVisible("input[data-id=firstName]", "ExceptionGoToSingUp");
            if (inputSignUp)
            {
                return await SignUp();
            }
            await ConsoleProgressGeneral("Zombificando ambiente. !Fallo¡", 20, "Fail");
            return false;
        }
        private async Task<bool> SignUp()
        {
            try
            {
                // browser.Screenshot("2.LogIn");
                await browser.Screenshot("3.SignUp_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressGeneral("Resolviendo teorema de Gibbs.", 30);
                await browser.SendKeys("input[data-id=firstName]", Faker.Name.FirstName());
                await browser.SendKeys("input[data-id=lastName]", Faker.Name.LastName());
                await browser.SendKeys("input[data-id=emailAddress]", Faker.Internet.Email());
                await browser.SendKeys("input[data-id=password]", Faker.Internet.Password());
                await browser.ExecuteScript("document.querySelector('div[data-testid=signup-button]').click();");
                //await Task.Delay(3000);
            }
            catch (Exception) { }

            Task<bool> task1 = browser.NodeVisibility("document.querySelector(\"iframe[title = 'recaptcha challenge']\").parentNode.parentNode.style.visibility", "3.ExceptionSignupWomen");
            Task<bool> task2 = browser.ElementVisible("div[data-testid=signup-women]", "3.1.ExceptionSignupCaptcha");

            await Task.WhenAny(task1, task2);
            bool task1Result = task1.Result; // or await task1
            bool task2Result = task2.Result; // or await task2


            //bool divSignUpWomen = await 
            //bool iframeReCaptcha = await ;
            //bool iframeReCaptcha = await browser.NodeVisibility("document.querySelector(\"iframe[sandbox = 'allow-forms allow-popups allow-same-origin allow-scripts allow-top-navigation allow-modals allow-popups-to-escape-sandbox']\").parentNode.parentNode.style.visibility", "ExceptionSignup");
            if (task1Result)
            {
                string urlIframe = (string)await browser.ExecuteScript("document.querySelector(\"iframe[title = 'recaptcha challenge']\").getAttribute('src')");
                Iframe = await browser.GetFrameByUrl(urlIframe, "3.ExceptionCaptcha");
                if (Iframe != null)
                {
                    return await ReCaptcha();
                }
            }
            else if (task2Result)
            {
                return await MoreInfo();
            }

            //bool divModal = await browser.ElementInvisible("input[data-id=firstName]", "ExceptionSignUp");
            //if (divModal)
            //{
            //    //////await browser.Screenshot("2.1.Login_" + Asatru.GetTimestamp(DateTime.Now));
            //    bool divWait = await browser.ElementInvisible("#take-a-deep-breath", "ExceptionLogInWait");
            //    if (divWait)
            //    {
            //        //////await browser.Screenshot("2.2.Login_" + Asatru.GetTimestamp(DateTime.Now));
            //        return await Last();
            //    }
            //}
            await ConsoleProgressGeneral("Resolviendo teorema de Gibbs. ¡Fallo!", 30, "Fail");
            return false;
        }

        private async Task<bool> ReCaptcha()
        {

            try
            {
                await browser.Screenshot("4.ReCaptcha_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressGeneral("Cotejando sinergias interdimensionales.", 30);
                await browser.DisableAlerts();
                //await Task.Delay(3000);

                sourceImageOriginal = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(1) > td:nth-child(1) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");

                if (sourceImageOriginal != null)
                {
                    var request = WebRequest.Create(sourceImageOriginal);

                    using (WebResponse response = await request.GetResponseAsync())
                    using (Stream stream = response.GetResponseStream())
                    {
                        PictureBoxOriginal.Image = Bitmap.FromStream(stream);
                        stream.Dispose();
                    }

                    int numberChild = (int)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody').children.length;");
                    LabelInfoReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect > div.rc-imageselect-payload > div.rc-imageselect-instructions > div.rc-imageselect-desc-wrapper > div').textContent");
                    LabelInfoStrongReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect > div.rc-imageselect-payload > div.rc-imageselect-instructions > div.rc-imageselect-desc-wrapper > div > strong').textContent");

                    IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");

                    //browser.Screenshot("4.recapcha");

                    if (numberChild == 3)
                    {
                        ReCaptcha3x3();
                        PanelReCaptcha3x3.Show();
                        PanelReCaptcha.Show();
                    }
                    else if (numberChild == 4)
                    {
                        ReCaptcha4x4();
                        PanelReCaptcha4x4.Show();
                        PanelReCaptcha.Show();
                    }
                    await IconButtonAceptReCaptcha.OnClickAsync();
                    await Task.Delay(5000);
                    await browser.ClickXY(819, 728);
                }

                if (!PanelReCaptcha.Visible)
                {
                    PanelReCaptcha.Show();
                }
                else
                {
                    PanelReCaptcha.Hide();
                }
                PanelReCaptcha3x3.Hide();
                PanelReCaptcha4x4.Hide();
                HideChecks();

            }
            catch (Exception) { throw; }

            bool iframeReCaptchaHide = await browser.NodeHidden("document.querySelector(\"iframe[title = 'recaptcha challenge']\").parentNode.parentNode.style.visibility", "4.ExceptionReCaptchaHide");
            if (iframeReCaptchaHide)
            {
                Task<bool> task1 = browser.NodeVisibility("document.querySelector(\"iframe[title = 'recaptcha challenge']\").parentNode.parentNode.style.visibility", "4.1.ExceptionReCaptchaOther");
                Task<bool> task2 = browser.ElementVisible("div[data-testid=signup-women]", "4.2.ExceptionReCaptchaMoreInfo");

                await Task.WhenAny(task1, task2);
                bool task1Result = task1.Result; // or await task1
                bool task2Result = task2.Result; // or await task2


                //bool divSignUpWomen = await 
                //bool iframeReCaptcha = await ;
                //bool iframeReCaptcha = await browser.NodeVisibility("document.querySelector(\"iframe[sandbox = 'allow-forms allow-popups allow-same-origin allow-scripts allow-top-navigation allow-modals allow-popups-to-escape-sandbox']\").parentNode.parentNode.style.visibility", "ExceptionSignup");
                if (task1Result)
                {
                    string urlIframe = (string)await browser.ExecuteScript("document.querySelector(\"iframe[title = 'recaptcha challenge']\").getAttribute('src')");
                    Iframe = await browser.GetFrameByUrl(urlIframe, "4.ExceptionCaptcha");
                    if (Iframe != null)
                    {
                        return await ReCaptcha();
                    }
                }
                else if (task2Result)
                {
                    return await MoreInfo();
                }
                else
                {
                    await browser.Screenshot("4.ReCaptchaExit_" + Asatru.GetTimestamp(DateTime.Now));
                }
            }
            else
            {
                bool paragraphTryAgain = await browser.ElementInnerTextContent("#page-wrapper > div.center-page > div > div > form > p", "try again", "ExceptionReCaptchaWrong", 10);
                if (paragraphTryAgain)
                {
                    await browser.Click("#page-wrapper > div.center-page > div > div > form > button:nth-child(13)");

                    return await ReCaptcha();

                }
                else
                {
                    //bool iframeCCNumber = await browser.ElementVisible("#stripe-form > div.checkout-form.payment-options > div > div > div.stripe-form-fields > fieldset.credit-card-info > fieldset:nth-child(1) > div.fields.cc-number-wrapper > div > div > iframe", "ExceptionRandomAddToCart");
                    //if (iframeCCNumber)
                    //{
                    //    return await Last();
                    //}
                }
            }

            await ConsoleProgressGeneral("Cotejando sinergias interdimensionales. ¡Fallo!", 30, "fail");
            return false;
        }

        private async Task<bool> MoreInfo()
        {
            try
            {
                if (new Random().Next(0, 2) > 0)
                {
                    await browser.ExecuteScript("document.querySelector('div[data-testid=signup-women]').click();");
                }
                else
                {
                    await browser.ExecuteScript("document.querySelector('div[data-testid=signup-men]').click();");
                }

                await browser.ExecuteScript("document.querySelector('div[data-testid=signup-ageselect]').click();");
                string clickAgeOption = @"var allAgeOptions = document.querySelectorAll('div[data-testid=signup-ageoptions]');
                    var randAgeOption = allAgeOptions[Math.floor(Math.random() * allAgeOptions.length)];
                    randAgeOption.click();";
                await browser.ExecuteScript(clickAgeOption);
                await browser.ExecuteScript("document.querySelector('div[data-testid=signup-finish]').click();");

            }
            catch (Exception)
            {

                throw;
            }

            bool divTopMenuProfile = await browser.ElementVisible("div[data-testid=topmenu-profile]", "ExceptionMoreInfo");



            return false;
        }

        private async Task<bool> Last()
        {
            try
            {
                // browser.Screenshot("3.Last");
                //////await browser.Screenshot("3.Last_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressGeneral("Mancillando reputaciones.", 50);
                string listLiveCreditCard = string.Empty;
                string listDieCreditCard = string.Empty;

                if (CreditCards.Count > 0)
                {
                    LiveCreditCards = new List<List<string>>();
                    DieCreditCards = new List<List<string>>();
                    int creditCardsCount = CreditCards.Count;

                    //double x = 50 + CalcIncrementX(50);
                    double x = 50;
                    for (int i = 0; i < creditCardsCount;)
                    {
                        await ConsoleProgressDetail("Cuidado con el dragón.", 0);
                        string number = CreditCards[i][0];
                        string month = Int32.Parse(CreditCards[i][1]).ToString();
                        string year = Int32.Parse(CreditCards[i][2].Substring(2, 2)).ToString();
                        string cvv = CreditCards[i][3];

                        bool latestData = await LatestData(number, month, year, cvv);
                        if (latestData)
                        {
                            bool forseti = await Forseti();
                            if (forseti)
                            {
                                await ConsoleProgressDetail(string.Join("|", CreditCards[i].ToArray()), 100);
                                if (TextBoxValhalla.Text == string.Empty)
                                {
                                    listLiveCreditCard += string.Join("|", CreditCards[i].ToArray());
                                }
                                else
                                {
                                    listLiveCreditCard = TextBoxValhalla.Text;
                                    listLiveCreditCard += "\r\n" + string.Join("|", CreditCards[i].ToArray());
                                }

                                TextBoxValhalla.Text = listLiveCreditCard;

                                IEnumerable<Task> tasks = new Task[] {
                                    Asatru.SetValhalla(UserId, Token, GateId, string.Join("|", CreditCards[i].ToArray())),
                                    ReduceListCreditCards(),
                                    Block(),
                                    OChecker.SetLabelGetRunes()
                                };

                                await Task.Run(() => Task.WhenAll(tasks));

                                x += CalcIncrementX(50);
                                await ConsoleProgressGeneral("Trazando retículas irreticulizables.", (int)Math.Round(x));
                                return false;
                            }
                            else
                            {
                                if (!forsetiError)
                                {
                                    await ConsoleProgressDetail(string.Join("|", CreditCards[i].ToArray()), 100);
                                    if (TextBoxHelheim.Text == string.Empty)
                                    {
                                        listDieCreditCard += string.Join("|", CreditCards[i].ToArray());
                                    }
                                    else
                                    {
                                        listDieCreditCard = TextBoxHelheim.Text;
                                        listDieCreditCard += "\r\n" + string.Join("|", CreditCards[i].ToArray());
                                    }

                                    IEnumerable<Task> tasks = new Task[] {
                                        Asatru.SetHelheim(UserId, Token, GateId, string.Join("|", CreditCards[i].ToArray())),
                                        ReduceListCreditCards()
                                    };

                                    await Task.WhenAll(tasks);

                                    TextBoxHelheim.Text = listDieCreditCard;
                                    x += CalcIncrementX(50);
                                    await ConsoleProgressGeneral("Trazando retículas irreticulizables.", (int)Math.Round(x));
                                    continue;
                                }
                                else
                                {
                                    forsetiError = false;
                                    return false;
                                }
                            }
                        }
                        else { return false; }
                    }
                }
            }
            catch (Exception) { }

            if (CreditCards.Count() > 0)
            {
                await ConsoleProgressGeneral("Mancillando reputaciones. ¡Fallo!", 90, "Fail");
                return false;
            }
            return true;
        }

        private async Task<bool> LatestData(string number, string month, string year, string cvv)
        {
            try
            {
                //////await browser.Screenshot("4.LatestData_" + Asatru.GetTimestamp(DateTime.Now));
                await ConsoleProgressDetail("Fregando animaciones.", 50);

                await browser.SendKeys("#id-first-name > input[type=text]", FullName);
                await browser.SendKeyCode(0x09);
                await browser.SendKeys(string.Empty, number);
                await browser.SendKeyCode(0x09);
                await browser.SendKeys(string.Empty, month + year);
                await browser.SendKeyCode(0x09);
                await browser.SendKeys(string.Empty, cvv);
                await browser.SendKeyCode(0x09);
                string clickContinue = @"document.querySelector('button[type=submit]').click();";
                await browser.ExecuteScript(clickContinue);
                return true;
            }
            catch (Exception) { }
            await ConsoleProgressDetail("Fregando animaciones. ¡Fallo!", 90, "Fail");
            return false;
        }

        private async Task<bool> Forseti()
        {
            try
            {
                await ConsoleProgressDetail("Eliminando trucos potenciales.", 90);
                bool divErrorCardNumber = await browser.ElementVisible("#card-number > div:nth-child(2)", "ExceptionForsetiHelheimDivErrorCardNumber", 10);
                if (divErrorCardNumber)
                {
                    await browser.LoadPage(browser.Address);
                    return false;
                }
                else
                {
                    bool status = false;
                    for (int i = 0; i < 10; i++)
                    {
                        string getButtonStatus = "document.querySelector('form > button[type=submit]').getAttribute('class').includes('disabled')";
                        if ((bool)await browser.ExecuteScript(getButtonStatus))
                        {
                            status = true;
                            break;
                        }
                        await Task.Delay(1000);
                    }

                    if (status)
                    {
                        await browser.LoadPage(browser.Address);
                        return false;
                    }
                    else
                    {
                        bool homepage = await browser.ElementVisible("#content-app", "ExceptionForsetiValhallaHomepage");
                        if (homepage)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception) { }
            await ConsoleProgressDetail("Eliminando trucos potenciales. ¡Fallo!", 90, "Fail");
            forsetiError = true;
            return false;
        }


        #endregion
        #region Methods
        private async Task ReduceListCreditCards()
        {
            try
            {
                CreditCards.RemoveAt(0);
                string listCreditCard = string.Empty;
                int creditCardsCount = CreditCards.Count;
                for (int y = 0; y < creditCardsCount; y++)
                {
                    if (CreditCards.Count > 0)
                    {
                        if (y < CreditCards.Count - 1)
                        {
                            listCreditCard += string.Join("|", CreditCards[y].ToArray()) + "\r\n";
                        }
                        else
                        {
                            listCreditCard += string.Join("|", CreditCards[y].ToArray());
                        }
                    }
                    else
                    {
                        listCreditCard = string.Empty;
                    }
                    //await Task.Delay(50);
                }
                TextBoxCreditCards.Text = listCreditCard;
            }

            catch (Exception) {/* throw; */}
        }

        private double CalcIncrementX(double value)
        {
            try
            {
                double x = value / OriginalCreditCardsCount;
                return x;
            }
            catch (Exception) { }
            return 0;
        }
        #endregion

        #endregion

        private void IconButtonConfirmClose_Click(object sender, EventArgs e)
        {
            PanelConfirm.Hide();
        }

        private void IconButtonCancel_Click(object sender, EventArgs e)
        {
            PanelConfirm.Hide();
        }

        #region ReCaptcha
        private void ReCaptcha3x3()
        {
            // Trocear una imagen en trozos más pequeños
            const int columnas = 3;
            const int filas = 3;
            //
            // El tamaño proporcional del ancho y alto
            // correspondientes a los trozos a usar
            int tamTrozoW = 300 / columnas;
            int tamTrozoH = 300 / filas;
            // El tamaño de cada trozo
            int nW = 100;
            int nH = 100;
            // El rectángulo que ocupará cada nuevo trozo
            Rectangle rectDest = new Rectangle(0, 0, nW, nH);
            // Estas variables se usan en el bucle
            Bitmap bmpDest;
            Graphics g;
            Rectangle rectOrig;
            //
            // Array con los pictures que hay en el formulario
            PictureBox[] trozos = { PictureBox300, PictureBox301, PictureBox302, PictureBox303, PictureBox304, PictureBox305, PictureBox306, PictureBox307, PictureBox308 };
            // Para contar cada columna
            int c = 0;
            // La posición X e Y en la imagen original
            int pX = 0, pY = 0;
            for (int i = 0; i < trozos.Length; i++)
            {
                // El trozo de la imagen original
                rectOrig = new Rectangle(pX, pY, tamTrozoW, tamTrozoH);
                // La imagen de destino
                bmpDest = new Bitmap(tamTrozoW, tamTrozoW);
                g = Graphics.FromImage(bmpDest);
                // Obtenemos un trozo de la imagen original
                // y lo dibujamos en la imagen de destino
                g.DrawImage(PictureBoxOriginal.Image, rectDest, rectOrig, GraphicsUnit.Pixel);
                // Asignamos la nueva imagen al picture correspondiente
                trozos[i].Image = bmpDest;
                c += 1;
                pX += tamTrozoW;
                // Cuando hayamos recorrido las columnas,
                // pasamos a la siguiente fila
                if (c >= columnas)
                {
                    c = 0;
                    pX = 0;
                    pY += tamTrozoH;
                }
            }
        }

        private async void PictureBox300_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha300.Show();
            if (IconButton300.Visible)
            {
                IconButton300.Hide();
                await browser.ClickXY(547, 367);
            }
            else
            {
                await browser.ClickXY(547, 367);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(1) > td:nth-child(1) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton300.Show();
                }
            }
            PictureBoxLoadRecaptcha300.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox301_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha301.Show();
            if (IconButton301.Visible)
            {
                IconButton301.Hide();
                await browser.ClickXY(677, 367);
            }
            else
            {
                await browser.ClickXY(677, 367);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(1) > td:nth-child(2) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton301.Show();
                }
            }
            PictureBoxLoadRecaptcha301.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox302_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha302.Show();
            if (IconButton302.Visible)
            {
                IconButton302.Hide();
                await browser.ClickXY(804, 367);
            }
            else
            {
                await browser.ClickXY(804, 367);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(1) > td:nth-child(3) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton302.Show();
                }
            }
            PictureBoxLoadRecaptcha302.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox303_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha303.Show();
            if (IconButton303.Visible)
            {
                IconButton303.Hide();
                await browser.ClickXY(545, 496);
            }
            else
            {
                await browser.ClickXY(545, 496);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(2) > td:nth-child(1) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton303.Show();
                }
            }
            PictureBoxLoadRecaptcha303.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox304_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha304.Show();
            if (IconButton304.Visible)
            {
                IconButton304.Hide();
                await browser.ClickXY(675, 495);
            }
            else
            {
                await browser.ClickXY(675, 495);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(2) > td:nth-child(2) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton304.Show();
                }
            }
            PictureBoxLoadRecaptcha304.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox305_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha305.Show();
            if (IconButton305.Visible)
            {
                IconButton305.Hide();
                await browser.ClickXY(805, 495);
            }
            else
            {
                await browser.ClickXY(805, 495);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(2) > td:nth-child(3) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton305.Show();
                }
            }
            PictureBoxLoadRecaptcha305.Hide();
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox306_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha306.Show();
            if (IconButton306.Visible)
            {
                IconButton306.Hide();
                await browser.ClickXY(546, 627);
            }
            else
            {
                await browser.ClickXY(546, 627);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(3) > td:nth-child(1) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton306.Show();
                }
            }
            PictureBoxLoadRecaptcha306.Hide();
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox307_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha307.Show();
            if (IconButton307.Visible)
            {
                IconButton307.Hide();
                await browser.ClickXY(677, 626);
            }
            else
            {
                await browser.ClickXY(677, 626);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(3) > td:nth-child(2) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton307.Show();
                }
            }
            PictureBoxLoadRecaptcha307.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox308_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            PictureBoxLoadRecaptcha308.Show();
            if (IconButton308.Visible)
            {
                IconButton308.Hide();
                await browser.ClickXY(805, 625);
            }
            else
            {
                await browser.ClickXY(805, 625);
                await Task.Delay(10000);
                string sourceImage = (string)await Iframe.FExecuteScript("document.querySelector('#rc-imageselect-target > table > tbody > tr:nth-child(3) > td:nth-child(3) > div > div.rc-image-tile-wrapper > img').getAttribute('src')");
                if (sourceImageOriginal != sourceImage)
                {
                    WebRequest request = WebRequest.Create(sourceImage);

                    using (WebResponse response = request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        pictureBox.Image = Bitmap.FromStream(stream);
                    }
                }
                else
                {
                    IconButton308.Show();
                }
            }
            PictureBoxLoadRecaptcha308.Hide();

            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }


        private void ReCaptcha4x4()
        {
            // Trocear una imagen en trozos más pequeños
            const int columnas = 4;
            const int filas = 4;
            //
            // El tamaño proporcional del ancho y alto
            // correspondientes a los trozos a usar
            int tamTrozoW = 300 / columnas;
            int tamTrozoH = 300 / filas;
            // El tamaño de cada trozo
            int nW = 75;
            int nH = 75;
            // El rectángulo que ocupará cada nuevo trozo
            Rectangle rectDest = new Rectangle(0, 0, nW, nH);
            // Estas variables se usan en el bucle
            Bitmap bmpDest;
            Graphics g;
            Rectangle rectOrig;
            //
            // Array con los pictures que hay en el formulario
            PictureBox[] trozos = { PictureBox400, PictureBox401, PictureBox402, PictureBox403, PictureBox404, PictureBox405, PictureBox406, PictureBox407, PictureBox408, PictureBox409, PictureBox410, PictureBox411, PictureBox412, PictureBox413, PictureBox414, PictureBox415 };
            // Para contar cada columna
            int c = 0;
            // La posición X e Y en la imagen original
            int pX = 0, pY = 0;
            for (int i = 0; i < trozos.Length; i++)
            {
                // El trozo de la imagen original
                rectOrig = new Rectangle(pX, pY, tamTrozoW, tamTrozoH);
                // La imagen de destino
                bmpDest = new Bitmap(tamTrozoW, tamTrozoW);
                g = Graphics.FromImage(bmpDest);
                // Obtenemos un trozo de la imagen original
                // y lo dibujamos en la imagen de destino
                Bitmap objBitmap = new Bitmap(PictureBoxOriginal.Image, new Size(300, 300));
                g.DrawImage(objBitmap, rectDest, rectOrig, GraphicsUnit.Pixel);


                // Asignamos la nueva imagen al picture correspondiente
                trozos[i].Image = bmpDest;
                c += 1;
                pX += tamTrozoW;
                // Cuando hayamos recorrido las columnas,
                // pasamos a la siguiente fila
                if (c >= columnas)
                {
                    c = 0;
                    pX = 0;
                    pY += tamTrozoH;
                }
            }
        }


        private async void PictureBox400_Click(object sender, EventArgs e)
        {
            //PictureBox pictureBox = (PictureBox)sender;
            if (IconButton400.Visible)
            {
                await browser.ClickXY(529, 351);
                IconButton400.Hide();
            }
            else
            {
                await browser.ClickXY(529, 351);
                IconButton400.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox401_Click(object sender, EventArgs e)
        {
            if (IconButton401.Visible)
            {
                await browser.ClickXY(629, 351);
                IconButton401.Hide();
            }
            else
            {
                await browser.ClickXY(629, 351);
                IconButton401.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox402_Click(object sender, EventArgs e)
        {
            if (IconButton402.Visible)
            {
                await browser.ClickXY(724, 353);
                IconButton402.Hide();
            }
            else
            {
                await browser.ClickXY(724, 353);
                IconButton402.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox403_Click(object sender, EventArgs e)
        {
            if (IconButton403.Visible)
            {
                await browser.ClickXY(821, 350);
                IconButton403.Hide();
            }
            else
            {
                await browser.ClickXY(821, 350);
                IconButton403.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox404_Click(object sender, EventArgs e)
        {
            if (IconButton404.Visible)
            {
                await browser.ClickXY(530, 447);
                IconButton404.Hide();
            }
            else
            {
                await browser.ClickXY(530, 447);
                IconButton404.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox405_Click(object sender, EventArgs e)
        {
            if (IconButton405.Visible)
            {
                await browser.ClickXY(627, 448);
                IconButton405.Hide();
            }
            else
            {
                await browser.ClickXY(627, 448);
                IconButton405.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox406_Click(object sender, EventArgs e)
        {
            if (IconButton406.Visible)
            {
                await browser.ClickXY(724, 447);
                IconButton406.Hide();
            }
            else
            {
                await browser.ClickXY(724, 447);
                IconButton406.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox407_Click(object sender, EventArgs e)
        {
            if (IconButton407.Visible)
            {
                await browser.ClickXY(819, 446);
                IconButton407.Hide();
            }
            else
            {
                await browser.ClickXY(819, 446);
                IconButton407.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox408_Click(object sender, EventArgs e)
        {
            if (IconButton408.Visible)
            {
                await browser.ClickXY(530, 546);
                IconButton408.Hide();
            }
            else
            {
                await browser.ClickXY(530, 546);
                IconButton408.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox409_Click(object sender, EventArgs e)
        {
            if (IconButton409.Visible)
            {
                await browser.ClickXY(628, 546);
                IconButton409.Hide();
            }
            else
            {
                await browser.ClickXY(628, 546);
                IconButton409.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox410_Click(object sender, EventArgs e)
        {
            if (IconButton410.Visible)
            {
                await browser.ClickXY(722, 546);
                IconButton410.Hide();
            }
            else
            {
                await browser.ClickXY(722, 546);
                IconButton410.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox411_Click(object sender, EventArgs e)
        {
            if (IconButton411.Visible)
            {
                await browser.ClickXY(822, 545);
                IconButton411.Hide();
            }
            else
            {
                await browser.ClickXY(822, 545);
                IconButton411.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox412_Click(object sender, EventArgs e)
        {
            if (IconButton412.Visible)
            {
                await browser.ClickXY(530, 642);
                IconButton412.Hide();
            }
            else
            {
                await browser.ClickXY(530, 642);
                IconButton412.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox413_Click(object sender, EventArgs e)
        {
            if (IconButton413.Visible)
            {
                await browser.ClickXY(625, 641);
                IconButton413.Hide();
            }
            else
            {
                await browser.ClickXY(625, 641);
                IconButton413.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox414_Click(object sender, EventArgs e)
        {
            if (IconButton414.Visible)
            {
                await browser.ClickXY(725, 642);
                IconButton414.Hide();
            }
            else
            {
                await browser.ClickXY(725, 642);
                IconButton414.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private async void PictureBox415_Click(object sender, EventArgs e)
        {
            if (IconButton415.Visible)
            {
                await browser.ClickXY(821, 644);
                IconButton415.Hide();
            }
            else
            {
                await browser.ClickXY(821, 644);
                IconButton415.Show();
            }
            IconButtonAceptReCaptcha.Text = (string)await Iframe.FExecuteScript("document.querySelector('#recaptcha-verify-button').textContent");
        }

        private void HideChecks()
        {
            IconButton300.Hide();
            IconButton301.Hide();
            IconButton302.Hide();
            IconButton303.Hide();
            IconButton304.Hide();
            IconButton305.Hide();
            IconButton306.Hide();
            IconButton307.Hide();
            IconButton308.Hide();

            IconButton400.Hide();
            IconButton401.Hide();
            IconButton402.Hide();
            IconButton403.Hide();
            IconButton404.Hide();
            IconButton405.Hide();
            IconButton406.Hide();
            IconButton407.Hide();
            IconButton408.Hide();
            IconButton409.Hide();
            IconButton410.Hide();
            IconButton411.Hide();
            IconButton412.Hide();
            IconButton413.Hide();
            IconButton414.Hide();
            IconButton415.Hide();
        }

        #endregion

        private async Task Block()
        {
            bool block = await Asatru.BlockAesir(UserId, 3, Token);
            if (block)
            {
                PanelBlockGate.Show();
                await IconButtonBlockGateClose.OnClickAsync();
                Owner.HideIconActive("frigg");
                PanelBlockGate.Hide();
                if (browser != null)
                {
                    await browser.Kill(web);
                }
                this.Close();
            }
        }
    }
}