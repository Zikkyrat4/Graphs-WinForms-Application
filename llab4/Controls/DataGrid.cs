using System.Drawing;
using System.Windows.Forms;

class BeautifulTable
{
    private DataGridView dataGridView;

    public BeautifulTable(DataGridView dataGridView)
    {
        this.dataGridView = dataGridView;

        // Настройка стилей для таблицы
        dataGridView.BorderStyle = BorderStyle.None;
        dataGridView.BackgroundColor = Color.White;
        dataGridView.GridColor = Color.LightGray;
        dataGridView.ForeColor = Color.Black;
        dataGridView.Font = new Font("Arial", 10);

        // Настройка стилей для заголовков столбцов
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);

        // Настройка стилей для строк таблицы
        DataGridViewCellStyle style = new DataGridViewCellStyle();
        style.BackColor = Color.LightGray;
        dataGridView.AlternatingRowsDefaultCellStyle = style;

        // Авто-размер столбцов
        dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }
}