using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace llab4
{
    public partial class Form1 : Form
    {
        private List<Vertex> vertices = new List<Vertex>();// Список всех вершин
        private List<Edge> edges = new List<Edge>(); //Список всех ребер

        private List<Edge> connectedEdges = new List<Edge>();// Список всез соединенных графов

        private Vertex selectedVertex;//выбранный граф

        private ContextMenuStrip vertexContextMenu;//меню


        private bool selectingFirstVertex = true;
        private Vertex firstVertex;
        private Vertex secondVertex;

        private bool isDragging = false; // Проверка нажатия клавиши
        private PointF previousLocation;



        public Form1()
        {
            InitializeComponent();
            pictureBox1.MouseClick += pictureBox1_MouseClick;

            // Создание контекстного меню для вершин
            vertexContextMenu = new ContextMenuStrip();

            // Добавление пунктов меню
            ToolStripMenuItem connectItem = new ToolStripMenuItem("Соединить два графа");
            connectItem.Click += ConnectItem_Click;
            vertexContextMenu.Items.Add(connectItem);

            ToolStripMenuItem renameItem = new ToolStripMenuItem("Переименовать");
            renameItem.Click += RenameItem_Click;
            vertexContextMenu.Items.Add(renameItem);

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Удалить");
            deleteItem.Click += DeleteItem_Click;
            vertexContextMenu.Items.Add(deleteItem);

            ApplyButtonStyle(button1);
            ApplyButtonStyle(button2);
            ApplyButtonStyle(button3);
            ApplyButtonStyle(button4);
            ApplyButtonStyle(button5);
            ApplyButtonStyle(button6);
            ApplyButtonStyle(button7);
            ApplyButtonStyle(button8);
            ApplyButtonStyle(button9);
            ApplyButtonStyle(button10);
            ApplyButtonStyle(button11);
            ApplyButtonStyle(button12);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            BeautifulTable beautifulTable = new BeautifulTable(dataGridView1);
            BeautifulTable beautifulTable2 = new BeautifulTable(dataGridView2);
            BeautifulTable beautifulTable3 = new BeautifulTable(dataGridView3);
            dataGridView1.Columns.Add(" ", "Матрица");

            dataGridView2.Columns.Add("ComponentIndex", "Индекс компонента");
            dataGridView2.Columns.Add("Component", "Компоненты связности");

            dataGridView3.Columns.Add(" ", "Список смежности");

        }
        //Стили для кнопок
        private void ApplyButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(102, 0, 255);
            button.ForeColor = Color.White;

            button.FlatAppearance.BorderColor = Color.FromArgb(51, 0, 102);
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(22, 112, 184);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, 112, 184);

            button.Font = new Font("Arial", 8, FontStyle.Bold);
        }
        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            SmoothingMode smoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Включаем антиалиасинг

            foreach (Vertex vertex in vertices)
            {
                int diameter = 60;
                int x = vertex.X - diameter / 2;
                int y = vertex.Y - diameter / 2;

                // Создаем градиентную кисть для заливки круга
                LinearGradientBrush fillBrush = new LinearGradientBrush(new Rectangle(x, y, diameter, diameter), Color.LightBlue, Color.White, 45f);

                // Отрисовка круга с градиентной заливкой
                g.FillEllipse(fillBrush, x, y, diameter, diameter);

                Pen borderPen = new Pen(Color.DarkBlue, 2f);

                // Отрисовка границы круга с использованием антиалиасинга
                g.DrawEllipse(borderPen, x, y, diameter, diameter);

                // Отрисовка текста в центре круга
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(vertex.Name, new Font("Arial", 10), Brushes.Black, new Rectangle(x, y, diameter, diameter), format);
            }

            g.SmoothingMode = smoothing; // Возвращаем изначальное состояние антиалиасинга

            foreach (Edge edge in edges)
            {
                Pen smoothPen = new Pen(Color.Purple, 3);
                smoothPen.DashStyle = DashStyle.Dot;
                smoothPen.StartCap = LineCap.Round; // Закругляем начало линии
                smoothPen.EndCap = LineCap.Round; // Закругляем конец линии
                g.DrawLine(smoothPen, edge.Start.X, edge.Start.Y, edge.End.X, edge.End.Y); // Нарисовать гладкую линию

            }

            g.SmoothingMode = smoothing; // Возвращаем изначальное состояние антиалиасинга
        }


        //-----------------------------------------------------------------
        //Обработчик нажатий и перемещения графа
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (Vertex vertex in vertices)
            {
                int diameter = 60;
                int x = vertex.X - diameter / 2;
                int y = vertex.Y - diameter / 2;

                if (e.X >= x && e.X <= x + diameter && e.Y >= y && e.Y <= y + diameter)
                {
                    selectedVertex = vertex;
                    vertexContextMenu.Show(pictureBox1, e.Location);
                    return;
                }
            }
            if (selectedDFSVertex == null)//визуализация DFS
            {
                DepthFirstSearch(selectedDFSVertex);
            }
            vertices.Add(new Vertex("V" + vertices.Count, e.X, e.Y));
            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (Vertex vertex in vertices)
            {
                int diameter = 60;
                int x = vertex.X - diameter / 2;
                int y = vertex.Y - diameter / 2;

                if (e.X >= x && e.X <= x + diameter && e.Y >= y && e.Y <= y + diameter)
                {
                    isDragging = true;
                    previousLocation = new PointF(e.X, e.Y);
                    selectedVertex = vertex;
                    return;
                }
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedVertex != null)
            {
                int dx = e.X - (int)previousLocation.X;
                int dy = e.Y - (int)previousLocation.Y;

                // Определяем долю смещения, на которую переместить вершину
                double moveRatio = 1; // Например, перемещаем вершину на 20% от смещения мыши
                int moveX = (int)(dx * moveRatio);
                int moveY = (int)(dy * moveRatio);

                selectedVertex.X += moveX;
                selectedVertex.Y += moveY;

                // Обновляем все соединенные ребра
                foreach (Edge edge in edges)
                {
                    if (edge.Start == selectedVertex || edge.End == selectedVertex)
                    {
                        edge.UpdateEdge(selectedVertex, moveX, moveY);
                    }
                }

                pictureBox1.Invalidate();
                previousLocation = new PointF(previousLocation.X + moveX, previousLocation.Y + moveY); // Обновляем предыдущую позицию для корректного смещения
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                pictureBox1.Invalidate();
            }
        }

        //----------------------------------------------------------
        // Функции управления графами
        private void DeleteItem_Click(object sender, EventArgs e)
        {
            if (selectedVertex != null)
            {
                vertices.Remove(selectedVertex);

                // Удаляем все ребра, связанные с удаляемой вершиной
                edges.RemoveAll(edge => edge.Start == selectedVertex || edge.End == selectedVertex);
                connectedEdges.RemoveAll(edge => edge.Start == selectedVertex || edge.End == selectedVertex);

                selectedVertex = null;
                pictureBox1.Invalidate();
            }
        }

        private void AddConnectedEdge(Vertex start, Vertex end)
        {
            connectedEdges.Add(new Edge(start, end));
        }

        private void MergeGraphs()
        {
            // Создаем ребро, соединяющее две вершины в двух графах
            Edge newEdge = new Edge(firstVertex, secondVertex);

            // Добавляем новое ребро в список ребер
            edges.Add(newEdge);

            // Добавляем соединенное ребро для хранения связей
            AddConnectedEdge(firstVertex, secondVertex);

            // Обновляем изображение
            pictureBox1.Invalidate();
        }

        private void ConnectItem_Click(object sender, EventArgs e)
        {
            if (selectingFirstVertex)
            {
                // Выбор первой вершины
                selectingFirstVertex = false;
                firstVertex = selectedVertex;
                //MessageBox.Show("Выберите вторую вершину для соединения.");
            }
            else
            {
                // Выбор второй вершины и объединение графов
                secondVertex = selectedVertex;
                selectingFirstVertex = true;

                // Объединяем графы
                MergeGraphs();
            }
        }
        private void RenameItem_Click(object sender, EventArgs e)
        {
            string newName = InputDialog.Show("Введите новое имя для вершины:", "Переименовать", selectedVertex.Name);

            if (!string.IsNullOrEmpty(newName))
            {
                selectedVertex.Name = newName;
                pictureBox1.Invalidate();
            }
        }

        //-----------------------------------------------------------
        // Создаю матрицу для заполнения DataGrid
        private int[,] CreateAdjacencyMatrix()
        {
            int verticesCount = vertices.Count;
            int[,] adjacencyMatrix = new int[verticesCount, verticesCount];

            // Инициализируем матрицу нулями
            for (int i = 0; i < verticesCount; i++)
            {
                for (int j = 0; j < verticesCount; j++)
                {
                    adjacencyMatrix[i, j] = 0;
                }
            }
            foreach (Edge edge in edges)
            {
                int startIndex = vertices.IndexOf(edge.Start);
                int endIndex = vertices.IndexOf(edge.End);

                // Заполняем матрицу смежности в соответствии с ребрами
                adjacencyMatrix[startIndex, endIndex] = 1;
                adjacencyMatrix[endIndex, startIndex] = 1; // для неориентированного графа
            }

            return adjacencyMatrix;
        }

        // А здесь из матрицы заполняю DataGrid
        private void PopulateDataGridViewWithAdjacencyMatrix()
        {
            int[,] adjacencyMatrix = CreateAdjacencyMatrix();

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // Добавляем колонки с числами для вершин графов
            for (int i = 0; i < vertices.Count; i++)
            {
                dataGridView1.Columns.Add(i.ToString(), "V" + i); // Называем вершины как V0, V1, ...
            }

            // Добавляем строки с названиями графов
            for (int i = 0; i < vertices.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].HeaderCell.Value = "V" + i; // Название графа
            }

            // Заполняем ячейки таблицы значением матрицы смежности
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices.Count; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = adjacencyMatrix[i, j];
                }
            }
        }

        // Кнопка запуска заполнения матрицы
        private void button1_Click(object sender, EventArgs e)
        {
            CreateAdjacencyMatrix();
            PopulateDataGridViewWithAdjacencyMatrix();
        }


        public int CountEdges() { return edges.Count; }
        public int CountVertex() { return vertices.Count; }

        private void button2_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Количество ребер на графе: " + CountEdges().ToString());
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Количество вершин на графе: " + CountVertex().ToString());
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            int sumDegrees = 0;
            foreach (Vertex vertex in vertices)
            {
                int degree = 0;
                foreach (Edge edge in edges)
                {
                    if (edge.Start == vertex || edge.End == vertex)
                    {
                        degree++;
                    }
                }
                sumDegrees += degree;
            }

            MessageBox.Show("Сумма степеней всех вершин графа: " + sumDegrees);
        }

        //Загрузка графа
        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    vertices.Clear();
                    edges.Clear();
                    connectedEdges.Clear();

                    // Загрузка вершин
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');

                        if (parts.Length == 3)
                        {
                            vertices.Add(new Vertex(parts[0], int.Parse(parts[1]), int.Parse(parts[2])));
                        }
                    }

                    // Загрузка рёбер
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');

                        if (parts.Length == 4)
                        {
                            Vertex start = vertices.Find(v => v.Name == parts[0]);
                            Vertex end = vertices.Find(v => v.Name == parts[1]);
                            edges.Add(new Edge(start, end));
                            connectedEdges.Add(new Edge(start, end));
                        }
                    }

                    pictureBox1.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке файла: " + ex.Message);
                }
            }
        }

        // Сохранение графа
        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    List<string> lines = new List<string>();
                    foreach (Vertex vertex in vertices)
                    {
                        lines.Add($"{vertex.Name},{vertex.X},{vertex.Y}");
                    }
                    foreach (Edge edge in edges)
                    {
                        lines.Add($"{edge.Start.Name},{edge.End.Name}");
                    }

                    File.WriteAllLines(filePath, lines);

                    MessageBox.Show("Граф успешно сохранен в файл.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении файла: " + ex.Message);
                }
            }
        }



        // Вывод изолированных вершин
        private List<Vertex> GetIsolatedVertices()
        {
            List<Vertex> isolatedVertices = new List<Vertex>();

            foreach (Vertex vertex in vertices)
            {
                bool isConnected = false;
                foreach (Edge edge in connectedEdges)
                {
                    if (edge.Start == vertex || edge.End == vertex)
                    {
                        isConnected = true;
                        break;
                    }
                }
                if (!isConnected)
                {
                    isolatedVertices.Add(vertex);
                }
            }

            return isolatedVertices;
        }
        private void button7_Click_1(object sender, EventArgs e)
        {
            List<Vertex> isolatedVertices = GetIsolatedVertices();

            if (isolatedVertices.Count > 0)
            {
                string message = "Изолированные вершины: ";
                foreach (Vertex vertex in isolatedVertices)
                {
                    message += vertex.Name + " ";
                }
                MessageBox.Show(message);
            }
            else
            {
                MessageBox.Show("Изолированные вершины отсутствуют.");
            }
        }
        //Алгоритм выделения компонет связности в несвязном графе
        private List<List<Vertex>> FindConnectedComponents()
        {
            List<List<Vertex>> connectedComponents = new List<List<Vertex>>();
            // Создается пустой список, в который будут добавляться компоненты связности

            HashSet<Vertex> visitedVertices = new HashSet<Vertex>();
            // Создается множество для отслеживания вершин, которые уже были посещены

            //Для каждой вершины в графе выполняется поиск в глубину,
            //если вершина не была посещена ранее.
            //Если вершина не связана с другой уже посещенной вершиной,
            //то она будет добавлена в новую компоненту связности.
            foreach (Vertex vertex in vertices)
            {
                if (!visitedVertices.Contains(vertex))
                {
                    List<Vertex> component = new List<Vertex>();
                    DFS(vertex, visitedVertices, component);
                    connectedComponents.Add(component);
                }
            }

            return connectedComponents;
        }

        private void DFS(Vertex currentVertex, HashSet<Vertex> visitedVertices, List<Vertex> component)
        {
            //  метод выполняет обход в глубину из текущей вершины currentVertex.
            visitedVertices.Add(currentVertex);
            component.Add(currentVertex);
            // добавляет текущую вершину в список посещенных и в компоненту


            //Затем он рекурсивно вызывает себя для всех смежных вершин, которые не были посещены ранее.
            foreach (Edge edge in edges)
            {
                if (edge.Start == currentVertex && !visitedVertices.Contains(edge.End))
                {
                    DFS(edge.End, visitedVertices, component);
                }
                else if (edge.End == currentVertex && !visitedVertices.Contains(edge.Start))
                {
                    DFS(edge.Start, visitedVertices, component);
                }
            }
        }
        // заполнение datagrid2
        private void PopulateDataGridViewWithConnectedComponents(List<List<Vertex>> connectedComponents)
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();

            dataGridView2.Columns.Add("ComponentIndex", "Индекс компонента");
            dataGridView2.Columns.Add("Component", "Компоненты связности");

            int componentIndex = 1;
            foreach (List<Vertex> component in connectedComponents)
            {
                dataGridView2.Rows.Add(componentIndex, string.Join(", ", component.Select(v => v.Name)));
                componentIndex++;
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            List<List<Vertex>> connectedComponents = FindConnectedComponents();
            //вызывается метод FindConnectedComponents для нахождения компонент связности в графе

            //Затем результаты выводятся в диалоговом окне

            int componentNumber = 1;
            foreach (List<Vertex> component in connectedComponents)
            {
                string verticesInComponent = string.Join(", ", component.Select(v => v.Name));
                PopulateDataGridViewWithConnectedComponents(connectedComponents);
                componentNumber++;
            }
            
        }
        //-----------------------------------------------------------------------------
        //Визуализация DFS
        private List<Vertex> dfsVisitedVertices = new List<Vertex>();

        private Vertex selectedDFSVertex;//выбранная вершина для визуализации

        private void DepthFirstSearch(Vertex startVertex)
        {
            //Очищает список посещенных вершин dfsVisitedVertices.
            dfsVisitedVertices.Clear();
            DFS(startVertex);
        }

        private void DFS(Vertex currentVertex)
        {
            dfsVisitedVertices.Add(currentVertex);
            // добавляю вершину в список посещенных

            // для каждого ребра проходит проверка на инцендентность текущей вершины
            foreach (Edge edge in edges)
            {
                //Если обнаруживается инцидентное ребро
                //и его конечная вершина еще не была посещена, то рисуется стрелка
                if (edge.Start == currentVertex && !dfsVisitedVertices.Contains(edge.End))
                {
                    // Рисуем стрелку от текущей вершины к следующей вершине
                    using (Pen arrowPen = new Pen(Color.Red, 3))
                    {
                        arrowPen.CustomEndCap = new AdjustableArrowCap(7, 7);
                        Graphics g = pictureBox1.CreateGraphics();
                        g.DrawLine(arrowPen, currentVertex.X, currentVertex.Y, edge.End.X, edge.End.Y);
                    }
                    // рекурсивный вызов DFS для следующей вершины.
                    DFS(edge.End);
                }

                //если обнаруживается инцидентное ребро, и его начальная вершина еще не была посещена
                else if (edge.End == currentVertex && !dfsVisitedVertices.Contains(edge.Start))
                {
                    // Рисуем стрелку от текущей вершины к предыдущей вершине
                    using (Pen arrowPen = new Pen(Color.Red, 3))
                    {
                        arrowPen.CustomEndCap = new AdjustableArrowCap(7, 7);
                        Graphics g = pictureBox1.CreateGraphics();
                        g.DrawLine(arrowPen, currentVertex.X, currentVertex.Y, edge.Start.X, edge.Start.Y);
                    }

                    DFS(edge.Start);
                }
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            if (selectedVertex != null)
            {
                selectedDFSVertex = selectedVertex;
                DepthFirstSearch(selectedDFSVertex);
            }
            else
            {
                MessageBox.Show("Выберите вершину для запуска DFS.");
            }
        }

        //Сохранение изображения
        private void SaveGraphImage()
        {
            // Создаем новое изображение и устанавливаем его размер равным размеру PictureBox
            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            // Создаем графический объект из Bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Отрисовываем содержимое PictureBox на Bitmap
                g.CopyFromScreen(this.Location.X + pictureBox1.Location.X, this.Location.Y + pictureBox1.Location.Y, 0, 0, pictureBox1.Size);
            }

            // Диалог для сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                // Получаем расширение выбранного файла
                string fileExt = Path.GetExtension(filePath).ToLower();

                // Сохраняем изображение в выбранный формат
                switch (fileExt)
                {
                    case ".png":
                        bitmap.Save(filePath, ImageFormat.Png);
                        break;
                    case ".jpg":
                        bitmap.Save(filePath, ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        bitmap.Save(filePath, ImageFormat.Bmp);
                        break;
                    default:
                        MessageBox.Show("Неподдерживаемый формат изображения.");
                        break;
                }

                MessageBox.Show("Изображение графа успешно сохранено.");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SaveGraphImage();
        }


        // очистка формы
        private void ClearForm()
        {
            vertices.Clear();
            edges.Clear();
            connectedEdges.Clear();
            selectedVertex = null;
            firstVertex = null;
            secondVertex = null;
            selectingFirstVertex = true;
            pictureBox1.Invalidate();
        }
        private void button11_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        // Список смжености
        private void PopulateDataGridViewWithAdjacencyList()
        {
            // Создаем пустой список для хранения списка смежности вершин
            List<List<Vertex>> adjacencyList = new List<List<Vertex>>();

            // Заполняем список смежности вершин
            foreach (Vertex vertex in vertices)
            {
                List<Vertex> neighbors = new List<Vertex>();
                foreach (Edge edge in edges)
                {
                    if (edge.Start == vertex)
                    {
                        neighbors.Add(edge.End);
                    }
                    else if (edge.End == vertex)
                    {
                        neighbors.Add(edge.Start);
                    }
                }
                adjacencyList.Add(neighbors);
            }

            // Очищаем DataGridView
            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();

            // Добавляем столбцы для вершин графа
            for (int i = 0; i < vertices.Count; i++)
            {
                dataGridView3.Columns.Add("V" + i, "V" + i);
            }

            // Добавляем строки для отображения списка смежности
            for (int i = 0; i < vertices.Count; i++)
            {
                dataGridView3.Rows.Add();
                dataGridView3.Rows[i].HeaderCell.Value = "V" + i;

                // Заполняем ячейки DataGridView значениями списка смежности
                for (int j = 0; j < adjacencyList[i].Count; j++)
                {
                    dataGridView3.Rows[i].Cells[j].Value = adjacencyList[i][j].Name;
                }
            }
        }
        private void button12_Click(object sender, EventArgs e)
        {
            PopulateDataGridViewWithAdjacencyList();
        }
    }

    public class Vertex
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Vertex(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }
    }

    public class Edge
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
        }
        public void UpdateEdge(Vertex movedVertex, int dx, int dy)
        {
            if (Start == movedVertex)
            {
                Start.X += dx;
                Start.Y += dy;
            }
            else if (End == movedVertex)
            {
                End.X += dx;
                End.Y += dy;
            }
        }
    }
}
