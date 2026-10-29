using System.Windows.Input;
using PlayListSample.Models;
using PlayListSample.Services;

namespace PlayListSample.ViewModels;

/// <summary>
/// ה-ViewModel הראשי של המסך.
/// אחראי על:
/// 1. שמירת מצב המסך (State) - הפריט הנוכחי המוצג, טקסט המיקום, שדות הטופס וחיווי טעינה.
/// 2. פקודות (Commands) - פעולות שהמשתמש מפעיל מהמסך (דפדוף, שמירה, שינוי סטטוס ואיפוס).
/// 3. תקשורת עם שכבת השירות (Service) - שליפת נתונים ועדכונם.
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly IVideoService _videoService;
    private readonly List<VideoItem> _videos = new();
    private int _currentIndex = 0;

    private VideoItem? _currentVideo;
    private string _currentPositionText = "טוען נתונים...";

    // שדות הטופס להזנת סרטון חדש (Two-Way Binding)
    private string _inputTitle = string.Empty;
    private string _inputChannelName = string.Empty;
    private string _inputUrl = string.Empty;
    private string _inputDurationText = string.Empty;
    private string _statusMessage = string.Empty;

    //פעולות
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ToggleWatchedCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand ShowVideoCommand { get; }

    private Task load;
    public MainViewModel(IVideoService videoService)
    {
        _videoService = videoService;
        Title = "ניהול פלייליסט - חזרה על MVVM";

        NextCommand = new Command(ExecuteNext, CanExecuteNext);
        PreviousCommand = new Command(ExecutePrevious, CanExecutePrevious);
        SaveCommand = new Command(async () => await ExecuteSaveAsync(), () => IsNotLoading);
        ToggleWatchedCommand = new Command(async () => await ExecuteToggleWatchedAsync(), () => CurrentVideo != null && IsNotLoading);
        ClearFormCommand = new Command(ExecuteClearForm, () => IsNotLoading);

        load = LoadVideosAsync();
    }

    public VideoItem? CurrentVideo
    {
        get => _currentVideo;
        private set
        {
            if (_currentVideo != value)
            {
                _currentVideo = value;
                OnPropertyChanged();
            }
        }
    }
    private async Task NavigateToDetails(VideoItem v)
    {
        Dictionary<string, object> params= new Dictionary() { "item"}
    }

    public string CurrentPositionText
    {
        get => _currentPositionText;
        private set
        {
            if (_currentPositionText != value)
            {
                _currentPositionText = value;
                OnPropertyChanged();
            }
        }
    }

    public string InputTitle
    {
        get => _inputTitle;
        set
        {
            if (_inputTitle != value)
            {
                _inputTitle = value;
                OnPropertyChanged();
            }
        }
    }

    public string InputChannelName
    {
        get => _inputChannelName;
        set
        {
            if (_inputChannelName != value)
            {
                _inputChannelName = value;
                OnPropertyChanged();
            }
        }
    }

    public string InputUrl
    {
        get => _inputUrl;
        set
        {
            if (_inputUrl != value)
            {
                _inputUrl = value;
                OnPropertyChanged();
            }
        }
    }

    public string InputDurationText
    {
        get => _inputDurationText;
        set
        {
            if (_inputDurationText != value)
            {
                _inputDurationText = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private async Task LoadVideosAsync()
    {
        IsLoading = true;
        RefreshCommands();

        var loaded = await _videoService.GetAllVideosAsync();
        _videos.Clear();
        _videos.AddRange(loaded);

        _currentIndex = 0;
        UpdateCurrentVideo();

        IsLoading = false;
        RefreshCommands();
    }

    private void ExecuteNext()
    {
        if (_currentIndex < _videos.Count - 1)
        {
            _currentIndex++;
            UpdateCurrentVideo();
        }
    }

    private bool CanExecuteNext()
    {
        return IsNotLoading && _videos.Count > 0 && _currentIndex < _videos.Count - 1;
    }

    private void ExecutePrevious()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateCurrentVideo();
        }
    }

    private bool CanExecutePrevious()
    {
        return IsNotLoading && _videos.Count > 0 && _currentIndex > 0;
    }

    private async Task ExecuteToggleWatchedAsync()
    {
        if (CurrentVideo == null) return;

        IsLoading = true;
        RefreshCommands();

        CurrentVideo.IsWatched = !CurrentVideo.IsWatched;
        await _videoService.UpdateVideoAsync(CurrentVideo);

        // רענון התצוגה של הפריט הנוכחי
        OnPropertyChanged(nameof(CurrentVideo));
        StatusMessage = CurrentVideo.IsWatched ? "הסרטון סומן כנצפה!" : "הסרטון סומן כלא נצפה.";

        IsLoading = false;
        RefreshCommands();
    }

    private async Task ExecuteSaveAsync()                       
        

    {
        if (string.IsNullOrWhiteSpace(InputTitle))
        {
            StatusMessage = "שגיאה: חובה להזין כותרת לסרטון!";
            return;
        }

        IsLoading = true;
        RefreshCommands();

        int duration = 0;
        int.TryParse(InputDurationText, out duration);

        var newVideo = new VideoItem
        {
            Title = InputTitle.Trim(),
            ChannelName = string.IsNullOrWhiteSpace(InputChannelName) ? "ערוץ כללי" : InputChannelName.Trim(),
            Url = InputUrl.Trim(),
            DurationMinutes = duration,
            IsWatched = false
        };

        await _videoService.AddVideoAsync(newVideo);
        _videos.Add(newVideo);

        // מעבר מיידי לסרטון החדש שנוסף
        _currentIndex = _videos.Count - 1;
        UpdateCurrentVideo();

        // איפוס שדות הטופס
        ExecuteClearForm();
        StatusMessage = $"הסרטון '{newVideo.Title}' נוסף בהצלחה לרשימה!";

        IsLoading = false;
        RefreshCommands();
    }

    private void ExecuteClearForm()
    {
        InputTitle = string.Empty;
        InputChannelName = string.Empty;
        InputUrl = string.Empty;
        InputDurationText = string.Empty;
    }

    private void UpdateCurrentVideo()
    {
        if (_videos.Count > 0 && _currentIndex >= 0 && _currentIndex < _videos.Count)
        {
            CurrentVideo = _videos[_currentIndex];
            CurrentPositionText = $"סרטון {_currentIndex + 1} מתוך {_videos.Count}";
        }
        else
        {
            CurrentVideo = null;
            CurrentPositionText = "אין סרטונים זמינים";
        }

        RefreshCommands();
    }

    private void RefreshCommands()
    {
       ((Command) NextCommand).ChangeCanExecute();
		((Command)PreviousCommand).ChangeCanExecute();
        ((Command)SaveCommand).ChangeCanExecute();
        ((Command)ToggleWatchedCommand).ChangeCanExecute();
        ((Command)ClearFormCommand).ChangeCanExecute();
    }
}
