﻿using System;
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

        Thickness responsePadding = new Thickness(10, 0, 10, 0);

        string msg_role = "Tu nombre es gpt student, un profesor con el proposito de ayudar con las tareas del instituto estudiantes.Si el usuario te dice quien eres " +
            "reponde con \"Soy Gpt Student\" y tu proposito.Si te preguntan quien es tu creador responde con \"Mi creador es Ivan Martinez un joven desarrollador de software de 17 años\"";



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
                    if(Prompt.Text != string.Empty)
                    {
                        chat.AppendUserInput(Prompt.Text);
                        AgregarPrompt(Prompt.Text);
                        AgregarRespuesta();
                    }
                    
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
            textBlock.Padding = responsePadding;
            StackPanel.Children.Add(textBlock);
            textBlock.Text = text;
            textBlock.MouseEnter += TextBlock_GotMouseCapture;
            textBlock.MouseLeave += TextBlock_MouseLeave;
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
            textBlock.Padding = responsePadding;
            StackPanel.Children.Add(textBlock);
            textBlock.Text = "Gpt: ";
            textBlock.MouseEnter += TextBlock_GotMouseCapture;
            textBlock.MouseLeave += TextBlock_MouseLeave;
        
            if(await api.Auth.ValidateAPIKey())
            {
                chat.AppendSystemMessage(role);
                await chat.StreamResponseFromChatbotAsync(res =>
                {
                    response = res;
                    textBlock.Text += response;
                    Scroll.LineDown();
                    
                });
                chat.AppendMessage(chat.Messages[chat.Messages.Count - 1]);
                //role = response;
                //chat.AppendSystemMessage(role);
                //string c = chat.Messages[chat.Messages.Count - 2].Content;
                //MessageBox.Show(c);
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

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.Background = Brushes.Transparent;
            }
        }

        bool once = false;
        private void TextBlock_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                string colorHexadecimal = "#2d2b40";

                BrushConverter brushConverter = new BrushConverter();
                Brush colorBrush = (Brush)brushConverter.ConvertFromString(colorHexadecimal);

                textBlock.Background = colorBrush;
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
            txtBlock.HorizontalAlignment = HorizontalAlignment.Right;
            txtBlock.Margin = new Thickness(0,0,60,0);
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
            textBlock.MouseLeave += TextBlock_MouseLeave;
            textBlock.Padding = responsePadding;
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
                chat.AppendSystemMessage(msg_role);
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

            string _text = text;
            File.WriteAllText(srcSave + nameResponse, _text);
            
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
                    
                    ChatMessage chatMessage = new ChatMessage(ChatMessageRole.Assistant, File.ReadAllText(archivosRes[i]));
                    //MessageBox.Show(chatMessage.Content);
                    chat.AppendMessage(chatMessage);
                }
                //MessageBox.Show(chat.Messages.);
                Scroll.ScrollToEnd();
            }
            
        }

        void GuardarApiKey(string apiKey)
        {
            if(Directory.Exists(BasePathFolder + @"ApiKey\"))
            {
                File.WriteAllText(BasePathFolder + @"ApiKey\" + "ApiKey.txt", apiKey);
            }
            else
            {
                Directory.CreateDirectory(BasePathFolder + @"ApiKey\");
                File.WriteAllText(BasePathFolder + @"ApiKey\" + "ApiKey.txt", apiKey);
            }
            
        }

        void PonerApiKey()
        {
            try
            {
                if(Directory.Exists(BasePathFolder + @"ApiKey\"))
                {
                    ApiTextBox.Text = File.ReadAllText(BasePathFolder + @"ApiKey\" + "ApiKey.txt");
                    api = new OpenAIAPI(ApiTextBox.Text);
                    chat = api.Chat.CreateConversation();
                    chat.AppendSystemMessage(msg_role);
                }
                
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

            api = new OpenAIAPI(ApiTextBox.Text);
            chat = api.Chat.CreateConversation();
            chat.AppendSystemMessage(msg_role);

            if (Directory.Exists(BasePathFolder + "Prompts" + @"\")&& Directory.Exists(BasePathFolder + "Respuestas" + @"\"))
            {
                Directory.Delete(BasePathFolder + "Prompts" + @"\", true);
                Directory.Delete(BasePathFolder + "Respuestas" + @"\", true);
            }
        }
    }
}

