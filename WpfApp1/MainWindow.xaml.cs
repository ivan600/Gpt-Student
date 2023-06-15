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


namespace WpfApp1
{
/// <summary>
    /// Lógica de interacción para MainWindow.xaml
/// </summary>
    public partial class MainWindow : Window
    {
        OpenAIAPI api;
    
    
        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            Grid1.PreviewKeyUp += TxtMensaje_KeyDown;
            Loading();
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
        string role;
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
        
            if(await api.Auth.ValidateAPIKey())
            {
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
        
        }
    
        private void AgregarPrompt(string _prompt)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Margin = margin_TxtBlk;
            textBlock.FontSize = fontSize;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Text = "Student: "+_prompt;
            textBlock.Foreground = Brushes.White;
            StackPanel.Children.Add(textBlock);
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
            }
        }
    
        public async Task Loading()
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.CenterX = Image_Loading.Width/2;
            rotateTransform.CenterY = Image_Loading.Height / 2;
            Image_Loading.RenderTransform = rotateTransform;
    
            double duracionSegundos = 2.0;
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
    }
}
    
