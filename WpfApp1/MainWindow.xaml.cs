using System;
using System.Collections;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Moderation;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace WpfApp1
{
/// <summary>
    /// Lógica de interacción para MainWindow.xaml
/// </summary>
    public partial class MainWindow : Window
    {
        OpenAIAPI api;

        string promptsPathFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts" + @"\");
        string respuestasPathFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Respuestas" + @"\");
        string imagePathFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images" + @"\");
        string BasePathFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);



        public MainWindow()
        {
            InitializeComponent();
            Inicializar();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            Grid1.PreviewKeyUp += TxtMensaje_KeyDown;
            PonerApiKey();
            GenerarRespuestasAnteriores();
            Task task = Loading();
        }

        void Inicializar()
        {
            BitmapImage bitmapEnvImage = new BitmapImage(new Uri(imagePathFolder + "enviar - copia.png"));
            img_Enviar.Source = bitmapEnvImage;

            BitmapImage bitmapClosImage = new BitmapImage(new Uri(imagePathFolder + "crossregular_106296.png"));
            img_Close.Source = bitmapClosImage;

        }
    
        string response;
        private void TxtMensaje_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GenerarRespuesta();
            }
            else if (e.Key == Key.Escape) { Close(); }
        }
        
        Conversation chat;
        string role = string.Empty;
        private void GenerarRespuesta()
        {
        
            try
            {
                if(api != null)
                {
                    chat.AppendUserInput(Prompt.Text);
                    AgregarPrompt(Prompt.Text);
                    AgregarRespuesta();
        
                    Prompt.Text = string.Empty;
                }else
                {
                    MessageBox.Show($"Se ha producido un error: No has introducido la apikey", "Error");
                }
                
            }
            catch(Exception ex) { 
            
                MessageBox.Show($"Se ha producido un error: {ex.Message}","Error");
            }
        
        }
        
        Thickness margin_TxtBlk = new Thickness(40, 5, 40, 5);
        double fontSize = 15;

        void AgregarTextBlock(string text, string color)
        {

            BrushConverter brushConverter = new BrushConverter();
            Brush colorBrush = (Brush)brushConverter.ConvertFromString(color);
            TextBlock textBlock = new TextBlock();
            textBlock.FontSize = fontSize;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = margin_TxtBlk;
            textBlock.Foreground = colorBrush;
            StackPanel.Children.Add(textBlock);
            textBlock.Text = text;
            textBlock.MouseEnter += TextBlock_GotMouseCapture;
        }

        private async void AgregarRespuesta()
        {
            string respuesta = string.Empty;
            response = string.Empty;
            
            string colorHexadecimal = "#a25523";
            
            BrushConverter brushConverter = new BrushConverter();
            Brush colorBrush = (Brush)brushConverter.ConvertFromString(colorHexadecimal);
            TextBlock textBlock = new TextBlock();
            textBlock.FontSize = fontSize;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = margin_TxtBlk;
            textBlock.Foreground = colorBrush;
            StackPanel.Children.Add(textBlock);
            textBlock.Text = "Gpt: ";
            textBlock.MouseEnter += TextBlock_GotMouseCapture;
        
            if(await api.Auth.ValidateAPIKey())
            {
                chat.AppendSystemMessage(role);
                await chat.StreamResponseFromChatbotAsync(res =>
                {
                    response = res;
                    textBlock.Text += response;
                    Scroll.LineDown();
                });
                role = response;
                chat.AppendSystemMessage(role);
            }
            else
            {
                MessageBox.Show("La ApiKey no es valida", "Error Api");
            }

            if (!Directory.Exists(BasePathFolder + "Respuestas" + @"\"))
            {
                Directory.CreateDirectory(BasePathFolder + "Respuestas" + @"\");
                GuardarRespuesta(textBlock.Text, respuestasPathFolder, "Response", respuestasPathFolder);
            }
            else
            {
                GuardarRespuesta(textBlock.Text, respuestasPathFolder, "Response", respuestasPathFolder);
            }

        }


        bool once = false;
        private void TextBlock_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                Clipboard.SetText(textBlock.Text);
            }
            if (!once)
            {
                CreateTextBlockCopyTo();
                once = true;
            }
            
        }

        async void CreateTextBlockCopyTo()
        {
            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = "Se ha copiado la respuesta";
            string colorHexadecimal = "#c3c3c3";
            BrushConverter brushConverter = new BrushConverter();
            Brush colorBrush = (Brush)brushConverter.ConvertFromString(colorHexadecimal);
            txtBlock.Foreground = colorBrush;
            txtBlock.HorizontalAlignment = HorizontalAlignment.Center;
            txtBlock.Margin = new Thickness(300,0,0,0);
            txtBlock.VerticalAlignment = VerticalAlignment.Center;

            Grid_Pantalla1.Children.Add(txtBlock);
            Grid.SetColumn(txtBlock, 1);

            await DeleteObjectByTime(txtBlock, Grid_Pantalla1, 2000);
        }

        async Task DeleteObjectByTime(Object objToDelete, Object objFrom, int time)
        {
            await Task.Delay(time);
            if (objFrom is Grid grid)
            {
                if (objToDelete is TextBlock textBlock)
                {
                    grid.Children.Remove(textBlock);
                    once = false;
                }
            }
        }

        private void AgregarPrompt(string _prompt)
        {
            string colorHexadecimal = "#c3c3c3";
            BrushConverter brushConverter = new BrushConverter();
            Brush colorBrush = (Brush)brushConverter.ConvertFromString(colorHexadecimal);

            TextBlock textBlock = new TextBlock();
            textBlock.Margin = margin_TxtBlk;
            textBlock.FontSize = fontSize;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Text = "Student: "+_prompt;
            textBlock.Foreground = colorBrush;
            textBlock.MouseEnter += TextBlock_GotMouseCapture;
            StackPanel.Children.Add(textBlock);

            if(!Directory.Exists(BasePathFolder + "Prompts" + @"\"))
            {
                Directory.CreateDirectory(BasePathFolder + "Prompts" + @"\");
                GuardarRespuesta(textBlock.Text, promptsPathFolder, "Prompts", promptsPathFolder);
            }
            else
            {
                GuardarRespuesta(textBlock.Text, promptsPathFolder, "Prompts", promptsPathFolder);
            }
        }
    
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void ColumnDefinition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GenerarRespuesta();
        }
    
        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if(ApiTextBox.Text != null)
            {
                api = new OpenAIAPI(ApiTextBox.Text);
                chat = api.Chat.CreateConversation();
                GuardarApiKey(ApiTextBox.Text);
            }
        }
    
        public async Task Loading()
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.CenterX = Image_Loading.Width/2;
            rotateTransform.CenterY = Image_Loading.Height / 2;
            Image_Loading.RenderTransform = rotateTransform;
    
            double duracionSegundos = 1f;
            double anguloInicial = 0;
            double anguloFinal = 360;
    
            DoubleAnimation animacionRotacion = new DoubleAnimation();
            animacionRotacion.From = anguloInicial;
            animacionRotacion.To = anguloFinal;
            animacionRotacion.Duration = TimeSpan.FromSeconds(duracionSegundos);
            animacionRotacion.RepeatBehavior = RepeatBehavior.Forever;
    
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animacionRotacion);
    
            await Task.Delay(1500);
    
            MostrarElemento(Grid_Pantalla1, Visibility.Visible);
            MostrarElemento(Grid_PantallaLoading, Visibility.Hidden);
        }
    
        void MostrarElemento(Grid grid, Visibility visibility)
        {
            grid.Visibility = visibility;
        }

        void GuardarRespuesta(string text, string srcSave, string name, string folderToGenerateName)
        {
            string nameResponse = GenerarNombre(name, folderToGenerateName);

            string[] lines = { text };
            File.WriteAllLines(srcSave + nameResponse, lines);
            
        }

        
        string GenerarNombre(string nameRes, string src)
        {
            string[] archivos = Directory.GetFiles(src);
            int numArchivos = archivos.Length;
            string name = nameRes + numArchivos.ToString() + ".txt";
            return name;
        }

        void GenerarRespuestasAnteriores()
        {
            if(Directory.Exists(BasePathFolder + "Respuestas" + @"\") && Directory.Exists(BasePathFolder + "Prompts" + @"\"))
            {
                string[] archivosRes = Directory.GetFiles(respuestasPathFolder);
                string[] archivosPrompt = Directory.GetFiles(promptsPathFolder);

                for (int i = 0; i < archivosRes.Length; i++)
                {
                    AgregarTextBlock(File.ReadAllText(archivosPrompt[i]), "#c3c3c3");
                    AgregarTextBlock(File.ReadAllText(archivosRes[i]), "#a25523");
                    if (i == archivosRes.Length - 1)
                    {
                        role = File.ReadAllText(archivosRes[i]);
                    }
                }
                Scroll.ScrollToEnd();
            }
            
        }

        void GuardarApiKey(string apiKey)
        {
            File.WriteAllText(@"C:\Users\ivan\Documents\CursosUdemy\C#\WpfApp1\WpfApp1\ApiKey\ApiKey.txt", apiKey);
        }

        void PonerApiKey()
        {
            try
            {
                ApiTextBox.Text = File.ReadAllText(@"C:\Users\ivan\Documents\CursosUdemy\C#\WpfApp1\WpfApp1\ApiKey\ApiKey.txt");
                api = new OpenAIAPI(ApiTextBox.Text);
                chat = api.Chat.CreateConversation();
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }

        void ClearAllResponses()
        {
            StackPanel.Children.Clear();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ClearAllResponses();
            if (Directory.Exists(BasePathFolder + "Prompts" + @"\")&& Directory.Exists(BasePathFolder + "Respuestas" + @"\"))
            {
                Directory.Delete(BasePathFolder + "Prompts" + @"\", true);
                Directory.Delete(BasePathFolder + "Respuestas" + @"\", true);
            }
        }
    }
}

