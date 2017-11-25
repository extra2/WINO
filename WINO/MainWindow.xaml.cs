using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using WINO.Properties;
using Microsoft.Win32;
using System.Diagnostics;
using Novacode;

namespace WINO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Term> terms = new List<Term>();
        Term term = new Term();
        int lastSelected = 0;
        public MainWindow()
        {
            InitializeComponent();
            // add +1 item to terms for now ======
            terms.Add(new Term());
            updateListView();
        }



        private void listViewPojecia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = listViewPojecia.SelectedIndex;
            if (selected == -1) return;
            lastSelected = selected;
            term.definition = terms[selected].definition;
            term.term = terms[selected].term;
            term.tag = terms[selected].tag;
            term.iKnowThat = terms[selected].iKnowThat;
            setListView();
        }

        private void buttonDodaj_Click(object sender, RoutedEventArgs e)
        {
            int selected = lastSelected;
            if (selected == -1 || selected + 1 > listViewPojecia.Items.Count) return;
            terms[selected].definition = textBoxDefinicja.Text;
            terms[selected].term = textBoxPojecie.Text;
            terms[selected].tag = textBoxTag.Text;
            terms[selected].iKnowThat = checkBoxZnam.IsChecked.Value;
            updateListView();
        }
        private void updateListView()
        {
            listViewPojecia.Items.Clear();
            foreach (Term ter in terms)
                listViewPojecia.Items.Add(ter);
            listViewPojecia.SelectedIndex = lastSelected;
        }

        private void buttonAddItem_Click(object sender, RoutedEventArgs e)
        {
            terms.Add(new Term());
            lastSelected = terms.Count - 1;
            updateListView();
        }

        private void setListView()
        {
            textBoxDefinicja.Text = term.definition;
            textBoxPojecie.Text = term.term;
            textBoxTag.Text = term.tag;
            checkBoxZnam.IsChecked = term.iKnowThat;
        }

        private void buttonRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (lastSelected >= 0)
            {
                terms.RemoveAt(lastSelected);
                lastSelected--;
                updateListView();
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "WIN files (*.win)|*.win|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
                WriteToBinaryFile(saveFileDialog1.FileName, terms, false);
        }



        //load and save terms:
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "WIN files (*.win)|*.win|All files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.RestoreDirectory = true;
            if (OFD.ShowDialog() == true)
            {
                terms = ReadFromBinaryFile<List<Term>>(OFD.FileName);
                lastSelected = terms.Count - 1;
            }
            updateListView();
        }

        private void buttonPDF_Click(object sender, RoutedEventArgs e)
        {
            PDFOptions PDFOWindow = new PDFOptions(terms, listViewPojecia);
            PDFOWindow.Show();
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonDOCX_Click(object sender, RoutedEventArgs e)
        {
            // --- getting filename
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = ".docx files (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == false)
            {
                MessageBox.Show("Wystąpił problem!");
                return;
            }
            // getting filename ---
            // --- create document
            DocX doc = DocX.Create(saveFileDialog1.FileName);
            // create document ---
            // -- create font
            Formatting termFont = new Formatting();
            termFont.FontFamily = new System.Drawing.FontFamily("Times New Roman");
            termFont.Size = 14D;
            // create font ---
            // adding text to document
            foreach (Term term in terms)
            {
                doc.InsertParagraph(term.term + " " + term.definition + "\n", false, termFont);
            }
            try
            {
                doc.Save();
            }
            catch
            {
                MessageBox.Show("Coś poszło nie tak.");
                return;
            }
            Process.Start("WINWORD.EXE", saveFileDialog1.FileName);
        }

        private void buttonInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("------ W.I.N Beta -------\n\n");
        }
    }
}
