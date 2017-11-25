using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WINO
{
    /// <summary>
    /// Interaction logic for PDFOptions.xaml
    /// </summary>
    public partial class PDFOptions : Window
    {
        private ListView listViewPojecia;
        private List<Term> terms;
        public PDFOptions(List<Term> terms, ListView pojecia)
        {
            InitializeComponent();
            listViewPojecia = pojecia;
            this.terms = terms;
        }
        private void buttonGeneratePDF_Click(object sender, RoutedEventArgs e)
        {
            createPDF();
        }
        private void createPDF() // too long function
        {
            int iloscWierszy;
            int iloscKolumn;
            try
            {
                iloscWierszy = Int32.Parse(textBoxIloscWierszy.Text);
                iloscKolumn = Int32.Parse(textBoxIloscKolumn.Text);
            }
            catch
            {
                MessageBox.Show("Podano nie-liczbę");
                return;
            }
            int numOfTerms = listViewPojecia.Items.Count;
            int termsPerPage = iloscWierszy * iloscKolumn;
            int numOfPages = numOfTerms / termsPerPage;
            if (numOfTerms % termsPerPage > 0) numOfPages++;
            if (iloscKolumn < 1 || iloscKolumn > 4 || iloscWierszy < 1 || iloscWierszy > 10)
            {
                MessageBox.Show("Dozwolona ilość kolumn: 1 do 4\nDozwolona ilość wierszy: 1 do 10");
                return;
            }
            if (numOfTerms == 0)
            {
                MessageBox.Show("Brak pojęć w bazie.");
                return;
            }
            int maxWidth = 4961 / iloscKolumn;
            int maxHeight = 7016 / iloscWierszy;
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            // Create pages
            List<PdfPage> pages = new List<PdfPage>();
            for (int i = 0; i < numOfPages; i++)
            {
                pages.Add(new PdfPage());
                pages[i] = document.AddPage();
                pages[i].Width = 4961;
                pages[i].Height = 7016; // 600ppi HD :D
            }

            // Get an XGraphics object for drawing
            List<XGraphics> gfx = new List<XGraphics>();
            for (int i = 0; i < numOfPages; i++)
            {
                gfx.Add(XGraphics.FromPdfPage(pages[i]));
            }
            // --- colrs for fonts:
            XBrush termColor = XBrushes.Black;
            XBrush definitionColor = XBrushes.Black;
            int fontSize = 70;
            if (radioButtonTermBlack.IsChecked == true) termColor = XBrushes.Black;
            else if (radioButtonTermRed.IsChecked == true) termColor = XBrushes.Red;
            else if (radioButtonTermGreen.IsChecked == true) termColor = XBrushes.Green;
            if (radioButtonDefBlack.IsChecked == true) definitionColor = XBrushes.Black;
            else if (radioButtonDefRed.IsChecked == true) definitionColor = XBrushes.Red;
            else if (radioButtonDefGreen.IsChecked == true) definitionColor = XBrushes.Green;
            if (radioButtonBig.IsChecked == true) fontSize = 100;
            else if (radioButtonMedium.IsChecked == true) fontSize = 80;
            else if (radioButtonSmall.IsChecked == true) fontSize = 60;
            // colors for fonts ---

            // --- Create fonts
            //XFont printFontArial8 = new XFont("Arial", 8, XFontStyle.Regular);
            //XFont printFontArial10 = new XFont("Arial", 10, XFontStyle.Regular);
            //XFont printFontArial10Bold = new XFont("Arial", 10, XFontStyle.Bold);
            //XFont printFontCour10BoldItalic = new XFont("Courier New", 10, XFontStyle.Bold | XFontStyle.Italic);
            //XFont printFontArial14 = new XFont("Arial", 14, XFontStyle.Bold);
            //XFont printFontCour8 = new XFont("Courier New", 8, XFontStyle.Regular);
            XFont printFontTNR60 = new XFont("Times New Roman", fontSize, XFontStyle.Regular);
            XFont printFontTNR60B = new XFont("Times New Roman", fontSize, XFontStyle.Regular | XFontStyle.Bold);
            // create fonts ---
            // --- create XPen to draw a line
            XColor c = XColors.Black;
            XPen blackPen = new XPen(c, 1);
            // create XPen to draw a line  ---
            foreach (XGraphics xgr in gfx)
            {
                for (int i = 0; i < iloscKolumn - 1; i++)
                {
                    xgr.DrawLine(blackPen, xgr.PageSize.Width / iloscKolumn * (i + 1), 0, xgr.PageSize.Width / iloscKolumn * (i + 1), xgr.PageSize.Height);
                }
                for (int i = 0; i < iloscWierszy - 1; i++)
                {
                    xgr.DrawLine(blackPen, 0, xgr.PageSize.Height / iloscWierszy * (i + 1), xgr.PageSize.Width, xgr.PageSize.Height / iloscWierszy * (i + 1));
                }
            }
            // draw line ---
            // --- draw  text
            int pojecie = 0;
            string termToPrint = "";
            string definitionToPrint = "";
            for (int i = 0; i < numOfPages; i++)//strona
            {
                for (int j = 0; j < iloscWierszy; j++)//wiersz
                {
                    for (int k = 0; k < iloscKolumn; k++)//kolumna
                    {
                        if (pojecie > terms.Count - 1) break; //If we have more filds on one pdf sheet than terms, we stop after all terms are printed
                        /*
                         * Ok here is some magic.
                         * Goal is to print terms and definitions using different fonts (size/style)
                         * problem 1) string is too long for one line. Solution: parse words to a string[], 
                         * add word after word to another string and check if size(OX) is correct
                         * problem 2) print all lines on the correct position (solved)
                         */
                        double currentHeigh = 0;
                        termToPrint = terms[pojecie].term;
                        definitionToPrint = terms[pojecie].definition;
                        string[] words = termToPrint.Split(' ');
                        string ready = ""; // one line that fit OX
                        List<string> linesToPrin = new List<string>(); // lines to print
                        double fontHeight = gfx[0].MeasureString("1j", printFontTNR60B).Height;
                        int iter = 0;
                        // --- preparing lines to fit OX. Putting them all into List<string> linesToPrin
                        foreach (string word in words)
                        {
                            if (gfx[0].MeasureString(word + " " + ready, printFontTNR60B).Width < maxWidth) ready += " " + word;
                            else
                            {
                                linesToPrin.Add(ready);
                                ready = "";
                            }
                            iter++;
                            if (iter == words.Length)
                            {
                                linesToPrin.Add(ready);
                            }
                        }
                        // preparing lines to fit OX. Putting them all into List<string> linesToPrin ---
                        // --- printing term in multiple lines
                        int iterator = 1;
                        XPoint point = new XPoint();
                        foreach (string line in linesToPrin)
                        {
                            double pX = (maxWidth - gfx[0].MeasureString(line, printFontTNR60B).Width) / 2 + maxWidth * k;
                            double pY = gfx[0].MeasureString(line, printFontTNR60B).Height * iterator + j * maxHeight;
                            point.X = pX;
                            point.Y = pY;
                            currentHeigh += gfx[0].MeasureString("1j", printFontTNR60B).Height;
                            if (currentHeigh > maxHeight) break;
                            gfx[i].DrawString(line, printFontTNR60B, termColor, point);
                            if (iterator == iloscWierszy) iterator = 1;
                            iterator++;
                        }
                        // printing term in multiple lines ---
                        definitionToPrint = terms[pojecie].definition;
                        words = definitionToPrint.Split(' ');
                        ready = ""; // one line that fit OX
                        int linesPrinted = linesToPrin.Count;
                        linesToPrin.Clear();
                        fontHeight = gfx[0].MeasureString("1j", printFontTNR60).Height;
                        iter = 0;
                        // --- preparing lines to fit OX. Putting them all into List<string> linesToPrin
                        foreach (string word in words)
                        {
                            if (gfx[0].MeasureString(word + " " + ready, printFontTNR60B).Width < maxWidth) ready += " " + word;
                            else
                            {
                                linesToPrin.Add(ready);
                                ready = "";
                            }
                            iter++;
                            if (iter == words.Length)
                            {
                                linesToPrin.Add(ready);
                            }
                        }
                        // preparing lines to fit OX. Putting them all into List<string> linesToPrin ---
                        // --- printing definition in multiple lines
                        iterator = 1;
                        
                        point = new XPoint();
                        double oneLineHeigh = gfx[0].MeasureString("1j", printFontTNR60B).Height;
                        double accessible = maxHeight - linesPrinted * oneLineHeigh;
                        double plus = (accessible - linesToPrin.Count*(int)gfx[0].MeasureString("1j", printFontTNR60B).Height) / 2;
                        if (plus < 0) plus = 0;
                        foreach (string line in linesToPrin)
                        {
                            double pX = (maxWidth - gfx[0].MeasureString(line, printFontTNR60).Width) / 2 + maxWidth * k;
                            double pY = gfx[0].MeasureString(line, printFontTNR60).Height * (iterator + linesPrinted) + j * maxHeight + plus;
                            point.X = pX;
                            point.Y = pY;
                            currentHeigh += gfx[0].MeasureString("1j", printFontTNR60).Height;
                            if (currentHeigh > maxHeight) break;
                            gfx[i].DrawString(line, printFontTNR60, definitionColor, point);
                            if (iterator == iloscWierszy) iterator = 1;
                            iterator++;
                        }
                        // printing definition in multiple lines ---
                        pojecie++;
                    }
                }
            }
            // draw text ---

            // --- saving file
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                document.Save(saveFileDialog1.FileName);
                Process.Start(saveFileDialog1.FileName);
            }
            // --- saving file
            Close();
        }
        private void radioButtonTermBlack_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonTermRed.IsChecked = false;
            radioButtonTermGreen.IsChecked = false;
        }
        private void radioButtonTermRed_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonTermBlack.IsChecked = false;
            radioButtonTermGreen.IsChecked = false;
        }
        private void radioButtonTermGreen_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonTermRed.IsChecked = false;
            radioButtonTermBlack.IsChecked = false;
        }
        private void radioButtonDefBlack_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonDefGreen.IsChecked = false;
            radioButtonDefRed.IsChecked = false;
        }
        private void radioButtonDefGreen_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonDefRed.IsChecked = false;
            radioButtonDefBlack.IsChecked = false;
        }

        private void radioButtonDefRed_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonDefGreen.IsChecked = false;
            radioButtonDefBlack.IsChecked = false;
        }
        private void radioButtonMedium_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonBig.IsChecked = false;
            radioButtonSmall.IsChecked = false;
        }

        private void radioButtonBig_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonMedium.IsChecked = false;
            radioButtonSmall.IsChecked = false;
        }

        private void radioButtonSmall_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonBig.IsChecked = false;
            radioButtonMedium.IsChecked = false;
        }
    }
}
