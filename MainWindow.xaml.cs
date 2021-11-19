using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;
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
using System;

namespace TestApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileInfo _file1;
        private FileInfo _file2;

        private DateTime _dateFrom = DateTime.MinValue;
        private DateTime _dateBy = DateTime.MaxValue;

        public MainWindow()
        {
            InitializeComponent();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Реализация загрузки из файла
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private List<Package> LoadFromExcel(FileInfo file)
        {
            // Считывание начинается со втророй строки, т.к. первая - это шапка
            int row = 2, col = 1;

            try
            {
                List<Package> output = new();

                using var package = new ExcelPackage(file);

                package.LoadAsync(file).GetAwaiter().GetResult();

                var ws = package.Workbook.Worksheets[0];

                while (string.IsNullOrEmpty(ws.Cells[row, col].Value?.ToString()) == false)
                {
                    Package p = new();

                    p.Id = int.Parse(ws.Cells[row, col].Value?.ToString());
                    p.Name = ws.Cells[row, col + 1].Value?.ToString() ?? string.Empty;
                    p.Cipher = ws.Cells[row, col + 2].Value?.ToString() ?? string.Empty;
                    p.DateFrom = ws.Cells[row, col + 3].Value != null ? Convert.ToDateTime(ws.Cells[row, col + 3].Value) : DateTime.MinValue;
                    p.DateBy = ws.Cells[row, col + 4].Value != null ? Convert.ToDateTime(ws.Cells[row, col + 4].Value) : DateTime.MaxValue;
                    output.Add(p);
                    row++;
                }

                return output;
            }
            catch (IOException)
            {
                MessageBox.Show("Файл " + file.Name + " используется");
                return new List<Package>();
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Формат файла некорректный");
                return new List<Package>();
            }
            catch (FormatException)
            {
                MessageBox.Show($"В строке {row} некорректный формат данных");
                return new List<Package>();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return new List<Package>();
            }

        }

        /// <summary>
        /// Проверка дат
        /// </summary>
        /// <param name="from"></param>
        /// <param name="by"></param>
        /// <returns></returns>
        private bool CheckDate(DateTime from, DateTime by)
            => (from >= _dateFrom && from <= _dateBy) && (by >= _dateFrom && by <= _dateBy);

        /// <summary>
        /// Реализация обработки выборки
        /// </summary>
        /// <param name="pack1">Данные из первого файла</param>
        /// <param name="pack2">Данные из второго файла</param>
        /// <returns></returns>
        private List<NewPackage> SampleProcessing(List<Package> pack1, List<Package> pack2)
        {
            List<NewPackage> output = new();

            if (pack1?.Any() == false || pack2?.Any() == false)
                return output;

            foreach (var p in pack1)
            {
                NewPackage np = new NewPackage(p);
                np.IsExt = 0;

                var p2 = pack2.Find(x => x.Cipher == p.Cipher);
                if (p2 != default)
                {
                    np.IsExt = 1;
                    np.ExtID = p2.Id;
                    np.DateFrom = p.DateFrom < p2.DateFrom ? p.DateFrom : p2.DateFrom;
                    np.DateBy = p.DateBy > p2.DateBy ? p.DateBy : p2.DateBy;

                    pack2.Remove(p2);
                }

                if (CheckDate(np.DateFrom, np.DateBy))
                    output.Add(np);
            }

            foreach (var p in pack2)
            {
                NewPackage np = new NewPackage(p);
                np.IsExt = 1;
                np.ExtID = np.Id;
                np.Id = default;

                if (CheckDate(np.DateFrom, np.DateBy))
                    output.Add(np);
            }

            return output;
        }

        private void DateFrom_SelectedDatesChange(Object sender, EventArgs e)
        {
            _dateFrom = DateFrom.SelectedDate.HasValue ? DateFrom.SelectedDate.Value : DateTime.MinValue;
        }

        private void DateBy_SelectedDatesChange(Object sender, EventArgs e)
        {
            _dateBy = DateBy.SelectedDate.HasValue ? DateBy.SelectedDate.Value : DateTime.MaxValue;
        }

        /// <summary>
        /// Метод для выбора загружаемого файла
        /// </summary>
        /// <returns></returns>
        private FileInfo OpenFileDlg()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "xlsx | *.xlsx";

            bool? result = dlg.ShowDialog();

            string filePath;

            if (result.HasValue && result.Value)
            {
                filePath = dlg.FileName;
                return new FileInfo(filePath);
            }

            return null;
        }

        private void Open_file1(object sender, RoutedEventArgs e)
        {
            _file1 = OpenFileDlg();
            label1.Content = _file1 == null ? "" : $"Загружен {_file1.Name}";
        }

        private void Open_file2(object sender, RoutedEventArgs e)
        {
            _file2 = OpenFileDlg();
            label2.Content = _file2 == null ? "" : $"Загружен {_file2.Name}";
        }

        /// <summary>
        /// Реализация обработки выборки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sample_Process_Button_Click(object sender, RoutedEventArgs e)
        {

            lView.ItemsSource = null;
            lView.Items.Clear();

            // Проверка загрузки файлов
            if (_file1 == null || _file2 == null)
            {
                MessageBox.Show(_file1 == null && _file2 == null ? "Файл 1 и 2 не загружены"
                                                                 : _file1 == null ? "Файл 1 не загружен"
                                                                                  : "Файл 2 не загружен", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pack1 = LoadFromExcel(_file1);
            var pack2 = LoadFromExcel(_file2);

            if (_dateFrom > _dateBy)
            {
                MessageBox.Show("Дата начала выборки не может превышать дату окончания выборки", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var res = SampleProcessing(pack1, pack2);

            if (res?.Any() == true)
            {
                lView.ItemsSource = res;
                MessageBox.Show("Слияние прошло успешно");
            }
            else
            {
                MessageBox.Show("Результат слияния пустой");
            }
        }


    }
}
