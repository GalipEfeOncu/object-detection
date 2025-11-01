# 🎯 Object Detection with YOLOv4 (Emgu CV + C#)

Bu proje, **Emgu CV** ve **YOLOv4** kullanılarak geliştirilmiş bir **gerçek zamanlı nesne tanıma uygulamasıdır**.  
Uygulama, bilgisayarın kamerasını kullanarak çevredeki nesneleri tespit eder ve sınıflandırır.  
Performansı artırmak için her birkaç karede bir YOLO çalıştırılır ve aradaki framelerde son sonuçlar korunur.

---

## 🚀 Özellikler

- Gerçek zamanlı kamera üzerinden nesne tespiti  
- **YOLOv4** modelini kullanır (COCO veri seti – 80 sınıf)  
- CPU üzerinde çalışır (GPU eklenebilir)  
- **Frame skipping** özelliği ile daha akıcı performans  
- Son tespit edilen kutuların korunması sayesinde kare atlamalarda bile düzgün görüntü  
- Dinamik buton renkleri ile modern görünüm  

---

## 🧩 Kullanılan Teknolojiler

- **.NET Framework / .NET 6+**
- **Emgu CV (OpenCV Wrapper for .NET)**
- **YOLOv4 (Darknet Model)**
- **Windows Forms**

---

## 📦 Gereksinimler

- Visual Studio (2022 önerilir)  
- Emgu.CV NuGet paketleri:
  ```bash
  Install-Package Emgu.CV
  Install-Package Emgu.CV.runtime.windows
  Install-Package Emgu.CV.ui
  Install-Package Emgu.CV.Bitmap
  yolov4.cfg, yolov4.weights ve coco.names dosyaları
  ```

---

## ⚙️ Kurulum ve Çalıştırma

1. Bu projeyi **GitHub’dan klonla veya ZIP olarak indir**:
   ```bash
   git clone https://github.com/kullaniciadi/ObjectDetection.git

2. Proje dizininde yolo klasörünün zaten bulunduğundan emin ol.

3. Visual Studio’da projeyi aç.

4. NuGet üzerinden gerekli **Emgu.CV** paketlerini yükle:

   ```bash
   Install-Package Emgu.CV
   Install-Package Emgu.CV.runtime.windows
   Install-Package Emgu.CV.ui
   Install-Package Emgu.CV.Bitmap
   ```
5. Projeyi derle (Ctrl + Shift + B) ve çalıştır (F5).

6. Uygulama açıldığında Start Camera butonuna basarak kamerayı başlat. Nesneler otomatik olarak tespit edilir ve isimleriyle birlikte gösterilir.

---

## 🎥 Kullanım

1. Start Camera butonuna tıklayarak kamerayı başlat.

2. Uygulama, nesneleri otomatik olarak algılar ve yeşil kutularla çerçeveler.

3. Algılanan nesnenin adı kutunun üst kısmında gösterilir.

4. Stop Camera butonuna basarak kamerayı durdur.

---

## ⚡ Performans Notu

- `frameSkip` değişkeni sayesinde YOLO her 5 karede bir çalışır.  
  Bu, CPU kullanımını azaltır ve performansı artırır.  
  Daha hassas tespit istersen `frameSkip = 1;` yapabilirsin (ama işlemci biraz ısınır 😅).

- Tespit doğruluğunu değiştirmek için şu iki değeri düzenleyebilirsin:
  ```csharp
  float confThreshold = 0.5f; // Güven eşiği (confidence threshold)
  float nmsThreshold = 0.4f;  // Non-Maximum Suppression eşiği
  ```

- confThreshold ne kadar düşükse, o kadar fazla nesne algılanır (ama hatalar da artar).

- nmsThreshold ise aynı nesneye çizilen fazla kutuları filtreler.

---

## 📜 Lisans

- Bu proje MIT Lisansı ile lisanslanmıştır.
- Dilediğin gibi kullanabilir, değiştirebilir ve paylaşabilirsin.
