using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;



namespace bilgisayarlıgörme
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> originalImage;
        private Image<Gray, byte> processedImage;




        public Form1()
        {
            InitializeComponent();

            // İşlem seçeneklerini ComboBox'a ekle
            comboBox1.Items.AddRange(new string[]
            {
                "Gri yap", "Y Yap", "Histogram", "KM intensity",
                "KM Öklit RGB", "KM Mahalonobis", "KMeans Mahalonobis ND","Sobel"
            });



            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;



        }

        // Resim yükleme işlemi
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    originalImage = new Image<Bgr, byte>(openFileDialog.FileName);
                    pictureBox1.Image = originalImage.ToBitmap();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Resim yüklenirken hata: {ex.Message}");
                }
            }

        }

        // İşlem uygula
        private void button1_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Lütfen bir resim yükleyin.");
                return;
            }

            switch (comboBox1.SelectedItem.ToString())
            {
                case "Gri yap":
                    ApplyGrayscale();
                    break;
                case "Y Yap":
                    ApplyYChannel();
                    break;
                case "Histogram":
                    ShowHistogram();
                    break;
                case "KM intensity":
                    ApplyKMeansIntensity();
                    break;
                case "KM Öklit RGB":
                    ApplyKMeansOklitRGB();
                    break; ;
                case "KM Mahalonobis":
                    ApplyKMeansMahalanobis();
                    break;
                case "KMeans Mahalonobis ND":
                    ApplyKMeansMahalanobisND();
                    break;
                case "Sobel":
                    ApplySobelEdgeDetection();
                    break;
                default:
                    MessageBox.Show("Geçerli bir işlem seçilmedi.");
                    break;
            }
        }

        // Gri tonlama işlemi
        private void ApplyGrayscale()
        {
            // pictureBox1'den mevcut resmi alıyoruz
            Bitmap image = new Bitmap(pictureBox1.Image);

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    // Pikselin RGB değerlerini alıyoruz
                    Color renk = image.GetPixel(j, i);
                    int r = renk.R;
                    int g = renk.G;
                    int b = renk.B;

                    // RGB ortalamasını alıyoruz
                    int gray = (r + g + b) / 3;

                    // Hesaplanan gri değeri yeni bir renk olarak oluşturuyoruz
                    Color newcolor = Color.FromArgb(gray, gray, gray);

                    // Yeni rengi piksele atıyoruz
                    image.SetPixel(j, i, newcolor);
                }
            }

            // İşlenmiş görüntüyü pictureBox2'ye atıyoruz
            pictureBox2.Image = image;
        }

        private void ApplyYChannel()
        {
            // pictureBox1'den mevcut resmi alıyoruz
            Bitmap image = new Bitmap(pictureBox1.Image);

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    // Pikselin RGB değerlerini alıyoruz
                    Color renk = image.GetPixel(j, i);
                    int r = renk.R;
                    int g = renk.G;
                    int b = renk.B;

                    // Gri ton değerini Y' kanalına göre hesaplıyoruz
                    int yu = (int)(0.299 * r + 0.587 * g + 0.114 * b);

                    // Hesaplanan Y değeriyle gri tonu oluşturuyoruz
                    Color newcolor = Color.FromArgb(yu, yu, yu);

                    // Yeni gri tonu piksellere uyguluyoruz
                    image.SetPixel(j, i, newcolor);
                }
            }

            // İşlenmiş görüntüyü pictureBox1'e geri atıyoruz
            pictureBox2.Image = image;
        }




        private void ShowHistogram()
        {
            // Histogram oluştur
            int[] histogram = new int[256];
            int totalPixelCount = originalImage.Width * originalImage.Height;

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    byte blue = originalImage.Data[y, x, 0];
                    byte green = originalImage.Data[y, x, 1];
                    byte red = originalImage.Data[y, x, 2];

                    int intensity = (int)(0.3 * red + 0.59 * green + 0.11 * blue);
                    histogram[intensity]++;
                }
            }

            // 2. Histogramı pürüzsüzleştirme (isteğe bağlı)
            int[] smoothedHistogram = new int[256];
            for (int i = 1; i < 255; i++)
            {
                smoothedHistogram[i] = (histogram[i - 1] + histogram[i] + histogram[i + 1]) / 3;
            }

            // Histogramı chart üzerinde göster
            chart1.Series.Clear();
            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Histogram",
                Color = System.Drawing.Color.Blue,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
            };
            chart1.Series.Add(series);

            // Histogram verilerini normalize et (görselleştirmeyi iyileştirme)
            int maxHistogramValue = histogram.Max();
            for (int i = 0; i < histogram.Length; i++)
            {
                // Histogramın normalizasyonu (y eksenini daha görünür yapmak için)
                double normalizedValue = (double)histogram[i] / maxHistogramValue * 100;  // Yüzde olarak normalize et
                series.Points.AddXY(i, normalizedValue);
            }

            // Y ekseninin dinamik olarak ayarlanması
            chart1.ChartAreas[0].AxisY.Maximum = 105; // Normalize edilmiş en yüksek değer üzerinden %105 olarak belirlenebilir
            chart1.ChartAreas[0].AxisY.Minimum = 0;  // Başlangıç noktası 0 olmalı
            chart1.ChartAreas[0].AxisX.Minimum = 0;  // Başlangıç noktası 0 olmalı
            chart1.ChartAreas[0].AxisX.Maximum = 255;  // Histogramın en yüksek değeri (256 renk)


        }



        private void ApplyKMeansIntensity()

        {
            // Kullanıcıdan tepe değeri sayısını al
            int k;
            if (!int.TryParse(textBox1.Text, out k) || k <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir tepe değer sayısı giriniz.");
                return;
            }

            // Rastgele merkezler belirle
            Random random = new Random();
            List<int> centers = new List<int>();
            for (int i = 0; i < k; i++)
            {
                centers.Add(random.Next(0, 256));
            }

            listBox2.Items.Clear();
            foreach (var center in centers)
            {
                listBox2.Items.Add($"Merkez: {center}");
            }

            // Histogram oluştur
            int[] histogram = new int[256];
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    byte blue = originalImage.Data[y, x, 0];
                    byte green = originalImage.Data[y, x, 1];
                    byte red = originalImage.Data[y, x, 2];

                    int intensity = (int)(0.3 * red + 0.59 * green + 0.11 * blue);
                    histogram[intensity]++;
                }
            }

            // Histogramı chart üzerinde göster
            chart1.Series.Clear();
            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Histogram",
                Color = System.Drawing.Color.Blue,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
            };
            chart1.Series.Add(series);

            int maxHistogramValue = histogram.Max();
            for (int i = 0; i < histogram.Length; i++)
            {
                double normalizedValue = (double)histogram[i] / maxHistogramValue * 100;
                series.Points.AddXY(i, normalizedValue);
            }

            chart1.ChartAreas[0].AxisY.Maximum = 105;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;

            // K-Means algoritması
            List<List<int>> clusters = new List<List<int>>();
            for (int i = 0; i < k; i++)
            {
                clusters.Add(new List<int>());
            }

            Image<Gray, byte> clusteredImage = new Image<Gray, byte>(originalImage.Width, originalImage.Height);
            bool isConverged = false;
            int iteration = 0;

            while (!isConverged)
            {
                iteration++;

                // Kümeleme işlemi
                foreach (var cluster in clusters)
                {
                    cluster.Clear();
                }

                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        byte blue = originalImage.Data[y, x, 0];
                        byte green = originalImage.Data[y, x, 1];
                        byte red = originalImage.Data[y, x, 2];

                        int intensity = (int)(0.3 * red + 0.59 * green + 0.11 * blue);

                        int closestCenterIndex = 0;
                        double minDistance = double.MaxValue;

                        for (int j = 0; j < k; j++)
                        {
                            double distance = Math.Abs(intensity - centers[j]);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestCenterIndex = j;
                            }
                        }

                        clusters[closestCenterIndex].Add(intensity);
                        clusteredImage.Data[y, x, 0] = (byte)centers[closestCenterIndex];
                    }
                }

                // Yeni merkezleri hesapla
                isConverged = true;
                for (int i = 0; i < k; i++)
                {
                    if (clusters[i].Count > 0)
                    {
                        int newCenter = (int)clusters[i].Average();

                        if (newCenter != centers[i])
                        {
                            isConverged = false;
                        }
                        centers[i] = newCenter;
                    }
                }
            }

            // Sonuçları listBox1'de göster
            listBox1.Items.Clear();

            for (int i = 0; i < k; i++)
            {
                listBox1.Items.Add($"Küme {i + 1}: Merkez: {centers[i]}, Piksel Sayısı: {clusters[i].Count}");
            }

            // Histogram üzerinde son iterasyon değerlerini göster
            foreach (var center in centers)
            {
                if (center >= 0 && center < 256)
                {
                    var point = new System.Windows.Forms.DataVisualization.Charting.DataPoint(center, histogram[center]);
                    point.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                    point.MarkerSize = 8;
                    point.MarkerColor = System.Drawing.Color.Red;
                    series.Points[center].MarkerStyle = point.MarkerStyle;
                    series.Points[center].MarkerSize = point.MarkerSize;
                    series.Points[center].MarkerColor = point.MarkerColor;
                }
            }

            // İşlenmiş görüntüyü göster
            pictureBox2.Image = clusteredImage.ToBitmap();
            label12.Text = $"İterasyon Sayısı: {iteration}";
        }





        private void ApplyKMeansOklitRGB()
        {
            // Kullanıcıdan tepe değeri sayısını al
            int k;
            if (!int.TryParse(textBox1.Text, out k) || k <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir tepe değer sayısı giriniz.");
                return;
            }

            // Rastgele merkezler belirle
            Random random = new Random();
            List<Tuple<byte, byte, byte>> centers = new List<Tuple<byte, byte, byte>>();
            for (int i = 0; i < k; i++)
            {
                byte r = (byte)random.Next(0, 256);
                byte g = (byte)random.Next(0, 256);
                byte b = (byte)random.Next(0, 256);
                centers.Add(new Tuple<byte, byte, byte>(r, g, b));
            }

            listBox2.Items.Clear();
            foreach (var center in centers)
            {
                listBox2.Items.Add($"R: {center.Item1}, G: {center.Item2}, B: {center.Item3}");
            }

            // Histogram oluştur
            int[] histogram = new int[256];
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    byte blue = originalImage.Data[y, x, 0];
                    byte green = originalImage.Data[y, x, 1];
                    byte red = originalImage.Data[y, x, 2];

                    int intensity = (int)(0.3 * red + 0.59 * green + 0.11 * blue);
                    histogram[intensity]++;
                }
            }

            // Histogramı chart üzerinde göster
            chart1.Series.Clear();
            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Histogram",
                Color = System.Drawing.Color.Blue,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
            };
            chart1.Series.Add(series);

            int maxHistogramValue = histogram.Max();
            for (int i = 0; i < histogram.Length; i++)
            {
                double normalizedValue = (double)histogram[i] / maxHistogramValue * 100;
                series.Points.AddXY(i, normalizedValue);
            }

            chart1.ChartAreas[0].AxisY.Maximum = 105;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;

            // K-Means algoritması
            List<List<Tuple<byte, byte, byte>>> clusters = new List<List<Tuple<byte, byte, byte>>>();
            for (int i = 0; i < k; i++)
            {
                clusters.Add(new List<Tuple<byte, byte, byte>>());
            }

            Image<Bgr, byte> clusteredImage = new Image<Bgr, byte>(originalImage.Width, originalImage.Height);
            bool isConverged = false;
            int iteration = 0;

            while (!isConverged)
            {
                iteration++;

                // Kümeleme işlemi
                foreach (var cluster in clusters)
                {
                    cluster.Clear();
                }

                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        byte blue = originalImage.Data[y, x, 0];
                        byte green = originalImage.Data[y, x, 1];
                        byte red = originalImage.Data[y, x, 2];

                        int closestCenterIndex = 0;
                        double minDistance = double.MaxValue;

                        for (int j = 0; j < k; j++)
                        {
                            double distance = Math.Sqrt(
                                Math.Pow(red - centers[j].Item1, 2) +
                                Math.Pow(green - centers[j].Item2, 2) +
                                Math.Pow(blue - centers[j].Item3, 2)
                            );
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestCenterIndex = j;
                            }
                        }

                        clusters[closestCenterIndex].Add(new Tuple<byte, byte, byte>(red, green, blue));
                        clusteredImage.Data[y, x, 0] = centers[closestCenterIndex].Item3;
                        clusteredImage.Data[y, x, 1] = centers[closestCenterIndex].Item2;
                        clusteredImage.Data[y, x, 2] = centers[closestCenterIndex].Item1;
                    }
                }

                // Yeni merkezleri hesapla
                isConverged = true;
                for (int i = 0; i < k; i++)
                {
                    if (clusters[i].Count > 0)
                    {
                        byte newR = (byte)clusters[i].Average(pixel => pixel.Item1);
                        byte newG = (byte)clusters[i].Average(pixel => pixel.Item2);
                        byte newB = (byte)clusters[i].Average(pixel => pixel.Item3);

                        if (newR != centers[i].Item1 || newG != centers[i].Item2 || newB != centers[i].Item3)
                        {
                            isConverged = false;
                        }
                        centers[i] = new Tuple<byte, byte, byte>(newR, newG, newB);
                    }
                }
            }

            // Sonuçları listBox1'de göster
            listBox1.Items.Clear();
            for (int i = 0; i < k; i++)
            {
                listBox1.Items.Add($"Küme {i + 1}: Merkez R: {centers[i].Item1}, G: {centers[i].Item2}, B: {centers[i].Item3}, Piksel Sayısı: {clusters[i].Count}");
            }

           

            // İşlenmiş görüntüyü göster
            pictureBox2.Image = clusteredImage.ToBitmap();
            label12.Text = $"İterasyon Sayısı: {iteration}";
        }


        private void ApplyKMeansMahalanobis()
        {
            // Kullanıcıdan K değerini al
            if (!int.TryParse(textBox1.Text, out int clusterCount) || clusterCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir K değeri girin.");
                return;
            }

            // Orijinal görüntüyü kontrol et
            if (originalImage == null)
            {
                MessageBox.Show("Lütfen bir resim yükleyin.");
                return;
            }

            // Gri tonlama dönüşümü
            Bitmap grayImage = ConvertToGrayscale(originalImage.ToBitmap());

            // Piksel verilerini ve histogramı oluştur
            byte[] pixelValues = ToPixelArray(grayImage);
            int[] histogram = CalculateHistogram(pixelValues);

            // Mahalanobis algoritmasıyla K-means işlemi
            var (resultPixels, initialMeans, finalMeans, iterations, clusterSizes) = PerformKMeansMahalanobis(pixelValues, clusterCount);

            // Başlangıç ve son küme merkezlerini ListBox'lara ekle
            UpdateListBox(listBox2, initialMeans, "Başlangıç Küme Merkezleri");
            UpdateListBox(listBox1, finalMeans, "Son Küme Merkezleri");

            // İterasyon sayısını göster
            label12.Text = $"İterasyon Sayısı: {iterations}";

            // Histogram çizimi
            DrawHistogram(histogram, finalMeans);

            // Sonuç görüntüsünü oluştur ve göster
            Bitmap resultImage = CreateResultBitmap(resultPixels, grayImage.Width, grayImage.Height);
            pictureBox2.Image = resultImage;
        }

        private Bitmap ConvertToGrayscale(Bitmap original)
        {
            int width = original.Width;
            int height = original.Height;
            Bitmap grayImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    byte gray = (byte)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);
                    grayImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            return grayImage;
        }

        private byte[] ToPixelArray(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixelArray = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixelArray[y * width + x] = bitmap.GetPixel(x, y).R;
                }
            }

            return pixelArray;
        }

        private int[] CalculateHistogram(byte[] pixelValues)
        {
            int[] histogram = new int[256];
            foreach (var pixel in pixelValues)
            {
                histogram[pixel]++;
            }
            return histogram;
        }

        private (byte[] resultPixels, double[] initialMeans, double[] finalMeans, int iterations, int[] clusterSizes) PerformKMeansMahalanobis(byte[] pixelValues, int clusterCount)
        {
            Random random = new Random();
            double[] means = new double[clusterCount];
            double[] initialMeans = new double[clusterCount];
            int[] clusterSizes = new int[clusterCount];

            // Başlangıç küme merkezlerini rastgele seç
            for (int i = 0; i < clusterCount; i++)
            {
                means[i] = pixelValues[random.Next(pixelValues.Length)];
                initialMeans[i] = means[i];
            }

            int[] labels = new int[pixelValues.Length];
            int iterations = 0;

            // Kovaryans matrisini manuel hesapla
            double variance = CalculateVariance(pixelValues);
            double inverseCovariance = 1.0 / variance; // Kovaryans matrisinin tersi

            while (iterations < 100)
            {
                bool updated = false;

                // Her piksel için en yakın küme merkezini bul
                for (int i = 0; i < pixelValues.Length; i++)
                {
                    double minDistance = double.MaxValue;
                    int bestCluster = 0;

                    for (int j = 0; j < clusterCount; j++)
                    {
                        double distance = MahalanobisDistance(pixelValues[i], means[j], inverseCovariance);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            bestCluster = j;
                        }
                    }

                    if (labels[i] != bestCluster)
                    {
                        labels[i] = bestCluster;
                        updated = true;
                    }
                }

                if (!updated) break;

                // Küme merkezlerini güncelle
                double[] newMeans = new double[clusterCount];
                clusterSizes = new int[clusterCount];

                for (int i = 0; i < pixelValues.Length; i++)
                {
                    newMeans[labels[i]] += pixelValues[i];
                    clusterSizes[labels[i]]++;
                }

                for (int j = 0; j < clusterCount; j++)
                {
                    if (clusterSizes[j] > 0)
                        means[j] = newMeans[j] / clusterSizes[j];
                }

                iterations++;
            }

            // Sonuç piksellerini oluştur
            byte[] resultPixels = new byte[pixelValues.Length];
            for (int i = 0; i < pixelValues.Length; i++)
            {
                resultPixels[i] = (byte)Clamp((int)means[labels[i]], 0, 255);
            }

            return (resultPixels, initialMeans, means, iterations, clusterSizes);
        }

        private double CalculateVariance(byte[] pixelValues)
        {
            double avg = pixelValues.Select(p => (int)p).Average();

            double variance = pixelValues.Select(val => Math.Pow(val - avg, 2)).Sum() / pixelValues.Length;
            return variance;
        }

        private double MahalanobisDistance(double x, double mean, double inverseCovariance)
        {
            double diff = x - mean;
            return Math.Sqrt(diff * diff * inverseCovariance);
        }

        private Bitmap CreateResultBitmap(byte[] pixels, int width, int height)
        {
            Bitmap resultImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte intensity = pixels[y * width + x];
                    resultImage.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
                }
            }

            return resultImage;
        }

        private void DrawHistogram(int[] histogram, double[] centers)
        {
            chart1.Series.Clear();

            int maxHistogramValue = histogram.Max();
            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Histogram",
                Color = System.Drawing.Color.Blue,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
            };

            chart1.Series.Add(series);

            for (int i = 0; i < histogram.Length; i++)
            {
                double normalizedValue = (double)histogram[i] / maxHistogramValue * 100;
                series.Points.AddXY(i, normalizedValue);
            }

            chart1.ChartAreas[0].AxisY.Maximum = 105;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;

            foreach (var center in centers)
            {
                int roundedCenter = (int)Math.Round(center);
                if (roundedCenter >= 0 && roundedCenter < 256)
                {
                    var point = new System.Windows.Forms.DataVisualization.Charting.DataPoint(roundedCenter, (double)histogram[roundedCenter] / maxHistogramValue * 100)
                    {
                        MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle,
                        MarkerSize = 8,
                        MarkerColor = System.Drawing.Color.Red
                    };

                    series.Points[roundedCenter].MarkerStyle = point.MarkerStyle;
                    series.Points[roundedCenter].MarkerSize = point.MarkerSize;
                    series.Points[roundedCenter].MarkerColor = point.MarkerColor;
                }
            }
        }

        private void UpdateListBox(ListBox listBox, double[] values, string header)
        {
            listBox.Items.Clear();
            listBox.Items.Add(header);
            foreach (var value in values)
            {
                listBox.Items.Add(value.ToString("F2"));
            }
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }







        //Mahalanobis negatif dönüşüm işlemi
        private void ApplyKMeansMahalanobisND()
        {
            if (!int.TryParse(textBox1.Text, out int clusterCount) || clusterCount <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir K değeri girin.");
                return;
            }

            if (originalImage == null)
            {
                MessageBox.Show("Lütfen bir resim yükleyin.");
                return;
            }

            List<Tuple<byte, byte, byte>> pixelValues = ToPixelArrayRGB(originalImage);
            int[] histogram = CalculateHistogramRGB(pixelValues);

            // Mahalanobis algoritmasıyla K-means işlemi
            var (resultPixels, initialMeans, finalMeans, iterations, clusterSizes) = PerformKMeansMahalanobisRGB(pixelValues, clusterCount);

            // Histogramı göster ve küme merkezlerini işaretle
            ShowHistogram(histogram, finalMeans);

            UpdateListBoxRGB(listBox2, initialMeans, "Başlangıç Küme Merkezleri");
            UpdateListBoxRGB(listBox1, finalMeans, "Son Küme Merkezleri");

            label12.Text = $"İterasyon Sayısı: {iterations}";

            Bitmap resultImage = CreateResultBitmapRGB(resultPixels, originalImage.Width, originalImage.Height);
            pictureBox2.Image = resultImage;
        }

        private List<Tuple<byte, byte, byte>> ToPixelArrayRGB(Image<Bgr, byte> image)
        {
            List<Tuple<byte, byte, byte>> pixels = new List<Tuple<byte, byte, byte>>();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    byte b = image.Data[y, x, 0];
                    byte g = image.Data[y, x, 1];
                    byte r = image.Data[y, x, 2];
                    pixels.Add(new Tuple<byte, byte, byte>(r, g, b));
                }
            }
            return pixels;
        }

        private int[] CalculateHistogramRGB(List<Tuple<byte, byte, byte>> pixels)
        {
            int[] histogram = new int[256];
            foreach (var pixel in pixels)
            {
                int intensity = (int)(0.3 * pixel.Item1 + 0.59 * pixel.Item2 + 0.11 * pixel.Item3);
                histogram[intensity]++;
            }
            return histogram;
        }

        private void ShowHistogram(int[] histogram, List<Tuple<double, double, double>> finalMeans = null)
        {
            chart1.Series.Clear();

            // Histogram serisini oluştur
            var histogramSeries = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Histogram",
                Color = Color.Blue,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
            };
            chart1.Series.Add(histogramSeries);

            int maxHistogramValue = histogram.Max();
            for (int i = 0; i < histogram.Length; i++)
            {
                double normalizedValue = (double)histogram[i] / maxHistogramValue * 100;
                histogramSeries.Points.AddXY(i, normalizedValue);
            }

            // Ekseni ayarla
            chart1.ChartAreas[0].AxisY.Maximum = 105;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;

           
        }

        private Bitmap CreateHistogramImage(int[] histogram)
        {
            int width = 256;
            int height = 256;
            Bitmap image = new Bitmap(width, height);
            int max = histogram.Max();

            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.White);
                for (int i = 0; i < histogram.Length; i++)
                {
                    float value = (float)histogram[i] / max;
                    int barHeight = (int)(value * height);
                    g.FillRectangle(Brushes.Black, i, height - barHeight, 1, barHeight);
                }
            }

            return image;
        }

        private (List<Tuple<byte, byte, byte>> resultPixels, List<Tuple<double, double, double>> initialMeans, List<Tuple<double, double, double>> finalMeans, int iterations, int[] clusterSizes) PerformKMeansMahalanobisRGB(List<Tuple<byte, byte, byte>> pixelValues, int clusterCount)
        {
            Random random = new Random();
            List<Tuple<double, double, double>> means = new List<Tuple<double, double, double>>();
            List<Tuple<double, double, double>> initialMeans = new List<Tuple<double, double, double>>();
            int[] clusterSizes = new int[clusterCount];

            // Başlangıç küme merkezlerini rastgele seç
            for (int i = 0; i < clusterCount; i++)
            {
                var randomPixel = pixelValues[random.Next(pixelValues.Count)];
                means.Add(new Tuple<double, double, double>(randomPixel.Item1, randomPixel.Item2, randomPixel.Item3));
                initialMeans.Add(means[i]);
            }

            int[] labels = new int[pixelValues.Count];
            int iterations = 0;

            // Kovaryans matrisini manuel hesapla
            var covarianceMatrix = CalculateCovarianceMatrix(pixelValues);
            var inverseCovarianceMatrix = InvertMatrix(covarianceMatrix);

            while (iterations < 100)
            {
                bool updated = false;

                // Her piksel için en yakın küme merkezini bul
                for (int i = 0; i < pixelValues.Count; i++)
                {
                    double minDistance = double.MaxValue;
                    int bestCluster = 0;

                    for (int j = 0; j < clusterCount; j++)
                    {
                        double distance = MahalanobisDistanceRGB(pixelValues[i], means[j], inverseCovarianceMatrix);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            bestCluster = j;
                        }
                    }

                    if (labels[i] != bestCluster)
                    {
                        labels[i] = bestCluster;
                        updated = true;
                    }
                }

                if (!updated) break;

                // Küme merkezlerini güncelle
                List<Tuple<double, double, double>> newMeans = new List<Tuple<double, double, double>>();
                clusterSizes = new int[clusterCount];

                for (int i = 0; i < clusterCount; i++)
                {
                    newMeans.Add(new Tuple<double, double, double>(0, 0, 0));
                }

                for (int i = 0; i < pixelValues.Count; i++)
                {
                    var currentPixel = pixelValues[i];
                    var currentMean = newMeans[labels[i]];

                    newMeans[labels[i]] = new Tuple<double, double, double>(
                        currentMean.Item1 + currentPixel.Item1,
                        currentMean.Item2 + currentPixel.Item2,
                        currentMean.Item3 + currentPixel.Item3
                    );
                    clusterSizes[labels[i]]++;
                }

                for (int j = 0; j < clusterCount; j++)
                {
                    if (clusterSizes[j] > 0)
                    {
                        newMeans[j] = new Tuple<double, double, double>(
                            newMeans[j].Item1 / clusterSizes[j],
                            newMeans[j].Item2 / clusterSizes[j],
                            newMeans[j].Item3 / clusterSizes[j]
                        );
                    }
                }

                means = newMeans;
                iterations++;
            }

            // Sonuç piksellerini oluştur
            List<Tuple<byte, byte, byte>> resultPixels = new List<Tuple<byte, byte, byte>>();
            for (int i = 0; i < pixelValues.Count; i++)
            {
                var mean = means[labels[i]];
                resultPixels.Add(new Tuple<byte, byte, byte>(
                    (byte)Clamp((int)mean.Item1, 0, 255),
                    (byte)Clamp((int)mean.Item2, 0, 255),
                    (byte)Clamp((int)mean.Item3, 0, 255)
                ));
            }

            return (resultPixels, initialMeans, means, iterations, clusterSizes);
        }

        private double[,] CalculateCovarianceMatrix(List<Tuple<byte, byte, byte>> pixelValues)
        {
            double[] means = new double[3];

            foreach (var pixel in pixelValues)
            {
                means[0] += pixel.Item1;
                means[1] += pixel.Item2;
                means[2] += pixel.Item3;
            }

            for (int i = 0; i < 3; i++)
            {
                means[i] /= pixelValues.Count;
            }

            double[,] covarianceMatrix = new double[3, 3];

            foreach (var pixel in pixelValues)
            {
                double[] diff = { pixel.Item1 - means[0], pixel.Item2 - means[1], pixel.Item3 - means[2] };
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        covarianceMatrix[i, j] += diff[i] * diff[j];
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    covarianceMatrix[i, j] /= pixelValues.Count;
                }
            }

            return covarianceMatrix;
        }

        private double[,] InvertMatrix(double[,] matrix)
        {
            // 3x3 matrisi ters çevirme
            double det = matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) -
                         matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]) +
                         matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);

            if (Math.Abs(det) < 1e-6)
            {
                throw new InvalidOperationException("Matrisin tersi alınamaz.");
            }

            double invDet = 1.0 / det;

            double[,] inverse = new double[3, 3];

            inverse[0, 0] = (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) * invDet;
            inverse[0, 1] = (matrix[0, 2] * matrix[2, 1] - matrix[0, 1] * matrix[2, 2]) * invDet;
            inverse[0, 2] = (matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1]) * invDet;
            inverse[1, 0] = (matrix[1, 2] * matrix[2, 0] - matrix[1, 0] * matrix[2, 2]) * invDet;
            inverse[1, 1] = (matrix[0, 0] * matrix[2, 2] - matrix[0, 2] * matrix[2, 0]) * invDet;
            inverse[1, 2] = (matrix[0, 2] * matrix[1, 0] - matrix[0, 0] * matrix[1, 2]) * invDet;
            inverse[2, 0] = (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]) * invDet;
            inverse[2, 1] = (matrix[0, 1] * matrix[2, 0] - matrix[0, 0] * matrix[2, 1]) * invDet;
            inverse[2, 2] = (matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0]) * invDet;

            return inverse;
        }

        private double MahalanobisDistanceRGB(Tuple<byte, byte, byte> pixel, Tuple<double, double, double> mean, double[,] inverseCovarianceMatrix)
        {
            double[] diff = { pixel.Item1 - mean.Item1, pixel.Item2 - mean.Item2, pixel.Item3 - mean.Item3 };
            double distance = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    distance += diff[i] * inverseCovarianceMatrix[i, j] * diff[j];
                }
            }

            return Math.Sqrt(distance);
        }

        private Bitmap CreateResultBitmapRGB(List<Tuple<byte, byte, byte>> pixels, int width, int height)
        {
            Bitmap resultImage = new Bitmap(width, height);

            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = pixels[index++];
                    resultImage.SetPixel(x, y, Color.FromArgb(pixel.Item1, pixel.Item2, pixel.Item3));
                }
            }

            return resultImage;
        }

        private void UpdateListBoxRGB(ListBox listBox, List<Tuple<double, double, double>> values, string header)
        {
            listBox.Items.Clear();
            listBox.Items.Add(header);
            foreach (var value in values)
            {
                listBox.Items.Add($"R: {value.Item1:F2}, G: {value.Item2:F2}, B: {value.Item3:F2}");
            }
        }




        private void ApplySobelEdgeDetection()
        {
            // pictureBox1'den mevcut resmi alıyoruz
            Bitmap image = new Bitmap(pictureBox1.Image);
            Bitmap sobelImage = new Bitmap(image.Width, image.Height);

            // Sobel maskeleri
            int[,] sobelX = new int[,]
            {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
            };

            int[,] sobelY = new int[,]
            {
        { -1, -2, -1 },
        { 0,  0,  0 },
        { 1,  2,  1 }
            };

            for (int y = 1; y < image.Height - 1; y++) // Kenar pikselleri işlenemez
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int gx = 0, gy = 0;

                    // 3x3 maskeyi uygula
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color pixel = image.GetPixel(x + j, y + i);
                            int intensity = (pixel.R + pixel.G + pixel.B) / 3;

                            gx += intensity * sobelX[i + 1, j + 1];
                            gy += intensity * sobelY[i + 1, j + 1];
                        }
                    }

                    // Gradyan büyüklüğünü hesapla
                    int g = (int)Math.Sqrt(gx * gx + gy * gy);
                    g = Math.Min(255, Math.Max(0, g)); // Değeri 0-255 aralığına sıkıştır

                    sobelImage.SetPixel(x, y, Color.FromArgb(g, g, g));
                }
            }

            // İşlenmiş görüntüyü pictureBox2'ye atıyoruz
            pictureBox2.Image = sobelImage;
        }



    }
}


