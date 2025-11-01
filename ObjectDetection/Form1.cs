using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using System.Drawing;

namespace ObjectDetection
{
    public partial class Form1 : Form
    {
        VideoCapture? capture;
        Net? yoloNet;
        string[] labels;
        bool isCameraRunning = false;

        private List<Rectangle> lastBoxes = new List<Rectangle>();
        private List<int> lastClassIds = new List<int>();

        private int frameCounter = 0;
        private int frameSkip = 5;

        public Form1()
        {
            InitializeComponent();

            // Yolo dosya yolları
            string yoloPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yolo");
            string cfgPath = System.IO.Path.Combine(yoloPath, "yolov4.cfg");
            string weightsPath = System.IO.Path.Combine(yoloPath, "yolov4.weights");
            string namesPath = System.IO.Path.Combine(yoloPath, "coco.names");

            // Labels
            labels = System.IO.File.ReadAllLines(namesPath);

            // YOLO ağı yükle
            yoloNet = DnnInvoke.ReadNetFromDarknet(cfgPath, weightsPath);
            yoloNet.SetPreferableBackend(Emgu.CV.Dnn.Backend.OpenCV);
            yoloNet.SetPreferableTarget(Emgu.CV.Dnn.Target.Cpu);

            // Form başlangıç ayarları
            this.StartPosition = FormStartPosition.CenterScreen;

            // Buton stil ayarları
            btnStart.MouseEnter += (s, e) => { btnStart.BackColor = Color.FromArgb(0, 150, 255); };
            btnStart.MouseLeave += (s, e) => { btnStart.BackColor = Color.FromArgb(0, 122, 204); };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isCameraRunning)
            {
                capture = new VideoCapture(0);
                Application.Idle += ProcessFrame;
                isCameraRunning = true;
                btnStart.Text = "Stop Camera";
            }
            else
            {
                Application.Idle -= ProcessFrame;
                if (capture != null)
                    capture.Dispose();
                isCameraRunning = false;
                pictureBox1.Image = null;
                btnStart.Text = "Start Camera";
            }
        }

        private void ProcessFrame(object? sender, EventArgs e)
        {
            frameCounter++;
            Mat frame = capture?.QueryFrame();
            if (frame == null) return;

            // Eğer frame atlanıyorsa YOLO çalıştırma ama son kutuları çiz
            if (frameCounter % frameSkip != 0)
            {
                // Son kutuları çiz
                for (int i = 0; i < lastBoxes.Count; i++)
                {
                    CvInvoke.Rectangle(frame, lastBoxes[i], new MCvScalar(0, 255, 0), 2); // Kutuyu çizer
                    CvInvoke.PutText(frame, labels[lastClassIds[i]], new Point(lastBoxes[i].X, lastBoxes[i].Y - 5),
                                     FontFace.HersheySimplex, 0.7, new MCvScalar(0, 255, 0), 2); // Etiketi yazar
                }

                pictureBox1.Image = frame.ToBitmap(); // Kamera görüntüsünü ve son kutuları gösterir
                return;
            }

            if (capture != null && capture.Ptr != IntPtr.Zero && yoloNet != null)
            {
                // YOLO için blob oluştur
                // Yolo ağı her zaman 320x320 boyutunda giriş bekler
                Mat blob = DnnInvoke.BlobFromImage(frame, 1 / 255.0, new Size(320, 320), new MCvScalar(), true, false);
                yoloNet.SetInput(blob);

                // Çıkış katmanları
                string[] outNames = yoloNet.UnconnectedOutLayersNames; // Çıkış katmanlarının isimlerini alır
                using (var output = new Emgu.CV.Util.VectorOfMat()) // Çıkışları tutmak için vektör
                {
                    yoloNet.Forward(output, outNames); // İleri besleme yapar ve çıkışları alır

                    float confThreshold = 0.5f; // Güven eşiği
                    float nmsThreshold = 0.4f; // Non-Maximum Suppression eşiği

                    // NMS için listeler oluştur
                    List<Rectangle> boxes = new List<Rectangle>(); // Tüm kutuları tutar
                    List<float> confidences = new List<float>(); // Tüm confidence değerlerini tutar
                    List<int> classIds = new List<int>(); // Tüm sınıf ID’lerini tutar

                    // Her çıkış katmanı için döngü
                    for (int i = 0; i < output.Size; i++)
                    {
                        float[,] data = (float[,])output[i].GetData();
                        for (int j = 0; j < data.GetLength(0); j++)
                        {
                            float confidence = data[j, 4];
                            if (confidence > confThreshold)
                            {
                                int bestClassId = -1; // En iyi sınıf kimliği
                                float bestScore = 0; // En iyi güven skoru
                                for (int k = 5; k < data.GetLength(1); k++)
                                {
                                    if (data[j, k] > bestScore)
                                    {
                                        bestScore = data[j, k];
                                        bestClassId = k - 5;
                                    }
                                }

                                // Sadece güven skoru belirli bir eşiğin üzerindeyse kutu ekler
                                if (bestScore > confThreshold)
                                {
                                    int centerX = (int)(data[j, 0] * frame.Cols);
                                    int centerY = (int)(data[j, 1] * frame.Rows);
                                    int width = (int)(data[j, 2] * frame.Cols);
                                    int height = (int)(data[j, 3] * frame.Rows);
                                    int x = centerX - width / 2;
                                    int y = centerY - height / 2;

                                    Rectangle rect = new Rectangle(x, y, width, height);
                                    boxes.Add(rect); // NMS için kutuyu listeye ekler
                                    confidences.Add(bestScore); // NMS için confidence ekler
                                    classIds.Add(bestClassId); // NMS için sınıf ID ekler
                                }
                            }
                        }
                    }

                    // Non-Maximum Suppression uygular ve tekrar çizer
                    int[] indices = DnnInvoke.NMSBoxes(boxes.ToArray(), confidences.ToArray(), confThreshold, nmsThreshold); // Aynı nesneye ait fazla kutuları filtreler

                    // Son kutuları sakla ki sonraki frame atlamalarda kaybolmasın
                    lastBoxes.Clear();
                    lastClassIds.Clear();
                    foreach (int idx in indices)
                    {
                        Rectangle box = boxes[idx]; // NMS sonrası kutuyu alır
                        CvInvoke.Rectangle(frame, box, new MCvScalar(0, 255, 0), 2); // Kutuyu çizer
                        CvInvoke.PutText(frame, labels[classIds[idx]], new Point(box.X, box.Y - 5),
                                         FontFace.HersheySimplex, 0.7, new MCvScalar(0, 255, 0), 2); // Etiketi yazar

                        lastBoxes.Add(box); // Son kutuları sakla
                        lastClassIds.Add(classIds[idx]); // Son sınıf ID’lerini sakla
                    }
                }

                // Son görüntüyü PictureBox'a gönderir
                pictureBox1.Image = frame.ToBitmap();
            }
        }
    }
}
