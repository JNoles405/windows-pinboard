// Disambiguate types that exist in both System.Windows (WPF) and System.Windows.Forms.
global using Application = System.Windows.Application;
global using MessageBox = System.Windows.MessageBox;
global using TextBox = System.Windows.Controls.TextBox;
global using KeyEventArgs = System.Windows.Input.KeyEventArgs;
