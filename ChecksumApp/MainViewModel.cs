using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommandLineTools.Options;
using CommandLineTools.Tools;

namespace ChecksumApp;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public MainViewModel(string fileName)
    {
        this.PropertyChanged += MainViewModel_PropertyChanged;
        this.FileName = fileName;
        this.CheckCommand = new Command(
            _ => Task.Run(() =>
            {
                var t = new ChecksumTool();
                t.SetFileService(new MauiFileService());
                var alg = this.FileName.Split('.').Last();
                var cs = t.CalculateChecksum(new ChecksumOptions()
                {
                    Algorithm = alg,
                    InFile = this.FileName.Substring(0, this.FileName.Length - alg.Length - 1),
                });
                var asString = BitConverter.ToString(cs).Replace("-", string.Empty).ToUpper();
                this.ChecksumIs = asString;
                this.Success = this.ChecksumShouldBe == this.ChecksumIs
                    ? "Checksum matches"
                    : "Checksums do not match";
            }),
            _ => !string.IsNullOrEmpty(this.ChecksumShouldBe));
    }

    private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(this.FileName):
                if (string.IsNullOrEmpty(this.FileName))
                {
                    this.ChecksumShouldBe = string.Empty;
                    this.ChecksumIs = string.Empty;
                    this.Success = string.Empty;
                }
                else
                {
                    Task.Run(() =>
                    {
                        var content = File.ReadAllText(this.FileName);
                        var checksum = content.Split(' ');
                        this.ChecksumShouldBe = checksum[0].ToUpper();
                        if (this.CheckCommand is Command c)
                        {
                            c.ChangeCanExecute();
                        }
                    });
                }
                break;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public string FileName { get; set; }

    public string ChecksumShouldBe { get; set; }

    public string ChecksumIs { get; set; }

    public ICommand CheckCommand { get; set; }

    public string Success { get; set; }
}