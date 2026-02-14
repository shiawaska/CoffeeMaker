using System.Diagnostics;
using StartupScriptApp.Enums;
using StartupScriptApp.Enums.SnapPosition;
using StartupScriptApp.models.ArgumentDefinitions;
using StartupScriptApp.Models.Constants;

namespace StartupScriptApp.models.ApplicationDefinition;

public sealed class ApplicationDefinition
{
    public string Name { get; }
    public Categories Category { get; }
    public string ExecutablePath { get; }
    public string? WorkingDirectory { get; }
    public string? Arguments { get; }
    public string ProcessName { get; }
    public int Order { get; }
    public bool IsActive { get; }
    public bool SkipRunningCheck { get; }
    public int MonitorIndex { get; }
    public SnapPosition Position { get; }
    public string? Verb { get; }
    public bool UseShellExecute { get; }
    public bool CreateNoWindow { get; }
    public IReadOnlyList<string> WindowTitles { get; }
    public IReadOnlyList<string> SplashTitles { get; }
    public ProcessWindowStyle WindowStyle { get; }

    private ApplicationDefinition(Builder builder)
    {
        Name = builder._name;
        Category = builder._category;
        ExecutablePath = builder._executablePath;
        WorkingDirectory = builder._workingDirectory;
        Arguments = builder._arguments;
        ProcessName = builder._processName;
        Order = builder._order;
        IsActive = builder._isActive;
        SkipRunningCheck = builder._skipRunningCheck;
        MonitorIndex = builder._monitorIndex;
        Position = builder._position;
        Verb = builder._verb;
        UseShellExecute = builder._useShellExecute;
        CreateNoWindow = builder._createNoWindow;
        WindowStyle = builder._windowStyle;

        WindowTitles = builder._windowTitles.AsReadOnly();
        SplashTitles = builder._splashTitles.AsReadOnly();
    }

    public ProcessStartInfo ToProcessStartInfo()
    {
        var psi = new ProcessStartInfo
        {
            FileName = ExecutablePath,
            Arguments = Arguments ?? string.Empty,
            WorkingDirectory = WorkingDirectory ?? string.Empty,
            UseShellExecute = UseShellExecute,
            CreateNoWindow = CreateNoWindow,
            WindowStyle = WindowStyle,
        };

        if (!string.IsNullOrWhiteSpace(Verb))
            psi.Verb = Verb;

        return psi;
    }

    public static Builder New() => new();

    public sealed class Builder
    {
        internal string _name = string.Empty;
        internal Categories _category;
        internal string _executablePath = string.Empty;
        internal string? _workingDirectory;
        internal string? _arguments;
        internal string _processName = string.Empty;
        internal int _order;
        internal bool _isActive = true;
        internal bool _skipRunningCheck;
        internal int _monitorIndex;
        internal SnapPosition _position;
        internal string? _verb;
        internal bool _useShellExecute = true;
        internal bool _createNoWindow;
        internal readonly List<string> _windowTitles = new();
        internal readonly List<string> _splashTitles = new();
        internal ProcessWindowStyle _windowStyle = ProcessWindowStyle.Normal;

        private Builder Set(Action<Builder> apply)
        {
            if (apply == null)
                throw new ArgumentNullException(nameof(apply));
            if (this == null)
                throw new InvalidOperationException("Cannot set properties on a null builder.");
            apply(this);
            return this;
        }

        public Builder WithName(string name) =>
            Set(n => n._name = name ?? throw new ArgumentNullException(nameof(name)));

        public Builder WithCategory(Categories category) =>
            Set(n => n._category = category);

        public Builder WithExecutablePath(string path) =>
            Set(ep => ep._executablePath = path ?? throw new ArgumentNullException(nameof(path)));

        public Builder WithWorkingDirectory(string? workingDirectory) =>
            Set(wd => wd._workingDirectory = workingDirectory);

        public Builder WithArguments(string? arguments) => Set(a => a._arguments = arguments);

        public Builder WithProcessName(string processName) =>
            Set(pn => pn._processName = processName);

        public Builder WithOrder(int order) => Set(o => o._order = order);

        public Builder IsActive(bool isActive = true) => Set(a => a._isActive = isActive);

        public Builder SkipRunningCheck(bool skip = true) => Set(sr => sr._skipRunningCheck = skip);

        public Builder WithMonitorIndex(int monitorIndex) =>
            Set(mi => mi._monitorIndex = monitorIndex);

        public Builder WithPosition(SnapPosition position) => Set(p => p._position = position);

        public Builder WithVerb(string? verb) => Set(v => v._verb = verb);

        public Builder UseShellExecute(bool useShellExecute = true) =>
            Set(se => se._useShellExecute = useShellExecute);

        public Builder CreateNoWindow(bool createNoWindow = true) =>
            Set(cn => cn._createNoWindow = createNoWindow);

        public Builder WithWindowStyle(ProcessWindowStyle style) =>
            Set(ws => ws._windowStyle = style);

        public Builder AddWindowTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
                _windowTitles.Add(title);
            return this;
        }

        /// <summary>
        /// Adds an argument to the arguments string.
        /// </summary>
        /// <param name="arg"> The argument definition to add to the arguments string. </param>
        /// <param name="includeNameInWindowTitles"> Whether to include the argument name in the window titles. Used for IDE and text editors </param>
        /// <returns> The builder instance for method chaining. </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Builder AddArgument(ArgumentDefinition arg, bool includeNameInWindowTitles = false)
        {
            if (arg == null)
                throw new ArgumentNullException(nameof(arg));

            if (includeNameInWindowTitles)
                AddWindowTitle(arg.Name);

            AddArgumentString(arg.Value);
            return this;
        }

        private void AddArgumentString(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
                throw new ArgumentNullException(nameof(arg));

            _arguments = _arguments == null ? arg : $"{_arguments} {arg}";
        }

        public Builder AddSplashTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
                _splashTitles.Add(title);
            return this;
        }

        public ApplicationDefinition Build()
        {
            // Validate required fields here (fail fast)
            if (string.IsNullOrWhiteSpace(_name))
                throw new InvalidOperationException("Name is required.");
            if (string.IsNullOrWhiteSpace(_executablePath))
                throw new InvalidOperationException("ExecutablePath is required.");

            return new ApplicationDefinition(this);
        }
    }
}
