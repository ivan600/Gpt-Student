﻿using System;
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
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Moderation;

namespace WpfApp1
{
/// <summary>
    /// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
        OpenAIAPI api = new OpenAIAPI("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");


        public MainWindow()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            Grid1.PreviewKeyUp += TxtMensaje_KeyDown;
            chat = api.Chat.CreateConversation();

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
            
            
            chat.AppendUserInput(Prompt.Text);
            AgregarPrompt(Prompt.Text);
            AgregarRespuesta();

            Prompt.Text = string.Empty;
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
            await chat.StreamResponseFromChatbotAsync(res =>
            {
                response = res;
                textBlock.Text += response;
                Scroll.LineDown();
            });
            role = response;
            chat.AppendSystemMessage(role);
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
    }
}