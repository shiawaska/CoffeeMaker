using System.Diagnostics;
using StartupScriptApp.Enums;
using StartupScriptApp.Models.Configurations;

namespace StartupScriptApp.Models.ApplicationDefinition;

public sealed class ApplicationDefinition
{
    public string? Name { get; }
    public Categories? Category { get; }
    public string ExecutablePath { get; }
    public string? WorkingDirectory { get; }
    public string? Arguments { get; }
    public string? ProcessName { get; }
    public int Order { get; } = 0;
    public bool IsActive { get; } = true;
    public bool SkipRunningCheck { get; } = false;
    public int MonitorIndex { get; } = 0;
    public SnapPosition Position { get; } = SnapPosition.FullScreen;
    public string? Verb { get; }
    public bool UseShellExecute { get; } = false;
    public bool CreateNoWindow { get; } = false;
    public IReadOnlyList<string> SplashTitles { get; } = Array.Empty<string>();
    public ProcessWindowStyle WindowStyle { get; } = ProcessWindowStyle.Normal;

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
        internal Categories? _category;
        internal string _executablePath = string.Empty;
        internal string? _workingDirectory;
        internal string? _arguments;
        internal string _processName = string.Empty;
        internal int _order;
        internal bool _isActive = true;
        internal bool _skipRunningCheck;
        internal int _monitorIndex;
        internal SnapPosition _position = SnapPosition.FullScreen;
        internal string? _verb;
        internal bool _useShellExecute = true;
        internal bool _createNoWindow;
        internal List<string> _splashTitles = new();

        private Builder Set(Action<Builder> apply)
        {
            if (apply == null)
                throw new ArgumentNullException(nameof(apply));
            if (this == null)
                throw new InvalidOperationException("Cannot set properties on a null builder.");
            apply(this);
            return this;
        }

        public Builder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return this;
            return Set(n => n._name = name);
        }

        public Builder WithCategory(Categories category) => Set(n => n._category = category);

        public Builder WithCategory(string category)
        {
            if (EnumTryParse<Categories>(category, true, out var parsedCategory))
                return WithCategory(parsedCategory);
            return this;
        }

        public Builder WithExecutablePath(string path)
        {
            if (File.Exists(path))
                _executablePath = path ?? throw new ArgumentNullException(nameof(path));
            return this;
        }

        public Builder WithWorkingDirectory(string? workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory))
                return this;
            return Set(wd => wd._workingDirectory = workingDirectory);
        }

        public Builder WithProcessName(string? processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return this;
            return Set(pn => pn._processName = processName.Trim());
        }

        public Builder WithOrder(int order)
        {
            return Set(o => o._order = order);
        }

        public Builder IsActive(bool isActive = true) => Set(a => a._isActive = isActive);

        public Builder SkipRunningCheck(bool skip = false) =>
            Set(sr => sr._skipRunningCheck = skip);

        public Builder WithMonitorIndex(int monitorIndex) =>
            Set(mi => mi._monitorIndex = monitorIndex);

        public Builder WithPosition(SnapPosition position) => Set(p => p._position = position);

        public Builder WithPosition(string position)
        {
            if (EnumTryParse<SnapPosition>(position, true, out var parsedPosition))
                return WithPosition(parsedPosition);
            return this;
        }

        public Builder WithVerb(string? verb)
        {
            if (string.IsNullOrWhiteSpace(verb))
                return this;
            return Set(v => v._verb = verb);
        }

        public Builder UseShellExecute(bool useShellExecute = true) =>
            Set(se => se._useShellExecute = useShellExecute);

        public Builder CreateNoWindow(bool createNoWindow = true) =>
            Set(cn => cn._createNoWindow = createNoWindow);

        public Builder AddArguments(List<string> args)
        {
            if (args == null || args.Count == 0)
                return this;
            
            foreach (var arg in args)
                AddArgumentString(arg);
            return this;
        }

        private void AddArgumentString(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
                throw new ArgumentNullException(nameof(arg));

            _arguments = _arguments == null ? arg : $"{_arguments} {arg}";
        }

        public Builder AddSplashTitle(List<string> title)
        {
            _splashTitles.AddRange(title.Where(t => !string.IsNullOrWhiteSpace(t.Trim())));
            return this;
        }

        /// <summary>
        /// Constructs an <see cref="ApplicationDefinition"/> instance based on the configured builder properties.
        /// </summary>
        /// <returns> An instance of <see cref="ApplicationDefinition"/>. </returns>
        /// <exception cref="ArgumentException">Thrown when the executable path is null or empty.</exception>
        public ApplicationDefinition Build()
        {
            if (string.IsNullOrWhiteSpace(_executablePath))
                throw new ArgumentException($"Executable path cannot be empty. {_name}");

            return new ApplicationDefinition(this);
        }

        /// <summary>
        /// Tries to parse a string representation of the name or numeric value of one or more enumerated constants
        /// into an equivalent enumerated object. The return value indicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type to parse.</typeparam>
        /// <param name="value">The string representation of the enumerated value to parse.</param>
        /// <param name="ignoreCase">Specifies whether the parsing is case-insensitive.</param>
        /// <param name="result">When this method returns, contains the enumerated type if the parse operation succeeds,
        /// or the default value of TEnum if the parse operation fails.</param>
        /// <returns>True if the value was parsed successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when the trimmed value is not null or whitespace and not a valid member of the specified enumeration type.</exception>
        private static bool EnumTryParse<TEnum>(string? value, bool ignoreCase, out TEnum result)
            where TEnum : struct
        {
            value = value?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            if (Enum.TryParse<TEnum>(value, ignoreCase, out result))
                return true;

            throw new ArgumentException($"Value '{value}' is not a valid {typeof(TEnum).Name}.");
        }

        /// <summary>
        /// Allows implicit conversion from a Builder to an ApplicationDefinition.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static implicit operator ApplicationDefinition(Builder builder) => builder.Build();

        /// <summary>
        /// Implicitly converts an instance of <see cref="ApplicationDefinitionDto"/> to an instance of <see cref="Builder"/>.
        /// </summary>
        /// <param name="definition">The application definition data transfer object to be converted.</param>
        /// <returns>A builder instance populated with data from the <see cref="ApplicationDefinitionDto"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="definition"/> is null.</exception>
        public static implicit operator Builder(ApplicationDefinitionDto definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            return New()
                .WithName(
                    string.IsNullOrEmpty(definition.Name)
                        ? Path.GetFileNameWithoutExtension(definition.ExecutablePath)
                        : definition.Name
                )
                .WithCategory(definition.Category)
                .WithExecutablePath(definition.ExecutablePath)
                .WithWorkingDirectory(definition.WorkingDirectory)
                .AddArguments(definition.Arguments)
                .WithProcessName(
                    definition.ProcessName
                    ?? Path.GetFileNameWithoutExtension(definition.ExecutablePath)
                )
                .WithOrder(definition.Order)
                .IsActive(definition.IsActive)
                .SkipRunningCheck(definition.SkipRunningCheck)
                .WithMonitorIndex(definition.MonitorIndex)
                .WithPosition(definition.Position)
                .WithVerb(definition.Verb)
                .UseShellExecute(definition.UseShellExecute)
                .CreateNoWindow(definition.CreateNoWindow)
                .AddSplashTitle(definition.SplashTitles);
        }
    }
}
